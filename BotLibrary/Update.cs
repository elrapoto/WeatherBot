using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLibrary
{
    public enum UpdateType { Message, Typing, ConversationUpdate }

    public class Update
    {
        public UpdateType Type { get; set; }
        public string Text { get; set; }
        public DateTime? UTCTime { get; set; }
        public DateTimeOffset? LocalTime { get; set; }
        public string From { get; set; }

        public Update(UpdateType type)
        {
            Type = type;
            Text = null;
        }

        public Update(UpdateType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}
