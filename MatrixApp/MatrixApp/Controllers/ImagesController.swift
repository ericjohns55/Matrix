//
//  RenderedImageViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import UIKit
import Foundation

struct ImagePayload: Codable {
    var base64Image: String
}

@MainActor
class ImagesController: ObservableObject {
    @Published var matrixRendering: UIImage = UIImage()
    
    @Published var awaitingContent: UIImage
    @Published var couldNotConnect: UIImage
    @Published var failedToLoad: UIImage
    @Published var invalidContent: UIImage
    
    init() {
        let emptyImage = ImagesController.createEmptyImage()
        
        self.awaitingContent = ImagesController.loadImageFromAssetsOrDefault(assetName: "AwaitingContent", emptyImage: emptyImage)
        
        self.couldNotConnect = ImagesController.loadImageFromAssetsOrDefault(assetName: "CouldNotConnect", emptyImage: emptyImage)
        
        self.failedToLoad = ImagesController.loadImageFromAssetsOrDefault(assetName: "FailedToLoad", emptyImage: emptyImage)
        
        self.invalidContent = ImagesController.loadImageFromAssetsOrDefault(assetName: "InvalidContent", emptyImage: emptyImage)
        
        matrixRendering = awaitingContent
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
    
    func postUIImage(image: UIImage?) async {
        if let imageData = image?.pngData() {
            let imagePayload = ImagePayload(base64Image: imageData.base64EncodedString())
            
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            guard let _: MatrixResponse<String> = try? await client.PostRequest(route: "image/base64", body: imagePayload) else {
                return
            }
        }
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
