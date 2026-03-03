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
**Meaning:** Encapsulation means data and the rules that control that data are kept in the same class.

**How this project uses it:**  
- `AppUser` controls account transitions (`Approve`, `Reject`, `Terminate`, `MarkDeleted`) so UI/services do not directly set status rules.
- `Loan` keeps amortization formula and schedule generation logic inside domain methods.

```csharp
public bool CanLogin()
{
    return Status == AccountStatus.Active;
}

public void Approve()
{
    if (!IsAdmin)
    {
        Status = AccountStatus.Active;
    }
}
```

```csharp
public override decimal CalculateMonthlyPayment()
{
    var monthlyRate = MonthlyInterestRate;
    var totalMonths = TotalMonths;
    // calculation is hidden inside the domain class
}
```

### B. Abstraction
**Meaning:** Abstraction means exposing a clear contract and hiding implementation details.

**How this project uses it:**  
- Data operations are abstracted by `IAppDataStore`, so services only depend on the contract.
- Game behavior is abstracted by `IGameSession`.
- Loan behavior contract is abstracted by `LoanBase`.

`IAppDataStore` contract:

```csharp
public interface IAppDataStore
{
    AppUser? GetUserByEmail(string email);
    void AddUser(AppUser user);
    void UpdateUser(AppUser user);
}
```

Service depending on abstraction:

```csharp
public class AuthService
{
    private readonly IAppDataStore _store;
    public AuthService(IAppDataStore store) => _store = store;
}
```

### C. Abstract Class
**Meaning:** An abstract class provides shared members plus abstract members that child classes must implement.

**How this project uses it:**  
- `LoanBase` defines common loan structure (`TotalMonths`, `MonthlyInterestRate`) and abstract operations (`CalculateMonthlyPayment`, schedule generation).
- `GameSessionBase` defines shared game session state (`Score`, `IsRunning`) and the abstract `BuildOutcome`.

```csharp
public abstract class LoanBase
{
    protected int TotalMonths => (TermYears * 12) + TermMonths;
    protected double MonthlyInterestRate => (InterestRate / 100d) / 12d;

    public abstract decimal CalculateMonthlyPayment();
    public abstract List<MonthlyAmortizationSchedule> GenerateMonthlySchedule();
}
```

```csharp
public abstract class GameSessionBase : IGameSession
{
    public bool IsRunning { get; protected set; }
    public int Score { get; protected set; }
    public abstract GameOutcome BuildOutcome();
}
```

### D. Inheritance
**Meaning:** Inheritance allows a class to reuse and extend behavior from a base class.

**How this project uses it:**  
- `Loan` inherits from `LoanBase`.
- `TicTacToeGame` and `CarRacingGame` inherit from `GameSessionBase`.
- UI classes inherit WinForms base classes such as `Form` and `Panel`.

```csharp
public class Loan : LoanBase
public class TicTacToeGame : GameSessionBase
public class CarRacingGame : GameSessionBase
```

### E. Polymorphism
**Meaning:** Polymorphism lets one interface/base type represent many concrete implementations.

**How this project uses it:**  
- `GameService.SaveScore(string userId, IGameSession game)` works with any game that implements `IGameSession`.
- Both Tic-Tac-Toe and Car Racing forms call the same save method with different concrete game objects.

```csharp
public void SaveScore(string userId, IGameSession game)
{
    var outcome = game.BuildOutcome();
    SaveScore(userId, outcome.GameName, outcome.Score, outcome.Result);
}
```

This enables extending with new games without changing `GameService` logic.

### F. Composition
**Meaning:** Composition means building a larger object by combining smaller objects.

**How this project uses it:**  
- `AppServices` composes data store + services + session in one place (composition root).

```csharp
public AppServices()
{
    DataStore = new AppDataStore();
    AuthService = new AuthService(DataStore);
    AdminService = new AdminService(DataStore);
    LoanService = new LoanService(DataStore);
    GameService = new GameService(DataStore);
    UserDashboardService = new UserDashboardService(DataStore);
    Session = new AppSession();
}
```

### G. Association
**Meaning:** Association means objects are linked to each other (often by identifiers) without ownership inheritance.

**How this project uses it:**  
- `GameHistory` and `LoanHistory` are associated with users via `UserId`.
- Repository queries filter by `UserId` to retrieve each user’s records.

```csharp
public class GameHistory
{
    public string UserId { get; set; } = string.Empty;
}
```

```csharp
return _data.GameHistories
    .Where(x => x.UserId == userId)
    .OrderByDescending(x => x.PlayedAt)
    .Take(take)
    .ToList();
```

### H. Separation of Concerns
**Meaning:** Separation of Concerns means each layer/class has one clear responsibility.

**How this project uses it:**  
- Forms manage UI and user interaction.
- Services coordinate use-cases.
- Domain classes hold business rules.
- Data layer handles persistence.

```csharp
// Form layer
var loan = new Loan { /* user input */ };
var result = _services.LoanService.Calculate(loan);

// Service layer
var monthlySchedule = loan.GenerateMonthlySchedule();
```

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
