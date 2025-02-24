//
//  CropOverlay.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/23/25.
//

import SwiftUI

struct CropOverlay: View {
    @Binding var cropRect: CGRect
    var imageSize: CGFloat
    
    @GestureState private var pinchScale: CGFloat = 1.0
    @State private var lastScale: CGFloat = 1.0
    
    var body: some View {
        ZStack {
            Rectangle()
                .fill(Color.black.opacity(0.5))
                .mask(Rectangle()
                    .fill(Color.black.opacity(0.7))
                    .overlay(Rectangle()
                        .stroke(Color.white, lineWidth: 2)
                        .frame(width: cropRect.width, height: cropRect.height)
                        .position(x: cropRect.midX, y: cropRect.midY)))
                .gesture(DragGesture()
                    .onChanged { value in
                        cropRect.origin.x = min(max(0, value.location.x - cropRect.width / 2), imageSize - cropRect.width)
                        cropRect.origin.y = min(max(0, value.location.y - cropRect.height / 2), imageSize - cropRect.height)
                    }
                )
                .gesture(
                    MagnificationGesture()
                        .updating($pinchScale) { value, state, _ in
                            state = value
                        }
                        .onChanged { value in
                            let newSize = cropRect.width * (value / lastScale)
                            let clampedSize = min(max(50, newSize), imageSize)
                            
                            cropRect.size.width = clampedSize
                            cropRect.size.height = clampedSize
                            lastScale = value
                        }
                        .onEnded { _ in
                            lastScale = 1.0
                        }
                )
        }
    }
}
