using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;
using Fm2ndParser.Kgt;
using Fm2ndParser.Stage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fm2ndParser.Compilers
{
    public class StageCompiler : BaseCompiler<StageFile>
    {
        public StageCompiler(StageFile stageFile, KGTFile kgtFile)
            : base(stageFile, kgtFile) { }

        override protected void CompileInternal()
        {
            base.CompileInternal();

            writeZeros(4);

            writeUInt16((ushort)_fmFile.BGM.Number);

            writeZeros(1024);
            writeZeros(7);
        }
    }
}
