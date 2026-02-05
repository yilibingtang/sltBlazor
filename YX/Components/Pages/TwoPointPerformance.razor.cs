using Microsoft.AspNetCore.Components;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    /// <summary>
    /// 两点计算性能页面
    /// 用于通过两个测试点计算电机性能曲线
    /// </summary>
    public partial class TwoPointPerformance : ComponentBase
    {
        #region 依赖注入
        
        /// <summary>
        /// 电机计算服务
        /// </summary>
        [Inject]
        public IMotorCalculator MotorCalculator { get; set; } = default!;
        
        #endregion

        #region 基本信息
        
        /// <summary>
        /// 电机模型
        /// </summary>
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = string.Empty,
            Voltage = 12.2m, // 默认电压12.2V
        };
        
        #endregion

        #region 单位设置
        
        /// <summary>
        /// 输入扭矩单位（默认：Nm）
        /// </summary>
        public TorqueUnit InputTorqueUnit { get; set; } = TorqueUnit.Nm;
        
        /// <summary>
        /// 输入电流单位（默认：mA）
        /// </summary>
        public CurrentUnit InputCurrentUnit { get; set; } = CurrentUnit.mA;
        
        /// <summary>
        /// 输出扭矩单位（默认：g.cm）
        /// </summary>
        public TorqueUnit OutputTorqueUnit { get; set; } = TorqueUnit.gcm;
        
        /// <summary>
        /// 输出电流单位（默认：mA）
        /// </summary>
        public CurrentUnit OutputCurrentUnit { get; set; } = CurrentUnit.mA;
        
        #endregion

        #region 输入数据
        
        /// <summary>
        /// 第一点转速 (rpm)
        /// </summary>
        public decimal InputSpeed1 { get; set; } = 137.73m;
        
        /// <summary>
        /// 第一点扭矩
        /// </summary>
        public decimal InputTorque1 { get; set; } = 0m;
        
        /// <summary>
        /// 第一点电流
        /// </summary>
        public decimal InputCurrent1 { get; set; } = 85m;
        
        /// <summary>
        /// 第二点转速 (rpm)
        /// </summary>
        public decimal InputSpeed2 { get; set; } = 111.36m;
        
        /// <summary>
        /// 第二点扭矩
        /// </summary>
        public decimal InputTorque2 { get; set; } = 400m;
        
        /// <summary>
        /// 第二点电流
        /// </summary>
        public decimal InputCurrent2 { get; set; } = 432.73m;
        
        #endregion

        #region 输出数据
        
        /// <summary>
        /// 第一点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed1 { get; set; } = 0m;
        
        /// <summary>
        /// 第一点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque1 { get; set; } = 0m;
        
        /// <summary>
        /// 第一点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent1 { get; set; } = 0m;
        
        /// <summary>
        /// 第二点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed2 { get; set; } = 0m;
        
        /// <summary>
        /// 第二点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque2 { get; set; } = 0m;
        
        /// <summary>
        /// 第二点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent2 { get; set; } = 0m;
        
        /// <summary>
        /// 中间点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed3 { get; set; } = 0m;
        
        /// <summary>
        /// 中间点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque3 { get; set; } = 0m;
        
        /// <summary>
        /// 中间点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent3 { get; set; } = 0m;
        
        #endregion

        #region 状态管理
        
        /// <summary>
        /// 是否显示计算结果
        /// </summary>
        public bool ShowResults { get; set; } = false;
        
        #endregion

        #region 单位转换方法
        
        /// <summary>
        /// 扭矩单位转换：将输入单位转换为标准单位（Nm）
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="unit">输入单位</param>
        /// <returns>转换为标准单位的值</returns>
        private decimal ConvertToStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value / 1000m,
                TorqueUnit.Kgcm => value * 0.0980665m,
                TorqueUnit.gcm => value * 0.001m * 0.0980665m,
                _ => value
            };
        }
        
        /// <summary>
        /// 扭矩单位转换：将标准单位（Nm）转换为显示单位
        /// </summary>
        /// <param name="value">标准单位值</param>
        /// <param name="unit">目标单位</param>
        /// <returns>转换为目标单位的值</returns>
        private decimal ConvertFromStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value * 1000m,
                TorqueUnit.Kgcm => value / 0.0980665m,
                TorqueUnit.gcm => value / (1000m * 0.0980665m),
                _ => value
            };
        }
        
        /// <summary>
        /// 电流单位转换：将输入单位转换为标准单位（A）
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="unit">输入单位</param>
        /// <returns>转换为标准单位的值</returns>
        private decimal ConvertToStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value / 1000m,
                _ => value
            };
        }
        
        /// <summary>
        /// 电流单位转换：将标准单位（A）转换为显示单位
        /// </summary>
        /// <param name="value">标准单位值</param>
        /// <param name="unit">目标单位</param>
        /// <returns>转换为目标单位的值</returns>
        private decimal ConvertFromStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value * 1000m,
                _ => value
            };
        }
        
        #endregion

        #region 两点计算直线类
        
        /// <summary>
        /// 两点计算直线类
        /// 用于通过两个点计算直线方程
        /// </summary>
        public class Liangdianjisuanzhixian
        {
            private readonly decimal[] _point1;
            private readonly decimal[] _point2;
            
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="point1">第一个点 [x, y]</param>
            /// <param name="point2">第二个点 [x, y]</param>
            public Liangdianjisuanzhixian(decimal[] point1, decimal[] point2)
            {
                _point1 = point1;
                _point2 = point2;
            }
            
            /// <summary>
            /// 计算斜率
            /// </summary>
            /// <returns>直线斜率</returns>
            public decimal CalculateSlope()
            {
                return (_point2[1] - _point1[1]) / (_point2[0] - _point1[0]);
            }
            
            /// <summary>
            /// 根据x值计算y值
            /// </summary>
            /// <param name="x">x值</param>
            /// <returns>对应的y值</returns>
            public decimal CalculateY(decimal x)
            {
                return (x - _point1[0]) * CalculateSlope() + _point1[1];
            }
            
            /// <summary>
            /// 根据y值计算x值
            /// </summary>
            /// <param name="y">y值</param>
            /// <returns>对应的x值</returns>
            public decimal CalculateX(decimal y)
            {
                return (y - _point1[1]) / CalculateSlope() + _point1[0];
            }
        }
        
        #endregion

        #region 计算方法
        
        /// <summary>
        /// 计算入口方法
        /// </summary>
        public async Task CalculateFits()
        {
            // 转换输入数据为标准单位
            decimal convertedSpeed1 = InputSpeed1;
            decimal convertedTorque1 = ConvertToStandardTorque(InputTorque1, InputTorqueUnit);
            decimal convertedCurrent1 = ConvertToStandardCurrent(InputCurrent1, InputCurrentUnit);
            
            decimal convertedSpeed2 = InputSpeed2;
            decimal convertedTorque2 = ConvertToStandardTorque(InputTorque2, InputTorqueUnit);
            decimal convertedCurrent2 = ConvertToStandardCurrent(InputCurrent2, InputCurrentUnit);
            
            // 计算新的转速值
            decimal speed1 = Math.Floor(convertedSpeed1); // 第一行转速的最大整数
            decimal speed2 = Math.Floor(convertedSpeed2); // 第二行转速的最大整数
            decimal speed3 =  2*speed2- speed1; // 等差数列中间值
            
            // 计算扭矩值
            var torqueCalculator = new Liangdianjisuanzhixian(
                new decimal[] { convertedSpeed1, convertedTorque1 },
                new decimal[] { convertedSpeed2, convertedTorque2 }
            );
            decimal torque1 = torqueCalculator.CalculateX(speed1);
            decimal torque2 = torqueCalculator.CalculateX(speed2);
            decimal torque3 = torqueCalculator.CalculateX(speed3);
            
            // 计算电流值
            var currentCalculator = new Liangdianjisuanzhixian(
                new decimal[] { convertedSpeed1, convertedCurrent1 },
                new decimal[] { convertedSpeed2, convertedCurrent2 }
            );
            decimal current1 = currentCalculator.CalculateY(speed1);
            decimal current2 = currentCalculator.CalculateY(speed2);
            decimal current3 = currentCalculator.CalculateY(speed3);
            
            // 转换输出数据为显示单位并赋值
            OutputSpeed1 = speed1;
            OutputTorque1 = ConvertFromStandardTorque(torque1, OutputTorqueUnit);
            OutputCurrent1 = ConvertFromStandardCurrent(current1, OutputCurrentUnit);
            
            OutputSpeed2 = speed2;
            OutputTorque2 = ConvertFromStandardTorque(torque2, OutputTorqueUnit);
            OutputCurrent2 = ConvertFromStandardCurrent(current2, OutputCurrentUnit);
            
            OutputSpeed3 = speed3;
            OutputTorque3 = ConvertFromStandardTorque(torque3, OutputTorqueUnit);
            OutputCurrent3 = ConvertFromStandardCurrent(current3, OutputCurrentUnit);
            
            // 强制重新渲染UI
            StateHasChanged();
            
            // 设置显示结果
            ShowResults = true;
        }
        
        #endregion
    }
}
