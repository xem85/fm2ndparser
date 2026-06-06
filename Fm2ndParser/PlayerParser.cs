using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Fm2ndParser
{
    public class PlayerParser : BaseParser
    {

        public PlayerParser(string filename, KGT kgt) : base(filename, kgt)
        {
        }

        public Player Parse()
        {
            return base.parse<Player>();
        }

        protected override Player ParseInternal<Player>(Span<byte> bytes, ref int offset)
        {
            var player = base.ParseInternal<Player>(bytes, ref offset);

            //empty
            getInt32(bytes, ref offset);
            var commandsCount = getInt32(bytes, ref offset);
            var commandsSkills = readCommands(commandsCount, bytes, ref offset);
            var commandBlocksCount = getInt16(bytes, ref offset);
            // empty skill
            getWord(bytes, 2, ref offset);

            return player;
        }
    }
}