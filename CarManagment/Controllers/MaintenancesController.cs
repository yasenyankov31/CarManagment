using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarManagment.Data;
using CarManagment.Models;
using CarManagment.Dto;
using CarManagment.Filters;

namespace CarManagment.Controllers
{
    [Route("maintenance")]
    [ApiController]
    public class MaintenancesController : ControllerBase
    {
        private readonly CarManagmentContext _context;

        public MaintenancesController(CarManagmentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceDto>>> GetMaintenances([FromQuery] MaintenanceFilter filter)
        {
            if (_context.Maintenances == null)
            {
                return NotFound();
            }
            var query = _context.Maintenances.Include(x=>x.Car).Include(x => x.Garage).AsQueryable();

            if (filter.CarId != null)
            {
                query = query.Where(m => m.CarId >= filter.CarId);
            }
            if (filter.GarageId != null)
            {
                query = query.Where(m => m.GarageId >= filter.GarageId);
            }
            if (filter.StartDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDate >= filter.StartDate);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(m => m.ScheduledDate <= filter.EndDate);
            }

            return await query.Select(x => x.AsDto()).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceDto>> GetMaintenance(long id)
        {
          if (_context.Maintenances == null)
          {
              return NotFound();
          }
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            return maintenance.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMaintenance(long id, MaintenanceDto maintenanceDto)
        {
            var maintenance = await _context.Maintenances
                .Include(m => m.Car)
                .Include(m => m.Garage)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenance == null)
            {
                return NotFound("Maintenance not found");
            }

            maintenance.ServiceType = maintenanceDto.ServiceType;
            maintenance.ScheduledDate = maintenanceDto.ScheduledDate.ToDateTime(TimeOnly.MinValue);

            maintenance.Car = await _context.Cars.FindAsync(maintenanceDto.CarId);
            maintenance.Garage = await _context.Garages.FindAsync(maintenanceDto.GarageId);

            _context.Entry(maintenance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceExists(id))
                {
                    return NotFound("Maintenance not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceDto>> PostMaintenance(MaintenanceDto maintenanceDto)
        {
            Maintenance maintenance = ToEntity(maintenanceDto);
            if (_context.Maintenances == null)
            {
                return Problem("Entity set 'CarManagmentContext.Maintenances'  is null.");
            }
            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaintenance", new { id = maintenance.Id }, maintenance.AsDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenance(long id)
        {
            if (_context.Maintenances == null)
            {
                return NotFound();
            }
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                return NotFound("Maintenance not found");
            }

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaintenanceExists(long id)
        {
            return (_context.Maintenances?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet("monthlyRequestsReport")]
        public async Task<ActionResult<IEnumerable<MonthlyMaintenanceReport>>> GetMonthlyReport(
        [FromQuery] MaintenanceMonthlyReportFilter filter)
        {
            if (_context.Maintenances == null)
            {
                return NotFound();
            }

            if (filter.StartMonth > filter.EndMonth)
            {
                return BadRequest("Start date cannot be later than end date.");
            }

            var monthsInRange = Enumerable.Range(0, ((filter.EndMonth.Year - filter.StartMonth.Year) * 12) + filter.EndMonth.Month - filter.StartMonth.Month + 1)
                .Select(offset => new DateTime(filter.StartMonth.Year, filter.StartMonth.Month, 1).AddMonths(offset))
                .ToList();

            var query = _context.Maintenances.AsQueryable();

            if (filter.GarageId.HasValue)
            {
                query = query.Where(m => m.GarageId == filter.GarageId);
            }

            query = query.Where(m => m.ScheduledDate >= filter.StartMonth && m.ScheduledDate <= filter.EndMonth);

            var groupedData = await query
                .GroupBy(m => new { m.ScheduledDate.Year, m.ScheduledDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var report = monthsInRange.Select(month => new MonthlyMaintenanceReport
            {
                YearMonth = month.Month,
                Requests = groupedData
                    .Where(g => g.Year == month.Year && g.Month == month.Month)
                    .Select(g => g.Count)
                    .FirstOrDefault()
            }).ToList();

            return Ok(report);
        }
        private Maintenance ToEntity(MaintenanceDto maintenanceDto)
        {
            Maintenance maintenance = new()
            {
                Id = maintenanceDto.Id,
                Car= _context.Cars.Find(maintenanceDto.CarId),
                ServiceType = maintenanceDto.ServiceType,
                ScheduledDate = maintenanceDto.ScheduledDate.ToDateTime(TimeOnly.MinValue),
                Garage = _context.Garages.Find(maintenanceDto.GarageId)
            };

            return maintenance;
        }
    }
}