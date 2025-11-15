using CSVision.Models;
using Microsoft.ML;

namespace CSVision.MachineLearningModels
{
    public sealed class LogisticRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Logistic Regression Model";

        internal LogisticRegressionModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        public override ModelResult TrainModel(IFormFile file)
        {
            var mlContext = Seed == -1 ? new MLContext() : new MLContext(Seed);
            var dataView = CreateDataViewFromCsvFile(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = BuildFeaturePipeline(mlContext, dataView, Target)
                .Append(
                    mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                        labelColumnName: Target,
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
