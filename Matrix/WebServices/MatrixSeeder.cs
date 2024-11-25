using Matrix.DataModels;

namespace Matrix.WebServices;

public class MatrixSeeder
{
    private readonly ILogger<MatrixSeeder> _logger;
    private MatrixContext _matrixContext;

    public MatrixSeeder(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
        
        _logger = new LoggerFactory().CreateLogger<MatrixSeeder>();
    }

    public async Task Seed(bool drop)
    {
        if (drop)
        {
            _matrixContext.TestData.RemoveRange(_matrixContext.TestData);
            
            await _matrixContext.SaveChangesAsync();
        }
        
        _logger.LogInformation("Seeding matrix context");

        _matrixContext.TestData.Add(new TestData() { Data = "Test data" });
        await _matrixContext.SaveChangesAsync();
    }
}