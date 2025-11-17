using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.Models;
using CSVision.Services;
using CSVision.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace CSVision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PredictionsController : ControllerBase
    {
        private readonly IPredictionService _predictionService;
        private readonly IGraphService _graphService;

        public PredictionsController(IPredictionService predictionService, IGraphService graphService)
        {
            _predictionService = predictionService;
            _graphService = graphService;
        }

        [HttpPost]
        public IActionResult Predict(PredictionsRequestDto requestDto)
        {
            ModelResult result = _predictionService.GeneratePredictionsAsync(requestDto);

            byte[] graphImage = _graphService.GeneratePredictionGraph(result);

            string html = HtmlUtilities.GenerateHtmlResponse(result, graphImage);

            return Content(html, "text/html");
        }
    }
}
