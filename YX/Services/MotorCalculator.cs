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
        public double[] CurrentCoeffs { get; set; } = Array.Empty<double>();
        public double[] SpeedCoeffs { get; set; } = Array.Empty<double>();
        public double NoLoadSpeed { get; set; }
        public double StallTorque { get; set; }
        public double NoLoadCurrent { get; set; }
        public double StallCurrent { get; set; }
        public double PlotXMin { get; set; }
        public double PlotXMax { get; set; }
        public double PlotYMin { get; set; }
        public double PlotYMax { get; set; }
        public double[] Torques { get; set; } = Array.Empty<double>();
        public double[] Speeds { get; set; } = Array.Empty<double>();
        public double[] Currents { get; set; } = Array.Empty<double>();
    }

    /// <summary>
    /// 电机计算服务类，整合所有电机相关计算逻辑
    /// </summary>
    public class MotorCalculator
    {
        /// <summary>
        /// 计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        public static double EvalPoly(double[] coeffs, double x)
        {
            double y = 0;
            for (int i = 0; i < coeffs.Length; i++) 
            {
                y += coeffs[i] * Math.Pow(x, i);
            }
            return y;
        }

        /// <summary>
        /// 计算电机数据拟合结果
        /// </summary>
        /// <param name="points">电机测试数据点集合</param>
        /// <returns>拟合结果</returns>
        public static MotorFitResult ComputeFits(System.Collections.Generic.IEnumerable<MotorDataPoint> points)
        {
            var list = points?.ToList() ?? new System.Collections.Generic.List<MotorDataPoint>();
            var result = new MotorFitResult();
            if (list.Count < 2) return result;

            var torques = list.Select(p => p.Torque).ToArray();
            var currents = list.Select(p => p.Current).ToArray();
            var speeds = list.Select(p => p.Speed).ToArray();

            result.CurrentCoeffs = Fit.Polynomial(torques, currents, 1);
            result.SpeedCoeffs = Fit.Polynomial(torques, speeds, 1);

            result.NoLoadSpeed = EvalPoly(result.SpeedCoeffs, 0);
            result.NoLoadCurrent = EvalPoly(result.CurrentCoeffs, 0);

            var a0 = result.SpeedCoeffs.Length > 0 ? result.SpeedCoeffs[0] : 0;
            var a1 = result.SpeedCoeffs.Length > 1 ? result.SpeedCoeffs[1] : 0;
            if (Math.Abs(a1) > 1e-12) 
            {
                result.StallTorque = -a0 / a1;
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
