using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Translation
{
    public class TranslateLocate
    {
        private string locateId;
        private Dictionary<string, string> locateStrings;

        public string ID
        {
            get { return locateId; }
        }

        public TranslateLocate(string locateId)
        {
            this.locateId = locateId;
            locateStrings = new Dictionary<string, string>();
        }

        public void AppendLocateString(string id, string locatedString)
        {
            locateStrings[id] = locatedString;
        }

        public string this[string strId]
        {
            get
            {
                return locateStrings[strId];
            }
        }

        public bool ContainsKey(string id)
        {
            return locateStrings.ContainsKey(id);
        }
    }
}
