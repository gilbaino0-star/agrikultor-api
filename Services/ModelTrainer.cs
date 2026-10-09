using Microsoft.ML;
using MyPwaApp.Api.Models;

namespace MyPwaApp.Api.Services
{
    public static class ModelTrainer
    {
        public static void EnsureModelExists(string modelPath)
        {
            if (File.Exists(modelPath)) return;

            var mlContext = new MLContext(seed: 0);

            // Sample dataset pelatihan standar agrikultur
            var sampleData = new List<CropInput>
            {
                // Rice (Padi) - Butuh air/kelembaban tinggi, pH netral
                new() { N = 90, P = 42, K = 43, Temperature = 20.8f, Humidity = 82.0f, Ph = 6.5f, Label = "Rice" },
                new() { N = 85, P = 58, K = 41, Temperature = 21.7f, Humidity = 80.3f, Ph = 7.0f, Label = "Rice" },

                // Maize (Jagung) - N sedang-tinggi, kelembaban sedang
                new() { N = 75, P = 48, K = 20, Temperature = 22.6f, Humidity = 65.0f, Ph = 6.2f, Label = "Maize" },
                new() { N = 80, P = 50, K = 22, Temperature = 23.8f, Humidity = 62.1f, Ph = 5.9f, Label = "Maize" },

                // Coffee (Kopi) - Suhu sejuk/sedang, kelembaban cukup, N & K baik
                new() { N = 100, P = 28, K = 30, Temperature = 25.5f, Humidity = 58.0f, Ph = 6.8f, Label = "Coffee" },

                // Tomato (Tomat) - pH agak asam (6.0 - 6.8), N, P, K seimbang
                new() { N = 60, P = 55, K = 50, Temperature = 24.0f, Humidity = 70.0f, Ph = 6.3f, Label = "Tomato" },

                // Chili (Cabai) - Butuh N & K cukup, pH 6.0 - 7.0, hangat
                new() { N = 70, P = 40, K = 45, Temperature = 27.0f, Humidity = 68.0f, Ph = 6.5f, Label = "Chili" }
            };

            var trainingData = mlContext.Data.LoadFromEnumerable(sampleData);

            // Pipeline pelatihan Multiclass Classification
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Concatenate("Features", nameof(CropInput.N), nameof(CropInput.P), nameof(CropInput.K), nameof(CropInput.Temperature), nameof(CropInput.Humidity), nameof(CropInput.Ph)))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(trainingData);

            // Simpan model ke file .zip
            var directory = Path.GetDirectoryName(modelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            mlContext.Model.Save(model, trainingData.Schema, modelPath);
        }
    }
}