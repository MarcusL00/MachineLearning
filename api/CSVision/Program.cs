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
        }
    }
}
