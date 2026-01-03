using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YX.Models
{
    /// <summary>
    /// 电机类型枚举
    /// </summary>
    public enum MotorType
    {
        SingleMotor,
        IntegratedMotor
    }
    
    /// <summary>
    /// 电机基础抽象模型
    /// </summary>
    public abstract class BaseMotorModel
{
    /// <summary>
    /// 电机名称
    /// </summary>
    [Required(ErrorMessage = "电机名称不能为空")]
    public string MotorName { get; set; } = string.Empty;

    /// <summary>
    /// 电机类型
    /// </summary>
    public MotorType MotorType { get; set; }

    /// <summary>
    /// 额定电压（V）
    /// </summary>
    [Range(0.1, double.MaxValue, ErrorMessage = "电压必须大于0")]
    public double Voltage { get; set; }

    /// <summary>
    /// 电机效率（%）
    /// </summary>
    [Range(0, 100, ErrorMessage = "效率需在0-100之间")]
    public double MotorEfficiency { get; set; }

    /// <summary>
    /// 最大效率点位置（%额定负载）
    /// </summary>
    [Range(0, 100, ErrorMessage = "最大效率点需在0-100之间")]
    public double MaxEfficiencyLoadRatio { get; set; }

#region 特性参数（使用 MotorDataPoint 封装）
/// <summary>
/// 空载特性点
/// </summary>
[NotMapped]
public MotorDataPoint NoLoadPoint { get; set; } = new MotorDataPoint();
    /// <summary>
    /// 负载特性点
    /// </summary>
    [NotMapped]
    public MotorDataPoint LoadPoint { get; set; } = new MotorDataPoint();
    /// <summary>
    /// 堵转特性点
    /// </summary>
    [NotMapped]
    public MotorDataPoint StallPoint { get; set; } = new MotorDataPoint();
#endregion
    }
}