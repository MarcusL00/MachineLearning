using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.MachineLearningModels;
using Microsoft.AspNetCore.Mvc;

namespace CSVision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PredictionsController : ControllerBase
    {
        private readonly IPredictionService _predictionService;
        private readonly IFileService _fileService;

        public PredictionsController(IPredictionService predictionService, IFileService fileService)
        {
            _predictionService = predictionService;
            _fileService = fileService;
        }

        [HttpPost]
        public IActionResult Predict(PredictionsRequestDto requestDto)
        {
            var cleanedFile = _fileService.RemoveIdColumnAsync(requestDto.File);
            var whack = new linearRegressionModel();
            whack.TrainModel(cleanedFile);
            // _predictionService

            var html =
                $@"
                <html>
                    <body>
                        <h2>File Uploaded</h2>
                    </body>
                </html>";

            return Content(html, "text/html");
        }
    }
}
