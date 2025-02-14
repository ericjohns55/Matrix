//
//  ContentView.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/28/25.
//
// Future References:
// https://paulallies.medium.com/swiftui-5-5-api-data-to-list-view-776c69a456d3
// https://www.swiftbysundell.com/articles/building-an-async-swiftui-button/

import SwiftUI

enum AppPage {
    case image, text, overview, clockFaces, timers
}

enum TextType {
    case stationary, scrolling
}

struct ContentView: View {    
    @StateObject var matrixController = MatrixController()
    @StateObject var textController = TextController()
    @StateObject var imagesController = ImagesController()
    
    @State private var selectedTab: AppPage = .text
    
    @State private var textType: TextType = .stationary
    @State private var currentTextImage: UIImage? = nil
    @State private var textControllerText: String = ""
    @State private var textScrollIterations: Int = 3
    @State private var textScrollInterval: Int = 10
    @State private var selectedTextAlignment: String = "Center"
    @State private var selectedColor: MatrixColor? = nil
    @State private var selectedFont: MatrixFont? = nil
    @State private var textSplitByWord: Bool = true
    
    @State private var showAlert = false
        
    func formatText() -> String {
        var text = "Text: [TEXT]\nAlignment: [ALIGNMENT]\nColor: [COLOR]\nFont: [FONT]"
        text = text.replacingOccurrences(of: "[TEXT]", with: textControllerText)
        text = text.replacingOccurrences(of: "[ALIGNMENT]", with: selectedTextAlignment)
                
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
    
        
    var body: some View {
        Text("Matrix Controller")
            .fontWeight(.bold)
            .font(.system(size: 32.0))
            .task {
                await textController.loadAllData()
                await imagesController.fetchMatrixRendering()
            }
        
        TabView(selection: $selectedTab) {
            VStack {
                VStack {
                    List {
                        ForEach(textController.colors) { color in
                            HStack {
                                Text(color.name)
                                Spacer()
                                Text("\(color.id)")
                            }
                        }
                    }
                    .listStyle(PlainListStyle())
                    .navigationTitle("Colors")
                    
                }
                .padding()
                
                Button(action: {
                    Task {
                        await textController.loadAllData()
                        await imagesController.fetchMatrixRendering()
                    }
                }, label: {
                    Text("Refresh Colors")
                })
            }
            .tabItem {
                Label("Image", systemImage: "photo")
            }.tag(AppPage.image)
            
            
            
            VStack {
                Image(uiImage: (currentTextImage ?? imagesController.awaitingContent))
                    .resizable()
                    .scaledToFit()
                    .padding(20)
                    .frame(width: 224, height: 224)
                    .border(.gray)
                
                Picker("Server Config", selection: $textType) {
                    Text("Stationary").tag(TextType.stationary)
                    Text("Scrolling").tag(TextType.scrolling)
                }.pickerStyle(.segmented)
                    .onChange(of: textType, {
                        Task {
                            await optionallyUpdateTextPreview()
                        }
                    })
                    .padding(10)
                
                LabeledContent {
                    TextField("(example text)", text: $textControllerText)
                        .multilineTextAlignment(.trailing)
                        .onSubmit {
                            Task {
                                await optionallyUpdateTextPreview()
                            }
                        }
                } label: {
                    Text("Text Content")
                }.padding(10)
                
                if (textType == .stationary) {
                    LabeledContent {
                        Picker("Text Orientation", selection: $selectedTextAlignment) {
                            Text("Left").tag("Left")
                            Text("Center").tag("Center")
                            Text("Right").tag("Right")
                        }
                        .onChange(of: selectedTextAlignment) {
                            Task {
                                await optionallyUpdateTextPreview()
                            }
                        }
                    } label: {
                        Text("Text Orientation")
                    }.padding(10)
                } else {
                    LabeledContent {
                        TextField("(number of repetitions)", value: $textScrollIterations, format: .number)
                            .multilineTextAlignment(.trailing)
                            .onSubmit {
                                Task {
                                    await optionallyUpdateTextPreview()
                                }
                            }
                    } label: {
                        Text("Iterations")
                    }.padding(15)
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
                }.padding(10)
                
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
                }.padding(10)
                
                if (textType == .stationary) {
                    LabeledContent {
                        Toggle(isOn: $textSplitByWord) {
                            EmptyView()
                        }
                        .toggleStyle(SwitchToggleStyle(tint: .blue))
                        .onChange(of: textSplitByWord) {
                            Task {
                                await optionallyUpdateTextPreview()
                            }
                        }
                    } label: {
                        Text("Split by Word")
                    }.padding(10)
                } else {
                    LabeledContent {
                        TextField("(interval in milliseconds)", value: $textScrollInterval, format: .number)
                            .multilineTextAlignment(.trailing)
                            .onSubmit {
                                Task {
                                    await optionallyUpdateTextPreview()
                                }
                            }
                    } label: {
                        Text("Scroll Interval (ms)")
                    }.padding(16)
                }
                
                Button(action: {
                    Task {
                        await tryPostText()
                    }
                }) {
                    Text("Send to Matrix")
                }
                .padding(10)
                .frame(width: 256, height: 48)
                .border(.gray)
            }
            .tabItem {
                Label("Text", systemImage: "textformat")
            }.tag(AppPage.text)
                
            VStack {
                VStack(spacing: 0) {
                    Button(action: {
                        Task {
                            await imagesController.fetchMatrixRendering()
                            await matrixController.getProgramOverview()
                        }
                    }) {
                        Image(uiImage: imagesController.matrixRendering)
                            .resizable()
                            .scaledToFit()
                            .frame(width: 256, height: 256)
                    }.border(.gray)
                    
                    Text("Current State: \(matrixController.programOverview.matrixState)")
                        .task {
                            await imagesController.fetchMatrixRendering()
                            await matrixController.getProgramOverview()
                        }
                }.padding(10)
                
                Button(action: {
                    Task {
                        _ = await matrixController.restoreMatrix()
                        await imagesController.fetchMatrixRendering()
                        await matrixController.getProgramOverview()
                    }
                }) {
                    Text("Restore Matrix")
                }
                .padding(20)
                .frame(minWidth: 0, maxWidth: .infinity)
                .border(.gray)
            }
            .tabItem {
                Label("Overview", systemImage: "house.fill")
            }.tag(AppPage.overview)
            
            
            
            VStack {
                VStack {
                    List {
                        ForEach(textController.fonts) { font in
                            HStack {
                                Text(font.name)
                            }
                        }
                    }
                    .listStyle(PlainListStyle())
                    .navigationTitle("Fonts")
                    
                }
                .padding()
                
                Button(action: {
                    Task {
                        await textController.getFonts()
                    }
                }, label: {
                    Text("Refresh Fonts")
                })
            }
            .tabItem {
                Label("Clock Faces", systemImage: "clock")
            }.tag(AppPage.clockFaces)
            
            
            
            VStack {
//                List(colors.colors, id: \.id, selection: $selectedColor) { color in
//                    Text(color.name)
//                }.task {
//                    await colors.getColors()
//                }
            }
            .tabItem {
                Label("Timers", systemImage: "timer")
            }.tag(AppPage.timers)
        }
    }
    
    private func optionallyUpdateTextPreview() async {
        var renderedImage: UIImage? = nil
        
        if (textType == .stationary) {
            renderedImage = await textController.tryRenderPlainTextPreview(text: textControllerText, color: selectedColor, font: selectedFont, alignment: selectedTextAlignment, splitByWord: textSplitByWord)
        } else {
            renderedImage = await textController.tryRenderScrollingTextPreview(text: textControllerText, scrollingInterval: textScrollInterval, iterations: textScrollIterations, color: selectedColor, font: selectedFont)
        }
        
        if (renderedImage != nil) {
            currentTextImage = renderedImage
        } else {
            currentTextImage = imagesController.invalidContent
        }
    }
    
    private func tryPostText() async {
        if (textType == .stationary) {
            await textController.tryPostText(text: textControllerText, color: selectedColor, font: selectedFont, alignment: selectedTextAlignment, splitByWord: textSplitByWord)
        } else {
            await textController.tryPostScrollingText(text: textControllerText, scrollingInterval: textScrollInterval, iterations: textScrollIterations, color: selectedColor, font: selectedFont)
        }
    }
}

#Preview {
    ContentView()
}
