using Newtonsoft.Json;

namespace ImageFakeScraperExample.config
{
    internal class buildJsonFile
    {
        public string ConfigPath { get; set; } = "Config.json";
        public jsonConfigFile Config { get; set; }

        public async Task InitializeAsync()
        {
            var json = string.Empty;

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

        private jsonConfigFile GenerateNewConfig() => new jsonConfigFile
        {
            Credential = "Redis Login",
            Sleep = 0,
            Pseudo = "Pseudo",
            domain_blacklist = "domain_blacklist",
            words_list = "words_list",
            words_done = "words_done",
            record_push = "record_push",
            jobs_last_index = "jobs_last_index",
            image_jobsPattern = "*image_jobs_*"
        };
    }
}
