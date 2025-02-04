//
//  ColorsViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/29/25.
//

import UIKit
import Foundation

struct MatrixColor: Identifiable, Decodable {
    let id: Int
    let name: String
    let red: Int
    let green: Int
    let blue: Int
    let deleted: Bool
}

@MainActor
class ColorsViewModel: ObservableObject {
    @Published var colors: [MatrixColor] = []
    
    func getColors() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[MatrixColor]> = try? await client.GetRequest(route: "colors") else {
            self.colors = []
            return
        }
        
        self.colors = matrixResponse.data
    }
}
