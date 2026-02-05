using System.Threading.Tasks;
using YX.Models;

namespace YX.Services
{
    public class TwoPointCalculator : ITwoPointCalculator
    {
        // 单位转换方法（和组件中保持一致）
        private decimal ConvertToStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value / 1000m,
                TorqueUnit.Kgcm => value * 0.0980665m,
                TorqueUnit.gcm => value * 0.0980665m / 1000m,
                _ => value
            };
        }

        private decimal ConvertFromStandardTorque(decimal value, TorqueUnit unit)
        {
            return unit switch
            {
                TorqueUnit.Nm => value,
                TorqueUnit.mNm => value * 1000m,
                TorqueUnit.Kgcm => value / 0.0980665m,
                TorqueUnit.gcm => value / 0.0000980665m,
                _ => value
            };
        }

        private decimal ConvertToStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value / 1000m,
                _ => value
            };
        }

        private decimal ConvertFromStandardCurrent(decimal value, CurrentUnit unit)
        {
            return unit switch
            {
                CurrentUnit.A => value,
                CurrentUnit.mA => value * 1000m,
                _ => value
            };
        }

        private class LineCalc
        {
            private readonly decimal[] _p1;
            private readonly decimal[] _p2;
            public LineCalc(decimal[] p1, decimal[] p2) { _p1 = p1; _p2 = p2; }
            public decimal Slope() => (_p2[1] - _p1[1]) / (_p2[0] - _p1[0]);
            public decimal Y(decimal x) => (x - _p1[0]) * Slope() + _p1[1];
            public decimal X(decimal y) => (y - _p1[1]) / Slope() + _p1[0];
        }

        public Task<TwoPointResult> CalculateAsync(TwoPointInput input)
        {
            // 转换输入
            decimal convertedSpeed1 = input.InputSpeed1;
            decimal convertedTorque1 = ConvertToStandardTorque(input.InputTorque1, input.InputTorqueUnit);
            decimal convertedCurrent1 = ConvertToStandardCurrent(input.InputCurrent1, input.InputCurrentUnit);

            decimal convertedSpeed2 = input.InputSpeed2;
            decimal convertedTorque2 = ConvertToStandardTorque(input.InputTorque2, input.InputTorqueUnit);
            decimal convertedCurrent2 = ConvertToStandardCurrent(input.InputCurrent2, input.InputCurrentUnit);

            decimal speed1 = decimal.Floor(convertedSpeed1);
            decimal speed2 = decimal.Floor(convertedSpeed2);
            decimal speed3 = 2 * speed2 - speed1;

            var torqueCalc = new LineCalc(new decimal[] { convertedTorque1, convertedSpeed1 }, new decimal[] { convertedTorque2, convertedSpeed2 });
            decimal torque1 = torqueCalc.X(speed1);
            decimal torque2 = torqueCalc.X(speed2);
            decimal torque3 = torqueCalc.X(speed3);

            var currentCalc = new LineCalc(new decimal[] { convertedSpeed1, convertedCurrent1 }, new decimal[] { convertedSpeed2, convertedCurrent2 });
            decimal current1 = currentCalc.Y(speed1);
            decimal current2 = currentCalc.Y(speed2);
            decimal current3 = currentCalc.Y(speed3);

            var result = new TwoPointResult
            {
                OutputSpeed1 = speed1,
                OutputTorque1 = ConvertFromStandardTorque(torque1, input.OutputTorqueUnit),
                OutputCurrent1 = ConvertFromStandardCurrent(current1, input.OutputCurrentUnit),

                OutputSpeed2 = speed2,
                OutputTorque2 = ConvertFromStandardTorque(torque2, input.OutputTorqueUnit),
                OutputCurrent2 = ConvertFromStandardCurrent(current2, input.OutputCurrentUnit),

                OutputSpeed3 = speed3,
                OutputTorque3 = ConvertFromStandardTorque(torque3, input.OutputTorqueUnit),
                OutputCurrent3 = ConvertFromStandardCurrent(current3, input.OutputCurrentUnit),
            };

            return Task.FromResult(result);
        }
    }
}
