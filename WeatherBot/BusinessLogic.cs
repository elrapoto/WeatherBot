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
using System.Text.RegularExpressions;

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

            //create response to the text inside the message
            if (update?.Text != null && update.Text?.Length != 0)
                resultingMessages.AddRange(await TextUpdateHandler(conversation, update, intellectInstance));

            //create response to the location inside the message
            if (update?.Location!=null)
                resultingMessages.AddRange(await LocationUpdateHandler(conversation, update, intellectInstance));

            return resultingMessages.ToArray();
        }

        /// <summary>
        /// Generates a response for an Update with text.
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="update"></param>
        /// <param name="intellectInstance"></param>
        /// <returns></returns>
        public static async Task<Update[]> TextUpdateHandler(Conversation conversation, Update update, 
            IntellectInstanse intellectInstance)
        {
            if (update.Text == String.Empty || update.Text == null)
                throw new Exception("Empty update");

            var intellectResponse = intellectInstance.GetResponse(update.Text);

            //genarate a reply
            switch (intellectResponse.Action)
            {
                case "GetWeather":
                    string city = intellectResponse.Parameters["geo-city"].ToString();
                    if (city == "")
                    {
                        //getting a default city from database
                        string defaultCity = await dbController.GetDefaultCityAsync(conversation);

                        //if there's no default city, pass API.AI message
                        if (defaultCity == null || defaultCity == "")
                        {
                            goto default;
                        }

                        //checking wether default location is set with coordinates
                        if (Regex.IsMatch(defaultCity, ".*[0-9]+.*"))
                        {
                            return new Update[] 
                                { new Update(UpdateType.Message, "Data on your default location:"),
                                await GetLocationWeatherAsync(new GeoLocation(Regex.Match(defaultCity, "^[^\\,]+").Value,
                                                                                    Regex.Match(defaultCity, "[^\\,]+$").Value)) };
                        }
                        
                        city = defaultCity;
                        intellectInstance.GetResponse(defaultCity);
                    }
                    //post weather conditions to the user
                    return new Update[] { await GetCityWeatherAsync(city) };
                case "DefaultLocationSetUp":
                    string latitude = intellectResponse.Parameters["lat"].ToString();
                    string longtidute = intellectResponse.Parameters["lon"].ToString();
                    if (latitude != "" && longtidute != "")
                    {
                        await dbController.SetDefaultCityAsync(conversation, $"{latitude},{longtidute}");
                    }                        
                    goto default;
                case "DefaultCitySetUp":
                    city = intellectResponse.Parameters["geo-city"].ToString();
                    if (city != "")
                    {
                        await dbController.SetDefaultCityAsync(conversation, city);
                    }
                    goto default;
                default:
                    //return a reply from IntellectInstance
                    return new Update[] {new Update(UpdateType.Message, intellectResponse.Speech ?? "")};
            }            
        }

        /// <summary>
        /// Generates a response for an Update with location.
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="update"></param>
        /// <param name="intellectInstance"></param>
        /// <returns></returns>
        public static async Task<Update[]> LocationUpdateHandler(Conversation conversation, Update update,
            IntellectInstanse intellectInstance)
        {
            if (update?.Location?.Latidute == null || update?.Location?.Longitude == null)
                throw new Exception("No location");

            //setting up a correct context inside api.ai
            var intellectResponse = 
                intellectInstance.GetResponse($"Recieved location lat:{update.Location.Latidute}, lon:{update.Location.Longitude}");

            return new Update[]{(await GetLocationWeatherAsync(update.Location))};
        }

        /// <summary>
        /// Returns weather update for selected geolocation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private static async Task<Update> GetLocationWeatherAsync(GeoLocation location)
        {
            try
            {
                var weatherInfo = await GetWeatherAsync($"{location.Latidute},{location.Longitude}");
                //Returning a resulting message        
                return new Update(UpdateType.Message,
                    $"It is {weatherInfo.Item1} there, {weatherInfo.Item2}°C.");
            }
            catch
            {
                return new Update(UpdateType.Message, "Sorry, the weather info is not accessible...");
            }
        }

        /// <summary>
        /// Returns weather update for selected  city
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private static async Task<Update> GetCityWeatherAsync(string city)
        {
            try
            {
                var weatherInfo = await GetWeatherAsync(city);
                //Returning a resulting message        
                return new Update(UpdateType.Message,
                    $"It is {weatherInfo.Item1} in {city}, {weatherInfo.Item2}°C.");
            }
            catch
            {
                return new Update(UpdateType.Message, "Sorry, the weather info is not accessible...");
            }                                       
        }

        /// <summary>
        /// Returns weather info in Tuple (string condition, string temperature).         
        /// </summary>
        /// <param name="location">Either city's name: {name} or coordinates: {latidute, longtidute}</param>
        /// <returns></returns>
        private static async Task<Tuple<string, string>> GetWeatherAsync(string location)
        {
            try
            {
                //Creating a request
                var request = (HttpWebRequest)WebRequest.Create($"http://api.apixu.com/v1/current.json?key={ weatherServiceAPIKey }&q={location}");

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

                return new Tuple<string, string>(weatherInfo["current"]["condition"]["text"].Value<string>().ToLower(),
                    weatherInfo["current"]["feelslike_c"].Value<string>().ToLower());                
            }
            catch
            {
                throw new Exception("Weather is not accessible");
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