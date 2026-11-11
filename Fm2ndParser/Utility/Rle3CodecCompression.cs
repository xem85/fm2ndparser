using System;
using System.Collections.Generic;

namespace Fm2ndParser.Utility
{

    /// <summary>
    /// Compression algorithm that is the counterpart of the provided Extract function
    /// (2D Fighter Maker 2nd). Reproduces the original compressor's output
    /// EXACTLY, byte for byte, all bit-identical to the file produced by the original tool.
    ///
    /// ==================== FORMAT ====================
    /// 1 control byte per token:
    ///   bits 7-6 = opcode
    ///   bits 5-0 = count (1..63)
    ///
    /// If count == 0 in the control byte, the real count is read from following bytes:
    ///   - if the next byte != 0  -> count = that byte + 63              (range 64..318)
    ///   - if the next byte == 0  -> count = 24-bit little-endian value
    ///                                read from the following 3 bytes + 319
    ///                                (range 319..319+0xFFFFFF)
    ///
    /// Opcodes:
    ///   0 = fill with `count` zero bytes (no payload)
    ///   1 = literal copy of `count` bytes (payload = count raw bytes)
    ///   2 = repeat one byte `count` times (payload = 1 byte, the value to repeat)
    ///   3 = back-reference: copy `count` bytes from `distance` positions back
    ///       (payload = 1 distance byte, 1..255; the original tool never uses the "long
    ///        distance" form that Extract also supports, so this encoder never emits it)
    ///
    /// ================ SELECTION RULE (reverse-engineered empirically) ================
    /// At every position only two quantities are computed:
    ///   RUN   = length of the run of identical bytes starting there (used both for
    ///           zero-fill and byte-fill: it is the exact same measurement, only the
    ///           resulting opcode/payload differs depending on whether the repeated
    ///           byte is 0 or not)
    ///   MATCH = length of the best back-reference found in history, searching ONLY at
    ///           distance 2..255 (distance 1 is excluded up front: it is equivalent to
    ///           RUN and is always handled by that, never by an explicit back-reference)
    ///
    /// The original tool does NOT compare the byte cost of the options: it simply applies
    /// this fixed priority:
    ///   1) if MATCH >= RUN (even on a tie!) and MATCH >= 3  -> back-reference (opcode 3)
    ///   2) otherwise, if RUN >= 3                           -> zero-fill/byte-fill (opcode 0/2)
    ///   3) otherwise                                        -> literal (opcode 1)
    /// When several distances tie for the same match length, the LARGEST distance always
    /// wins (the oldest match found in history).
    /// ===================================================================================
    /// </summary>
    public static class Rle3CodecCompression
    {
        private const int OP_ZERO = 0;
        private const int OP_LITERAL = 1;
        private const int OP_FILL = 2;
        private const int OP_COPY = 3;

        private const int MIN_LEN = 3;        // minimum length worth encoding as a dedicated token
        private const int MAX_DIST = 255;     // match search window (never wider than this)
        private const long MAX_SINGLE_COUNT = 319L + 0xFFFFFF; // max count fitting in one token

        public static byte[] Compress(byte[] source)
        {
            var output = new List<byte>();
            var dict = new Dictionary<int, List<int>>();
            int pos = 0;
            int n = source.Length;

            while (pos < n)
            {
                int run = RunLength(source, pos);
                FindMatch(source, pos, dict, out int matchLen, out int matchDist);

                if (matchLen >= MIN_LEN && matchLen >= run)
                {
                    WriteHeader(output, OP_COPY, matchLen);
                    WriteDistance(output, matchDist);
                    IndexRange(dict, source, pos, matchLen);
                    pos += matchLen;
                }
                else if (run >= MIN_LEN)
                {
                    int opcode = source[pos] == 0 ? OP_ZERO : OP_FILL;
                    byte? value = source[pos] == 0 ? (byte?)null : source[pos];
                    EmitFill(output, opcode, run, value);
                    IndexRange(dict, source, pos, run);
                    pos += run;
                }
                else
                {
                    int litStart = pos;
                    IndexPosition(dict, source, pos);
                    pos++;

                    while (pos < n)
                    {
                        int r = RunLength(source, pos);
                        FindMatch(source, pos, dict, out int ml, out _);
                        if (r >= MIN_LEN || ml >= MIN_LEN) break;

                        IndexPosition(dict, source, pos);
                        pos++;
                    }

                    EmitLiteral(output, source, litStart, pos - litStart);
                }
            }

            return output.ToArray();
        }

        // ---------- token emission ----------

        private static void EmitFill(List<byte> output, int opcode, int totalCount, byte? value)
        {
            int remaining = totalCount;
            while (remaining > 0)
            {
                int chunk = (int)Math.Min(remaining, MAX_SINGLE_COUNT);
                WriteHeader(output, opcode, chunk);
                if (value.HasValue) output.Add(value.Value);
                remaining -= chunk;
            }
        }

        private static void EmitLiteral(List<byte> output, byte[] source, int start, int totalLen)
        {
            int remaining = totalLen;
            int cur = start;
            while (remaining > 0)
            {
                int chunk = (int)Math.Min(remaining, MAX_SINGLE_COUNT);
                WriteHeader(output, OP_LITERAL, chunk);
                for (int i = 0; i < chunk; i++)
                    output.Add(source[cur + i]);
                cur += chunk;
                remaining -= chunk;
            }
        }

        private static void WriteHeader(List<byte> output, int opcode, long count)
        {
            if (count >= 1 && count <= 63)
            {
                output.Add((byte)((opcode << 6) | count));
            }
            else if (count >= 64 && count <= 318)
            {
                output.Add((byte)(opcode << 6));
                output.Add((byte)(count - 63));
            }
            else if (count >= 319 && count <= MAX_SINGLE_COUNT)
            {
                long ext = count - 319;
                output.Add((byte)(opcode << 6));
                output.Add(0);
                output.Add((byte)(ext & 0xFF));
                output.Add((byte)((ext >> 8) & 0xFF));
                output.Add((byte)((ext >> 16) & 0xFF));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private static void WriteDistance(List<byte> output, int distance)
        {
            if (distance >= 1 && distance <= MAX_DIST)
            {
                output.Add((byte)distance);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(distance),
                    "Distance outside the supported search window (1..255).");
            }
        }

        // ---------- match search ----------

        /// <summary>
        /// Finds the best back-reference at distance 2..255 (distance 1 is excluded: it is
        /// equivalent to a run and is always handled by that, never by an opcode-3 token).
        /// The largest distance wins on a length tie: candidates are therefore scanned from
        /// the oldest (largest distance) to the most recent, updating the best match only on
        /// a strict improvement (>), so the first one found (the oldest) stays the winner on
        /// a tie.
        /// </summary>
        private static void FindMatch(byte[] data, int pos, Dictionary<int, List<int>> dict, out int bestLen, out int bestDist)
        {
            bestLen = 0;
            bestDist = 0;
            int n = data.Length;
            if (pos + MIN_LEN > n) return;

            int key = Hash3(data, pos);
            if (!dict.TryGetValue(key, out var candidates)) return;

            foreach (int cand in candidates)
            {
                int dist = pos - cand;
                if (dist <= 1 || dist > MAX_DIST) continue;

                int maxLen = n - pos;
                int len = 0;
                while (len < maxLen && data[cand + len] == data[pos + len]) len++;

                if (len > bestLen)
                {
                    bestLen = len;
                    bestDist = dist;
                }
            }
        }

        private static int Hash3(byte[] data, int pos)
        {
            return (data[pos] << 16) | (data[pos + 1] << 8) | data[pos + 2];
        }

        private static void IndexPosition(Dictionary<int, List<int>> dict, byte[] data, int pos)
        {
            if (pos + 3 > data.Length) return;
            int key = Hash3(data, pos);
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<int>();
                dict[key] = list;
            }
            list.Add(pos);
        }

        private static void IndexRange(Dictionary<int, List<int>> dict, byte[] data, int start, int len)
        {
            int end = Math.Min(start + len, data.Length);
            for (int i = start; i < end; i++)
                IndexPosition(dict, data, i);
        }

        private static int RunLength(byte[] data, int pos)
        {
            byte b = data[pos];
            int i = pos + 1;
            int n = data.Length;
            while (i < n && data[i] == b) i++;
            return i - pos;
        }

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
    }
}