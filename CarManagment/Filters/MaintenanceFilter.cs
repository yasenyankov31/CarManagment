namespace CarManagment.Filters
{
    public class MaintenanceFilter
    {
        public long? CarId { get; set; }
        public int? GarageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
