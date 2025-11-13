using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        internal static AbstractMachineLearningModel MapToModel(string modelType)
        {
            return modelType switch
            {
                "LinearRegression" => new LinearRegressionModel(),
                "LogisticRegression" => new LogisticRegressionModel(),
                "DecisionTree" => new DecisionTreeModel(),
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}
