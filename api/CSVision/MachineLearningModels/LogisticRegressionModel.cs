using CSVision.Models;
using Microsoft.ML;

namespace CSVision.MachineLearningModels
{
    public sealed class LogisticRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Logistic Regression Model";

        public override ModelResult TrainModel(IFormFile file)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext
                .Transforms.Concatenate("Features", features)
                .Append(
                    mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                        labelColumnName: "Label",
                        featureColumnName: "Features"
                    )
                );
            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            // Model training logic will be implemented here
            return new ModelResult
            {
                ModelName = ModelName,
                TrainedModel = model,
                Metrics = new Dictionary<string, double>
                {
                    { "Accuracy", metrics.Accuracy },
                    { "AUC", metrics.AreaUnderRocCurve },
                    { "F1Score", metrics.F1Score },
                },
            };
        }
    }
}
