//
//  TextView.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/21/25.
//

import SwiftUI

enum TextType {
    case stationary, scrolling
}

struct TextView: View {
    private let ROW_SIZE = CGFloat(32)
    private let PADDING = CGFloat(8)
    
    @State private var keyboardResponder = KeyboardResponder()
    
    @State private var textType: TextType = .stationary
        
    @State private var renderedPreview: UIImage? = nil
    @State private var text: String = ""
    @State private var selectedColor: MatrixColor? = nil
    @State private var selectedFont: MatrixFont? = nil
    @State private var selectedImage: SavedImage? = nil
    
    @State private var iterations: Int = 3
    @State private var scrollInterval: Int = 10
    
    @State private var splitByWord: Bool = true
    @State private var alignment: String = "Center"
    @State private var positioning: String = "Center"
    
    @State private var animationTimer = Timer.publish(every: 1, on: .main, in: .common).autoconnect()
    @State private var animationEnabled: Bool = true
    @State private var animationRunning: Bool = false
    @State private var offsetBounds: Int = 0
    @State private var currentOffset: Int = 0
    @State private var pixelsPerFrame: Int = 0
    
    let matrixInformation: MatrixInformation
    let textController: TextController
    let imagesController: ImagesController
    
    init(matrixInformation: MatrixInformation?, textController: TextController, imagesController: ImagesController) {
        self.matrixInformation = matrixInformation ?? MatrixInformation(brightness: 50, width: 64, height: 32)
        self.textController = textController
        self.imagesController = imagesController
        
        self.selectedImage = imagesController.EmptyImage
    }
    
    var body: some View {
        VStack {
            if (textType == .stationary) {
                Image(uiImage: (renderedPreview ?? imagesController.awaitingContent))
                    .resizable()
                    .scaledToFit()
                    .frame(width: CGFloat(ContentView.IMAGE_VIEW_SIZE),
                           height: CGFloat(ContentView.IMAGE_VIEW_SIZE))
                    .border(.gray)
            } else {
                VStack(spacing: 4) {
                    Image(uiImage: (renderedPreview ?? imagesController.awaitingContent))
                        .resizable()
                        .scaledToFill()
                        .clipped()
                        .offset(x: CGFloat(currentOffset))
                        .frame(width: CGFloat(ContentView.IMAGE_VIEW_SIZE),
                               height: CGFloat(ContentView.IMAGE_VIEW_SIZE))
                        .clipped()
                        .border(.gray)
                        .onTapGesture { _ in
                            // this is because the image moves out of the frame as it scrolls
                            // if the keyboard is active they want to clear the keyboard, not stop the animation
                            if (keyboardResponder.keyboardActive) {
                                UIApplication.shared.finishEditing()
                            } else {
                                animationEnabled.toggle()
                                setupAnimation()
                            }
                        }
                        .onReceive(animationTimer) { _ in
                            if (animationEnabled && animationRunning) {
                                currentOffset -= pixelsPerFrame
                                
                                if (currentOffset <= -1 * offsetBounds) {
                                    currentOffset = offsetBounds + Int(ContentView.IMAGE_VIEW_SIZE)
                                }
                            }
                        }
                    
                    if (textType == .scrolling && optionallyParseBackgroundImage() != nil) {
                        Text("Background Image Preview not Available in App")
                            .foregroundStyle(Color(red: 128 / 255, green: 0, blue: 0))
                            .font(.system(size: 12))
                            .italic()
                    }
                }
            }
            
            
            Picker("Text Config", selection: $textType) {
                Text("Stationary").tag(TextType.stationary)
                Text("Scrolling").tag(TextType.scrolling)
            }.pickerStyle(.segmented)
                .onChange(of: textType, {
                    Task {
                        await optionallyUpdateTextPreview()
                    }
                })
                .padding(PADDING)
            
            LabeledContent {
                TextField("(example text)", text: $text)
                    .multilineTextAlignment(.trailing)
                    .onSubmit {
                        Task {
                            await optionallyUpdateTextPreview()
                        }
                    }
                    .onAppear() {
                        UITextField.appearance().clearButtonMode = .whileEditing
                    }
            } label: {
                Text("Text Content")
            }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            
            
            LabeledContent {
                Picker("Text Orientation", selection: $positioning) {
                    Text("Top").tag("Top")
                    Text("Center").tag("Center")
                    Text("Bottom").tag("Bottom")
                }
                .onChange(of: positioning) {
                    Task {
                        await optionallyUpdateTextPreview()
                    }
                }
            } label: {
                Text("Vertical Positioning")
            }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            
            if (textType == .stationary) {
                LabeledContent {
                    Picker("Text Alignment", selection: $alignment) {
                        Text("Left").tag("Left")
                        Text("Center").tag("Center")
                        Text("Right").tag("Right")
                    }
                    .onChange(of: alignment) {
                        Task {
                            await optionallyUpdateTextPreview()
                        }
                    }
                } label: {
                    Text("Text Orientation")
                }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            } else {
                LabeledContent {
                    TextField("(number of repetitions)", value: $iterations, format: .number)
                        .keyboardType(.numberPad)
                        .multilineTextAlignment(.trailing)
                        .onSubmit {
                            Task {
                                await optionallyUpdateTextPreview()
                            }
                        }
                } label: {
                    Text("Iterations")
                }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            }
            
            LabeledContent {
                Picker("Color", selection: $selectedColor) {
                    ForEach(textController.colors) { color in
                        HStack {
                            Text(color.name)
                        }
                        .tag(color as MatrixColor?)
                    }
                } currentValueLabel: {
                    Text(selectedColor?.name ?? "Select a Color")
                }
                .pickerStyle(MenuPickerStyle())
                .onChange(of: selectedColor) {
                    Task {
                        await optionallyUpdateTextPreview()
                    }
                }
            } label: {
                Text("Color")
            }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            
            LabeledContent {
                Picker("Font", selection: $selectedFont) {
                    ForEach(textController.fonts) { font in
                        HStack {
                            Text(font.name)
                        }
                        .tag(font as MatrixFont?)
                    }
                } currentValueLabel: {
                    Text(selectedFont?.name ?? "Select a Font")
                }
                .pickerStyle(MenuPickerStyle())
                .onChange(of: selectedFont) {
                    Task {
                        await optionallyUpdateTextPreview()
                    }
                }
            } label: {
                Text("Font")
            }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            
            if (textType == .stationary) {
                LabeledContent {
                    Toggle(isOn: $splitByWord) {
                        EmptyView()
                    }
                    .toggleStyle(SwitchToggleStyle(tint: .blue))
                    .onChange(of: splitByWord) {
                        Task {
                            await optionallyUpdateTextPreview()
                        }
                    }
                } label: {
                    Text("Split by Word")
                }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            } else {
                LabeledContent {
                    TextField("(interval in milliseconds)", value: $scrollInterval, format: .number)
                        .multilineTextAlignment(.trailing)
                        .onSubmit {
                            Task {
                                await optionallyUpdateTextPreview()
                            }
                        }
                } label: {
                    Text("Scroll Interval (ms)")
                }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            }
                        
            LabeledContent {
                Picker("Background Image", selection: $selectedImage) {
                    ForEach(imagesController.savedImages) { savedImage in
                        HStack {
                            Text(savedImage.name)
                        }
                        .tag(savedImage as SavedImage?)
                    }
                } currentValueLabel: {
                    Text(selectedImage?.name ?? "(None)")
                }
                .pickerStyle(MenuPickerStyle())
                .onChange(of: selectedImage) {
                    Task {
                        await optionallyUpdateTextPreview()
                    }
                }
            } label: {
                Text("Background Image")
            }.frame(minHeight: ROW_SIZE, maxHeight: ROW_SIZE).padding(PADDING)
            
            Button(action: {
                Task {
                    await tryPostText()
                }
            }) {
                Text("Send to Matrix")
            }
            .frame(width: CGFloat(ContentView.IMAGE_VIEW_SIZE), height: ROW_SIZE) // TODO: whole bounds need to be clickable
            .border(.gray)
            .padding(PADDING)
        }
        .padding(.bottom, keyboardResponder.keyboardHeight)
        .animation(.easeOut(duration: 0.2), value: keyboardResponder.keyboardHeight)
        .background(Color(UIColor.systemBackground))
        .onTapGesture {
            UIApplication.shared.finishEditing()
        }
    }
    
    private func allParamsAreValid() -> Bool {
        if (textType == .stationary) {
            return textController.validatePlainText(
                text: text,
                color: selectedColor,
                font: selectedFont,
                alignment: alignment,
                verticalPositioning: positioning,
                splitByWord: splitByWord,
                backgroundImageId: nil)
            .successfullyValidated
        } else {
            return textController.validateScrollingText(
                text: text,
                verticalPositioning: positioning,
                scrollingDelay: scrollInterval,
                iterations: iterations,
                color: selectedColor,
                font: selectedFont,
                backgroundImageId: nil)
            .successfullyValidated
        }
    }
        
    private func optionallyUpdateTextPreview(requeryImage: Bool = true) async {
        let backgroundImageId = optionallyParseBackgroundImage()
        var renderedImage: UIImage? = renderedPreview
        
        if (requeryImage) {
            if (textType == .stationary) {
                renderedImage = await textController.tryRenderPlainTextPreview(
                    text: text,
                    color: selectedColor,
                    font: selectedFont,
                    alignment: alignment,
                    verticalPositioning: positioning,
                    splitByWord: splitByWord,
                    backgroundImageId: backgroundImageId)
            } else {
                renderedImage = await textController.tryRenderScrollingTextPreview(
                    text: text,
                    verticalPositioning: positioning,
                    scrollingInterval: scrollInterval,
                    iterations: iterations,
                    color: selectedColor,
                    font: selectedFont,
                    backgroundImageId: backgroundImageId)
            }
        }
        
        if (renderedImage != nil) {
            renderedPreview = renderedImage
            
            if (textType == .scrolling && renderedPreview!.size.width > renderedPreview!.size.height) {
                setupAnimation()
                
                animationTimer = Timer.publish(
                    every: Double(scrollInterval) * 0.001,
                    on: .main,
                    in: .common)
                .autoconnect()
                
                animationRunning = true
            }
        } else {
            renderedPreview = imagesController.invalidContent
            
            animationTimer.upstream.connect().cancel()
            animationRunning = false
            currentOffset = 0
        }
    }
    
    private func tryPostText() async {
        let backgroundImageId = optionallyParseBackgroundImage()
        
        if (textType == .stationary) {
            await textController.tryPostText(
                text: text,
                color: selectedColor,
                font: selectedFont,
                alignment: alignment,
                verticalPositioning: positioning,
                splitByWord: splitByWord,
                backgroundImageId: backgroundImageId)
        } else {
            await textController.tryPostScrollingText(
                text: text,
                verticalPositioning: positioning,
                scrollingInterval: scrollInterval,
                iterations: iterations,
                color: selectedColor,
                font: selectedFont,
                backgroundImageId: backgroundImageId)
        }
    }
    
    private func setupAnimation() {
        if (renderedPreview != nil) {
            offsetBounds = Int(renderedPreview!.size.width) - matrixInformation.width * 2
            currentOffset = offsetBounds
            pixelsPerFrame = Int(renderedPreview!.size.height) / matrixInformation.height
        }
    }
    
    func formatText() -> String {
        var text = "Text: [TEXT]\nAlignment: [ALIGNMENT]\nColor: [COLOR]\nFont: [FONT]"
        text = text.replacingOccurrences(of: "[TEXT]", with: text)
        text = text.replacingOccurrences(of: "[ALIGNMENT]", with: alignment)
                
        if let selectedColor = selectedColor {
            text = text.replacingOccurrences(of: "[COLOR]", with: "\(selectedColor.name) [\(selectedColor.id)]")
        } else {
            text = text.replacingOccurrences(of: "[COLOR]", with: "(NOT SELECTED)")
        }
        
        if let selectedFont = selectedFont {
            text = text.replacingOccurrences(of: "[FONT]", with: "\(selectedFont.name) [\(selectedFont.id)]")
        } else {
            text = text.replacingOccurrences(of: "[FONT]", with: "(NOT SELECTED)")
        }
        
        return text
    }
    
    private func optionallyParseBackgroundImage() -> Int? {
        return selectedImage?.id != -1 ? selectedImage?.id : nil ?? nil
    }
}
