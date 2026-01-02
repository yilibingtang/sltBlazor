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
    double NoLoadRpm { get; set; } = 50;
    
    /// <summary>
    /// 效率扭矩 (g.cm)
    /// </summary>
    double EffTorque { get; set; } = 67513;
    
    /// <summary>
    /// 效率转速 (rpm)，使用 MotorCalculatorHelper 计算
    /// </summary>
    double EffRpm => MotorCalculatorHelper.CalTorqueRpm(NoLoadRpm, EffTorque, StallTorque);
    
    /// <summary>
    /// 堵转扭矩 (g.cm)
    /// </summary>
    double StallTorque { get; set; } = 413516.8;
    
    /// <summary>
    /// 减速比
    /// </summary>
    double GearRatio { get; set; } = 84.2;
    
    /// <summary>
    /// 电压 (V)
    /// </summary>
    double Voltage { get; set; } = 12.2;
    #endregion

    #region 单电机数据
    /// <summary>
    /// 单电机空载转速 (rpm) = 空载转速 * 减速比
    /// </summary>
    double SingleNoLoadRpm => NoLoadRpm * GearRatio;
    
    /// <summary>
    /// 单电机效率扭矩 (g.cm) = 效率扭矩 / (减速比 * 齿轮箱效率)
    /// </summary>
    double SingleEffTorque => EffTorque / (GearRatio * GearboxEfficiency);
    
    /// <summary>
    /// 单电机效率转速 (rpm) = 效率转速 * 减速比
    /// </summary>
    double SingleEffRpm => EffRpm * GearRatio;
    
    /// <summary>
    /// 单电机堵转扭矩 (g.cm) = 堵转扭矩 / (减速比 * 齿轮箱效率)
    /// </summary>
    double SingleStallTorque => StallTorque / (GearRatio * GearboxEfficiency);
    #endregion

    #region 效率计算相关
    /// <summary>
    /// 总效率
    /// </summary>
    double TotalEfficiency => 0.272;
    
    /// <summary>
    /// 假设电机效率
    /// </summary>
    double AssumedMotorEff { get; set; } = 0.665;
    
    /// <summary>
    /// 减速级数
    /// </summary>
    double GearStages { get; set; } = 4;
    
    /// <summary>
    /// 齿轮箱效率 = 总效率 / 假设电机效率
    /// </summary>
    double GearboxEfficiency => AssumedMotorEff == 0 ? 0 : TotalEfficiency / AssumedMotorEff;
    
    /// <summary>
    /// 效率位置 = 效率扭矩 / 堵转扭矩
    /// </summary>
    double EffPosition => StallTorque == 0 ? 0 : EffTorque / StallTorque;
    
    /// <summary>
    /// 效率电流 (A) = 单电机效率扭矩 * 单电机效率转速 * 0.0980665 / (9.5493 * 电压 * 假设电机效率)
    /// </summary>
    double EffCurrent => AssumedMotorEff == 0 ? 0 : SingleEffTorque * SingleEffRpm * 0.0980665 / (9.5493 * Voltage * AssumedMotorEff);
    
    /// <summary>
    /// 空载电流 (A) = 效率电流 * (1 - 根号(假设电机效率))
    /// </summary>
    double NoLoadCurrent => EffCurrent * (1 - Math.Sqrt(AssumedMotorEff));
    
    /// <summary>
    /// 堵转值
    /// </summary>
    double StallValue { get; set; }
    #endregion
}