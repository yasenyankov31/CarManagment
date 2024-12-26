using CarManagment.Models;

namespace CarManagment.Dto
{
    public static class EntityExtensions
    {
        public static CarDto AsDto(this Car car)
        {
            return new CarDto(
                 car.Id,
                 car.Make,
                 car.Model,
                 car.ProductionYear,
                 car.LicensePlate,
                 car.Garages.Select(g => g.AsDto()).ToList(),
                 car.Garages.Select(g => g.Id).ToList()
            );
        }
        public static GaragesDto AsDto(this Garage garage)
        {
            return new GaragesDto(
                garage.Id,
                garage.Location,
                garage.Name,
                garage.City,
                garage.Capacity
            );
        }
        public static MaintenanceDto AsDto(this Maintenance maintenance)
        {
            return new MaintenanceDto(
                maintenance.Id,
                maintenance.Car.Id,
                maintenance.Car.Model,
                maintenance.ServiceType,
                DateOnly.FromDateTime(maintenance.ScheduledDate),
                maintenance.Garage.Id,
                maintenance.Garage.Name
            );
        }
    }
}