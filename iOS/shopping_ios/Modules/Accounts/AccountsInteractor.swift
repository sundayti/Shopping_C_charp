//
//  AccountsInteractor.swift
//  shopping_ios
//
//  Created by Tom Tim on 17.06.2025.
//

import Foundation

final class AccountsInteractor: AccountsInteractable {
    private let network = NetworkService.shared
    weak var presenter: AccountsPresentable?

    func fetchAccounts() {
        let ids = AccountManager.shared.accounts
        presenter?.didFetchAccounts(.success(ids))
        // если в памяти ещё нет ни одного – создаём первый
        guard !ids.isEmpty else {
            createAccount()
            return
        }
    }

    func createAccount() {
        network.request(
            endpoint: "payments/",
            method: .post,
            queryItems: nil,
            body: nil
        ) { [weak self] (result: Result<AccountResponse, NetworkError>) in
            guard let self = self else { return }
            switch result {
            case .success(let resp):
                let newId = resp.accountId
                // сохраняем в UserDefaults
                AccountManager.shared.add(newId)
                self.presenter?.didCreateAccount(.success(newId))
            case .failure(let error):
                self.presenter?.didCreateAccount(.failure(error))
            }
        }
    }
}
