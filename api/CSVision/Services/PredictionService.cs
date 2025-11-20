using System.Data;
using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.Mapper;
using CSVision.Models;

namespace CSVision.Services
{
    public sealed class PredictionService : IPredictionService
    {
        private readonly IFileService _fileService;

        public PredictionService(IFileService fileService)
        {
            _fileService = fileService;
        }

        // TODO: Change return so it also returns a graph
        public ModelResult GeneratePredictions(PredictionsRequestDto requestDto)
        {
            var cleanedFile = _fileService.CleanseCsvFile(requestDto.File);

            uint minimumLines = 101; // 100 data rows + 1 header row
            if (!_fileService.IsValidLength(cleanedFile, minimumLines))
            {
                throw new DataException($"CSV file must contain at least {minimumLines} rows.");
            }

            var machineLearningModel = MapToMachineLearningModel.MapToModel(
                requestDto.MachineLearningModel,
                requestDto.Features.ToArray(),
                requestDto.Target,
                requestDto.Seed
            );

            ModelResult result = machineLearningModel.TrainModel(cleanedFile);

            return result;
        }
    }
}
