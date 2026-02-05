using System.Collections.Generic;

namespace YX.Models.Dto
{
    public class MotorDto
    {
        public int Id { get; set; }
        public string MotorName { get; set; } = string.Empty;
        public int MotorType { get; set; }
        public decimal Voltage { get; set; }
        public decimal MotorEfficiency { get; set; }
        public decimal MaxEfficiencyLoadRatio { get; set; }
        public decimal TotalReductionRatio { get; set; }
        public int ReductionStageCount { get; set; }
        public decimal TotalEfficiency { get; set; }
        public List<MotorDataPointDto> DataPoints { get; set; } = new List<MotorDataPointDto>();
    }
}
