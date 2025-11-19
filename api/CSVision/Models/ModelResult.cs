using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.Models
{
    public class ModelResult
    {
        public string ModelName { get; set; }
        public ITransformer TrainedModel { get; set; }
        public Dictionary<string, double> Metrics { get; set; } = new();
        public double[] Predictions { get; set; } = Array.Empty<double>(); // numeric values for plotting/HTML
        public double[] Actuals { get; set; } = Array.Empty<double>();
        // Optional: numeric values for a selected continuous feature used to plot logistic regressions
        public double[] FeatureValues { get; set; } = Array.Empty<double>();
        // Name of the feature used for plotting (if any)
        public string FeatureName { get; set; } = string.Empty;
        public ConfusionMatrix? ConfusionMatrix { get; set; }
        public double Coefficients
        {
            get; set;
        } = 0;
    }
}
