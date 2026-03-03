# Loan Management & Game System (WinForms)

This project is a Windows Forms desktop application that combines:
- Loan Management (amortization, extra payments, history, CSV export)
- Game Center (Tic-Tac-Toe and Car Racing)
- Account workflow (register, admin approval, role-based dashboard)

## Architecture

Single WinForms project:
- `LoanSystem.WinForms`

Layers inside the project:
- `Domain/` domain models and loan calculations
- `Data/` SQL Server persistence (`IAppDataStore` + `AppDataStore`)
- `Security/` password hashing
- `Services/` business/application services
- `Forms/` UI forms and navigation flow

## OOP Concepts Applied

- Encapsulation:
  - `Domain/Loan.cs` encapsulates amortization and schedule generation logic.
  - `Domain/Games/TicTacToeGame.cs` and `Domain/Games/CarRacingGame.cs` encapsulate game rules and state transitions.
- Abstraction:
  - `Data/IAppDataStore.cs` abstracts persistence operations.
  - `Domain/Games/IGameSession` abstracts shared game behaviors.
  - `Domain/LoanBase.cs` defines an abstract loan contract for calculation behavior.
- Inheritance:
  - `Loan` inherits `LoanBase`.
  - `TicTacToeGame` and `CarRacingGame` inherit `GameSessionBase`.
- Polymorphism:
  - `GameService.SaveScore(string userId, IGameSession game)` accepts any game implementation and persists results without game-specific branching.
- Separation of Concerns:
  - Forms are UI-only orchestration.
  - Services coordinate use-cases.
  - Domain classes contain business rules.

## Prerequisites

- .NET 10 SDK
- Visual Studio 2022 (Desktop development with .NET) or `dotnet` CLI on Windows

## Run

From repository root:

```powershell
dotnet build LoanSystem.slnx
dotnet run --project LoanSystem.WinForms
```

## Default Admin Credentials

- Email: `admin@loansystem.com`
- Password: `Admin123!`

## Data Storage

By default, data is stored in SQL Server LocalDB:

`Server=(localdb)\MSSQLLocalDB;Database=LoanSystemWinForms;Trusted_Connection=True;TrustServerCertificate=True`

This includes users, game history, and loan history.

To use another SQL Server instance, set environment variable:

`LOANSYSTEM_SQLSERVER_CONNECTION`

## Import Database Script

If you want to import a ready schema manually, run:

`Database/LoanSystemWinForms.sql`

It creates:
- database: `LoanSystemWinForms`
- tables: `Users`, `GameHistories`, `LoanHistories`
- indexes used by dashboard history screens
- default admin account:
  - Email: `admin@loansystem.com`
  - Password: `Admin123!`
