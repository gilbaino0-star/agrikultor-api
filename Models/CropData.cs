using Microsoft.ML.Data;

namespace MyPwaApp.Api.Models
{
    public class CropInput
    {
        [LoadColumn(0)] public float N { get; set; }
        [LoadColumn(1)] public float P { get; set; }
        [LoadColumn(2)] public float K { get; set; }
        [LoadColumn(3)] public float Temperature { get; set; }
        [LoadColumn(4)] public float Humidity { get; set; }
        [LoadColumn(5)] public float Ph { get; set; }
        [LoadColumn(6)] public string Label { get; set; } = string.Empty;
    }

    public class CropPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedCrop { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }
}