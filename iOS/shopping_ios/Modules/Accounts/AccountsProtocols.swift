//
//  AccountsProtocols.swift
//  shopping_ios
//
//  Created by Tom Tim on 17.06.2025.
//

import Foundation

protocol AccountsPresentable: AnyObject {
    func didFetchAccounts(_ result: Result<[UUID], NetworkError>)
    func didCreateAccount(_ result: Result<UUID, NetworkError>)
}

protocol AccountsInteractable {
    func fetchAccounts()
    func createAccount()
}
