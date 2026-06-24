# Order Management System

A minimalistic course project implementing an Order Management System built using the pure **ASP.NET Core MVC** pattern, **Entity Framework Core**, **SQL Server LocalDB**, and **LINQ** queries.

---

## 📋 Key Features

1. **Order Listing**: Retrieves and displays the full list of orders along with eager-loaded customer details using `.Include(o => o.Customer)`.
2. **Search by Customer Name**: Performs dynamic database filtering on customer names using `.Where(o => o.Customer.Name.Contains(keyword))`.
3. **Filter by Order Status**: Instantly filters orders based on their status: *Pending, Processing, Completed, or Cancelled*.
4. **Top 5 Orders**: Sorts all orders by their total amount in descending order and retrieves the top 5 records using `.OrderByDescending().Take(5)`.
5. **Status Breakdown (Statistics)**: Groups orders by their status and retrieves the counts using `.GroupBy(o => o.Status).Select(...)`.
6. **Total Revenue**: Calculates the sum of the total amounts of all completed orders using `.Where(o => o.Status == "Completed").Sum(...)`.
7. **Order Actions (CRUD)**:
   - **Create Order**: Adds a new order mapped to an existing customer.
   - **Update Status**: Dynamically modifies an order's status via dropdown postbacks.
   - **Delete Order**: Permanently removes an order from the database.
8. **Interactive LINQ Query Panel**: A dedicated side-by-side visualization panel showing the exact LINQ code and plain-English explanation behind the active database operation (perfect for reports and presentations).

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **ORM**: Entity Framework Core (EF Core)
- **Database**: Microsoft SQL Server LocalDB (`MSSQLLocalDB`)
- **Frontend**: HTML5, Vanilla CSS3 (Clean Light Theme, no dark mode, no heavy AI styling), Razor View Engine, and basic Vanilla JS for modal control.
- **Architecture**: Pure Server-Side MVC (traditional postbacks, no AJAX/JSON API controllers).

---

## 🚀 Local Setup Guide

1. **Start SQL Server LocalDB**:
   Open PowerShell or Command Prompt and run the following command to activate the LocalDB instance:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

2. **Run the Application**:
   Navigate to the project root directory and execute:
   ```bash
   dotnet run
   ```
   *Note: EF Core will automatically create the database `OrderManagementDb` and populate it with seed data on the first run using `context.Database.EnsureCreated()`.*

3. **Access the Application**:
   Open your browser and navigate to:
   👉 **[http://localhost:5093](http://localhost:5093)**

---

## 📤 Push to GitHub Guide

To push your local project to a GitHub repository, follow these standard Git commands in your terminal:

1. **Initialize Git Repository**:
   ```bash
   git init
   ```
2. **Stage All Files**:
   *(Files generated during build or IDE config are automatically excluded via the `.gitignore` file)*
   ```bash
   git add .
   ```
3. **Commit Changes**:
   ```bash
   git commit -m "Initial commit: Pure ASP.NET Core MVC Order Management System in English"
   ```
4. **Rename Default Branch to `main`**:
   ```bash
   git branch -M main
   ```
5. **Add GitHub Remote Origin**:
   *Replace `<your-github-url>` with your actual remote repository URL.*
   ```bash
   git remote add origin <your-github-url>
   ```
6. **Push to GitHub**:
   ```bash
   git push -u origin main
   ```

---

## 💬 Message and Localization Analysis

All user-facing strings, system notifications, error messages, model validations, and LINQ explanations have been translated **100% to English** to comply with academic standards and provide an internationalized presentation.

### 1. Action Notifications (`TempData` Messages)
Displayed as dismissible status banners at the top of the order list page upon redirection:
- **Order Creation**:
  - *Previous*: `Tạo đơn hàng #{Id} thành công!`
  - *English*: `Order #{Id} created successfully!`
- **Status Updates**:
  - *Previous*: `Đã cập nhật đơn hàng #{Id} sang trạng thái {Status}!`
  - *English*: `Order #{Id} status updated to {Status}!`
- **Order Deletion**:
  - *Previous*: `Đã xóa đơn hàng #{Id} thành công!`
  - *English*: `Order #{Id} deleted successfully!`

### 2. Business Validation & Error Logs
- **Customer Verification**:
  - *Previous*: `Khách hàng không tồn tại.`
  - *English*: `Customer does not exist.`
- **Invalid Amounts**:
  - *Previous*: `Số tiền phải lớn hơn 0.`
  - *English*: `Amount must be greater than 0.`
- **Order Lookups**:
  - *Previous*: `Không tìm thấy đơn hàng.`
  - *English*: `Order not found.`
- **Database Startup Logs**:
  - *Previous*: `Đã xảy ra lỗi khi khởi tạo cơ sở dữ liệu.`
  - *English*: `An error occurred while seeding/initializing the database.`

### 3. Data Validation (DTO Data Annotations)
Validations applied to client-side inputs prior to controller submission:
- `[Required(ErrorMessage = "Customer ID is required.")]`
- `[Range(0.01, ..., ErrorMessage = "Total Amount must be greater than 0.")]`
- `[RegularExpression(..., ErrorMessage = "Invalid Status. Must be: Pending, Processing, Completed, Cancelled.")]`

### 4. Interactive LINQ Panel Explanations
Explanations rendered dynamically inside the right-hand panel of the MVC interface:
- **Default View**: `"Retrieve all orders, eagerly load the related Customer details, and sort by date descending."`
- **Keyword Search**: `"Search for orders where the related customer's name contains '{searchString}'."`
- **Status Filter**: `"Filter orders that match the specified status '{statusFilter}'."`
- **Top 5 Orders**: `"Sort all orders descending by TotalAmount and retrieve the first 5 records."`
- **Total Revenue**: `"Filter orders by 'Completed' status and calculate the total sum of their amounts."`
- **Status Statistics**: `"Group orders by status and count the number of orders in each group."`
