using System;
using System.Linq;
using MathNet.Numerics;
using YX.Models;

namespace YX.Services
{
    /// <summary>
    /// 电机拟合结果类
    /// </summary>
    public class MotorFitResult
    {
        // 拟合系数
        public decimal[] CurrentCoeffs { get; set; } = Array.Empty<decimal>();
        public decimal[] SpeedCoeffs { get; set; } = Array.Empty<decimal>();
        
        // 电机特性参数
        public decimal NoLoadSpeed { get; set; }
        public decimal StallTorque { get; set; }
        public decimal NoLoadCurrent { get; set; }
        public decimal StallCurrent { get; set; }
        
        // 图表范围
        public decimal PlotXMin { get; set; }
        public decimal PlotXMax { get; set; }
        public decimal PlotYMin { get; set; }
        public decimal PlotYMax { get; set; }
        
        // 原始数据
        public decimal[] Torques { get; set; } = Array.Empty<decimal>();
        public decimal[] Speeds { get; set; } = Array.Empty<decimal>();
        public decimal[] Currents { get; set; } = Array.Empty<decimal>();
        
        // 误差分析（电流拟合）
        public decimal CurrentR2 { get; set; }
        public decimal CurrentMSE { get; set; }
        public decimal CurrentRMSE { get; set; }
        public decimal CurrentMAE { get; set; }
        
        // 误差分析（转速拟合）
        public decimal SpeedR2 { get; set; }
        public decimal SpeedMSE { get; set; }
        public decimal SpeedRMSE { get; set; }
        public decimal SpeedMAE { get; set; }
    }

    /// <summary>
    /// 电机计算服务类，整合所有电机相关计算逻辑
    /// </summary>
    public class MotorCalculator : IMotorCalculator
    {
        // Decimal数学运算辅助方法
        private static decimal DecimalPow(decimal x, int power)
        {
            decimal result = 1;
            for (int i = 0; i < power; i++)
            {
                result *= x;
            }
            return result;
        }
        
        private static decimal DecimalAbs(decimal value)
        {
            return value < 0 ? -value : value;
        }
        
        private static decimal DecimalSqrt(decimal value)
        {
            if (value < 0) return 0;
            if (value == 0) return 0;
            
            decimal x = value;
            decimal prevX;
            do
            {
                prevX = x;
                x = (x + value / x) / 2;
            } while (DecimalAbs(x - prevX) > 0.0000000000000000000000000001m);
            
            return x;
        }
        
        private static decimal DecimalMax(decimal a, decimal b)
        {
            return a > b ? a : b;
        }
        
        private static decimal DecimalMin(decimal a, decimal b)
        {
            return a < b ? a : b;
        }
        
        private static decimal DecimalCeiling(decimal value)
        {
            return Math.Ceiling(value);
        }
        
        private static decimal DecimalRound(decimal value, int decimals)
        {
            return Math.Round(value, decimals);
        }
        
        /// <summary>
        /// 计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        public decimal EvalPoly(decimal[] coeffs, decimal x)
        {
            decimal y = 0;
            for (int i = 0; i < coeffs.Length; i++) 
            {
                y += coeffs[i] * DecimalPow(x, i);
            }
            return y;
        }
        
        /// <summary>
        /// 静态方法：计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        public static decimal EvalPolyStatic(decimal[] coeffs, decimal x)
        {
            decimal y = 0;
            for (int i = 0; i < coeffs.Length; i++) 
            {
                y += coeffs[i] * DecimalPow(x, i);
            }
            return y;
        }

        /// <summary>
        /// 线性拟合结果类，包含系数和误差分析
        /// </summary>
        public class LinearFitResult
        {
            public decimal[] Coeffs { get; set; } = Array.Empty<decimal>();
            public decimal R2 { get; set; } // 决定系数
            public decimal MSE { get; set; } // 均方误差
            public decimal RMSE { get; set; } // 均方根误差
            public decimal MAE { get; set; } // 平均绝对误差
        }

        /// <summary>
        /// 手动计算线性拟合系数（y = a0 + a1*x）
        /// </summary>
        /// <param name="x">自变量数组</param>
        /// <param name="y">因变量数组</param>
        /// <returns>拟合结果，包含系数和误差分析</returns>
        private static LinearFitResult CalculateLinearFit(decimal[] x, decimal[] y)
        {
            var result = new LinearFitResult();
            
            if (x.Length != y.Length || x.Length < 2)
            {
                result.Coeffs = new decimal[] { 0, 0 };
                return result;
            }

            decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
                sumY2 += y[i] * y[i];
            }

            decimal denominator = n * sumX2 - sumX * sumX;
            if (DecimalAbs(denominator) < 0.0000000000000000000000000001m)
            {
                result.Coeffs = new decimal[] { 0, 0 };
                return result;
            }

            decimal a1 = (n * sumXY - sumX * sumY) / denominator;
            decimal a0 = (sumY - a1 * sumX) / n;
            
            result.Coeffs = new decimal[] { a0, a1 };
            
            // 计算误差分析指标
            decimal yMean = sumY / n;
            decimal ssTotal = 0; // 总平方和
            decimal ssResidual = 0; // 残差平方和
            decimal sumAbsoluteErrors = 0; // 绝对误差总和
            
            for (int i = 0; i < n; i++)
            {
                decimal yPred = EvalPolyStatic(result.Coeffs, x[i]);
                decimal error = y[i] - yPred;
                
                ssTotal += DecimalPow(y[i] - yMean, 2);
                ssResidual += DecimalPow(error, 2);
                sumAbsoluteErrors += DecimalAbs(error);
            }
            
            // 计算R²值
            if (DecimalAbs(ssTotal) < 0.0000000000000000000000000001m)
            {
                result.R2 = 1.0m; // 所有y值相同，拟合完美
            }
            else
            {
                result.R2 = 1 - (ssResidual / ssTotal);
            }
            
            // 计算均方误差（MSE）
            result.MSE = ssResidual / n;
            
            // 计算均方根误差（RMSE）
            result.RMSE = DecimalSqrt(result.MSE);
            
            // 计算平均绝对误差（MAE）
            result.MAE = sumAbsoluteErrors / n;
            
            return result;
        }

        /// <summary>
        /// 计算电机数据拟合结果
        /// </summary>
        /// <param name="points">电机测试数据点集合</param>
        /// <returns>拟合结果</returns>
        public MotorFitResult ComputeFits(System.Collections.Generic.IEnumerable<MotorDataPoint> points)
        {
            var list = points?.ToList() ?? new System.Collections.Generic.List<MotorDataPoint>();
            var result = new MotorFitResult();
            if (list.Count < 2) return result;

            var torques = list.Select(p => p.Torque).ToArray();
            var currents = list.Select(p => p.Current).ToArray();
            var speeds = list.Select(p => p.Speed).ToArray();

            // 使用优化的线性拟合，包含误差分析
            var currentFit = CalculateLinearFit(torques, currents);
            var speedFit = CalculateLinearFit(torques, speeds);
            
            result.CurrentCoeffs = currentFit.Coeffs;
            result.SpeedCoeffs = speedFit.Coeffs;
            
            // 设置误差分析结果
            result.CurrentR2 = currentFit.R2;
            result.CurrentMSE = currentFit.MSE;
            result.CurrentRMSE = currentFit.RMSE;
            result.CurrentMAE = currentFit.MAE;
            
            result.SpeedR2 = speedFit.R2;
            result.SpeedMSE = speedFit.MSE;
            result.SpeedRMSE = speedFit.RMSE;
            result.SpeedMAE = speedFit.MAE;

            result.NoLoadSpeed = EvalPoly(result.SpeedCoeffs, 0);
            result.NoLoadCurrent = EvalPoly(result.CurrentCoeffs, 0);

            var a0 = result.SpeedCoeffs[0]; // 空载转速
            var a1 = result.SpeedCoeffs[1]; // 转速-扭矩斜率
            if (DecimalAbs(a1) > 0.0000000000000000000000000001m) 
            {
                result.StallTorque = -a0 / a1; // 堵转扭矩
            } 
            else 
            {
                result.StallTorque = decimal.MinValue;
            }
            result.StallCurrent = result.StallTorque == decimal.MinValue ? decimal.MinValue : EvalPoly(result.CurrentCoeffs, result.StallTorque);

            result.PlotXMin = torques.Min(); 
            result.PlotXMax = torques.Max();
            if (DecimalAbs(result.PlotXMax - result.PlotXMin) < 0.000001m) 
            {
                result.PlotXMin -= 1; 
                result.PlotXMax += 1;
            }
            var yVals = speeds.Concat(currents).ToArray();
            result.PlotYMin = yVals.Min(); 
            result.PlotYMax = yVals.Max();
            if (DecimalAbs(result.PlotYMax - result.PlotYMin) < 0.000001m) 
            {
                result.PlotYMin -= 1; 
                result.PlotYMax += 1;
            }

            result.Torques = torques; 
            result.Speeds = speeds; 
            result.Currents = currents;
            return result;
        }
        
        /// <summary>
        /// 静态方法：计算电机数据拟合结果
        /// </summary>
        /// <param name="points">电机测试数据点集合</param>
        /// <returns>拟合结果</returns>
        public static MotorFitResult ComputeFitsStatic(System.Collections.Generic.IEnumerable<MotorDataPoint> points)
        {
            return new MotorCalculator().ComputeFits(points);
        }
        
        /// <summary>
        /// 生成性能曲线数据
        /// </summary>
        /// <param name="fitResult">拟合结果</param>
        /// <param name="voltage">电压</param>
        /// <param name="speedStep">转速步长</param>
        /// <returns>性能曲线数据列表</returns>
        public List<PerformanceCurvePoint> GeneratePerformanceCurve(MotorFitResult fitResult, decimal voltage, decimal speedStep = 0.1m)
        {
            var curveData = new List<PerformanceCurvePoint>();
            
            if (fitResult.StallTorque <= 0 || fitResult.StallTorque == decimal.MinValue)
                return curveData;
            
            // 从空载转速开始，每次减少0.1，直到转速为0
            for (decimal speed = DecimalCeiling(fitResult.NoLoadSpeed); speed >= 0; speed -= speedStep)
            {
                decimal currentSpeed = DecimalRound(speed, 1);
                
                // 计算扭矩：x = Tk * (1 - n / n0)
                decimal torque = fitResult.StallTorque * (1 - currentSpeed / fitResult.NoLoadSpeed);
                
                // 计算电流：I = I0 + (Ik - I0) * x / Tk
                decimal current = fitResult.NoLoadCurrent + (fitResult.StallCurrent - fitResult.NoLoadCurrent) * torque / fitResult.StallTorque;
                
                // 计算效率：η = (n * x) / (K * U * I)
                decimal efficiency = 0;
                if (current != 0)
                {
                    efficiency = (currentSpeed * torque) / (PhysicalConstants.MotorEfficiencyConstant * voltage * current);
                }
                
                // 添加到性能曲线数据列表
                curveData.Add(new PerformanceCurvePoint
                {
                    Torque = (double)torque,
                    Speed = (double)currentSpeed,
                    Current = (double)current,
                    Efficiency = (double)efficiency
                });
            }
            
            return curveData;
        }
        
        /// <summary>
        /// 计算负载转速
        /// </summary>
        /// <param name="noLoadRpm">空载转速</param>
        /// <param name="loadTorque">负载扭矩</param>
        /// <param name="stallTorque">堵转扭矩</param>
        /// <returns>负载转速</returns>
        public decimal CalculateLoadSpeed(decimal noLoadRpm, decimal loadTorque, decimal stallTorque)
        {
            return noLoadRpm - noLoadRpm * loadTorque / stallTorque;
        }
        
        /// <summary>
        /// 计算理论最大效率点
        /// </summary>
        /// <param name="fitResult">拟合结果</param>
        /// <param name="voltage">电压</param>
        /// <returns>最大效率点数据</returns>
        public MaxEfficiencyResult CalculateMaxEfficiency(MotorFitResult fitResult, decimal voltage)
        {
            var result = new MaxEfficiencyResult();
            
            // 从拟合结果中获取基础参数
            decimal n0 = fitResult.NoLoadSpeed;       // 空载转速
            decimal I0 = fitResult.NoLoadCurrent;     // 空载电流
            decimal Tk = fitResult.StallTorque;       // 堵转扭矩
            decimal Ik = fitResult.StallCurrent;      // 堵转电流
            decimal U = voltage;                    // 电压
            decimal K = PhysicalConstants.MotorEfficiencyConstant; // 9.5493
            
            // 核心方程：(Ik−I0)x² + 2I0Tk x − I0Tk² = 0
            decimal a = Ik - I0;
            decimal b = 2 * I0 * Tk;
            decimal c = -I0 * Tk * Tk;
            decimal discriminant = b * b - 4 * a * c;
            decimal maxEffTorque = 0;
            
            if (discriminant >= 0 && a != 0)
            {
                maxEffTorque = (-b + DecimalSqrt(discriminant)) / (2 * a);
                // 确保扭矩在合理范围内
                maxEffTorque = DecimalMax(0, DecimalMin(maxEffTorque, Tk));
            }
            
            // 计算对应转速（精准）
            decimal maxEffSpeed = n0 * (1 - maxEffTorque / Tk);
            
            // 计算对应电流（精准）
            decimal maxEffCurrent = I0 + (Ik - I0) * maxEffTorque / Tk;
            
            // 计算最大效率（精准）
            decimal maxEff = 0;
            if (maxEffCurrent != 0)
            {
                maxEff = (maxEffSpeed * maxEffTorque) / (K * U * maxEffCurrent);
            }
            
            // 导数方程（显示用）
            string derivativeEquation = $"dη/dt = {(double)(n0*Tk/(K*U)):F4} * ({(double)I0:F4}*t² - {(double)(2*I0*Tk):F4}*t + {(double)(I0*Tk*Tk):F4}) / [(Ik-I0)*t + I0*Tk]^2";
            
            return new MaxEfficiencyResult
            {
                Torque = (double)maxEffTorque,
                Speed = (double)maxEffSpeed,
                Current = (double)maxEffCurrent,
                Efficiency = (double)maxEff,
                EfficiencyDerivativeEquation = derivativeEquation
            };
        }
    }
    
    /// <summary>
    /// 电机计算静态工具类，用于直接调用计算方法（不通过依赖注入）
    /// </summary>
    public static class MotorCalculatorHelper
    {
        /// <summary>
        /// 计算负载转速
        /// </summary>
        /// <param name="noLoadRpm">空载转速</param>
        /// <param name="loadTorque">负载扭矩</param>
        /// <param name="stallTorque">堵转扭矩</param>
        /// <returns>负载转速</returns>
        public static decimal CalTorqueRpm(decimal noLoadRpm, decimal loadTorque, decimal stallTorque)
        {
            return noLoadRpm - noLoadRpm * loadTorque / stallTorque;
        }
    }
}
