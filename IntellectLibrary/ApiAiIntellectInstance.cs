using ApiAiSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellectLibrary
{
    public class ApiAiIntellectInstance: IntellectInstanse
    {
        public static string clientAccessToken;
        public static string inputLanguage;
        
        ApiAi api;

        public ApiAiIntellectInstance(string clientAccessToken, string inputLanguage)
        {
            SupportedLanguage chosenLanguage;
            switch (inputLanguage.ToLower())
            {
                case "russian":
                    chosenLanguage = SupportedLanguage.Russian;
                    break;
                case "english":
                    chosenLanguage = SupportedLanguage.English;
                    break;
                case "german":
                    chosenLanguage = SupportedLanguage.German;
                    break;
                case "french":
                    chosenLanguage = SupportedLanguage.French;
                    break;
                case "spanish":
                    chosenLanguage = SupportedLanguage.Spanish;
                    break;
                case "italian":
                    chosenLanguage = SupportedLanguage.Italian;
                    break;
                default:
                    chosenLanguage = SupportedLanguage.ChineseChina;
                    break;

            }
            AIConfiguration configuration = new AIConfiguration(clientAccessToken, chosenLanguage);
            api = new ApiAi(configuration);
            AddItseldToList();
        }

        public ApiAiIntellectInstance(): this(clientAccessToken, inputLanguage)
        {
        }

        public override IntellectResponse GetResponse(string input)
        {
            return ConvertResponse(api.TextRequest(input));
        }

        private void AddItseldToList()
        {
            Idx = AddInstance(this);            
        }

        private static IntellectResponse ConvertResponse(ApiAiSDK.Model.AIResponse aiResponse)
        {
            if (aiResponse == null)
                return null;
            return new IntellectResponse(aiResponse.Result.Fulfillment.Speech, aiResponse.Result.Action,
                aiResponse.Result.Parameters);
        }
    }
}
