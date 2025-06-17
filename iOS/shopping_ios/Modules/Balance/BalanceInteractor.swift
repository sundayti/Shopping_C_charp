//
//  BalanceInteractor.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

final class BalanceInteractor: BalanceInteractable {
    private let network = NetworkService.shared
    weak var presenter: BalancePresentable?

    func fetchBalance(for userId: UUID) {
        let queryItems = [URLQueryItem(name: "userId", value: userId.uuidString)]
        network.request(
            endpoint: "payments/balance",
            method: .get,
            queryItems: queryItems,
            body: nil
        ) { (result: Result<BalanceResponse, NetworkError>) in
            switch result {
            case .success(let resp):
                self.presenter?.didFetchBalance(.success(resp.balance))
            case .failure(let error):
                self.presenter?.didFetchBalance(.failure(error))
            }
        }
    }

    func topUp(userId: UUID, amount: Decimal) {
        let queryItems = [
            URLQueryItem(name: "userId", value: userId.uuidString),
            URLQueryItem(name: "amount", value: "\(amount)")
        ]
        network.request(
            endpoint: "payments/deposit",
            method: .post,
            queryItems: queryItems,
            body: nil
        ) { (result: Result<BalanceResponse, NetworkError>) in
            switch result {
            case .success(let resp):
                self.presenter?.didTopUp(.success(resp.balance))
            case .failure(let error):
                self.presenter?.didTopUp(.failure(error))
            }
        }
    }
}
