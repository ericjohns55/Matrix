using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.Utilities;

namespace MatrixTests;

public class TextLineParserTests : MatrixTestBase
{
    private readonly MatrixColor _color = new MatrixColor() { Red = 255, Green = 255, Blue = 255 };
    private const string _fontSmall = "5x8";
    private const string _fontMedium = "8x10";
    private const string _fontLarge = "10x20";

    private const string _forecastTiny = "XY";
    private const string _forecastShort = "Test";
    private const string _forecast = "This is a long sentence that will be trimmed during parsing";

    private const int _forecastTinyLength = 2;
    private const int _forecastShortLength = 4;

    private Dictionary<string, string> _currentVariables;

    [OneTimeSetUp]
    public void Setup()
    {
        MatrixUpdater.MatrixWidth = 64;
        MatrixUpdater.MatrixHeight = 64;

        _currentVariables = new Dictionary<string, string>()
        {
            { VariableConstants.ForecastCurrentShortVariable, _forecastShort },
            { VariableConstants.ForecastCurrentVariable, _forecast },
            { VariableConstants.ForecastDay, _forecastTiny }
        };
    }

    [Test]
    [TestCase(_fontSmall, 22, _forecastShortLength)]
    [TestCase(_fontMedium, 16, _forecastShortLength)]
    [TestCase(_fontLarge, 12, _forecastShortLength)]
    public void ParseText_CenterX_ValidWidth(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 2, 12)]
    [TestCase(_fontMedium, 0, 8)]
    [TestCase(_fontLarge, 2, 6)]
    public void ParseText_CenterX_LongString(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecast.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 11, _forecastTinyLength)]
    [TestCase(_fontMedium, 8, _forecastTinyLength)]
    [TestCase(_fontLarge, 6, _forecastTinyLength)]
    public void ParseText_CenterLeftX_ValidWidth(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastDay,
            XPositioning = Positioning.XPositioning.CenterLeft,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastTiny.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 1, 6)]
    [TestCase(_fontMedium, 0, 4)]
    [TestCase(_fontLarge, 1, 3)]
    public void ParseText_CenterLeftX_LongString(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentVariable,
            XPositioning = Positioning.XPositioning.CenterLeft,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecast.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 43, _forecastTinyLength)]
    [TestCase(_fontMedium, 40, _forecastTinyLength)]
    [TestCase(_fontLarge, 38, _forecastTinyLength)]
    public void ParseText_CenterRightX_ValidWidth(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastDay,
            XPositioning = Positioning.XPositioning.CenterRight,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize },
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastTiny.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 33, 6)]
    [TestCase(_fontMedium, 32, 4)]
    [TestCase(_fontLarge, 33, 3)]
    public void ParseText_CenterRightX_LongString(string fontSize, int expectedXPos, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentVariable,
            XPositioning = Positioning.XPositioning.CenterRight,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecast.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(expectedXPos));
    }

    [Test]
    [TestCase(_fontSmall, 10, _forecastShortLength)]
    [TestCase(_fontMedium, 10, _forecastShortLength)]
    [TestCase(_fontLarge, 10, _forecastShortLength)]
    public void ParseText_AbsoluteX_ValidWidth(string fontSize, int xLocation, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Absolute,
            YPositioning = Positioning.YPositioning.Center,
            XLocation = xLocation,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(xLocation));
    }

    [Test]
    [TestCase(_fontSmall, 10, 10)]
    [TestCase(_fontMedium, 10, 6)]
    [TestCase(_fontLarge, 10, 5)]
    public void ParseText_AbsoluteX_LongString(string fontSize, int xLocation, int expectedTextLength)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentVariable,
            YPositioning = Positioning.YPositioning.Center,
            XPositioning = Positioning.XPositioning.Absolute,
            XLocation = xLocation,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };

        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);

        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecast.Substring(0, expectedTextLength)));
        Assert.That(result.ParsedText.Length, Is.EqualTo(expectedTextLength));
        Assert.That(result.XPosition, Is.EqualTo(xLocation));
    }

    [Test]
    [TestCase(_fontSmall, 36)]
    [TestCase(_fontMedium, 37)]
    [TestCase(_fontLarge, 42)]
    public void ParseText_CenterY(string fontSize, int expectedYPos)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.Center,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };
        
        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);
        
        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort));
        Assert.That(result.YPosition, Is.EqualTo(expectedYPos));
    }

    [Test]
    [TestCase(_fontSmall, 20)]
    [TestCase(_fontMedium, 21)]
    [TestCase(_fontLarge, 26)]
    public void ParseText_TopQuarterY(string fontSize, int expectedYPos)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.TopQuarter,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };
        
        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);
        
        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort));
        Assert.That(result.YPosition, Is.EqualTo(expectedYPos));
    }

    [Test]
    [TestCase(_fontSmall, 52)]
    [TestCase(_fontMedium, 53)]
    [TestCase(_fontLarge, 58)]
    public void ParseText_BottomQuarterY(string fontSize, int expectedYPos)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.BottomQuarter,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };
        
        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);
        
        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort));
        Assert.That(result.YPosition, Is.EqualTo(expectedYPos));
    }

    [Test]
    [TestCase(_fontSmall, 20)]
    [TestCase(_fontMedium, 20)]
    [TestCase(_fontLarge, 20)]
    public void ParseText_AbsoluteY(string fontSize, int expectedYPos)
    {
        // Arrange
        var textLine = new TextLine()
        {
            Text = VariableConstants.ForecastCurrentShortVariable,
            XPositioning = Positioning.XPositioning.Center,
            YPositioning = Positioning.YPositioning.Absolute,
            YLocation = expectedYPos,
            Color = _color,
            Font = new MatrixFont() { Name = fontSize }
        };
        
        // Act
        var result = TextLineParser.ParseTextLine(textLine, _currentVariables);
        
        // Assert
        Assert.That(result.ParsedText, Is.EqualTo(_forecastShort));
        Assert.That(result.YPosition, Is.EqualTo(expectedYPos));
    }
}