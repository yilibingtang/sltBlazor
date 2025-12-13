using System.ComponentModel.DataAnnotations;

/// <summary>
/// 电机基础抽象模型
/// </summary>
public enum ReductionMotorType
{
    SingleMotor,
    IntegratedMotor
}
public abstract class BaseMotorModel
{
    /// <summary>
    /// 电机名称
    /// </summary>
    [Required(ErrorMessage = "电机名称不能为空")]
    public string MotorName { get; set; } = string.Empty;

    /// <summary>
    /// 减速电机分类
    /// </summary>
    public ReductionMotorType MotorType { get; set; }

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
public MotorDataPoint NoLoadPoint { get; set; } = new MotorDataPoint();

/// <summary>
/// 堵转特性点
/// </summary>
public MotorDataPoint StallPoint { get; set; } = new MotorDataPoint();
#endregion
}