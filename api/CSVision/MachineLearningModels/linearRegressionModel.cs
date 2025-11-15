using System.Linq;
using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public sealed class LinearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        internal LinearRegressionModel(string[] features, string target)
            : base(features, target) { }

        public override ModelResult TrainModel(IFormFile file)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Build the feature + label conversion estimator (without the trainer) so we can
            // inspect the transformed label column before attempting to train. This avoids
            // the opaque "½set has 0 instances" error when label conversion drops all rows.
            var baseEstimator = BuildFeaturePipeline(mlContext, dataView, Target)
                .Append(
                    mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Single)
                );

            // Fit the base estimator and transform the training set to inspect labels
            var baseTransformer = baseEstimator.Fit(split.TrainSet);
            var transformedTrain = baseTransformer.Transform(split.TrainSet);

            // Extract the converted Label column values
            var labelValues = transformedTrain.GetColumn<float>("Label").ToArray();

            var totalTrainRows = labelValues.Length;
            var validLabelCount = labelValues.Count(v => !float.IsNaN(v));

            // Train only on the already-transformed training data to avoid re-applying
            // and potentially changing the base transformer during Fit. This also gives
            // us control to validate the transformed features/labels before training.
            var trainerEstimator = mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );

            ITransformer trainerTransformer;
            try
            {
                trainerTransformer = trainerEstimator.Fit(transformedTrain);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Trainer.Fit exception: " + ex.Message);
                try
                {
                    var preview = transformedTrain.Preview(maxRows: 10);
                    Console.WriteLine("--- Transformed train preview ---");
                    foreach (var col in preview.ColumnView)
                    {
                        Console.WriteLine(
                            $"Column: {col.Column.Name} (Type: {col.Column.Type}) Values: {string.Join(",", col.Values.Select(v => v?.ToString() ?? "<null>"))}"
                        );
                    }
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Failed to preview transformed train: " + ex2.Message);
                }

                throw;
            }

            // Transform test set using the base transformer, then run the trainer transformer
            var transformedTest = baseTransformer.Transform(split.TestSet);
            var predictions = trainerTransformer.Transform(transformedTest);

            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            // Compose the final model: baseTransformer followed by trainer transformer
            var model = baseTransformer.Append(trainerTransformer);

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
