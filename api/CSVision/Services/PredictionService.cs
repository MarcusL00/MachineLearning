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
        public async Task<ModelResult> GeneratePredictionsAsync(PredictionsRequestDto requestDto)
        {
            //var cleanedFile = _fileService.RemoveIdColumnAsync(requestDto.File);
            var machineLearningModel = MapToMachineLearningModel.MapToModel(
                requestDto.MachineLearningModel,
                requestDto.Features.ToArray(),
                requestDto.Target
            );

            ModelResult result = machineLearningModel.TrainModel(requestDto.File);

            return result;
        }
    }
}
