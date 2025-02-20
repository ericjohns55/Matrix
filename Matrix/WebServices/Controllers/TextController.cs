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
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Controllers;

[Route("text")]
[ApiKeyAuthFilter]
public class TextController : MatrixBaseController
{
    private readonly MatrixContext _matrixContext;

    public TextController(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }

    [HttpGet("fonts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixListResponse<MatrixFont>))]
    public async Task<IActionResult> GetFonts()
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
            (await _matrixContext.MatrixFont.ToListAsync()).OrderBy(font => font.Width).ThenBy(font => font.Height)));
    }
    
    [HttpPost("plain")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<PlainText>))]
    public async Task<IActionResult> CreateText([FromBody] PlainTextPayload plainTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            if (plainTextPayload.Color == null)
            {
                plainTextPayload.Color = await LoadMatrixColorFromId(_matrixContext, plainTextPayload.MatrixColorId);
            }

            if (plainTextPayload.Font == null)
            {
                plainTextPayload.Font = await LoadMatrixFontFromId(_matrixContext, plainTextPayload.MatrixFontId);
            }
            
            ProgramState.PlainText = new PlainText(plainTextPayload);
            
            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.Text;
            ProgramState.UpdateNextTick = true;

            return ProgramState.PlainText;
        }));
    }

    [HttpPost("plain/render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderPlainText([FromBody] PlainTextPayload plainTextPayload, bool trimHeader = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            if (plainTextPayload.Color == null)
            {
                plainTextPayload.Color = await LoadMatrixColorFromId(_matrixContext, plainTextPayload.MatrixColorId);
            }

            if (plainTextPayload.Font == null)
            {
                plainTextPayload.Font = await LoadMatrixFontFromId(_matrixContext, plainTextPayload.MatrixFontId);
            }
            
            var plainText = new PlainText(plainTextPayload);
            
            return MatrixRenderer.ImageToBase64(MatrixRenderer.RenderPlainText(plainText, scaleFactor), trimHeader);
        }));
    }
    
    [HttpPost("scrolling")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ScrollingText>))]
    public async Task<IActionResult> CreateScrollingText([FromBody] ScrollingTextPayload scrollingTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            if (scrollingTextPayload.Color == null)
            {
                scrollingTextPayload.Color = await LoadMatrixColorFromId(_matrixContext, scrollingTextPayload.MatrixColorId);
            }

            if (scrollingTextPayload.Font == null)
            {
                scrollingTextPayload.Font = await LoadMatrixFontFromId(_matrixContext, scrollingTextPayload.MatrixFontId);
            }
            
            ProgramState.ScrollingText = new ScrollingText(scrollingTextPayload);

            ProgramState.PreviousState = ProgramState.State;
            ProgramState.State = MatrixState.ScrollingText;
            ProgramState.UpdateNextTick = true;

            return ProgramState.ScrollingText;
        }));
    }
    
    [HttpPost("scrolling/render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderPlainText([FromBody] ScrollingTextPayload scrollingTextPayload, bool trimHeader = false, int scaleFactor = 1, bool cropToMatrixSize = true)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async() =>
        {
            if (scrollingTextPayload.Color == null)
            {
                scrollingTextPayload.Color = await LoadMatrixColorFromId(_matrixContext, scrollingTextPayload.MatrixColorId);
            }

            if (scrollingTextPayload.Font == null)
            {
                scrollingTextPayload.Font = await LoadMatrixFontFromId(_matrixContext, scrollingTextPayload.MatrixFontId);
            }
            
            var scrollingText = new ScrollingText(scrollingTextPayload);
            
            return MatrixRenderer.ImageToBase64(MatrixRenderer.RenderScrollingText(scrollingText, scaleFactor, cropToMatrixSize), trimHeader);
        }));
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<MatrixState>))]
    public IActionResult CancelText()
    {
        return Ok(ExecuteToMatrixResponse(() =>
        {
            if (ProgramState.State == MatrixState.ScrollingText || ProgramState.State == MatrixState.Text)
            {
                if (ProgramState.PreviousState != MatrixState.Text &&
                    ProgramState.PreviousState != MatrixState.ScrollingText)
                {
                    (ProgramState.PreviousState, ProgramState.State) = (ProgramState.State, ProgramState.PreviousState);
                }
                else
                {
                    ProgramState.PreviousState = ProgramState.State;
                    ProgramState.State = MatrixState.Clock;
                }
                
                ProgramState.UpdateNextTick = true;
            }
            
            return ProgramState.State;
        }));
    }
}