using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YX.Models;
using YX.Data;

namespace YX.Services
{
    public class MotorManager : IMotorManager
    {
        readonly IMotorRepository _repo;

        public MotorManager(IMotorRepository repo) => _repo = repo;
       
        public async Task<List<MotorModel>> GetAllMotorsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<(MotorModel?, List<MotorDataPoint>)> GetMotorWithPointsAsync(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            var pts = await _repo.GetPointsByMotorIdAsync(id);
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
            return await _repo.AddAsync(motor, points);
        }

        public async Task UpdateMotorAsync(int id, MotorModel editingMotor, List<MotorDataPoint> points)
        {
            await _repo.UpdateAsync(new MotorModel {
                Id = id,
                MotorName = editingMotor.MotorName,
                MotorType = editingMotor.MotorType,
                Voltage = editingMotor.Voltage,
                MotorEfficiency = editingMotor.MotorEfficiency,
                MaxEfficiencyLoadRatio = editingMotor.MaxEfficiencyLoadRatio,
                TotalReductionRatio = editingMotor.TotalReductionRatio,
                ReductionStageCount = editingMotor.ReductionStageCount,
                TotalEfficiency = editingMotor.TotalEfficiency
            }, points);
        }

        public async Task DeleteMotorAsync(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
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
