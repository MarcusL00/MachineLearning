using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.Models;
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
            ModelResult result = _predictionService.GeneratePredictionsAsync(requestDto);

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
