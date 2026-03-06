using System.ComponentModel.DataAnnotations;

namespace YX.Models
{
    /// <summary>
    /// 螺纹数据模型
    /// </summary>
    public class ThreadDataModel
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 尺寸
        /// </summary>
        public string Size { get; set; } = string.Empty;
        
        /// <summary>
        /// 螺纹规格
        /// </summary>
        public string ThreadDesignation { get; set; } = string.Empty;
        
        /// <summary>
        /// 外螺纹等级
        /// </summary>
        public string ExternalClass { get; set; } = string.Empty;
        
        /// <summary>
        /// 外螺纹大径最大值
        /// </summary>
        public string ExternalMajorDiaMax { get; set; } = string.Empty;
        
        /// <summary>
        /// 外螺纹大径最小值
        /// </summary>
        public string ExternalMajorDiaMin { get; set; } = string.Empty;
        
        /// <summary>
        /// 内螺纹等级
        /// </summary>
        public string InternalClass { get; set; } = string.Empty;
        
        /// <summary>
        /// 内螺纹小径最小值
        /// </summary>
        public string InternalMinorDiaMin { get; set; } = string.Empty;
        
        /// <summary>
        /// 内螺纹小径最大值
        /// </summary>
        public string InternalMinorDiaMax { get; set; } = string.Empty;
    }
}
