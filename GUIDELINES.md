# Inventory and Sales System - Guidelines

## Project Information

**Project Name:** InventoryAndSalesSystem  
**Type:** Desktop Application (WPF - Windows Presentation Foundation)  
**Framework:** .NET 8.0  
**Language:** C# with XAML for UI  
**Architecture:** MVVM (Model-View-ViewModel)

## What's It About

This is a comprehensive inventory management system designed to help businesses track products, manage stock levels, process sales, and generate advanced reports with profit analysis. The application provides:

- **Product Management**: Add, edit, and delete products with categories, pricing, and stock tracking
- **Inventory Control**: Monitor stock levels with automatic low-stock alerts
- **Sales Processing**: Record sales transactions and automatically update inventory
- **Stock Operations**:
  - Restock products
  - Process sales returns (customer returns items)
  - Process purchase returns (return items to supplier)
  - Record product losses (expired, damaged, broken items)
- **Advanced Reporting & Analytics**:
  - View sales data with date range filtering
  - Analyze profit margins and cost tracking
  - Generate bar charts (Revenue, Profit, Units Sold) via ScottPlot
  - Export comprehensive reports to PDF via QuestPDF
- **Cycle Count**: Physical inventory counting with variance tracking per session
- **Audit Trail**: Complete log of every action performed in the system
- **Auto Backup**: Weekly automatic backup of all Excel data files, with manual backup option
- **Data Persistence**: All data stored in Excel files using EPPlus library

## Tech Stack

### Core Technologies
- **.NET 8.0** - Target framework
- **WPF (Windows Presentation Foundation)** - UI framework
- **C#** - Primary programming language
- **XAML** - UI markup language

### Key NuGet Packages
- **EPPlus 7.0.3** - Excel file manipulation for data storage
- **ScottPlot.WPF 5.1.57** - Data visualization and charting (replaces LiveChartsCore)
- **QuestPDF 2025.12.3** - PDF generation and export (replaces itext7)

### Design Patterns
- **MVVM (Model-View-ViewModel)** - Separation of concerns
- **INotifyPropertyChanged** - Data binding and UI updates
- **RelayCommand** - Command pattern for UI actions
- **Value Converters** - Data transformation for UI binding

## Project Structure

```
InventoryAndSalesSystem/
├── Assets/
│   └── logo.png                    # App logo shown in header
├── Models/                         # Data models
│   ├── Product.cs                  # Product entity
│   ├── Sale.cs                     # Sales transaction entity
│   └── StockLog.cs                 # Stock movement tracking
├── ViewModels/                     # Business logic and data binding
│   ├── MainViewModel.cs
│   ├── ProductViewModel.cs         # Handles all product CRUD + stock ops + audit logging
│   ├── ReportViewModel.cs          # Profit analysis, date filtering, PDF export
│   ├── CycleCountViewModel.cs      # Cycle count session management
│   ├── AuditTrailViewModel.cs      # Audit log viewing and filtering
│   └── BackupViewModel.cs          # Manual/auto backup management
├── Views/                          # UI components
│   ├── ProductListView.xaml        # Product list with search/filter
│   ├── AddEditProductView.xaml     # Product create/edit form
│   ├── SellView.xaml               # Sales transaction entry
│   ├── RestockView.xaml            # Stock replenishment
│   ├── SalesReturnView.xaml        # Customer return processing
│   ├── PurchaseReturnView.xaml     # Supplier return processing
│   ├── ProductLossView.xaml        # Loss/damage recording
│   ├── ReportsView.xaml            # Advanced analytics + ScottPlot charts
│   ├── CycleCountView.xaml         # Physical count sessions + count sheet
│   ├── AuditTrailView.xaml         # Filterable audit log grid
│   └── BackupView.xaml             # Backup status and history
├── Services/                       # Data access and business logic layer
│   ├── ExcelDataService.cs         # All Products/Sales/StockLogs Excel operations
│   ├── PdfExportService.cs         # QuestPDF report generation
│   ├── AuditTrailService.cs        # AuditTrail.xlsx read/write + helper log methods
│   ├── BackupService.cs            # Weekly auto-backup timer, zip creation, pruning
│   └── CycleCountService.cs        # CycleCounts.xlsx session and item management
├── Helpers/                        # Utility classes
│   ├── RelayCommand.cs             # ICommand implementation
│   ├── ValueConverters.cs          # BoolToVisibility, NullToVisibility, BoolToStatus, Multiply
│   └── ReportConverters.cs         # ReportTypeToVisibility, ProfitColor
├── Data/                           # Excel data files (auto-created at runtime)
│   ├── Products.xlsx
│   ├── Sales.xlsx
│   ├── StockLogs.xlsx
│   ├── AuditTrail.xlsx
│   ├── CycleCounts.xlsx            # Two sheets: Sessions, Items
│   └── Backups/                    # Auto-backup zip files (max 8 kept)
├── App.xaml                        # Global resources, styles, converters
├── App.xaml.cs                     # EPPlus license init
├── MainWindow.xaml                 # Navigation sidebar + ContentArea
└── MainWindow.xaml.cs              # View switching, service instantiation
```

## Important Commands & Operations

### Building and Running

```bash
# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build --configuration Release
```

### Visual Studio Commands
- **F5** - Build and run with debugging
- **Ctrl+F5** - Build and run without debugging
- **Ctrl+Shift+B** - Build solution

## Navigation Structure

The sidebar is divided into four groups:

| Group | Button | View |
|-------|--------|------|
| INVENTORY | 📦 Products | ProductListView |
| INVENTORY | ➕ Add / Edit Product | AddEditProductView |
| OPERATIONS | 🛒 Sell Product | SellView |
| OPERATIONS | 📈 Restock | RestockView |
| OPERATIONS | ↩️ Sales Return | SalesReturnView |
| OPERATIONS | 📥 Purchase Return | PurchaseReturnView |
| OPERATIONS | ❌ Product Loss | ProductLossView |
| ANALYTICS | 📊 Reports | ReportsView |
| ANALYTICS | 🔢 Cycle Count | CycleCountView |
| SYSTEM | 📋 Audit Trail | AuditTrailView |
| SYSTEM | 💾 Backup | BackupView |

## Workflow

### 1. Application Startup
1. EPPlus license context set to NonCommercial in `App.xaml.cs`
2. `MainWindow` instantiates: `ExcelDataService`, `MainViewModel`, `AuditTrailService`, `BackupService`, `CycleCountService`
3. `BackupService.Start()` is called — runs backup immediately if 7+ days overdue, then checks hourly
4. Default view is `ProductListView` with `ProductViewModel`
5. Products loaded from `Products.xlsx`; low-stock alert shown if any product is at/below MinStock

### 2. Product Management Flow
```
Add Product:
1. Navigate to "Add/Edit Product"
2. Fill in: Name, Category (editable ComboBox), Unit Cost, Price, Stock, MinStock
3. Click "Save Product"
4. Product saved to Products.xlsx; StockLog created with action "New Product"
5. AuditTrailService.LogProductAdded() called
6. Product list refreshes; form clears

Edit Product:
1. Navigate to "Add/Edit Product"; select product from right-side list
2. Form auto-populates with current values
3. Modify details → Click "Save Product"
4. Before-state snapshot taken; AuditTrailService.LogProductEdited(before, after) called
5. Product updated in Products.xlsx

Delete Product:
1. Select product → Click "Delete" → Confirm dialog
2. AuditTrailService.LogProductDeleted() called
3. Product removed from Products.xlsx
```

### 3. Sales Transaction Flow
```
1. Navigate to "Sell Product"
2. Select product from dropdown (shows price + available stock)
3. Enter quantity; live total preview shown via MultiplyConverter
4. Click "Complete Sale"
5. Validates quantity ≤ available stock
6. Sale record saved to Sales.xlsx
7. Product stock updated in Products.xlsx
8. StockLog entry created with action "Sale"
9. AuditTrailService.LogSale() called
```

### 4. Stock Management Flows

#### Restock
```
1. Navigate to "Restock"; select product (low-stock warning shown if applicable)
2. Enter quantity to add; after-restock preview shown
3. Click "Add Stock"
4. Product stock updated; StockLog created with action "Restock"
5. AuditTrailService.LogRestock() called
```

#### Sales Return (Customer returns items)
```
1. Navigate to "Sales Return"; select product; enter quantity and reason
2. Click "Process Return"
3. Stock increased; StockLog created with action "Sales Return" and reason
4. AuditTrailService.LogSalesReturn() called
```

#### Purchase Return (Return to supplier)
```
1. Navigate to "Purchase Return"; select product; enter quantity and reason
2. Validates quantity ≤ current stock
3. Click "Return to Supplier"
4. Stock decreased; StockLog created with action "Purchase Return" and reason
5. AuditTrailService.LogPurchaseReturn() called
```

#### Product Loss
```
1. Navigate to "Product Loss"; select product; enter quantity and reason
2. Validates quantity ≤ current stock
3. Confirmation dialog (warns action cannot be undone)
4. Stock decreased; StockLog created with action "Loss" and reason
5. AuditTrailService.LogProductLoss() called
```

### 5. Advanced Reporting Flow
```
1. Navigate to "Reports"
2. Set date range (From/To); click "Apply Filter"
3. Stats update: Total Revenue, Total Cost, Net Profit, Profit Margin %, Units Sold
4. Switch chart types via ComboBox: Revenue / Profit / Units Sold (ScottPlot bar charts, top 10)
5. Review Product Profit Analysis table (per-product revenue, cost, profit, margin)
6. Recent Sales and Recent Stock Activity tables show last 10 entries each
7. Click "Export to PDF" → SaveFileDialog → QuestPDF generates professional report
```

### 6. Cycle Count Flow
```
1. Navigate to "Cycle Count"
2. Enter optional session notes → Click "+ Start New Count"
   - Snapshots current stock quantities as "expected" for all products
   - AuditTrailService logs "Cycle Count Started"
3. Select session from left panel → Count sheet appears on right
4. Summary cards show: Total Items, Counted, Pending, Variances + variance value
5. Click a row in the DataGrid to select it
6. Enter Physical Count and optional notes → Click "Save Count"
   - Variance = Counted - Expected; VarianceValue = Variance × UnitCost
   - Auto-advances to next uncounted row
   - AuditTrailService logs "Cycle Count Item"
7. Click "Complete Session" when done (warns if items remain uncounted)
   - AuditTrailService logs "Cycle Count Completed"
8. Completed sessions are read-only; Open sessions allow counts
```

### 7. Audit Trail Flow
```
1. Navigate to "Audit Trail"
2. Filter by: keyword search (EntityName, Details, Action), Action Type dropdown, From/To dates
3. Real-time filtering; record count shown ("Showing X of Y records")
4. Click "Refresh" to reload from file; "Clear" to reset all filters
```

### 8. Backup Flow
```
Auto Backup (automatic):
- BackupService checks hourly if 7+ days have passed since last backup
- If overdue: zips all .xlsx files from Data/ → Data/Backups/Backup_yyyy-MM-dd_HH-mm-ss.zip
- Includes backup_manifest.txt inside zip
- Keeps max 8 backups (oldest pruned automatically)
- Logs "Auto Backup" to AuditTrail

Manual Backup:
1. Navigate to "Backup"
2. View: Last Backup date, Next Scheduled date, Stored Backups count
3. Click "Backup Now" → backup created immediately
4. AuditTrailService.LogManualBackup() called
5. Click "Open Backup Folder" to open Windows Explorer to backups directory
```

## Data Storage Schema

### Products.xlsx
| Column | Type | Description |
|--------|------|-------------|
| ID | int | Unique identifier |
| Name | string | Product name |
| Category | string | Product category |
| UnitCost | decimal | Cost per unit (for profit calculation) |
| Price | decimal | Selling price |
| Stock | int | Current stock quantity |
| MinStock | int | Minimum stock threshold |

### Sales.xlsx
| Column | Type | Description |
|--------|------|-------------|
| ID | int | Transaction ID |
| ProductId | int | Reference to product |
| ProductName | string | Product name snapshot |
| Quantity | int | Units sold |
| Price | decimal | Sale price |
| Total | decimal | Calculated (Quantity × Price) |
| Date | DateTime | Transaction timestamp |

### StockLogs.xlsx
| Column | Type | Description |
|--------|------|-------------|
| ID | int | Log entry ID |
| ProductId | int | Reference to product |
| ProductName | string | Product name snapshot |
| Action | string | "New Product", "Sale", "Restock", "Sales Return", "Purchase Return", "Loss" |
| Quantity | int | Quantity affected |
| StockBefore | int | Stock level before action |
| StockAfter | int | Stock level after action |
| Reason | string | Explanation (for returns/losses) |
| Date | DateTime | Action timestamp |

### AuditTrail.xlsx
| Column | Type | Description |
|--------|------|-------------|
| ID | int | Entry ID |
| Timestamp | string | "yyyy-MM-dd hh:mm:ss tt" |
| Action | string | "Product Added", "Product Edited", "Product Deleted", "Sale", "Restock", "Sales Return", "Purchase Return", "Product Loss", "Cycle Count Started", "Cycle Count Item", "Cycle Count Completed", "Manual Backup", "Auto Backup" |
| Entity | string | "Product", "Sale", "System" |
| EntityId | int | Product/Sale ID (0 for system actions) |
| EntityName | string | Product name or "Backup", "Session #N" |
| Details | string | Human-readable description of the action |
| OldValue | string | Before-state (pipe-delimited for product edits) |
| NewValue | string | After-state (pipe-delimited for product edits) |

### CycleCounts.xlsx — Sheet: Sessions
| Column | Type | Description |
|--------|------|-------------|
| SessionId | int | Unique session ID |
| StartDate | string | Session creation timestamp |
| CompletedDate | string | Completion timestamp (empty if Open) |
| Status | string | "Open" or "Completed" |
| Notes | string | Optional session notes |

### CycleCounts.xlsx — Sheet: Items
| Column | Type | Description |
|--------|------|-------------|
| SessionId | int | Reference to session |
| ProductId | int | Reference to product |
| ProductName | string | Product name snapshot |
| Category | string | Product category snapshot |
| ExpectedQty | int | System stock at session start |
| CountedQty | int | Physical count (-1 = not yet counted) |
| Variance | int | CountedQty - ExpectedQty |
| UnitCost | decimal | Cost per unit at time of count |
| VarianceValue | decimal | Variance × UnitCost |
| Notes | string | Counter notes |
| Counted | bool | True if physical count has been entered |

## Key Features & Validations

### Low Stock Alert
- Triggered on application start
- Shows alert when `Stock <= MinStock`
- Lists all low-stock products in a warning dialog

### Product Categories
Predefined in ComboBox (editable — user can type custom categories):
- Garments, Snacks, Biscuits, Beverages, Accessories, Souvenir Bag, Stuff Toy

### Search & Filter (Product List)
- Search by: Product name, Category, or ID
- Filter by: Category dropdown
- Real-time filtering as you type
- Shows filtered count vs total count

### Cycle Count Variance Status
Each item in a count session is assigned a status:
- **Pending** — not yet counted (orange badge)
- **OK** — counted, variance = 0 (green badge)
- **Overage** — counted > expected (blue badge)
- **Shortage** — counted < expected (red badge)

### Currency Formatting
- Uses Philippine Peso (`en-PH` culture)
- Format: ₱X,XXX.XX

### Date/Time Format
- Storage: `"yyyy-MM-dd hh:mm:ss tt"`
- Display varies by view (e.g., `"MM/dd/yyyy hh:mm tt"`)

## MVVM Pattern Implementation

### Models
- Plain C# classes implementing `INotifyPropertyChanged`
- `Product`: Id, Name, Category, UnitCost, Price, Stock, MinStock, IsLowStock (computed)
- `Sale`: Id, ProductId, ProductName, Quantity, Price, Total (computed), Date
- `StockLog`: Id, ProductId, ProductName, Action, Quantity, StockBefore, StockAfter, Reason, Date
- `ProductProfitReport` (in ReportViewModel.cs): ProductName, UnitsSold, Revenue, Cost, Profit, ProfitMargin
- `CycleCountSession` (in CycleCountService.cs): SessionId, StartDate, CompletedDate, Status, Notes
- `CycleCountItem` (in CycleCountService.cs): full item data + computed HasVariance, VarianceStatus
- `AuditEntry` (in AuditTrailService.cs): full audit record

### Views
- XAML files with minimal or no code-behind (only `InitializeComponent()`)
- Exception: `ReportsView.xaml.cs` — contains ScottPlot chart update logic (`UserControl_Loaded`, `ChartType_SelectionChanged`, `UpdateChart`, chart loader methods)
- Exception: `SellView.xaml.cs` — contains a duplicate `NullToVisibilityConverter` class (does not conflict because the one in `App.xaml` from `Helpers/ValueConverters.cs` takes precedence globally)

### ViewModels
All implement `INotifyPropertyChanged` and use `RelayCommand`.

| ViewModel | Key Responsibilities |
|-----------|---------------------|
| ProductViewModel | Product CRUD, sell, restock, returns, loss — all with audit logging |
| ReportViewModel | Profit calculations, date range filtering, chart data, PDF export |
| CycleCountViewModel | Session creation, item counting, variance summary stats |
| AuditTrailViewModel | Load, filter, and display audit entries |
| BackupViewModel | Manual backup, backup info display, open folder |
| MainViewModel | Shell — holds ExcelDataService reference |

### Services
| Service | Responsibilities |
|---------|----------------|
| ExcelDataService | CRUD for Products.xlsx, Sales.xlsx, StockLogs.xlsx; robust date parsing for OADate/string/DateTime |
| PdfExportService | QuestPDF document with header, summary cards, profit table, recent sales, stock activity, footer |
| AuditTrailService | Append-only log to AuditTrail.xlsx; convenience helpers (LogProductAdded, LogSale, etc.); GetAllEntries() |
| BackupService | Timer-based weekly backup; zip all .xlsx from Data/; prune to 8 max; IDisposable |
| CycleCountService | CreateSession, GetAllSessions, GetSessionItems, SaveItemCount, CompleteSession |

### Helpers
| Class | Description |
|-------|-------------|
| RelayCommand | `ICommand` with optional `canExecute`; uses `CommandManager.RequerySuggested` |
| BoolToVisibilityConverter | `bool → Visibility` (true = Visible, false = Collapsed) |
| NullToVisibilityConverter | `null → Collapsed`, non-null → Visible |
| BoolToStatusConverter | `bool → "Low Stock" / "In Stock"` |
| MultiplyConverter | `IMultiValueConverter` — multiplies two values (used for sale total preview) |
| ReportTypeToVisibilityConverter | Shows/hides elements by matching string parameter to bound value |
| ProfitColorConverter | `decimal → "Positive" / "Negative" / "Neutral"` (used for DataGrid styling) |

## App.xaml Global Resources

All converters are registered in `App.xaml` as application-level resources:
```xml
BoolToVisibilityConverter, NullToVisibilityConverter, BoolToStatusConverter,
MultiplyConverter, ReportTypeToVisibilityConverter, ProfitColorConverter
```

Button styles defined globally: default `Button`, `PrimaryButton` (green), `SecondaryButton` (blue), `DangerButton` (red).

## Known Issues & Gotchas

### Duplicate NullToVisibilityConverter
`SellView.xaml.cs` contains a local `NullToVisibilityConverter` class. This is a leftover that does not cause a runtime error because the application-wide converter from `App.xaml` takes precedence. It should be cleaned up but is harmless.

### LetterSpacing not supported in WPF
`LetterSpacing` is a WinUI 3/MAUI property. Do not use it on `TextBlock` in WPF — it will cause a XAML parse error.

### CycleCountView Visibility Pattern
The right panel uses `DataTrigger` on `SelectedSession` with `{x:Null}` to toggle between the placeholder `StackPanel` (shown when null) and the session content `Grid` (shown when not null). A `Grid.Style` element can only appear once per element — having two `<Grid.Style>` blocks silently ignores the first, causing the panel to be permanently collapsed.

### Excel Date Parsing
`ExcelDataService.ParseExcelDate()` handles three formats: `DateTime` object, `double` (OA date serial), or formatted string. Always use this helper when reading date cells.

### BackupService Lifecycle
`BackupService` implements `IDisposable` and holds a `System.Threading.Timer`. `MainWindow.OnClosed` calls `_backupService.Dispose()` to stop the timer cleanly.

## Profit Calculation Logic

```csharp
// Per Sale
Revenue = Quantity × Selling Price
Cost    = Quantity × Unit Cost
Profit  = Revenue - Cost
Margin  = (Profit / Revenue) × 100

// Totals (for filtered period)
Total Revenue = Σ(sale.Total) for filtered sales
Total Cost    = Σ(product.UnitCost × sale.Quantity) for filtered sales
Total Profit  = Total Revenue - Total Cost
Profit Margin = (Total Profit / Total Revenue) × 100
```

Unit Cost is pulled from the current product record at report time (not stored per sale).

## Development Best Practices

1. Always validate user input before processing
2. Refresh product list after any CRUD operation (`LoadProducts()`)
3. Create StockLog entries for all stock-affecting actions
4. Call the appropriate `AuditTrailService` method after every meaningful user action
5. Show confirmation dialogs for destructive actions (delete, loss, complete session)
6. Show success/error `MessageBox` for user feedback
7. Handle null/empty `ComboBox` selections defensively
8. Use `UpdateSourceTrigger=PropertyChanged` for real-time form binding
9. Keep code-behind minimal — logic belongs in ViewModels
10. Use `RelayCommand` for all button bindings
11. Never add a second `<Grid.Style>` (or any property element) to the same element — WPF silently ignores duplicates
12. Never use `LetterSpacing` on WPF TextBlocks

## PDF Export Contents (QuestPDF)

Generated file includes:
- **Header**: System name + report title
- **Date Range**: Report period
- **Summary Cards**: Revenue (green), Cost (orange), Net Profit (blue/red), Profit Margin (purple)
- **Product Profit Analysis**: Top 20 products sorted by profit — Name, Units, Revenue, Cost, Profit (color-coded), Margin%
- **Recent Sales**: Last 15 transactions — Product, Qty, Total, Date
- **Recent Stock Activity**: Last 15 stock logs — Product, Action, Qty, Reason, Date
- **Footer**: Generation timestamp + copyright

`QuestPDF.Settings.License = LicenseType.Community` is set inside `PdfExportService.ExportReportToPdf()`.

## Version History

### Version 3.0 (Current)
- ✅ Added Cycle Count feature (sessions, item counting, variance tracking)
- ✅ Added Audit Trail (complete action log with filtering)
- ✅ Added Auto Backup (weekly schedule, manual trigger, 8-backup retention)
- ✅ ProductViewModel now logs all actions to AuditTrail
- ✅ Fixed CycleCountView blank panel bug (duplicate Grid.Style)
- ✅ Migrated charting from LiveChartsCore to ScottPlot.WPF
- ✅ Migrated PDF export from itext7 to QuestPDF

### Version 2.0
- ✅ Added advanced reporting with date range filtering
- ✅ Implemented profit margin calculations and cost tracking
- ✅ Added multiple chart types (Revenue, Profit, Units Sold)
- ✅ Integrated PDF export functionality
- ✅ Added ReportConverters for UI handling

### Version 1.0
- Initial release: product CRUD, sales, restock, returns, losses, basic charts, low-stock alerts, Excel persistence

## Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or later (recommended)
- Windows 10/11

### Steps
1. Clone or download the project
2. Open `InventoryAndSalesSystem.sln` in Visual Studio
3. Restore NuGet packages: `dotnet restore`
4. Build: `dotnet build`
5. Run: `dotnet run` or press **F5**

### Required NuGet Packages
```bash
dotnet add package EPPlus --version 7.0.3
dotnet add package ScottPlot.WPF --version 5.1.57
dotnet add package QuestPDF --version 2025.12.3
```

## Support & References

- .NET WPF docs: https://docs.microsoft.com/wpf
- EPPlus docs: https://github.com/EPPlusSoftware/EPPlus
- ScottPlot docs: https://scottplot.net/
- QuestPDF docs: https://www.questpdf.com/
