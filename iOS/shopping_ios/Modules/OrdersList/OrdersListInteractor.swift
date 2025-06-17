//
//  OrdersListInteractor.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

final class OrdersListInteractor: OrdersListInteractable {
    private let network = NetworkService.shared
    weak var presenter: OrdersListPresentable?

    func fetchOrders(for userId: UUID) {
        let endpoint = "orders/\(userId.uuidString)"
        network.request(
            endpoint: endpoint,
            method: .get,
            queryItems: nil,
            body: nil
        ) { [weak self] (result: Result<[Order], NetworkError>) in
            self?.presenter?.didFetchOrders(result)
        }
    }
}
