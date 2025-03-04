//
//  RenderedImageViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import UIKit
import SwiftUI
import Foundation

struct ImagePayload: Codable {
    var imageName: String?
    var base64Image: String
}

@MainActor
class ImagesController: BaseController {
    @Published var matrixRendering: UIImage?
    
    @Published var awaitingContent: UIImage
    @Published var couldNotConnect: UIImage
    @Published var failedToLoad: UIImage
    @Published var invalidContent: UIImage
    @Published var loading: UIImage
    
    @Published var savedImages: [SavedImage] = []
    @Published var savedImagesWithNone: [SavedImage] = []
    
    public var emptyImage: SavedImage = SavedImage(id: -1, name: "(None)", fileName: "none.png")
        
    override init() {
        let emptyImage = ImagesController.createEmptyImage()
        
        self.loading = ImagesController.loadImageFromAssetsOrDefault(assetName: "Loading", emptyImage: emptyImage)
        
        self.matrixRendering = ImagesController.loadImageFromAssetsOrDefault(assetName: "Loading", emptyImage: emptyImage)
        
        self.awaitingContent = ImagesController.loadImageFromAssetsOrDefault(assetName: "AwaitingContent", emptyImage: emptyImage)
        
        self.couldNotConnect = ImagesController.loadImageFromAssetsOrDefault(assetName: "CouldNotConnect", emptyImage: emptyImage)
        
        self.failedToLoad = ImagesController.loadImageFromAssetsOrDefault(assetName: "FailedToLoad", emptyImage: emptyImage)
        
        self.invalidContent = ImagesController.loadImageFromAssetsOrDefault(assetName: "InvalidContent", emptyImage: emptyImage)
    }
    
    private static func loadImageFromAssetsOrDefault(assetName: String, emptyImage: UIImage) -> UIImage {
        if let image = UIImage(named: assetName) {
            return image
        } else {
            return emptyImage
        }
    }
        
    func fetchMatrixRendering() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let image: UIImage = try? await client.GetAsImage(route: "image/render?trimHeader=true&scaleFactor=4", body: nil) else {
            self.matrixRendering = failedToLoad
            return
        }
        
        self.matrixRendering = image
    }
    
    func loadSavedImages() async {
        self.savedImages = []
        self.savedImagesWithNone = [emptyImage]
        
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[SavedImage]> = try? await client.GetRequest(route: "image/saved?trimHeader=true") else {
            self.savedImages = []
            return
        }
        
        self.savedImages = matrixResponse.data
        self.savedImagesWithNone = [emptyImage] + self.savedImages
    }
    
    func setMatrixRenderingById(imageId: Int) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
                    
        guard let _: MatrixResponse<SavedImage> = try? await client.PostRequest(route: "image/\(imageId)", body: nil) else {
            print("Failed to post image")
            return
        }
    }
    
    func postUIImage(image: UIImage?) async {
        if let imageData = image?.pngData() {
            let imagePayload = ImagePayload(base64Image: imageData.base64EncodedString())
            
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            guard let _: MatrixResponse<String> = try? await client.PostRequest(route: "image/base64", body: imagePayload) else {
                return
            }
        }
    }
    
    func saveUIImage(image: UIImage?, imageName: String) async {
        if let imageData = image?.pngData() {
            let imagePayload = ImagePayload(imageName: imageName, base64Image: imageData.base64EncodedString())
                
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            guard let _: MatrixResponse<SavedImage> = try? await client.PostRequest(route: "image/save", body: imagePayload) else {
                return
            }
        }
    }
    
    func updateUIImage(imageId: Int, image: UIImage?, imageName: String) async {
        if let imageData = image?.pngData() {
            let imagePayload = ImagePayload(imageName: imageName, base64Image: imageData.base64EncodedString())
                
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            guard let _: MatrixResponse<SavedImage> = try? await client.PutRequest(route: "image/\(imageId)", body: imagePayload) else {
                return
            }
        }
    }
    
    func deleteImageById(imageId: Int) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        guard let _: MatrixResponse<SavedImage> = try? await client.DeleteRequest(route: "image/\(imageId)") else {
            return
        }
    }
        
    func imageFromBase64(base64String: String, returnEmptyImage: Bool = false) -> UIImage {
        if let base64 = Data(base64Encoded: base64String), let uiImage = UIImage(data: base64) {
            return uiImage
        }
        
        return returnEmptyImage ? ImagesController.createEmptyImage() : self.failedToLoad
    }
    
    private static func createEmptyImage() -> UIImage {
        let imageSize = CGSize(width: 64, height: 64)
        
        let renderer = UIGraphicsImageRenderer(size: imageSize)
        
        return renderer.image { context in
            UIColor.black.setFill()
            context.fill(CGRect(origin: .zero, size: imageSize))
        }
    }
}
