using Microsoft.AspNetCore.Components;
using YX.Models;

namespace YX.Components.Pages
{
    public partial class MotorPowerCal : ComponentBase
    {
        // 平均数计算器
        public decimal Number1 { get; set; } = 0;
        public decimal Number2 { get; set; } = 0;
        public decimal Average => (Number1 + Number2) / 2;
        
        // 电机性能计算
        public MotorDataPoint MotorOne { get; set; } = new() { Torque = 0m };
        public MotorDataPoint MotorTwo { get; set; } = new() { Torque = 45887.229584m };
        public MotorDataPoint MotorCalOne { get; set; } = new();
        public MotorDataPoint MotorCalTwo { get; set; } = new();
        public MotorDataPoint MotorCalThree { get; set; } = new();
        
        // 计算电机性能
        private void CalculateMotor()
        {
            // 计算三个转速点
            MotorCalOne.Speed = decimal.Floor(MotorOne.Speed);
            MotorCalTwo.Speed = decimal.Floor(MotorTwo.Speed);
            MotorCalThree.Speed = 2 * MotorCalTwo.Speed - MotorCalOne.Speed;
            
            // 复用方法计算每个点
            CalculatePoint(MotorCalOne);
            CalculatePoint(MotorCalTwo);
            CalculatePoint(MotorCalThree);
        }
        
        // 通用计算方法
        private void CalculatePoint(MotorDataPoint point)
        {
            var ratio = (MotorOne.Speed - point.Speed) / (MotorOne.Speed - MotorTwo.Speed);
            point.Torque = ratio * MotorTwo.Torque;
            point.Current = ratio * (MotorTwo.Current - MotorOne.Current) + MotorOne.Current;
        }
    }
}