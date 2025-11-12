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
            builder.Services.AddControllers();

            builder.Services.AddScoped<IPredictionService, PredictionService>();
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
