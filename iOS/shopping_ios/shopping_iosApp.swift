//
//  shopping_iosApp.swift
//  shopping_ios
//
//  Created by Vladislav Pankratov on 17.06.2025.
//

import SwiftUI

@main
struct Shopping_iOSApp: App {
    // 1) Поднимаем один экземпляр AccountsPresenter на весь App
    @StateObject private var accountVM = AccountsPresenter()
    private let interactor = AccountsInteractor()

    var body: some Scene {
        WindowGroup {
            Group {
                // 2) Если аккаунт уже есть — сразу показываем табы
                if accountVM.selectedAccount != nil {
                    MainTabView()
                }
                // 3) Иначе — индикатор и инициализация
                else {
                    VStack {
                        ProgressView("Initializing…")
                    }
                    .onAppear {
                        interactor.presenter = accountVM
                        interactor.fetchAccounts()
                    }
                }
            }
            // 4) Прокидываем accountVM в иерархию
            .environmentObject(accountVM)
        }
    }
}

struct MainTabView: View {
    // 5) Забираем его через EnvironmentObject
    @EnvironmentObject private var accountVM: AccountsPresenter

    var body: some View {
        TabView {
            OrdersListView()
                .tabItem { Label("Orders",   systemImage: "list.bullet") }
            BalanceView()
                .tabItem { Label("Balance",  systemImage: "creditcard") }
            AccountsView()
                .tabItem { Label("Accounts", systemImage: "person.circle") }
        }
    }
}
