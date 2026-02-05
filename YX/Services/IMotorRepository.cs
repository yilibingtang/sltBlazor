using System.Collections.Generic;
using System.Threading.Tasks;
using YX.Models;

namespace YX.Services
{
    public interface IMotorRepository
    {
        Task<List<MotorModel>> GetAllAsync();
        Task<MotorModel?> GetByIdAsync(int id);
        Task<List<MotorDataPoint>> GetPointsByMotorIdAsync(int motorId);
        Task<int> AddAsync(MotorModel motor, List<MotorDataPoint> points);
        Task UpdateAsync(MotorModel motor, List<MotorDataPoint> points);
        Task DeleteAsync(int id);
    }
}
