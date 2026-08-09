using System;
using System.Collections.Generic;
using System.Text;

namespace Fm2ndParser.Utility
{
    public static class ByteUtility
    {
        public static uint CreateBitMask(int start, int length)
        {
            uint mask = 0xffffffff;
            mask >>= 32 - length;
            mask <<= start;
            return mask;
        }
    }
}
