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
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            response = try JSONDecoder().decode(T.self, from: responseBody)
        } catch {
            print("GET FAILED: \(error)")
        }
        
        try await httpClient.shutdown()
        
        return response
    }
    
    // TODO: make helpers for private RequestWithBody and RequestWithoutBody that POST/PUT and GET/DELETE respectively call
    func DeleteRequest<T: Decodable>(route: String) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = .DELETE
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            response = try JSONDecoder().decode(T.self, from: responseBody)
        } catch {
            print("GET FAILED: \(error)")
        }
        
        try await httpClient.shutdown()
        
        return response
    }
    
    func PostRequest<T: Decodable>(route: String, body: Codable?) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = .POST
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
            
            if (body != nil) {
                let jsonData = try JSONEncoder().encode(body!)
                request.body = .bytes(jsonData)
            }
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            response = try JSONDecoder().decode(T.self, from: responseBody)
        } catch {
            print("POST FAILED: \(error)")
        }
        
        try await httpClient.shutdown()
        
        return response
    }
        
    func PutRequest<T: Decodable>(route: String, body: Codable?) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = .PUT
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
                        
            if (body != nil) {
                let jsonData = try JSONEncoder().encode(body!)
                request.body = .bytes(jsonData)
            }
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            response = try JSONDecoder().decode(T.self, from: responseBody)
        } catch {
            print("PUT FAILED: \(error)")
        }
        
        try await httpClient.shutdown()
        
        return response
    }
    
    func GetAsImage(route: String, body: Codable?) async throws -> UIImage? {
        if (body == nil) {
            guard let base64String: MatrixResponse<String> = try? await GetRequest(route: route) else {
                return nil
            }
            
            if let base64 = Data(base64Encoded: base64String.data), let uiImage = UIImage(data: base64) {
                return uiImage
            }
        } else {
            guard let base64String: MatrixResponse<String> = try? await PostRequest(route: route, body: body!) else {
                return nil
            }
            
            if let base64 = Data(base64Encoded: base64String.data), let uiImage = UIImage(data: base64) {
                return uiImage
            }
        }
        
        return nil
    }
}
