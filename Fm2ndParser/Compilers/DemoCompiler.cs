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
    public class DemoCompiler : BaseCompiler<DemoFile>
    {
        public DemoCompiler(DemoFile demoFile, KGTFile kgtFile)
            : base(demoFile, kgtFile) { }

        override protected void CompileInternal()
        {
            base.CompileInternal();

            writeZeros(4);

            writeUInt16((ushort)_fmFile.BGM.Number);

            writeUInt16(Convert.ToUInt16(_fmFile.SkipWithInput));
            writeZeros(1);
            writeUInt32(_fmFile.Time);

            writeZeros(1024);
        }
    }
}
