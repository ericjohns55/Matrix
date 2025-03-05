//
//  TimerController.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/4/25.
//

import Foundation
import SwiftUI

@MainActor
class TimerController: ObservableObject {
    @Published var timerClockFaces: [ClockFace] = []
    @Published var lastKnownTimer: MatrixTimer? = nil
    @Published var lastKnownState: String? = nil
    
    @Published var selectedTimerFace: ClockFace? = nil
    @Published var rendering: UIImage? = nil
    
    func loadTimerClockFaces() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[ClockFace]> = try? await client.GetRequest(route: "clockface?timerFace=true") else {
            self.timerClockFaces = []
            return
        }
        
        self.timerClockFaces = matrixResponse.data
        self.selectedTimerFace = self.timerClockFaces.first
    }
    
    func createTimer(timer: TimerCodable) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<MatrixTimer> = try? await client.PostRequest(route: "timer/create?alsoStart=true", body: timer) else {
            return
        }
        
        self.lastKnownTimer = matrixResponse.data
    }
    
    func updateTimerState() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let matrixResponse: MatrixResponse<String> = try? await client.GetRequest(route: "timer/state") else {
            lastKnownState = "Unknown"
            return
        }
            
        lastKnownState = matrixResponse.data
    }
    
    func renderTimer(timer: TimerCodable) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let image = try? await client.GetAsImage(route: "timer/render?trimHeader=true&scaleFactor=4", body: timer) else {
            return
        }
        
        self.rendering = image
    }
    
    func stopTimer() async {
        return await modifyTimerState(route: "timer/stop")
    }
    
    func pauseTimer() async {
        return await modifyTimerState(route: "timer/pause")
    }
    
    func resumeTimer() async {
        return await modifyTimerState(route: "timer/resume")
    }
    
    func resetStopwatch() async {
        return await modifyTimerState(route: "timer/reset")
    }
    
    private func modifyTimerState(route: String) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let matrixResponse: MatrixResponse<MatrixTimer> = try? await client.PostRequest(route: route, body: nil) else {
            return
        }
            
        self.lastKnownTimer = matrixResponse.data
    }
}
