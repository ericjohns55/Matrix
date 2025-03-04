//
//  HomeView.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/2/25.
//

import AlertToast
import SwiftUI

struct HomeView: View {
    private static let BUTTON_HEIGHT: CGFloat = CGFloat(64)
    
    @ObservedObject var matrixController: MatrixController
    @ObservedObject var textController: TextController
    @ObservedObject var imagesController: ImagesController
    @ObservedObject var appController: AppController
    
    @State private var inSettings: Bool = false
    
    init(matrixController: MatrixController,
         textController: TextController,
         imagesController: ImagesController,
         appController: AppController) {
        self.matrixController = matrixController
        self.textController = textController
        self.imagesController = imagesController
        self.appController = appController
    }
    
    var body: some View {
        if (!inSettings) {
            VStack(spacing: 0) {
                Image(uiImage: imagesController.matrixRendering ?? imagesController.loading)
                    .resizable()
                    .scaledToFit()
                    .frame(width: 256, height: 256)
                    .border(.gray)
                    .task {
                        await imagesController.fetchMatrixRendering()
                        await matrixController.getProgramOverview()
                    }
                
                Text("Current State: \(matrixController.programOverview.matrixState)")
            }.padding(10)
            
            Button(action: {
                Task {
                    await appController.executeRequestToToast(task: {
                        await matrixController.getProgramOverview()
                        await imagesController.fetchMatrixRendering()
                    })
                }
            }) {
                Text("Refresh State")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            }
            .padding(20)
            .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: HomeView.BUTTON_HEIGHT)
            .border(.gray)
            .contentShape(Rectangle())
            
            Button(action: {
                Task {
                    await appController.executeRequestToToast(task: {
                        await matrixController.restoreMatrix() // also updates overview
                        await imagesController.fetchMatrixRendering()
                    })
                }
            }) {
                Text("Restore Matrix")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            }
            .padding(20)
            .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: HomeView.BUTTON_HEIGHT)
            .border(.gray)
            .contentShape(Rectangle())
            
            Button(action: {
                inSettings.toggle()
            }) {
                Text("Settings")
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            }
            .padding(20)
            .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: HomeView.BUTTON_HEIGHT)
            .border(.gray)
            .contentShape(Rectangle())
        } else {
            Text("Settings")
            
            Button(action: {
                inSettings.toggle()
            }) {
                Text("Return to Overview")
            }
            .padding(20)
            .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: HomeView.BUTTON_HEIGHT)
            .border(.gray)
            .contentShape(Rectangle())
        }
        
    }
}
