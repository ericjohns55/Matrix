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
    
    @Published var isRunning: Bool = false
    
    func loadTimerClockFaces() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<[ClockFace]> = try? await client.GetRequest(route: "clockface?timerFace=true") else {
            self.timerClockFaces = []
            return
        }
        
        self.timerClockFaces = matrixResponse.data
        self.selectedTimerFace = self.timerClockFaces.first
    }
    
    func loadCurrentTimer() async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let matrixResponse: MatrixResponse<MatrixTimer> = try? await client.GetRequest(route: "timer/current") else {
            return
        }
            
        self.lastKnownTimer = matrixResponse.data
        self.updateRunningStatus()
    }
    
    func createTimer(timer: TimerCodable) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let matrixResponse: MatrixResponse<MatrixTimer> = try? await client.PostRequest(route: "timer/create", body: timer) else {
            return
        }
        
        self.lastKnownTimer = matrixResponse.data
        self.updateRunningStatus()
    }
    
    func renderTimer(timer: TimerCodable) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
        
        guard let image = try? await client.GetAsImage(route: "timer/render?trimHeader=true&scaleFactor=4", body: timer) else {
            return
        }
        
        self.rendering = image
    }
    
    func startTimer() async {
        return await modifyTimerState(route: "timer/start")
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
    
    private func updateRunningStatus() {
        self.isRunning = ["Running", "Blinking"].contains(self.lastKnownTimer?.state)
    }
    
    private func modifyTimerState(route: String) async {
        let client = MatrixClient(serverUrl: MatrixApp.ServerUrl, apiKey: MatrixApp.ApiKey)
            
        guard let matrixResponse: MatrixResponse<MatrixTimer> = try? await client.PostRequest(route: route, body: nil) else {
            return
        }
            
        self.lastKnownTimer = matrixResponse.data
        self.updateRunningStatus()
    }
}
