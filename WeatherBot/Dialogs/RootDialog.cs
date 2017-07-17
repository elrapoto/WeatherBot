using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ApiAiSDK;
using System.Configuration;
using ApiAiSDK.Model;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace WeatherBot.Dialogs
{   

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        int apiId;
        string weatherServiceAPIKey = ConfigurationManager.AppSettings["WeatherServiceAPIKey"];

        public Task StartAsync(IDialogContext context)
        {
            var config = new AIConfiguration(ConfigurationManager.AppSettings["ApiAiID"], SupportedLanguage.English);
            ApiAi apiAi = new ApiAi(config);
            //adding apiAi object to the collection
            lock ("lockString")
            {                
                ApiConnectorsContainer.apiList.Add(apiAi);
                apiId = ApiConnectorsContainer.apiList.Count-1;
            }
            context.Wait(MessageReceivedAsync);        
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //Checking wether any text was recieved
            if (activity.Text == "/start")
            {
                await SendHello(context);
            }
            else if (activity.Text!=null)
            {
                var response = ApiConnectorsContainer.apiList[apiId].TextRequest(activity.Text);
                string responseText = response.Result.Fulfillment.Speech ?? "";                

                if (response.Result.Action == "GetWeather" && response.Result.Parameters["geo-city"] != "")
                {
                    //post weather conditions to the user
                    await context.PostAsync( await GetWeatherAsync(response.Result.Parameters["geo-city"].ToString()) );
                }
                else
                {
                    //post API.AI reply to the user
                    await context.PostAsync(responseText);
                }
            }

            context.Wait(MessageReceivedAsync);
        }

        async Task SendHello(IDialogContext context)
        {
            var response = ApiConnectorsContainer.apiList[apiId].TextRequest("Hello");
            string responseText = response.Result.Fulfillment.Speech ?? "";
            // return our reply to the user
            await context.PostAsync(responseText);
            await context.PostAsync("I am a weather bot. You could ask me about weather conditions in different cities and we could also have some small talk.");
            //await context.PostAsync("I was built using Microsoft Bot Framework and API.AI. Also a special webhook service was created for me to get the weather info, implementing REST API model to communicate with API.AI.");
            await context.PostAsync("Please, enjoy using me, cause I am great! =)");
        }

        async Task<string> GetWeatherAsync(string city)
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
            return $"It is {weatherInfo["current"]["condition"]["text"].Value<string>().ToLower()} in {city}, {weatherInfo["current"]["feelslike_c"].Value<string>()}°C.";
        }
    }
}