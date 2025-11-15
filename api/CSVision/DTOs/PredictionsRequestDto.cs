using Microsoft.AspNetCore.Mvc;

namespace CSVision.DTOs
{
    public class PredictionsRequestDto
    {
        [FromForm(Name = "csv_file")]
        public IFormFile File { get; set; }

        [FromForm(Name = "model")]
        public string MachineLearningModel { get; set; }

        [FromForm(Name = "features[]")]
        public List<string> Features { get; set; }

        [FromForm(Name = "target")]
        public string Target { get; set; }

        [FromForm(Name = "seed")]
        public int Seed { get; set; }

        [FromForm(Name = "confusion_matrix")]
        public bool ConfusionMatrix { get; set; }
    }
}
