using ApiAiSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellectLibrary
{
    public abstract class IntellectInstanse
    {
        private static readonly List<IntellectInstanse> instances = new List<IntellectInstanse>();

        public static int AddInstance(IntellectInstanse instance)
        {
            int idx;
            lock("AddInstanceLockString")
            {
                idx = instances.Count;
                instances.Add(instance);
            }
            return idx;
        }

        public static IntellectInstanse GetInstance(int idx)
        {
            return instances[idx];
        }

        public abstract IntellectResponse GetResponse(string input);

        public int Idx { get; protected set; }        
    }
}
