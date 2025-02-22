//
//  UIHelpers.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/22/25.
//

import SwiftUI
import Combine

extension UIApplication {
    func finishEditing() {
        if let windowScene = connectedScenes.first as? UIWindowScene {
            windowScene.windows.first?.endEditing(true)
        }
    }
}

class KeyboardResponder: ObservableObject {
    @Published var keyboardHeight: CGFloat = 0
    @Published var keyboardActive: Bool = false
    
    private var cancellables = Set<AnyCancellable>()
    
    init() {
        NotificationCenter.default.publisher(for: UIResponder.keyboardWillShowNotification)
            .sink { notification in
                self.keyboardActive = true
                
                if let userInfo = notification.userInfo,
                   let frame = userInfo[UIResponder.keyboardFrameEndUserInfoKey] as? CGRect {
                    self.keyboardHeight = frame.height
                }
            }.store(in: &cancellables)
        
        NotificationCenter.default.publisher(for: UIResponder.keyboardWillHideNotification)
            .sink { _ in
                self.keyboardActive = false
                self.keyboardHeight = 0
            }.store(in: &cancellables)
    }
}
