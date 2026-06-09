using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Fm2ndParser
{
    public class DemoParser : BaseParser
    {

        public DemoParser(string filename, KGT kgt) : base(filename, kgt)
        {
        }

        public Demo Parse()
        {
            return base.parse<Demo>();
        }

        protected override Demo ParseInternal<Demo>(Span<byte> bytes, ref int offset)
        {
            var demo = base.ParseInternal<Demo>(bytes, ref offset);

            skipEmptyBytes(bytes, 4, ref offset);
            var bgm = getUInt16(bytes, ref offset);
            demo.BGM = new SkillReference
            {
                Number = bgm,
                Name = demo.Sounds.Skip(bgm).First().Name,
            };
            var skipWithInput = Convert.ToBoolean(getInt16(bytes, ref offset));
            skipEmptyBytes(bytes, 1, ref offset);
            var time = getUInt32(bytes, ref offset);



            skiRemaningEmptyBytes(bytes, ref offset);
            return demo;
        }
    }
}