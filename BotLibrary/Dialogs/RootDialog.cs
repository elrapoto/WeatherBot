using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotLibrary.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private Conversation externalConversation;

        public Task StartAsync(IDialogContext context)
        {
            externalConversation = new Conversation()
            {
                Platfrom = "Microsoft Bot Framework",
                Channel = context.Activity.ChannelId,
                Id = context.Activity.Conversation.Id
            };
            Conversation.onStartConversation?.Invoke(externalConversation);
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {            
            var activity = await result as Activity;
            var incomeUpdate = ConvertActivity(activity);

            if(incomeUpdate !=null)
            {
                var outputedUpdates = await Conversation.onNewUpdateAsync(externalConversation, incomeUpdate);
                if (outputedUpdates != null)
                {
                    foreach (var update in outputedUpdates)
                    {
                        await context.PostAsync(update.Text);
                    }
                }
            }            

            context.Wait(MessageReceivedAsync);
        }

        private static Update ConvertActivity(Activity activity)
        {
            try
            {
                Update result;
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        result = new Update(UpdateType.Message, activity.Text);
                        break;
                    case ActivityTypes.Typing:
                        result = new Update(UpdateType.Typing);
                        break;
                    case ActivityTypes.ConversationUpdate:
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