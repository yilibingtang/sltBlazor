using Microsoft.AspNetCore.Components;
using System;

namespace YX.Components.Pages
{
    public partial class LeadScrewCalculator : ComponentBase
    {
        // 输入参数
        private double diameter = 6;         // 公称直径d
        private double lead = 1;             // 导程S
        private double pitchDiameter = 5.5;  // 中径d2
        private int starts = 1;              // 头数n
        private double threadAngle = 60;     // 螺纹角
        private double pitch = 1;            // 螺距p
        private double friction = 0.17;       // 摩擦因数
        private int reductionRatio = 133;    // 减速比
        private int gearboxRpm = 300;        // 齿轮箱输出额定转速nN
        private double gearboxTorque = 0.00070701; // 齿轮箱输出额定扭矩TN

        // 计算结果
        private double threadRiseAngle;      // 螺纹升角φ
        private double equivalentFrictionAngle; // 当量摩擦角φv
        private double tanSumAngle;          // tan(φ+φv)
        private int motorRpm;                // 电机额定转速nN
        private double motorTorque;          // 电机额定扭矩TN
        private int screwSpeed;              // 丝杆速度
        private double thrustForce = 1;      // 推力
        private double forceInGrams;         // 推力（克）

        protected override void OnInitialized()
        {
            // 初始化计算
            Calculate();
        }
        private void SCalP()
        {
            pitch = lead / starts;
        }
        private void PCalS()
        {
            lead = pitch * starts;
        }

        private void Calculate()
        {
            pitchDiameter = diameter - 0.5 * lead;
            // 计算螺纹升角φ（弧度）
            double phiRad = Math.Atan(lead / (Math.PI * pitchDiameter));
            // 转换为角度
            threadRiseAngle = phiRad * 180 / Math.PI;

            // 计算当量摩擦角φv（弧度）
            double phiVRad = Math.Atan(friction / Math.Cos(threadAngle / 2 * Math.PI / 180));
            // 转换为角度
            equivalentFrictionAngle = phiVRad * 180 / Math.PI;

            // 计算tan(φ+φv)
            tanSumAngle = Math.Tan(phiRad + phiVRad);

            // 计算电机转速
            motorRpm = gearboxRpm * reductionRatio;

            // 计算电机扭矩
            motorTorque = gearboxTorque / reductionRatio;

            // 计算丝杆速度
            screwSpeed = (int)(lead * gearboxRpm);

            // 计算推力（如果需要）
            // thrustForce = (2 * Math.PI * gearboxTorque * tanSumAngle) / lead;

            // 计算推力（克）
            forceInGrams = thrustForce * 101.9716;
        }
    }
}
