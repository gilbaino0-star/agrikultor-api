using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using MyPwaApp.Api.Models;
using MyPwaApp.Api.Services;

namespace MyPwaApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly string _modelPath;

        public RecommendationController(IWebHostEnvironment env)
        {
            _modelPath = Path.Combine(env.ContentRootPath, "data", "crop_recommendation_model.zip");
            
            // Generate model otomatis jika belum ada file .zip nya
            ModelTrainer.EnsureModelExists(_modelPath);
        }

        [HttpPost("predict")]
        public IActionResult Predict([FromBody] CropInput input)
        {
            var mlContext = new MLContext();
            
            if (!System.IO.File.Exists(_modelPath))
            {
                return BadRequest(new { message = "File model ML tidak ditemukan." });
            }

            var mlModel = mlContext.Model.Load(_modelPath, out _);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<CropInput, CropPrediction>(mlModel);

            var result = predictionEngine.Predict(input);

            return Ok(new
            {
                predictedCrop = result.PredictedCrop,
                score = result.Score
            });
        }
    }
}