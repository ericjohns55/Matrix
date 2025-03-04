//
//  AppController.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/2/25.
//

import AlertToast
import SwiftUI
import Foundation

enum LogType {
    case initialization, request, response, uiLoad, debug, data_refresh
}

@MainActor
class AppController: ObservableObject {
    @Published public var toastText: String = "N/A"
    @Published public var toastColor: Color = .gray
    @Published public var showToast = false
    
    func executeRequestToToast<T>(task: @escaping () async throws -> T, successMessage: String? = nil, failureMessage: String? = nil) async {
        showToast = false
        
        do {
            _ = try await task()
            
            toastText = successMessage ?? "Success!"
            toastColor = .green
        } catch let error as NSError {
            if let errorResponse = error.userInfo["error"] as? ErrorResponse {
                toastText = failureMessage ?? "Fail - \(errorResponse.message)"
            } else {
                toastText = failureMessage ?? "Fail - Unknown error"
            }
            
            toastColor = .red
        }
        
        showToast = true
    }
    
    func displayToastMessage(message: String, color: Color) {
        toastText = message
        toastColor = color
        showToast = true
    }
}
