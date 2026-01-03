using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YX.Models;

namespace YX.Services
{
    public class MotorManager
    {
        readonly MotorDbContext _db;

        public MotorManager(MotorDbContext db) => _db = db;
       
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

        public async Task UpdateMotorAsync(int id, MotorModel editingMotor, List<MotorDataPoint> points)
        {
            var entity = await _db.Motors.FindAsync(id);
            if (entity != null)
            {
                // 更新电机基本信息
                entity.MotorName = editingMotor.MotorName;
                entity.MotorType = editingMotor.MotorType;
                entity.Voltage = editingMotor.Voltage;
                entity.MotorEfficiency = editingMotor.MotorEfficiency;
                entity.MaxEfficiencyLoadRatio = editingMotor.MaxEfficiencyLoadRatio;
                entity.TotalReductionRatio = editingMotor.TotalReductionRatio;
                entity.ReductionStageCount = editingMotor.ReductionStageCount;
                entity.TotalEfficiency = editingMotor.TotalEfficiency;
                
                // 更新测试数据点
                var existing = _db.DataPoints.Where(d => d.MotorId == entity.Id);
                _db.DataPoints.RemoveRange(existing);
                
                if (points?.Count > 0)
                {
                    foreach (var p in points)
                    {
                        _db.DataPoints.Add(new MotorDataPoint { MotorId = entity.Id, Torque = p.Torque, Speed = p.Speed, Current = p.Current, Type = p.Type });
                    }
                }
                
                // 单次保存所有更改
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteMotorAsync(int id)
        {
            try
            {
                var entity = await _db.Motors.FindAsync(id);
                if (entity != null)
                {
                    _db.Motors.Remove(entity);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // 记录错误日志
                // 可以考虑添加重试逻辑或其他错误处理
                throw;
            }
        }
    }
}
