//
//  BalancePresenter.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation
import Combine

final class BalancePresenter: ObservableObject, BalancePresentable {
    @Published var balanceText: String = ""
    @Published var errorMessage: String?

    func didFetchBalance(_ result: Result<Decimal, NetworkError>) {
        switch result {
        case .success(let balance):
            balanceText = String(describing: balance)
            errorMessage = nil
        case .failure(let error):
            balanceText = ""
            errorMessage = error.localizedDescription
        }
    }

    func didTopUp(_ result: Result<Decimal, NetworkError>) {
        switch result {
        case .success(let balance):
            balanceText = String(describing: balance)
            errorMessage = nil
        case .failure(let error):
            errorMessage = error.localizedDescription
        }
    }
}
