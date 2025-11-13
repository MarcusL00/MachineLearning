using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        internal static AbstractMachineLearningModel MapToModel(string modelType)
        {
            return modelType.ToLower() switch
            {
                "LinearRegression" => new LinearRegressionModel(),
                "LogisticRegression" => new LogisticRegressionModel(),
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}
