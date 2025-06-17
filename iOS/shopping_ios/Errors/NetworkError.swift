//
//  NetworkError.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation

enum NetworkError: LocalizedError {
    case invalidURL
    case noData
    case decodingError
    case serverError(code: Int, message: String)
    case unknown(message: String)

    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid URL"
        case .noData:
            return "No data received from server"
        case .decodingError:
            return "Failed to decode response"
        case let .serverError(code, message):
            return "Server error (\(code)): \(message)"
        case let .unknown(message):
            return "Unknown error: \(message)"
        }
    }
}
