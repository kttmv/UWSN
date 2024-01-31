using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UWSN
{
    public class Loader
    {
        public string EnvFilePath { get; set; }

        public Model.Environment LoadEnv()
        {
            using StreamReader reader = new StreamReader(EnvFilePath);

            var env = JsonConvert.DeserializeObject<Model.Environment>(reader.ReadToEnd(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            return env; 
        }
        
        public Loader(string envFilePath) 
        {
            EnvFilePath = envFilePath;
        }
    }
}
