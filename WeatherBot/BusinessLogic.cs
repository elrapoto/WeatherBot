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
        private static readonly string weatherServiceAPIKey = ConfigurationManager.AppSettings["WeatherServiceAPIKey"];
        private static readonly DatabaseLibrary.IDbController dbController = DatabaseLibrary.PostgreDbController.Controller;        

        public static void OnStart(Conversation conversation)
        {            
            var intellectInstance = new ApiAiIntellectInstance();
            apiDictionary.TryAdd(new Tuple<string, string, string>(conversation.Channel, conversation.Id, conversation.Platfrom),
                new Tuple<int, bool>(intellectInstance.Idx, true));            
        }

        public static async Task<Update[]> NewUpdateHandler(Conversation conversation, Update update)
        {
            //check wether message is empty
            if (update?.Text == null || update.Text?.Length == 0)
                return null;

            List<Update> resultingMessages = new List<Update>();
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

            //check wether its the first message and if so - add greeting messages to the output and insert the data into the apiDictionary
            if (dictionaryValue.Item2)
            {
                resultingMessages.InsertRange(0, GetGreetingMessages());
                apiDictionary[new Tuple<string, string, string>(conversation.Channel, conversation.Id,
                conversation.Platfrom)] = new Tuple<int, bool>(intellectInstance.Idx, false);
            }

            var response = intellectInstance.GetResponse(update.Text);            
            
            //genarate a reply
            switch(response.Action)
            {
                case "GetWeather":
                    string city = response.Parameters["geo-city"].ToString();
                    if (city == "")
                    {
                        string defaultCity = await dbController.GetDefaultCityAsync(conversation);

                        //if there's no default city, pass API.AI message
                        if (defaultCity == null || defaultCity == "")
                        {                            
                            goto default;
                        }
                        city = defaultCity;
                        intellectInstance.GetResponse(defaultCity);
                    }
                    //post weather conditions to the user
                    resultingMessages.Insert(resultingMessages.Count, await GetWeatherAsync(city));
                    break;
                case "DefaultCitySetUp":
                    city = response.Parameters["geo-city"].ToString();
                    if (city != "")
                    {
                        await dbController.SetDefaultCityAsync(conversation, city);
                    }
                    goto default;
                default:
                    //post API.AI reply to the user
                    resultingMessages.Add(new Update(UpdateType.Message, response.Speech ?? ""));
                    break;
            }

            return resultingMessages.ToArray();
        }

        private static async Task<Update> GetWeatherAsync(string city)
        {
            try
            {
                //Creating a request
                var request = (HttpWebRequest)WebRequest.Create($"http://api.apixu.com/v1/current.json?key={ weatherServiceAPIKey }&q={city}");

                request.Method = "GET";
                request.Accept = "application/json";

                //Sending a request and reading a response
                string output;
                using (var weatherServerResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var reader = new StreamReader(weatherServerResponse.GetResponseStream());
                    output = reader.ReadToEnd();
                }
                var weatherInfo = JToken.Parse(output);

                //Returning a resulting message            
                return new Update(UpdateType.Message, 
                    $"It is {weatherInfo["current"]["condition"]["text"].Value<string>().ToLower()} in {city}, {weatherInfo["current"]["feelslike_c"].Value<string>()}°C.");
            }
            catch
            {
                return new Update(UpdateType.Message, "Weather info is not accessible");
            }           
        }

        private static Update[] GetGreetingMessages()
        {
            var result = new List<Update>
            {
                new Update(UpdateType.Message, "Hello"),
                new Update(UpdateType.Message,
                    "I am a weather bot. You could ask me about weather conditions in different cities and we could also have some small talk."),
                new Update(UpdateType.Message, "Please, enjoy using me, cause I am great! =)")
            };
            // return our reply to the user
            return result.ToArray();
        }                  
    }
}