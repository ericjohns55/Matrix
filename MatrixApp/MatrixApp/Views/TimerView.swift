//
//  TimerView.swift
//  MatrixApp
//
//  Created by Eric Johns on 3/4/25.
//

import SwiftUI

enum TimerType {
    case timer, stopwatch
}

struct TimerView: View {
    private static let BUTTON_SIZE: CGFloat = CGFloat(32)
    
    @ObservedObject var matrixController: MatrixController
    @ObservedObject var timerController: TimerController
    @ObservedObject var imagesController: ImagesController
    @ObservedObject var appController: AppController
    
    @State private var timerType: TimerType = .timer
    
    @State private var selectedHour: Int = 0
    @State private var selectedMinute: Int = 0
    @State private var selectedSecond: Int = 0
    
    init(matrixController: MatrixController,
         timerController: TimerController,
         imagesController: ImagesController,
         appController: AppController) {
        self.matrixController = matrixController
        self.timerController = timerController
        self.imagesController = imagesController
        self.appController = appController
    }
    
    var body: some View {
        VStack(spacing: 0) {
            Image(uiImage: (timerController.rendering ?? imagesController.awaitingContent))
                .resizable()
                .scaledToFit()
                .frame(width: MatrixAppView.IMAGE_VIEW_SIZE, height: MatrixAppView.IMAGE_VIEW_SIZE)
                .border(.gray)
                .task {
                    await timerController.loadTimerClockFaces()
                }
            
            Text("Current Timer Status: \(timerController.lastKnownState ?? timerController.lastKnownTimer?.state ?? matrixController.programOverview.timer?.state ?? "None Exists")")
                .task {
                    await timerController.updateTimerState()
                }
        }.padding(.bottom, 10)
        
        HStack {
            Button(action: {
                // start
            }) {
                Image(systemName: "play.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            
            Button(action: {
                // pause/resume
            }) {
                Image(systemName: "pause.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            
            Button(action: {
                // stop
            }) {
                Image(systemName: "stop.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            
            Button(action: {
                // restart
            }) {
                Image(systemName: "arrow.counterclockwise.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
        }.padding(.bottom, 10)
        
        Picker("Timer Type", selection: $timerType) {
            Text("Timer").tag(TimerType.timer)
            Text("Stopwatch").tag(TimerType.stopwatch)
        }
        .onChange(of: timerType, {
            Task { await optionallyUpdateRendering() }
        })
        .pickerStyle(.segmented)
        .padding(.bottom, 10)
        .padding(.horizontal, 10)
        
        LabeledContent {
            Picker("Timer Face", selection: $timerController.selectedTimerFace) {
                ForEach(timerController.timerClockFaces) { timerFace in
                    HStack {
                        Text(timerFace.name)
                    }.tag(timerFace as ClockFace?)
                }
            } currentValueLabel: {
                Text(timerController.selectedTimerFace?.name ?? "Select a Timer Face")
            }
        } label: {
            Text("Timer Face")
        }
        .padding(.horizontal, 10)
        
        VStack {
            if (timerType == .timer) {
                HStack {
                    Picker("Hour", selection: $selectedHour) {
                        ForEach(0..<24, id: \.self) { hour in
                            Text("\(hour) h").tag(hour)
                        }
                    }
                    .pickerStyle(.wheel)
                    .onChange(of: selectedHour, {
                        Task { await optionallyUpdateRendering() }
                    })
                    
                    Picker("Minute", selection: $selectedMinute) {
                        ForEach(0..<59, id: \.self) { minute in
                            Text("\(minute) m").tag(minute)
                        }
                    }
                    .pickerStyle(.wheel)
                    .onChange(of: selectedMinute, {
                        Task { await optionallyUpdateRendering() }
                    })
                    
                    Picker("Second", selection: $selectedSecond) {
                        ForEach(0..<59, id: \.self) { second in
                            Text("\(second) s").tag(second)
                        }
                    }
                    .pickerStyle(.wheel)
                    .onChange(of: selectedSecond, {
                        Task { await optionallyUpdateRendering() }
                    })
                }
            } else {
                Spacer()
                Text("Stopwatches always start at 00:00")
                Spacer()
            }
        }
        .frame(maxHeight: .infinity)
    }
    
    private func optionallyUpdateRendering() async {
        if (timerController.selectedTimerFace == nil) {
            return
        }
        
        var timerPayload: TimerCodable
        
        if (timerType == .timer) {
            timerPayload = TimerCodable(
                hour: selectedHour,
                minute: selectedMinute,
                second: selectedSecond,
                timerFaceId: timerController.selectedTimerFace!.id,
                isStopwatch: false)
        } else {
            timerPayload = TimerCodable(
                hour: 0,
                minute: 0,
                second: 0,
                timerFaceId: timerController.selectedTimerFace!.id,
                isStopwatch: true)
        }
        
        await timerController.renderTimer(timer: timerPayload)
    }
}
