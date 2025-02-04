//
//  MatrixStateModels.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/3/25.
//

struct MatrixTimer: Decodable {
    let hour: Int
    let minute: Int
    let second: Int
    let state: String
}

struct PlainText: Decodable {
    let parsedText: String?
    let splitByWord: Bool
    let textAlignment: String
    let color: MatrixColor
    let font: MatrixFont
    let shouldUpdateSecondly: Bool
}

struct ScrollingText: Decodable {
    let scrollingDelay: Int
}
