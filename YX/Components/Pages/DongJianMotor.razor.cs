using Microsoft.AspNetCore.Components;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    public partial class DongJianMotor : ComponentBase
    {
        // 使用现有的BaseMotorModel
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = string.Empty,
            Voltage = 12.2, // 默认电压12.2V
            NoLoadPoint = { Speed = 48.5, Current = 1.905 },
            LoadPoint = { Torque = 4.5, Current = 7.4, Speed = 43.545 },
            StallPoint = { Torque = 9, Current = 11.505, Speed = 38.68 }
        };
        
        // 数据和状态
        public List<MotorDataPoint> DataPoints { get; set; } = new List<MotorDataPoint> 
        {
            new MotorDataPoint { Torque = 0, Current = 1.905, Speed = 48.5 },
            new MotorDataPoint { Torque = 4.5, Current = 7.4, Speed = 43.545 },
            new MotorDataPoint { Torque = 9, Current = 11.505, Speed = 38.68 }
        };
        public int SelectedIndex { get; set; } = 0;
        public bool ShowResults { get; set; } = false;
        public bool ShowEfficiencyResults { get; set; } = false;
        private YX.Services.MotorFitResult FitResult { get; set; } = new();
        
        // 效率计算结果
        public MotorDataPoint MaxEfficiencyPoint { get; set; } = new MotorDataPoint();
        public double MaxEfficiencyValue { get; set; } = 0;
        public string EfficiencyDerivativeEquation { get; set; } = string.Empty;
        
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
                // 如果没有选中行，添加到末尾
                DataPoints.Add(new MotorDataPoint());
                SelectedIndex = DataPoints.Count - 1;
            }
            else
            {
                // 在选中行下方添加
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
        
        // 计算直线拟合
        public void CalculateFits()
        {
            if (DataPoints.Count < 2)
            {
                return; // 至少需要两个点才能拟合
            }
            
            // 使用MotorCalculator计算拟合结果
            FitResult = YX.Services.MotorCalculator.ComputeFits(DataPoints);
            
            ShowResults = true;
            
            // 计算效率相关结果
            CalculateEfficiencyResults();
        }
        
        // 计算效率相关结果
        private void CalculateEfficiencyResults()
        {
            // 使用拟合结果更新模型中的空载点和堵转点
            Motor.NoLoadPoint.Speed = FitResult.NoLoadSpeed; // 当扭矩为0时的转速
            Motor.NoLoadPoint.Current = FitResult.NoLoadCurrent; // 当扭矩为0时的电流
            
            // 计算堵转扭矩和堵转电流
            double stallTorque = FitResult.StallTorque;
            double stallCurrent = FitResult.StallCurrent;
            
            Motor.StallPoint.Torque = stallTorque;
            Motor.StallPoint.Current = stallCurrent;
            Motor.StallPoint.Speed = 0; // 堵转时转速为0
            
            // 根据用户提供的正确公式：
            // 1. 转速 = 空载转速 - 空载转速 * 扭矩 / 堵转扭矩
            // 2. 电流 = （堵转电流 - 空载电流）/ 堵转电流 * 扭矩 + 空载电流
            // 3. 效率 = 转速 * 扭矩 / (9.4293 * 电流 * 电压)
            
            // 计算效率对扭矩的导数，找到最大值点
            // 这里使用数值方法寻找最大值
            double maxEff = 0;
            double maxEffTorque = 0;
            double maxEffSpeed = 0;
            double maxEffCurrent = 0;
            
            // 遍历扭矩范围，寻找最大效率
            double torqueStep = stallTorque / 1000.0;
            for (double t = 0; t <= stallTorque; t += torqueStep)
            {
                // 计算当前扭矩下的转速（使用用户提供的公式）
                double speed = Motor.NoLoadPoint.Speed - Motor.NoLoadPoint.Speed * t / stallTorque;
                
                // 计算当前扭矩下的电流（使用用户提供的公式）
                double current = (stallCurrent - Motor.NoLoadPoint.Current) / stallCurrent * t + Motor.NoLoadPoint.Current;
                
                // 计算效率（使用用户提供的公式，K=9.5493）
                double efficiency = 0;
                if (current != 0 && Motor.Voltage != 0 && stallTorque != 0)
                {
                    efficiency = (speed * t) / (PhysicalConstants.MotorEfficiencyConstant * current * Motor.Voltage);
                }
                
                // 更新最大效率
                if (efficiency > maxEff)
                {
                    maxEff = efficiency;
                    maxEffTorque = t;
                    maxEffSpeed = speed;
                    maxEffCurrent = current;
                }
            }
            
            // 将最大效率点保存到模型中
            MaxEfficiencyPoint.Torque = maxEffTorque;
            MaxEfficiencyPoint.Speed = maxEffSpeed;
            MaxEfficiencyPoint.Current = maxEffCurrent;
            MaxEfficiencyValue = maxEff;
            
            // 更新模型中的电机效率（转换为百分比）
            Motor.MotorEfficiency = maxEff * 100;
            
            // 计算效率导数方程（显示用）
            // 根据用户提供的公式重新推导
            double a = Motor.NoLoadPoint.Speed;
            double b = stallTorque;
            double c = stallCurrent - Motor.NoLoadPoint.Current;
            double d = stallCurrent;
            double e = Motor.NoLoadPoint.Current;
            double u = Motor.Voltage;
            
            // 效率导数方程的字符串表示
            // η = ( (a - a*t/b) * t ) / ( K * (c/d*t + e) * u )
            // 导数计算后简化
            string numerator = $"({a:F4} - {a:F4}*t/{b:F4}) * ({c:F4}/{d:F4}*t + {e:F4}) + ({a:F4}/{b:F4}*t) * ({c:F4}/{d:F4}*t + {e:F4}) - ({a:F4} - {a:F4}*t/{b:F4}) * t * ({c:F4}/{d:F4})";
            string denominator = $"{PhysicalConstants.MotorEfficiencyConstant:F4} * {u:F4} * ({c:F4}/{d:F4}*t + {e:F4})^2";
            EfficiencyDerivativeEquation = $"({numerator}) / ({denominator})";
            
            ShowEfficiencyResults = true;
        }
    }
}