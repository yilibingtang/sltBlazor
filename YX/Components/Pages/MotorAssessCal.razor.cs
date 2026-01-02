using Microsoft.AspNetCore.Components;

// 注意：类名必须与Razor文件名一致，且继承ComponentBase
public partial class MotorAssessCal : ComponentBase
{
    #region 1. 客户要求性能（输入参数）
    /// <summary>
    /// 客户要求空载转速 (rpm)
    /// </summary>
    public double CustomerNoLoadRpm { get; set; } = 2700;

    /// <summary>
    /// 客户要求负载扭矩 (g.cm)
    /// </summary>
    public double CustomerLoadTorque { get; set; } = 2700;

    /// <summary>
    /// 客户要求负载转速 (rpm)
    /// </summary>
    public double CustomerLoadRpm { get; set; } = 70;

    /// <summary>
    /// 客户要求堵转扭矩 (g.cm)
    /// </summary>
    public double CustomerStallTorque { get; set; } = 18000;
    #endregion

    #region 2. 减速箱参数（输入参数）
    /// <summary>
    /// 减速比
    /// </summary>
    public double GearRatio { get; set; } = 162;

    /// <summary>
    /// 减速箱效率 (0-1)
    /// </summary>
    public double GearboxEfficiency { get; set; } = 0.614;

    /// <summary>
    /// 电机电压 (V)
    /// </summary>
    public double MotorVoltage { get; set; } = 24;
    #endregion

    #region 3. 电机参数（输入参数）
    /// <summary>
    /// 电机型号
    /// </summary>
    public string MotorModel { get; set; } = "050";

    /// <summary>
    /// 电机类型
    /// </summary>
    public string MotorType { get; set; } = "有刷";

    /// <summary>
    /// 电机空载转速 (rpm)
    /// </summary>
    public double MotorNoLoadRpm { get; set; } = 14500;

    /// <summary>
    /// 电机堵转扭矩 (g.cm)
    /// </summary>
    public double MotorStallTorque { get; set; } = 140.72;

    /// <summary>
    /// 电机效率 (0-1)
    /// </summary>
    public double MotorEfficiency { get; set; } = 0.75;
    #endregion

    #region 4. 计算结果（输出参数）
    /// <summary>
    /// 计算得到的电机负载扭矩 (g.cm)
    /// </summary>
    public double CalculatedMotorLoadTorque { get; set; }

    /// <summary>
    /// 计算得到的电机负载转速 (rpm)
    /// </summary>
    public double CalculatedMotorLoadRpm { get; set; }

    /// <summary>
    /// 效率位置
    /// </summary>
    public double EfficiencyPosition { get; set; }

    /// <summary>
    /// 电机效率电流 (mA)
    /// </summary>
    public double CalculatedMotorEffCurrent { get; set; }

    /// <summary>
    /// 电机空载电流 (mA)
    /// </summary>
    public double CalculatedMotorNoLoadCurrent { get; set; }

    /// <summary>
    /// 电机堵转电流 (A)
    /// </summary>
    public double CalculatedMotorStallCurrent { get; set; }
    #endregion

    #region 5. 减速电机输出性能（输出参数）
    /// <summary>
    /// 输出空载转速 (rpm)
    /// </summary>
    public double OutputNoLoadRpm { get; set; }

    /// <summary>
    /// 输出负载扭矩 (kg.cm)
    /// </summary>
    public double OutputLoadTorque { get; set; }

    /// <summary>
    /// 输出负载转速 (rpm)
    /// </summary>
    public double OutputLoadRpm { get; set; }

    /// <summary>
    /// 输出堵转扭矩 (kg.cm)
    /// </summary>
    public double OutputStallTorque { get; set; }

    /// <summary>
    /// 总效率（电机效率×减速箱效率）
    /// </summary>
    public double TotalEfficiency { get; set; }
    #endregion

    #region 核心计算方法
    /// <summary>
    /// 执行所有计算逻辑（基于Excel公式推导）
    /// </summary>
    public void CalculateAll()
    {
        // 1. 计算电机侧负载扭矩（客户负载扭矩 ÷ 减速比 ÷ 减速箱效率）
        CalculatedMotorLoadTorque = CustomerLoadTorque / (GearRatio * GearboxEfficiency);

        // 2. 计算电机侧负载转速（客户负载转速 × 减速比）
        CalculatedMotorLoadRpm = CustomerLoadRpm * GearRatio;

        // 3. 计算效率位置（电机负载转速 ÷ 电机空载转速）
        EfficiencyPosition = CalculatedMotorLoadRpm / MotorNoLoadRpm;

        // 4. 计算电机效率电流（公式：T×n×0.0980665 ÷ (9.5493×U×η)，单位转换为mA）
        CalculatedMotorEffCurrent = (CalculatedMotorLoadTorque * CalculatedMotorLoadRpm * 0.0980665)
                                  / (9.5493 * MotorVoltage * MotorEfficiency) * 1000;

        // 5. 计算电机空载电流（经验公式：效率电流 × (1 - √效率)）
        CalculatedMotorNoLoadCurrent = CalculatedMotorEffCurrent * (1 - Math.Sqrt(MotorEfficiency));

        // 6. 计算电机堵转电流（功率平衡公式推导）
        CalculatedMotorStallCurrent = (MotorStallTorque * MotorNoLoadRpm * 0.0980665)
                                    / (9549.3 * MotorVoltage * MotorEfficiency);

        // 7. 计算减速电机输出性能
        OutputNoLoadRpm = MotorNoLoadRpm / GearRatio;
        OutputLoadTorque = (CalculatedMotorLoadTorque * MotorEfficiency * GearboxEfficiency) / 1000; // 转换为kg.cm
        OutputLoadRpm = CalculatedMotorLoadRpm / GearRatio;
        OutputStallTorque = (MotorStallTorque * GearboxEfficiency) / 1000;
        TotalEfficiency = MotorEfficiency * GearboxEfficiency;
    }
    #endregion

}