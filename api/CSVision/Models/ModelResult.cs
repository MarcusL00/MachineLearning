using Microsoft.ML.Data;

namespace CSVision.Models
{
    /// <summary>
    /// Represents the result of a machine learning model training and evaluation.
    /// </summary>
    public class ModelResult
    {
        public string ModelName { get; set; } = string.Empty;
        public Dictionary<string, double> Metrics { get; set; } = new();
        public double[] PredictionValues { get; set; } = Array.Empty<double>(); // numeric values for plotting/HTML
        public double[] ActualValues { get; set; } = Array.Empty<double>();
        public ConfusionMatrix? ConfusionMatrix { get; set; }
    }
}
