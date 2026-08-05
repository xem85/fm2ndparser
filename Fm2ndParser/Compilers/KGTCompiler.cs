using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;
using Fm2ndParser.Kgt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fm2ndParser.Compilers
{
    public class KGTCompiler : BaseCompiler<KGTFile>
    {
        public KGTCompiler(string filename, KGTFile kgtFile)
            : base(filename, kgtFile)
        {

        }

        override protected void CompileInternal(BinaryWriter writer)
        {
            base.CompileInternal(writer);
        }
    }
}
