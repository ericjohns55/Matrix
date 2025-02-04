//
//  ImageExtensions.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import UIKit
import AVFoundation

public extension UIImage {
    func resize(width: Int, height: Int) -> UIImage {
        let newSize = CGSize(width: width, height: height)
        
        let format = UIGraphicsImageRendererFormat()
        format.scale = 1
        
        let resized = UIGraphicsImageRenderer(size: newSize, format: format).image { _ in
            self.draw(in: CGRect(origin: .zero, size: newSize))
        }
        
        return resized
    }
}
