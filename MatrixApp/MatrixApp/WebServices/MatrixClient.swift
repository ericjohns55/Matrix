//
//  MatrixClient.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/28/25.
//

import UIKit
import Foundation
import AsyncHTTPClient

struct MatrixResponse<T: Decodable>: Decodable {
    let data: T
    let elapsedMilliseconds: Int
}

class MatrixClient {
    var _serverUrl: String
    var _encodedApiKey: String
    
    init (serverUrl: String, apiKey: String) {
        _serverUrl = serverUrl
        _encodedApiKey = apiKey.data(using: .utf8)!.base64EncodedString()
    }
    
    func GetRequest<T: Decodable>(route: String) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = .GET
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            response = try JSONDecoder().decode(T.self, from: responseBody)
        } catch {
            print("GET FAILED: \(error)")
        }
        
        try await httpClient.shutdown()
        
        return response
    }
    
    func RenderMatrixImage() async throws -> UIImage? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var uiImage: UIImage?
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)image/render")
            request.method = .GET
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            let data = Data(buffer: responseBody)
            
            if let image = UIImage(data: data) {
                uiImage = image
            }
        } catch {
            print(error)
        }
        
        try await httpClient.shutdown()
        
        return uiImage
    }
}
