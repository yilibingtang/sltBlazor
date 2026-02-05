using System.Collections.Generic;
using System.Threading.Tasks;
using YX.Models;

namespace YX.Services
{
    public interface IMotorManager
    {
        Task<List<MotorModel>> GetAllMotorsAsync();
        Task<(MotorModel?, List<MotorDataPoint>)> GetMotorWithPointsAsync(int id);
        Task<int> AddMotorAsync(MotorModel motor, List<MotorDataPoint> points);
        Task UpdateMotorAsync(int id, MotorModel editingMotor, List<MotorDataPoint> points);
        Task DeleteMotorAsync(int id);
    }
}
