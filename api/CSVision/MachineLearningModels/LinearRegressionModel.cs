using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public sealed class LinearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        internal LinearRegressionModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );
        }

        protected override Dictionary<string, double> EvaluateModel(
            MLContext mlContext,
            IDataView predictions
        )
        {
            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            return new Dictionary<string, double>
            {
                { "RSquared", metrics.RSquared },
                { "MeanAbsoluteError", metrics.MeanAbsoluteError },
                { "MeanSquaredError", metrics.MeanSquaredError },
                { "RootMeanSquaredError", metrics.RootMeanSquaredError },
            };
        }

        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Single);
        }

        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);
    }
}
