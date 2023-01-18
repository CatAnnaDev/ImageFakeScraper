using ImageFakeScraper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable
namespace ImageFakeScraperExample.config
{
    internal class buildJsonFile
    {
        public string ConfigPath { get; set; } = "Config.json";
        public jsonConfigFile? Config { get; set; }
        public JObject Configs { get; set; }

        public async Task InitializeAsync()
        {
            string jsonUser;
            string jsonDefault = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
            if (File.Exists(ConfigPath))
            {
                jsonUser = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
                Config = JsonConvert.DeserializeObject<jsonConfigFile>(jsonUser);

                JObject userConf = JObject.Parse(jsonUser);

                JObject defaultConf = JObject.Parse(jsonDefault);

                //userConf.Merge(defaultConf, new JsonMergeSettings
                //{
                //    MergeArrayHandling = MergeArrayHandling.Union
                //});

                Configs = new JObject();

                Configs.Merge(defaultConf);
                Configs.Merge(userConf);
            }
            else
            {
                File.WriteAllText("Config.json", jsonDefault, new UTF8Encoding(false));
                Console.WriteLine($"Update config file \n{Directory.GetCurrentDirectory()}\\Config.json");
                await Task.Delay(-1);
            }

            jsonUser = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<jsonConfigFile>(jsonUser);

            File.WriteAllText("Config.json", Configs.ToString(), new UTF8Encoding(false));
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
                    GoogleRun = false,
                    UnsplashRun = true,
                    QwantRun = true,
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
                },
                settingsDLL = new()
                {
                    printLog = false,
                    printErrorLog = false
                }
            };
        }
    }
}