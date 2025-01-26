using BdfFontParser;
using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices.Authentication;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.WebServices.Controllers;

[Route("image")]
[ApiKeyAuthFilter]
public class ImageController : MatrixBaseController
{
    private readonly ILogger<ImageController> _logger;

    public ImageController(ILogger<ImageController> logger)
    {
        _logger = logger;
    }

    [Produces("image/png")]
    [HttpGet("render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderCurrent()
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var clockFace = ProgramState.OverrideClockFace ?
                MatrixUpdater.OverridenClockFace : 
                MatrixMain.MatrixUpdater.CurrentClockFace;

            if (clockFace == null)
            {
                throw new ClockFaceException(WebConstants.ClockFaceNull);
            }

            var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);

            foreach (var textLine in clockFace.TextLines)
            {
                DrawTextLine(image, textLine);
            }

            await image.SaveAsPngAsync(Response.Body);
            
            return string.Empty;
        }));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixState>))]
    public async Task<IActionResult> SendImage()
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var decoderOptions = new DecoderOptions()
            {
                TargetSize = new Size(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight)
            };
            
            var postedImage = await Image.LoadAsync<Rgb24>(decoderOptions, Request.Body);

            if (postedImage.Height != MatrixUpdater.MatrixHeight || postedImage.Width != MatrixUpdater.MatrixWidth)
            {
                throw new InvalidImageException(WebConstants.InvalidImageSize);
            }

            ProgramState.Image = postedImage;
            
            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.Image;
            ProgramState.UpdateNextTick = true;

            return ProgramState.State;
        }));
    }

    private void DrawTextLine(Image<Rgb24> image, TextLine textLine)
    {
        var parsedLine = TextLineParser.ParseTextLine(textLine, ProgramState.CurrentVariables);
        
        BdfFont font = new BdfFont(textLine.Font.FileLocation);
        var mapping = font.GetMapOfString(parsedLine.ParsedText);
        
        var imageXOffset = parsedLine.XPosition;
        var imageYOffset = parsedLine.YPosition - textLine.Font.Height + 1;
        
        var textColor = new Rgb24(
            Convert.ToByte(textLine.Color.Red), 
            Convert.ToByte(textLine.Color.Green),
            Convert.ToByte(textLine.Color.Blue));
        
        for (int i = 0; i < mapping.GetLength(1); i++)
        {
            for (int j = 0; j < mapping.GetLength(0); j++)
            {
                if (mapping[j, i])
                {
                    image[j + imageXOffset, i + imageYOffset] = textColor;
                }
            }
        }
    }
}