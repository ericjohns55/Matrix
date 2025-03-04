//
//  ClockFaceView.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/2/25.
//

import SwiftUI

struct ClockFaceView: View {
    @ObservedObject var matrixController: MatrixController
    @ObservedObject var clockFaceController: ClockFaceController
    @ObservedObject var imagesController: ImagesController
    @ObservedObject var appController: AppController
    
    init(matrixController: MatrixController,
         clockFaceController: ClockFaceController,
         imagesController: ImagesController,
         appController: AppController) {
        self.matrixController = matrixController
        self.clockFaceController = clockFaceController
        self.imagesController = imagesController
        self.appController = appController
    }
    
    var body: some View {
        Image(uiImage: (clockFaceController.overridenClockFaceImage ?? clockFaceController.currentClockFaceImage ?? imagesController.loading))
            .resizable()
            .scaledToFit()
            .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: MatrixAppView.IMAGE_VIEW_SIZE)
            .border(.gray)
            .task {
                await clockFaceController.loadClockFaces()
            }
        
        Text("Current Clock Face: \(matrixController.programOverview.currentClockFace?.name ?? "(Unknown)")")
        Text("Overriden Clock Face: \(clockFaceController.overridenClockFace?.name ?? "(None)")")
        
        List {
            ForEach(clockFaceController.clockFaces, id: \.id) { clockFace in
                HStack {
                    Image(uiImage: imagesController.imageFromBase64(base64String: clockFace.base64Rendering ?? "invalid"))
                        .resizable()
                        .scaledToFit()
                        .frame(width: 64, height: 64)
                        .border(.gray)
                    
                    Text(clockFace.name)
                    
                    Spacer()
                    
                    if (clockFace.isCurrentFace) {
                        Image(systemName: "clock")
                    }
                    
                    if (clockFaceController.isOverridenFace(face: clockFace)) {
                        Image(systemName: "display")
                    }
                }
                .frame(height: 64)
                .contentShape(Rectangle())
                .onTapGesture {
                    Task {
                        if (clockFaceController.isOverridenFace(face: clockFace)) {
                            await appController.executeRequestToToast(task: {
                                await clockFaceController.cancelOverride()
                            }, successMessage: "Successfully removed override", failureMessage: "Failed to cancel override")
                        } else {
                            await appController.executeRequestToToast(task: {
                                await clockFaceController.overrideClockFace(clockFaceId: clockFace.id)
                            }, successMessage: "Set override to \"\(clockFace.name)\"", failureMessage: "Failed to override clock face")
                        }
                    }
                }
            }
        }
        .refreshable {
            await clockFaceController.loadClockFaces()
        }
        
        Button(action: {
            Task {
                if (clockFaceController.overridenClockFace == nil) {
                    appController.displayToastMessage(message: "There is currently no override", color: .gray)
                    return
                }
                
                await appController.executeRequestToToast(task: {
                    await clockFaceController.cancelOverride()
                }, successMessage: "Successfully removed override", failureMessage: "Failed to cancel override")
            }
        }) {
            Text("Remove Override")
        }
        .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: 50)
        .border(.gray)
        .padding(10)
    }
}
