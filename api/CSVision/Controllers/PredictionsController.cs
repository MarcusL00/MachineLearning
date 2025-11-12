using CSVision.DTOs;
using CSVision.Interfaces;
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
