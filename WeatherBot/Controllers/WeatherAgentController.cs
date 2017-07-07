using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WeatherBot.Controllers
{
    public class WeatherAgentController : ApiController
    {
        string weatherServiceAPIKey = ConfigurationManager.AppSettings["WeatherServiceAPIKey"];

        // GET api/<controller>
        public string Get()
        {
            return "Weather agent is running!";
        }

        // POST api/<controller>/
        [HttpPost]
        public async Task<JObject> Post([FromBody]JToken value)
        {
            //Creating a request
            string city = value.SelectToken("result").SelectToken("parameters").SelectToken("geo-city").Value<string>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://api.apixu.com/v1/current.json?key={ weatherServiceAPIKey }&q={city}");

            request.Method = "GET";
            request.Accept = "application/json";

            //Sending a request and reading a response
            string output;
            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                output = reader.ReadToEnd();
            }
            var weatherInfo = JToken.Parse(output);

            //Creating a resulting message
            string text = $"It is {weatherInfo["current"]["condition"]["text"].Value<string>().ToLower()} in {city}, {weatherInfo["current"]["feelslike_c"].Value<string>()}°C.";
            var result = new JObject(new JProperty("speech", text),
                new JProperty("displayText", text));

            return result;
        }
    }
}