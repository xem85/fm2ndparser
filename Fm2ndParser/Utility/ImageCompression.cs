using System;
using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser.Utility
{
    public static class ImageCompression
    {
        public static byte[] Extract(byte[] source, uint destinationSize)
        {
            var destination = new byte[destinationSize];

            var pos = 0;
            var pos2 = 0;

            while (pos < source.Length && pos2 < destinationSize)
            {
                uint tmp = source[pos];
                var tmp2 = tmp >> 6;
                tmp = tmp & 0x3f;

                if (tmp == 0)
                {
                    pos = pos + 1;
                    if (pos >= source.Length)
                        break;

                    tmp = source[pos];
                    if (tmp != 0)
                    {
                        tmp = tmp + 0x3f;
                    }
                    else
                    {
                        if (pos + 3 >= source.Length)
                            break;

                        tmp = BitConverter.ToUInt16(source, pos + 1);
                        var tmp3 = (uint)(source[pos + 3] << 0x10);
                        tmp = tmp + tmp3 + 0x13f;
                        pos = pos + 3;
                    }
                }

                switch (tmp2)
                {
                    case 0:
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            destination[pos2++] = 0;
                        }
                        break;

                    case 1:
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            pos = pos + 1;
                            if (pos >= source.Length)
                                break;

                            destination[pos2++] = source[pos];
                        }
                        break;

                    case 2:
                        pos = pos + 1;
                        if (pos >= source.Length)
                            break;

                        var repeatedByte = source[pos];
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            destination[pos2++] = repeatedByte;
                        }
                        break;

                    case 3:
                        pos = pos + 1;
                        if (pos >= source.Length)
                            break;

                        var copyDistance = (int)source[pos];
                        if (copyDistance == 0)
                        {
                            pos = pos + 1;
                            if (pos >= source.Length)
                                break;

                            copyDistance = (source[pos] + 1) << 8;
                            pos = pos + 1;
                        }

                        var readPos = pos2 - copyDistance;
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            if (readPos < 0 || readPos >= destinationSize)
                                break;

                            destination[pos2++] = destination[readPos++];
                        }
                        break;
                }

                pos = pos + 1;
            }

            return destination;
        }

        public static byte[] Compress(byte[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Length == 0) return Array.Empty<byte>();

            var output = new List<byte>(source.Length);
            var i = 0;

            while (i < source.Length)
            {
                var zeroRun = CountRun(source, i, source[i], onlyZero: true);
                if (zeroRun >= 3)
                {
                    EmitRun(output, 0, zeroRun, null);
                    i += zeroRun;
                    continue;
                }

                var repeatRun = CountRun(source, i, source[i], onlyZero: false);
                if (repeatRun >= 4)
                {
                    EmitRun(output, 2, repeatRun, source[i]);
                    i += repeatRun;
                    continue;
                }

                var backRef = FindBestBackRef(source, i, 0xFF);
                if (backRef.length >= 4)
                {
                    EmitRun(output, 3, backRef.length, (byte)backRef.distance);
                    i += backRef.length;
                    continue;
                }

                var literalStart = i;
                i++;

                while (i < source.Length)
                {
                    var zr = CountRun(source, i, source[i], onlyZero: true);
                    if (zr >= 3)
                        break;

                    var rr = CountRun(source, i, source[i], onlyZero: false);
                    if (rr >= 4)
                        break;

                    var br = FindBestBackRef(source, i, 0xFF);
                    if (br.length >= 4)
                        break;

                    i++;
                }

                EmitLiteral(output, source, literalStart, i - literalStart);
            }

            return output.ToArray();
        }


        private static void EmitLiteral(List<byte> output, byte[] src, int start, int length)
        {
            var remaining = length;
            var pos = start;

            while (remaining > 0)
            {
                var chunk = Math.Min(remaining, 0xFFFFFF + 0x13F);
                WriteTokenHeader(output, 1, chunk);
                for (var k = 0; k < chunk; k++)
                    output.Add(src[pos + k]);

                pos += chunk;
                remaining -= chunk;
            }
        }

        // op: 0=zero run, 2=repeat byte, 3=backref
        private static void EmitRun(List<byte> output, int op, int length, byte? arg)
        {
            var remaining = length;

            while (remaining > 0)
            {
                var chunk = Math.Min(remaining, 0xFFFFFF + 0x13F);
                WriteTokenHeader(output, op, chunk);

                if (op == 2)
                {
                    if (!arg.HasValue)
                        throw new InvalidOperationException("Argomento mancante per repeat-run.");

                    output.Add((byte)arg.Value);
                }
                else if (op == 3)
                {
                    if (!arg.HasValue)
                        throw new InvalidOperationException("Argomento mancante per back-reference.");

                    output.Add(arg.Value);
                }

                remaining -= chunk;
            }
        }

        private static void WriteTokenHeader(List<byte> output, int op, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length <= 0x3F)
            {
                output.Add((byte)((op << 6) | length));
                return;
            }

            // len = ext + 0x3F (ext != 0)
            if (length <= 0x13E)
            {
                output.Add((byte)(op << 6));
                output.Add((byte)(length - 0x3F));
                return;
            }

            // len = u24 + 0x13F
            var baseLength = length - 0x13F;
            output.Add((byte)(op << 6));
            output.Add(0x00);
            output.Add((byte)(baseLength & 0xFF));
            output.Add((byte)((baseLength >> 8) & 0xFF));
            output.Add((byte)((baseLength >> 16) & 0xFF));
        }

        private static int CountRun(byte[] src, int start, byte value, bool onlyZero)
        {
            if (onlyZero && value != 0)
                return 0;

            var i = start + 1;
            while (i < src.Length && src[i] == value)
                i++;

            return i - start;
        }

        private static (int distance, int length) FindBestBackRef(byte[] src, int pos, int maxDistance)
        {
            int bestDist = 0;
            int bestLen = 0;

            int maxDist = Math.Min(maxDistance, pos);
            if (maxDist <= 0) return (0, 0);

            for (int dist = 1; dist <= maxDist; dist++)
            {
                int from = pos - dist;
                int len = 0;

                while (pos + len < src.Length && src[from + len] == src[pos + len])
                {
                    len++;
                    // compatibile con loop di copia sovrapposta del decoder
                    if (from + len >= pos && src[from + len - dist] != src[pos + len - dist]) break;
                }

                if (len > bestLen)
                {
                    bestLen = len;
                    bestDist = dist;
                }
            }

            return (bestDist, bestLen);
        }

    }
}
