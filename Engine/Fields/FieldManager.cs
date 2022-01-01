using CorrinoEngine.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class FieldManager
    {
        private Dictionary<string, Type> avaiableFields;

        private static FieldManager instance;
        public static FieldManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new FieldManager();
                }
                return instance;
            }
        }
        private ModData modData;

        public void Init(ModData modData)
        {
            this.modData = modData;
            getAvailableFields();
        }

        private void getAvailableFields()
        {
            avaiableFields = new Dictionary<string, Type>();
            Assembly thisAssembly = GetType().Assembly;
            Type[] internalTypes = thisAssembly.GetTypes();
            foreach (var internalType in internalTypes)
            {
                if(internalType.GetInterface("IField") != null && !internalType.IsAbstract)
                {
                    var fieldIns = Activator.CreateInstance(internalType) as IField;
                    avaiableFields.Add(fieldIns.Name, internalType);
                }
            }
        }

        public bool TryParse(string key)
        {
            return avaiableFields.ContainsKey(key);
        }

        public Field Parse(string key)
        {
            Type type = avaiableFields[key];
            return Activator.CreateInstance(type) as Field;
        }
    }
}
