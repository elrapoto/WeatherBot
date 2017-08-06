using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLibrary
{
    public delegate Task<Update[]> NewUpdateHandler(Conversation conversation, Update update);
    public delegate void StartConversationHandler(Conversation conversation);

    /// <summary>
    /// Describes a data identifying a single dialog
    /// </summary>
    [Serializable]
    public class Conversation
    {
        public string Platfrom { get; set; }
        public string Channel { get; set; }
        public string Id { get; set; }

        /// <summary>
        /// Delegate that points to a method which starts when the Dialog is started
        /// </summary>
        public static StartConversationHandler onStartConversation;
        /// <summary>
        /// Delegate that points to a method which starts when a new update is recieved
        /// </summary>
        public static NewUpdateHandler onNewUpdateAsync;
    }
}
