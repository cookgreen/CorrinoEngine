using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Translation
{
    public class TranslableString : ITranslable
    {
        private string id;
        private string defaultStr;

        public TranslableString(string id, string defaultStr)
        {
            this.id = id;
            this.defaultStr = defaultStr;
        }

        public string Translate(string translateLocateID)
        {
            string result = TranslateManager.Instance.GetTranslableString(translateLocateID, id);
            return !string.IsNullOrEmpty(result) ? result : defaultStr;
        }

        public override string ToString()
        {
            return string.Format("%{{0}}{1}", id, defaultStr);
        }
    }
}
