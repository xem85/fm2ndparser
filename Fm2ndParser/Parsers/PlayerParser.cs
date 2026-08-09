using Fm2ndParser.Kgt;
using Fm2ndParser.Character;
using Fm2ndParser.Character.Story;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Fm2ndParser.Common;

namespace Fm2ndParser.Parsers
{
    public class PlayerParser : BaseParser<PlayerFile>
    {
        public override string FileExtension => "player";

        public PlayerParser(string filename, KGTFile kgt) : base(filename, kgt)
        {
        }
    
        protected override PlayerFile ParseInternal()
        {
            var player = base.ParseInternal();

            setSettingsBlocksData();

            //empty
            skipEmptyBytes(4);

            player.Commands = parseCommands();

            player.HitJunctionsSkills = parseHitJunctionsSkills();

            player.CommonImages = parseCommonImages();

            skipEmptyBytes(10);

            player.Cpu = parseCpus(player.Commands);

            player.BuiltInSkills = parseDefaultSkillsIndex();

            skipEmptyBytes(0x26);

            player.Settings = parsePlayerSettings();

            player.StoryMode = parseStoryMode();

            skipRemaningEmptyBytes();

            return player;
        }

        private ICollection<HitJunctionSkills> parseHitJunctionsSkills()
        {
            var hitJunctionsCount = getUInt32();
            var result = new List<HitJunctionSkills>();
            for (int i = 0; i < hitJunctionsCount; i++)
            {
                var hitJunction = new HitJunctionSkills
                {
                    HitJunction = getSkill(),
                    Spark = getSkill(),
                };
                result.Add(hitJunction);
            }

            return result;
        }

        private ICollection<CommonImage> parseCommonImages()
        {
            var count = getUInt32();

            var result = new List<CommonImage>();

            for (int i = 0; i < count; i++)
            {
                var commonImage = new CommonImage
                {
                    Number = getUInt16(),
                    X = getInt16(),
                    Y = getInt16(),
                };
                result.Add(commonImage);
            }
            return result;
        }


        protected IList<Command> parseCommands()
        {
            var count = getUInt32();

            var commands = new List<Command>();

            for (int i = 0; i < count; i++)
            {
                var command = parseCommand();
                commands.Add(command);
            }

            return commands;
        }

        protected Command parseCommand()
        {
            var result = new Command
            {
                Name = getString(32),
                Time = getUInt16(),
                AirSkill = getSkill(),
                StandSkill = getSkill(),
                StandFarSkill = getSkill(),
                CrouchedSkill = getSkill(),
            };

            var steps = new List<CommandStep>();
            for (int i = 0; i < 10; i++)
            {
                var step = getCommandStep();
                steps.Add(step);
            }
            for (int i = 0; i < 10; i++)
            {
                steps[i].Amount = getUInt16();
            }

            result.Steps = steps;

            return result;
        }

        private StoryMode parseStoryMode()
        {
            // 206 bytes per entry
            var entries = new List<StoryEntry>();
            for (int i = 0; i < 100; i++)
            {
                StoryEntry entry;
                var type = (StoryEntryType)getByte();
                switch (type)
                {
                    case StoryEntryType.None:
                        continue;
                    case StoryEntryType.Fight:
                        entry = parseFightStoryEntry();
                        break;
                    case StoryEntryType.Demo:
                        entry = parseDemoStoryEntry();
                        break;
                    case StoryEntryType.IfDiversion:
                        entry = parseJumpStoryEntry();
                        break;
                    case StoryEntryType.End:
                        entry = parseEndStoryEntry();
                        break;
                    default:
                        throw new Exception($"Unknown story mode type: {type}");

                }
                entry.Type = type;
                entries.Add(entry);
            }
            return new StoryMode
            {
                Entries = entries
            };
        }

        private StoryEntry parseFightStoryEntry()
        {
            var stage = getByte();
            var numbOfRounds = getByte();
            var firstLife = (StoryFirstLife)getByte();
            var lifeRecover = getByte();

            var flag3 = getByte();
            var ifDefeated = (StoryIfDefeated)Convert.ToInt32(isFlagOn(flag3, 0));
            var startingRound = (StoryStartingRound)Convert.ToInt32(isFlagOn(flag3, 1));

            var time = getUInt16();
            var playerStartPos = getUInt32();
            var flags = getByte();

            var showRoundSkill = isFlagOn(flags, 0);
            var showFightSkill = isFlagOn(flags, 1);
            var WL = isFlagOn(flags, 2);

            assertUnusedFlags(flags, 0b11111000);

            skipEmptyBytes(3);

            var ifTimeIsOverCpu = (CPU)getByte();
            var ifTimeIsOverValue = getByte();
            var cpuWinPoints = (StoryCpuWinsPoints)getByte();
            var cpuWinPointsValue = getByte();

            skipEmptyBytes(4);

            var cpus = new List<StoryEntryCpu>();   
            for (int i = 0; i < 7; i++)
            {
                var cpu = parseStoryEntryFightCpu();
                cpus.Add(cpu);
            }

            var result = new FightStoryEntry
            {
                //Type = "F",
                Stage = stage > 0 ? new SkillReference
                {
                    Number = stage,
                    Name = _kgt?.Stages[stage - 1],
                } : null,
                NumbOfRounds = numbOfRounds,
                FirstLife = firstLife,
                LifeRecover = lifeRecover,
                IfDefeated = ifDefeated,
                StartingRound = startingRound,
                Time = time,
                PlayerStartPos = playerStartPos,
                ShowRoundSkill = showRoundSkill,
                ShowFightSkill = showFightSkill,
                WL = WL,
                IfTimeIsOverCpu = ifTimeIsOverCpu,
                IfTimeIsOverValue = ifTimeIsOverValue,
                CpuWinPoints = cpuWinPoints,
                CpuWinPointsValue = cpuWinPointsValue,
                Cpus = cpus,
            };

            return result;
        }

        private StoryEntryCpu parseStoryEntryFightCpu()
        {
            var flags1 = getByte();
            var flags2 = getByte();

            var showLife = isFlagOn(flags1, 0);
            var cpuIgnoresPlayer = isFlagOn(flags2, 1);
            var method = (StoryEntryCpuMethod)((flags1 & 0b00000110) >> 1);
            // this is weird
            var effect = StoryEntryCpuEffect.None;
            if (isFlagOn(flags1, 7))
                effect = StoryEntryCpuEffect.Player;
            else if (isFlagOn(flags2, 0))
                effect = StoryEntryCpuEffect.LastGivenAttack;

            var winPause = (StoryEntryWinPause)((flags1 & 0b01100000) >> 5);


            skipEmptyBytes(2);
            var character = getByte();
            var cpuLevel = getByte();
            var cpuEnemyFlags = getByte();
            var playerIsEnemy = isFlagOn(cpuEnemyFlags, 0);
            var cpu1IsEnemy = isFlagOn(cpuEnemyFlags, 1);
            var cpu2IsEnemy = isFlagOn(cpuEnemyFlags, 2);
            var cpu3IsEnemy = isFlagOn(cpuEnemyFlags, 3);
            var cpu4IsEnemy = isFlagOn(cpuEnemyFlags, 4);
            var cpu5IsEnemy = isFlagOn(cpuEnemyFlags, 5);
            var cpu6IsEnemy = isFlagOn(cpuEnemyFlags, 6);
            var cpu7IsEnemy = isFlagOn(cpuEnemyFlags, 7);

            var startPosition = getUInt16();
            skipEmptyBytes(2);
            var methodTimeSec = getByte();
            var methodTimeNumber = getByte();
            var methodLifeToCheck = (StoryPlayerToCheck)getByte();
            var methodLifeToCheckValue = getByte();

            var victoryPoints = getByte();
            var lifeEffectValue = getInt8();
            var specialEffectValue = getInt8();
            var victoryPointsAssignee = (StoryCpuWinsPoints)getByte(); // 0 for last given attack, 1 for player, 2-8 for cpu1-7
            var whenTime = (StoryPlayerToCheck)getByte(); // 0 for player, 1-7 for cpu1-7
            var whenTimeValue = getByte();

            skipEmptyBytes(5);

            var result = new StoryEntryCpu
            {
                ShowLife = showLife,
                CpuIgnoresPlayer = cpuIgnoresPlayer,
                Method = method,
                Effect = effect,
                WinPause = winPause,
                Character = character > 0 ? new SkillReference
                {
                    Number = character,
                    Name = _kgt?.Characters.Skip(character - 1).First().Name,
                } : null,
                CpuLevel = cpuLevel,
                PlayerIsEnemy = playerIsEnemy,
                Cpu1IsEnemy = cpu1IsEnemy,
                Cpu2IsEnemy = cpu2IsEnemy,
                Cpu3IsEnemy = cpu3IsEnemy,
                Cpu4IsEnemy = cpu4IsEnemy,
                Cpu5IsEnemy = cpu5IsEnemy,
                Cpu6IsEnemy = cpu6IsEnemy,
                Cpu7IsEnemy = cpu7IsEnemy,
                StartPosition = startPosition,
                MethodTimeSec = methodTimeSec,
                MethodTimeNumber = methodTimeNumber,
                MethodLifeToCheck = methodLifeToCheck,
                MethodLifeToCheckValue = methodLifeToCheckValue,
                VictoryPoints = victoryPoints,
                LifeEffectValue = lifeEffectValue,
                SpecialEffectValue = specialEffectValue,
                VictoryPointsAssignee = victoryPointsAssignee,
                WhenTime = whenTime,
                WhenTimeValue = whenTimeValue
            };

            return result;
        }

        private StoryEntry parseDemoStoryEntry()
        {
            var demoIndex = getUInt16();
            var result = new DemoStoryEntry
            {
                //Type = "D",
                Demo = demoIndex > 0 ? new SkillReference
                {
                    Number = demoIndex,
                    Name = _kgt?.Demos.Skip(demoIndex - 1).First(),
                } : null,
            };
            skipEmptyBytes(0xCB);
            return result;
        }


        private StoryEntry parseJumpStoryEntry()
        {
            var result = new JumpStoryEntry
            {
                //Type = "J",
                If = (StoryEntryJump)getByte(),
                Value = getByte()
            };
            skipEmptyBytes(2);
            var goToEvent = getInt8();
            result.GoToEvent = goToEvent;
            skipEmptyBytes(0xC8);
            return result;
        }

        private EndStoryEntry parseEndStoryEntry()
        {
            skipEmptyBytes(205);
            var result = new EndStoryEntry
            {
                //Type = "E"
            };
            return result;
        }

        private PlayerSettings parsePlayerSettings()
        {
            var age = getUInt32();
            var gender = (Gender)getByte();
            skipEmptyBytes(1740);

            var sideHPYPos = getUInt16();
            var interval = getUInt16();
            var hRatio = getByte();
            var startPos = getByte();
            var correct = getByte();
            var combo = getByte();

            var guardButton = (Button)getByte();
            var lifeGaugeMax = getUInt32();
            var specialGaugeMax = getUInt32();
            var specialMaxStock = getUInt32();
            var flags = getByte();

            var neutralGuard = isFlagOn(flags, 0);
            var skyGuard = isFlagOn(flags, 1);
            var guardWithButton = isFlagOn(flags, 3);

            assertUnusedFlags(flags, 0b11110100);

            skipEmptyBytes(7);

            var playerAttacks = getInt16();
            var enemyAttacks = getInt16();
            var startStock = getUInt32();

            skipEmptyBytes(3);

            var result = new PlayerSettings
            {
                Age = age,
                Gender = gender,
                SideHPYPos = sideHPYPos,
                Interval = interval,
                HRatio = hRatio,
                StartPos = startPos,
                Correct = correct,
                Combo = combo,
                GuardButton = guardButton,
                LifeGaugeMax = lifeGaugeMax,
                SpecialGaugeMax = specialGaugeMax,
                SpecialMaxStock = specialMaxStock,
                NeutralGuard = neutralGuard,
                SkyGuard = skyGuard,
                GuardWithButton = guardWithButton,
                PlayerAttacks = playerAttacks,
                EnemyAttacks = enemyAttacks,
                StartStock = startStock
            };
            return result;
        }


        private ICollection<CpuCommand> parseCpus(ICollection<Command> commands)
        {
            var list = new List<CpuCommand>();
            for (int i = 0; i < 100; i++)
            {
                var name = getString(0x20);
                if (name == string.Empty)
                {
                    getBytes(0x4F);
                    continue;
                }

                var cpu = new CpuCommand
                {
                    Name = name,
                };
                var airFlag = getByte();
                cpu.CharacterInAir = isFlagOn(airFlag, 0);
                cpu.EnemyInAir = isFlagOn(airFlag, 1);

                cpu.Probability = getByte();
                cpu.Close = getUInt16();
                cpu.Far = getUInt16();

                skipEmptyBytes(3);

                var steps = new List<CpuCommandStep>();
                for (int s = 0; s < 10; s++)
                {
                    skipEmptyBytes(1);

                    var directionFlag = getByte();
                    var activeFlag = getByte();
                    var command = getSkill();
                    var amount = getUInt16();

                    var step = new CpuCommandStep
                    {
                        Continue = isFlagOn(activeFlag, 4),
                        Active = isFlagOn(activeFlag, 5),
                        Direction = (ComDirection)(directionFlag & 0b00001111),
                        Amount = amount,
                        Command = command
                    };
                    steps.Add(step);
                }
                cpu.Steps = steps;
                list.Add(cpu);
            }
            return list;
        }

        private PlayerBuiltInSkills parseDefaultSkillsIndex()
        {
            var result = new PlayerBuiltInSkills
            {
                Standing = getUInt16(),
                Forward = getUInt16(),
                Backward = getUInt16(),
                JumpUp = getUInt16(),
                FrontJump = getUInt16(),
                BackJump = getUInt16(),
                Falling = getUInt16(),
                MidCrouch = getUInt16(),
                Crouching = getUInt16(),
                StandFromCrouch = getUInt16(),
                CrouchAdvance = getUInt16(),
                CrouchRetreat = getUInt16(),
                TurnStanding = getUInt16(),
                TurnCrouching = getUInt16(),
                ButtonGuardStand = getUInt16(),
                ButtonGuardCrouch = getUInt16(),
                ButtonGuardAir = getUInt16(),
                Start = getUInt16(),
                Victory = getUInt16(),
                Loss = getUInt16(),
                Draw = getUInt16(),
                CharSelectPic = getUInt16(),
                StageFacePic = getUInt16(),
                RI = getUInt16(),
            };
            return result;
        }

        protected override SettingsType getSettingsType(uint skillIdx)
        {
            return SettingsType.Character;
        }
    }
}