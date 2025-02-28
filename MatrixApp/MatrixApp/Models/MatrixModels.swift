//
//  MatrixStateModels.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/3/25.
//

struct MatrixColor: Identifiable, Decodable, Hashable {
    let id: Int
    let name: String
    let red: Int
    let green: Int
    let blue: Int
    let deleted: Bool
}

struct MatrixFont: Identifiable, Decodable, Hashable {
    let id: Int
    let name: String
    let fileLocation: String
    let width: Int
    let height: Int
}

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

struct MatrixTimer: Decodable {
    let hour: Int
    let minute: Int
    let second: Int
    let state: String
}

struct PlainText: Decodable {
    let text: String
    let parsedText: String?
    let splitByWord: Bool
    let textAlignment: String
    let verticalPositioning: String
    let color: MatrixColor
    let font: MatrixFont
    let background: SavedImage?
    let shouldUpdateSecondly: Bool
}

struct PlainTextPayload: Decodable, Identifiable, Hashable {
    let id: Int
    let text: String
    let textAlignment: String
    let verticalPositioning: String
    let splitByWord: Bool
    let matrixColorId: Int
    let matrixFontId: Int
    var backgroundImageId: Int? = nil
    var backgroundImage: SavedImage? = nil
}

struct PlainTextPayloadEncodable: Codable {
    let text: String
    let textAlignment: String
    let verticalPositioning: String
    let splitByWord: Bool
    let matrixColorId: Int
    let matrixFontId: Int
    var backgroundImageId: Int? = nil
}

struct ScrollingText: Decodable {
    let text: String
    let color: MatrixColor
    let font: MatrixFont
    let background: SavedImage?
    let scrollingDelay: Int
    let iterations: Int
    let parsedText: String
    let verticalPositioning: String
    let currentPosition: Int
    let iterationsLeft: Int
}

struct ScrollingTextPayload: Decodable, Identifiable, Hashable {
    let id: Int
    let text: String
    let verticalPositioning: String
    let scrollingDelay: Int
    let iterations: Int
    let matrixColorId: Int
    let matrixFontId: Int
    var backgroundImageId: Int? = nil
    var backgroundImage: SavedImage? = nil
}

struct ScrollingTextPayloadEncodable: Codable {
    let text: String
    let verticalPositioning: String
    let scrollingDelay: Int
    let iterations: Int
    let matrixColorId: Int
    let matrixFontId: Int
    var backgroundImageId: Int? = nil
}

struct SavedImage: Decodable, Identifiable, Hashable {
    var id: Int
    var name: String
    var fileName: String
    var base64Rendering: String?
    var scaledRendering: String?
}
