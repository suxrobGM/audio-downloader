using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace AudioDownloader
{
    class Program
    {
        static readonly string[] audioTypes = { ".mp3", ".m4a", ".wave" };
        static string jsonFile;

        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.Write("Please enter JSON file name: ");
                jsonFile = Console.ReadLine();
            }
            else
            {
                jsonFile = args[0];
            }

            if (!File.Exists(jsonFile))
            {
                Console.WriteLine("Json file did not found. \nPress any key to exit.");
                Console.ReadKey();
                return;
            }

            var jsonReader = new JsonTextReader(new StringReader(File.ReadAllText(jsonFile)));
            var audioFiles = new List<FileLink>();
            Console.WriteLine("Parsed audio links: ");

            while (jsonReader.Read())
            {
                if (jsonReader.Value != null && jsonReader.TokenType == JsonToken.String)
                {
                    var jsonValue = jsonReader.Value.ToString();
                    bool isValidUrl = Uri.TryCreate(jsonValue, UriKind.Absolute, out Uri uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                       
                    if (isValidUrl)
                    {
                        var fileName = uriResult.Segments.Last();

                        try
                        {
                            var fileExtension = fileName.Substring(fileName.IndexOf("."));
                            if (audioTypes.Contains(fileExtension))
                            {
                                audioFiles.Add(new FileLink()
                                {
                                    Name = fileName,
                                    Url = jsonValue
                                });

                                Console.WriteLine(jsonValue);
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            continue;
                        }                                                                   
                    }
                }          
            }

            Console.WriteLine("\nDownloading files...");
            using var client = new WebClient();
            foreach (var audioFile in audioFiles)
            {
                try
                {
                    client.DownloadFile(audioFile.Url, audioFile.Name);
                }
                catch (WebException)
                {
                    Console.WriteLine($"Could not download file \'{audioFile.Name}\' from \'{audioFile.Url}\'");
                    continue;
                }

                Console.WriteLine($"Downloaded file \'{audioFile.Name}\' from \'{audioFile.Url}\'");
            }

            Console.WriteLine("\nAll operations are finished. \nPress any key to exit.");
            Console.ReadKey();
        }
    }

    struct FileLink
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
}
