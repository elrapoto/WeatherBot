using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellectLibrary
{
    public class IntellectResponse
    {
        public string Speech { get; private set; }
        public String Action
        {
            get
            {
                if (string.IsNullOrEmpty(action))
                {
                    return string.Empty;
                }
                return action;
            }
            private set
            {
                action = value;
            }
        }
        public Dictionary<string, object> Parameters { get; private set; }

        private string action;

        public IntellectResponse(string speech, string action, Dictionary<string, object> parameters)
        {
            Speech = speech;
            Action = action;
            Parameters = parameters;
        }
    }
}
