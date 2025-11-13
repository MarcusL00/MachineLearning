using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public class LogisticRegression : AbstractMachineLearningModel
    {
        internal override string ModelName => "Logistic Regression Model";

        public CalibratedBinaryClassificationMetrics TrainModel(IFormFile file)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Concatenate("Features", features)
                .Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));
            var model = pipeline.Fit(split.TrainSet);   


            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");


            // Model training logic will be implemented here
            return metrics;
        }
    }
}