using System.ComponentModel.DataAnnotations;

/// <summary>
/// 电机测试数据点（用于持久化）
/// </summary>
public class MotorDataPoint
{
    public int Id { get; set; }
    public int MotorId { get; set; }

    /// <summary>
    /// 扭矩 (N·m)
    /// </summary>
        [Required(ErrorMessage = "扭矩不能为空")]
    [Range(0, double.MaxValue, ErrorMessage = "扭矩需为非负数")]
    public double Torque { get; set; }

    /// <summary>
    /// 转速 (rpm)
    /// </summary>
        [Required(ErrorMessage = "转速不能为空")]
    [Range(0, double.MaxValue, ErrorMessage = "转速需为非负数")]
    public double Speed { get; set; }

    /// <summary>
    /// 电流 (A)
    /// </summary>
        [Required(ErrorMessage = "电流不能为空")]
    [Range(0, double.MaxValue, ErrorMessage = "电流需为非负数")]
    public double Current { get; set; }
}
