using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Translation
{
    public class TranslateManager
    {
        private Dictionary<string, Dictionary<string, string>> translableStringDictory;

        public TranslateManager()
        {
            translableStringDictory = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Init()
        {
            //Load all translable texts
        }
    }
}
