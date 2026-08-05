using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;
using Fm2ndParser.Stage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fm2ndParser.Compilers
{
    public class StageCompiler : BaseCompiler<StageFile>
    {
        public StageCompiler(string filename, StageFile stageFile)
            : base(filename, stageFile)
        {

        }

        override protected void CompileInternal(BinaryWriter writer)
        {
            base.CompileInternal(writer);

            writeZeros(writer, 4);

            writeUInt16(writer, (ushort)_fmFile.BGM.Number);

            writeZeros(writer, 1024);
        }
    }
}
