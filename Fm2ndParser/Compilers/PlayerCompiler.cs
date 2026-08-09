using Fm2ndParser.Character;
using Fm2ndParser.Character.Story;
using Fm2ndParser.Kgt;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fm2ndParser.Compilers
{
    public class PlayerCompiler : BaseCompiler<PlayerFile>
    {
        public PlayerCompiler(PlayerFile playerFile, KGTFile kgtFile)
            : base(playerFile, kgtFile) { }

        override protected void CompileInternal()
        {
            base.CompileInternal();

            writeZeros(4);

            writeCommands();
            writeHitJunctionsSkills();
            writeCommonImages();

            writeZeros(10);

            writeCpus();
            writeDefaultSkillsIndex();
            writeZeros(0x26);
            writePlayerSettings();
            writeStoryMode();
            writeZeros(8);
        }

        private void writeCommands()
        {
            writeUInt32((uint)_fmFile.Commands.Count);

            foreach (var command in _fmFile.Commands)
            {
                writeString(command.Name, 32);
                writeUInt16(command.Time);
                writeSkillReference(command.AirSkill);
                writeSkillReference(command.StandSkill);
                writeSkillReference(command.StandFarSkill);
                writeSkillReference(command.CrouchedSkill);

                foreach (var step in command.Steps)
                {
                    writeCommandStep(step);
                }

                foreach (var step in command.Steps)
                {
                    writeUInt16(step.Amount);
                }
            }
        }

        private void writeHitJunctionsSkills()
        {
            writeUInt32((uint)_fmFile.HitJunctionsSkills.Count);

            foreach (var skill in _fmFile.HitJunctionsSkills)
            {
                writeSkillReference(skill.HitJunction);
                writeSkillReference(skill.Spark);
            }
        }

        private void writeCommonImages()
        {
            writeUInt32((uint)_fmFile.CommonImages.Count);

            foreach (var image in _fmFile.CommonImages)
            {
                writeUInt16(image.Number);
                writeInt16(image.X);
                writeInt16(image.Y);
            }
        }

        private void writeCpus()
        {
            for (int i = 0; i < 100; i++)
            {
                if (_fmFile.Cpu.Count <= i)
                {
                    // write empty cpu

                    // name
                    writeZeros(0x20);

                    writeZeros(9);

                    // steps
                    for (int j = 0; j < 10; j++)
                    {
                        // flags
                        writeZeros(3);
                        // skill reference
                        writeBytes([(byte)_fmFile.Commands.Count, 0x00]);
                        // amount
                        writeZeros(2);
                    }

                    continue;
                }

                var cpu = _fmFile.Cpu.ElementAt(i);
                writeString(cpu.Name, 0x20);

                byte flags = 0;
                if (cpu.CharacterInAir) flags |= 1 << 0;
                if (cpu.EnemyInAir) flags |= 1 << 1;

                writeBytes([flags]);

                writeBytes([cpu.Probability]);
                writeUInt16(cpu.Close);
                writeUInt16(cpu.Far);

                writeZeros(3);

                foreach (var command in cpu.Steps)
                {
                    writeZeros(1);

                    writeBytes([(byte)command.Direction]);

                    byte activeFlags = 0;
                    if (command.Continue) activeFlags |= 1 << 4;
                    if (command.Active) activeFlags |= 1 << 5;
                    writeBytes([activeFlags]);

                    writeSkillReference(command.Command);

                    writeUInt16(command.Amount);
                }
            }
        }

        private void writeDefaultSkillsIndex()
        {
            writeUInt16(_fmFile.BuiltInSkills.Standing);
            writeUInt16(_fmFile.BuiltInSkills.Forward);
            writeUInt16(_fmFile.BuiltInSkills.Backward);
            writeUInt16(_fmFile.BuiltInSkills.JumpUp);
            writeUInt16(_fmFile.BuiltInSkills.FrontJump);
            writeUInt16(_fmFile.BuiltInSkills.BackJump);
            writeUInt16(_fmFile.BuiltInSkills.Falling);
            writeUInt16(_fmFile.BuiltInSkills.MidCrouch);
            writeUInt16(_fmFile.BuiltInSkills.Crouching);
            writeUInt16(_fmFile.BuiltInSkills.StandFromCrouch);
            writeUInt16(_fmFile.BuiltInSkills.CrouchAdvance);
            writeUInt16(_fmFile.BuiltInSkills.CrouchRetreat);
            writeUInt16(_fmFile.BuiltInSkills.TurnStanding);
            writeUInt16(_fmFile.BuiltInSkills.TurnCrouching);
            writeUInt16(_fmFile.BuiltInSkills.ButtonGuardStand);
            writeUInt16(_fmFile.BuiltInSkills.ButtonGuardCrouch);
            writeUInt16(_fmFile.BuiltInSkills.ButtonGuardAir);
            writeUInt16(_fmFile.BuiltInSkills.Start);
            writeUInt16(_fmFile.BuiltInSkills.Victory);
            writeUInt16(_fmFile.BuiltInSkills.Loss);
            writeUInt16(_fmFile.BuiltInSkills.Draw);
            writeUInt16(_fmFile.BuiltInSkills.CharSelectPic);
            writeUInt16(_fmFile.BuiltInSkills.StageFacePic);
            writeUInt16(_fmFile.BuiltInSkills.RI);
        }

        private void writePlayerSettings()
        {
            writeUInt32(_fmFile.Settings.Age);
            writeBytes([(byte)_fmFile.Settings.Gender]);
            writeZeros(1740);
            writeUInt16(_fmFile.Settings.SideHPYPos);
            writeUInt16(_fmFile.Settings.Interval);
            writeBytes([_fmFile.Settings.HRatio]);
            writeBytes([_fmFile.Settings.StartPos]);
            writeBytes([_fmFile.Settings.Correct]);
            writeBytes([_fmFile.Settings.Combo]);

            writeBytes([(byte)_fmFile.Settings.GuardButton]);
            writeUInt32(_fmFile.Settings.LifeGaugeMax);
            writeUInt32(_fmFile.Settings.SpecialGaugeMax);
            writeUInt32(_fmFile.Settings.SpecialMaxStock);

            var flags = 0;
            if (_fmFile.Settings.NeutralGuard) flags |= 1 << 0;
            if (_fmFile.Settings.SkyGuard) flags |= 1 << 1;
            if (_fmFile.Settings.GuardWithButton) flags |= 1 << 3;
            writeBytes([(byte)flags]);

            writeZeros(7);

            writeInt16(_fmFile.Settings.PlayerAttacks);
            writeInt16(_fmFile.Settings.EnemyAttacks);
            writeUInt32(_fmFile.Settings.StartStock);

            writeZeros(3);
        }

        private void writeStoryMode()
        {
            for (int i = 0; i < 122; i++)
            {
                if (i >= _fmFile.StoryMode.Entries.Count())
                {
                    writeZeros(0xCE);
                    continue;
                }
                var entry = _fmFile.StoryMode.Entries.ElementAtOrDefault(i);
                writeBytes([(byte)entry.Type]);
                switch (entry.Type)
                {
                    case StoryEntryType.None:
                        break;
                    case StoryEntryType.Fight:
                        writeStoryEntryFight((FightStoryEntry)entry);
                        break;
                    case StoryEntryType.Demo:
                        writeStoryEntryDemo((DemoStoryEntry)entry);
                        break;
                    case StoryEntryType.IfDiversion:
                        writeStoryEntryIfDiversion((JumpStoryEntry)entry);
                        break;
                    case StoryEntryType.End:
                        writeStoryEntryEnd((EndStoryEntry)entry);
                        break;
                }
            }
        }

        private void writeStoryEntryFight(FightStoryEntry entry)
        {
            writeBytes([(byte)entry.Stage.Number]);
            writeBytes([(byte)entry.NumbOfRounds]);
            writeBytes([(byte)entry.FirstLife]);
            writeBytes([(byte)entry.LifeRecover]);

            byte flags = 0;
            if (entry.IfDefeated == StoryIfDefeated.GameOver) flags |= 1 << 0;
            if (entry.StartingRound == StoryStartingRound.PrevFight) flags |= 1 << 1;
            writeBytes([(byte)flags]);

            writeUInt16(entry.Time);
            writeUInt32(entry.PlayerStartPos);

            flags = 0;
            if (entry.ShowRoundSkill) flags |= 1 << 0;
            if (entry.ShowFightSkill) flags |= 1 << 1;
            if (entry.WL) flags |= 1 << 2;
            writeBytes([(byte)flags]);

            writeZeros(3);

            writeBytes([(byte)entry.IfTimeIsOverCpu]);
            writeBytes([(byte)entry.IfTimeIsOverValue]);
            writeBytes([(byte)entry.CpuWinPoints]);
            writeBytes([(byte)entry.CpuWinPointsValue]);

            writeZeros(4);

            foreach (var cpu in entry.Cpus)
            {
                writeStoryEntryFightCpu(cpu);
            }
        }

        private void writeStoryEntryFightCpu(StoryEntryCpu cpu)
        {
            byte flags1 = 0;
            byte flags2 = 0;

            if (cpu.ShowLife) flags1 |= 1 << 0;
            if (cpu.CpuIgnoresPlayer) flags2 |= 1 << 1;
            flags1 |= (byte)((byte)cpu.Method << 1);

            if (cpu.Effect == StoryEntryCpuEffect.Player) flags1 |= 1 << 7;
            if (cpu.Effect == StoryEntryCpuEffect.LastGivenAttack) flags2 |= 1 << 0;

            flags1 |= (byte)((byte)cpu.WinPause << 5);
            writeBytes([flags1, flags2]);

            writeZeros(2);

            writeBytes([(byte)(cpu.Character?.Number ?? 0)]);
            writeBytes([(byte)cpu.CpuLevel]);
            byte cpuEnemyFlags = 0;
            if (cpu.PlayerIsEnemy) cpuEnemyFlags |= 1 << 0;
            if (cpu.Cpu1IsEnemy) cpuEnemyFlags |= 1 << 1;
            if (cpu.Cpu2IsEnemy) cpuEnemyFlags |= 1 << 2;
            if (cpu.Cpu3IsEnemy) cpuEnemyFlags |= 1 << 3;
            if (cpu.Cpu4IsEnemy) cpuEnemyFlags |= 1 << 4;
            if (cpu.Cpu5IsEnemy) cpuEnemyFlags |= 1 << 5;
            if (cpu.Cpu6IsEnemy) cpuEnemyFlags |= 1 << 6;
            if (cpu.Cpu7IsEnemy) cpuEnemyFlags |= 1 << 7;
            writeBytes([cpuEnemyFlags]);

            writeUInt16(cpu.StartPosition);
            writeZeros(2);
            writeBytes([cpu.MethodTimeSec]);
            writeBytes([cpu.MethodTimeNumber]);
            writeBytes([(byte)cpu.MethodLifeToCheck]);
            writeBytes([cpu.MethodLifeToCheckValue]);

            writeBytes([cpu.VictoryPoints]);
            writeInt8(cpu.LifeEffectValue);
            writeInt8(cpu.SpecialEffectValue);
            writeBytes([(byte)cpu.VictoryPointsAssignee]);
            writeBytes([(byte)cpu.WhenTime]);
            writeBytes([cpu.WhenTimeValue]);
            writeZeros(5);
        }

        private void writeStoryEntryDemo(DemoStoryEntry entry)
        {
            writeUInt16(entry.Demo.Number);
            writeZeros(0xCB);
        }

        private void writeStoryEntryIfDiversion(JumpStoryEntry entry)
        {
            writeBytes([(byte)entry.If]);
            writeBytes([(byte)entry.Value]);
            writeZeros(2);
            writeInt8(entry.GoToEvent);

            writeZeros(0xC8);
        }

        private void writeStoryEntryEnd(EndStoryEntry entry)
        {
            writeZeros(205);
        }
    }
}
