using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Linq;
using YX.Components;
using YX.Data;
using YX.Services;

namespace YX.Extensions
{
    public static class YXServiceExtensions
    {
        // 把服务注册集中到一个地方，便于单元测试和解耦
        public static IServiceCollection AddYXServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // EF Core - SQLite DbContext for motor persistence
            services.AddDbContext<MotorDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("MotorDb") ?? "Data Source=motors.db"));

            // Bind theme options
            services.Configure<ThemeOptions>(configuration.GetSection("Theme"));

            // AutoMapper (register profile assembly)
            services.AddAutoMapper(cfg => { cfg.AddProfile<YX.Mapping.AutoMapperProfile>(); }, typeof(YX.Mapping.AutoMapperProfile).Assembly);

            // Notification service for Blazor toasts
            services.AddSingleton<NotificationService>();
            // Motor services
            services.AddScoped<IMotorManager, MotorManager>();
            // Repository
            services.AddScoped<IMotorRepository, EfMotorRepository>();
            // Two-point calculator service
            services.AddScoped<ITwoPointCalculator, TwoPointCalculator>();
            services.AddSingleton<MotorValidator>();
            services.AddScoped<IMotorCalculator, MotorCalculator>();
            services.AddScoped<ICsvExportService, CsvExportService>();
            // Database initializer
            services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

            return services;
        }

        // 配置中间件和默认管道
        public static WebApplication UseYXDefaults(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Ensure database is created on startup via initializer (testable)
            var initializer = app.Services.GetRequiredService<IDatabaseInitializer>();
            initializer.Initialize();

            // 在开发环境下应用启动后自动打开系统默认浏览器（跨平台实现）
            if (app.Environment.IsDevelopment())
            {
                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    try
                    {
                        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5000";

                        // 跨平台打开默认浏览器
                        // Windows: ProcessStartInfo(FileName = url) + UseShellExecute=true
                        // macOS: open <url>
                        // Linux: xdg-open <url>
                        if (OperatingSystem.IsWindows())
                        {
                            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        }
                        else if (OperatingSystem.IsMacOS())
                        {
                            Process.Start("open", url);
                        }
                        else if (OperatingSystem.IsLinux())
                        {
                            Process.Start("xdg-open", url);
                        }
                    }
                    catch
                    {
                        // 忽略异常
                    }
                });
            }

            return app;
        }
    }
}
