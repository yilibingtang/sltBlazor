using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YX.Models;

namespace YX.Services
{
    public class MotorValidator
    {
        public bool ValidatePoints(IEnumerable<MotorDataPoint> points, out List<string> errors)
        {
            errors = new List<string>();
            var list = points == null ? new List<MotorDataPoint>() : new List<MotorDataPoint>(points);
            if (list.Count == 0)
            {
                errors.Add("请至少添加一组测试点。");
                return false;
            }
            int idx = 1;
            foreach (var p in list)
            {
                var ctx = new ValidationContext(p);
                var r = new List<ValidationResult>();
                if (!Validator.TryValidateObject(p, ctx, r, true))
                {
                    foreach (var vr in r)
                    {
                        errors.Add($"第{idx}行: {vr.ErrorMessage}");
                    }
                }
                idx++;
            }
            if (list.Count < 2)
            {
                errors.Add("至少需要两组测试点以进行拟合。");
            }
            return errors.Count == 0;
        }
    }
}
