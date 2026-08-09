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
        public KGTCompiler(KGTFile kgtFile)
            : base(kgtFile, kgtFile) { }

        override protected void CompileInternal()
        {
            base.CompileInternal();

            writeZeros(4);

            writeCharacters();
            writeHitJunctions();

            writeBytes([0x02]);

            writeZeros(4);

            writeStiffTime();
            writeStages();
            writeDemos();
            writeScreenSelect();
            writeZeros(2);

            writeBaseSettings();
            writeZeros(3);

            writeCommonImages();
            writeBuiltInSkills();

            writeZeros(0x38);

            writeSelectionScreen();
            writeCharactersSettings();

            writeZeros(0x3B2);
        }

        private void writeCharacters()
        {
            for (int i = 0; i < 50; i++)
            {
                var character = _fmFile.Characters.ElementAtOrDefault(i)?.Name ?? "";
                if (character != null)
                    writeString(character, 0x100);
            }
        }

        private void writeHitJunctions()
        {
            for (int i = 0; i < 200; i++)
            {
                var hit = _fmFile.HitJunctions.ElementAtOrDefault(i) ?? new HitJunction { Name = "", Active = false };
                if (hit != null)
                {
                    writeString(hit.Name, 0x20);
                    writeUInt32(hit.Active ? 1u : 0u);
                }
            }
        }

        private void writeStiffTime()
        {
            writeBytes([
                _fmFile.BaseSettings.StiffTime.Hit,
                _fmFile.BaseSettings.StiffTime.Guard,
                _fmFile.BaseSettings.StiffTime.Offset,
            ]);
        }

        private void writeStages()
        {
            for (int i = 0; i < 50; i++)
            {
                var stage = _fmFile.Stages.ElementAtOrDefault(i) ?? "";
                if (stage != null)
                    writeString(stage, 0x100);
            }
        }

        private void writeDemos()
        {
            for (int i = 0; i < 100; i++)
            {
                var demo = _fmFile.Demos.ElementAtOrDefault(i) ?? "";
                if (demo != null)
                    writeString(demo, 0x100);
            }
        }

        private void writeScreenSelect()
        {
            writeBytes([
                _fmFile.BaseSettings.Select.TitleScreen,
                _fmFile.BaseSettings.Select.P1vsCPU,
                _fmFile.BaseSettings.Select.P1vsP2,
                _fmFile.BaseSettings.Select.TeamVSTeam,
                _fmFile.BaseSettings.Select.GameOver,
                _fmFile.BaseSettings.Select.OpeningDemo,
            ]);
        }

        private void writeBaseSettings()
        {
            byte flags = 0;

            if (_fmFile.BaseSettings.Offset) flags |= 1 << 1;
            if (_fmFile.BaseSettings.StoryMode) flags |= 1 << 2;
            if (_fmFile.BaseSettings.VsMode) flags |= 1 << 3;
            if (_fmFile.BaseSettings.VsTeamMode) flags |= 1 << 4;
            if (_fmFile.BaseSettings.LockSource) flags |= 1 << 0;
            if (_fmFile.BaseSettings.NumbersOnHPLifeBar) flags |= 1 << 5;
            if (_fmFile.BaseSettings.CursorAppearsPressingAButton) flags |= 1 << 6;

            writeBytes([flags]);
        }

        private void writeCommonImages()
        {
            for (int i = 0; i < 200; i++)
            {
                var commonImage = _fmFile.CommonImages.ElementAtOrDefault(i) ?? "";
                if (commonImage != null)
                    writeString(commonImage, 0x20);
            }
        }

        private void writeBuiltInSkills()
        {
            var s = _fmFile.BuiltInSkills;
            writeUInt16(s.None);
            writeUInt16(s.HitLetterHit);
            writeUInt16(s.HitNumber0);
            writeUInt16(s.HitNumber1);
            writeUInt16(s.HitNumber2);
            writeUInt16(s.HitNumber3);
            writeUInt16(s.HitNumber4);
            writeUInt16(s.HitNumber5);
            writeUInt16(s.HitNumber6);
            writeUInt16(s.HitNumber7);
            writeUInt16(s.HitNumber8);
            writeUInt16(s.HitNumber9);
            writeUInt16(s.OffsetHitMark);
            writeUInt16(s.RoundAniStarttime);
            writeUInt16(s.RoundAniEndtime);
            writeUInt16(s.Round1);
            writeUInt16(s.Round2);
            writeUInt16(s.Round3);
            writeUInt16(s.Round4);
            writeUInt16(s.Round5);
            writeUInt16(s.Round6);
            writeUInt16(s.Round7);
            writeUInt16(s.Round8);
            writeUInt16(s.Round9);
            writeUInt16(s.RoundFinal);
            writeUInt16(s.Spirits);
            writeUInt16(s.KO);
            writeUInt16(s.Perfect);
            writeUInt16(s.YouWin);
            writeUInt16(s.YouLose);
            writeUInt16(s.P1Wins);
            writeUInt16(s.P2Wins);
            writeUInt16(s.Draw);
            writeUInt16(s.DoubleKo);
            writeUInt16(s.UnlimitedSign);
            writeUInt16(s.TimeNumber0);
            writeUInt16(s.TimeNumber1);
            writeUInt16(s.TimeNumber2);
            writeUInt16(s.TimeNumber3);
            writeUInt16(s.TimeNumber4);
            writeUInt16(s.TimeNumber5);
            writeUInt16(s.TimeNumber6);
            writeUInt16(s.TimeNumber7);
            writeUInt16(s.TimeNumber8);
            writeUInt16(s.TimeNumber9);
            writeUInt16(s.SpecialStockNumber0);
            writeUInt16(s.SpecialStockNumber1);
            writeUInt16(s.SpecialStockNumber2);
            writeUInt16(s.SpecialStockNumber3);
            writeUInt16(s.SpecialStockNumber4);
            writeUInt16(s.SpecialStockNumber5);
            writeUInt16(s.SpecialStockNumber6);
            writeUInt16(s.SpecialStockNumber7);
            writeUInt16(s.SpecialStockNumber8);
            writeUInt16(s.SpecialStockNumber9);
            writeUInt16(s.VictoryMarkOn);
            writeUInt16(s.VictoryMarkOff);
            writeUInt16(s.StageLayout1);
            writeUInt16(s.StageLayout2);
            writeUInt16(s.StageLayout3);
            writeUInt16(s.StageLayout4);
            writeUInt16(s.StageLayout5);
            writeUInt16(s.StageLayout6);
            writeUInt16(s.StageLayout7);
            writeUInt16(s.StageLayout8);
            writeUInt16(s.StageLayout9);
            writeUInt16(s.StageLayout10);
            writeUInt16(s.P1LifeGauge);
            writeUInt16(s.P2LifeGauge);
            writeUInt16(s.P1SpecialGauge);
            writeUInt16(s.P2SpecialGauge);
            writeUInt16(s.PositionTimer);
            writeUInt16(s.Pos1pFace);
            writeUInt16(s.Pos2pFace);
            writeUInt16(s.PosSpecialStock1p);
            writeUInt16(s.PosSpecialStock2p);
            writeUInt16(s.PosVictoryMark1p);
            writeUInt16(s.VPosVictoryMark2p);
            writeUInt16(s.TitleCursor);
            writeUInt16(s.PositionForStoryMode);
            writeUInt16(s.PositionForVsMode);
            writeUInt16(s.ContinuteCursor);
            writeUInt16(s.PositionCursorItDoes);
            writeUInt16(s.PositionCursorItDoesNot);
            writeUInt16(s.P1VsScreenCursor);
            writeUInt16(s.P2VsScreenCursor);
            writeUInt16(s.P1VsScreenCursorAfterInput);
            writeUInt16(s.P2VsScreenCursorAfterInput);
            writeUInt16(s.PosCursorForTeamBattle);
            writeUInt16(s.Pause);
            writeUInt16(s.Spare6);
            writeUInt16(s.Spare7);
            writeUInt16(s.Spare8);
            writeUInt16(s.Spare9);
            writeUInt16(s.Spare10);
            writeUInt16(s.Spare11);
            writeUInt16(s.Spare12);
            writeUInt16(s.Spare13);
            writeUInt16(s.Spare14);
            writeUInt16(s.Spare15);
            writeUInt16(s.Spare16);
            writeUInt16(s.Spare17);
            writeUInt16(s.Spare18);
            writeUInt16(s.Spare19);
        }

        private void writeSelectionScreen()
        {
            var s = _fmFile.SelectionScreen;
            writeUInt16(s.CharStartPosX);
            writeUInt16(s.CharStartPosY);
            writeUInt16(s.DistanceBetweenCharsX);
            writeUInt16(s.DistanceBetweenCharsY);
            writeUInt16(s.Columns);
            writeUInt16(s.Rows);
            writeUInt16(s.P1CursorPosX);
            writeUInt16(s.P1CursorPosY);
            writeInt16(s.P1TeamBattleDiscanceX);
            writeInt16(s.P1TeamBattleDiscanceY);
            writeUInt16(s.P2CursorPosX);
            writeUInt16(s.P2CursorPosY);
            writeInt16(s.P2TeamBattleDiscanceX);
            writeInt16(s.P2TeamBattleDiscanceY);
        }

        private void writeCharactersSettings()
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
                writeBytes([flags]);
            }
        }
    }
}
