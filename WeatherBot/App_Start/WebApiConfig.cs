using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace WeatherBot
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Intellect library settings
            IntellectLibrary.ApiAiIntellectInstance.clientAccessToken = ConfigurationManager.AppSettings["ApiAiID"];
            IntellectLibrary.ApiAiIntellectInstance.inputLanguage = "english";

            //Bot library settings
            BotLibrary.Conversation.onNewUpdateAsync += BusinessLogic.NewUpdateHandler;
            BotLibrary.Conversation.onStartConversation += BusinessLogic.OnStart;

            //Database library settings
            DatabaseLibrary.PostgreDbController.userId = ConfigurationManager.AppSettings["DbUser"];
            DatabaseLibrary.PostgreDbController.password = ConfigurationManager.AppSettings["DbPassword"];
            DatabaseLibrary.PostgreDbController.databaseName = ConfigurationManager.AppSettings["DbName"];
            DatabaseLibrary.PostgreDbController.port = ConfigurationManager.AppSettings["DbPort"];
            DatabaseLibrary.PostgreDbController.server = ConfigurationManager.AppSettings["DbServerAddress"];

            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );
        }
    }
}
