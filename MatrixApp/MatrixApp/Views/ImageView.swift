//
//  ImageView.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/21/25.
//

import SwiftUI
import PhotosUI
import SwiftyCrop
import Combine

struct ImageView: View {
    private static let BUTTON_HEIGHT: CGFloat = CGFloat(50)
    
    @State private var showImageCropper: Bool = false
    
    @State private var selectedItem: PhotosPickerItem? = nil
    @State private var selectedImage: UIImage? = nil
    @State private var loadedImage: UIImage? = nil
    @State private var croppedImage: UIImage? = nil
    
    @State private var selectedSavedImage: SavedImage? = nil
    @State private var imageName: String = ""
    @State private var selectionEdited: Bool = false
    
    @State private var showImageNameDialog = false
    
    let matrixInformation: MatrixInformation
    @ObservedObject var imagesController: ImagesController
    @ObservedObject var appController: AppController
    
    init(imagesController: ImagesController,
         matrixInformation: MatrixInformation,
         appController: AppController) {
        self.imagesController = imagesController
        self.matrixInformation = matrixInformation
        self.appController = appController
    }
    
    var body: some View {
        VStack {
            Image(uiImage: (croppedImage ?? loadedImage ?? imagesController.awaitingContent))
                .resizable()
                .scaledToFit()
                .frame(width: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE),
                       height: CGFloat(MatrixAppView.IMAGE_VIEW_SIZE))
                .border(.gray)
            
            PhotosPicker(selection: $selectedItem, matching: .images) {
                Text("Select Image")
                    .underline()
            }
            
            HStack {
                Button(action: {
                    Task {
                        await sendToMatrix()
                    }
                }) {
                    Text("Send to Matrix")
                        .frame(maxWidth: .infinity, maxHeight: .infinity)
                }
                .frame(maxWidth: .infinity, maxHeight: ImageView.BUTTON_HEIGHT)
                .border(.gray)
                .contentShape(Rectangle())
                .padding(10)
                
                Button(action: {
                    if (croppedImage == nil && loadedImage == nil && selectedImage == nil) {
                        appController.displayToastMessage(message: "No image picked", color: .red)
                        return
                    }
                    
                    showImageNameDialog = true
                }) {
                    Text((selectedSavedImage != nil) ? "Update Image" : "Save Image")
                        .frame(maxWidth: .infinity, maxHeight: .infinity)
                }
                .frame(maxWidth: .infinity, maxHeight: ImageView.BUTTON_HEIGHT)
                .border(.gray)
                .contentShape(Rectangle())
                .padding(10)
                .alert("Enter Image Name", isPresented: $showImageNameDialog) {
                    TextField("", text: $imageName)
                    HStack {
                        Button("Submit", role: .cancel) { 
                            Task {
                                await saveUpdateImage()
                            }
                        }
                        
                        Button("Cancel", role: .destructive) { }
                    }
                }
                
            }
            
            List {
                ForEach(imagesController.savedImages, id: \.id) { image in
                    HStack {
                        Image(uiImage: imagesController.imageFromBase64(base64String: image.base64Rendering ?? "invalid"))
                            .resizable()
                            .scaledToFit()
                            .frame(width: 48, height: 48)
                            .border(.gray)
                        Text(image.name)
                        
                        Spacer()
                        
                        if (selectedSavedImage == image) {
                            Image(systemName: "checkmark.circle.fill")
                        }
                    }
                    .frame(maxWidth: .infinity, maxHeight: ImageView.BUTTON_HEIGHT)
                    .contentShape(Rectangle())
                    .onTapGesture {
                        selectedSavedImage = (selectedSavedImage == image) ? nil : image
                        
                        loadedImage = (selectedSavedImage != nil)
                            ? imagesController.imageFromBase64(base64String: image.base64Rendering ?? "invalid")
                            : nil
                        
                        imageName = selectedSavedImage?.name ?? ""
                        croppedImage = nil
                        
                        if (selectedSavedImage != nil) {
                            selectionEdited = false
                        }
                    }
                }
                .onDelete { offsets in
                    let savedImage = imagesController.savedImages[offsets.first!]
                    
                    Task {
                        await appController.executeRequestToToast(task: {
                            await imagesController.deleteImageById(imageId: savedImage.id)
                        }, successMessage: "Successfully deleted image", failureMessage: "Failed to delete image")
                        
                        await requerySavedImages()
                    }
                }
            }
            .refreshable {
                await imagesController.loadSavedImages()
            }
        }
        .task(id: selectedItem) {
            if let data = try? await selectedItem?.loadTransferable(type: Data.self),
               let image = UIImage(data: data) {
                selectedImage = image
                showImageCropper.toggle()
                selectedItem = nil
            }
        }
        .fullScreenCover(isPresented: $showImageCropper) {
            if let selectedImage = selectedImage {
                SwiftyCropView(imageToCrop: selectedImage,
                               maskShape: .square) { croppedImage in
                    self.croppedImage = croppedImage?.resize(width: 64, height: 64)
                    
                    if (selectedSavedImage != nil) {
                        selectionEdited = true
                    }
                }
            }
        }
    }
    
    private func sendToMatrix() async {
        if (selectedSavedImage == nil && croppedImage == nil) {
            appController.displayToastMessage(message: "No image selected", color: .red)
            return
        }
        
        await appController.executeRequestToToast(task: {
            if (selectedSavedImage != nil && !selectionEdited) {
                await imagesController.setMatrixRenderingById(imageId: selectedSavedImage!.id)
            } else {
                await imagesController.postUIImage(image: croppedImage)
            }
        })
    }
    
    private func saveUpdateImage() async {
        if (selectedSavedImage != nil) {
            await appController.executeRequestToToast(task: {
                await imagesController.updateUIImage(imageId: selectedSavedImage!.id, image: croppedImage ?? loadedImage, imageName: imageName)
            }, successMessage: "Successfully updated image", failureMessage: "Failed to update image")
        } else {
            await appController.executeRequestToToast(task: {
                await imagesController.saveUIImage(image: croppedImage, imageName: imageName)
            }, successMessage: "Successfully saved image", failureMessage: "Failed to save image")
        }
        
        await requerySavedImages()
    }
    
    private func requerySavedImages() async {
        await imagesController.loadSavedImages()
        
        if (selectedSavedImage != nil) {
            selectedSavedImage = imagesController.savedImages.first(where: { $0.id == selectedSavedImage!.id })
        }
        
        if (selectedSavedImage == nil) {
            loadedImage = nil
            imageName = ""
        }
    }
}
