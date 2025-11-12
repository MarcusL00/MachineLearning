using CSVision.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CSVision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionsController : ControllerBase
    {
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
