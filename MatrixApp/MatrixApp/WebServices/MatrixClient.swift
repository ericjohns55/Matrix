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

struct ErrorResponse: Decodable {
    let statusCode: Int
    let message: String
    let details: String
}

enum ClientError: Error {
    case runtimeError(String)
}

class MatrixClient {
    var _serverUrl: String
    var _encodedApiKey: String
    
    init (serverUrl: String, apiKey: String) {
        _serverUrl = serverUrl
        _encodedApiKey = apiKey.data(using: .utf8)!.base64EncodedString()
    }
    
    private func RequestNoBody<T: Decodable>(route: String, getRequest: Bool = true) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        defer {
            Task {
                try await httpClient.shutdown()
            }
        }
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = getRequest ? .GET : .DELETE
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
            
            print("[REQUEST] \(String(describing: request.method).uppercased()) - \(route)")
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            if (200...299).contains(httpResponse.status.code) {
                response = try JSONDecoder().decode(T.self, from: responseBody)
                print("[RESPONSE] \(httpResponse.status.code) - returning \(T.self)")
            } else {
                let errorResponse = try JSONDecoder().decode(ErrorResponse.self, from: responseBody)
                print("[RESPONSE] \(httpResponse.status.code) - \(errorResponse.message)")
                throw NSError(domain: "Server Error", code: Int(httpResponse.status.code), userInfo: ["error": errorResponse])
            }
        } catch {
            throw error
        }
        
        return response
    }
    
    private func RequestWithBody<T: Decodable>(route: String, body: Codable?, postRequest: Bool = true) async throws -> T? {
        let httpClient = HTTPClient(eventLoopGroupProvider: .singleton)
        var response: T?
        
        defer {
            Task {
                try await httpClient.shutdown()
            }
        }
        
        do {
            var request = HTTPClientRequest(url: "\(_serverUrl)\(route)")
            request.method = postRequest ? .POST : .PUT
            request.headers.add(contentsOf: ["Authorization": "Basic \(_encodedApiKey)"])
            request.headers.add(contentsOf: ["Content-Type": "application/json"])
            
            print("[REQUEST] \(String(describing: request.method).uppercased()) - \(route)")
            
            if (body != nil) {
                let jsonData = try JSONEncoder().encode(body!)
                request.body = .bytes(jsonData)
            }
            
            let httpResponse = try await httpClient.execute(request, timeout: .seconds(3))
            let responseBody = try await httpResponse.body.collect(upTo: 1024 * 1024)
            
            if (200...299).contains(httpResponse.status.code) {
                response = try JSONDecoder().decode(T.self, from: responseBody)
                print("[RESPONSE] \(httpResponse.status.code) - returning \(T.self)")
            } else {
                let errorResponse = try JSONDecoder().decode(ErrorResponse.self, from: responseBody)
                print("[RESPONSE] \(httpResponse.status.code) - \(errorResponse.message)")
                throw NSError(domain: "Server Error", code: Int(httpResponse.status.code), userInfo: ["error": errorResponse])
            }
        } catch {
            throw error
        }
        
        return response
    }
    
    func GetRequest<T: Decodable>(route: String) async throws -> T? {
        return try await RequestNoBody(route: route)
    }
    
    func DeleteRequest<T: Decodable>(route: String) async throws -> T? {
        return try await RequestNoBody(route: route, getRequest: false)
    }
    
    func PostRequest<T: Decodable>(route: String, body: Codable?) async throws -> T? {
        return try await RequestWithBody(route: route, body: body)
    }
        
    func PutRequest<T: Decodable>(route: String, body: Codable?) async throws -> T? {
        return try await RequestWithBody(route: route, body: body, postRequest: false)
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
