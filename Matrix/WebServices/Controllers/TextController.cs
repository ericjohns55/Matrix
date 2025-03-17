using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
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
    private readonly TextService _textService;

    public TextController(MatrixContext matrixContext, TextService textService)
    {
        _matrixContext = matrixContext;
        _textService = textService;
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
            await _textService.LoadPlainTextDependencies(plainTextPayload);

            return PostPlainTextState(plainTextPayload);
        }));
    }

    [HttpGet("plain/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<PlainText>))]
    public async Task<IActionResult> GetPlainText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.GetPlainTextById(id)));
    }

    [HttpPut("plain/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<PlainText>))]
    public async Task<IActionResult> UpdateText([FromRoute] int id, [FromBody] PlainTextPayload plainTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.UpdatePlainText(id, plainTextPayload)));
    }

    [HttpPost("plain/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<PlainText>))]
    public async Task<IActionResult> CreateText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
            PostPlainTextState(await _textService.GetPlainTextById(id, true))));
    }

    [HttpDelete("plain/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<int>))]
    public async Task<IActionResult> DeletePlainText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.DeletePlainText(id)));
    }

    [HttpPost("plain/save")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<PlainTextPayload>))]
    public async Task<IActionResult> SavePlainText([FromBody] PlainTextPayload plainTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.SavePlainText(plainTextPayload)));
    }

    [HttpGet("plain")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<List<PlainTextPayload>>))]
    public async Task<IActionResult> GetAllSavedPlainText(bool trimHeader = false)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.GetSavedPlainText(trimHeader)));
    }

    [HttpPost("plain/render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderPlainText([FromBody] PlainTextPayload plainTextPayload, bool trimHeader = false, int scaleFactor = 1)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
        {
            await _textService.LoadPlainTextDependencies(plainTextPayload);
            
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
            await _textService.LoadScrollingTextDependencies(scrollingTextPayload);

            return PostScrollingTextState(scrollingTextPayload);
        }));
    }

    [HttpGet("scrolling/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ScrollingTextPayload>))]
    public async Task<IActionResult> GetScrollingText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.GetScrollingTextById(id)));
    }

    [HttpPut("scrolling/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ScrollingTextPayload>))]
    public async Task<IActionResult> UpdateScrollingText([FromRoute] int id, [FromBody] ScrollingTextPayload scrollingTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.UpdateScrollingText(id, scrollingTextPayload)));
    }
    
    [HttpPost("scrolling/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<ScrollingText>))]
    public async Task<IActionResult> CreateScrollingText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async () =>
            PostScrollingTextState(await _textService.GetScrollingTextById(id, true))));
    }

    [HttpDelete("scrolling/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<int>))]
    public async Task<IActionResult> DeleteScrollingText([FromRoute] int id)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.DeleteScrollingText(id)));
    }

    [HttpPost("scrolling/save")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<List<ScrollingTextPayload>>))]
    public async Task<IActionResult> SaveScrollingText([FromBody] ScrollingTextPayload scrollingTextPayload)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.SaveScrollingText(scrollingTextPayload)));
    }

    [HttpGet("scrolling")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<List<ScrollingTextPayload>>))]
    public async Task<IActionResult> GetAllScrollingText(bool trimHeader = false)
    {
        return Ok(await ExecuteToMatrixResponseAsync(() => _textService.GetSavedScrollingText(trimHeader)));
    }
    
    [HttpPost("scrolling/render")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatrixResponse<string>))]
    public async Task<IActionResult> RenderPlainText([FromBody] ScrollingTextPayload scrollingTextPayload, bool trimHeader = false, int scaleFactor = 1, bool cropToMatrixSize = true)
    {
        return Ok(await ExecuteToMatrixResponseAsync(async() =>
        {
            await _textService.LoadScrollingTextDependencies(scrollingTextPayload);
            
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
    
    private PlainText PostPlainTextState(PlainTextPayload plainTextPayload)
    {
        ProgramState.PlainText = new PlainText(plainTextPayload);
            
        ProgramState.PreviousState = ProgramState.State;
        ProgramState.State = MatrixState.Text;
        ProgramState.UpdateNextTick = true;
        
        return ProgramState.PlainText;
    }

    private ScrollingText PostScrollingTextState(ScrollingTextPayload scrollingTextPayload)
    {
        ProgramState.ScrollingText = new ScrollingText(scrollingTextPayload);

        ProgramState.PreviousState = ProgramState.State;
        ProgramState.State = MatrixState.ScrollingText;
        ProgramState.UpdateNextTick = true;

        return ProgramState.ScrollingText;
    }
}