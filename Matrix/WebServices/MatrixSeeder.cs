using Matrix.Data.Models;

namespace Matrix.WebServices;

public class MatrixSeeder
{
    private readonly ILogger<MatrixSeeder> _logger;
    private readonly MatrixContext _matrixContext;

    public MatrixSeeder(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
        
        _logger = new LoggerFactory().CreateLogger<MatrixSeeder>();
    }

    public async Task Seed(bool drop)
    {
        _logger.LogInformation("Seeding matrix ...");
        if (drop)
        {
            _logger.LogInformation("Dropping data ...");
            
            _matrixContext.ClockFace.RemoveRange(_matrixContext.ClockFace);
            _matrixContext.TextLine.RemoveRange(_matrixContext.TextLine);
            _matrixContext.MatrixFont.RemoveRange(_matrixContext.MatrixFont);
            _matrixContext.MatrixColor.RemoveRange(_matrixContext.MatrixColor);
            _matrixContext.TimePeriod.RemoveRange(_matrixContext.TimePeriod);
            _matrixContext.Timer.RemoveRange(_matrixContext.Timer);
            
            await _matrixContext.SaveChangesAsync();
            _logger.LogInformation("Drop complete.");
        }
        
        _logger.LogInformation("Seeding matrix colors ...");
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "red", Red = 128, Green = 0, Blue = 0 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "orange", Red = 253, Green = 88, Blue = 0 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "yellow", Red = 255, Green = 228, Blue = 0 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "green", Red = 0, Green = 160, Blue = 0 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "blue", Red = 0, Green = 64, Blue = 255 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "purple", Red = 128, Green = 0, Blue = 128 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "pink", Red = 255, Green = 0, Blue = 255 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "white", Red = 255, Green = 255, Blue = 255 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "gray", Red = 128, Green = 128, Blue = 128 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "black", Red = 0, Green = 0, Blue = 0 });
        _matrixContext.MatrixColor.Add(new MatrixColor() { Name = "brown", Red = 101, Green = 67, Blue = 33 });
        await _matrixContext.SaveChangesAsync();
        
    }
}