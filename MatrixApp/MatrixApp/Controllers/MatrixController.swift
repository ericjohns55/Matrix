//
//  ProgramOverviewViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import Foundation

struct MatrixInformation: Decodable {
    let brightness: Int
    let width: Int
    let height: Int
}

struct ProgramOverview: Decodable {
    let matrixState: String
    let matrixInformation: MatrixInformation?
    let updateInterval: Int
    let currentVariables: Dictionary<String, String>?
    let timer: MatrixTimer?
    let plainText: PlainText?
    let scrollingText: ScrollingText?
    let currentClockFace: ClockFace?
    let overridenClockFace: ClockFace?
}

@MainActor
class MatrixController: ObservableObject {
    @Published var programOverview: ProgramOverview = ProgramOverview(matrixState: "Unknown", matrixInformation: MatrixInformation(brightness: -1, width: -1, height: -1), updateInterval: -1, currentVariables: nil, timer: nil, plainText: nil, scrollingText: nil, currentClockFace: nil, overridenClockFace: nil)
    
    func getProgramOverview() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<ProgramOverview> = try? await client.GetRequest(route: "matrix/overview") else {
            programOverview = ProgramOverview(matrixState: "Unknown", matrixInformation: MatrixInformation(brightness: -1, width: -1, height: -1), updateInterval: -1, currentVariables: nil, timer: nil, plainText: nil, scrollingText: nil, currentClockFace: nil, overridenClockFace: nil)
            return
        }
        
        self.programOverview = matrixResponse.data
    }
    
    func restoreMatrix() async -> ProgramOverview {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let response: MatrixResponse<ProgramOverview> = try? await client.PostRequest(route: "matrix/restore", body: nil) else {
            return programOverview
        }
        
        programOverview = response.data
        return programOverview
    }
}
