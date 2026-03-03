# Project Documentation: Loan Management & Game System (WinForms)

## 1. Project Overview

This project is a **C# Windows Forms desktop application** focused on two modules:
- **Loan Management**: amortization, extra payments, loan history, CSV export.
- **Game Center**: Tic-Tac-Toe and Car Racing with score tracking.

The project uses a clean layered structure (`Domain`, `Data`, `Services`, `Forms`) to highlight OOP and maintainability.

---

## 2. C# Architecture Used in This Project

### A. Project Structure
- `Domain/`: business entities and game/loan logic.
- `Data/`: persistence contracts and implementation (`IAppDataStore`, `AppDataStore`).
- `Services/`: application use-cases (`AuthService`, `LoanService`, `AdminService`, etc.).
- `Forms/`: UI screens (Login, Register, Dashboard, Loan, Games).
- `Security/`: reusable password hashing utility.

### B. Startup and Dependency Composition
The app starts in `Program.cs`, creates `AppServices`, and opens `LoginForm`.

```csharp
ApplicationConfiguration.Initialize();
var services = new AppServices();
Application.Run(new LoginForm(services));
```

`AppServices` acts as a composition root where all services are created once and shared.

---

## 3. C# Features Applied

### A. Enums
Used to model fixed domain states:
- `UserRole` (`User`, `Admin`)
- `AccountStatus` (`Pending`, `Active`, `Rejected`)
- `TicTacToeOutcome`

### B. Interface-based design
Persistence is abstracted using an interface and consumed by services:

```csharp
public interface IAppDataStore
{
    AppUser? GetUserByEmail(string email);
    void AddUser(AppUser user);
    void UpdateUser(AppUser user);
    // ...
}
```

### C. Tuples for use-case results
Used in authentication and game outcome methods for clean return values.

```csharp
public (AppUser? User, string Message) Login(string email, string password)
```

### D. Static utility class
`PasswordHashing` is implemented as a static helper for PBKDF2 hashing and verification.

---

## 4. OOP Concepts Implemented (with Code Examples)

### A. Encapsulation
Business rules are encapsulated inside domain classes, not UI forms.

Example from `Domain/Loan.cs`:

```csharp
public decimal CalculateMonthlyPayment()
{
    var monthlyRate = (InterestRate / 100d) / 12d;
    var totalMonths = (TermYears * 12) + TermMonths;
    if (totalMonths <= 0) return 0;
    // formula hidden inside class
    var payment = (double)LoanAmount *
                  (monthlyRate * Math.Pow(1 + monthlyRate, totalMonths)) /
                  (Math.Pow(1 + monthlyRate, totalMonths) - 1);
    return Math.Round((decimal)payment, 2);
}
```

Another encapsulation example from `Domain/AppUser.cs`:

```csharp
public bool CanLogin() => Status == AccountStatus.Active;
public void Approve() { if (!IsAdmin) Status = AccountStatus.Active; }
public void Reject()  { if (!IsAdmin) Status = AccountStatus.Rejected; }
```

### B. Abstraction
The app abstracts storage operations through `IAppDataStore`, so services do not depend on SQL Server details.

```csharp
public class AuthService
{
    private readonly IAppDataStore _store;
    public AuthService(IAppDataStore store) => _store = store;
}
```

### C. Polymorphism
Polymorphism is applied in two places:
- `AppDataStore : IAppDataStore` for storage abstraction.
- `GameService.SaveScore(string userId, IGameSession game)` consumes both `TicTacToeGame` and `CarRacingGame` through the same interface and persists outcomes without game-specific branching.

This enables replacing `AppDataStore` with another implementation (e.g., SQL Server) without changing service logic.

### D. Inheritance
Game domain inheritance:
- `Loan : LoanBase`
- `TicTacToeGame : GameSessionBase`
- `CarRacingGame : GameSessionBase`
- Shared members (`TotalMonths`, `MonthlyInterestRate`, `GameName`, `Score`, `IsRunning`, `BuildOutcome`) are defined in base classes and specialized per concrete class.

UI forms also reuse framework behavior through inheritance:

```csharp
public class LoginForm : Form
{
    // form behavior
}
```

Also, `GameCanvas : Panel` in Car Racing extends panel behavior (double buffering) for smooth rendering.

### E. Composition and Association
Composition:
- `AppServices` composes store + services + session in one object graph.

Association:
- `GameHistory` and `LoanHistory` associate records to users via `UserId`.

---

## 5. Where OOP Is Visible in the Flow

- **Login/Register**: `Forms/LoginForm` and `Forms/RegisterForm` call `AuthService`, which applies user/account rules (`CanLogin`) and persistence through `IAppDataStore`.
- **Admin Dashboard**: `Forms/AdminDashboardForm` calls `AdminService`, which updates user objects through domain methods (`Approve`, `Reject`).
- **Loan Module**: `Forms/LoanForm` creates a `Loan` object and calls `LoanService`; math and schedule logic stay encapsulated in `Domain/Loan`.
- **Game Module**: UI forms delegate game state/rules to `Domain/Games/TicTacToeGame` and `Domain/Games/CarRacingGame`; scores are persisted by `GameService`.

---

## 6. Summary

This project demonstrates C# and OOP by separating UI, business rules, and data access into dedicated classes/layers.  
The result is:
- clearer responsibilities,
- reusable business logic,
- easier testing and extension,
- and strong alignment with OOP lecture concepts in a practical WinForms application.
