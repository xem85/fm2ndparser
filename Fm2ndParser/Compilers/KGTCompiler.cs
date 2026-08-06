using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Demo;
using Fm2ndParser.Kgt;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fm2ndParser.Compilers
{
    public class KGTCompiler : BaseCompiler<KGTFile>
    {
        public KGTCompiler(string filename, KGTFile kgtFile)
            : base(filename, kgtFile)
        {

        }

        override protected void CompileInternal(BinaryWriter writer)
        {
            base.CompileInternal(writer);

            writeZeros(writer, 4);

            writeCharacters(writer);
            writeHitJunctions(writer);

            writeBytes(writer, [0x02]);

            writeZeros(writer, 4);

            writeStiffTime(writer);
            writeStages(writer);
            writeDemos(writer);
            writeScreenSelect(writer);
            writeZeros(writer, 2);

            writeBaseSettings(writer);
            writeZeros(writer, 3);

            writeCommonImages(writer);
            writeBuiltInSkills(writer);

            writeZeros(writer, 0x38);

            writeSelectionScreen(writer);
            writeCharactersSettings(writer);

            writeZeros(writer, 0x3B2);
        }

        private void writeCharacters(BinaryWriter writer)
        {
            for (int i = 0; i < 50; i++)
            {
                var character = _fmFile.Characters.ElementAtOrDefault(i)?.Name ?? "";
                if (character != null)
                    writeString(writer, character, 0x100);
            }
        }

        private void writeHitJunctions(BinaryWriter writer)
        {
            for (int i = 0; i < 200; i++)
            {
                var hit = _fmFile.HitJunctions.ElementAtOrDefault(i) ?? new HitJunction { Name = "", Active = false };
                if (hit != null)
                {
                    writeString(writer, hit.Name, 0x20);
                    writeUInt32(writer, hit.Active ? 1u : 0u);
                }
            }
        }

        private void writeStiffTime(BinaryWriter writer)
        {
            writeBytes(writer, [
                _fmFile.BaseSettings.StiffTime.Hit,
                _fmFile.BaseSettings.StiffTime.Guard,
                _fmFile.BaseSettings.StiffTime.Offset,
            ]);
        }

        private void writeStages(BinaryWriter writer)
        {
            for (int i = 0; i < 50; i++)
            {
                var stage = _fmFile.Stages.ElementAtOrDefault(i) ?? "";
                if (stage != null)
                    writeString(writer, stage, 0x100);
            }
        }

        private void writeDemos(BinaryWriter writer)
        {
            for (int i = 0; i < 100; i++)
            {
                var demo = _fmFile.Demos.ElementAtOrDefault(i) ?? "";
                if (demo != null)
                    writeString(writer, demo, 0x100);
            }
        }

        private void writeScreenSelect(BinaryWriter writer)
        {
            writeBytes(writer, [
                _fmFile.BaseSettings.Select.TitleScreen,
                _fmFile.BaseSettings.Select.P1vsCPU,
                _fmFile.BaseSettings.Select.P1vsP2,
                _fmFile.BaseSettings.Select.TeamVSTeam,
                _fmFile.BaseSettings.Select.GameOver,
                _fmFile.BaseSettings.Select.OpeningDemo,
            ]);
        }

        private void writeBaseSettings(BinaryWriter writer)
        {
            byte flags = 0;

            if (_fmFile.BaseSettings.Offset) flags |= 1 << 1;
            if (_fmFile.BaseSettings.StoryMode) flags |= 1 << 2;
            if (_fmFile.BaseSettings.VsMode) flags |= 1 << 3;
            if (_fmFile.BaseSettings.VsTeamMode) flags |= 1 << 4;
            if (_fmFile.BaseSettings.LockSource) flags |= 1 << 0;
            if (_fmFile.BaseSettings.NumbersOnHPLifeBar) flags |= 1 << 5;
            if (_fmFile.BaseSettings.CursorAppearsPressingAButton) flags |= 1 << 6;

            writeBytes(writer, [flags]);
        }

        private void writeCommonImages(BinaryWriter writer)
        {
            for (int i = 0; i < 200; i++)
            {
                var commonImage = _fmFile.CommonImages.ElementAtOrDefault(i) ?? "";
                if (commonImage != null)
                    writeString(writer, commonImage, 0x20);
            }
        }

        private void writeBuiltInSkills(BinaryWriter writer)
        {
            var s = _fmFile.BuiltInSkills;
            writeUInt16(writer, s.None);
            writeUInt16(writer, s.HitLetterHit);
            writeUInt16(writer, s.HitNumber0);
            writeUInt16(writer, s.HitNumber1);
            writeUInt16(writer, s.HitNumber2);
            writeUInt16(writer, s.HitNumber3);
            writeUInt16(writer, s.HitNumber4);
            writeUInt16(writer, s.HitNumber5);
            writeUInt16(writer, s.HitNumber6);
            writeUInt16(writer, s.HitNumber7);
            writeUInt16(writer, s.HitNumber8);
            writeUInt16(writer, s.HitNumber9);
            writeUInt16(writer, s.OffsetHitMark);
            writeUInt16(writer, s.RoundAniStarttime);
            writeUInt16(writer, s.RoundAniEndtime);
            writeUInt16(writer, s.Round1);
            writeUInt16(writer, s.Round2);
            writeUInt16(writer, s.Round3);
            writeUInt16(writer, s.Round4);
            writeUInt16(writer, s.Round5);
            writeUInt16(writer, s.Round6);
            writeUInt16(writer, s.Round7);
            writeUInt16(writer, s.Round8);
            writeUInt16(writer, s.Round9);
            writeUInt16(writer, s.RoundFinal);
            writeUInt16(writer, s.Spirits);
            writeUInt16(writer, s.KO);
            writeUInt16(writer, s.Perfect);
            writeUInt16(writer, s.YouWin);
            writeUInt16(writer, s.YouLose);
            writeUInt16(writer, s.P1Wins);
            writeUInt16(writer, s.P2Wins);
            writeUInt16(writer, s.Draw);
            writeUInt16(writer, s.DoubleKo);
            writeUInt16(writer, s.UnlimitedSign);
            writeUInt16(writer, s.TimeNumber0);
            writeUInt16(writer, s.TimeNumber1);
            writeUInt16(writer, s.TimeNumber2);
            writeUInt16(writer, s.TimeNumber3);
            writeUInt16(writer, s.TimeNumber4);
            writeUInt16(writer, s.TimeNumber5);
            writeUInt16(writer, s.TimeNumber6);
            writeUInt16(writer, s.TimeNumber7);
            writeUInt16(writer, s.TimeNumber8);
            writeUInt16(writer, s.TimeNumber9);
            writeUInt16(writer, s.SpecialStockNumber0);
            writeUInt16(writer, s.SpecialStockNumber1);
            writeUInt16(writer, s.SpecialStockNumber2);
            writeUInt16(writer, s.SpecialStockNumber3);
            writeUInt16(writer, s.SpecialStockNumber4);
            writeUInt16(writer, s.SpecialStockNumber5);
            writeUInt16(writer, s.SpecialStockNumber6);
            writeUInt16(writer, s.SpecialStockNumber7);
            writeUInt16(writer, s.SpecialStockNumber8);
            writeUInt16(writer, s.SpecialStockNumber9);
            writeUInt16(writer, s.VictoryMarkOn);
            writeUInt16(writer, s.VictoryMarkOff);
            writeUInt16(writer, s.StageLayout1);
            writeUInt16(writer, s.StageLayout2);
            writeUInt16(writer, s.StageLayout3);
            writeUInt16(writer, s.StageLayout4);
            writeUInt16(writer, s.StageLayout5);
            writeUInt16(writer, s.StageLayout6);
            writeUInt16(writer, s.StageLayout7);
            writeUInt16(writer, s.StageLayout8);
            writeUInt16(writer, s.StageLayout9);
            writeUInt16(writer, s.StageLayout10);
            writeUInt16(writer, s.P1LifeGauge);
            writeUInt16(writer, s.P2LifeGauge);
            writeUInt16(writer, s.P1SpecialGauge);
            writeUInt16(writer, s.P2SpecialGauge);
            writeUInt16(writer, s.PositionTimer);
            writeUInt16(writer, s.Pos1pFace);
            writeUInt16(writer, s.Pos2pFace);
            writeUInt16(writer, s.PosSpecialStock1p);
            writeUInt16(writer, s.PosSpecialStock2p);
            writeUInt16(writer, s.PosVictoryMark1p);
            writeUInt16(writer, s.VPosVictoryMark2p);
            writeUInt16(writer, s.TitleCursor);
            writeUInt16(writer, s.PositionForStoryMode);
            writeUInt16(writer, s.PositionForVsMode);
            writeUInt16(writer, s.ContinuteCursor);
            writeUInt16(writer, s.PositionCursorItDoes);
            writeUInt16(writer, s.PositionCursorItDoesNot);
            writeUInt16(writer, s.P1VsScreenCursor);
            writeUInt16(writer, s.P2VsScreenCursor);
            writeUInt16(writer, s.P1VsScreenCursorAfterInput);
            writeUInt16(writer, s.P2VsScreenCursorAfterInput);
            writeUInt16(writer, s.PosCursorForTeamBattle);
            writeUInt16(writer, s.Pause);
            writeUInt16(writer, s.Spare6);
            writeUInt16(writer, s.Spare7);
            writeUInt16(writer, s.Spare8);
            writeUInt16(writer, s.Spare9);
            writeUInt16(writer, s.Spare10);
            writeUInt16(writer, s.Spare11);
            writeUInt16(writer, s.Spare12);
            writeUInt16(writer, s.Spare13);
            writeUInt16(writer, s.Spare14);
            writeUInt16(writer, s.Spare15);
            writeUInt16(writer, s.Spare16);
            writeUInt16(writer, s.Spare17);
            writeUInt16(writer, s.Spare18);
            writeUInt16(writer, s.Spare19);
        }

        private void writeSelectionScreen(BinaryWriter writer)
        {
            var s = _fmFile.SelectionScreen;
            writeUInt16(writer, s.CharStartPosX);
            writeUInt16(writer, s.CharStartPosY);
            writeUInt16(writer, s.DistanceBetweenCharsX);
            writeUInt16(writer, s.DistanceBetweenCharsY);
            writeUInt16(writer, s.Columns);
            writeUInt16(writer, s.Rows);
            writeUInt16(writer, s.P1CursorPosX);
            writeUInt16(writer, s.P1CursorPosY);
            writeInt16(writer, s.P1TeamBattleDiscanceX);
            writeInt16(writer, s.P1TeamBattleDiscanceY);
            writeUInt16(writer, s.P2CursorPosX);
            writeUInt16(writer, s.P2CursorPosY);
            writeInt16(writer, s.P2TeamBattleDiscanceX);
            writeInt16(writer, s.P2TeamBattleDiscanceY);
        }

        private void writeCharactersSettings(BinaryWriter writer)
        {
            for (int i = 0; i < 50; i++)
            {
                var character = _fmFile.Characters.ElementAtOrDefault(i);

                byte flags = 0;
                if (character != null)
                {
                    if (character.EnabledForStoryMode) flags |= 1 << 0;
                    if (character.EnabledForVsMode) flags |= 1 << 1;
                }
                writeBytes(writer, [flags]);
            }
        }
    }
}
