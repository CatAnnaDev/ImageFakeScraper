using ImageFakeScraper;
using Newtonsoft.Json;

namespace ImageFakeScraperExample.config
{
    internal class buildJsonFile
    {
        public string ConfigPath { get; set; } = "Config.json";
        public jsonConfigFile? Config { get; set; }

        public async Task InitializeAsync()
        {
            string json;
            if (!File.Exists(ConfigPath))
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
                File.WriteAllText("Config.json", json, new UTF8Encoding(false));
                Console.WriteLine($"Update config file \n{Directory.GetCurrentDirectory()}\\Config.json");
                await Task.Delay(-1);
            }

            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<jsonConfigFile>(json);
        }

        private jsonConfigFile GenerateNewConfig()
        {
            return new jsonConfigFile
            {
                Credential = "Redis Login",
                requestMaxPerSec = 8,
                nbThread = 4,
				QueueLimit = 30,
                words_list = "words_list",
                images_jobs = "image_jobs",
                to_download = "to_download",

                settings = new()
                {
                    GoogleRun = true,
                    DuckduckGORun = false,
                    BraveRun = false,
                    AlamyRun = true,
                    OpenVerseRun = true,
                    BingRun = true,
                    YahooRun = true,
                    GettyImageRun = true,
                    ImmerseRun = true,
                    EveryPixelRun = true,
                    stopAfter = 8000000,
                    PrintLogTag = true,
                    PrintLog = true
                }
            };
        }
    }
}