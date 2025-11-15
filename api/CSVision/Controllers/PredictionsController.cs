using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.Models;
using CSVision.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace CSVision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PredictionsController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionsController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost]
        public IActionResult Predict(PredictionsRequestDto requestDto)
        {
            ModelResult result = _predictionService.GeneratePredictionsAsync(requestDto);
            string html = HtmlUtilities.GenerateHtmlResponse(result);

            return Content(html, "text/html");
        }
    }
}
