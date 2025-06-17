//
//  OrdersListPresenter.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation
import Combine

final class OrdersListPresenter: ObservableObject, OrdersListPresentable {
    @Published var orders: [Order] = []
    @Published var errorMessage: String?

    func didFetchOrders(_ result: Result<[Order], NetworkError>) {
        switch result {
        case .success(let list):
            orders = list
            errorMessage = nil
        case .failure(let error):
            orders = []
            errorMessage = error.localizedDescription
        }
    }
}
