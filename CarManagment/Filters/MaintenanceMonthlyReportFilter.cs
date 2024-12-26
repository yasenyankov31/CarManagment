using Microsoft.AspNetCore.Mvc;

namespace CarManagment.Filters
{
    public class MaintenanceMonthlyReportFilter
    {
        public int? GarageId { get; set; }

        public DateTime StartMonth { get; set; }

        public DateTime EndMonth { get; set; }
    }
}
