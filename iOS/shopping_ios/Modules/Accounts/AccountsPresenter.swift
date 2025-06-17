//
//  AccountsPresenter.swift
//  shopping_ios
//
//  Created by Tom Tim on 17.06.2025.
//

import Foundation
import Combine

final class AccountsPresenter: ObservableObject, AccountsPresentable {
    @Published var accounts: [UUID] = []
    @Published var selectedAccount: UUID?
    @Published var errorMessage: String?

    func didFetchAccounts(_ result: Result<[UUID], NetworkError>) {
        switch result {
        case .success(let ids):
            accounts = ids
            if selectedAccount == nil, let first = ids.first {
                selectedAccount = first
            }
        case .failure(let error):
            errorMessage = error.localizedDescription
        }
    }

    func didCreateAccount(_ result: Result<UUID, NetworkError>) {
        switch result {
        case .success(let id):
            accounts.append(id)
            selectedAccount = id
        case .failure(let error):
            errorMessage = error.localizedDescription
        }
    }
}
