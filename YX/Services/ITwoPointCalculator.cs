using System.Threading.Tasks;
using YX.Models;

namespace YX.Services
{
    public interface ITwoPointCalculator
    {
        Task<TwoPointResult> CalculateAsync(TwoPointInput input);
    }

    public class TwoPointInput
    {
        public decimal InputSpeed1 { get; set; }
        public decimal InputTorque1 { get; set; }
        public TorqueUnit InputTorqueUnit { get; set; }
        public decimal InputCurrent1 { get; set; }
        public CurrentUnit InputCurrentUnit { get; set; }

        public decimal InputSpeed2 { get; set; }
        public decimal InputTorque2 { get; set; }
        public decimal InputCurrent2 { get; set; }

        public TorqueUnit OutputTorqueUnit { get; set; }
        public CurrentUnit OutputCurrentUnit { get; set; }
    }

    public class TwoPointResult
    {
        public decimal OutputSpeed1 { get; set; }
        public decimal OutputTorque1 { get; set; }
        public decimal OutputCurrent1 { get; set; }

        public decimal OutputSpeed2 { get; set; }
        public decimal OutputTorque2 { get; set; }
        public decimal OutputCurrent2 { get; set; }

        public decimal OutputSpeed3 { get; set; }
        public decimal OutputTorque3 { get; set; }
        public decimal OutputCurrent3 { get; set; }
    }
}
