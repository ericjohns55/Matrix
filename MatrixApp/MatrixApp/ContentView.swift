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
    case image, text, home, clockFaces, timers
}

struct ContentView: View {
    @StateObject var programOverview = ProgramOverviewViewModel()
    @StateObject var fonts = FontsViewModel()
    @StateObject var colors = ColorsViewModel()
    @StateObject var renderedImages = RenderedImageViewModel()
    
    var body: some View {
        Text("Matrix Controller")
            .fontWeight(.bold)
            .font(.system(size: 32.0))
        
        TabView {
            Tab("Image", systemImage: "photo") {
                VStack {
                    List {
                        ForEach(colors.colors) { color in
                            HStack {
                                Text(color.name)
                                Spacer()
                                Text("\(color.id)")
                            }
                        }
                    }
                    .task {
                        await colors.getColors()
                    }
                    .listStyle(PlainListStyle())
                    .navigationTitle("Colors")
                    
                }
                .padding()
                
                Button(action: {
                    Task {
                        await colors.getColors()
                        await renderedImages.fetchDecodeImage()
                    }
                }, label: {
                    Text("Refresh Colors")
                })
            }
            
            Tab("Text", systemImage: "textformat") {
                VStack {
                    List {
                        ForEach(fonts.fonts) { font in
                            HStack {
                                Text(font.name)
                            }
                        }
                    }
                    .task {
                        await fonts.getFonts()
                    }
                    .listStyle(PlainListStyle())
                    .navigationTitle("Fonts")
                    
                }
                .padding()
                
                Button(action: {
                    Task {
                        await fonts.getFonts()
                    }
                }, label: {
                    Text("Refresh Fonts")
                })
            }
            
            Tab("Overview", systemImage: "house.fill") {
                VStack(spacing: 0) {
                    Button(action: {
                        Task {
                            await renderedImages.fetchDecodeImage()
                            await programOverview.getProgramOverview()
                        }
                    }) {
                        Image(uiImage: renderedImages.image)
                            .resizable()
                            .scaledToFit()
                            .task {
                                await renderedImages.fetchDecodeImage()
                            }
                    }
                    
                    Text("Current State: \(programOverview.programOverview.matrixState)")
                        .task {
                            await programOverview.getProgramOverview()
                        }
                }
            }
            
            Tab("Clock Faces", systemImage: "clock") {
                
            }
            
            Tab("Timers", systemImage: "timer") {
                
            }
        }
    }
}

#Preview {
    ContentView()
}
