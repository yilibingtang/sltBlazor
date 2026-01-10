using System.Collections.Generic;
using YX.Models;
using YX.Services;

namespace YX.Services
{
    /// <summary>
    /// CSV导出服务接口
    /// </summary>
    public interface ICsvExportService
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
        string ExportMotorDataToCsv(BaseMotorModel motor, List<MotorDataPoint> dataPoints, MotorFitResult fitResult, MaxEfficiencyResult maxEfficiencyResult, List<PerformanceCurvePoint> performanceCurve);
        
        /// <summary>
        /// 将CSV字符串保存到本地文件
        /// </summary>
        /// <param name="csvContent">CSV内容</param>
        /// <param name="fileName">文件名，不包含扩展名</param>
        /// <returns>保存的文件路径</returns>
        string SaveCsvToFile(string csvContent, string fileName = "电机数据");
    }
}