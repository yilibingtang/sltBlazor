using System;
using System.Linq;
using MathNet.Numerics;

namespace YX.Services
{
    public class MotorFitResult
    {
        public double[] CurrentCoeffs { get; set; } = Array.Empty<double>();
        public double[] SpeedCoeffs { get; set; } = Array.Empty<double>();
        public double NoLoadSpeed { get; set; }
        public double StallTorque { get; set; }
        public double NoLoadCurrent { get; set; }
        public double StallCurrent { get; set; }
        public double PlotXMin { get; set; }
        public double PlotXMax { get; set; }
        public double PlotYMin { get; set; }
        public double PlotYMax { get; set; }
        public double[] Torques { get; set; } = Array.Empty<double>();
        public double[] Speeds { get; set; } = Array.Empty<double>();
        public double[] Currents { get; set; } = Array.Empty<double>();
    }

    public class MotorCalculator
    {
        static double EvalPoly(double[] coeffs, double x)
        {
            double y = 0;
            for (int i = 0; i < coeffs.Length; i++) y += coeffs[i] * Math.Pow(x, i);
            return y;
        }

        public MotorFitResult ComputeFits(System.Collections.Generic.IEnumerable<MotorDataPoint> points)
        {
            var list = points?.ToList() ?? new System.Collections.Generic.List<MotorDataPoint>();
            var result = new MotorFitResult();
            if (list.Count < 2) return result;

            var torques = list.Select(p => p.Torque).ToArray();
            var currents = list.Select(p => p.Current).ToArray();
            var speeds = list.Select(p => p.Speed).ToArray();

            result.CurrentCoeffs = Fit.Polynomial(torques, currents, 1);
            result.SpeedCoeffs = Fit.Polynomial(torques, speeds, 1);

            result.NoLoadSpeed = EvalPoly(result.SpeedCoeffs, 0);
            result.NoLoadCurrent = EvalPoly(result.CurrentCoeffs, 0);

            var a0 = result.SpeedCoeffs.Length > 0 ? result.SpeedCoeffs[0] : 0;
            var a1 = result.SpeedCoeffs.Length > 1 ? result.SpeedCoeffs[1] : 0;
            if (Math.Abs(a1) > 1e-12) result.StallTorque = -a0 / a1; else result.StallTorque = double.NaN;
            result.StallCurrent = double.IsNaN(result.StallTorque) ? double.NaN : EvalPoly(result.CurrentCoeffs, result.StallTorque);

            result.PlotXMin = torques.Min(); result.PlotXMax = torques.Max();
            if (Math.Abs(result.PlotXMax - result.PlotXMin) < 1e-6) { result.PlotXMin -= 1; result.PlotXMax += 1; }
            var yVals = speeds.Concat(currents).ToArray();
            result.PlotYMin = yVals.Min(); result.PlotYMax = yVals.Max();
            if (Math.Abs(result.PlotYMax - result.PlotYMin) < 1e-6) { result.PlotYMin -= 1; result.PlotYMax += 1; }

            result.Torques = torques; result.Speeds = speeds; result.Currents = currents;
            return result;
        }
    }
}
