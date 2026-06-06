using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Fm2ndParser
{
    public class StageParser : BaseParser
    {

        public StageParser(string filename, KGT kgt) : base(filename, kgt)
        {
        }

        public Stage Parse()
        {
            return base.parse<Stage>();
        }

        protected override Stage ParseInternal<Stage>(Span<byte> bytes, ref int offset)
        {
            var stage = base.ParseInternal<Stage>(bytes, ref offset);

            //empty
            getInt32(bytes, ref offset);
            var unknown = getInt32(bytes, ref offset);
            skiRemaningEmptyBytes(bytes, ref offset);
            return stage;
        }
    }
}