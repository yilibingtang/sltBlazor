using Microsoft.AspNetCore.Components;
using YX.Services;

namespace YX.Components.Pages;

/// <summary>
/// 电机效率计算组件
/// </summary>
public partial class MotorEfficiencyCalculator : ComponentBase
{
    #region 实测数据
    /// <summary>
    /// 空载转速 (rpm)
    /// </summary>
    decimal NoLoadRpm { get; set; } = 50;
    
    /// <summary>
    /// 效率扭矩 (g.cm)
    /// </summary>
    decimal EffTorque { get; set; } = 67513m;
    
    /// <summary>
    /// 效率转速 (rpm)，使用 MotorCalculatorHelper 计算
    /// </summary>
    decimal EffRpm => MotorCalculatorHelper.CalTorqueRpm(NoLoadRpm, EffTorque, StallTorque);
    
    /// <summary>
    /// 堵转扭矩 (g.cm)
    /// </summary>
    decimal StallTorque { get; set; } = 413516.8m;
    
    /// <summary>
    /// 减速比
    /// </summary>
    decimal GearRatio { get; set; } = 84.2m;
    
    /// <summary>
    /// 电压 (V)
    /// </summary>
    decimal Voltage { get; set; } = 12.2m;
    #endregion

    #region 单电机数据
    /// <summary>
    /// 单电机空载转速 (rpm) = 空载转速 * 减速比
    /// </summary>
    decimal SingleNoLoadRpm => NoLoadRpm * GearRatio;
    
    /// <summary>
    /// 单电机效率扭矩 (g.cm) = 效率扭矩 / (减速比 * 齿轮箱效率)
    /// </summary>
    decimal SingleEffTorque => EffTorque / (GearRatio * GearboxEfficiency);
    
    /// <summary>
    /// 单电机效率转速 (rpm) = 效率转速 * 减速比
    /// </summary>
    decimal SingleEffRpm => EffRpm * GearRatio;
    
    /// <summary>
    /// 单电机堵转扭矩 (g.cm) = 堵转扭矩 / (减速比 * 齿轮箱效率)
    /// </summary>
    decimal SingleStallTorque => StallTorque / (GearRatio * GearboxEfficiency);
    #endregion

    #region 效率计算相关
    /// <summary>
    /// 总效率
    /// </summary>
    decimal TotalEfficiency => 0.272m;
    
    /// <summary>
    /// 假设电机效率
    /// </summary>
    decimal AssumedMotorEff { get; set; } = 0.665m;
    
    /// <summary>
    /// 减速级数
    /// </summary>
    decimal GearStages { get; set; } = 4;
    
    /// <summary>
    /// 齿轮箱效率 = 总效率 / 假设电机效率
    /// </summary>
    decimal GearboxEfficiency => AssumedMotorEff == 0 ? 0 : TotalEfficiency / AssumedMotorEff;
    
    /// <summary>
    /// 效率位置 = 效率扭矩 / 堵转扭矩
    /// </summary>
    decimal EffPosition => StallTorque == 0 ? 0 : EffTorque / StallTorque;
    
    /// <summary>
    /// 效率电流 (A) = 单电机效率扭矩 * 单电机效率转速 * 0.0980665 / (9.5493 * 电压 * 假设电机效率)
    /// </summary>
    decimal EffCurrent => AssumedMotorEff == 0 ? 0 : SingleEffTorque * SingleEffRpm * 0.0980665m / (9.5493m * Voltage * AssumedMotorEff);
    
    /// <summary>
    /// 空载电流 (A) = 效率电流 * (1 - 根号(假设电机效率))
    /// </summary>
    decimal NoLoadCurrent => EffCurrent * (1 - (decimal)Math.Sqrt((double)AssumedMotorEff));
    
    /// <summary>
    /// 堵转值
    /// </summary>
    decimal StallValue { get; set; }
    #endregion
}