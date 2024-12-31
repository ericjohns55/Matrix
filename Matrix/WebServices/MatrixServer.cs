using Matrix.Data;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices;

public static class MatrixServer
{
    public static string ApiKey = "test-key";
    
    public static async Task<WebApplication> CreateWebServer(string[] args, IConfigurationRoot configuration)
    {
        string dataFolderPath = Path.Combine(Environment.CurrentDirectory, "Data");
        
        string databasePath = Path.Combine(Environment.CurrentDirectory, "Data", "matrix.db");
        if (!string.IsNullOrWhiteSpace(configuration[ConfigConstants.DatabasePath]))
        {
            databasePath = configuration[ConfigConstants.DatabasePath]!;
        }

        string apiKeyPath = Path.Combine(dataFolderPath, "api_key");
        ApiKey = await LoadOrGenerateApiKey(apiKeyPath);
        
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMvc();

        services.AddScoped<ApiKeyAuthFilter>();

        services.AddScoped<IMatrixService, MatrixService>();
        services.AddScoped<IColorService, ColorService>();
        services.AddScoped<IClockFaceService, ClockFaceService>();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        services.AddDbContext<MatrixContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Mode=ReadWriteCreate"));

        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MatrixContext>();
            await context.Database.EnsureCreatedAsync();

            if (configuration.GetValue<bool>(ConfigConstants.RunSeedOnStart))
            {
                MatrixSeeder seeder = new MatrixSeeder(context);
                await seeder.Seed(configuration.GetValue<bool>(ConfigConstants.SeedDrop));
            }
        }

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