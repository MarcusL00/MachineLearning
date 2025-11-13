using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        internal static AbstractMachineLearningModel MapToModel(
            string modelType,
            string[] features,
            string[] targets
        )
        {
            return modelType switch
            {
                "LinearRegression" => new LinearRegressionModel(features, targets),
                "LogisticRegression" => new LogisticRegressionModel(features, targets),
                "DecisionTree" => new DecisionTreeModel(features, targets),
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}
