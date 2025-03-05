//
//  ClockFaceController.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/2/25.
//

import Foundation
import SwiftUI
import UIKit

@MainActor
class ClockFaceController: ObservableObject {
    @Published var clockFaces: [ClockFace] = []
    @Published var overridenClockFace: ClockFace? = nil
    
    @Published var currentClockFaceImage: UIImage? = nil
    @Published var overridenClockFaceImage: UIImage? = nil
    
    func loadClockFaces() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[ClockFace]> = try? await client.GetRequest(route: "clockface?render=true&scaleFactor=4") else {
            self.clockFaces = []
            return
        }
        
        self.clockFaces = matrixResponse.data
        
        let currentClockFace = clockFaces.first(where: { $0.isCurrentFace })
        self.currentClockFaceImage = loadRenderingFromClockFace(clockFace: currentClockFace)
        
        await loadOverridenClockFaces()
    }
    
    func isOverridenFace(face: ClockFace?) -> Bool {
        if (face != nil) {
            if (overridenClockFace != nil) {
                if (overridenClockFace!.id == face!.id) {
                    return true
                }
            }
        }
        
        return false
    }
    
    private func loadOverridenClockFaces() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let matrixResponse: MatrixResponse<ClockFace?> = try? await client.GetRequest(route: "clockface/override?render=true&scaleFactor=4") else {
            self.overridenClockFace = nil
            return
        }
            
        self.overridenClockFace = matrixResponse.data
        self.overridenClockFaceImage = loadRenderingFromClockFace(clockFace: self.overridenClockFace)
    }
    
    func overrideClockFace(clockFaceId: Int) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
                
        guard let matrixResponse: MatrixResponse<ClockFace> = try? await client.PostRequest(route: "clockface/override/\(clockFaceId)?render=true&scaleFactor=4", body: nil) else {
            self.overridenClockFace = nil
            return
        }
                
        self.overridenClockFace = matrixResponse.data
        self.overridenClockFaceImage = loadRenderingFromClockFace(clockFace: self.overridenClockFace)
    }
    
    func cancelOverride() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let _: MatrixResponse<Bool> = try? await client.PostRequest(route: "clockface/override", body: nil) else {
            return
        }
        
        self.overridenClockFace = nil
        self.overridenClockFaceImage = nil
    }
    
    private func loadRenderingFromClockFace(clockFace: ClockFace?) -> UIImage? {
        if (clockFace != nil) {
            if let base64 = Data(base64Encoded: clockFace?.base64Rendering ?? "invalid"), let uiImage = UIImage(data: base64) {
                return uiImage
            }
        }
        
        return nil
    }
}
