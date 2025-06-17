//
//  OrderDetailView.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import SwiftUI

struct OrderDetailView: View {
    @StateObject private var presenter: OrderDetailPresenter
    private let interactor: OrderDetailInteractable
    private let order: Order

    init(order: Order) {
        self.order = order
        let presenter = OrderDetailPresenter()
        let interactor = OrderDetailInteractor()
        interactor.presenter = presenter
        _presenter = StateObject(wrappedValue: presenter)
        self.interactor = interactor
    }

    var body: some View {
        VStack(spacing: 24) {
            Text("Order ID: \(order.id.uuidString)")
                .font(.footnote)
                .foregroundColor(.gray)

            StatusLineView(status: presenter.status)
                .frame(height: 50)

            if let error = presenter.errorMessage {
                Text(error)
                    .foregroundColor(.red)
            }

            Spacer()
        }
        .padding()
        .navigationTitle("Order Details")
        .onAppear {
            interactor.subscribeStatusUpdates(orderId: order.id)
        }
        .onDisappear {
            interactor.cancelUpdates()
        }
    }
}
