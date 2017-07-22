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
using IntellectLibrary;
using System.Web;

namespace WeatherBot
{
    public class BusinessLogic
    {
        public static ConcurrentDictionary<Tuple<string, string, string>, Tuple<int, bool>> apiDictionary = new ConcurrentDictionary<Tuple<string, string, string>, Tuple<int, bool>>();
        static string weatherServiceAPIKey = ConfigurationManager.AppSettings["WeatherServiceAPIKey"];
        

        public static void OnStart(Conversation conversation)
        {            
            var intellectInstance = new ApiAiIntellectInstance();
            apiDictionary.TryAdd(new Tuple<string, string, string>(conversation.Channel, conversation.Id, conversation.Platfrom),
                new Tuple<int, bool>(intellectInstance.Idx, true));            
        }

        public static async Task<Update[]> NewUpdateHandler(Conversation conversation, Update update)
        {
            List<Update> result = null;
            if (update.Text!=null && update.Text?.Length>0)
            {
                IntellectInstanse intellectInstance;
                Tuple<int, bool> dictionaryValue;
                
                //Getting ApiAi object to call the service and getting a bool value which indicates wether it is the first  message in the conversation
                try
                {
                    dictionaryValue = apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                    conversation.Platfrom)];
                    intellectInstance = IntellectInstanse.GetInstance(dictionaryValue.Item1);
                }
                catch
                {
                    OnStart(conversation);
                    dictionaryValue = apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                    conversation.Platfrom)];
                    intellectInstance = IntellectInstanse.GetInstance(dictionaryValue.Item1);
                }
                
                //check wether its the first message
                if (dictionaryValue.Item2)
                {
                    result = new List<Update>(SayHello());
                    apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                    conversation.Platfrom)] = new Tuple<int, bool>(intellectInstance.Idx, false);
                }
                else
                {
                    result = new List<Update>();
                }                

                var response = intellectInstance.GetResponse(update.Text);
                string responseText = response.Speech ?? "";

                if (response.Action == "GetWeather" && response.Parameters["geo-city"].ToString() != "")
                {
                    //post weather conditions to the user
                    result.Insert(result.Count, await GetWeatherAsync(response.Parameters["geo-city"].ToString()));
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