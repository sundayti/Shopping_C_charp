//
//  StatusLineView.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import SwiftUICore

struct StatusLineView: View {
    let status: OrderStatus

    var body: some View {
        HStack {
            statusCircle(.New)
            lineSegment
            statusCircle(.Finished)
            lineSegment
            statusCircle(.Cancelled)
        }
    }

    private func statusCircle(_ step: OrderStatus) -> some View {
        Circle()
            .fill(color(for: step))
            .frame(width: 20, height: 20)
    }

    private var lineSegment: some View {
        Rectangle()
            .fill(Color.gray.opacity(0.5))
            .frame(height: 2)
    }

    private func color(for step: OrderStatus) -> Color {
        switch (step == status) {
        case true:
            switch status {
            case .New: return .orange
            case .Finished: return .green
            case .Cancelled: return .red
            }
        default:
            return .gray.opacity(0.3)
        }
    }
}
