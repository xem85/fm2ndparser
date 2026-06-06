using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace Fm2ndParser
{
    public class KGTParser : BaseParser
    {
        public KGTParser(string filename) : base(filename, null)
        {
        }

        public KGT Parse()
        {
            return base.parse<KGT>();
        }

        protected override T ParseInternal<T>(Span<byte> bytes, ref int offset)
        {
            _kgt = base.ParseInternal<KGT>(bytes, ref offset);

            skipEmptyBytes(bytes, 4, ref offset);

            _kgt.Characters = parseCharacters(bytes, ref offset);

            _kgt.HitJunctions = parseHitJuncrions(bytes, ref offset);

            // unknown
            var unknown = getUInt32(bytes, ref offset);
            Debug.Assert(unknown == 2);

            skipEmptyBytes(bytes, 1, ref offset);

            // offsets
            var stiffTime = parseStiffTime(bytes, ref offset);

            _kgt.Stages = parseStages(bytes, ref offset);

            _kgt.Demos = parseDemos(bytes, ref offset);

            var screens = parseScreenSelect(bytes, ref offset);

            // these may be unused screen
            skipEmptyBytes(bytes, 2, ref offset);

            // base settings
            _kgt.BaseSettings = parseBaseSettings(bytes, ref offset);
            _kgt.BaseSettings.StiffTime = stiffTime;
            _kgt.BaseSettings.Select = screens;

            // empty bytes
            skipEmptyBytes(bytes, 3, ref offset);

            _kgt.CommonImages = parseCommonImages(bytes, ref offset);

            // sequence from 0 to 89 ???
            var sequence = getWord(bytes, 256, ref offset);
            
            skipEmptyBytes(bytes, 8, ref offset);

            _kgt.SelectionScreen = parseSelectionScreen(bytes, ref offset);

            setCharactersSettings(bytes, _kgt.Characters, ref offset);

            skiRemaningEmptyBytes(bytes, ref offset);
            return (T)(object)_kgt;
        }

        private void setCharactersSettings(Span<byte> bytes, ICollection<string> characters, ref int offset)
        {
            for (int i = 0; i < 50; i++)
            {
                var flags = getByte(bytes, ref offset);

                var enabledForStoryMode = isFlagOn(flags, 0);
                var enabledForVsMode = isFlagOn(flags, 1);

                // todo
                assertUnusedFlags(flags, 0b11111100);
            }
        }

        private List<string> parseCommonImages(Span<byte> bytes, ref int offset)
        {
            var result = new List<string>();
            for (int i = 0; i < 200; i++)
            {
                var name = getString(bytes, 0x20, ref offset);
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            return result;
        }

        private SelectionScreenSettings parseSelectionScreen(Span<byte> bytes, ref int offset)
        {
            var result = new SelectionScreenSettings
            {
                CharStartPosX = getUInt16(bytes, ref offset),
                CharStartPosY = getUInt16(bytes, ref offset),
                DistanceBetweenCharsX = getUInt16(bytes, ref offset),
                DistanceBetweenCharsY = getUInt16(bytes, ref offset),
                Columns = getUInt16(bytes, ref offset),
                Rows = getUInt16(bytes, ref offset),
                P1CursorPosX = getUInt16(bytes, ref offset),
                P1CursorPosY = getUInt16(bytes, ref offset),
                P1TeamBattleDiscanceX = getInt16(bytes, ref offset),
                P1TeamBattleDiscanceY = getInt16(bytes, ref offset),
                P2CursorPosX = getUInt16(bytes, ref offset),
                P2CursorPosY = getUInt16(bytes, ref offset),
                P2TeamBattleDiscanceX = getInt16(bytes, ref offset),
                P2TeamBattleDiscanceY = getInt16(bytes, ref offset),
            };
            return result;
        }

        private BaseSettings parseBaseSettings(Span<byte> bytes, ref int offset)
        {
            var flags = getByte(bytes, ref offset);
            var baseSettings = new BaseSettings
            {
                Offset = isFlagOn(flags, 1),
                StoryMode = isFlagOn(flags, 2),
                VsMode = isFlagOn(flags, 3),
                VsTeamMode = isFlagOn(flags, 4),
                LockSource = isFlagOn(flags, 0),
                NumbersOnHPLifeBar = isFlagOn(flags, 5),
                CursorAppearsPressingAButton = isFlagOn(flags, 6),
            };
            return baseSettings;
        }

        private ScreenSelect parseScreenSelect(Span<byte> bytes, ref int offset)
        {
            return new ScreenSelect
            {
                TitleScreen = getByte(bytes, ref offset),
                P1vsCPU = getByte(bytes, ref offset),
                P1vsP2 = getByte(bytes, ref offset),
                TeamVSTeam = getByte(bytes, ref offset),
                GameOver = getByte(bytes, ref offset),
                OpeningDemo = getByte(bytes, ref offset)
            };
        }

        private List<string> parseDemos(Span<byte> bytes, ref int offset)
        {
            var result = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                var name = getString(bytes, 0x100, ref offset);
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            return result;
        }

        private List<string> parseStages(Span<byte> bytes, ref int offset)
        {
            var result = new List<string>();
            for (int i = 0; i < 50; i++)
            {
                var name = getString(bytes, 0x100, ref offset);
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            return result;
        }

        private StiffTime parseStiffTime(Span<byte> bytes, ref int offset)
        {
            return new StiffTime
            {
                Hit = getByte(bytes, ref offset),
                Guard = getByte(bytes, ref offset),
                Offset = getByte(bytes, ref offset),
            };
        }

        private List<string> parseHitJuncrions(Span<byte> bytes, ref int offset)
        {
            var result = new List<string>();
            for (int i = 0; i < 200; i++)
            {
                var name = getString(bytes, 0x20, ref offset);
                var active = getUInt32(bytes, ref offset);
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            return result;
        }

        private List<string> parseCharacters(Span<byte> bytes, ref int offset)
        {
            var result = new List<string>();
            for (int i = 0; i < 50; i++)
            {
                var name = getString(bytes, 0x100, ref offset);
                if (!string.IsNullOrEmpty(name))
                    result.Add(name);
            }

            return result;
        }
    }
}