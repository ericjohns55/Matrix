//
//  ImageView.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/21/25.
//

import SwiftUI
import PhotosUI
import SwiftyCrop

struct ImageView: View {
    @State private var keyboardResponder = KeyboardResponder()
    @State private var showImageCropper: Bool = false
    
    @State private var selectedItem: PhotosPickerItem? = nil
    @State private var selectedImage: UIImage? = nil
    @State private var croppedImage: UIImage? = nil
    
    let matrixInformation: MatrixInformation
    let imagesController: ImagesController
    
    init(imagesController: ImagesController, matrixInformation: MatrixInformation) {
        self.imagesController = imagesController
        self.matrixInformation = matrixInformation
    }
    
    var body: some View {
        VStack {
            Image(uiImage: (selectedImage ?? imagesController.awaitingContent))
                .resizable()
                .scaledToFit()
                .frame(width: CGFloat(ContentView.IMAGE_VIEW_SIZE),
                       height: CGFloat(ContentView.IMAGE_VIEW_SIZE))
                .border(.gray)
                .onTapGesture {
                    Task {
                        await imagesController.postUIImage(image: croppedImage)
                    }
                }
            
            PhotosPicker(selection: $selectedItem, matching: .images) {
                Text("Select")
            }
            
            if let croppedImage {
                Image(uiImage: croppedImage)
                    .resizable()
                    .scaledToFit()
                    .frame(width: ContentView.IMAGE_VIEW_SIZE, height: ContentView.IMAGE_VIEW_SIZE)
            }
        }
        .padding(.bottom, keyboardResponder.keyboardHeight)
        .animation(.easeOut(duration: 0.2), value: keyboardResponder.keyboardHeight)
        .background(Color(UIColor.systemBackground))
        .onTapGesture {
            UIApplication.shared.finishEditing()
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
                    
                    if let imageData = self.croppedImage?.pngData() {
                        print(imageData.base64EncodedString())
                    }
                }
            }
        }
    }
}
