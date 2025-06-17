//
//  CreateOrderView.swift
//  shopping_ios
//
//  Created by Tom Tim on 17.06.2025.
//

// CreateOrderView.swift

import SwiftUI
#if canImport(UIKit)
import UIKit
#endif

struct CreateOrderView: View {
  @EnvironmentObject private var accountVM: AccountsPresenter
  @Environment(\.dismiss) private var dismiss

  @State private var description = ""
  @State private var amountText = ""
  @State private var errorMessage: String?
  @State private var isSubmitting = false

  private let network = NetworkService.shared

  var body: some View {
    Form {
      Section("Order Details") {
        TextField("Description", text: $description)
        #if canImport(UIKit)
        TextField("Amount", text: $amountText)
          .keyboardType(.decimalPad)
        #else
        TextField("Amount", text: $amountText)
        #endif
      }

      if let error = errorMessage {
        Section { Text(error).foregroundColor(.red) }
      }

      Section {
        Button {
          submit()
        } label: {
          HStack {
            Spacer()
            if isSubmitting { ProgressView() }
            else { Text("Submit Order") }
            Spacer()
          }
        }
        .disabled(isSubmitting || description.isEmpty || Decimal(string: amountText) == nil)
      }
    }
    .navigationTitle("New Order")
  }

  private func submit() {
    guard
      let userId = accountVM.selectedAccount,
      let amount = Decimal(string: amountText)
    else {
      errorMessage = "Invalid input"
      return
    }

    isSubmitting = true
    let body: [String: Any] = [
      "userId": userId.uuidString,
      "amount": amount,
      "description": description
    ]

    network.request(
      endpoint: "orders/",
      method: .post,
      queryItems: nil,
      body: body
    ) { (result: Result<CreateOrderResponse, NetworkError>) in
      DispatchQueue.main.async {
        isSubmitting = false
        switch result {
        case .success:
          // уведомляем всех об обновлении списка
          NotificationCenter.default.post(name: .orderCreated, object: nil)
          dismiss()
        case .failure(let err):
          errorMessage = err.localizedDescription
        }
      }
    }
  }
}

struct CreateOrderResponse: Decodable {
  let orderId: UUID
}
