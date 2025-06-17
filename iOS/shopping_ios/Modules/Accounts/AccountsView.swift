//
//  AccountsView.swift
//  shopping_ios
//
//  Created by Tom Tim on 17.06.2025.
//

import SwiftUI

struct AccountsView: View {
    @EnvironmentObject private var accountVM: AccountsPresenter
    private let interactor = AccountsInteractor()

    var body: some View {
        NavigationView {
            VStack(spacing: 16) {
                if let error = accountVM.errorMessage {
                    Text(error)
                        .foregroundColor(.red)
                }

                if !accountVM.accounts.isEmpty {
                    Picker("Account", selection: $accountVM.selectedAccount) {
                        ForEach(accountVM.accounts, id: \.self) { id in
                            Text(id.uuidString.prefix(8) + "...")
                                .tag(Optional(id))
                        }
                    }
                    .pickerStyle(MenuPickerStyle())
                } else {
                    Text("No accounts. Create one.")
                        .foregroundColor(.gray)
                }

                Button("Create New Account") {
                    interactor.presenter = accountVM
                    interactor.createAccount()
                }
                .padding(.horizontal, 24)
                .padding(.vertical, 12)
                .background(Color.blue)
                .foregroundColor(.white)
                .cornerRadius(8)

                Button("Удалить все кроме текущего") {
                    guard let current = accountVM.selectedAccount else { return }
                    AccountManager.shared.clearExcept(current)
                    interactor.presenter = accountVM
                    interactor.fetchAccounts() // << ОБНОВЛЯЕМ!
                }
                .padding(.top, 16)
                .foregroundColor(.red)

                Spacer()
            }
            .padding()
            .navigationTitle("Accounts")
            .onAppear {
                interactor.presenter = accountVM
                interactor.fetchAccounts()
            }
        }
    }
}
