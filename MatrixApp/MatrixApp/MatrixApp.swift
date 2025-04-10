//
//  MatrixAppApp.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/28/25.
//

import SwiftUI

@main
struct MatrixApp: App {
    static var ServerUrl = ""
    static var ApiKey = ""
        
    @StateObject private var appController = AppController()
    @StateObject private var matrixController = MatrixController()
    @StateObject private var clockFaceController = ClockFaceController()
    @StateObject private var textController = TextController()
    @StateObject private var imagesController = ImagesController()
    @StateObject private var timerController = TimerController()
    
    var body: some Scene {
        WindowGroup {
            MatrixAppView()
                .environmentObject(appController)
                .environmentObject(matrixController)
                .environmentObject(clockFaceController)
                .environmentObject(textController)
                .environmentObject(imagesController)
                .environmentObject(timerController)
                .onAppear() {
                    Task {
                        await textController.loadColors()
                        await textController.loadFonts()
                        await imagesController.loadSavedImages()
                    }
                }
        }
    }
}
