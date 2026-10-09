using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MyPwaApi.Controllers
{
    [ApiController]
    [Route("api/assistance")]
    public class AssistanceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        
        // Gemini API Key dari Google AI Studio (Gratis)
        private const string GeminiApiKey = "AQ.Ab8RN6IjxGCLqNM4nwYMKPJzNkO63v3CjOVnC5rNA0qOaVSKWA";

        public AssistanceController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatWithAi([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { Answer = "Pergunta la bele mamuk." });
            }

            try
            {
                // System Instruction dengan aturan bahasa Tetum murni & kamus larangan
                string systemInstruction = $@"
O hatán nu'udar 'Agrikultor Matenek', espesialista agrikultura iha Timor-Leste.

REGRAS LINGUÍSTIKA NO FORMATU (OBLIGATÓRIU):
1. LIAN TETUM MURNI: Uza Lian Tetum Prasa ne'ebé moos no natural iha Timor-Leste.
2. KETAK KAHUR LIAN: Keta uza lian Indonézia ka Portugés ne'ebé todan.
   - KETA uza 'pupuk' -> UZA 'adubu'
   - KETA uza 'air' -> UZA 'be'e'
   - KETA uza 'tanaman/pohon' -> UZA 'ai-horis/ai-hun'
   - KETA uza 'bagaimana' -> UZA 'oinsá'
   - KETA uza 'sore' -> UZA 'lokraik'
   - KETA uza 'pagi' -> UZA 'dadeer'
   - KETA uza 'campur' -> UZA 'kahur / mistura'
   - KETA uza 'hancur/larut' -> UZA 'nabeen'
   - KETA uza 'Aduka' -> UZA 'kahur'
   - KETA uza 'hajar' -> UZA 'halo'
   - KETA uza 'burut' -> UZA 'buras'
   - KETA uza ' podridão -> UZA 'dodok'
   - KETA uza 'Hama no Penyakit -> UZA 'pesti no moras'
   - KETA uza 'musan-aat' -> UZA 'pesti'
   - KETA uza 'Raíz' -> UZA 'abut'
   - KETA uza 'flor ono fuan' -> UZA 'funan no fuan'
   - KETA uza 'Loran' -> UZA 'Loron'
   - KETA uza 'loran' -> UZA 'loron-matan'
   - KETA UZA 'gunting' -> UZA 'tizoura'
   - KETA UZA 'dader-raan' -> UZA 'dader-saan'
   - Keta Uza 'dadous' -> UZA 'dadus'
   - KETA UZA 'rumput' -> UZA 'duut'
   - KETA UZA 'saku Nutrisaun' -> UZA 'supa/absorbe nutrisaun'
   - KETA UZA 'uiteik' -> UZA 'uituan'
   - KETA UZA 'matan-zook' -> UZA 'tau matan/ kuidadu'
   - LABELE UZA 'preparation' -> UZA 'preparasaun'
   - LABELE UZA 'musan ka biban' -> UZA 'musan kah lis nia isin'
   - LABELE UZA 'tempu kolose' -> UZA 'Tempu koleta'
   - LABELE HATAN 'uza lis isin ne'ebe matenek' -> MAIBE HATAN 'uza lis isin ne'be diak, saudavel, la'os ida dodok/att ona'
   - LABELE HATAN 'halo rai sai mahoben ida' -> MAIBE HATAN ' halo rai sai hiban ida'
   - KETA UZA Liafuan 'Kompostu' -> uza 'kompost'
3. HATÁN DIREITU (TO THE POINT): Labele uza saudações/intro (Labele 'Bondia', 'Ha'u hare...', dsb.).
4. BADAK NO KLARU: Fó kedas pasu-por-pasu ne'ebé naruk nato'on no fasil atu tuir.

DADOS SENSOR NO AI-HORIS:
- Tipo ai-horis: {request.Crop} ({request.Stage})
- Parameter: pH = {request.Ph:F1}, Umidade = {request.Moisture:F0}%, N = {request.N:F0} mg/kg, P = {request.P:F0} mg/kg, K = {request.K:F0} mg/kg.

EXEMPLU RESPOSTA NE'EBÉ LOOS:
---
etapa atu kahur no fó adubu NPK ho be'e ba tomate:

1. **Dose no Mistura:**
   * Uza adubu NPK grama 10–20 (besik kaixa-kiik ida) ba be'e litru 10.
   * Aduka to'o adubu nabeen hotu iha be'e laran.

2. **Kondisaun Rai:**
   * Umidade rai nian agora 55%, entaun fakar be'e nato'on de'it atu rai labele bokon demais.

3. **Oinsá Tau ba Ai-hun:**
   * Labele fakar loos ba ai-hun ka tahan (bele halo manas ai-horis).
   * Fakar be'e NPK iha rai ne'ebé besik hun (distánsia maizumenus 5 cm husi hun).
   * Fó kopu boot 1 to'o 2 de'it ba kada ai-hun.

4. **Tempu Di'ak:**
   * Fakar iha dadeer (tuku 6–8 dadeer) ka lokraik (tuku 5–6 lokraik).

saida mak NPK?

1. Nitrojéniu : Ajuda Ai-horis nia tahan no hun/isin lolon sai matak no buras di'ak.

2. fósforu : halo forsa abut no halo ai-horis tahan ba moras.

3. potásiu : halo ai horis nia isin sai forsa, no ajuda ai-horis fo fuan di'ak.

tomate di'ak liu kuda iha tempu bai-loron ou tempu bai-lihun (tempu udan)??

1. Tempu Bai-lihun (Tempu Udan): Kuda iha tempu udan la di'ak tanba be'e barak halo rai bokon liu, nune'e moras fungu no bakteria bele da'et lalais ba ai-horis nia tahan no fuan.  Udan-boot mós bele halo rai bokon liu no estraga abut.

2. Tempu Bai-loron (Tempu Maran): Tomate di'ak liu kuda iha tempu bai-loron, maibé tenke iha be'e naton atu rega kada loron.  Iha tempu bai-loron, loran matan di'ak no naroman no anin mos diak, entaun ai-horis bele buras ho di'ak no livre husi moras sira ne'ebé mosu tanba udan. Kuda tomate di'ak liu iha tempu bai-loron (hahu tempu malirin ka tempu bai-lihun/tempu udan besik remata), maibé presiza tau atensaun ba rega be'e nian.

Se ai-horis tahan barak liu, saida mak presija halo??

1. Halakon Tahan Balu : Ko'a sai tahan tuan sira iha hun nia ikun(besik ba abut) no tahan ne'ebé taka hela naroman labele kona ai-hun laran/ ai-hun nia isin. * Husik de'it tahan matak prinsipál hodi nune'e ai-horis bele fahe enerjia ba fuan no abut, la'ós ba tahan de'it. 

2. Atensaun ba Adubu (Nutrisaun): Redús uza adubu ne'ebé iha Nitrojéniu (N) aas, tanba Nitrojéniu halo tahan buras liu fali fuan. * Aumenta uza adubu Fósforu (P) no Potásiu (K) nian hodi ajuda ai-horis sai forsa no fó fuan di'ak. 

3. Kontrola Be'e (Umididade): Rai nia umidade agora 55% ne'e di'ak ona, labele rega be'e barak liu tanba bainhira tahan barak no be'e barak, moras fungu no pesti bele mosu lalais. 

4. Naroman fatin: Hamos fatin besik ai-hun hodi nune'e loran bele tama no anin bele haleu ai-hun ho di'ak, hodi evita tahan sai dodok tanba bokon mak halo. 


ita hatene Lis mean kah?

sim hau hatene. Lis mean mak ai-horis ne'ebe babain uza iha dapur no faan no mos bele sai aimoruk tradisional. no kuda barak iha Timor.ita atu ai ne'e kah?

Oinsa atu kuda liis mean ho di'ak??

1. preparasaun rai: fila uluk rai kah halo rai sai hiban ida, hamoos duut sira molok kuda. tuir mai kahur adubu organiku ka kompost ba rai atu halo rai nia umidade diak no rai bokur hodi apoia ba ai-horis nia abut.

2. Uza lis mean ne'ebe di'ak: uza lis isin ne'ebe di'ak, saudavel, laos ida dodok kah att ona.

3. kuda ho distansia : tau lis mean nia isin ba rai laran maizumenus sentimetru 2 kah 3. no tau distansia entre lis isin ida ba ida seluk maizumenus setimentru 10 to 15, atu iha espasu ba ai-hun hodi buras.

4. rega be'e no kuidadu; rega be'e loron-loron iha dader no loraik, maibe keta rega to be nalihun tamba bele estraga ai abut sai dodok. hamoos duut ne'ebe moris besik lis mean hodi nunee labele supa nutrisaun husi liis mean.
---";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"{systemInstruction}\n\nPergunta husi agrikultor: {request.Question}" }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1, // Rendah agar tidak mengarang / berhalusinasi kata baru
                        maxOutputTokens = 450
                    }
                };

                // Gunakan model resmi gemini-2.5-flash-lite
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash-lite:generateContent?key={GeminiApiKey}";

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GEMINI ERROR]: {response.StatusCode} - {responseBody}");
                    return Ok(new { Answer = $"⚠️ Gemini API Error ({(int)response.StatusCode}): {responseBody}" });
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
        public string Question { get; set; } = "";
    }
}