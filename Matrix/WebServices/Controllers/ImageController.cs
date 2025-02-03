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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult GetImageBase64(bool trimHeader = false)
    {
        return Ok(ExecuteToMatrixResponse(() =>
            MatrixRenderer.ImageToBase64(ProgramState.Image, trimHeader)));
    }
    
    [HttpGet("render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult RenderCurrent(bool trimHeader = false)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            Image<Rgb24>? image;

            switch (ProgramState.State)
            {
                case MatrixState.Clock:
                    var clockFace = ProgramState.OverrideClockFace ?
                        MatrixUpdater.OverridenClockFace : 
                        MatrixMain.MatrixUpdater.CurrentClockFace;
                    
                    image = MatrixRenderer.RenderClockFace(clockFace);
                    
                    break;
                case MatrixState.Timer:
                    image = MatrixRenderer.RenderClockFace(MatrixMain.MatrixUpdater.TimerClockFace);
                    
                    break;
                case MatrixState.Text:
                    image = MatrixRenderer.RenderPlainText(ProgramState.PlainText);
                    
                    break;
                case MatrixState.ScrollingText:
                    image = MatrixRenderer.RenderScrollingText(ProgramState.ScrollingText);

                    break;
                case MatrixState.Image:
                    image = ProgramState.Image;
                    
                    break;
                default:
                    image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);
                    break;
            }
            
            return MatrixRenderer.ImageToBase64(image, trimHeader);
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

    [HttpPost("base64")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixState>))]
    public IActionResult PostImageBase64([FromBody] Base64Payload imagePayload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (string.IsNullOrWhiteSpace(imagePayload.Base64Image))
            {
                throw new InvalidImageException(WebConstants.MissingImage);
            }

            var imageBytes = Convert.FromBase64String(imagePayload.Base64Image);
            var image = Image.Load<Rgb24>(imageBytes);

            if (image.Height != MatrixUpdater.MatrixHeight || image.Width != MatrixUpdater.MatrixWidth)
            {
                throw new InvalidImageException(WebConstants.InvalidImageSize);
            }

            ProgramState.Image = image;

            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.Image;
            ProgramState.UpdateNextTick = true;

            return ProgramState.State;
        }));
    }
    

}