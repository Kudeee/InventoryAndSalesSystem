using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem.Models
{
    public class StockLog
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "Restock" or "Sale"
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public DateTime Date { get; set; }
    }
}
