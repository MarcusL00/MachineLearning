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
                // Generate predictions using the prediction service
                result = _predictionService.GeneratePredictions(requestDto);
            }
            catch (DataException ex)
            {
                // Handle csv row data-related exceptions and return an error HTML response
                return Content(HtmlUtilities.GenerateErrorHtmlResponse(ex.Message), "text/html");
            }
            catch
            {
                // Handle any other exceptions and return a generic error HTML response
                return Content(
                    HtmlUtilities.GenerateErrorHtmlResponse(
                        "An error occurred while processing the prediction."
                    ),
                    "text/html"
                );
            }

            // Generate graphs using the graph service
            byte[] graphImage = _graphService.GenerateGraph(result);

            byte[]? confusionMatrixImage = null;

            // Generate confusion matrix graph if requested
            if (requestDto.ConfusionMatrix == true)
            {
                confusionMatrixImage = _graphService.GenerateConfusionMatrixGraph(result);
            }

            // Generate the final HTML response and return it
            string html = HtmlUtilities.GenerateSuccessfulHtmlResponse(
                result,
                graphImage,
                confusionMatrixImage
            );

            return Content(html, "text/html");
        }
    }
}
