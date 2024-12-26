using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarManagment.Data;
using CarManagment.Models;
using CarManagment.Dto;
using CarManagment.Filters;

namespace CarManagment.Controllers
{
    [Route("garages")]
    [ApiController]
    public class GaragesController : ControllerBase
    {
        private readonly CarManagmentContext _context;

        public GaragesController(CarManagmentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GaragesDto>>> GetGarages([FromQuery] string? city)
        {
            if (_context.Garages == null)
            {
                return NotFound();
            }

            var query = _context.Garages
              .AsQueryable();

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(m => m.City.ToLower().Contains(city));
            }
            return await query.Select(x => x.AsDto()).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GaragesDto>> GetGarage(long id)
        {
          if (_context.Garages == null)
          {
              return NotFound();
          }
            var garage = await _context.Garages.FindAsync(id);

            if (garage == null)
            {
                return NotFound();
            }

            return garage.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGarage(long id, GaragesDto garagesDto)
        {
            var garage = await _context.Garages
                .Include(g => g.Cars)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (garage == null)
            {
                return NotFound("Garage not found");
            }

            garage.Name = garagesDto.Name;
            garage.Location = garagesDto.Location;
            garage.City = garagesDto.City;
            garage.Capacity = garagesDto.Capacity;

            _context.Entry(garage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GarageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<GaragesDto>> PostGarage(GaragesDto garagesDto)
        {
            Garage garage = ToEntity(garagesDto);

          if (_context.Garages == null)
          {
              return Problem("Entity set 'CarManagmentContext.Garages'  is null.");
          }
            _context.Garages.Add(garage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGarage", new { id = garage.Id }, garage.AsDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGarage(long id)
        {
            if (_context.Garages == null)
            {
                return NotFound();
            }
            var garage = await _context.Garages.FindAsync(id);
            if (garage == null)
            {
                return NotFound("Garage not found");
            }

            _context.Garages.Remove(garage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GarageExists(long id)
        {
            return (_context.Garages?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet("dailyAvailabilityReport")]
        public async Task<ActionResult<IEnumerable<GarageReport>>> GetGarageReport([FromQuery] GarageReportFilter filter)
        {
            if (_context.Garages == null)
            {
                return NotFound("Garage context is null.");
            }

            var garage = await _context.Garages.FirstOrDefaultAsync(g => g.Id == filter.GarageId);

            if (garage == null)
            {
                return NotFound($"Garage with ID {filter.GarageId} not found.");
            }
            var query = _context.Maintenances
                .AsQueryable().Where(x => x.GarageId == filter.GarageId);

            if (filter.StartDate.HasValue)
            {
                query = query.Where(r => r.ScheduledDate >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(r => r.ScheduledDate <= filter.EndDate.Value);
            }

            var queryResult = await query
                .GroupBy(m => new { m.ScheduledDate.Year, m.ScheduledDate.Month })
                .Select(g => new
                {
                    ScheduledDate = new DateTime(g.Key.Year, g.Key.Month, 1),
                    RequestCount = g.Count()
                })
                .ToListAsync();

            var monthlyReport = queryResult
                .Select(g => new GarageReport
                {
                    Date = g.ScheduledDate.ToString("yyyy-MM"),
                    Requests = g.RequestCount,
                    AvailableCapacity = garage.Capacity - g.RequestCount
                })
                .OrderBy(r => r.Date)
                .ToList();

            return Ok(monthlyReport);
        }

        private static Garage ToEntity(GaragesDto garagesDto)
        {
            Garage garage = new()
            {
                Id = garagesDto.Id,
                Name = garagesDto.Name,
                Location = garagesDto.Location,
                City = garagesDto.City,
                Capacity = garagesDto.Capacity,
            };

            return garage;
        }

    }
}
