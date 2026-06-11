using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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

            _kgt.BuiltInSkills = parseBuiltInSkills(bytes, ref offset);

            setSettingsBlocksData();

            skipEmptyBytes(bytes, 0x38, ref offset);

            _kgt.SelectionScreen = parseSelectionScreen(bytes, ref offset);

            setCharactersSettings(bytes, _kgt.Characters, ref offset);

            skiRemaningEmptyBytes(bytes, ref offset);
            return (T)(object)_kgt;
        }

        private KGTBuiltInSkills parseBuiltInSkills(Span<byte> bytes, ref int offset)
        {
            var shSkillIdxNone = getUInt16(bytes, ref offset);
            var shSkillIdxHitLetterHit = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber0 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber1 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber2 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber3 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber4 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber5 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber6 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber7 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber8 = getUInt16(bytes, ref offset);
            var shSkillIdxHitNumber9 = getUInt16(bytes, ref offset);
            var shSkillIdxOffsetHitMark = getUInt16(bytes, ref offset);
            var shSkillIdxRoundAniStarttime = getUInt16(bytes, ref offset);
            var shSkillIdxRoundAniEndtime = getUInt16(bytes, ref offset);
            var shSkillIdxRound1 = getUInt16(bytes, ref offset);
            var shSkillIdxRound2 = getUInt16(bytes, ref offset);
            var shSkillIdxRound3 = getUInt16(bytes, ref offset);
            var shSkillIdxRound4 = getUInt16(bytes, ref offset);
            var shSkillIdxRound5 = getUInt16(bytes, ref offset);
            var shSkillIdxRound6 = getUInt16(bytes, ref offset);
            var shSkillIdxRound7 = getUInt16(bytes, ref offset);
            var shSkillIdxRound8 = getUInt16(bytes, ref offset);
            var shSkillIdxRound9 = getUInt16(bytes, ref offset);
            var shSkillIdxRoundFinal = getUInt16(bytes, ref offset);
            var shSkillIdxSpirits = getUInt16(bytes, ref offset);
            var shSkillIdxKO = getUInt16(bytes, ref offset);
            var shSkillIdxPerfect = getUInt16(bytes, ref offset);
            var shSkillIdxYouWin = getUInt16(bytes, ref offset);
            var shSkillIdxYouLose = getUInt16(bytes, ref offset);
            var shSkillIdx1pWins = getUInt16(bytes, ref offset);
            var shSkillIdx2pWins = getUInt16(bytes, ref offset);
            var shSkillIdxDraw = getUInt16(bytes, ref offset);
            var shSkillIdxDoubleKo = getUInt16(bytes, ref offset);
            var shSkillIdxUnlimitedSign = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber0 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber1 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber2 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber3 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber4 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber5 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber6 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber7 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber8 = getUInt16(bytes, ref offset);
            var shSkillIdxTimeNumber9 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber0 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber1 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber2 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber3 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber4 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber5 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber6 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber7 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber8 = getUInt16(bytes, ref offset);
            var shSkillIdxSpecialStockNumber9 = getUInt16(bytes, ref offset);
            var shSkillIdxVictoryMarkOn = getUInt16(bytes, ref offset);
            var shSkillIdxVictoryMarkOff = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout1 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout2 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout3 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout4 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout5 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout6 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout7 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout8 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout9 = getUInt16(bytes, ref offset);
            var shSkillIdxStageLayout10 = getUInt16(bytes, ref offset);
            var shSkillIdx1pLifeGauge = getUInt16(bytes, ref offset);
            var shSkillIdx2pLifeGauge = getUInt16(bytes, ref offset);
            var shSkillIdx1pSpecialGauge = getUInt16(bytes, ref offset);
            var shSkillIdx2pSpecialGauge = getUInt16(bytes, ref offset);
            var shSkillIdxPositionTimer = getUInt16(bytes, ref offset);
            var shSkillIdxPos1pFace = getUInt16(bytes, ref offset);
            var shSkillIdxPos2pFace = getUInt16(bytes, ref offset);
            var shSkillIdxPosSpecialStock1p = getUInt16(bytes, ref offset);
            var shSkillIdxPosSpecialStock2p = getUInt16(bytes, ref offset);
            var shSkillIdxPosVictoryMark1p = getUInt16(bytes, ref offset);
            var shSkillIdxVPosVictoryMark2p = getUInt16(bytes, ref offset);
            var shSkillIdxTitleCursor = getUInt16(bytes, ref offset);
            var shSkillIdxPositionForStoryMode = getUInt16(bytes, ref offset);
            var shSkillIdxPositionForVsMode = getUInt16(bytes, ref offset);
            var shSkillIdxContinuteCursor = getUInt16(bytes, ref offset);
            var shSkillIdxPositionCursorItDoes = getUInt16(bytes, ref offset);
            var shSkillIdxPositionCursorItDoesNot = getUInt16(bytes, ref offset);
            var shSkillIdx1pVsScreenCursor = getUInt16(bytes, ref offset);
            var shSkillIdx2pVsScreenCursor = getUInt16(bytes, ref offset);
            var shSkillIdx1pVsScreenCursorAfterInput = getUInt16(bytes, ref offset);
            var shSkillIdx2pVsScreenCursorAfterInput = getUInt16(bytes, ref offset);
            var shSkillIdxPosCursorForTeamBattle = getUInt16(bytes, ref offset);
            var shSkillIdxPause = getUInt16(bytes, ref offset);
            var skill_idx_spare6 = getUInt16(bytes, ref offset);
            var skill_idx_spare7 = getUInt16(bytes, ref offset);
            var skill_idx_spare8 = getUInt16(bytes, ref offset);
            var skill_idx_spare9 = getUInt16(bytes, ref offset);
            var skill_idx_spare10 = getUInt16(bytes, ref offset);
            var skill_idx_spare11 = getUInt16(bytes, ref offset);
            var skill_idx_spare12 = getUInt16(bytes, ref offset);
            var skill_idx_spare13 = getUInt16(bytes, ref offset);
            var skill_idx_spare14 = getUInt16(bytes, ref offset);
            var skill_idx_spare15 = getUInt16(bytes, ref offset);
            var skill_idx_spare16 = getUInt16(bytes, ref offset);
            var skill_idx_spare17 = getUInt16(bytes, ref offset);
            var skill_idx_spare18 = getUInt16(bytes, ref offset);
            var skill_idx_spare19 = getUInt16(bytes, ref offset);

            var result = new KGTBuiltInSkills
            {
                None = shSkillIdxNone,
                HitLetterHit = shSkillIdxHitLetterHit,
                HitNumber0 = shSkillIdxHitNumber0,
                HitNumber1 = shSkillIdxHitNumber1,
                HitNumber2 = shSkillIdxHitNumber2,
                HitNumber3 = shSkillIdxHitNumber3,
                HitNumber4 = shSkillIdxHitNumber4,
                HitNumber5 = shSkillIdxHitNumber5,
                HitNumber6 = shSkillIdxHitNumber6,
                HitNumber7 = shSkillIdxHitNumber7,
                HitNumber8 = shSkillIdxHitNumber8,
                HitNumber9 = shSkillIdxHitNumber9,
                OffsetHitMark = shSkillIdxOffsetHitMark,
                RoundAniStarttime = shSkillIdxRoundAniStarttime,
                RoundAniEndtime = shSkillIdxRoundAniEndtime,
                Round1 = shSkillIdxRound1,
                Round2 = shSkillIdxRound2,
                Round3 = shSkillIdxRound3,
                Round4 = shSkillIdxRound4,
                Round5 = shSkillIdxRound5,
                Round6 = shSkillIdxRound6,
                Round7 = shSkillIdxRound7,
                Round8 = shSkillIdxRound8,
                Round9 = shSkillIdxRound9,
                RoundFinal = shSkillIdxRoundFinal,
                Spirits = shSkillIdxSpirits,
                KO = shSkillIdxKO,
                Perfect = shSkillIdxPerfect,
                YouWin = shSkillIdxYouWin,
                YouLose = shSkillIdxYouLose,
                P1Wins = shSkillIdx1pWins,
                P2Wins = shSkillIdx2pWins,
                Draw = shSkillIdxDraw,
                DoubleKo = shSkillIdxDoubleKo,
                UnlimitedSign = shSkillIdxUnlimitedSign,
                TimeNumber0 = shSkillIdxTimeNumber0,
                TimeNumber1 = shSkillIdxTimeNumber1,
                TimeNumber2 = shSkillIdxTimeNumber2,
                TimeNumber3 = shSkillIdxTimeNumber3,
                TimeNumber4 = shSkillIdxTimeNumber4,
                TimeNumber5 = shSkillIdxTimeNumber5,
                TimeNumber6 = shSkillIdxTimeNumber6,
                TimeNumber7 = shSkillIdxTimeNumber7,
                TimeNumber8 = shSkillIdxTimeNumber8,
                TimeNumber9 = shSkillIdxTimeNumber9,
                VictoryMarkOn = shSkillIdxVictoryMarkOn,
                VictoryMarkOff = shSkillIdxVictoryMarkOff,
                StageLayout1 = shSkillIdxStageLayout1,
                StageLayout2 = shSkillIdxStageLayout2,
                StageLayout3 = shSkillIdxStageLayout3,
                StageLayout4 = shSkillIdxStageLayout4,
                StageLayout5 = shSkillIdxStageLayout5,
                StageLayout6 = shSkillIdxStageLayout6,
                StageLayout7 = shSkillIdxStageLayout7,
                StageLayout8 = shSkillIdxStageLayout8,
                StageLayout9 = shSkillIdxStageLayout9,
                StageLayout10 = shSkillIdxStageLayout10,
                P1LifeGauge = shSkillIdx1pLifeGauge,
                P2LifeGauge = shSkillIdx2pLifeGauge,
                P1SpecialGauge = shSkillIdx1pSpecialGauge,
                P2SpecialGauge = shSkillIdx2pSpecialGauge,
                PositionTimer = shSkillIdxPositionTimer,
                Pos1pFace = shSkillIdxPos1pFace,
                Pos2pFace = shSkillIdxPos2pFace,
                PosSpecialStock1p = shSkillIdxPosSpecialStock1p,
                PosSpecialStock2p = shSkillIdxPosSpecialStock2p,
                PosVictoryMark1p = shSkillIdxPosVictoryMark1p,
                VPosVictoryMark2p = shSkillIdxVPosVictoryMark2p,
                TitleCursor = shSkillIdxTitleCursor,
                PositionForStoryMode = shSkillIdxPositionForStoryMode,
                PositionForVsMode = shSkillIdxPositionForVsMode,
                ContinuteCursor = shSkillIdxContinuteCursor,
                PositionCursorItDoes = shSkillIdxPositionCursorItDoes,
                PositionCursorItDoesNot = shSkillIdxPositionCursorItDoesNot,
                P1VsScreenCursor = shSkillIdx1pVsScreenCursor,
                P2VsScreenCursor = shSkillIdx2pVsScreenCursor,
                P1VsScreenCursorAfterInput = shSkillIdx1pVsScreenCursorAfterInput,
                P2VsScreenCursorAfterInput = shSkillIdx2pVsScreenCursorAfterInput,
                PosCursorForTeamBattle = shSkillIdxPosCursorForTeamBattle,
                Pause = shSkillIdxPause,
                Spare6 = skill_idx_spare6,
                Spare7 = skill_idx_spare7,
                Spare8 = skill_idx_spare8,
                Spare9 = skill_idx_spare9,
                Spare10 = skill_idx_spare10,
                Spare11 = skill_idx_spare11,
                Spare12 = skill_idx_spare12,
                Spare13 = skill_idx_spare13,
                Spare14 = skill_idx_spare14,
                Spare15 = skill_idx_spare15,
                Spare16 = skill_idx_spare16,
                Spare17 = skill_idx_spare17,
                Spare18 = skill_idx_spare18,
                Spare19 = skill_idx_spare19,
            };
            return result;
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

        protected override SettingsType getSettingsType(uint skillIdx)
        {
            if (skillIdx == _kgt.BuiltInSkills.HitLetterHit)
                return SettingsType.HitMark;

            var timeSkills = new List<uint>
            {
                _kgt.BuiltInSkills.RoundAniStarttime,
                _kgt.BuiltInSkills.RoundAniEndtime,
                _kgt.BuiltInSkills.Round1,
                _kgt.BuiltInSkills.Round2,
                _kgt.BuiltInSkills.Round3,
                _kgt.BuiltInSkills.Round4,
                _kgt.BuiltInSkills.Round5,
                _kgt.BuiltInSkills.Round6,
                _kgt.BuiltInSkills.Round7,
                _kgt.BuiltInSkills.Round8,
                _kgt.BuiltInSkills.Round9,
                _kgt.BuiltInSkills.RoundFinal,
                _kgt.BuiltInSkills.Spirits,
                _kgt.BuiltInSkills.KO,
                _kgt.BuiltInSkills.Perfect,
                _kgt.BuiltInSkills.YouWin,
                _kgt.BuiltInSkills.YouLose,
                _kgt.BuiltInSkills.P1Wins,
                _kgt.BuiltInSkills.P2Wins,
                _kgt.BuiltInSkills.Draw,
                _kgt.BuiltInSkills.DoubleKo,
            };

            if (timeSkills.Contains(skillIdx))
                return SettingsType.Time;

            var positionSkills = new List<uint>
            {
                _kgt.BuiltInSkills.PositionTimer,
                _kgt.BuiltInSkills.Pos1pFace,
                _kgt.BuiltInSkills.Pos2pFace,
                _kgt.BuiltInSkills.PosSpecialStock1p,
                _kgt.BuiltInSkills.PosSpecialStock2p,
                _kgt.BuiltInSkills.PositionForStoryMode,
                _kgt.BuiltInSkills.PositionForVsMode,
                _kgt.BuiltInSkills.PositionCursorItDoes,
                _kgt.BuiltInSkills.PositionCursorItDoesNot,
                _kgt.BuiltInSkills.PosCursorForTeamBattle,
            };

            if (positionSkills.Contains(skillIdx))
                return SettingsType.Position;

            var markSkills = new List<uint>
            {
                _kgt.BuiltInSkills.PosVictoryMark1p,
                _kgt.BuiltInSkills.VPosVictoryMark2p,
            };
            if (markSkills.Contains(skillIdx))
                return SettingsType.MarkPosition;

            return SettingsType.None;
        }
    }
}