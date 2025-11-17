using CSVision.Interfaces;
using CSVision.Services;

namespace CSVision
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                // Overkill timeout, but ensures long-running requests complete
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(60);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(60);
            });

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
            builder.Services.AddScoped<IGraphService, GraphService>();
        }
    }
}
