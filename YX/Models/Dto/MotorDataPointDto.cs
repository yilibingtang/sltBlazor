namespace YX.Models.Dto
{
    public class MotorDataPointDto
    {
        public int Id { get; set; }
        public int MotorId { get; set; }
        public int Type { get; set; }
        public decimal Torque { get; set; }
        public decimal Speed { get; set; }
        public decimal Current { get; set; }
    }
}
