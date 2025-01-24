using BdfFontParser;
using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
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

    [HttpGet("render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult RenderCurrent()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            throw new NotImplementedException("Not yet implemented");
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
}