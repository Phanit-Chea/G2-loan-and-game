# Loan Management & Game System

A comprehensive ASP.NET Core MVC application combining a Loan Management System (Amortization, Extra Payments, History) and a Game Center (Tic-Tac-Toe, Car Racing).

## Prerequisites

Before running this project, ensure you have the following installed:

*   **[.NET 8.0 / 10.0 SDK](https://dotnet.microsoft.com/download)** (depending on your VS version)
*   **Visual Studio 2022** (with ASP.NET and Web Development workload)
*   **SQL Server Express** or **LocalDB**

## Getting Started

### 1. Clone/Download the Project
Download the source code to your local machine.

### 2. Configure Database Connection
Open `LoanSystem.Web/appsettings.json`.
Ensure the `DefaultConnection` string points to your valid SQL Server instance.
By default, it uses `(localdb)\\mssqllocaldb`. If you have a full SQL instance, change it to your server name.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LoanSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Setup the Database
You need to apply the migrations to create the database and tables.
Open a terminal (Command Prompt or PowerShell) in the `LoanSystem.Web` folder and run:

```bash
cd LoanSystem.Web
dotnet ef database update
```

*Note: If you don't have the EF Core tool installed globally, run `dotnet tool install --global dotnet-ef` first.*

### 4. Run the Application
You can run the application directly from Visual Studio (F5) or via terminal:

```bash
dotnet run
```

Access the application at: `https://localhost:7288` (or the port shown in your terminal).

## Default Admin Credentials

The system automatically creates an Admin user on first run (via `DbInitializer`).

*   **Email**: `admin@loansystem.com`
*   **Password**: `Admin123!`

**Note for New Users**:
New user registrations default to **"Pending"** status. You must log in as the Admin first to **Approvove** new accounts via the Admin Dashboard before they can log in.

## Features

*   **Loan Calculator**:
    *   Monthly & Yearly Amortization Schedules.
    *   Support for Extra Payments (Monthly, Yearly, One-time).
    *   Calculation History Tracking.
    *   Export Schedule to CSV.
*   **Game Center**:
    *   Tic-Tac-Toe (vs AI).
    *   Car Racing (Canvas Game).
    *   Score History & Leaderboard.
*   **Security**:
    *   Role-based access (Admin/User).
    *   Admin approval workflow for new registrations.
