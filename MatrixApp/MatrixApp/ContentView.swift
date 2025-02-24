//
//  ContentView.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/28/25.
//

import Combine
import SwiftUI

enum AppPage {
    case image, text, overview, clockFaces, timers
}

struct ContentView: View {
    public static let IMAGE_VIEW_SIZE = CGFloat(256)
    
    @StateObject var matrixController = MatrixController()
    @StateObject var textController = TextController()
    @StateObject var imagesController = ImagesController()
    @State private var selectedTab: AppPage = .image
    
        
    var body: some View {
        Text("Matrix Controller")
            .fontWeight(.bold)
            .font(.system(size: 32.0))
            .task {
                await matrixController.getProgramOverview()
                await textController.loadAllData()
                await imagesController.fetchMatrixRendering()
                await imagesController.loadSavedImages()
            }
        
        TabView(selection: $selectedTab) {
            VStack {
                ImageView(imagesController: imagesController,
                          matrixInformation: matrixController.programOverview.matrixInformation ?? MatrixInformation(brightness: 50, width: 64, height: 32))
            }
            .tabItem {
                Label("Image", systemImage: "photo")
            }.tag(AppPage.image)
            
            
            VStack {
                ScrollView {
                    TextView(
                        matrixInformation: matrixController.programOverview.matrixInformation,
                        textController: textController,
                        imagesController: imagesController)
                }
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
                
            }
            .tabItem {
                Label("Timers", systemImage: "timer")
            }.tag(AppPage.timers)
        }
    }
}

#Preview {
    ContentView()
}
