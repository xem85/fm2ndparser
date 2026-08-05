using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Fm2ndParser.Kgt;
using Fm2ndParser.Common;
using Fm2ndParser.Stage;

namespace Fm2ndParser.Parsers
{
    public class StageParser : BaseParser<StageFile>
    {

        public StageParser(string filename, KGTFile kgt) : base(filename, kgt)
        {
        }

        protected override SettingsType getSettingsType(uint skillIdx)
        {
            return SettingsType.Stage;
        }

        protected override StageFile ParseInternal(Span<byte> bytes, ref int offset)
        {
            var stage = base.ParseInternal(bytes, ref offset);

            setSettingsBlocksData();

            skipEmptyBytes(bytes, 4, ref offset);

            var bgm = getUInt16(bytes, ref offset);
            stage.BGM = new SkillReference
            {
                Number = bgm,
                Name = stage.Sounds.Skip(bgm).First().Name,
            };
         
            skiRemaningEmptyBytes(bytes, ref offset);
            return stage;
        }
    }
}