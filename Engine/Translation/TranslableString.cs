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
        private string defaultDisplayString;

        public TranslableString(string id, string defaultDisplayString)
        {
            
        }

        public void Translate(string translateLocateID)
        {
            throw new NotImplementedException();
        }
    }
}
