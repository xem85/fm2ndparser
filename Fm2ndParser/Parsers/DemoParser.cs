using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Fm2ndParser.Kgt;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;

namespace Fm2ndParser.Parsers
{
    public class DemoParser : BaseParser<DemoFile>
    {
        public override string FileExtension => "demo";

        public DemoParser(string filename, KGTFile kgt) : base(filename, kgt)
        {
        }

        protected override DemoFile ParseInternal()
        {
            var demo = base.ParseInternal();

            setSettingsBlocksData();

            skipEmptyBytes(4);
            var bgm = getUInt16();
            demo.BGM = new SkillReference
            {
                Number = bgm,
                Name = demo.Sounds.Skip(bgm).First().Name,
            };
            var skipWithInput = Convert.ToBoolean(getUInt16());
            skipEmptyBytes(1);
            var time = getUInt32();
            demo.Time = time;
            demo.SkipWithInput = skipWithInput;


            skipRemaningEmptyBytes();
            return demo;
        }
        protected override SettingsType getSettingsType(uint skillIdx)
        {
            return SettingsType.None;
        }
    }
}