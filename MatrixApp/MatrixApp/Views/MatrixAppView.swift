//
//  ContentView.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/28/25.
//

import AlertToast
import Combine
import SwiftUI

enum AppPage {
    case image, text, overview, clockFaces, timers
}

struct MatrixAppView: View {
    public static let IMAGE_VIEW_SIZE = CGFloat(256)
    
    @EnvironmentObject var matrixController: MatrixController
    @EnvironmentObject var clockFaceController: ClockFaceController
    @EnvironmentObject var textController: TextController
    @EnvironmentObject var imagesController: ImagesController
    @EnvironmentObject var appController: AppController
    
    @State private var selectedTab: AppPage = .overview
    
    @State private var showToast = false
                
    var body: some View {
        Text("Matrix Controller")
            .fontWeight(.bold)
            .font(.system(size: 32.0))
        
        TabView(selection: $selectedTab) {
            VStack {
                ImageView(imagesController: imagesController,
                          matrixInformation: matrixController.programOverview.matrixInformation
                            ?? MatrixInformation(brightness: 50, width: 64, height: 32),
                          appController: appController)
            }
            .tabItem {
                Label("Image", systemImage: "photo")
            }.tag(AppPage.image)
            
            
            
            VStack {
                ScrollView {
                    TextView(
                        matrixInformation: matrixController.programOverview.matrixInformation,
                        textController: textController,
                        imagesController: imagesController,
                        appController: appController)
                    .frame(height: 1000)
                }
            }
            .tabItem {
                Label("Text", systemImage: "textformat")
            }.tag(AppPage.text)
                
            
            
            VStack {
                HomeView(matrixController: matrixController,
                         textController: textController,
                         imagesController: imagesController,
                         appController: appController)
                
            }
            .tabItem {
                Label("Overview", systemImage: "house.fill")
            }.tag(AppPage.overview)
            
            
            
            VStack {
                ClockFaceView(matrixController: matrixController,
                              clockFaceController: clockFaceController,
                              imagesController: imagesController,
                              appController: appController)
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
        .toast(isPresenting: $appController.showToast, duration: 2, tapToDismiss: true) {
            AlertToast(displayMode: .banner(.pop),
                       type: .regular,
                       title: appController.toastText,
                       style: .style(backgroundColor: appController.toastColor))
        }
    }
}

#Preview {
    MatrixAppView()
}
