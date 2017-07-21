using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BotLibrary.Implementations;

namespace BotLibrary.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private MSConversation externalConversation;

        public Task StartAsync(IDialogContext context)
        {
            externalConversation = new MSConversation(this)
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
            var incomeUpdate = MSConversation.ConvertActivity(activity);

            if(incomeUpdate !=null)
            {
                Update[] outputedUpdates = await Conversation.onNewUpdateAsync(externalConversation, incomeUpdate);
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
    }
}