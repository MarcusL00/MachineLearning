using CSVision.DTOs;
using CSVision.Interfaces;
using CSVision.Mapper;

namespace CSVision.Services
{
    public sealed class PredictionService : IPredictionService
    {
        private readonly IFileService _fileService;

        public PredictionService(IFileService fileService)
        {
            _fileService = fileService;
        }

        // TODO: Make this return something useful
        public async Task GeneratePredictionsAsync(PredictionsRequestDto requestDto)
        {
            var cleanedFile = _fileService.RemoveIdColumnAsync(requestDto.File);
            var machineLearningModel = MapToMachineLearningModel.MapToModel(
                requestDto.MachineLearningModel
            );

            machineLearningModel.TrainModel(cleanedFile);
        }
    }
}
