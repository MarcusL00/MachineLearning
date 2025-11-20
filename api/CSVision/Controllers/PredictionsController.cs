using System.Data;
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
        private readonly IGraphService _graphService;

        public PredictionsController(
            IPredictionService predictionService,
            IGraphService graphService
        )
        {
            _predictionService = predictionService;
            _graphService = graphService;
        }

        [HttpPost]
        public IActionResult Predict(PredictionsRequestDto requestDto)
        {
            ModelResult result;

            try
            {
                result = _predictionService.GeneratePredictions(requestDto);
            }
            catch (DataException ex)
            {
                return Content(HtmlUtilities.GenerateErrorHtmlResponse(ex.Message), "text/html");
            }
            catch
            {
                return Content(
                    HtmlUtilities.GenerateErrorHtmlResponse(
                        "An error occurred while processing the prediction."
                    ),
                    "text/html"
                );
            }

            byte[] graphImage = _graphService.GenerateGraph(result);

            byte[]? confusionMatrixImage = null;

            if (requestDto.ConfusionMatrix == true)
            {
                confusionMatrixImage = _graphService.GenerateConfusionMatrixGraph(result);
            }

            string html = HtmlUtilities.GenerateSuccessfulHtmlResponse(
                result,
                graphImage,
                confusionMatrixImage
            );

            return Content(html, "text/html");
        }
    }
}
