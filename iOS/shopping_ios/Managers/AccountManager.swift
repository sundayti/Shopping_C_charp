//
//  AccountManager.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

final class AccountManager {
    static let shared = AccountManager()
    private let defaults = UserDefaults.standard
    private let prefix = "User_"
    private let countKey = "count"

    var accounts: [UUID] {
        let count = defaults.integer(forKey: countKey)
        guard count > 0 else { return [] }
        return (1...count).compactMap { idx in
            let key = "\(prefix)\(idx)"
            return defaults.string(forKey: key).flatMap(UUID.init)
        }
    }

    func add(_ id: UUID) {
        var count = defaults.integer(forKey: countKey)
        count += 1
        defaults.set(id.uuidString, forKey: "\(prefix)\(count)")
        defaults.set(count, forKey: countKey)
    }

    /// Текущий выбранный аккаунт
    func current() -> UUID? {
        return accounts.first
    }
}

extension AccountManager {
    /// Оставляет в памяти только один переданный id
    func clearExcept(_ keep: UUID) {
        let dict = defaults.dictionaryRepresentation()
        // удаляем все старые User_*
        for key in dict.keys where key.hasPrefix(prefix) || key == countKey {
            defaults.removeObject(forKey: key)
        }
        // сохраняем только keep как единственный
        defaults.set(keep.uuidString, forKey: "\(prefix)1")
        defaults.set(1, forKey: countKey)
    }
}
