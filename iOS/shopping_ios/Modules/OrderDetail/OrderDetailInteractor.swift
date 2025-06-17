//
//  OrderDetailInteractor.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import Foundation
import Starscream

protocol OrderDetailInteractable {
    func subscribeStatusUpdates(orderId: UUID)
    func cancelUpdates()
}

final class OrderDetailInteractor: OrderDetailInteractable, WebSocketDelegate {
    private let network = NetworkService.shared
    private var socket: WebSocket?
    var presenter: OrderDetailPresentable?
    private let webSocketBase = "ws://sundayti.ru:6001/api/ws/"
    

    func subscribeStatusUpdates(orderId: UUID) {
        let urlString = webSocketBase + orderId.uuidString
        guard let url = URL(string: urlString) else {
            DispatchQueue.main.async {
                self.presenter?.didReceiveUpdate(.failure(.invalidURL))
            }
            return
        }

        print("WS connecting to:", urlString)
        
        var request = URLRequest(url: url)
        request.timeoutInterval = 10
        
        // Устанавливаем необходимые заголовки
        request.setValue("websocket", forHTTPHeaderField: "Upgrade")
        request.setValue("Upgrade", forHTTPHeaderField: "Connection")
        request.setValue("13", forHTTPHeaderField: "Sec-WebSocket-Version")
        
        // Создаем сокет
        socket = WebSocket(request: request)
        socket?.delegate = self
        socket?.connect()
    }

    // MARK: - WebSocketDelegate
    
    func didReceive(event: WebSocketEvent, client: any WebSocketClient) {
        switch event {
        case .connected(let headers):
            print("WebSocket connected: \(headers)")
            
        case .text(let text):
            print("Received text: \(text)")
            handleText(text)
            
        case .binary(let data):
            if let text = String(data: data, encoding: .utf8) {
                print("Received binary: \(text)")
                handleText(text)
            }
            
        case .disconnected(let reason, let code):
            print("WebSocket disconnected: \(reason) (\(code))")
            
        case .error(let error):
            if let error = error {
                print("WebSocket error: \(error.localizedDescription)")
                handleError(error)
            } else {
                print("WebSocket error: unknown")
                handleError(NSError(domain: "WebSocket", code: 0, userInfo: nil))
            }
            
        case .cancelled:
            print("WebSocket cancelled")
            
        case .ping, .pong, .viabilityChanged, .reconnectSuggested, .peerClosed:
            break
                        
        @unknown default:
            break
        }
    }

    private func handleText(_ text: String) {
        print("Handling text: \(text)")
        guard
            let data = text.data(using: .utf8),
            let resp = try? JSONDecoder().decode(OrderStatusResponse.self, from: data)
        else {
            print("Failed to decode message: \(text)")
            return
        }

        DispatchQueue.main.async {
            self.presenter?.didReceiveUpdate(.success(resp.status))
        }

        if resp.status == .Finished || resp.status == .Cancelled {
            socket?.disconnect()
        }
    }

    private func handleError(_ error: Error) {
        print("WebSocket error: \(error)")
        DispatchQueue.main.async {
            self.presenter?.didReceiveUpdate(.failure(.unknown(message: error.localizedDescription)))
        }
    }

    func cancelUpdates() {
        socket?.disconnect()
        socket = nil
    }
}

struct OrderStatusResponse: Decodable {
    let status: OrderStatus
}
