//
//  Order.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

struct Order: Decodable, Identifiable {
    let id: UUID
    let userId: UUID
    let amount: Decimal
    let description: String
    let status: OrderStatus
}
