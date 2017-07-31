using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLibrary
{
    public interface IDbController
    {
        Task SetDefaultCityAsync(BotLibrary.Conversation conversation, string cityName);
        Task<string> GetDefaultCityAsync(BotLibrary.Conversation conversation);
    }
}
