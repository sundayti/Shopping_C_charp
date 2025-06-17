//
//  BalanceView.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import SwiftUI
#if canImport(UIKit)
import UIKit
#endif

struct BalanceView: View {
  @EnvironmentObject private var accountVM: AccountsPresenter
  @StateObject private var presenter = BalancePresenter()
  @State private var topUpAmount = ""

  private var interactor: BalanceInteractor {
    let i = BalanceInteractor()
    i.presenter = presenter
    return i
  }

  var body: some View {
    NavigationView {
      VStack(spacing: 20) {
        if let error = presenter.errorMessage {
          Text(error).foregroundColor(.red)
        }

        Text("Balance: \(presenter.balanceText)")
          .font(.largeTitle)

        HStack {
          #if canImport(UIKit)
          TextField("Amount", text: $topUpAmount)
            .keyboardType(.decimalPad)
            .textFieldStyle(RoundedBorderTextFieldStyle())
          #else
          TextField("Amount", text: $topUpAmount)
            .textFieldStyle(RoundedBorderTextFieldStyle())
          #endif

          Button("Top Up") {
            guard
              let id = accountVM.selectedAccount,
              let amt = Decimal(string: topUpAmount)
            else { return }
            interactor.topUp(userId: id, amount: amt)
          }
          .padding(.horizontal)
        }
        .padding()

        Spacer()
      }
      .padding()
      .navigationTitle("Balance")
      .task(id: accountVM.selectedAccount) {
          fetchBalance()
      }
    }
  }

  private func fetchBalance() {
    guard let id = accountVM.selectedAccount else { return }
    interactor.fetchBalance(for: id)
  }
}
