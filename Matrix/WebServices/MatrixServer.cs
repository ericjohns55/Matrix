using Matrix.WebServices.Services;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices;

public static class MatrixServer
{
    private static readonly bool SEED_DATA = true;
    
    public static async Task<WebApplication> CreateWebServer(string[] args)
    {
        // TODO configuration builder
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMvc();

        services.AddScoped<IMatrixService, MatrixService>();
        
        // TODO move matrix.db into Data and move DataModels to Data/Models
        services.AddDbContext<MatrixContext>(options =>
            options.UseSqlite("Data Source=matrix.db"));

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

            if (SEED_DATA)
            {
                MatrixSeeder seeder = new MatrixSeeder(context);
                await seeder.Seed(false);
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