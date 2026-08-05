using Fm2ndParser.Character;
using Fm2ndParser.Character.Story;
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
        public PlayerCompiler(string filename, PlayerFile playerFile)
            : base(filename, playerFile)
        {

        }

        override protected void CompileInternal(BinaryWriter writer)
        {
            base.CompileInternal(writer);

            writeZeros(writer, 4);

            writeCommands(writer);
            writeHitJunctionsSkills(writer);
            writeCommonImages(writer);

            writeZeros(writer, 10);

            writeCpus(writer);
            writeDefaultSkillsIndex(writer);
            writeZeros(writer, 0x26);
            writePlayerSettings(writer);
            writeStoryMode(writer);
            writeZeros(writer, 0); // todo

        }

        private void writeCommands(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.Commands.Count);

            foreach (var command in _fmFile.Commands)
            {
                writeString(writer, command.Name, 32);
                writeUInt16(writer, command.Time);
                writeSkillReference(writer, command.AirSkill);
                writeSkillReference(writer, command.StandSkill);
                writeSkillReference(writer, command.StandFarSkill);
                writeSkillReference(writer, command.CrouchedSkill);

                foreach (var step in command.Steps)
                {
                    writeCommandStep(writer, step);
                }

                foreach (var step in command.Steps)
                {
                    writeUInt16(writer, step.Amount);
                }
            }
        }

        private void writeHitJunctionsSkills(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.HitJunctionsSkills.Count);

            foreach (var skill in _fmFile.HitJunctionsSkills)
            {
                writeSkillReference(writer, skill.HitJunction);
                writeSkillReference(writer, skill.Spark);
            }
        }

        private void writeCommonImages(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.CommonImages.Count);

            foreach (var image in _fmFile.CommonImages)
            {
                writeUInt16(writer, image.Number);
                writeInt16(writer, image.X);
                writeInt16(writer, image.Y);
            }
        }

        private void writeCpus(BinaryWriter writer)
        {
            for (int i = 0; i < 100; i++)
            {
                if (_fmFile.Cpu.Count <= i)
                {
                    writeZeros(writer, 0x20);
                    writeZeros(writer, 9);
                    for (int j = 0; j < 10; j++)
                    {
                        writeZeros(writer, 3);
                        writeBytes(writer, [0x1C]);
                        writeZeros(writer, 3);
                    }

                    continue;
                }

                var cpu = _fmFile.Cpu.ElementAt(i);
                writeString(writer, cpu.Name, 0x20);

                byte flags = 0;
                if (cpu.CharacterInAir) flags |= 1 << 0;
                if (cpu.EnemyInAir) flags |= 1 << 1;

                writeBytes(writer, [flags]);

                writeBytes(writer, [cpu.Probability]);
                writeUInt16(writer, cpu.Close);
                writeUInt16(writer, cpu.Far);

                writeZeros(writer, 3);

                foreach (var command in cpu.Steps)
                {
                    writeZeros(writer, 1);

                    writeBytes(writer, [(byte)command.Direction]);

                    byte activeFlags = 0;
                    if (command.Continue) activeFlags |= 1 << 4;
                    if (command.Active) activeFlags |= 1 << 5;
                    writeBytes(writer, [activeFlags]);

                    writeSkillReference(writer, command.Command);

                    writeUInt16(writer, command.Amount);
                }
            }
        }

        private void writeDefaultSkillsIndex(BinaryWriter writer)
        {
            writeUInt16(writer, _fmFile.BuiltInSkills.Standing);
            writeUInt16(writer, _fmFile.BuiltInSkills.Forward);
            writeUInt16(writer, _fmFile.BuiltInSkills.Backward);
            writeUInt16(writer, _fmFile.BuiltInSkills.JumpUp);
            writeUInt16(writer, _fmFile.BuiltInSkills.FrontJump);
            writeUInt16(writer, _fmFile.BuiltInSkills.BackJump);
            writeUInt16(writer, _fmFile.BuiltInSkills.Falling);
            writeUInt16(writer, _fmFile.BuiltInSkills.MidCrouch);
            writeUInt16(writer, _fmFile.BuiltInSkills.Crouching);
            writeUInt16(writer, _fmFile.BuiltInSkills.StandFromCrouch);
            writeUInt16(writer, _fmFile.BuiltInSkills.CrouchAdvance);
            writeUInt16(writer, _fmFile.BuiltInSkills.CrouchRetreat);
            writeUInt16(writer, _fmFile.BuiltInSkills.TurnStanding);
            writeUInt16(writer, _fmFile.BuiltInSkills.TurnCrouching);
            writeUInt16(writer, _fmFile.BuiltInSkills.ButtonGuardStand);
            writeUInt16(writer, _fmFile.BuiltInSkills.ButtonGuardCrouch);
            writeUInt16(writer, _fmFile.BuiltInSkills.ButtonGuardAir);
            writeUInt16(writer, _fmFile.BuiltInSkills.Start);
            writeUInt16(writer, _fmFile.BuiltInSkills.Victory);
            writeUInt16(writer, _fmFile.BuiltInSkills.Loss);
            writeUInt16(writer, _fmFile.BuiltInSkills.Draw);
            writeUInt16(writer, _fmFile.BuiltInSkills.CharSelectPic);
            writeUInt16(writer, _fmFile.BuiltInSkills.StageFacePic);
            writeUInt16(writer, _fmFile.BuiltInSkills.RI);
        }

        private void writePlayerSettings(BinaryWriter writer)
        {
            writeUInt32(writer, _fmFile.Settings.Age);
            writeBytes(writer, [(byte)_fmFile.Settings.Gender]);
            writeZeros(writer, 1740);
            writeUInt16(writer, _fmFile.Settings.SideHPYPos);
            writeUInt16(writer, _fmFile.Settings.Interval);
            writeBytes(writer, [_fmFile.Settings.HRatio]);
            writeBytes(writer, [_fmFile.Settings.StartPos]);
            writeBytes(writer, [_fmFile.Settings.Correct]);
            writeBytes(writer, [_fmFile.Settings.Combo]);

            writeBytes(writer, [(byte)_fmFile.Settings.GuardButton]);
            writeUInt32(writer, _fmFile.Settings.LifeGaugeMax);
            writeUInt32(writer, _fmFile.Settings.SpecialGaugeMax);
            writeUInt32(writer, _fmFile.Settings.SpecialMaxStock);

            var flags = 0;
            if (_fmFile.Settings.NeutralGuard) flags |= 1 << 0;
            if (_fmFile.Settings.SkyGuard) flags |= 1 << 1;
            if (_fmFile.Settings.GuardWithButton) flags |= 1 << 3;
            writeBytes(writer, [(byte)flags]);

            writeZeros(writer, 7);

            writeInt16(writer, _fmFile.Settings.PlayerAttacks);
            writeInt16(writer, _fmFile.Settings.EnemyAttacks);
            writeUInt32(writer, _fmFile.Settings.StartStock);

            writeZeros(writer, 3);
        }

        private void writeStoryMode(BinaryWriter writer)
        {
            for (int i = 0; i < 122; i++)
            {
                if (i >= _fmFile.StoryMode.Entries.Count())
                {
                    writeZeros(writer, 0xCE);
                    continue;
                }
                var entry = _fmFile.StoryMode.Entries.ElementAtOrDefault(i);
                writeBytes(writer, [(byte)entry.Type]);
                switch (entry.Type)
                {
                    case StoryEntryType.None:
                        break;
                    case StoryEntryType.Fight:
                        writeStoryEntryFight(writer, (FightStoryEntry)entry);
                        break;
                    case StoryEntryType.Demo:
                        writeStoryEntryDemo(writer, (DemoStoryEntry)entry);
                        break;
                    case StoryEntryType.IfDiversion:
                        writeStoryEntryIfDiversion(writer, (JumpStoryEntry)entry);
                        break;
                    case StoryEntryType.End:
                        writeStoryEntryEnd(writer, (EndStoryEntry)entry);
                        break;
                }
            }
        }

        private void writeStoryEntryFight(BinaryWriter writer, FightStoryEntry entry)
        {
            writeBytes(writer, [(byte)entry.Stage.Number]);
            writeBytes(writer, [(byte)entry.NumbOfRounds]);
            writeBytes(writer, [(byte)entry.FirstLife]);
            writeBytes(writer, [(byte)entry.LifeRecover]);

            byte flags = 0;
            if (entry.IfDefeated == StoryIfDefeated.GameOver) flags |= 1 << 0;
            if (entry.StartingRound == StoryStartingRound.PrevFight) flags |= 1 << 1;
            writeBytes(writer, [(byte)flags]);

            writeUInt16(writer, entry.Time);
            writeUInt32(writer, entry.PlayerStartPos);

            flags = 0;
            if (entry.ShowRoundSkill) flags |= 1 << 0;
            if (entry.ShowFightSkill) flags |= 1 << 1;
            if (entry.WL) flags |= 1 << 2;
            writeBytes(writer, [(byte)flags]);

            writeZeros(writer, 3);

            writeBytes(writer, [(byte)entry.IfTimeIsOverCpu]);
            writeBytes(writer, [(byte)entry.IfTimeIsOverValue]);
            writeBytes(writer, [(byte)entry.CpuWinPoints]);
            writeBytes(writer, [(byte)entry.CpuWinPointsValue]);

            writeZeros(writer, 4);

            foreach (var cpu in entry.Cpus)
            {
                writeStoryEntryFightCpu(writer, cpu);
            }
        }

        private void writeStoryEntryFightCpu(BinaryWriter writer, StoryEntryCpu cpu)
        {
            byte flags1 = 0;
            byte flags2 = 0;

            if (cpu.ShowLife) flags1 |= 1 << 0;
            if (cpu.CpuIgnoresPlayer) flags2 |= 1 << 1;
            flags2 |= (byte)((byte)cpu.Method << 1);

            if (cpu.Effect == StoryEntryCpuEffect.Player) flags1 |= 1 << 7;
            if (cpu.Effect == StoryEntryCpuEffect.LastGivenAttack) flags2 |= 1 << 0;

            flags1 |= (byte)((byte)cpu.WinPause << 5);
            writeBytes(writer, [flags1, flags2]);

            writeZeros(writer, 2);

            writeBytes(writer, [(byte)(cpu.Character?.Number ?? 0)]);
            writeBytes(writer, [(byte)cpu.CpuLevel]);
            byte cpuEnemyFlags = 0;
            if (cpu.PlayerIsEnemy) cpuEnemyFlags |= 1 << 0;
            if (cpu.Cpu1IsEnemy) cpuEnemyFlags |= 1 << 1;
            if (cpu.Cpu2IsEnemy) cpuEnemyFlags |= 1 << 2;
            if (cpu.Cpu3IsEnemy) cpuEnemyFlags |= 1 << 3;
            if (cpu.Cpu4IsEnemy) cpuEnemyFlags |= 1 << 4;
            if (cpu.Cpu5IsEnemy) cpuEnemyFlags |= 1 << 5;
            if (cpu.Cpu6IsEnemy) cpuEnemyFlags |= 1 << 6;
            if (cpu.Cpu7IsEnemy) cpuEnemyFlags |= 1 << 7;
            writeBytes(writer, [cpuEnemyFlags]);

            writeUInt16(writer, cpu.StartPosition);
            writeZeros(writer, 2);
            writeBytes(writer, [cpu.MethodTimeSec]);
            writeBytes(writer, [cpu.MethodTimeNumber]);
            writeBytes(writer, [(byte)cpu.MethodLifeToCheck]);
            writeBytes(writer, [cpu.MethodLifeToCheckValue]);

            writeBytes(writer, [cpu.VictoryPoints]);
            writeInt8(writer, cpu.LifeEffectValue);
            writeInt8(writer, cpu.SpecialEffectValue);
            writeBytes(writer, [(byte)cpu.VictoryPointsAssignee]);
            writeBytes(writer, [(byte)cpu.WhenTime]);
            writeBytes(writer, [cpu.WhenTimeValue]);
            writeZeros(writer, 5);
        }

        private void writeStoryEntryDemo(BinaryWriter writer, DemoStoryEntry entry)
        {
            writeUInt16(writer, entry.Demo.Number);
            writeZeros(writer, 0xCB);
        }

        private void writeStoryEntryIfDiversion(BinaryWriter writer, JumpStoryEntry entry)
        {
            writeBytes(writer, [(byte)entry.If]);
            writeBytes(writer, [(byte)entry.Value]);
            writeZeros(writer, 2);
            writeInt8(writer, entry.GoToEvent);

            writeZeros(writer, 0xC8);
        }

        private void writeStoryEntryEnd(BinaryWriter writer, EndStoryEntry entry)
        {
            writeZeros(writer, 205);
        }
    }
}
