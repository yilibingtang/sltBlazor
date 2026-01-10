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
        public double[] CurrentCoeffs { get; set; } = Array.Empty<double>();
        public double[] SpeedCoeffs { get; set; } = Array.Empty<double>();
        
        // 电机特性参数
        public double NoLoadSpeed { get; set; }
        public double StallTorque { get; set; }
        public double NoLoadCurrent { get; set; }
        public double StallCurrent { get; set; }
        
        // 图表范围
        public double PlotXMin { get; set; }
        public double PlotXMax { get; set; }
        public double PlotYMin { get; set; }
        public double PlotYMax { get; set; }
        
        // 原始数据
        public double[] Torques { get; set; } = Array.Empty<double>();
        public double[] Speeds { get; set; } = Array.Empty<double>();
        public double[] Currents { get; set; } = Array.Empty<double>();
        
        // 误差分析（电流拟合）
        public double CurrentR2 { get; set; }
        public double CurrentMSE { get; set; }
        public double CurrentRMSE { get; set; }
        public double CurrentMAE { get; set; }
        
        // 误差分析（转速拟合）
        public double SpeedR2 { get; set; }
        public double SpeedMSE { get; set; }
        public double SpeedRMSE { get; set; }
        public double SpeedMAE { get; set; }
    }

    /// <summary>
    /// 电机计算服务类，整合所有电机相关计算逻辑
    /// </summary>
    public class MotorCalculator : IMotorCalculator
    {
        /// <summary>
        /// 计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        public double EvalPoly(double[] coeffs, double x)
        {
            double y = 0;
            for (int i = 0; i < coeffs.Length; i++) 
            {
                y += coeffs[i] * Math.Pow(x, i);
            }
            return y;
        }
        
        /// <summary>
        /// 静态方法：计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        public static double EvalPolyStatic(double[] coeffs, double x)
        {
            double y = 0;
            for (int i = 0; i < coeffs.Length; i++) 
            {
                y += coeffs[i] * Math.Pow(x, i);
            }
            return y;
        }

        /// <summary>
        /// 线性拟合结果类，包含系数和误差分析
        /// </summary>
        public class LinearFitResult
        {
            public double[] Coeffs { get; set; } = Array.Empty<double>();
            public double R2 { get; set; } // 决定系数
            public double MSE { get; set; } // 均方误差
            public double RMSE { get; set; } // 均方根误差
            public double MAE { get; set; } // 平均绝对误差
        }

        /// <summary>
        /// 手动计算线性拟合系数（y = a0 + a1*x）
        /// </summary>
        /// <param name="x">自变量数组</param>
        /// <param name="y">因变量数组</param>
        /// <returns>拟合结果，包含系数和误差分析</returns>
        private static LinearFitResult CalculateLinearFit(double[] x, double[] y)
        {
            var result = new LinearFitResult();
            
            if (x.Length != y.Length || x.Length < 2)
            {
                result.Coeffs = new double[] { 0, 0 };
                return result;
            }

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
                sumY2 += y[i] * y[i];
            }

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-12)
            {
                result.Coeffs = new double[] { 0, 0 };
                return result;
            }

            double a1 = (n * sumXY - sumX * sumY) / denominator;
            double a0 = (sumY - a1 * sumX) / n;
            
            result.Coeffs = new double[] { a0, a1 };
            
            // 计算误差分析指标
            double yMean = sumY / n;
            double ssTotal = 0; // 总平方和
            double ssResidual = 0; // 残差平方和
            double sumAbsoluteErrors = 0; // 绝对误差总和
            
            for (int i = 0; i < n; i++)
            {
                double yPred = EvalPolyStatic(result.Coeffs, x[i]);
                double error = y[i] - yPred;
                
                ssTotal += Math.Pow(y[i] - yMean, 2);
                ssResidual += Math.Pow(error, 2);
                sumAbsoluteErrors += Math.Abs(error);
            }
            
            // 计算R²值
            if (Math.Abs(ssTotal) < 1e-12)
            {
                result.R2 = 1.0; // 所有y值相同，拟合完美
            }
            else
            {
                result.R2 = 1 - (ssResidual / ssTotal);
            }
            
            // 计算均方误差（MSE）
            result.MSE = ssResidual / n;
            
            // 计算均方根误差（RMSE）
            result.RMSE = Math.Sqrt(result.MSE);
            
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

            var torques = list.Select(p => (double)p.Torque).ToArray();
            var currents = list.Select(p => (double)p.Current).ToArray();
            var speeds = list.Select(p => (double)p.Speed).ToArray();

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
            if (Math.Abs(a1) > 1e-12) 
            {
                result.StallTorque = -a0 / a1; // 堵转扭矩
            } 
            else 
            {
                result.StallTorque = double.NaN;
            }
            result.StallCurrent = double.IsNaN(result.StallTorque) ? double.NaN : EvalPoly(result.CurrentCoeffs, result.StallTorque);

            result.PlotXMin = torques.Min(); 
            result.PlotXMax = torques.Max();
            if (Math.Abs(result.PlotXMax - result.PlotXMin) < 1e-6) 
            {
                result.PlotXMin -= 1; 
                result.PlotXMax += 1;
            }
            var yVals = speeds.Concat(currents).ToArray();
            result.PlotYMin = yVals.Min(); 
            result.PlotYMax = yVals.Max();
            if (Math.Abs(result.PlotYMax - result.PlotYMin) < 1e-6) 
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
        public List<PerformanceCurvePoint> GeneratePerformanceCurve(MotorFitResult fitResult, decimal voltage, double speedStep = 0.1)
        {
            var curveData = new List<PerformanceCurvePoint>();
            
            if (fitResult.StallTorque <= 0 || double.IsNaN(fitResult.StallTorque))
                return curveData;
            
            // 从空载转速开始，每次减少0.1，直到转速为0
            for (double speed = Math.Ceiling(fitResult.NoLoadSpeed); speed >= 0; speed -= speedStep)
            {
                double currentSpeed = Math.Round(speed, 1);
                
                // 计算扭矩：x = Tk * (1 - n / n0)
                double torque = fitResult.StallTorque * (1 - currentSpeed / fitResult.NoLoadSpeed);
                
                // 计算电流：I = I0 + (Ik - I0) * x / Tk
                double current = fitResult.NoLoadCurrent + (fitResult.StallCurrent - fitResult.NoLoadCurrent) * torque / fitResult.StallTorque;
                
                // 计算效率：η = (n * x) / (K * U * I)
                double efficiency = 0;
                if (current != 0)
                {
                    efficiency = (currentSpeed * torque) / (PhysicalConstants.MotorEfficiencyConstant * (double)voltage * current);
                }
                
                // 添加到性能曲线数据列表
                curveData.Add(new PerformanceCurvePoint
                {
                    Torque = torque,
                    Speed = currentSpeed,
                    Current = current,
                    Efficiency = efficiency
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
        public double CalculateLoadSpeed(double noLoadRpm, double loadTorque, double stallTorque)
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
            double n0 = fitResult.NoLoadSpeed;       // 空载转速
            double I0 = fitResult.NoLoadCurrent;     // 空载电流
            double Tk = fitResult.StallTorque;       // 堵转扭矩
            double Ik = fitResult.StallCurrent;      // 堵转电流
            double U = (double)voltage;              // 电压，转换为double用于计算
            double K = PhysicalConstants.MotorEfficiencyConstant; // 9.5493
            
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
            
            // 计算对应转速（精准）
            double maxEffSpeed = n0 * (1 - maxEffTorque / Tk);
            
            // 计算对应电流（精准）
            double maxEffCurrent = I0 + (Ik - I0) * maxEffTorque / Tk;
            
            // 计算最大效率（精准）
            double maxEff = 0;
            if (maxEffCurrent != 0)
            {
                maxEff = (maxEffSpeed * maxEffTorque) / (K * U * maxEffCurrent);
            }
            
            // 导数方程（显示用）
            string derivativeEquation = $"dη/dt = {n0*Tk/(K*U):F4} * ({I0:F4}*t² - {2*I0*Tk:F4}*t + {I0*Tk*Tk:F4}) / [(Ik-I0)*t + I0*Tk]^2";
            
            return new MaxEfficiencyResult
            {
                Torque = maxEffTorque,
                Speed = maxEffSpeed,
                Current = maxEffCurrent,
                Efficiency = maxEff,
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
        public static double CalTorqueRpm(double noLoadRpm, double loadTorque, double stallTorque)
        {
            return noLoadRpm - noLoadRpm * loadTorque / stallTorque;
        }
    }
}
