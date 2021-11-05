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

        public TranslableString(string id)
        {
            this.id = id;
        }

        public string Translate(string translateLocateID)
        {
            return TranslateManager.Instance.GetTranslableString(translateLocateID, id);
        }
    }
}
