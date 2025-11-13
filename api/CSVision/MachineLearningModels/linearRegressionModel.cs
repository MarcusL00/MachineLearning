using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public class linearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        public RegressionMetrics TrainModel(IFormFile trainingData)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(trainingData);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Concatenate("Features", features)
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            return metrics;

        }

    }
}