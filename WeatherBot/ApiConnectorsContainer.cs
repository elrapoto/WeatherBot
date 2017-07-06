using ApiAiSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherBot
{
    public static class ApiConnectorsContainer
    //VERY UGLY BUT SOMEHOW IT WORKS
    {
        public static List<ApiAi> apiList = new List<ApiAi>();
    }
}