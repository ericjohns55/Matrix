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
                    await optionallyLoadTimerFromMatrix()
                }
        }.padding(.bottom, 10)
        
        HStack {
            Button(action: {
                if (timerType == .timer && selectedHour == 0 && selectedMinute == 0 && selectedSecond == 0) {
                    appController.displayToastMessage(message: "Invalid time frame for timer", color: .red)
                    return
                }
                
                Task {
                    await appController.executeRequestToToast(task: {
                        await timerController.createTimer(timer: createTimerPayload())
                    }, successMessage: "Successfully created timer", failureMessage: "Failed to create timer")
                }
            }) {
                Image(systemName: "arrowtriangle.up.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            
            Button(action: {
                if (timerController.lastKnownTimer == nil) {
                    appController.displayToastMessage(message: "No timer currently exists", color: .red)
                    return
                }
                
                Task {
                    if (!timerController.isRunning) {
                        if (timerController.lastKnownTimer?.state == "Waiting") {
                            await appController.executeRequestToToast(task: {
                                await timerController.startTimer()
                            }, successMessage: "Started timer", failureMessage: "Could not start timer")
                        } else {
                            await appController.executeRequestToToast(task: {
                                await timerController.resumeTimer()
                            }, successMessage: "Resumed timer", failureMessage: "Could not resume timer")
                        }
                    } else {
                        await appController.executeRequestToToast(task: {
                            await timerController.pauseTimer()
                        }, successMessage: "Paused timer", failureMessage: "Could not pause timer")
                    }
                }
            }) {
                Image(systemName: (timerController.isRunning ? "pause.circle" : "play.circle"))
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            
            Button(action: {
                if (timerController.lastKnownTimer == nil) {
                    appController.displayToastMessage(message: "No timer currently exists", color: .red)
                    return
                }
                
                Task {
                    await appController.executeRequestToToast(task: {
                        await timerController.stopTimer()
                    }, successMessage: "Successfully cancelled timer", failureMessage: "Could not stop timer")
                }
            }) {
                Image(systemName: "stop.circle")
                    .resizable()
                    .renderingMode(.original)
                    .scaledToFit()
                    .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
            }.padding(.horizontal, 10)
            
            if (timerType == .stopwatch || timerController.lastKnownTimer?.isStopwatch ?? false) {
                Button(action: {
                    if (timerController.lastKnownTimer == nil) {
                        appController.displayToastMessage(message: "No timer currently exists", color: .red)
                        return
                    }
                    
                    Task {
                        await appController.executeRequestToToast(task: {
                            await timerController.resetStopwatch()
                        }, successMessage: "Successfully reset stopwatch", failureMessage: "Could not reset stopwatch")
                    }
                }) {
                    Image(systemName: "arrow.counterclockwise.circle")
                        .resizable()
                        .renderingMode(.original)
                        .scaledToFit()
                        .frame(width: TimerView.BUTTON_SIZE, height: TimerView.BUTTON_SIZE)
                }.padding(.horizontal, 10)
            }
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
        
        await timerController.renderTimer(timer: createTimerPayload())
    }
    
    private func optionallyLoadTimerFromMatrix() async {
        if (matrixController.programOverview.matrixState != "Timer") {
            return
        }
        
        await timerController.loadCurrentTimer()
        
        if (timerController.lastKnownTimer != nil) {
            timerType = timerController.lastKnownTimer!.isStopwatch ? .stopwatch : .timer
            
            if (timerType == .timer) {
                selectedHour = timerController.lastKnownTimer!.hour
                selectedMinute = timerController.lastKnownTimer!.minute
                selectedSecond = timerController.lastKnownTimer!.second
            }
            
            timerController.rendering = imagesController.matrixRendering
        }
    }
    
    private func createTimerPayload() -> TimerCodable {
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
        
        return timerPayload
    }
}
