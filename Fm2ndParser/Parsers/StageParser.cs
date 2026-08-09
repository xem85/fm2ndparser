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
        public override string FileExtension => "stage";

        public StageParser(string filename, KGTFile kgt) : base(filename, kgt)
        {
        }

        protected override SettingsType getSettingsType(uint skillIdx)
        {
            return SettingsType.Stage;
        }

        protected override StageFile ParseInternal()
        {
            var stage = base.ParseInternal();

            setSettingsBlocksData();

            skipEmptyBytes(4);

            var bgm = getUInt16();
            stage.BGM = new SkillReference
            {
                Number = bgm,
                Name = stage.Sounds.Skip(bgm).First().Name,
            };

            skipEmptyBytes(1);

            var count = getUInt16();

            skipRemaningEmptyBytes();
            return stage;
        }
    }
}