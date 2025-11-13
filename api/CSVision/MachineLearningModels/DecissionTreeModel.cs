using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace CSVision.MachineLearningModels
{
    public class DecissionTreeModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Decision Tree Model";
        /// <summary>
        /// Train a binary decision-tree style model using FastTree (gradient boosted trees).
        /// Expects the incoming CSV to include a "Label" column and feature columns that match the
        /// inherited <c>features</c> list.
        /// </summary>
        public CalibratedBinaryClassificationMetrics TrainModel(IFormFile file)
        {
            var mlContext = new MLContext();
            var dataView = HandleCSV(file);

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Concatenate("Features", features)
                .Append(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            return metrics;
        }
    }
}