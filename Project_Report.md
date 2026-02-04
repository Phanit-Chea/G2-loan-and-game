# Project Documentation: Loan Management & Game System

## 1. Project Overview

### Loan Management System
The Loan Management System is a financial tool designed to assist users in understanding repayment schedules for loans. It allows users to input principal amounts, interest rates, and terms to generate detailed amortization schedules. The system supports complex scenarios including extra monthly, yearly, and one-time payments, providing users with immediate feedback on how these extra payments impact their total interest and payoff date.

### Game Center
The Game Center serves as an entertainment module within the application, featuring two distinct games:
*   **Tic-Tac-Toe**: A classic strategy game implemented with a clean web interface, allowing users to play against an AI logic.
*   **Car Racing**: A fast-paced, reflex-based game built using HTML5 Canvas, where players dodge obstacles and collect coins to achieve high scores.
All game scores are tracked and persisted to a history log, allowing users to review their performance over time.

---

## 2. Project Scope

### Loan System Scope
*   **Input Handling**: Validates user inputs for Loan Amount, Interest Rate, Term (Years/Months), and Start Date.
*   **Calculation Engine**: Performs real-time calculation of monthly payments using standard amortization formulas.
*   **Schedule Generation**: Generates both Monthly and Yearly amortization schedules showing Principal, Interest, and Balance for each period.
*   **Advanced Features**:
    *   Supports **Extra Monthly Payments**.
    *   Supports **Extra Yearly Payments**.
    *   Supports **One-Time Payments** on specific dates.
*   **Data Persistence**: Saves every calculation into a `LoanHistory` table for future reference.
*   **Export**: Capability to export the calculated schedule to a CSV file.

### Game System Scope
*   **Tic-Tac-Toe**:
    *   Single-player mode (User vs System).
    *   Win/Loss/Draw detection logic.
    *   Automatic score saving upon game completion.
*   **Car Racing**:
    *   Real-time graphical rendering using HTML5 Canvas.
    *   Collision detection logic (Player vs Enemy, Player vs Coin).
    *   Dynamic scoring system (Distance + Coin Collection).
    *   Visual assets using Emojis for lightweight performance.
*   **History Tracking**: A centralized dashboard that displays the history of all games played, including scores and dates.

---

## 3. Lecture Review: OOP Concepts Implemented

This project is built using **ASP.NET Core MVC**, which naturally enforces Object-Oriented Programming (OOP) principles. Below are the key concepts applied:

### A. Encapsulation
Encapsulation bundles data (properties) and methods (behavior) together and restricts direct access to some of an object's components.
*   **Concept**: We hide the internal calculation logic of the loan within the `Loan` class. The Controller doesn't need to know *how* to calculate the formula, it just asks the `Loan` object to do it.
*   **Code Example** (`Loan.cs`):
    ```csharp
    public class Loan
    {
        // Data Properties
        public decimal LoanAmount { get; set; }
        public double InterestRate { get; set; }

        // Encapsulated Behavior: The formula is hidden inside this method.
        public decimal CalculateMonthlyPayment()
        {
            double monthlyRate = (InterestRate / 100) / 12;
            // ... complex math formula ...
            return Math.Round((decimal)payment, 2);
        }
    }
    ```

### B. Inheritance
Inheritance allows a class to derive properties and characteristics from another class, promoting code reuse.
*   **Concept**: Our user model `ApplicationUser` extends the built-in `IdentityUser`. This means `ApplicationUser` automatically has all functionality of a standard user (Password hashing, Email storage, Id) but we can extend it with our own specific fields.
*   **Code Example** (`ApplicationUser.cs`):
    ```csharp
    // Inherits from IdentityUser (Base Class)
    public class ApplicationUser : IdentityUser 
    {
        // Added specialized properties
        public string FullName { get; set; }
        public string AccountStatus { get; set; }
    }
    ```

### C. Polymorphism
Polymorphism allows methods to do different things based on the object it is acting upon, typically through method overriding.
*   **Concept**: We override the `OnModelCreating` method in our `DbContext`. The base `IdentityDbContext` has its own configuration, but we override it to add our own custom database configurations while still keeping the base behavior.
*   **Code Example** (`ApplicationDbContext.cs`):
    ```csharp
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Calls the base implementation first
        base.OnModelCreating(builder);
        
        // Adds our custom polymorphic behavior
        // (Configuration for LoanHistory types, etc.)
    }
    ```

### D. Association & Composition
Objects relate to one another. Composition implies a relationship where "Part" cannot exist without "Whole".
*   **Concept**: `LoanHistory` has a relationship with `ApplicationUser`. A history record "belongs to" a user.
*   **Code Example** (`LoanHistory.cs`):
    ```csharp
    public class LoanHistory
    {
        public string UserId { get; set; }
        
        // Navigation Property (Association)
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } 
    }
    ```

---

## 4. Summary

This project successfully integrates a robust financial tool with an engaging entertainment platform. By strictly adhering to **Object-Oriented Programming principles**, we achieved a codebase that is:
*   **Maintainable**: Logic is encapsulated in Models (`Loan.cs`), keeping Controllers thin.
*   **Extensible**: Using Inheritance (`ApplicationUser`) allowed us to easily customize the identity system.
*   **Reliable**: Separation of concerns ensures that game logic does not interfere with critical loan calculation logic.

The system is fully functional, secure (Role-Based Access Control), and ready for deployment.
