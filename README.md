# Shopping_C_charp Microservices
### **‼️ Приложение развёрнуто на sundayti.ru (подробности далее) ‼️**
## Описание проекта

В этом репозитории реализован **микросервисный набор** на .NET для управления заказами и платежными аккаунтами. Проект использует шаблон **Transactional Outbox & Inbox**, чтобы гарантировать **надёжную доставку событий** через Kafka.

### Основные компоненты

- **OrdersService** — микросервис для создания и получения заказов.
- **PaymentsService** — микросервис для управления платежными аккаунтами и балансом.
- **PostgreSQL** — отдельные БД для заказов и платежей.
- **Kafka** — система обмена сообщениями (create-order-topic).
  - В OrdersService реализован клиент-производитель (OutboxWorker).
  - В PaymentsService реализован клиент-потребитель (InboxWorker).
- **Docker Compose** — оркестратор для всех сервисов в локальной среде.
- **Swagger UI** — автодокументация REST API.

---

## Архитектура

```plaintext
+--------------+             +----------------+             +-----------------+
|  Orders API  | <---------> |   Kafka Topics | <---------> | Payments API    |
|(inbox/outbox)|             |                |             | (inbox/outbox)  |
+--------------+             +----------------+             +-----------------+
      |                                                       |
      v                                                       v
 Postgres (orders)                                  Postgres (payments)
```

- **OutboxWorker** (OrdersService) пишет события в таблицу `outbox_messages` и публикует в Kafka-топик.
- **StupidInboxWorker** (PaymentsService) читает из Kafka, сохраняет их в `inbox_messages` и обновляет баланс.

---

## Технологический стек

| Компонент           | Технология             |
|---------------------|------------------------|
| Язык разработки     | C# .NET 9              |
| Веб-фреймворк       | ASP.NET Core Web API   |
| Интеграция          | MediatR (CQRS)         |
| БД                  | PostgreSQL             |
| Сообщения           | Apache Kafka           |
| Docker              | Docker, Docker Compose |
| Документация API    | Swagger / Swashbuckle  |
| Миграции            | EF Core Migrations     |


---

## Быстрый запуск

1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/sundayti/Shopping_C_charp.git
   cd Shopping_C_charp
   ```

2. Создайте файл `.env` рядом с `docker-compose.yml`:
   ```dotenv
   ConnectionStrings_Payments_Postgres=Host=payments_postgres;Port=5432;Database=payments_db
   ConnectionStrings_Orders_Postgres=Host=orders_postgres;Port=2345;Database=orders_db
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=pg_password
   POSTGRES_PAYMENTS_DB=payments_db
   POSTGRES_ORDERS_DB=orders_db
   ```

3. Поднимите среду через Docker Compose:
   ```bash
   docker-compose up -d --build
   ```

4. Откройте Swagger UI для сервисов:
   - OrdersService: http://localhost:6001/swagger
   - PaymentsService: http://localhost:6000/swagger

---

## Переменные окружения (`.env`)

| Переменная                             | Описание                                    |
|----------------------------------------|---------------------------------------------|
| `ConnectionStrings_Payments_Postgres`  | Строка подключения к БД PaymentsService     |
| `ConnectionStrings_Orders_Postgres`    | Строка подключения к БД OrdersService       |
| `POSTGRES_USER`                        | Пользователь PostgreSQL                     |
| `POSTGRES_PASSWORD`                    | Пароль пользователя PostgreSQL              |
| `POSTGRES_PAYMENTS_DB`                 | Название БД для PaymentsService             |
| `POSTGRES_ORDERS_DB`                   | Название БД для OrdersService               |

---

## Описание сервисов

### OrdersService

- **OutboxWorker**
  - Периодически читает из `outbox_messages` таблицы 
  - Публикует сообщения в Kafka-топик `create-order-topic`
  - Помечает сообщения как `Success`

- **API**
  - `POST /api` — создание заказа (записывает в БД и в outbox)
  - `GET /api/{userId}` — список заказов пользователя
  - `GET /api/status/{orderId}` — статус заказа

### PaymentsService

- **InboxWorker (StupidInboxWorker)**
  - Подписывается на `create-order-topic`
  - Читает сообщения, сохраняет их в `inbox_messages`
  - Обновляет баланс в БД (TopUpBalanceCommand)

- **API**
  - `POST /api/{userId}` — создать счёт для пользователя
  - `POST /api/` — создать счёт и вернуть ID
  - `GET /api/balance?userId={userId}` — получить баланс
  - `POST /api/deposit?userId={userId}&amount={amount}` — пополнить баланс

---

Прямо сейчас приложение развёрнуто на http://sundayti.ru/kpo_3:
  - `POST /orders` — создание заказа (записывает в БД и в outbox)
  - `GET /orders/{userId}` — список заказов пользователя
  - `GET /orders/status/{orderId}` — статус заказа
  - `POST /payments/{userId}` — создать счёт для пользователя
  - `POST /payments/` — создать счёт и вернуть ID
  - `GET /payments/balance?userId={userId}` — получить баланс
  - `POST /payments/deposit?userId={userId}&amount={amount}` — пополнить баланс


## Документация API (Swagger)

Swagger UI автоматически доступен по адресам:

- OrdersService: `http://sundayti.ru:6001/swagger`
- PaymentsService: `http://sundayti.ru:6000/swagger`


---

## Важные моменты

- Реализован **Transactional Outbox & Inbox** для надёжности доставки
- Swagger генерируется автоматически на основе XML-комментариев и `[ProducesResponseType]`

