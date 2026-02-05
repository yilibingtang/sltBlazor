using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YX.Data;
using YX.Models;

namespace YX.Services
{
    public class EfMotorRepository : IMotorRepository
    {
        private readonly MotorDbContext _db;
        public EfMotorRepository(MotorDbContext db) => _db = db;

        public async Task<List<MotorModel>> GetAllAsync()
        {
            return await _db.Motors.AsNoTracking().OrderBy(m => m.Id).ToListAsync();
        }

        public async Task<MotorModel?> GetByIdAsync(int id)
        {
            return await _db.Motors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<MotorDataPoint>> GetPointsByMotorIdAsync(int motorId)
        {
            return await _db.DataPoints.AsNoTracking().Where(d => d.MotorId == motorId).OrderBy(d => d.Id).ToListAsync();
        }

        public async Task<int> AddAsync(MotorModel motor, List<MotorDataPoint> points)
        {
            var added = new MotorModel
            {
                MotorName = string.IsNullOrWhiteSpace(motor.MotorName) ? "未命名" : motor.MotorName,
                MotorType = motor.MotorType,
                Voltage = motor.Voltage,
                MotorEfficiency = motor.MotorEfficiency,
                MaxEfficiencyLoadRatio = motor.MaxEfficiencyLoadRatio,
                TotalReductionRatio = motor.TotalReductionRatio,
                ReductionStageCount = motor.ReductionStageCount,
                TotalEfficiency = motor.TotalEfficiency
            };
            _db.Motors.Add(added);
            
            if (points?.Count > 0)
            {
                foreach (var p in points)
                {
                    _db.DataPoints.Add(new MotorDataPoint { MotorId = added.Id, Torque = p.Torque, Speed = p.Speed, Current = p.Current, Type = p.Type });
                }
            }
            
            await _db.SaveChangesAsync();
            return added.Id;
        }

        public async Task UpdateAsync(MotorModel motor, List<MotorDataPoint> points)
        {
            var entity = await _db.Motors.FindAsync(motor.Id);
            if (entity != null)
            {
                entity.MotorName = motor.MotorName;
                entity.MotorType = motor.MotorType;
                entity.Voltage = motor.Voltage;
                entity.MotorEfficiency = motor.MotorEfficiency;
                entity.MaxEfficiencyLoadRatio = motor.MaxEfficiencyLoadRatio;
                entity.TotalReductionRatio = motor.TotalReductionRatio;
                entity.ReductionStageCount = motor.ReductionStageCount;
                entity.TotalEfficiency = motor.TotalEfficiency;

                var existing = _db.DataPoints.Where(d => d.MotorId == entity.Id);
                _db.DataPoints.RemoveRange(existing);

                if (points?.Count > 0)
                {
                    foreach (var p in points)
                    {
                        _db.DataPoints.Add(new MotorDataPoint { MotorId = entity.Id, Torque = p.Torque, Speed = p.Speed, Current = p.Current, Type = p.Type });
                    }
                }

                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Motors.FindAsync(id);
            if (entity != null)
            {
                _db.Motors.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
