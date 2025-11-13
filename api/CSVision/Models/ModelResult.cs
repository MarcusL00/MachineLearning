using Microsoft.ML;

namespace CSVision.Models
{
    public class ModelResult
    {
        public string ModelName { get; set; }
        public ITransformer TrainedModel { get; set; }
        public Dictionary<string, double> Metrics { get; set; } = new();
    }
}
