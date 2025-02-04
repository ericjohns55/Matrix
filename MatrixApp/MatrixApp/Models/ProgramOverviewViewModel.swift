//
//  ProgramOverviewViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

import Foundation

struct ProgramOverview: Decodable {
    let matrixState: String
    let brightness: Int
    let updateInterval: Int
    let currentVariables: Dictionary<String, String>?
    let timer: MatrixTimer?
    let plainText: PlainText?
    let scrollingText: ScrollingText?
    let currentClockFace: ClockFace?
    let overridenClockFace: ClockFace?
}

@MainActor
class ProgramOverviewViewModel: ObservableObject {
    @Published var programOverview: ProgramOverview = ProgramOverview(matrixState: "Unknown", brightness: -1, updateInterval: -1, currentVariables: nil, timer: nil, plainText: nil, scrollingText: nil, currentClockFace: nil, overridenClockFace: nil)
    
    func getProgramOverview() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<ProgramOverview> = try? await client.GetRequest(route: "matrix/overview") else {
            programOverview = ProgramOverview(matrixState: "Unknown", brightness: -1, updateInterval: -1, currentVariables: nil, timer: nil, plainText: nil, scrollingText: nil, currentClockFace: nil, overridenClockFace: nil)
            return
        }
        
        self.programOverview = matrixResponse.data
    }
}
