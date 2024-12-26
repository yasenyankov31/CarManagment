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
    [Route("cars")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly CarManagmentContext _context;

        public CarsController(CarManagmentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetCars([FromQuery] CarFilter filter)
        {
            if (_context.Cars == null)
            {
                return NotFound();
            }


            var query = _context.Cars
                .Include(m => m.Garages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.CarMake))
            {
                query = query.Where(m => m.Make.ToLower().Contains(filter.CarMake.ToLower()));
            }

            if (filter.GarageId != null)
            {
                query = query.Where(m => m.Garages.Any(g => g.Id == filter.GarageId));
            }

            if (filter.FromYear != null)
            {
                query = query.Where(m => m.ProductionYear >= filter.FromYear);
            }

            if (filter.ToYear != null)
            {
                query = query.Where(m => m.ProductionYear <= filter.ToYear);
            }

            return await query.Select(x => x.AsDto()).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> GetCar(long id)
        {
          if (_context.Cars == null)
          {
              return NotFound();
          }
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            return car.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCar(long id, CarDto carDto)
        {
            var car = await _context.Cars.Include(c => c.Garages).FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound("Car not found");
            }

            car.Make = carDto.Make;
            car.Model = carDto.Model;
            car.ProductionYear = carDto.ProductionYear;
            car.LicensePlate = carDto.LicensePlate;

            var newGarages = await Task.WhenAll(carDto.GarageIds.Select(async garageId => await _context.Garages.FindAsync(garageId)));
            car.Garages = newGarages.Where(g => g != null).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(id))
                {
                    return NotFound("Car not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<CarDto>> PostCar(CarDto carDto)
        {
            if (carDto.GarageIds.Count == 0) {
                return BadRequest("Select at least one garage!");
            }

            Car car = await ToEntityAsync(carDto);
          if (_context.Cars == null)
          {
              return Problem("Entity set 'CarManagmentContext.Cars'  is null.");
          }
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCar", new { id = car.Id }, car.AsDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(long id)
        {
            if (_context.Cars == null)
            {
                return NotFound();
            }
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound("Car not found");
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CarExists(long id)
        {
            return (_context.Cars?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task<Car> ToEntityAsync(CarDto carDto)
        {
            List<Garage> garages = (await Task.WhenAll(carDto.GarageIds.Select(async id => await _context.Garages.FindAsync(id))))
                                  .Where(g => g != null)
                                  .ToList();
            Car car = new()
            {
                Id = carDto.Id,
                Make = carDto.Make,
                Model = carDto.Model,
                ProductionYear = carDto.ProductionYear,
                LicensePlate = carDto.LicensePlate,
                Garages = garages
            };

            return car;
        }

    }
}
