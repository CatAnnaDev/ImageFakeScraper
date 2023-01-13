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
                Sleep = 0,
                Pseudo = "Pseudo",
                domain_blacklist = "domain_blacklist",
                words_list = "words_list",
                words_done = "words_done",
                record_push = "record_push",
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
                    useMongoDB = false,
                    PrintLog = true,
                    PrintLogMain = true,
                    GetNewTagGoogle = false,
                    GetNewTagBing = false
                },

                settingsDll = new()
                {
                    AlamyMaxPage = 200,
                    AlamyPageSize = 1500,
                    AlamyUnlimitedPage = true,
                    GettyMaxPage = 500,
                    ImmerseMaxPage = 2,
                    ImmersePageSize = 1000,
                    OpenVerseMaxPage = 2,
                    EveryPixelMaxPage = 500
                }
            };
        }
    }
}