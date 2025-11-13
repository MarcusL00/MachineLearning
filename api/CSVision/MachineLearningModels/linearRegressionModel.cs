using CSVision.Models;
using Microsoft.ML;

namespace CSVision.MachineLearningModels
{
    public sealed class LinearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        public override ModelResult TrainModel(IFormFile file)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext
                .Transforms.Concatenate("Features", features)
                .Append(
                    mlContext.Regression.Trainers.Sdca(
                        labelColumnName: "Label",
                        featureColumnName: "Features"
                    )
                );

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            return new ModelResult
            {
                ModelName = ModelName,
                TrainedModel = model,
                Metrics = new Dictionary<string, double>
                {
                    { "RSquared", metrics.RSquared },
                    { "MeanAbsoluteError", metrics.MeanAbsoluteError },
                    { "MeanSquaredError", metrics.MeanSquaredError },
                    { "RootMeanSquaredError", metrics.RootMeanSquaredError },
                },
            };
        }
    }
}
