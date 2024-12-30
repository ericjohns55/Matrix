using Matrix.Data;
using Matrix.WebServices.Services;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices;

public static class MatrixServer
{
    public static async Task<WebApplication> CreateWebServer(string[] args, IConfigurationRoot configuration)
    {
        string dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "matrix.db");
        if (!string.IsNullOrWhiteSpace(configuration[ConfigConstants.DatabasePath]))
        {
            dataPath = configuration[ConfigConstants.DatabasePath]!;
        }
        
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMvc();

        services.AddScoped<IMatrixService, MatrixService>();
        services.AddScoped<IClockFaceService, ClockFaceService>();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        services.AddDbContext<MatrixContext>(options =>
            options.UseSqlite($"Data Source={dataPath};Mode=ReadWriteCreate"));

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

        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}