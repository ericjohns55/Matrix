//
//  FontsViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import SwiftUI
import Foundation

struct MatrixFont: Identifiable, Decodable {
    let id: Int
    let name: String
    let fileLocation: String
    let width: Int
    let height: Int
}


@MainActor
class FontsViewModel: ObservableObject {
    @Published var fonts: [MatrixFont] = []
    
    func getFonts() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[MatrixFont]> = try? await client.GetRequest(route: "text/fonts") else {
            self.fonts = []
            return
        }
        
        self.fonts = matrixResponse.data
    }
}
