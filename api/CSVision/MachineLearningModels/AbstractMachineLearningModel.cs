using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public abstract class AbstractMachineLearningModel
    {
        internal abstract string ModelName { get; }
        private protected string[] features { get; }


        private protected IDataView HandleCSV(IFormFile file)
        {
            var mlContext = new MLContext();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            string headerLine = reader.ReadLine();
            var headers = headerLine.Split(',');

            // Build schema dynamically
            var columns = headers.Select((h, i) =>
                new TextLoader.Column(h, DataKind.String, i)).ToArray();

            var textLoader = mlContext.Data.CreateTextLoader(new TextLoader.Options
            {
                Separators = new[] { ',' },
                HasHeader = true,
                Columns = columns
            });

            // Reset stream to beginning
            stream.Position = 0;
            return textLoader.Load((IMultiStreamSource)stream);
        }
    }
}