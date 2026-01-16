using CsvHelper;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    public partial class DongJianMotor : ComponentBase
    {
        // 注入电机计算服务
        [Inject]
        public IMotorCalculator MotorCalculator { get; set; } = default!;
        
        // 注入CSV导出服务
        [Inject]
        public ICsvExportService CsvExportService { get; set; } = default!;
        
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
        private YX.Services.MotorFitResult FitResult { get; set; } = new();
        
        // 效率计算结果（理论精准值）
        public MotorDataPoint MaxEfficiencyPoint { get; set; } = new MotorDataPoint();
        public double MaxEfficiencyValue { get; set; } = 0;
        public string EfficiencyDerivativeEquation { get; set; } = string.Empty;
        
        // 性能曲线数据列表
        public List<PerformanceCurvePoint> PerformanceCurveData { get; set; } = new List<PerformanceCurvePoint>();
        
        // 性能曲线数据点类
        public class PerformanceCurvePoint
        {
            public double Torque { get; set; }
            public double Speed { get; set; }
            public double Current { get; set; }
            public double Efficiency { get; set; }
        }
        
        // 扭矩单位转换：将输入单位转换为标准单位（Nm）
        private decimal ConvertToStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value / 1000m,
                TorqueUnit.Kgcm => value * 0.0980665m,
                TorqueUnit.gcm => value * 0.0000980665m,
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
                TorqueUnit.gcm => value / 0.0000980665m,
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
            
            // 1. 先计算拟合结果
            FitResult = MotorCalculator.ComputeFits(convertedDataPoints);
            
            // 2. 再计算效率结果
            await CalculateEfficiencyResults();
            ShowResults = true;
        }
        
        // 核心：计算理论精准的最大效率点
        private async Task CalculateEfficiencyResults()
        {
            // 从拟合结果中获取基础参数
            double n0 = FitResult.NoLoadSpeed;       // 空载转速
            double I0 = FitResult.NoLoadCurrent;     // 空载电流
            double Tk = FitResult.StallTorque;       // 堵转扭矩
            double Ik = FitResult.StallCurrent;      // 堵转电流
            double U = (double)Motor.Voltage;        // 电压，转换为double用于计算
            double K = PhysicalConstants.MotorEfficiencyConstant; // 9.5493

            // 更新模型中的特性点
            Motor.NoLoadPoint.Speed = (decimal)n0;
            Motor.NoLoadPoint.Current = (decimal)I0;
            Motor.StallPoint.Torque = (decimal)Tk;
            Motor.StallPoint.Current = (decimal)Ik;
            Motor.StallPoint.Speed = 0m; // 堵转时转速为0

            // 调试输出
            System.Console.WriteLine($"=== 拟合参数 ===");
            System.Console.WriteLine($"n0 = {n0}");
            System.Console.WriteLine($"I0 = {I0}");
            System.Console.WriteLine($"Tk = {Tk}");
            System.Console.WriteLine($"Ik = {Ik}");
            System.Console.WriteLine($"U = {U}");
            System.Console.WriteLine($"K = {K}");
            System.Console.WriteLine($"==============");

            // ========== 理论精准公式计算最大效率扭矩 ==========
            // 核心方程：(Ik−I0)x² + 2I0Tk x − I0Tk² = 0
            double a = Ik - I0;
            double b = 2 * I0 * Tk;
            double c = -I0 * Tk * Tk;
            double discriminant = b * b - 4 * a * c;
            double maxEffTorque = 0;
            if (discriminant >= 0 && a != 0)
            {
                maxEffTorque = (-b + Math.Sqrt(discriminant)) / (2 * a);
                // 确保扭矩在合理范围内
                maxEffTorque = Math.Max(0, Math.Min(maxEffTorque, Tk));
            }

            // ========== 计算对应转速（精准） ==========
            // 转速公式：n = n0 * (1 - x / Tk)
            double maxEffSpeed = n0 * (1 - maxEffTorque / Tk);

            // ========== 计算对应电流（精准） ==========
            // 电流公式：I = I0 + (Ik - I0) * x / Tk
            double maxEffCurrent = I0 + (Ik - I0) * maxEffTorque / Tk;

            // ========== 计算最大效率（精准） ==========
            // 效率公式：η = (n * x) / (K * U * I)
            double maxEff = 0;
            if (maxEffCurrent != 0)
            {
                maxEff = (maxEffSpeed * maxEffTorque) / (K * U * maxEffCurrent);
            }
            
            // ========== 验证公式 ==========
            // 验证：空载电流 = 最大效率点电流 × (1 - √(最大效率点效率))
            double verification = maxEffCurrent * (1 - Math.Sqrt(maxEff));
            System.Console.WriteLine($"=== 验证结果 ===");
            System.Console.WriteLine($"空载电流 = {I0}");
            System.Console.WriteLine($"验证值 = {verification}");
            System.Console.WriteLine($"误差 = {Math.Abs(I0 - verification)}");
            string verificationResult = Math.Abs(I0 - verification) < 0.01 ? "通过" : "失败";
            System.Console.WriteLine($"验证 {verificationResult}");
            System.Console.WriteLine($"==============");

            // 调试输出
            System.Console.WriteLine($"=== 计算结果 ===");
            System.Console.WriteLine($"maxEffTorque = {maxEffTorque}");
            System.Console.WriteLine($"maxEffSpeed = {maxEffSpeed}");
            System.Console.WriteLine($"maxEffCurrent = {maxEffCurrent}");
            System.Console.WriteLine($"maxEff = {maxEff}");
            System.Console.WriteLine($"==============");

            // ========== 结果保留8位小数 ==========
            MaxEfficiencyPoint.Torque = (decimal)Math.Round(maxEffTorque, 8);
            MaxEfficiencyPoint.Speed = (decimal)Math.Round(maxEffSpeed, 8);
            MaxEfficiencyPoint.Current = (decimal)Math.Round(maxEffCurrent, 8);
            MaxEfficiencyValue = Math.Round(maxEff, 8);

            // 更新模型效率（百分比）
            Motor.MotorEfficiency = (decimal)(MaxEfficiencyValue * 100);

            // 导数方程（显示用）
            EfficiencyDerivativeEquation = $"dη/dt = {n0*Tk/(K*U):F4} * ({I0:F4}*t² - {2*I0*Tk:F4}*t + {I0*Tk*Tk:F4}) / [(Ik-I0)*t + I0*Tk]^2";
            
            // 生成性能曲线数据：从空载转速到0，每次减少1
            GeneratePerformanceCurveData(n0, I0, Tk, Ik, U, K);
            
            ShowEfficiencyResults = true;
        }
        
        // 生成性能曲线数据
        private void GeneratePerformanceCurveData(double n0, double I0, double Tk, double Ik, double U, double K)
        {
            PerformanceCurveData.Clear();
            
            // 从空载转速开始，每次减少0.1，直到转速为0
            for (decimal speed = (decimal)Math.Floor(n0); speed >= 0; speed --)
            {
                double currentSpeed = Math.Round(speed, 1);
                
                // 计算扭矩：x = Tk * (1 - n / n0)
                double torque = Tk * (1 - currentSpeed / n0);
                
                // 计算电流：I = I0 + (Ik - I0) * x / Tk
                double current = I0 + (Ik - I0) * torque / Tk;
                
                // 计算效率：η = (n * x) / (K * U * I)
                double efficiency = 0;
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
        
        // 导出数据为CSV格式
        public void ExportData()
        {
            try
            {
                // 创建CSV内容
                var csvContent = new StringBuilder();
                using (var writer = new StringWriter(csvContent))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // 写入电机基本信息
                    csv.WriteField("电机名称");
                    csv.WriteField(Motor.MotorName);
                    csv.NextRecord();
                    
                    csv.WriteField("电压 (V)");
                    csv.WriteField(Motor.Voltage);
                    csv.NextRecord();
                    
                    csv.NextRecord(); // 空行
                    
                    // 写入测试数据
                    csv.WriteField("测试数据 - 扭矩 (Nm)");
                    csv.WriteField("测试数据 - 电流 (A)");
                    csv.WriteField("测试数据 - 转速 (rpm)");
                    csv.NextRecord();
                    
                    foreach (var point in DataPoints)
                    {
                        csv.WriteField(point.Torque);
                        csv.WriteField(point.Current);
                        csv.WriteField(point.Speed);
                        csv.NextRecord();
                    }
                    
                    csv.NextRecord(); // 空行
                    
                    if (ShowResults)
                    {
                        // 写入拟合结果
                        csv.WriteField("拟合结果 - 空载转速 (rpm)");
                        csv.WriteField(FitResult.NoLoadSpeed);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 空载电流 (A)");
                        csv.WriteField(FitResult.NoLoadCurrent);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 堵转扭矩 (Nm)");
                        csv.WriteField(FitResult.StallTorque);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 堵转电流 (A)");
                        csv.WriteField(FitResult.StallCurrent);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 电流-扭矩系数 (a1)");
                        csv.WriteField(FitResult.CurrentCoeffs.Length > 1 ? FitResult.CurrentCoeffs[1] : 0);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 电流-扭矩截距 (a0)");
                        csv.WriteField(FitResult.CurrentCoeffs.Length > 0 ? FitResult.CurrentCoeffs[0] : 0);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 转速-扭矩系数 (b1)");
                        csv.WriteField(FitResult.SpeedCoeffs.Length > 1 ? FitResult.SpeedCoeffs[1] : 0);
                        csv.NextRecord();
                        
                        csv.WriteField("拟合结果 - 转速-扭矩截距 (b0)");
                        csv.WriteField(FitResult.SpeedCoeffs.Length > 0 ? FitResult.SpeedCoeffs[0] : 0);
                        csv.NextRecord();
                        
                        csv.NextRecord(); // 空行
                        
                        // 写入最大效率点
                        if (ShowEfficiencyResults)
                        {
                            csv.WriteField("最大效率点 - 效率");
                            csv.WriteField(MaxEfficiencyValue);
                            csv.NextRecord();
                            
                            csv.WriteField("最大效率点 - 扭矩 (Nm)");
                            csv.WriteField(MaxEfficiencyPoint.Torque);
                            csv.NextRecord();
                            
                            csv.WriteField("最大效率点 - 转速 (rpm)");
                            csv.WriteField(MaxEfficiencyPoint.Speed);
                            csv.NextRecord();
                            
                            csv.WriteField("最大效率点 - 电流 (A)");
                            csv.WriteField(MaxEfficiencyPoint.Current);
                            csv.NextRecord();
                            
                            csv.NextRecord(); // 空行
                        }
                    }
                    
                    // 写入性能曲线数据（从0到堵转扭矩）
                    csv.WriteField("性能曲线 - 扭矩 (Nm)");
                    csv.WriteField("性能曲线 - 转速 (rpm)");
                    csv.WriteField("性能曲线 - 电流 (A)");
                    csv.WriteField("性能曲线 - 效率");
                    csv.NextRecord();
                    
                    if (FitResult.StallTorque > 0)
                    {
                        int pointsCount = 50;
                        for (int i = 0; i <= pointsCount; i++)
                        {
                            double torque = i * FitResult.StallTorque / pointsCount;
                            
                            // 计算转速：n = n0 * (1 - torque / Tk)
                            double speed = FitResult.NoLoadSpeed * (1 - torque / FitResult.StallTorque);
                            
                            // 计算电流：I = I0 + (Ik - I0) * torque / Tk
                            double current = FitResult.NoLoadCurrent + (FitResult.StallCurrent - FitResult.NoLoadCurrent) * torque / FitResult.StallTorque;
                            
                            // 计算效率：η = (speed * torque) / (K * U * current)
                            double K = PhysicalConstants.MotorEfficiencyConstant; // 9.5493
                            double efficiency = 0;
                            if (current != 0)
                            {
                                efficiency = (speed * torque) / (K * (double)Motor.Voltage * current);
                            }
                            
                            csv.WriteField(torque);
                            csv.WriteField(speed);
                            csv.WriteField(current);
                            csv.WriteField(efficiency);
                            csv.NextRecord();
                        }
                    }
                }
                
                // 保存CSV文件到本地桌面
                var fileName = $"电机数据_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
                
                // 输出保存路径到控制台
                System.Console.WriteLine($"CSV文件已保存到：{filePath}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"导出数据时出错：{ex.Message}");
            }
        }
    }
}