using System.Text.Json.Serialization;
using Matrix.Data.Utilities;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices;

public static class MatrixServer
{
    public static async Task<WebApplication> CreateWebServer(string[] args, IConfigurationRoot configuration, string? fontsPath = null)
    {
        string databasePath = Path.Combine(Environment.CurrentDirectory, "Data", "matrix.db");
        if (!string.IsNullOrWhiteSpace(configuration[ConfigConstants.DatabasePath]))
        {
            databasePath = configuration[ConfigConstants.DatabasePath]!;
        }
        
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMvc();

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        services.AddScoped<ApiKeyAuthFilter>();

        services.AddScoped<IMatrixService, MatrixService>();
        services.AddScoped<IColorService, ColorService>();
        services.AddScoped<IClockFaceService, ClockFaceService>();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        services.AddDbContext<MatrixContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Mode=ReadWriteCreate"));

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {
            config.SwaggerEndpoint("/swagger/v1/swagger.json", "Matrix API");
            config.RoutePrefix = string.Empty;
        });

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MatrixContext>();
            await context.Database.EnsureCreatedAsync();

            MatrixSeeder? seeder = null;
            if (configuration.GetValue<bool>(ConfigConstants.RunSeedOnStart))
            {
                seeder = new MatrixSeeder(context);
                await seeder.Seed(configuration.GetValue<bool>(ConfigConstants.SeedDrop), fontsPath);
            }

            if (seeder == null && args.Contains("--rebuild-fonts")) // SeedFonts is called within the seeder already if it was already ran
            {
                seeder = new MatrixSeeder(context);
                await seeder.SeedFonts(fontsPath);
            }
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        
        return app;
    }
    
    private static async Task<string> LoadOrGenerateApiKey(string apiKeyPath)
    {
        string apiKey;
        
        if (!File.Exists(apiKeyPath))
        {
            using (var newFile = File.Create(apiKeyPath))
            {
                using (var streamWriter = new StreamWriter(newFile))
                {
                    apiKey = Guid.NewGuid().ToString().Replace("-", "");
                    await streamWriter.WriteAsync(apiKey);
                }
            }
        }
        else
        {
            using (var keyFile = File.OpenRead(apiKeyPath))
            {
                using (var streamReader = new StreamReader(keyFile))
                {
                    apiKey = await streamReader.ReadToEndAsync();
                }
            }
        }
        
        return apiKey;
    }
}