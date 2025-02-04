//
//  ClockFaceViewModel.swift
//  MatrixApp
//
//  Created by Eric Johns on 1/30/25.
//

struct TextLine: Identifiable, Decodable {
    let id: Int
    let text: String
    let xLocation: Int
    let yLocation: Int
    let xPositioning: String
    let yPositioning: String
    let matrixColorId: Int
    let color: MatrixColor
    let matrixFontId: Int
    let font: MatrixFont
    let clockFaceId: Int
}

struct TimePeriod: Identifiable, Decodable {
    let id: Int
    let clockFaceId: Int
    let startHour: Int
    let endHour: Int
    let startMinute: Int
    let endMinute: Int
    let startSecond: Int
    let endSecond: Int
    let daysOfWeek: [String]
}

struct ClockFace: Identifiable, Decodable {
    let id: Int
    let name: String
    let textLines: [TextLine]
    let timePeriods: [TimePeriod]
    let isTimerFace: Bool
    let deleted: Bool
    let updatesEverySecond: Bool
}
