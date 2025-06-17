//
//  OrdersListView.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import SwiftUI

struct OrdersListView: View {
    @EnvironmentObject private var accountVM: AccountsPresenter
    @StateObject private var presenter = OrdersListPresenter()
    private let interactor = OrdersListInteractor()
    
    var body: some View {
        NavigationView {
            Group {
                if let error = presenter.errorMessage {
                    Text(error).foregroundColor(.red)
                } else if presenter.orders.isEmpty {
                    Text("No orders yet")
                        .foregroundColor(.secondary)
                } else {
                    List(presenter.orders) { order in
                        NavigationLink(destination: OrderDetailView(order: order)) {
                            VStack(alignment: .leading, spacing: 4) {
                                Text(order.description).font(.headline)
                                Text("Amount: \(order.amount)").font(.subheadline)
                                Text(order.status.rawValue)
                                    .font(.caption)
                                    .foregroundColor(color(for: order.status))
                            }
                        }
                    }
                }
            }
            .navigationTitle("Orders")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    NavigationLink {
                        CreateOrderView()
                            .environmentObject(accountVM)
                    } label: {
                        Image(systemName: "plus")
                    }
                }
            }
            .task(id: accountVM.selectedAccount) {
                fetchOrders()
            }
            .onReceive(NotificationCenter.default.publisher(for: .orderCreated)) { _ in
                fetchOrders()
            }
        }
    }
    
    private func fetchOrders() {
        guard let id = accountVM.selectedAccount else { return }
        interactor.presenter = presenter
        interactor.fetchOrders(for: id)
    }
    
    private func color(for status: OrderStatus) -> Color {
        switch status {
        case .New:      return .orange
        case .Finished: return .green
        case .Cancelled:return .red
        }
    }
}
