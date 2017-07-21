using ApiAiSDK;
using BotLibrary;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace WeatherBot
{
    public class BusinessLogic
    //VERY UGLY BUT SOMEHOW IT WORKS
    {
        public static List<ApiAi> apiList = new List<ApiAi>();
        public static ConcurrentDictionary<Tuple<string, string, string>, Tuple<ApiAi, bool>> apiDictionary = new ConcurrentDictionary<Tuple<string, string, string>, Tuple<ApiAi, bool>>();
        static string weatherServiceAPIKey = ConfigurationManager.AppSettings["WeatherServiceAPIKey"];
        

        public static void OnStart(Conversation conversation)
        {
            var config = new AIConfiguration(ConfigurationManager.AppSettings["ApiAiID"], SupportedLanguage.English);
            var aiApi = new ApiAi(config);
            apiDictionary.TryAdd(new Tuple<string, string, string>(conversation.Channel, conversation.Id, conversation.Platfrom),
                new Tuple<ApiAi, bool>(aiApi, true));
        }

        public static async Task<Update[]> NewUpdateHandler(Conversation conversation, Update update)
        {
            List<Update> result = null;
            if (update.Text!=null && update.Text?.Length>0)
            {
                ApiAi apiAi;
                Tuple<ApiAi, bool> dictionaryValue;
                
                //Getting ApiAi object to call the service and getting a bool value which indicates wether it is the first  message in the conversation
                try
                {
                    dictionaryValue = apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                    conversation.Platfrom)];
                    apiAi = dictionaryValue.Item1;
                }
                catch
                {
                    var config = new AIConfiguration(ConfigurationManager.AppSettings["ApiAiID"], SupportedLanguage.English);
                    apiAi = new ApiAi(config);
                    dictionaryValue = new Tuple<ApiAi, bool>(apiAi, true);
                    apiDictionary.TryAdd(new Tuple<string, string, string>(conversation.Channel, conversation.Id, conversation.Platfrom),
                       dictionaryValue);
                }
                
                //check wether its the first message
                if (dictionaryValue.Item2)
                {
                    result = new List<Update>(SayHello());
                    apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                    conversation.Platfrom)] = new Tuple<ApiAi, bool>(apiAi, false);
                }
                else
                {
                    result = new List<Update>();
                }                

                var response = apiAi.TextRequest(update.Text);
                string responseText = response.Result.Fulfillment.Speech ?? "";

                if (response.Result.Action == "GetWeather" && response.Result.Parameters["geo-city"].ToString() != "")
                {
                    //post weather conditions to the user
                    result.Insert(result.Count, await GetWeatherAsync(response.Result.Parameters["geo-city"].ToString()));
                }
                else
                {
                    //post API.AI reply to the user
                    result.Add(new Update(UpdateType.Message, responseText));
                }                
            }
            return result.ToArray();
        }

        private static async Task<Update> GetWeatherAsync(string city)
        {
            //Creating a request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://api.apixu.com/v1/current.json?key={ weatherServiceAPIKey }&q={city}");

            request.Method = "GET";
            request.Accept = "application/json";

            //Sending a request and reading a response
            string output;
            using (var weatherServerResponse = (HttpWebResponse)(await request.GetResponseAsync()))
            {
                StreamReader reader = new StreamReader(weatherServerResponse.GetResponseStream());
                output = reader.ReadToEnd();
            }
            var weatherInfo = JToken.Parse(output);

            //Returning a resulting message            
            return new Update(UpdateType.Message, 
                $"It is {weatherInfo["current"]["condition"]["text"].Value<string>().ToLower()} in {city}, {weatherInfo["current"]["feelslike_c"].Value<string>()}°C.");
        }

        private static Update[] SayHello()
        {
            var result = new List<Update>();
            // return our reply to the user
            result.Add(new Update(UpdateType.Message, "Hello"));
            result.Add(new Update(UpdateType.Message, "I am a weather bot. You could ask me about weather conditions in different cities and we could also have some small talk."));
            result.Add(new Update(UpdateType.Message, "Please, enjoy using me, cause I am great! =)"));
            return result.ToArray();
        }                  
    }
}