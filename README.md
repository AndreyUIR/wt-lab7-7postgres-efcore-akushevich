# WT-7. PostgreSQL и Entity Framework Core

Лабораторная работа № 7 по дисциплине «Web-технологии».
Специальность: «Управление информационными ресурсами».

**Автор:** Акушевич Андрей Сергеевич, группа УИР-1

## Описание

REST API для управления банковскими счетами. Данные хранятся в
PostgreSQL, доступ к базе выполняется через Entity Framework Core
(подход Code First). Структура базы создаётся и обновляется миграциями,
которые применяются автоматически при запуске приложения.

## Технологии

- C# и ASP.NET Core Minimal API (.NET 9)
- PostgreSQL и Entity Framework Core (провайдер Npgsql)
- Миграции EF Core (Code First)
- OpenAPI, Swagger UI (только в режиме разработки)
- Docker, публикация на Render

## Структура проекта

```
wt-lab7-7postgres-efcore-akushevich/
├── Data/AppDbContext.cs
├── Models/BankAccount.cs
├── Migrations/
├── Program.cs
├── appsettings.json
├── Dockerfile
└── README.md
```

## Модель данных

| Поле | Тип | Описание |
|---|---|---|
| `id` | число | идентификатор, выдаётся базой |
| `ownerName` | строка | владелец счёта (обязательное, до 100 символов) |
| `accountNumber` | строка | номер счёта (от 6 до 34 символов) |
| `balance` | число | баланс (от 0 до 1 000 000 000) |
| `isActive` | логическое | счёт активен |
| `createdAt` | дата и время (UTC) | выставляется сервером при создании |

## Эндпоинты

| Метод | URL | Назначение | Статус |
|---|---|---|---|
| GET | `/` | проверка приложения | 200 |
| GET | `/api/info` | информация о приложении | 200 |
| GET | `/api/accounts` | список счетов | 200 |
| GET | `/api/accounts/{id}` | получить счёт | 200 / 404 |
| GET | `/api/accounts/search?search=...` | поиск по владельцу или номеру | 200 |
| GET | `/api/accounts/rich?min=...` | баланс больше порога | 200 |
| GET | `/api/accounts/sorted` | сортировка по балансу | 200 |
| GET | `/api/accounts/active` | активные счета | 200 |
| GET | `/api/accounts/stats` | статистика по счетам | 200 |
| POST | `/api/accounts` | создать счёт | 201 / 400 |
| PUT | `/api/accounts/{id}` | изменить счёт | 200 / 400 / 404 |
| DELETE | `/api/accounts/{id}` | удалить счёт | 204 / 404 |

## Локальный запуск

1. Установить PostgreSQL и создать базу `wt_lab7`.
2. Скопировать `appsettings.Development.example.json` в
   `appsettings.Development.json` и указать свой пароль.
3. Запустить приложение. Миграции применятся автоматически:

```bash
dotnet run
```

## Миграции

Создать миграцию после изменения модели:

```bash
dotnet ef migrations add ИмяМиграции
```

Применить миграции вручную:

```bash
dotnet ef database update
```

Список миграций и применённые миграции в базе:

```bash
dotnet ef migrations list
```

```sql
SELECT * FROM "__EFMigrationsHistory";
```

## Переменные окружения

| Переменная | Назначение |
|---|---|
| `DATABASE_URL` | адрес PostgreSQL в формате `postgresql://user:password@host/database` (на Render) |
| `PORT` | порт HTTP-сервера, задаётся платформой |

Пароли и строки подключения в репозитории не хранятся.

## Запуск в Docker

```bash
docker build -t wt-lab7-7postgres-efcore-akushevich .
docker run --rm -p 10000:10000 -e DATABASE_URL="postgresql://user:password@host:5432/database" wt-lab7-7postgres-efcore-akushevich
```

Подставьте свой адрес базы вместо `host`.