using Microsoft.AspNetCore.Components;
using System;

namespace YX.Components.Pages
{
    public partial class MotorEfficiencyCalculator : ComponentBase
    {
        // 公式中的固定常数（提取为常量，便于维护）
        private const double 常数A = 9.5493;
        private const double 常数B = 0.0980665;

        // 输入参数
        private double 整机空载转速 = 3000;    // rpm
        private double 整机效率扭矩 = 5;       // N·m
        private double 整机堵转扭矩 = 20;      // N·m
        private double 减速比 = 10;            // 无单位
        private double 总效率 = 0.75;          // 0-1
        private double 电压 = 24;              // V
        private double 单电机空载电流 = 0.5;   // A

        // 计算结果
        private double 整机效率转速;
        private double 分子;
        private double 分母;
        private double 中间项;
        private double 电机效率;
        private string 错误信息 = string.Empty;

        protected override void OnInitialized()
        {
            // 初始化计算
            Calculate();
        }

        private void Calculate()
        {
            错误信息 = string.Empty;
            try
            {
                // 步骤1：计算整机效率转速（复用方法）
                整机效率转速 = 计算整机效率转速(整机空载转速, 整机效率扭矩, 整机堵转扭矩);
                
                // 步骤2：计算公式分子和分母（复用方法）
                分子 = 计算公式分子(单电机空载电流, 电压, 总效率, 减速比);
                分母 = 计算公式分母(整机效率扭矩, 整机效率转速);
                
                // 步骤3：核心数学公式计算
                中间项 = 1 - 分子 / 分母;
                // 防护：避免根号负数（参数异常时提示）
                if (中间项 < 0)
                {
                    错误信息 = "计算异常：中间项为负数，请检查输入参数是否合理！";
                    return;
                }
                电机效率 = Math.Pow(中间项, 2);
            }
            catch (Exception ex)
            {
                错误信息 = $"计算出错：{ex.Message}";
            }
        }

        /// <summary>
        /// 【可复用方法1】计算整机效率转速（公式6）
        /// </summary>
        /// <param name="整机空载转速">n0 (rpm)</param>
        /// <param name="整机效率扭矩">Tw (N·m)</param>
        /// <param name="整机堵转扭矩">Tk (N·m)</param>
        /// <returns>整机效率转速 (rpm)</returns>
        private double 计算整机效率转速(double 整机空载转速, double 整机效率扭矩, double 整机堵转扭矩)
        {
            return 整机空载转速 * (1 - 整机效率扭矩 / 整机堵转扭矩);
        }

        /// <summary>
        /// 【可复用方法2】计算数学公式中的分母部分（便于复用和排查）
        /// </summary>
        /// <param name="整机效率扭矩">Tw (N·m)</param>
        /// <param name="整机效率转速">步骤1计算结果 (rpm)</param>
        /// <returns>公式分母值</returns>
        private double 计算公式分母(double 整机效率扭矩, double 整机效率转速)
        {
            return 常数B * 整机效率扭矩 * 整机效率转速;
        }

        /// <summary>
        /// 【可复用方法3】计算数学公式中的分子部分（便于复用和排查）
        /// </summary>
        /// <param name="单电机空载电流">I0 (A)</param>
        /// <param name="电压">U (V)</param>
        /// <param name="总效率">ηtotal</param>
        /// <param name="减速比">i</param>
        /// <returns>公式分子值</returns>
        private double 计算公式分子(double 单电机空载电流, double 电压, double 总效率, double 减速比)
        {
            return 单电机空载电流 * 电压 * 总效率 * Math.Pow(减速比, 2) * 常数A;
        }
    }
}
