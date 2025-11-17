using Microsoft.ML;

namespace CSVision.Models
{
    public class ModelResult
    {
        public string ModelName { get; set; }
        public ITransformer TrainedModel { get; set; }
        public Dictionary<string, double> Metrics { get; set; } = new();
        public double[] Predictions { get; set; } = Array.Empty<double>(); // numeric values for plotting/HTML
        public double[] Actuals { get; set; } = Array.Empty<double>();
    }
}
