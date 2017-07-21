using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLibrary
{
    public delegate Task<Update[]> NewUpdateHandler(Conversation conversation, Update update);
    public delegate void StartConversationHandler(Conversation conversation);

    [Serializable]
    public abstract class Conversation
    {
        public string Platfrom { get; set; }
        public string Channel { get; set; }
        public string Id { get; set; }

        public static StartConversationHandler onStartConversation;
        public static NewUpdateHandler onNewUpdateAsync;
    }
}
