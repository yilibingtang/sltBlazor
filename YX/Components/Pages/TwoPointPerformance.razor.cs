using Microsoft.AspNetCore.Components;
using YX.Models;
using YX.Services;

namespace YX.Components.Pages
{
    /// <summary>
    /// 两点计算性能页面
    /// 用于通过两个测试点计算电机性能曲线
    /// </summary>
    public partial class TwoPointPerformance : ComponentBase
    {
        #region 依赖注入

        /// <summary>
        /// 两点计算服务（已抽离逻辑）
        /// </summary>
        [Inject]
        public ITwoPointCalculator TwoPointCalculator { get; set; } = default!;

        #endregion

        #region 基本信息

        /// <summary>
        /// 电机模型
        /// </summary>
        public BaseMotorModel Motor { get; set; } = new MotorModel
        {
            MotorName = string.Empty,
            Voltage = 12.2m, // 默认电压12.2V
        };

        #endregion

        #region 单位设置

        /// <summary>
        /// 输入扭矩单位（默认：Nm）
        /// </summary>
        public TorqueUnit InputTorqueUnit { get; set; } = TorqueUnit.Nm;

        /// <summary>
        /// 输入电流单位（默认：mA）
        /// </summary>
        public CurrentUnit InputCurrentUnit { get; set; } = CurrentUnit.mA;

        /// <summary>
        /// 输出扭矩单位（默认：g.cm）
        /// </summary>
        public TorqueUnit OutputTorqueUnit { get; set; } = TorqueUnit.gcm;

        /// <summary>
        /// 输出电流单位（默认：mA）
        /// </summary>
        public CurrentUnit OutputCurrentUnit { get; set; } = CurrentUnit.mA;

        #endregion

        #region 输入数据

        /// <summary>
        /// 第一点转速 (rpm)
        /// </summary>
        public decimal InputSpeed1 { get; set; } = 49.6m;

        /// <summary>
        /// 第一点扭矩
        /// </summary>
        public decimal InputTorque1 { get; set; } = 0m;

        /// <summary>
        /// 第一点电流
        /// </summary>
        public decimal InputCurrent1 { get; set; } = 1530m;

        /// <summary>
        /// 第二点转速 (rpm)
        /// </summary>
        public decimal InputSpeed2 { get; set; } = 43.25m;

        /// <summary>
        /// 第二点扭矩
        /// </summary>
        public decimal InputTorque2 { get; set; } = 4.5m;

        /// <summary>
        /// 第二点电流
        /// </summary>
        public decimal InputCurrent2 { get; set; } = 6670m;

        #endregion

        #region 输出数据

        /// <summary>
        /// 第一点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed1 { get; set; } = 0m;

        /// <summary>
        /// 第一点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque1 { get; set; } = 0m;

        /// <summary>
        /// 第一点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent1 { get; set; } = 0m;

        /// <summary>
        /// 第二点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed2 { get; set; } = 0m;

        /// <summary>
        /// 第二点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque2 { get; set; } = 0m;

        /// <summary>
        /// 第二点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent2 { get; set; } = 0m;

        /// <summary>
        /// 中间点计算结果 - 转速 (rpm)
        /// </summary>
        public decimal OutputSpeed3 { get; set; } = 0m;

        /// <summary>
        /// 中间点计算结果 - 扭矩
        /// </summary>
        public decimal OutputTorque3 { get; set; } = 0m;

        /// <summary>
        /// 中间点计算结果 - 电流
        /// </summary>
        public decimal OutputCurrent3 { get; set; } = 0m;

        #endregion

        #region 状态管理

        /// <summary>
        /// 是否显示计算结果
        /// </summary>
        public bool ShowResults { get; set; } = false;

        #endregion

        #region 单位转换方法

        // 单位转换逻辑已提取到 ITwoPointCalculator/TwoPointCalculator

        #endregion

        #region 两点计算直线类

        // 直线计算已由 TwoPointCalculator 内部实现，此处保留注释说明

        #endregion

        #region 计算方法

        /// <summary>
        /// 计算入口方法
        /// </summary>
        public async Task CalculateFits()
        {
            var input = new TwoPointInput
            {
                InputSpeed1 = InputSpeed1,
                InputTorque1 = InputTorque1,
                InputTorqueUnit = InputTorqueUnit,
                InputCurrent1 = InputCurrent1,
                InputCurrentUnit = InputCurrentUnit,
                InputSpeed2 = InputSpeed2,
                InputTorque2 = InputTorque2,
                InputCurrent2 = InputCurrent2,
                OutputTorqueUnit = OutputTorqueUnit,
                OutputCurrentUnit = OutputCurrentUnit
            };

            var result = await TwoPointCalculator.CalculateAsync(input);

            OutputSpeed1 = result.OutputSpeed1;
            OutputTorque1 = result.OutputTorque1;
            OutputCurrent1 = result.OutputCurrent1;

            OutputSpeed2 = result.OutputSpeed2;
            OutputTorque2 = result.OutputTorque2;
            OutputCurrent2 = result.OutputCurrent2;

            OutputSpeed3 = result.OutputSpeed3;
            OutputTorque3 = result.OutputTorque3;
            OutputCurrent3 = result.OutputCurrent3;

            ShowResults = true;
            StateHasChanged();
        }

        #endregion
    }
}
