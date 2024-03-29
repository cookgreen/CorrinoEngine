﻿using CorrinoEngine.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Fields
{
    public class NameField : Field
    {
        private TranslableString translableString;

        public override string Name
        {
            get { return "Name"; }
        }

        public override object Params
        {
            get { return new object[] { "TranslatableString" }; }
        }

        public override void Execute(params object[] param)
        {
            translableString = TranslateManager.Instance.Parse((Params as object[])[0].ToString());
        }
    }
}
