//
//  RenderedImageViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import UIKit
import Foundation

@MainActor
class RenderedImageViewModel: ObservableObject {
    @Published var image: UIImage = UIImage()
        
    func fetchDecodeImage() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let base64String: MatrixResponse<String> = try? await client.GetRequest(route: "image/render?trimHeader=true") else {
            self.image = createEmptyImage()
            return
        }
        
        if let base64Data = Data(base64Encoded: base64String.data), let uiImage = UIImage(data: base64Data) {
            self.image = uiImage.resize(width: 128, height: 128)
        } else {
            self.image = createEmptyImage()
        }
    }
    
    func createEmptyImage() -> UIImage {
        let imageSize = CGSize(width: 64, height: 64)
        
        let renderer = UIGraphicsImageRenderer(size: imageSize)
        
        return renderer.image { context in
            UIColor.blue.setFill()
            context.fill(CGRect(origin: .zero, size: imageSize))
        }
    }
}
