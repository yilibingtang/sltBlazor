using System.ComponentModel.DataAnnotations; // 必须保留此命名空间

/// <summary>
/// 整机模型（含减速机构属性）
/// </summary>
public class MotorModel : BaseMotorModel
{
    // Primary key for EF Core
    public int Id { get; set; }
    /// <summary>
    /// 总减速比
    /// </summary>
    [RangeAttribute(1, double.MaxValue, ErrorMessage = "总减速比必须大于等于1")]
    public double TotalReductionRatio { get; set; }

    /// <summary>
    /// 减速级数
    /// </summary>
    [RangeAttribute(1, int.MaxValue, ErrorMessage = "级数必须大于等于1")]
    public int ReductionStageCount { get; set; }

    /// <summary>
    /// 总效率（%）
    /// </summary>
    [RangeAttribute(0, 100, ErrorMessage = "总效率需在0-100之间")]
    public double TotalEfficiency { get; set; }
}