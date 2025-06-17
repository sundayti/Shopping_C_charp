//
//  BalanceProtocols.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

protocol BalancePresentable: AnyObject {
    func didFetchBalance(_ result: Result<Decimal, NetworkError>)
    func didTopUp(_ result: Result<Decimal, NetworkError>)
}

protocol BalanceInteractable {
    func fetchBalance(for userId: UUID)
    func topUp(userId: UUID, amount: Decimal)
}
