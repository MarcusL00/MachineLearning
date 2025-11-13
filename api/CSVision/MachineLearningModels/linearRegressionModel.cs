using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public class linearRegressionModel : AbstractMachineLearningModel
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        internal override string ModelName => "Linear Regression Model";

    }
}