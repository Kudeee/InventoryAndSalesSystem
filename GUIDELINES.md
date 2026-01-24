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
  - Generate multiple chart types (Revenue, Profit, Units Sold)
  - Export comprehensive reports to PDF
- **Data Persistence**: All data stored in Excel files using EPPlus library

## Tech Stack

### Core Technologies
- **.NET 8.0** - Target framework
- **WPF (Windows Presentation Foundation)** - UI framework
- **C#** - Primary programming language
- **XAML** - UI markup language

### Key NuGet Packages
- **EPPlus 7.0.3** - Excel file manipulation for data storage
- **LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc2** - Data visualization and charting
- **itext7 8.0.2** - PDF generation and export functionality

### Design Patterns
- **MVVM (Model-View-ViewModel)** - Separation of concerns
- **INotifyPropertyChanged** - Data binding and UI updates
- **RelayCommand** - Command pattern for UI actions
- **Value Converters** - Data transformation for UI binding

## Project Structure

```
InventoryAndSalesSystem/
├── Models/                  # Data models
│   ├── Product.cs          # Product entity
│   ├── Sale.cs             # Sales transaction entity
│   └── StockLog.cs         # Stock movement tracking
├── ViewModels/              # Business logic and data binding
│   ├── MainViewModel.cs
│   ├── ProductViewModel.cs
│   └── ReportViewModel.cs  # Enhanced with profit analysis
├── Views/                   # UI components
│   ├── ProductListView.xaml
│   ├── AddEditProductView.xaml
│   ├── SellView.xaml
│   ├── RestockView.xaml
│   ├── SalesReturnView.xaml
│   ├── PurchaseReturnView.xaml
│   ├── ProductLossView.xaml
│   └── ReportsView.xaml    # Enhanced with advanced features
├── Services/                # Data access layer
│   ├── ExcelDataService.cs
│   └── PdfExportService.cs # NEW: PDF report generation
├── Helpers/                 # Utility classes
│   ├── RelayCommand.cs
│   ├── ValueConverters.cs
│   └── ReportConverters.cs # NEW: Report-specific converters
├── Data/                    # Excel data files (generated at runtime)
│   ├── Products.xlsx
│   ├── Sales.xlsx
│   └── StockLogs.xlsx
└── MainWindow.xaml          # Main application window
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
- **F6** - Build current project

## Workflow

### 1. Application Startup
1. Application initializes and sets EPPlus license context to NonCommercial
2. ExcelDataService creates Data folder and initializes Excel files if they don't exist
3. MainWindow loads with ProductListView as default view
4. Products are loaded from Products.xlsx
5. Low stock check is performed and alerts are shown

### 2. Product Management Flow
```
Add Product:
1. Navigate to "Add/Edit Product"
2. Fill in product details (Name, Category, Unit Cost, Price, Stock, MinStock)
3. Click "Save Product"
4. Product saved to Products.xlsx
5. Stock log created for new product
6. Product list refreshes

Edit Product:
1. Navigate to "Add/Edit Product"
2. Select product from list
3. Modify details in form
4. Click "Save Product"
5. Product updated in Products.xlsx

Delete Product:
1. Select product from list
2. Click "Delete"
3. Confirm deletion
4. Product removed from Products.xlsx
```

### 3. Sales Transaction Flow
```
1. Navigate to "Sell Product"
2. Select product from dropdown
3. Enter quantity
4. Review transaction summary (shows total price)
5. Click "Complete Sale"
6. Validation: Check if quantity <= available stock
7. If valid:
   - Create Sale record in Sales.xlsx
   - Update product stock in Products.xlsx
   - Create StockLog entry with action "Sale"
   - Show success message
8. Product list refreshes
```

### 4. Stock Management Flows

#### Restock
```
1. Navigate to "Restock"
2. Select product
3. View current stock and low-stock warning (if applicable)
4. Enter quantity to add
5. Preview new stock level
6. Click "Add Stock"
7. Stock updated in Products.xlsx
8. StockLog created with action "Restock"
```

#### Sales Return (Customer returns items)
```
1. Navigate to "Sales Return"
2. Select product
3. Enter return quantity
4. Enter reason for return
5. Click "Process Return"
6. Stock increased in Products.xlsx
7. StockLog created with action "Sales Return" and reason
```

#### Purchase Return (Return to supplier)
```
1. Navigate to "Purchase Return"
2. Select product
3. Enter return quantity
4. Enter reason (e.g., "Defective", "Broken")
5. Validation: Check if quantity <= current stock
6. Click "Return to Supplier"
7. Stock decreased in Products.xlsx
8. StockLog created with action "Purchase Return" and reason
```

#### Product Loss
```
1. Navigate to "Product Loss"
2. Select product
3. Enter loss quantity
4. Enter reason (e.g., "Expired", "Damaged", "Broken")
5. Validation: Check if quantity <= current stock
6. Confirm action (cannot be undone)
7. Stock decreased in Products.xlsx
8. StockLog created with action "Loss" and reason
```

### 5. Advanced Reporting Flow (NEW)
```
1. Navigate to "Reports"
2. Set date range filter (From/To dates)
3. Click "Apply Filter" to update statistics
4. View comprehensive analytics:
   - Total Revenue
   - Total Cost
   - Net Profit
   - Profit Margin %
   - Total Units Sold
5. Switch between chart types:
   - Revenue Chart (top products by revenue)
   - Profit Chart (top products by profit)
   - Units Sold Chart (top products by quantity)
6. Review Product Profit Analysis table:
   - Per-product revenue, cost, profit, and margin
   - Color-coded profit indicators
7. Check Recent Sales (last 10 transactions)
8. Review Recent Stock Activity (last 10 logs)
9. Export entire report to PDF:
   - Click "Export to PDF" button
   - Choose save location
   - Professional PDF generated with all data
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
| Action | string | "Restock", "Sale", "Sales Return", "Purchase Return", "Loss", "New Product" |
| Quantity | int | Quantity affected |
| StockBefore | int | Stock level before action |
| StockAfter | int | Stock level after action |
| Reason | string | Explanation (for returns/losses) |
| Date | DateTime | Action timestamp |

## Key Features & Validations

### Low Stock Alert
- Triggered on application start
- Shows alert when `Stock <= MinStock`
- Lists all low-stock products in a warning dialog

### Product Categories
Predefined categories (editable via ComboBox):
- All Categories (filter only)
- Garments
- Snacks
- Biscuits
- Beverages
- Accessories
- Souvenir Bag
- Stuff Toy

### Search & Filter
- Search by: Product name, Category, or ID
- Filter by: Category
- Real-time filtering as you type
- Shows filtered count vs total count

### Advanced Reporting Features (NEW)
- **Date Range Filtering**: Filter reports by custom date ranges
- **Profit Margin Calculation**: Automatic calculation of profit margins
- **Cost Tracking**: Track unit costs vs selling prices
- **Multiple Chart Types**: Switch between Revenue, Profit, and Units Sold views
- **Product Profit Analysis**: Detailed per-product profitability breakdown
- **PDF Export**: Generate professional PDF reports with all analytics

### Currency Formatting
- Uses Philippine Peso (en-PH culture)
- Format: ₱X,XXX.XX

### Date/Time Format
- Storage: "yyyy-MM-dd hh:mm:ss tt"
- Display: "MM/dd hh:mm tt"

## MVVM Pattern Implementation

### Models
- Plain C# classes with INotifyPropertyChanged
- Represent data entities
- No business logic
- **ProductProfitReport** (NEW): Model for profit analysis data

### Views
- XAML files defining UI
- No code-behind logic (except InitializeComponent)
- Bound to ViewModels via DataContext
- **Enhanced ReportsView**: Advanced analytics with date filters and PDF export

### ViewModels
- Implement INotifyPropertyChanged
- Expose data as properties
- Implement commands for user actions
- Handle business logic and data validation
- Interact with Services for data persistence
- **Enhanced ReportViewModel**: Profit calculations, date filtering, PDF export

### Services
- **ExcelDataService**: Handles all Excel data persistence
- **PdfExportService** (NEW): Generates professional PDF reports
- Separates data access from business logic

### Helpers
- **RelayCommand**: Command pattern implementation
- **ValueConverters**: Standard UI converters
- **ReportConverters** (NEW): Report-specific converters
  - ReportTypeToVisibilityConverter
  - ProfitColorConverter

## PDF Export Features (NEW)

### Report Contents
The PDF export includes:
- **Professional Header**: System name and report title
- **Date Range**: Report period display
- **Summary Statistics**: Color-coded cards with:
  - Total Revenue (Green)
  - Total Cost (Orange)
  - Net Profit (Blue/Red)
  - Profit Margin (Purple)
  - Total Units Sold
- **Product Profit Analysis**: Top 20 products with revenue, cost, profit, and margin
- **Recent Sales**: Last 15 transactions
- **Recent Stock Activity**: Last 15 stock movements
- **Footer**: Generation timestamp

### PDF Formatting
- Color-coded headers (blue background, white text)
- Profit highlighting (green for positive, red for negative)
- Philippine Peso currency formatting
- Professional table layouts with borders
- Proper alignment and spacing
- Auto-generated timestamped filenames

## Common Issues & Solutions

### Issue: Excel file locked
**Solution:** Close any Excel instances that may have the file open

### Issue: Data not refreshing
**Solution:** Call `LoadProducts()` after any data modification

### Issue: Low stock alert too frequent
**Solution:** Adjust `MinStock` values for products

### Issue: Chart not displaying
**Solution:** Ensure sales data exists; chart shows top 10 products

### Issue: PDF export fails
**Solution:** Ensure itext7 package is installed; check file permissions for save location

### Issue: iText7 color errors
**Solution:** Use `new DeviceRgb(r, g, b)` instead of color constants

## Development Best Practices

1. **Always validate user input** before processing
2. **Refresh product list** after any CRUD operation
3. **Create stock logs** for audit trail on all stock changes
4. **Show confirmation** for destructive actions (delete, loss)
5. **Display success messages** to provide user feedback
6. **Handle null/empty selections** in ComboBox selections
7. **Use UpdateSourceTrigger=PropertyChanged** for real-time binding
8. **Follow MVVM pattern** - keep code-behind minimal
9. **Use RelayCommand** for all button/action bindings
10. **Maintain data consistency** - update related tables together
11. **Track unit costs** for accurate profit calculations (NEW)
12. **Use date filtering** for time-based analysis (NEW)
13. **Handle PDF generation errors** gracefully (NEW)

## Profit Calculation Logic (NEW)

### How Profit is Calculated
```csharp
// Per Sale
Revenue = Quantity × Selling Price
Cost = Quantity × Unit Cost
Profit = Revenue - Cost
Profit Margin = (Profit / Revenue) × 100

// Total (for period)
Total Revenue = Sum of all sales revenue
Total Cost = Sum of (quantity × unit cost) for all sales
Total Profit = Total Revenue - Total Cost
Profit Margin = (Total Profit / Total Revenue) × 100
```

### Important Notes
- Unit Cost must be set when adding/editing products
- Profit calculations use the Unit Cost from the product record
- Historical sales use the Unit Cost at time of sale
- Negative profit margins indicate selling below cost

## Future Enhancement Ideas

- ~~Multi-user support with user authentication~~
- ~~Database migration (SQL Server, SQLite)~~
- ~~Barcode scanning integration~~
- ~~Print receipts/invoices~~
- ✅ **Advanced reporting (date ranges, profit margins)** - IMPLEMENTED
- ✅ **Export reports to PDF** - IMPLEMENTED
- Cloud backup integration
- Mobile app companion
- Supplier management
- Multi-location inventory tracking
- Batch import/export of products
- Custom report templates
- Email report delivery
- Scheduled automated reports

## License & Dependencies

- **EPPlus**: NonCommercial license (set in App.xaml.cs)
- **LiveChartsCore**: MIT License
- **itext7**: AGPL License (or Commercial License required for commercial use)
- **.NET**: Microsoft License

## Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or later (recommended)
- Windows 10/11

### Installation Steps
1. Clone or download the project
2. Open `InventoryAndSalesSystem.sln` in Visual Studio
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Build the solution:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```
   or press F5 in Visual Studio

### Required NuGet Packages
```bash
dotnet add package EPPlus --version 7.0.3
dotnet add package LiveChartsCore.SkiaSharpView.WPF --version 2.0.0-rc2
dotnet add package itext7 --version 8.0.2
```

## Support & Contact

For issues or questions, refer to:
- Project documentation
- .NET WPF documentation: https://docs.microsoft.com/wpf
- EPPlus documentation: https://github.com/EPPlusSoftware/EPPlus
- LiveCharts documentation: https://livecharts.dev/
- iText7 documentation: https://itextpdf.com/resources/api-documentation

## Version History

### Version 2.0 (Current)
- ✅ Added advanced reporting with date range filtering
- ✅ Implemented profit margin calculations and cost tracking
- ✅ Added multiple chart types (Revenue, Profit, Units Sold)
- ✅ Integrated PDF export functionality
- ✅ Enhanced product profit analysis
- ✅ Added ReportConverters for better UI handling

### Version 1.0
- Initial release with basic inventory management
- Product CRUD operations
- Sales processing
- Stock management (Restock, Returns, Losses)
- Basic reporting with charts
- Low stock alerts
- Excel data persistence
