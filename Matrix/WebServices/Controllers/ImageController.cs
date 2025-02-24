using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.WebServices.Authentication;
using Matrix.WebServices.Services;
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
    private readonly ImageService _imageService;

    public ImageController(ImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
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
    public IActionResult RenderCurrent(bool trimHeader = false, int scaleFactor = 1)
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
                    
                    image = MatrixRenderer.RenderClockFace(clockFace, scaleFactor);
                    
                    break;
                case MatrixState.Timer:
                    image = MatrixRenderer.RenderClockFace(MatrixMain.MatrixUpdater.TimerClockFace, scaleFactor);
                    
                    break;
                case MatrixState.Text:
                    image = MatrixRenderer.RenderPlainText(ProgramState.PlainText, scaleFactor);
                    
                    break;
                case MatrixState.ScrollingText:
                    image = MatrixRenderer.RenderScrollingText(ProgramState.ScrollingText, scaleFactor);

                    break;
                case MatrixState.Image:
                    image = MatrixRenderer.RenderImage(ProgramState.Image, scaleFactor);
                    
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

            SetImageState(postedImage);

            return ProgramState.State;
        }));
    }

    [HttpPost("base64")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixState>))]
    public IActionResult PostImageBase64([FromBody] ImagePayload imagePayload)
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (string.IsNullOrWhiteSpace(imagePayload.Base64Image))
            {
                throw new InvalidImageException(WebConstants.MissingImage);
            }

            var imageBytes = Convert.FromBase64String(imagePayload.Base64Image);
            var image = Image.Load<Rgb24>(imageBytes);

            SetImageState(image);
            
            return ProgramState.State;
        }));
    }

    private void SetImageState(Image<Rgb24> image)
    {
        if (image.Height != MatrixUpdater.MatrixHeight || image.Width != MatrixUpdater.MatrixWidth)
        {
            throw new InvalidImageException(WebConstants.InvalidImageSize);
        }

        ProgramState.Image = image;

        ProgramState.PreviousState = ProgramState.State;
        ProgramState.State = MatrixState.Image;
        ProgramState.UpdateNextTick = true;
    }

    [HttpPost("base64/scale")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public IActionResult ScaleImageBase64([FromBody] ImagePayload imagePayload, int scaleFactor = 1)
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

            var resultImage = MatrixRenderer.RenderImage(image, scaleFactor);

            return MatrixRenderer.ImageToBase64(resultImage);
        }));
    }

    [HttpGet("saved")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<List<SavedImage>>))]
    public async Task<IActionResult> GetSavedImages(int scaleFactor = 0, bool trimHeader = false)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _imageService.GetSavedImages(scaleFactor, trimHeader)));
    }

    [HttpPost("save")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<SavedImage>))]
    public async Task<IActionResult> SaveImage([FromBody] ImagePayload imagePayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _imageService.SaveImage(imagePayload)));
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<SavedImage>))]
    public async Task<IActionResult> GetImageById(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _imageService.GetImageById(id)));
    }

    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<SavedImage>))]
    public async Task<IActionResult> SetImageById(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            var image = await _imageService.GetImageById(id, true);

            try
            {
                SetImageState(image.Image!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new InvalidImageException(WebConstants.ImageLoadFailed);
            }

            return image;
        }));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<SavedImage>))]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] ImagePayload imagePayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _imageService.UpdateImage(id, imagePayload)));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<SavedImage>))]
    public async Task<IActionResult> DeleteImage(int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _imageService.DeleteImage(id)));
    }
}