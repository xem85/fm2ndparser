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
    public class DemoParser : BaseParser
    {

        public DemoParser(string filename, KGT kgt) : base(filename, kgt)
        {
        }

        public DemoFile Parse()
        {
            return base.parse<DemoFile>();
        }


        protected override Demo ParseInternal<Demo>(Span<byte> bytes, ref int offset)
        {
            var demo = base.ParseInternal<Demo>(bytes, ref offset);

            setSettingsBlocksData();

            skipEmptyBytes(bytes, 4, ref offset);
            var bgm = getUInt16(bytes, ref offset);
            demo.BGM = new SkillReference
            {
                Number = bgm,
                Name = demo.Sounds.Skip(bgm).First().Name,
            };
            var skipWithInput = Convert.ToBoolean(getUInt16(bytes, ref offset));
            skipEmptyBytes(bytes, 1, ref offset);
            var time = getUInt32(bytes, ref offset);
            demo.Time = time;


            skiRemaningEmptyBytes(bytes, ref offset);
            return demo;
        }
        protected override SettingsType getSettingsType(uint skillIdx)
        {
            return SettingsType.None;
        }
    }
}