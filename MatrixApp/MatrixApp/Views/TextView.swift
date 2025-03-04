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
    private static let BUTTON_HEIGHT: CGFloat = CGFloat(50)
    private let ROW_SIZE = CGFloat(32)
    private let LIST_ROW_SIZE = CGFloat(48)
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
        
    @State private var selectedPlainText: PlainTextPayload? = nil
    @State private var selectedScrollingText: ScrollingTextPayload? = nil
    @State private var selectionEdited: Bool = false
    
    @ObservedObject var textController: TextController
    @ObservedObject var imagesController: ImagesController
    @ObservedObject var appController: AppController
    let matrixInformation: MatrixInformation
    
    init(matrixInformation: MatrixInformation?,
         textController: TextController,
         imagesController: ImagesController,
         appController: AppController) {
        self.matrixInformation = matrixInformation ?? MatrixInformation(brightness: 50, width: 64, height: 32)
        self.textController = textController
        self.imagesController = imagesController
        self.appController = appController
        
        self.selectedImage = imagesController.emptyImage
    }
    
    var body: some View {
        VStack {
            if (textType == .stationary) {
                Image(uiImage: (renderedPreview ?? imagesController.awaitingContent))
                    .resizable()
                    .scaledToFit()
                    .frame(width: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE),
                           height: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE))
                    .border(.gray)
            } else {
                VStack(spacing: 4) {
                    Image(uiImage: (renderedPreview ?? imagesController.awaitingContent))
                        .resizable()
                        .scaledToFill()
                        .clipped()
                        .offset(x: CGFloat(currentOffset))
                        .frame(width: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE),
                               height: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE))
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
                                    currentOffset = offsetBounds + Int(MatrixAppView.IMAGE_VIEW_SIZE)
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
                    ForEach(imagesController.savedImagesWithNone) { savedImage in
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
                        
            HStack {
                Button(action: {
                    Task {
                        await tryPostText()
                    }
                }) {
                    Text("Send to Matrix")
                        .frame(maxWidth: .infinity, maxHeight: .infinity)
                }
                .frame(maxWidth: .infinity, maxHeight: TextView.BUTTON_HEIGHT)
                .contentShape(Rectangle())
                .border(.gray)
                .padding(10)
                
                Button(action: {
                    var textId = 0;
                    
                    if (selectedPlainText != nil) {
                        textId = selectedPlainText!.id
                    } else if (selectedScrollingText != nil) {
                        textId = selectedScrollingText!.id
                    }
                    
                    Task {
                        await saveOrUpdateText(textId: textId)
                        
                        if (textType == .stationary) {
                            await requerySavedPlainText()
                        } else {
                            await requerySavedScrollingText()
                        }
                    }
                }) {
                    Text(((selectedPlainText != nil && textType == .stationary)
                          || (selectedScrollingText != nil && textType == .scrolling)) ? "Update Text" : "Save Text")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                }
                .frame(maxWidth: .infinity, maxHeight: TextView.BUTTON_HEIGHT)
                .contentShape(Rectangle())
                .border(.gray)
                .padding(10)
            }
            
            if (textType == .stationary) {
                List {
                    ForEach(textController.savedPlainText, id: \.id) { plainTextPayload in
                        HStack {
                            Image(uiImage: imagesController.imageFromBase64(base64String: plainTextPayload.backgroundImage?.base64Rendering ?? "invalid"))
                                .resizable()
                                .scaledToFit()
                                .frame(width: LIST_ROW_SIZE, height: LIST_ROW_SIZE)
                                .border(.gray)
                            Text(plainTextPayload.text)
                            
                            Spacer()
                            
                            if (selectedPlainText == plainTextPayload) {
                                Image(systemName: "checkmark.circle.fill")
                            }
                        }
                        .frame(maxWidth: .infinity, maxHeight: LIST_ROW_SIZE)
                        .contentShape(Rectangle())
                        .simultaneousGesture(TapGesture().onEnded {
                            selectedPlainText = (selectedPlainText == plainTextPayload) ? nil : plainTextPayload
                            selectionEdited = false
                            
                            if (selectedPlainText != nil) {
                                text = plainTextPayload.text
                                alignment = plainTextPayload.textAlignment
                                positioning = plainTextPayload.verticalPositioning
                                splitByWord = plainTextPayload.splitByWord
                                loadSavedDetails(fontId: plainTextPayload.matrixFontId, colorId: plainTextPayload.matrixColorId, imageId: plainTextPayload.backgroundImageId)
                            }
                        })
                    }
                    .onDelete { offsets in
                        let plainText = textController.savedPlainText[offsets.first!]
                        
                        Task {
                            await appController.executeRequestToToast(task: {
                                await textController.deletePlainText(plainTextId: plainText.id)
                            }, successMessage: "Successfully deleted stationary text", failureMessage: "Failed to delete stationary text")
                            
                            await requerySavedPlainText()
                        }
                    }
                }
                .refreshable {
                    await textController.loadSavedPlainTexts()
                }
                .task {
                    await textController.loadSavedPlainTexts()
                }
            } else {
                List {
                    ForEach(textController.savedScrollingText, id: \.id) { scrollingTextPayload in
                        HStack {
                            Image(uiImage: imagesController.imageFromBase64(base64String: scrollingTextPayload.backgroundImage?.base64Rendering ?? "invalid"))
                                .resizable()
                                .scaledToFit()
                                .frame(width: LIST_ROW_SIZE, height: LIST_ROW_SIZE)
                                .border(.gray)
                            Text(scrollingTextPayload.text)
                            
                            Spacer()
                            
                            if (selectedScrollingText == scrollingTextPayload) {
                                Image(systemName: "checkmark.circle.fill")
                            }
                        }
                        .frame(maxWidth: .infinity, maxHeight: LIST_ROW_SIZE)
                        .contentShape(Rectangle())
                        .simultaneousGesture(TapGesture().onEnded {
                            selectedScrollingText = (selectedScrollingText == scrollingTextPayload) ? nil : scrollingTextPayload
                            selectionEdited = false
                                                        
                            if (selectedScrollingText != nil) {
                                text = selectedScrollingText!.text
                                positioning = selectedScrollingText!.verticalPositioning
                                scrollInterval = selectedScrollingText!.scrollingDelay
                                iterations = selectedScrollingText!.iterations
                                loadSavedDetails(fontId: selectedScrollingText!.matrixFontId, colorId: selectedScrollingText!.matrixColorId, imageId: selectedScrollingText!.backgroundImageId)
                            }
                        })
                    }
                    .onDelete { offsets in
                        let scrollingText = textController.savedScrollingText[offsets.first!]
                            
                        Task {
                            await appController.executeRequestToToast(task: {
                                await textController.deleteScrollingText(scrollingTextId: scrollingText.id)
                            }, successMessage: "Successfully deleted scrolling text", failureMessage: "Failed to delete scrolling text")
                            
                            await requerySavedScrollingText()
                        }
                    }
                }
                .refreshable {
                    await textController.loadSavedScrollingTexts()
                }
                .task {
                    await textController.loadSavedScrollingTexts()
                }
            }
        }
        .padding(.bottom, keyboardResponder.keyboardHeight)
        .animation(.easeOut(duration: 0.2), value: keyboardResponder.keyboardHeight)
        .background(Color(UIColor.systemBackground))
        .onTapGesture {
            UIApplication.shared.finishEditing()
        }
    }
    
    private func loadSavedDetails(fontId: Int, colorId: Int, imageId: Int?) {
        selectedColor = textController.colors.first(where: { $0.id == colorId })
        selectedFont = textController.fonts.first(where: { $0.id == fontId })
        
        if (imageId != nil) {
            selectedImage = imagesController.savedImages.first(where: { $0.id == imageId! })
        } else {
            selectedImage = imagesController.emptyImage
        }
    }
    
    private func requerySavedPlainText() async {
        await textController.loadSavedPlainTexts()
        
        if (selectedPlainText != nil) {
            selectedPlainText = textController.savedPlainText.first(where: { $0.id == selectedPlainText!.id })
        }
    }
    
    private func requerySavedScrollingText() async {
        await textController.loadSavedScrollingTexts()
        
        if (selectedScrollingText != nil) {
            selectedScrollingText = textController.savedScrollingText.first(where: { $0.id == selectedScrollingText!.id })
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
        
        if (selectedPlainText != nil || selectedScrollingText != nil) {
            selectionEdited = true
        }
        
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
        
        await appController.executeRequestToToast(task: {
            if (textType == .stationary) {
                try await textController.tryPostText(
                    text: text,
                    color: selectedColor,
                    font: selectedFont,
                    alignment: alignment,
                    verticalPositioning: positioning,
                    splitByWord: splitByWord,
                    backgroundImageId: backgroundImageId)
            } else {
                try await textController.tryPostScrollingText(
                    text: text,
                    verticalPositioning: positioning,
                    scrollingInterval: scrollInterval,
                    iterations: iterations,
                    color: selectedColor,
                    font: selectedFont,
                    backgroundImageId: backgroundImageId)
            }
        }, failureMessage: "Text is invalid")
    }
    
    private func saveOrUpdateText(textId: Int = 0) async {
        if (textType == .stationary) {
            let payload = textController.validatePlainText(
                text: text,
                color: selectedColor,
                font: selectedFont,
                alignment: alignment,
                verticalPositioning: positioning,
                splitByWord: splitByWord,
                backgroundImageId: selectedImage?.id)
                .payload
            
            if (payload != nil) {
                if (selectionEdited) {
                    await appController.executeRequestToToast(task: {
                        await textController.saveOrUpdatePlainText(plainText: payload!, update: true, plainTextId: textId)
                    }, successMessage: "Successfully updated text", failureMessage: "Failed to update text")
                } else {
                    await appController.executeRequestToToast(task: {
                        await textController.saveOrUpdatePlainText(plainText: payload!)
                    }, successMessage: "Successfully saved text", failureMessage: "Failed to saved text")
                }
            } else {
                appController.displayToastMessage(message: "Text parameters are invalid", color: .red)
            }
        } else {
            let payload = textController.validateScrollingText(
                text: text,
                verticalPositioning: positioning,
                scrollingDelay: scrollInterval,
                iterations: iterations,
                color: selectedColor,
                font: selectedFont,
                backgroundImageId: selectedImage?.id)
            .payload
            
            if (payload != nil) {
                if (selectionEdited) {
                    await appController.executeRequestToToast(task: {
                        await textController.saveOrUpdateScrollingText(scrollingText: payload!, update: true, scrollingTextId: textId)
                    }, successMessage: "Successfully updated text", failureMessage: "Failed to update text")
                } else {
                    await appController.executeRequestToToast(task: {
                        await textController.saveOrUpdateScrollingText(scrollingText: payload!)
                    }, successMessage: "Successfully saved text", failureMessage: "Failed to save text")
                }
            } else {
                appController.displayToastMessage(message: "Text parameters are invalid", color: .red)
            }
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
