using Microsoft.AspNetCore.Components;

namespace YX.Components.Pages
{
    public partial class MotorPowerCal : ComponentBase
    {
        // 输入数据
        public double Number1 { get; set; } = 0;
        public double Number2 { get; set; } = 0;
        
        // 计算结果
        public double Average => (Number1 + Number2) / 2;
    }
}