# WT-6. REST API на ASP.NET Core Minimal API

Лабораторная работа № 6 по дисциплине «Web-технологии».
Специальность: «Управление информационными ресурсами».

**Автор:** Акушевич Андрей Сергеевич, группа УИР-1, 4 курс
## Ссылки

GitHub:
https://github.com/AndreyUIR/wt-lab7-7postgres-efcore-akushevich

Render:
https://wt-lab7-7postgres-efcore-akushevich.onrender.com

## Описание

REST API для управления банковскими счетами клиентов. Приложение
создаёт, читает, изменяет и удаляет счета (CRUD), передаёт данные в
формате JSON и возвращает корректные HTTP-статусы.

Данные хранятся в памяти приложения (статический список `BankAccountStore`),
база данных не используется. После перезапуска приложения данные
возвращаются к начальным.

## Технологии

- C# и ASP.NET Core Minimal API (.NET 9)
- Формат обмена данными: JSON
- Описание API: OpenAPI, Swagger UI
- Контейнеризация: Docker
- Публикация: Render (Web Service)

## Структура проекта

```
wt-lab7-7postgres-efcore-akushevich/
├── Models/BankAccount.cs
├── Data/BankAccountStore.cs
├── Program.cs
├── Dockerfile
└── README.md
```

## Модель данных

| Поле | Тип | Описание | Ограничения |
|---|---|---|---|
| `id` | число | идентификатор, задаётся сервером | — |
| `ownerName` | строка | владелец счёта | обязательное, до 100 символов |
| `accountNumber` | строка | номер счёта | обязательное, от 6 до 34 символов |
| `balance` | число | баланс | от 0 до 1 000 000 000 |
| `isActive` | логическое | счёт активен | — |

## Эндпоинты

| Метод | URL | Назначение | Успешный статус |
|---|---|---|---|
| GET | `/` | проверить работу приложения | 200 |
| GET | `/api/info` | информация о приложении | 200 |
| GET | `/api/accounts` | получить список счетов | 200 |
| GET | `/api/accounts?search=...` | поиск по владельцу или номеру | 200 |
| GET | `/api/accounts/{id}` | получить счёт | 200 / 404 |
| POST | `/api/accounts` | создать счёт | 201 / 400 |
| PUT | `/api/accounts/{id}` | изменить счёт | 200 / 400 / 404 |
| DELETE | `/api/accounts/{id}` | удалить счёт | 204 / 404 |

## HTTP-статусы

| Статус | Когда возвращается |
|---|---|
| 200 OK | успешное получение или изменение |
| 201 Created | счёт создан, в заголовке `Location` адрес нового счёта |
| 204 No Content | счёт удалён, тела ответа нет |
| 400 Bad Request | некорректные данные или сломанный JSON |
| 404 Not Found | счёт с таким `id` не найден |

## Примеры

### Получение списка

Запрос: `GET /api/accounts`

Ответ `200 OK`:

```json
[
  {
    "id": 1,
    "ownerName": "Акушевич Андрей Сергеевич",
    "accountNumber": "BY01SAV0001",
    "balance": 2000,
    "isActive": true
  }
]
```

### Поиск

Запрос: `GET /api/accounts?search=петр` возвращает счета, в имени
владельца или номере которых есть «петр» (регистр не важен).

### Создание счёта

Запрос: `POST /api/accounts`

```json
{
  "ownerName": "Новиков Сергей Олегович",
  "accountNumber": "BY06SAV0006",
  "balance": 1500.50,
  "isActive": true
}
```

Ответ `201 Created`, заголовок `Location: /api/accounts/6`:

```json
{
  "id": 6,
  "ownerName": "Новиков Сергей Олегович",
  "accountNumber": "BY06SAV0006",
  "balance": 1500.50,
  "isActive": true
}
```

### Изменение счёта

Запрос: `PUT /api/accounts/6` с телом в том же формате. Ответ `200 OK`
содержит изменённый счёт. Поле `id` изменить нельзя.

### Удаление счёта

Запрос: `DELETE /api/accounts/6`. Ответ `204 No Content`, тела нет.

### Некорректные данные

Запрос: `POST /api/accounts` с пустым `ownerName`.

Ответ `400 Bad Request`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "OwnerName": ["Укажите владельца счёта"]
  }
}
```

### Счёт не найден

`GET /api/accounts/999` возвращает `404 Not Found`.

## Запуск

```bash
dotnet run
```

Порт указан в консоли в строке `Now listening on: ...`.
## Запуск в Docker

Сборка образа (точка в конце обязательна):

```bash
docker build -t wt-lab7-7postgres-efcore-akushevich .
```

Запуск контейнера:

```bash
docker run --rm -p 10000:10000 wt-lab7-7postgres-efcore-akushevich
```

После запуска API доступен по адресу `http://localhost:10000`.

Приложение слушает адрес `0.0.0.0` и порт из переменной окружения `PORT`
(по умолчанию на Render это 10000).

## Документация OpenAPI

В режиме разработки (локальный запуск) доступны:

- `/openapi/v1.json` — описание API в формате OpenAPI;
- `/swagger` — Swagger UI, в нём можно выполнять запросы к API.

На Render документация отключена (среда `Production`).