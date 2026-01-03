namespace YX.Services
{
    /// <summary>
    /// 物理常量管理类，集中管理所有物理相关的常量
    /// </summary>
    public static class PhysicalConstants
    {
        /// <summary>
        /// 电机效率计算常数
        /// 公式：效率 = 转速 * 扭矩 / (K * 电流 * 电压)
        /// </summary>
        public const double MotorEfficiencyConstant = 9.5493;
        
        /// <summary>
        /// 重力加速度 (m/s²)
        /// </summary>
        public const double GravitationalAcceleration = 9.80665;
        

    }
}