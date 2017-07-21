using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace BotLibrary.Implementations
{
    [Serializable]
    internal class MSConversation: Conversation
    {
        private IDialog<object> parentDialog;


        public MSConversation(IDialog<object> parentDialog)
        {
            this.parentDialog = parentDialog;            
        }

        public static Update ConvertActivity (Microsoft.Bot.Connector.Activity activity)
        {                        
            try
            {
                Update result;
                switch (activity.Type)
                {                    
                    case Microsoft.Bot.Connector.ActivityTypes.Message:
                        result = new Update(UpdateType.Message, activity.Text);
                        break;
                    case Microsoft.Bot.Connector.ActivityTypes.Typing:
                        result = new Update(UpdateType.Typing);
                        break;
                    case Microsoft.Bot.Connector.ActivityTypes.ConversationUpdate:
                        result = new Update(UpdateType.ConversationUpdate);
                        break;
                    default:
                        return null;
                }
                result.LocalTime = activity.LocalTimestamp;
                result.UTCTime = activity.Timestamp;
                result.From = activity.From.Id;
                return result;
            }
            catch
            {
                return null;
            }                                 
        }                
    }
}
