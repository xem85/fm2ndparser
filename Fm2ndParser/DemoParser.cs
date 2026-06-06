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

            //empty
            getInt32(bytes, ref offset);
            var unknown = getInt32(bytes, ref offset);
            skiRemaningEmptyBytes(bytes, ref offset);
            return demo;
        }
    }
}