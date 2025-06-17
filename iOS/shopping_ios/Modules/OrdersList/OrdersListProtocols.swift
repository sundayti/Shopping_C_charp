//
//  Protocols.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

protocol OrdersListPresentable: AnyObject {
    func didFetchOrders(_ result: Result<[Order], NetworkError>)
}

protocol OrdersListInteractable {
    func fetchOrders(for userId: UUID)
}
