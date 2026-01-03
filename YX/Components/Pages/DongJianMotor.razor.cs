using Microsoft.AspNetCore.Components;
using YX.model;

namespace YX.Components.Pages
{
    public partial class DongJianMotor : ComponentBase
    {
        // 基本信息
        public string MotorName { get; set; } = "东建电机";
        
        // 拟合结果模型
        private class LineFitResult
        {
            public double A { get; set; } = 0; // 斜率
            public double B { get; set; } = 0; // 截距
        }
        
        // 数据和状态
        public List<MotorDataPoint> DataPoints { get; set; } = new List<MotorDataPoint> { new MotorDataPoint() };
        public int SelectedIndex { get; set; } = 0;
        public bool ShowResults { get; set; } = false;
        private LineFitResult CurrentFit { get; set; } = new();
        private LineFitResult SpeedFit { get; set; } = new();
        
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
            
            // 提取数据
            var torques = DataPoints.Select(p => p.Torque).ToArray();
            var currents = DataPoints.Select(p => p.Current).ToArray();
            var speeds = DataPoints.Select(p => p.Speed).ToArray();
            
            // 拟合电流-扭矩关系
            CurrentFit = LinearFit(torques, currents);
            
            // 拟合转速-扭矩关系
            SpeedFit = LinearFit(torques, speeds);
            
            ShowResults = true;
        }
        
        // 直线拟合算法 (使用MathNet.Numerics库的最小二乘法)
        private static LineFitResult LinearFit(double[] x, double[] y)
        {
            // 使用MathNet.Numerics进行线性回归
            var coeffs = MathNet.Numerics.Fit.Line(x, y);
            
            var result = new LineFitResult();
            result.A = coeffs.Item2; // 斜率
            result.B = coeffs.Item1; // 截距
            
            return result;
        }
    }
}