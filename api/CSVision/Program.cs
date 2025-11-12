using CSVision.BackgroundServices;
using CSVision.Interfaces;
using CSVision.Services;
using Microsoft.Extensions.FileProviders;

namespace CSVision
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            SetupServices(builder);

            var app = builder.Build();

            SetupStaticFiles(app, builder);

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void SetupServices(WebApplicationBuilder builder)
        {
            // Controllers
            builder.Services.AddControllers();

            // Scoped Services
            builder.Services.AddScoped<IPredictionService, PredictionService>();
            builder.Services.AddScoped<IFileService, FileService>();

            // Background Services
            builder.Services.AddHostedService(provider => new CleanUpStaticFilesBackgroundService(
                provider,
                TimeSpan.FromHours(24)
            ));
        }

        private static void SetupStaticFiles(WebApplication app, WebApplicationBuilder builder)
        {
            if (!Directory.Exists(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")))
            {
                Directory.CreateDirectory(
                    Path.Combine(builder.Environment.ContentRootPath, "wwwroot")
                );
            }

            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")
                    ),
                    RequestPath = "/static",
                }
            );
        }
    }
}
