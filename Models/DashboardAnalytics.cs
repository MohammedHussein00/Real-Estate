namespace Real_State.Models
{
    public class DashboardAnalytics
    {
        // Basic counts
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalProperties { get; set; }
        public int PropertiesSold { get; set; }
        public int PropertiesUnsold { get; set; }
        public decimal TotalRevenue { get; set; }

        public decimal RevenueToday { get; set; }
        public decimal TotalSales { get; set; }
        public decimal SalesToday { get; set; }

      
    }
}
