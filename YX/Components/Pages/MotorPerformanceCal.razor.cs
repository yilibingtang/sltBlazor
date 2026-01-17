using Microsoft.AspNetCore.Components;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    

    public partial class MotorPerformanceCal : ComponentBase
    {
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = "电机性能计算",
            MotorType = MotorType.SingleMotor,
            Voltage = 24m,
            MotorEfficiency = 75m,
            MaxEfficiencyLoadRatio = 90m,
            
            NoLoadPoint = { Speed = 26842m, Current = 67.04885348m },
            LoadPoint = { Torque = 88.24m, Speed = 20400m, Current = 1126.496992m },
            StallPoint = { Torque = 360.98m, Current = 4401.154874m }
        };
        
        #region 计算属性
        /// <summary>
        /// 转速比 (负载转速/空载转速)
        /// </summary>
        public decimal SpeedRatio => Motor.LoadPoint.Speed / Motor.NoLoadPoint.Speed;
        
        /// <summary>
        /// 使用 MotorCalculatorHelper 计算负载转速
        /// </summary>
        public decimal CalculatedLoadSpeed => MotorCalculatorHelper.CalTorqueRpm(
            Motor.NoLoadPoint.Speed, 
            Motor.LoadPoint.Torque, 
            Motor.StallPoint.Torque);
        
        /// <summary>
        /// 堵转扭矩计算：堵转扭矩 = 空载转速 * 负载扭矩 / (空载转速 - 负载转速)
        /// </summary>
        public decimal CalculatedStallTorque
        {
            get
            {
                // 确保分母不为零，避免除零错误
                if (Motor.NoLoadPoint.Speed == 0m || Motor.NoLoadPoint.Speed == Motor.LoadPoint.Speed)
                {
                    return 0;
                }
                
                decimal stallTorque = (Motor.NoLoadPoint.Speed * Motor.LoadPoint.Torque) / 
                                   (Motor.NoLoadPoint.Speed - Motor.LoadPoint.Speed);
                
                // 将计算结果写入到 Motor.StallPoint.Torque
                Motor.StallPoint.Torque = stallTorque;
                
                return stallTorque;
            }
        }
        #endregion
    }
}
