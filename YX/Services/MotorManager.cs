using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YX.Services
{
    public class MotorManager
    {
        readonly MotorDbContext _db;

        public MotorManager(MotorDbContext db)
        {
            _db = db;
        }

        public async Task<List<MotorModel>> GetAllMotorsAsync()
        {
            return await _db.Motors.AsNoTracking().OrderBy(m => m.Id).ToListAsync();
        }

        public async Task<(MotorModel?, List<MotorDataPoint>)> GetMotorWithPointsAsync(int id)
        {
            var m = await _db.Motors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            var pts = await _db.DataPoints.AsNoTracking().Where(d => d.MotorId == id).OrderBy(d => d.Id).ToListAsync();
            return (m, pts);
        }

        public async Task<int> AddMotorAsync(MotorModel motor, List<MotorDataPoint> points)
        {
            var added = new MotorModel
            {
                MotorName = string.IsNullOrWhiteSpace(motor.MotorName) ? "未命名" : motor.MotorName,
                MotorType = motor.MotorType,
                Voltage = motor.Voltage
            };
            _db.Motors.Add(added);
            await _db.SaveChangesAsync();
            if (points?.Count > 0)
            {
                foreach (var p in points)
                {
                    _db.DataPoints.Add(new MotorDataPoint { MotorId = added.Id, Torque = p.Torque, Speed = p.Speed, Current = p.Current });
                }
                await _db.SaveChangesAsync();
            }
            return added.Id;
        }

        public async Task UpdateMotorAsync(int id, MotorModel editingMotor, List<MotorDataPoint> points)
        {
            var entity = await _db.Motors.FindAsync(id);
            if (entity != null)
            {
                entity.MotorName = editingMotor.MotorName;
                entity.MotorType = editingMotor.MotorType;
                entity.Voltage = editingMotor.Voltage;
                await _db.SaveChangesAsync();

                var existing = _db.DataPoints.Where(d => d.MotorId == entity.Id);
                _db.DataPoints.RemoveRange(existing);
                await _db.SaveChangesAsync();
                if (points?.Count > 0)
                {
                    foreach (var p in points)
                    {
                        _db.DataPoints.Add(new MotorDataPoint { MotorId = entity.Id, Torque = p.Torque, Speed = p.Speed, Current = p.Current });
                    }
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteMotorAsync(int id)
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
