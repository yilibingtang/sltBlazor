using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CsvHelper;
using YX.Models;

namespace YX.Services
{
    /// <summary>
    /// CSV导出服务实现
    /// </summary>
    public class CsvExportService : ICsvExportService
    {
        /// <summary>
        /// 导出电机数据为CSV字符串
        /// </summary>
        /// <param name="motor">电机模型</param>
        /// <param name="dataPoints">测试数据点</param>
        /// <param name="fitResult">拟合结果</param>
        /// <param name="maxEfficiencyResult">最大效率点结果</param>
        /// <param name="performanceCurve">性能曲线数据</param>
        /// <returns>CSV字符串</returns>
        public string ExportMotorDataToCsv(BaseMotorModel motor, List<MotorDataPoint> dataPoints, MotorFitResult fitResult, MaxEfficiencyResult maxEfficiencyResult, List<PerformanceCurvePoint> performanceCurve)
        {
            // 创建CSV内容
            var csvContent = new StringBuilder();
            using (var writer = new StringWriter(csvContent))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // 写入电机基本信息
                csv.WriteField("电机名称");
                csv.WriteField(motor.MotorName);
                csv.NextRecord();
                
                csv.WriteField("电压 (V)");
                csv.WriteField(motor.Voltage);
                csv.NextRecord();
                
                csv.NextRecord(); // 空行
                
                // 写入测试数据
                csv.WriteField("测试数据 - 扭矩 (Nm)");
                csv.WriteField("测试数据 - 电流 (A)");
                csv.WriteField("测试数据 - 转速 (rpm)");
                csv.NextRecord();
                
                foreach (var point in dataPoints)
                {
                    csv.WriteField(point.Torque);
                    csv.WriteField(point.Current);
                    csv.WriteField(point.Speed);
                    csv.NextRecord();
                }
                
                csv.NextRecord(); // 空行
                
                // 写入拟合结果
                if (fitResult != null && fitResult.Torques != null && fitResult.Torques.Length > 0)
                {
                    csv.WriteField("拟合结果 - 空载转速 (rpm)");
                    csv.WriteField(fitResult.NoLoadSpeed);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 空载电流 (A)");
                    csv.WriteField(fitResult.NoLoadCurrent);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 堵转扭矩 (Nm)");
                    csv.WriteField(fitResult.StallTorque);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 堵转电流 (A)");
                    csv.WriteField(fitResult.StallCurrent);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 电流-扭矩系数 (a1)");
                    csv.WriteField(fitResult.CurrentCoeffs.Length > 1 ? fitResult.CurrentCoeffs[1] : 0);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 电流-扭矩截距 (a0)");
                    csv.WriteField(fitResult.CurrentCoeffs.Length > 0 ? fitResult.CurrentCoeffs[0] : 0);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 转速-扭矩系数 (b1)");
                    csv.WriteField(fitResult.SpeedCoeffs.Length > 1 ? fitResult.SpeedCoeffs[1] : 0);
                    csv.NextRecord();
                    
                    csv.WriteField("拟合结果 - 转速-扭矩截距 (b0)");
                    csv.WriteField(fitResult.SpeedCoeffs.Length > 0 ? fitResult.SpeedCoeffs[0] : 0);
                    csv.NextRecord();
                    
                    csv.NextRecord(); // 空行
                    
                    // 写入最大效率点
                    if (maxEfficiencyResult != null)
                    {
                        csv.WriteField("最大效率点 - 效率");
                        csv.WriteField(maxEfficiencyResult.Efficiency);
                        csv.NextRecord();
                        
                        csv.WriteField("最大效率点 - 扭矩 (Nm)");
                        csv.WriteField(maxEfficiencyResult.Torque);
                        csv.NextRecord();
                        
                        csv.WriteField("最大效率点 - 转速 (rpm)");
                        csv.WriteField(maxEfficiencyResult.Speed);
                        csv.NextRecord();
                        
                        csv.WriteField("最大效率点 - 电流 (A)");
                        csv.WriteField(maxEfficiencyResult.Current);
                        csv.NextRecord();
                        
                        csv.NextRecord(); // 空行
                    }
                }
                
                // 写入性能曲线数据
                csv.WriteField("性能曲线 - 扭矩 (Nm)");
                csv.WriteField("性能曲线 - 转速 (rpm)");
                csv.WriteField("性能曲线 - 电流 (A)");
                csv.WriteField("性能曲线 - 效率");
                csv.NextRecord();
                
                if (performanceCurve != null && performanceCurve.Count > 0)
                {
                    foreach (var point in performanceCurve)
                    {
                        csv.WriteField(point.Torque);
                        csv.WriteField(point.Speed);
                        csv.WriteField(point.Current);
                        csv.WriteField(point.Efficiency);
                        csv.NextRecord();
                    }
                }
            }
            
            return csvContent.ToString();
        }
        
        /// <summary>
        /// 将CSV字符串保存到本地文件
        /// </summary>
        /// <param name="csvContent">CSV内容</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        /// <returns>保存的文件路径</returns>
        public string SaveCsvToFile(string csvContent, string fileName = "电机数据")
        {
            var fullFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fullFileName);
            File.WriteAllText(filePath, csvContent, Encoding.UTF8);
            
            return filePath;
        }
    }
}