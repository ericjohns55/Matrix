//
//  ImageView.swift
//  MatrixApp
//
//  Created by Eric Johns on 2/21/25.
//

import SwiftUI

struct ImageView: View {
    let textController: TextController
    
    init(textController: TextController) {
        self.textController = textController
    }
    
    var body: some View {
        List {
            ForEach(textController.colors) { color in
                HStack {
                    Text(color.name)
                    Spacer()
                    Text("\(color.id)")
                }
            }
        }
        .listStyle(PlainListStyle())
        .navigationTitle("Colors")
        .padding()
        .task {
            await textController.getColors()
        }
        
        Button(action: {
            Task {
                await textController.getColors()
            }
        }, label: {
            Text("Refresh Colors")
        })
    };
}
