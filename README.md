# Order Management System

A course project to build a minimal order management system using **ASP.NET Core MVC** combined with **Entity Framework Core**, **SQL Server LocalDB**, and **LINQ** queries.

---

## 📋 Key Features

1. **List Orders**: Display a complete list of orders with the corresponding customer names (using Eager Loading `.Include()`).
2. **Search by Customer Name**: Dynamically search for orders of customers by matching keywords `.Where(o => o.Customer.Name.Contains(keyword))`.
3. **Filter by Order Status**: Quickly filter the list by statuses: *Pending, Processing, Completed, Cancelled*.
4. **Top 5 Largest Orders**: Sort in descending order by amount and retrieve the 5 orders with the highest values `.OrderByDescending().Take(5)`.
5. **Statistics by Status**: Group and display the number of orders for each status using the `.GroupBy().Select(g => g.Count())` operator.
6. **Calculate Total Revenue**: Calculate the total amount from completed orders (`Completed`) using the `.Sum()` clause.
7. **Order Operations (CRUD)**:
   - Create new orders.
   - Update status directly in the table.
   - Delete orders.
8. **Visual LINQ Framework**: The right column of the interface automatically displays the LINQ statement corresponding to the current action for demo/presentation purposes.

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **ORM**: Entity Framework Core (EF Core)
- **Database**: Microsoft SQL Server LocalDB (`MSSQLLocalDB`)
- **User Interface**: HTML5, CSS3 (Light Mode / White Theme), Razor View Engine, Vanilla JS (only used for modal toggle).
- **Architecture**: Pure MVC (Server-side rendering, no AJAX, no JSON API responses).

---

## 🚀 Local Setup Guide

1. **Start LocalDB**:
   Open PowerShell or Command Prompt and run the command to activate the SQL Server LocalDB instance:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

2. **Run the Application**:
   Navigate to the project directory and start the application:
   ```bash
   dotnet run
   ```
   *Note*: EF Core will automatically initialize the `OrderManagementDb` database and insert seed data on the first run via the `context.Database.EnsureCreated()` command.

3. **Access the Interface**:
   Open your web browser and navigate to:
   👉 **[http://localhost:5093](http://localhost:5093)**

---

## 📤 Push to GitHub Guide

To upload the project source code to your personal GitHub repository, execute the following Git commands in the root directory:

1. **Initialize Git Local Repository**:
   ```bash
   git init
   ```
2. **Add All Files to Staging** (temporary Visual Studio/build folder files are automatically ignored via `.gitignore`):
   ```bash
   git add .
   ```
3. **Commit the Initial Source Code**:
   ```bash
   git commit -m "Initial commit: Pure ASP.NET Core MVC Order Management System"
   ```
4. **Rename the Main Branch to `main`**:
   ```bash
   git branch -M main
   ```
5. **Link with Your Empty Repository on GitHub**:
   *Replace `<your-github-url>` with your actual repository URL.*
   ```bash
   git remote add origin <your-github-url>
   ```
6. **Push the Source Code to GitHub**:
   ```bash
   git push -u origin main
   ```

---

## 💬 Message Analysis

All system messages, interface labels, error information, and LINQ explanations in the source code and interface have been **100% converted to English** to meet professional standards, internationalization, and serve course project reports best.

Below is a detailed analysis of message categories:

### 1. Action Notification Messages
Use `TempData` to display success or error notification banners at the top of the web page after performing an action with automatic redirect:
- **Order Created Successfully**:
  - Message: `Order #{Id} created successfully!`
- **Order Status Updated Successfully**:
  - Message: `Order #{Id} status updated to {Status}!`
- **Order Deleted Successfully**:
  - Message: `Order #{Id} deleted successfully!`

### 2. Business Validation & Error Messages
Messages displayed when input data is invalid or entities do not exist:
- **Customer Does Not Exist**:
  - Message: `Customer does not exist.`
- **Invalid Amount**:
  - Message: `Total Amount must be greater than 0.`
- **Order Not Found**:
  - Message: `Order not found.`

### 3. DTO Data Annotation Messages
Configured with validation attributes before sending data to the database:
- `[Required(ErrorMessage = "Customer ID is required.")]`
- `[Range(0.01, ..., ErrorMessage = "Total Amount must be greater than 0.")]`
- `[RegularExpression(..., ErrorMessage = "Invalid Status. Must be: Pending, Processing, Completed, Cancelled.")]`

### 4. Active LINQ Descriptions
Description strings showing LINQ operations displayed dynamically in the right column of the interface:
- **View All**: `"Retrieve all orders, eagerly load the related Customer details, and sort by date descending."`
- **Search**: `"Search for orders where the related customer's name contains '{searchString}'."`
- **Filter**: `"Filter orders that match the specified status '{statusFilter}'."`
- **Top 5**: `"Sort all orders descending by TotalAmount and retrieve the first 5 records."`
- **Revenue Statistics**: `"Filter orders by 'Completed' status and calculate the total sum of their amounts."`
- **Status Statistics**: `"Group orders by status and count the number of orders in each group."`
