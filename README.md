# TradeSphere3

TradeSphere3 is a comprehensive trading platform built with ASP.NET Core 3.1 that connects traders and customers in a secure and efficient marketplace environment. The application provides a robust ecosystem for product listing, order management, trader profiles, and communication between users.

## Project Overview

TradeSphere3 is designed as a full-featured trading platform that allows:
- Users to register and authenticate
- Traders to create profiles and list products
- Customers to browse products and place orders
- Secure messaging between traders and customers
- Feedback system for quality assurance

The application is built using ASP.NET Core 3.1 with Entity Framework Core for data access, Identity for authentication and authorization, and follows a repository pattern architecture for clean separation of concerns.

## Features Implemented

### Core Features
- **User Authentication & Authorization**
  - Registration and login system
  - Role-based access control (Admin, Trader, Customer)
  - Secure password policies

- **Trader Management**
  - Trader profile creation and management
  - Trader directory for customers to browse
  - Trust score and turnover tracking

- **Product Management**
  - Product listing with details and pricing
  - Product search and filtering
  - Product editing and deletion

- **Order System**
  - Order creation and tracking
  - Order history for both traders and customers
  - Order status management

- **Messaging System**
  - Direct messaging between traders and customers
  - Message history and threading
  - Notification system

- **Feedback System**
  - Customer feedback on traders and products
  - Rating system for quality assurance

## Setup Instructions

### Prerequisites
- .NET Core 3.1 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2019+ or Visual Studio Code

### Installation Steps
1. Clone the repository:
   ```
   git clone https://github.com/yourusername/TradeSphere3.git
   cd TradeSphere3
   ```

2. Restore dependencies:
   ```
   dotnet restore
   ```

3. Update the database connection string in `appsettings.json` if needed (default uses LocalDB):
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TradeSphereDB31;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. Apply database migrations:
   ```
   dotnet ef database update
   ```

5. Run the application:
   ```
   dotnet run
   ```
   
   Alternatively, use the provided development scripts:
   - Windows Command Prompt: `dev.cmd`
   - PowerShell: `.\dev.ps1`

6. Access the application at:
   - https://localhost:5001
   - http://localhost:5000

## Team Members and Individual Contributions

### Vrund Patel
- **Message System**
  - Implemented the messaging functionality between users
  - Created message repository and controller
  - Designed message views and user interface

- **Product Management**
  - Developed product listing and management features
  - Implemented product search and filtering
  - Created product detail views and forms

- **Order System**
  - Built the order creation and tracking system
  - Implemented order history views
  - Developed order status management

### Romil Rabadiya
- **User Authentication**
  - Implemented user registration and login
  - Set up role-based authorization
  - Created account management features

- **Trader Management**
  - Developed trader profile creation and management
  - Built the trader directory
  - Implemented trader search and filtering

- **Feedback System**
  - Created the feedback submission system
  - Implemented rating functionality
  - Developed feedback display and management

- **Application Architecture**
  - Set up the project structure and patterns
  - Configured Entity Framework and Identity
  - Implemented repository pattern

