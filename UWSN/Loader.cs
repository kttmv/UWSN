using Newtonsoft.Json;

namespace UWSN
{
    public class Loader
    {
        public string EnvFilePath { get; set; }

        public Model.Environment LoadEnv()
        {
            if (!File.Exists(EnvFilePath))
            {
                throw new FileNotFoundException("Не удалось найти указанный файл.");
            }

            using StreamReader reader = new(EnvFilePath);

            var env = JsonConvert.DeserializeObject<Model.Environment>(reader.ReadToEnd(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return env ?? throw new NullReferenceException("Не удалось создать окружение из файла");
        }

        public Loader(string envFilePath)
        {
            EnvFilePath = envFilePath;
        }
    }
}