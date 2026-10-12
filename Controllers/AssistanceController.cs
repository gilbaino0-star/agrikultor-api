using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MyPwaApi.Controllers
{
    [ApiController]
    [Route("api/assistance")]
    public class AssistanceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        
        public AssistanceController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Diperpanjang untuk proses gambar
            _geminiApiKey = config["GEMINI_API_KEY"] ?? string.Empty;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatWithAi([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question) && string.IsNullOrEmpty(request.ImageBase64))
            {
                return BadRequest(new { Answer = "Pergunta ka foto la bele mamuk." });
            }

            try
            {
                // System Instruction dengan aturan bahasa Tetum murni yang ketat
                string systemInstruction = $@"
O hatán nu'udar 'Agrikultor Matenek', espesialista agrikultura iha Timor-Leste.

REGRAS LINGUÍSTIKA NO FORMATU (OBLIGATÓRIU):
1. LIAN TETUM MURNI: Uza Lian Tetum Prasa ne'ebé moos no natural iha Timor-Leste.
2. KETAK KAHUR LIAN: Keta uza lian Indonézia ka Portugés ne'ebé todan.
   - UZA 'adubu' (labele 'pupuk')
   - UZA 'be'e' (labele 'air')
   - UZA 'ai-horis/ai-hun' (labele 'tanaman/pohon')
   - UZA 'oinsá' (labele 'bagaimana')
   - UZA 'moras' (labele 'penyakit')
   - UZA 'abut' (labele 'akar')
3. HATÁN DIREITU (TO THE POINT): Labele uza saudações/intro (Labele 'Bondia', dsb.).
4. BADAK NO KLARU: Fó kedas pasu-por-pasu ne'ebé fasil tuir.

DADOS SENSOR NO AI-HORIS:
- Tipo ai-horis: {request.Crop} ({request.Stage})
- Parameter: pH = {request.Ph:F1}, Umidade = {request.Moisture:F0}%, N = {request.N:F0} mg/kg, P = {request.P:F0} mg/kg, K = {request.K:F0} mg/kg.
";

                // Membangun daftar contents untuk mendukung memori riwayat chat & gambar
                var contentsList = new List<object>();

                if (request.History != null && request.History.Count > 0)
                {
                    foreach (var h in request.History)
                    {
                        var partsList = new List<object>();

                        // Jika ada gambar di riwayat tersebut
                        if (!string.IsNullOrEmpty(h.ImageBase64))
                        {
                            partsList.Add(new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = h.ImageBase64
                                }
                            });
                        }

                        if (!string.IsNullOrEmpty(h.Text))
                        {
                            partsList.Add(new { text = h.Text });
                        }

                        contentsList.Add(new
                        {
                            role = h.Role, // "user" atau "model"
                            parts = partsList.ToArray()
                        });
                    }
                }
                else
                {
                    // Fallback jika history kosong, masukkan pertanyaan baru
                    var currentParts = new List<object>();
                    if (!string.IsNullOrEmpty(request.ImageBase64))
                    {
                        currentParts.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = "image/jpeg",
                                data = request.ImageBase64
                            }
                        });
                    }
                    currentParts.Add(new { text = request.Question });

                    contentsList.Add(new
                    {
                        role = "user",
                        parts = currentParts.ToArray()
                    });
                }

                var payload = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemInstruction } }
                    },
                    contents = contentsList.ToArray(),
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 600
                    }
                };

                // Menggunakan model Gemini terbaru yang mendukung Vision (Analisis Foto) & Chat Cerdas
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={_geminiApiKey}";

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Ok(new { Answer = $"⚠️ Gemini API Error: {responseBody}" });
                }

                using var doc = JsonDocument.Parse(responseBody);
                var candidates = doc.RootElement.GetProperty("candidates");
                
                if (candidates.GetArrayLength() > 0)
                {
                    string answer = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "Deskulpa, la bele hetan resposta.";

                    return Ok(new { Answer = answer.Trim() });
                }

                return Ok(new { Answer = "Deskulpa, AI la fó resposta." });
            }
            catch (Exception ex)
            {
                return Ok(new { Answer = $"⚠️ Erru servidor: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Crop { get; set; } = "";
        public string Stage { get; set; } = "";
        public string Condition { get; set; } = "";
        public double Ph { get; set; }
        public double Moisture { get; set; }
        public double N { get; set; }
        public double P { get; set; }
        public double K { get; set; }
        public List<ChatHistoryItem>? History { get; set; }
        public string Question { get; set; } = "";
        public string? ImageBase64 { get; set; }
    }

    public class ChatHistoryItem
    {
        public string Role { get; set; } = "user";
        public string Text { get; set; } = "";
        public string? ImageBase64 { get; set; }
    }
}