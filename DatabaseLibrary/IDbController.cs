using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    /// <summary>
    /// Interface to be used with any db controller
    /// Garantees the implementation of a functionality needed by the WeatherBot
    /// </summary>
    public interface IDbController
    {
        Task SetDefaultCityAsync(BotLibrary.Conversation conversation, string cityName);
        Task<string> GetDefaultCityAsync(BotLibrary.Conversation conversation);
    }
}
