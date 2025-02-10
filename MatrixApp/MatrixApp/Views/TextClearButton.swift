//
//  TextClearButton.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/6/25.
//

import SwiftUI

struct TextClearButton: ViewModifier {
    @Binding var text: String
    
    init(text: Binding<String>) {
        self._text = text
    }
    
    public func body(content: Content) -> some View {
        ZStack(alignment: .trailing, content: {
            content
            
            if (!text.isEmpty) {
                Button(action: {
                    self.text = ""
                }) {
                    Image(systemName: "delete.left")
                        .foregroundStyle(Color(UIColor.opaqueSeparator))
                        .padding(.trailing, 8)
                        .padding(.vertical, 8)
                        .contentShape(Rectangle())
                }
            }
        })
    }
}
