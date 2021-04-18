using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace RMMVCookTool.Core.Utilities
{
    public static class JsonProcessor
    {
        public static string ReadJson(in string fileLocation)
        {
            string JsonString;
            char[] JsonIn;
            using (StreamReader settingsLoader = new StreamReader(fileLocation))
            {
                JsonIn = new Char[(int)settingsLoader.BaseStream.Length];
                settingsLoader.Read(JsonIn, 0, (int)settingsLoader.BaseStream.Length);
            }

            JsonString = new string(JsonIn);
            return JsonString;
        }

        public static string FindGameFolder(in string metadataFile)
        {
            if (metadataFile != null)
            {
                string metadata = ReadJson(metadataFile);
                var projectMetadata = JObject.Parse(metadata);
                string tempstring = (string)projectMetadata["main"];
                if (tempstring != null)
                {
                    string[] dataPart = tempstring.Split('/');
                    string tempString2 = dataPart[0];
                    if (dataPart.Length >= 2)
                    {
                        for (int i = 1; i < dataPart.Length - 2; i++)
                        {
                            tempString2 += dataPart[i] + ((RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/");
                        }
                    }
                    return Path.Combine(metadataFile.Replace(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\package.json" : "/package.json", "", StringComparison.Ordinal), tempString2);
                }

                return "Null";

            }

            return "Unknown";
        }


    }
}
