using System.Collections.Generic;
using YX.Models;

namespace YX.Services
{
    /// <summary>
    /// 电机计算服务接口，定义所有电机计算相关方法
    /// </summary>
    public interface IMotorCalculator
    {
        /// <summary>
        /// 计算多项式值
        /// </summary>
        /// <param name="coeffs">多项式系数</param>
        /// <param name="x">自变量值</param>
        /// <returns>计算结果</returns>
        double EvalPoly(double[] coeffs, double x);
        
        /// <summary>
        /// 计算电机数据拟合结果
        /// </summary>
        /// <param name="points">电机测试数据点集合</param>
        /// <returns>拟合结果</returns>
        MotorFitResult ComputeFits(IEnumerable<MotorDataPoint> points);
        
        /// <summary>
        /// 生成性能曲线数据
        /// </summary>
        /// <param name="fitResult">拟合结果</param>
        /// <param name="voltage">电压</param>
        /// <param name="speedStep">转速步长</param>
        /// <returns>性能曲线数据列表</returns>
        List<PerformanceCurvePoint> GeneratePerformanceCurve(MotorFitResult fitResult, decimal voltage, double speedStep = 0.1);
        
        /// <summary>
        /// 计算负载转速
        /// </summary>
        /// <param name="noLoadRpm">空载转速</param>
        /// <param name="loadTorque">负载扭矩</param>
        /// <param name="stallTorque">堵转扭矩</param>
        /// <returns>负载转速</returns>
        double CalculateLoadSpeed(double noLoadRpm, double loadTorque, double stallTorque);
        
        /// <summary>
        /// 计算理论最大效率点
        /// </summary>
        /// <param name="fitResult">拟合结果</param>
        /// <param name="voltage">电压</param>
        /// <returns>最大效率点数据</returns>
        MaxEfficiencyResult CalculateMaxEfficiency(MotorFitResult fitResult, decimal voltage);
    }
    
    /// <summary>
    /// 性能曲线数据点
    /// </summary>
    public class PerformanceCurvePoint
    {
        public double Torque { get; set; }
        public double Speed { get; set; }
        public double Current { get; set; }
        public double Efficiency { get; set; }
    }
    
    /// <summary>
    /// 最大效率点计算结果
    /// </summary>
    public class MaxEfficiencyResult
    {
        public double Torque { get; set; }
        public double Speed { get; set; }
        public double Current { get; set; }
        public double Efficiency { get; set; }
        public string EfficiencyDerivativeEquation { get; set; } = string.Empty;
    }
}