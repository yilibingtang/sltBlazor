using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YX.Data;

namespace YX.Services
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MotorDbContext>();
                db.Database.EnsureCreated();
                
                // 初始化螺纹数据
                InitializeThreadData(db);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database");
            }
        }
        
        private void InitializeThreadData(MotorDbContext db)
        {
            // 检查ThreadData表是否为空
            if (!db.ThreadData.Any())
            {
                _logger.LogInformation("Initializing thread data...");
                
                var threadDataList = new List<YX.Models.ThreadDataModel>
                {
                    new YX.Models.ThreadDataModel { Size = "1", ThreadDesignation = "M1x0.25", ExternalClass = "6g", ExternalMajorDiaMax = "0.982", ExternalMajorDiaMin = "0.915", InternalClass = "6H", InternalMinorDiaMin = "0.729", InternalMinorDiaMax = "0.809" },
                    new YX.Models.ThreadDataModel { Size = "1.1", ThreadDesignation = "M1.1x0.25", ExternalClass = "6g", ExternalMajorDiaMax = "1.082", ExternalMajorDiaMin = "1.015", InternalClass = "6H", InternalMinorDiaMin = "0.829", InternalMinorDiaMax = "0.909" },
                    new YX.Models.ThreadDataModel { Size = "1.2", ThreadDesignation = "M1.2x0.25", ExternalClass = "6g", ExternalMajorDiaMax = "1.182", ExternalMajorDiaMin = "1.115", InternalClass = "6H", InternalMinorDiaMin = "0.929", InternalMinorDiaMax = "1.009" },
                    new YX.Models.ThreadDataModel { Size = "1.4", ThreadDesignation = "M1.4x0.3", ExternalClass = "6g", ExternalMajorDiaMax = "1.383", ExternalMajorDiaMin = "1.308", InternalClass = "6H", InternalMinorDiaMin = "1.183", InternalMinorDiaMax = "1.258" },
                    new YX.Models.ThreadDataModel { Size = "1.6", ThreadDesignation = "M1.6x0.35", ExternalClass = "6g", ExternalMajorDiaMax = "1.581", ExternalMajorDiaMin = "1.496", InternalClass = "6H", InternalMinorDiaMin = "1.221", InternalMinorDiaMax = "1.321" },
                    new YX.Models.ThreadDataModel { Size = "1.7", ThreadDesignation = "M1.7x0.35", ExternalClass = "6g", ExternalMajorDiaMax = "1.681", ExternalMajorDiaMin = "1.596", InternalClass = "6H", InternalMinorDiaMin = "1.321", InternalMinorDiaMax = "1.421" },
                    new YX.Models.ThreadDataModel { Size = "1.8", ThreadDesignation = "M1.8x0.35", ExternalClass = "6g", ExternalMajorDiaMax = "1.781", ExternalMajorDiaMin = "1.696", InternalClass = "6H", InternalMinorDiaMin = "1.421", InternalMinorDiaMax = "1.521" },
                    new YX.Models.ThreadDataModel { Size = "2", ThreadDesignation = "M2x0.4", ExternalClass = "6g", ExternalMajorDiaMax = "1.981", ExternalMajorDiaMin = "1.886", InternalClass = "6H", InternalMinorDiaMin = "1.567", InternalMinorDiaMax = "1.679" },
                    new YX.Models.ThreadDataModel { Size = "2.2", ThreadDesignation = "M2.2x0.45", ExternalClass = "6g", ExternalMajorDiaMax = "2.180", ExternalMajorDiaMin = "2.080", InternalClass = "6H", InternalMinorDiaMin = "1.713", InternalMinorDiaMax = "1.838" },
                    new YX.Models.ThreadDataModel { Size = "2.3", ThreadDesignation = "M2.3x0.45", ExternalClass = "6g", ExternalMajorDiaMax = "2.280", ExternalMajorDiaMin = "2.180", InternalClass = "6H", InternalMinorDiaMin = "1.813", InternalMinorDiaMax = "1.938" },
                    new YX.Models.ThreadDataModel { Size = "2.5", ThreadDesignation = "M2.5x0.45", ExternalClass = "6g", ExternalMajorDiaMax = "2.480", ExternalMajorDiaMin = "2.380", InternalClass = "6H", InternalMinorDiaMin = "2.013", InternalMinorDiaMax = "2.138" },
                    new YX.Models.ThreadDataModel { Size = "2.6", ThreadDesignation = "M2.6x0.45", ExternalClass = "6g", ExternalMajorDiaMax = "2.580", ExternalMajorDiaMin = "2.480", InternalClass = "6H", InternalMinorDiaMin = "2.113", InternalMinorDiaMax = "2.238" },
                    new YX.Models.ThreadDataModel { Size = "3", ThreadDesignation = "M3x0.5", ExternalClass = "6g", ExternalMajorDiaMax = "2.980", ExternalMajorDiaMin = "2.874", InternalClass = "6H", InternalMinorDiaMin = "2.459", InternalMinorDiaMax = "2.599" },
                    new YX.Models.ThreadDataModel { Size = "3.5", ThreadDesignation = "M3.5x0.6", ExternalClass = "6g", ExternalMajorDiaMax = "3.479", ExternalMajorDiaMin = "3.354", InternalClass = "6H", InternalMinorDiaMin = "2.850", InternalMinorDiaMax = "3.010" },
                    new YX.Models.ThreadDataModel { Size = "4", ThreadDesignation = "M4x0.7", ExternalClass = "6g", ExternalMajorDiaMax = "3.978", ExternalMajorDiaMin = "3.838", InternalClass = "6H", InternalMinorDiaMin = "3.242", InternalMinorDiaMax = "3.422" },
                    new YX.Models.ThreadDataModel { Size = "4.5", ThreadDesignation = "M4.5x0.75", ExternalClass = "6g", ExternalMajorDiaMax = "4.478", ExternalMajorDiaMin = "4.338", InternalClass = "6H", InternalMinorDiaMin = "3.688", InternalMinorDiaMax = "3.878" },
                    new YX.Models.ThreadDataModel { Size = "5", ThreadDesignation = "M5x0.8", ExternalClass = "6g", ExternalMajorDiaMax = "4.976", ExternalMajorDiaMin = "4.826", InternalClass = "6H", InternalMinorDiaMin = "4.134", InternalMinorDiaMax = "4.334" },
                    new YX.Models.ThreadDataModel { Size = "5.5", ThreadDesignation = "M5.5x0.5", ExternalClass = "6g", ExternalMajorDiaMax = "5.480", ExternalMajorDiaMin = "5.374", InternalClass = "6H", InternalMinorDiaMin = "4.959", InternalMinorDiaMax = "5.099" },
                    new YX.Models.ThreadDataModel { Size = "6", ThreadDesignation = "M6x1", ExternalClass = "6g", ExternalMajorDiaMax = "5.974", ExternalMajorDiaMin = "5.794", InternalClass = "6H", InternalMinorDiaMin = "4.917", InternalMinorDiaMax = "5.153" },
                    new YX.Models.ThreadDataModel { Size = "7", ThreadDesignation = "M7x1", ExternalClass = "6g", ExternalMajorDiaMax = "6.974", ExternalMajorDiaMin = "6.794", InternalClass = "6H", InternalMinorDiaMin = "5.917", InternalMinorDiaMax = "6.153" },
                    new YX.Models.ThreadDataModel { Size = "8", ThreadDesignation = "M8x1.25", ExternalClass = "6g", ExternalMajorDiaMax = "7.972", ExternalMajorDiaMin = "7.760", InternalClass = "6H", InternalMinorDiaMin = "6.647", InternalMinorDiaMax = "6.912" },
                    new YX.Models.ThreadDataModel { Size = "9", ThreadDesignation = "M9x1.25", ExternalClass = "6g", ExternalMajorDiaMax = "8.972", ExternalMajorDiaMin = "8.760", InternalClass = "6H", InternalMinorDiaMin = "7.647", InternalMinorDiaMax = "7.912" },
                    new YX.Models.ThreadDataModel { Size = "10", ThreadDesignation = "M10x1.5", ExternalClass = "6g", ExternalMajorDiaMax = "9.968", ExternalMajorDiaMin = "9.732", InternalClass = "6H", InternalMinorDiaMin = "8.376", InternalMinorDiaMax = "8.676" },
                    new YX.Models.ThreadDataModel { Size = "11", ThreadDesignation = "M11x1.5", ExternalClass = "6g", ExternalMajorDiaMax = "10.968", ExternalMajorDiaMin = "10.732", InternalClass = "6H", InternalMinorDiaMin = "9.376", InternalMinorDiaMax = "9.676" },
                    new YX.Models.ThreadDataModel { Size = "12", ThreadDesignation = "M12x1.75", ExternalClass = "6g", ExternalMajorDiaMax = "11.966", ExternalMajorDiaMin = "11.701", InternalClass = "6H", InternalMinorDiaMin = "10.106", InternalMinorDiaMax = "10.441" },
                    new YX.Models.ThreadDataModel { Size = "14", ThreadDesignation = "M14x2", ExternalClass = "6g", ExternalMajorDiaMax = "13.962", ExternalMajorDiaMin = "13.682", InternalClass = "6H", InternalMinorDiaMin = "11.835", InternalMinorDiaMax = "12.210" },
                    new YX.Models.ThreadDataModel { Size = "15", ThreadDesignation = "M15x1.5", ExternalClass = "6g", ExternalMajorDiaMax = "14.968", ExternalMajorDiaMin = "14.732", InternalClass = "6H", InternalMinorDiaMin = "13.376", InternalMinorDiaMax = "13.676" },
                    new YX.Models.ThreadDataModel { Size = "16", ThreadDesignation = "M16x2", ExternalClass = "6g", ExternalMajorDiaMax = "15.962", ExternalMajorDiaMin = "15.682", InternalClass = "6H", InternalMinorDiaMin = "13.835", InternalMinorDiaMax = "14.210" },
                    new YX.Models.ThreadDataModel { Size = "17", ThreadDesignation = "M17x1.5", ExternalClass = "6g", ExternalMajorDiaMax = "16.968", ExternalMajorDiaMin = "16.732", InternalClass = "6H", InternalMinorDiaMin = "15.376", InternalMinorDiaMax = "15.676" },
                    new YX.Models.ThreadDataModel { Size = "18", ThreadDesignation = "M18x2.5", ExternalClass = "6g", ExternalMajorDiaMax = "17.958", ExternalMajorDiaMin = "17.623", InternalClass = "6H", InternalMinorDiaMin = "15.294", InternalMinorDiaMax = "15.744" },
                    new YX.Models.ThreadDataModel { Size = "20", ThreadDesignation = "M20x2.5", ExternalClass = "6g", ExternalMajorDiaMax = "19.958", ExternalMajorDiaMin = "19.623", InternalClass = "6H", InternalMinorDiaMin = "17.294", InternalMinorDiaMax = "17.744" },
                    new YX.Models.ThreadDataModel { Size = "22", ThreadDesignation = "M22x3", ExternalClass = "6g", ExternalMajorDiaMax = "21.952", ExternalMajorDiaMin = "21.577", InternalClass = "6H", InternalMinorDiaMin = "18.752", InternalMinorDiaMax = "19.252" },
                    new YX.Models.ThreadDataModel { Size = "24", ThreadDesignation = "M24x3", ExternalClass = "6g", ExternalMajorDiaMax = "23.952", ExternalMajorDiaMin = "23.577", InternalClass = "6H", InternalMinorDiaMin = "20.752", InternalMinorDiaMax = "21.252" },
                    new YX.Models.ThreadDataModel { Size = "25", ThreadDesignation = "M25x2", ExternalClass = "6g", ExternalMajorDiaMax = "24.962", ExternalMajorDiaMin = "24.682", InternalClass = "6H", InternalMinorDiaMin = "22.835", InternalMinorDiaMax = "23.210" },
                    new YX.Models.ThreadDataModel { Size = "26", ThreadDesignation = "M26x1.5", ExternalClass = "6g", ExternalMajorDiaMax = "25.968", ExternalMajorDiaMin = "25.732", InternalClass = "6H", InternalMinorDiaMin = "24.376", InternalMinorDiaMax = "24.676" },
                    new YX.Models.ThreadDataModel { Size = "27", ThreadDesignation = "M27x3", ExternalClass = "6g", ExternalMajorDiaMax = "26.952", ExternalMajorDiaMin = "26.577", InternalClass = "6H", InternalMinorDiaMin = "23.752", InternalMinorDiaMax = "24.252" },
                    new YX.Models.ThreadDataModel { Size = "28", ThreadDesignation = "M28x2", ExternalClass = "6g", ExternalMajorDiaMax = "27.962", ExternalMajorDiaMin = "27.682", InternalClass = "6H", InternalMinorDiaMin = "25.835", InternalMinorDiaMax = "26.210" },
                    new YX.Models.ThreadDataModel { Size = "30", ThreadDesignation = "M30x3.5", ExternalClass = "6g", ExternalMajorDiaMax = "29.947", ExternalMajorDiaMin = "29.522", InternalClass = "6H", InternalMinorDiaMin = "26.211", InternalMinorDiaMax = "26.771" }
                };
                
                db.ThreadData.AddRange(threadDataList);
                db.SaveChanges();
                
                _logger.LogInformation("Thread data initialized successfully");
            }
        }
    }
}
