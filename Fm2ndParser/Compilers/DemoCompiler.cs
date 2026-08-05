using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fm2ndParser.Compilers
{
    public class DemoCompiler : BaseCompiler<DemoFile>
    {
        public DemoCompiler(string filename, DemoFile demoFile)
            : base(filename, demoFile)
        {

        }

        override protected void CompileInternal(BinaryWriter writer)
        {
            base.CompileInternal(writer);

            writeZeros(writer, 4);

            writeUInt16(writer, (ushort)_fmFile.BGM.Number);

            writeUInt16(writer, Convert.ToUInt16(_fmFile.SkipWithInput));
            writeZeros(writer, 1);
            writeUInt32(writer, _fmFile.Time);

            writeZeros(writer, 1024 - 9);
        }
    }
}
