//
//  OrderDetailPresenter.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation
import Combine

protocol OrderDetailPresentable: AnyObject {
    func didFetchStatus(_ result: Result<OrderStatus, NetworkError>)
    func didReceiveUpdate(_ result: Result<OrderStatus, NetworkError>)
}


final class OrderDetailPresenter: ObservableObject, OrderDetailPresentable {
    @Published var status: OrderStatus = .New
    @Published var errorMessage: String?

    func didFetchStatus(_ result: Result<OrderStatus, NetworkError>) {
        switch result {
        case .success(let st):
            status = st
            errorMessage = nil
        case .failure(let error):
            errorMessage = error.localizedDescription
        }
    }

    func didReceiveUpdate(_ result: Result<OrderStatus, NetworkError>) {
        switch result {
        case .success(let st):
            status = st
        case .failure(let error):
            errorMessage = error.localizedDescription
        }
    }
}
