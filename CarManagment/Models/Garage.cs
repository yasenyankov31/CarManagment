using System.ComponentModel.DataAnnotations;

namespace CarManagment.Models
{
    public class Garage
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public int Capacity { get; set; }
        public ICollection<Car> Cars { get; set; } = new List<Car>();
        public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
    }
}
