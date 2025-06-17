//
//  UserDefaults+uuid.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

extension UserDefaults {
    func uuid(forKey defaultName: String) -> UUID? {
        string(forKey: defaultName).flatMap(UUID.init)
    }
}
