using Microsoft.AspNetCore.Components;
using YX.model;
using YX.Services;

namespace YX.Components.Pages
{
    

    public partial class MotorPerformanceCal : ComponentBase
    {
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = "电机性能计算",
            MotorType = ReductionMotorType.SingleMotor,
            Voltage = 24,
            MotorEfficiency = 75,
            MaxEfficiencyLoadRatio = 90,
            
            NoLoadPoint = { Speed = 26842, Current = 67.04885348 },
            LoadPoint = { Torque = 88.24, Speed = 20400, Current = 1126.496992 },
            StallPoint = { Torque = 360.98, Current = 4401.154874 }
        };
        
        #region 计算属性
        /// <summary>
        /// 转速比 (负载转速/空载转速)
        /// </summary>
        public double SpeedRatio => Motor.LoadPoint.Speed / Motor.NoLoadPoint.Speed;
        
        /// <summary>
        /// 使用 MotorCalculatorHelper 计算负载转速
        /// </summary>
        public double CalculatedLoadSpeed => MotorCalculatorHelper.CalTorqueRpm(
            Motor.NoLoadPoint.Speed, 
            Motor.LoadPoint.Torque, 
            Motor.StallPoint.Torque);
        
        /// <summary>
        /// 堵转扭矩计算：堵转扭矩 = 空载转速 * 负载扭矩 / (空载转速 - 负载转速)
        /// </summary>
        public double CalculatedStallTorque
        {
            get
            {
                // 确保分母不为零，避免除零错误
                if (Motor.NoLoadPoint.Speed == 0 || Motor.NoLoadPoint.Speed == Motor.LoadPoint.Speed)
                {
                    return 0;
                }
                
                double stallTorque = (Motor.NoLoadPoint.Speed * Motor.LoadPoint.Torque) / 
                                   (Motor.NoLoadPoint.Speed - Motor.LoadPoint.Speed);
                
                // 将计算结果写入到 Motor.StallPoint.Torque
                Motor.StallPoint.Torque = stallTorque;
                
                return stallTorque;
            }
        }
        #endregion
    }
}
