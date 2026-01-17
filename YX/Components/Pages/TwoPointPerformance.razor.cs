using CsvHelper;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    public partial class TwoPointPerformance : ComponentBase
    {
        // 注入电机计算服务
        [Inject]
        public IMotorCalculator MotorCalculator { get; set; } = default!;
        
        // 初始化模型
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = string.Empty,
            Voltage = 12.2m, // 默认电压12.2V
            // 默认特性点（后续会被拟合结果覆盖）
            NoLoadPoint = { Speed = 0m, Current = 0m },
            LoadPoint = { Torque = 4.5m, Current = 7.4m, Speed = 43.545m },
            StallPoint = { Torque = 0m, Current = 0m, Speed = 0m }
        };
        
        // 单位设置
        public TorqueUnit InputTorqueUnit { get; set; } = TorqueUnit.Nm;
        public CurrentUnit InputCurrentUnit { get; set; } = CurrentUnit.A;
        public TorqueUnit OutputTorqueUnit { get; set; } = TorqueUnit.Nm;
        public CurrentUnit OutputCurrentUnit { get; set; } = CurrentUnit.A;
        
        // 数据和状态
        public List<MotorDataPoint> DataPoints { get; set; } = new List<MotorDataPoint>
        {
            new MotorDataPoint { Torque = 0m, Current = 85m, Speed = 137.73m },
            new MotorDataPoint { Torque = 400m, Current = 432.73m, Speed = 111.36m },
        };
        public int SelectedIndex { get; set; } = 0;
        public bool ShowResults { get; set; } = false;
        public bool ShowEfficiencyResults { get; set; } = false;
        private MotorFitResult FitResult { get; set; } = new();
        
        // 效率计算结果（理论精准值）
        public MotorDataPoint MaxEfficiencyPoint { get; set; } = new MotorDataPoint();
        public decimal MaxEfficiencyValue { get; set; } = 0;
        public string EfficiencyDerivativeEquation { get; set; } = string.Empty;
        
        // 性能曲线数据列表
        public List<PerformanceCurvePoint> PerformanceCurveData { get; set; } = new List<PerformanceCurvePoint>();
        
        // 性能曲线数据点类
        public class PerformanceCurvePoint
        {
            public decimal Torque { get; set; }
            public decimal Speed { get; set; }
            public decimal Current { get; set; }
            public decimal Efficiency { get; set; }
        }
        
        // 扭矩单位转换：将输入单位转换为标准单位（Nm）
        private decimal ConvertToStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value / 1000m,
                TorqueUnit.Kgcm => value * 0.0980665m,
                TorqueUnit.gcm => value *0.001m*0.0980665m,
                _ => value
            };
        }
        
        // 扭矩单位转换：将标准单位（Nm）转换为显示单位
        private decimal ConvertFromStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value * 1000m,
                TorqueUnit.Kgcm => value / 0.0980665m,
                TorqueUnit.gcm => value /(1000m*0.0980665m) ,
                _ => value
            };
        }
        
        // 电流单位转换：将输入单位转换为标准单位（A）
        private decimal ConvertToStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value / 1000m,
                _ => value
            };
        }
        
        // 电流单位转换：将标准单位（A）转换为显示单位
        private decimal ConvertFromStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value * 1000m,
                _ => value
            };
        }
        
        // 选择行
        public void SelectRow(int index)
        {
            SelectedIndex = index;
        }
        
        // 添加行
        public void AddRow()
        {
            if (SelectedIndex == -1)
            {
                DataPoints.Add(new MotorDataPoint());
                SelectedIndex = DataPoints.Count - 1;
            }
            else
            {
                DataPoints.Insert(SelectedIndex + 1, new MotorDataPoint());
                SelectedIndex++;
            }
        }
        
        // 删除行
        public void DeleteRow()
        {
            if (SelectedIndex >= 0 && DataPoints.Count > 1)
            {
                DataPoints.RemoveAt(SelectedIndex);
                SelectedIndex = Math.Min(SelectedIndex, DataPoints.Count - 1);
            }
        }
        public class Jisuanshujuji
        {
            public MotorDataPoint NoLoad { get; set; } = new MotorDataPoint();
            public MotorDataPoint StallLoad { get; set; }= new MotorDataPoint();
        }

        // 计算入口
        public async Task CalculateFits()
        {

            // 转换输入数据为标准单位（Nm和A）
            var convertedDataPoints = DataPoints.Select(point => new MotorDataPoint
            {
                Torque = ConvertToStandardTorque(point.Torque, InputTorqueUnit),
                Current = ConvertToStandardCurrent(point.Current, InputCurrentUnit),
                Speed = point.Speed // 转速单位保持不变
            }).ToList();

            Jisuanshujuji jisuanshujuji = new Jisuanshujuji();
            decimal k = (convertedDataPoints[1].Speed - convertedDataPoints[0].Speed) / (convertedDataPoints[1].Torque - convertedDataPoints[0].Torque);
            jisuanshujuji.NoLoad.Speed= convertedDataPoints[0].Speed- convertedDataPoints[0].Torque*k;
            jisuanshujuji.StallLoad.Torque = convertedDataPoints[0].Torque - convertedDataPoints[0].Speed / k;
            // 计算拟合结果
            FitResult = MotorCalculator.ComputeFits(convertedDataPoints);

        }

        // 核心：计算理论精准的最大效率点
        private async Task CalculateEfficiencyResults()
        {
            // 从拟合结果中获取基础参数
            decimal n0 = FitResult.NoLoadSpeed;       // 空载转速
            decimal I0 = FitResult.NoLoadCurrent;     // 空载电流
            decimal Tk = FitResult.StallTorque;       // 堵转扭矩
            decimal Ik = FitResult.StallCurrent;      // 堵转电流
            decimal U = Motor.Voltage;                // 电压
            decimal K = PhysicalConstants.MotorEfficiencyConstant; // 9.5493

            // 更新模型中的特性点
            Motor.NoLoadPoint.Speed = n0;
            Motor.NoLoadPoint.Current = I0;
            Motor.StallPoint.Torque = Tk;
            Motor.StallPoint.Current = Ik;
            Motor.StallPoint.Speed = 0m; // 堵转时转速为0

            // 生成性能曲线数据：从空载转速到0，每次减少1
            GeneratePerformanceCurveData(n0, I0, Tk, Ik, U, K);
            
            ShowEfficiencyResults = true;
        }
        
        // 生成性能曲线数据
        private void GeneratePerformanceCurveData(decimal n0, decimal I0, decimal Tk, decimal Ik, decimal U, decimal K)
        {
            PerformanceCurveData.Clear();
            
            // 从空载转速开始，每次减少0.1，直到转速为0
            for (decimal speed = Math.Floor(n0); speed >= 0; speed --)
            {
                decimal currentSpeed = Math.Round(speed, 1);
                
                // 计算扭矩：x = Tk * (1 - n / n0)
                decimal torque = Tk * (1 - currentSpeed / n0);
                
                // 计算电流：I = I0 + (Ik - I0) * x / Tk
                decimal current = I0 + (Ik - I0) * torque / Tk;
                
                // 计算效率：η = (n * x) / (K * U * I)
                decimal efficiency = 0;
                if (current != 0)
                {
                    efficiency = (currentSpeed * torque) / (K * U * current);
                }
                
                // 添加到性能曲线数据列表
                PerformanceCurveData.Add(new PerformanceCurvePoint
                {
                    Torque = torque,
                    Speed = currentSpeed,
                    Current = current,
                    Efficiency = efficiency
                });
            }
        }
        
    }
}
