using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;
using SmartHome.Core.Interfaces;
using SmartHome.Infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Infra.Repositories
{
    public class SensorsRepository(IndustrialDbContext context) : ISensorsRepository
    {

        public async Task<IEnumerable<Sensor>> GetSensors()
        {
            var query = context.Sensors
                .Select(s => new Sensor
                {
                    Id = s.Id,
                    SensorId = s.SensorId
                }
                );
            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task BatchInsertAsync(IEnumerable<Sensor> sensors)
        {
            var records = sensors as Sensor[] ?? [.. sensors];
            if (!records.Any())
            {
                Console.WriteLine("Попытка сохранить пустую пачку данных");
                return;
            }
            // Получаем список SensorId из входящих сенсоров
            var incomingSensorIds = records.Select(s => s.SensorId).ToList();

            // Получаем существующие сенсоры одним запросом
            var existing = await context.Sensors
                .Where(s => incomingSensorIds.Contains(s.SensorId))
                .Select(s => s.SensorId)
                .ToListAsync();

            // Создаем только новые сенсоры
            var newSensors = records
                .Where(sensor => !existing.Contains(sensor.SensorId))
                .ToList();


            await context.Sensors.AddRangeAsync(newSensors);
            await context.SaveChangesAsync();
            
        }
    }
}