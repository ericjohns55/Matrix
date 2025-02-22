//
//  ColorsViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/29/25.
//

import UIKit
import Foundation

struct PlainTextValidationResponse {
    var successfullyValidated: Bool
    var invalidFields: String
    var payload: PlainTextPayload?
}

struct ScrollingTextValidationResponse {
    var successfullyValidated: Bool
    var invalidFields: String
    var payload: ScrollingTextPayload?
}

@MainActor
class TextController: ObservableObject {
    static let validText: String = "Valid"
    let scaleFactor: Int = 2
    
    @Published var colors: [MatrixColor] = [MatrixColor(id: -1, name: "Temp", red: 0, green: 0, blue: 0, deleted: false)]
    @Published var fonts: [MatrixFont] = [MatrixFont(id: -1, name: "Test", fileLocation: "none", width: -1, height: -1)]
    
    @Published var renderedPreview: UIImage? = nil
    
    func validatePlainText(text: String, color: MatrixColor?, font: MatrixFont?, alignment: String, splitByWord: Bool) -> PlainTextValidationResponse {
        var invalidFieldsList: [String] = []
        
        if (text.isEmpty) {
            invalidFieldsList.append("Text")
        }
        
        if (color == nil) {
            invalidFieldsList.append("Color")
        }
        
        if (font == nil) {
            invalidFieldsList.append("Font")
        }
        
        let validAlignments = ["Left", "Center", "Right"]
        if (alignment.isEmpty || !validAlignments.contains(alignment)) {
            invalidFieldsList.append("Alignment")
        }
        
        let invalidFieldsFormatted = invalidFieldsList.isEmpty ? TextController.validText : invalidFieldsList.joined(separator: ", ")
        
        var payload: PlainTextPayload? = nil
        if (invalidFieldsList.isEmpty) {
            payload = PlainTextPayload(text: text, textAlignment: alignment, splitByWord: splitByWord, matrixColorId: color!.id, matrixFontId: font!.id)
        }
        
        return PlainTextValidationResponse(successfullyValidated: invalidFieldsList.isEmpty, invalidFields: invalidFieldsFormatted, payload: payload)
    }
    
    func validateScrollingText(text: String, scrollingDelay: Int, iterations: Int, color: MatrixColor?, font: MatrixFont?) -> ScrollingTextValidationResponse {
        var invalidFieldsList: [String] = []
        
        if (text.isEmpty) {
            invalidFieldsList.append("Text")
        }
        
        if (scrollingDelay < 0) {
            invalidFieldsList.append("ScrollingDelay")
        }
        
        if (iterations < -1) {
            invalidFieldsList.append("Iterations")
        }
        
        if (color == nil) {
            invalidFieldsList.append("Color")
        }
        
        if (font == nil) {
            invalidFieldsList.append("Font")
        }
        
        
        let invalidFieldsFormatted = invalidFieldsList.isEmpty ? TextController.validText : invalidFieldsList.joined(separator: ", ")
        
        var payload: ScrollingTextPayload? = nil
        if (invalidFieldsList.isEmpty) {
            payload = ScrollingTextPayload(
                text: text,
                scrollingDelay: scrollingDelay,
                iterations: iterations == 0 ? -1 : iterations, // numberPad does not permit negatives
                matrixColorId: color!.id,
                matrixFontId: font!.id)
        }
        
        return ScrollingTextValidationResponse(successfullyValidated: invalidFieldsList.isEmpty, invalidFields: invalidFieldsFormatted, payload: payload)
    }
    
    func tryRenderPlainTextPreview(text: String, color: MatrixColor?, font: MatrixFont?, alignment: String, splitByWord: Bool) async -> UIImage? {
        let validationResponse = validatePlainText(text: text, color: color, font: font, alignment: alignment, splitByWord: splitByWord)
        
        if (validationResponse.successfullyValidated) {
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            let payload = validationResponse.payload!
                        
            guard let image: UIImage = try? await client.GetAsImage(route: "text/plain/render?trimHeader=true&scaleFactor=\(scaleFactor)", body: payload) else {
                return nil
            }
            
            return image
        }
        
        return nil
    }
    
    func tryRenderScrollingTextPreview(text: String, scrollingInterval: Int, iterations: Int, color: MatrixColor?, font: MatrixFont?) async -> UIImage? {
        let validationResponse = validateScrollingText(text: text, scrollingDelay: scrollingInterval, iterations: iterations, color: color, font: font)
        
        if (validationResponse.successfullyValidated) {
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            let payload = validationResponse.payload!
            
            guard let image: UIImage = try? await client.GetAsImage(route: "text/scrolling/render?trimHeader=true&cropToMatrixSize=false&scaleFactor=\(scaleFactor)", body: payload) else {
                return nil
            }
            
            return image
        }
        
        return nil
    }
    
    func tryPostText(text: String, color: MatrixColor?, font: MatrixFont?, alignment: String, splitByWord: Bool) async {
        let validationResponse = validatePlainText(text: text, color: color, font: font, alignment: alignment, splitByWord: splitByWord)
        
        if (validationResponse.successfullyValidated) {
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            let payload = validationResponse.payload!
            
            guard let _: MatrixResponse<PlainText> = try? await client.PostRequest(route: "text/plain", body: payload) else {
                return
            }
        }
    }
    
    func tryPostScrollingText(text: String, scrollingInterval: Int, iterations: Int, color: MatrixColor?, font: MatrixFont?) async {
        let validationResponse = validateScrollingText(text: text, scrollingDelay: scrollingInterval, iterations: iterations, color: color, font: font)
        
        
        if (validationResponse.successfullyValidated) {
            let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            let payload = validationResponse.payload!
            
            guard let _: MatrixResponse<ScrollingText> = try? await client.PostRequest(route: "text/scrolling", body: payload) else {
                return
            }
        }
    }
    
    func loadAllData() async {
        await self.getColors()
        await self.getFonts()
    }
    
    func getColors() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[MatrixColor]> = try? await client.GetRequest(route: "colors") else {
            self.colors = []
            return
        }
        
        self.colors = matrixResponse.data
    }
    
    func getFonts() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[MatrixFont]> = try? await client.GetRequest(route: "text/fonts") else {
            self.fonts = []
            return
        }
        
        self.fonts = matrixResponse.data
    }
}
