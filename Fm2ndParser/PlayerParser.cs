using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fm2ndParser
{
    public class PlayerParser : BaseParser
    {

        public PlayerParser(string filename, KGT kgt) : base(filename, kgt)
        {
        }

        public Player Parse()
        {
            return base.parse<Player>();
        }

        protected override T ParseInternal<T>(Span<byte> bytes, ref int offset)
        {
            var player = base.ParseInternal<Player>(bytes, ref offset);

            //empty
            skipEmptyBytes(bytes, 4, ref offset);

            player.Commands = parseCommands(bytes, ref offset);

            player.HitJunctionsSkills = parseHitJunctionsSkills(bytes, ref offset);

            player.CommonImages = parseCommonImages(bytes, ref offset);

            skipEmptyBytes(bytes, 10, ref offset);

            player.Cpu = parseCpus(bytes, player.Commands, ref offset);

            // todo unknown
            skipNumberSequenceUInt16(bytes, 1, 24, ref offset);
            skipEmptyBytes(bytes, 0x26, ref offset);

            player.Settings = parsePlayerSettings(bytes, ref offset);

            player.StoryMode = parseStoryMode(bytes, ref offset);

            skiRemaningEmptyBytes(bytes, ref offset);

            return (T)(object)player;
        }

        private ICollection<HitJunctionSkills> parseHitJunctionsSkills(Span<byte> bytes, ref int offset)
        {
            var hitJunctionsCount = getUInt32(bytes, ref offset);
            var result = new List<HitJunctionSkills>();
            for (int i = 0; i < hitJunctionsCount; i++)
            {
                var hitJunction = new HitJunctionSkills
                {
                    HitJunction = getSkill(bytes, ref offset),
                    Spark = getSkill(bytes, ref offset),
                };
                result.Add(hitJunction);
            }

            return result;
        }

        private ICollection<CommonImage> parseCommonImages(Span<byte> bytes, ref int offset)
        {
            var count = getUInt32(bytes, ref offset);

            var result = new List<CommonImage>();

            for (int i = 0; i < count; i++)
            {
                var commonImage = new CommonImage
                {
                    Number = getUInt16(bytes, ref offset),
                    X = getInt16(bytes, ref offset),
                    Y = getInt16(bytes, ref offset),
                };
                result.Add(commonImage);
            }
            return result;
        }

        private StoryMode parseStoryMode(Span<byte> bytes, ref int offset)
        {
            // 206 bytes per entry
            var entries = new List<StoryEntry>();
            for (int i = 0; i < 100; i++)
            {
                StoryEntry entry;
                var type = getByte(bytes, ref offset);
                switch (type)
                {
                    case 0:
                        continue;
                    // fight
                    case 1:
                        entry = parseFightStoryEntry(bytes, ref offset);
                        break;
                    // demo
                    case 2:
                        entry = parseDemoStoryEntry(bytes, ref offset);
                        break;
                    // if diversion
                    case 3:
                        entry = parseJumpStoryEntry(bytes, ref offset);
                        break;
                    // end
                    case 4:
                        entry = parseEndStoryEntry(bytes, ref offset);
                        break;
                    default:
                        throw new Exception($"Unknown story mode type: {type}");

                }
                entry.TypeId = type;
                entries.Add(entry);
            }
            return new StoryMode
            {
                Entries = entries
            };
        }

        private StoryEntry parseFightStoryEntry(Span<byte> bytes, ref int offset)
        {
            var stage = getByte(bytes, ref offset);
            var numbOfRounds = getByte(bytes, ref offset);
            var firstLife = (StoryFirstLife)getByte(bytes, ref offset);
            var lifeRecover = getByte(bytes, ref offset);

            var flag3 = getByte(bytes, ref offset);
            var ifDefeated = (StoryIfDefeated)Convert.ToInt32(isFlagOn(flag3, 0));
            var startingRound = (StoryStartingRound)Convert.ToInt32(isFlagOn(flag3, 1));

            var time = getUInt16(bytes, ref offset);
            var playerStartPos = getUInt32(bytes, ref offset);
            var flags = getByte(bytes, ref offset);

            var showRoundSkill = isFlagOn(flags, 0);
            var showFightSkill = isFlagOn(flags, 1);
            var WL = isFlagOn(flags, 2);

            assertUnusedFlags(flags, 0b11111000);

            skipEmptyBytes(bytes, 3, ref offset);

            var ifTimeIsOverCpu = (CPU)getByte(bytes, ref offset);
            var ifTimeIsOverValue = getByte(bytes, ref offset);
            var cpuWinPoints = (StoryCpuWinsPoints)getByte(bytes, ref offset);
            var cpuWinPointsValue = getByte(bytes, ref offset);

            skipEmptyBytes(bytes, 4, ref offset);
            for (int i = 0; i < 7; i++)
            {
                var cpu = parseStoryEntryFightCpu(bytes, ref offset);
            }

            var result = new StoryEntryFight
            {
                Type = "F",
                Stage = stage > 0 ? new SkillReference
                {
                    Number = stage,
                    Name = _kgt.Stages[stage - 1],
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
                CpuWinPointsValue = cpuWinPointsValue
            };

            return result;
        }

        private StoryEntryCpu parseStoryEntryFightCpu(Span<byte> bytes, ref int offset)
        {
            var flags1 = getByte(bytes, ref offset);
            var flags2 = getByte(bytes, ref offset);

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


            skipEmptyBytes(bytes, 2, ref offset);
            var character = getByte(bytes, ref offset);
            var cpuLevel = getByte(bytes, ref offset);
            var cpuEnemyFlags = getByte(bytes, ref offset);
            var playerIsEnemy = isFlagOn(cpuEnemyFlags, 0);
            var cpu1IsEnemy = isFlagOn(cpuEnemyFlags, 1);
            var cpu2IsEnemy = isFlagOn(cpuEnemyFlags, 2);
            var cpu3IsEnemy = isFlagOn(cpuEnemyFlags, 3);
            var cpu4IsEnemy = isFlagOn(cpuEnemyFlags, 4);
            var cpu5IsEnemy = isFlagOn(cpuEnemyFlags, 5);
            var cpu6IsEnemy = isFlagOn(cpuEnemyFlags, 6);
            var cpu7IsEnemy = isFlagOn(cpuEnemyFlags, 7);

            var startPosition = getUInt16(bytes, ref offset);
            skipEmptyBytes(bytes, 2, ref offset);
            var methodTimeSec = getByte(bytes, ref offset);
            var methodTimeNumber = getByte(bytes, ref offset);
            var methodLifeToCheck = (StoryPlayerToCheck)getByte(bytes, ref offset);
            var methodLifeToCheckValue = getByte(bytes, ref offset);

            var victoryPoints = getByte(bytes, ref offset);
            var lifeEffectValue = getInt8(bytes, ref offset);
            var specialEffectValue = getInt8(bytes, ref offset);
            var victoryPointsAssignee = (StoryCpuWinsPoints)getByte(bytes, ref offset); // 0 for last given attack, 1 for player, 2-8 for cpu1-7
            var whenTime = (StoryPlayerToCheck)getByte(bytes, ref offset); // 0 for player, 1-7 for cpu1-7
            var whenTimeValue = getByte(bytes, ref offset);

            skipEmptyBytes(bytes, 5, ref offset);

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
                    Name = _kgt.Characters.Skip(character - 1).First(),
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

        private StoryEntry parseDemoStoryEntry(Span<byte> bytes, ref int offset)
        {
            var demoIndex = getUInt16(bytes, ref offset);
            var result = new DemoStoryEntry
            {
                Type = "D",
                Demo = demoIndex > 0 ? new SkillReference
                {
                    Number = demoIndex,
                    Name = _kgt.Demos.Skip(demoIndex - 1).First()
                } : null,
            };
            skipEmptyBytes(bytes, 0xCB, ref offset);
            return result;
        }


        private StoryEntry parseJumpStoryEntry(Span<byte> bytes, ref int offset)
        {
            var result = new JumpStoryEntry
            {
                Type = "J",
                If = (StoryEntryJump)getByte(bytes, ref offset),
                Value = getByte(bytes, ref offset)
            };
            skipEmptyBytes(bytes, 2, ref offset);
            var goToEvent = getInt8(bytes, ref offset);
            result.GoToEvent = goToEvent;
            skipEmptyBytes(bytes, 0xC8, ref offset);
            return result;
        }

        private EndStoryEntry parseEndStoryEntry(Span<byte> bytes, ref int offset)
        {
            skipEmptyBytes(bytes, 205, ref offset);
            var result = new EndStoryEntry
            {
                Type = "E"
            };
            return result;
        }

        private PlayerSettings parsePlayerSettings(Span<byte> bytes, ref int offset)
        {
            var age = getInt32(bytes, ref offset);
            var gender = (Gender)getByte(bytes, ref offset);
            skipEmptyBytes(bytes, 1740, ref offset);

            var sideHPYPos = getUInt16(bytes, ref offset);
            var interval = getUInt16(bytes, ref offset);
            var hRatio = getByte(bytes, ref offset);
            var startPos = getByte(bytes, ref offset);
            var correct = getByte(bytes, ref offset);
            var combo = getByte(bytes, ref offset);

            var guardButton = (Button)getByte(bytes, ref offset);
            var lifeGaugeMax = getUInt32(bytes, ref offset);
            var specialGaugeMax = getUInt32(bytes, ref offset);
            var specialMaxStock = getUInt32(bytes, ref offset);
            var flags = getByte(bytes, ref offset);

            var neutralGuard = isFlagOn(flags, 0);
            var skyGuard = isFlagOn(flags, 1);
            var guardWithButton = isFlagOn(flags, 3);

            assertUnusedFlags(flags, 0b11110100);

            skipEmptyBytes(bytes, 7, ref offset);

            var playerAttacks = getInt16(bytes, ref offset);
            var enemyAttacks = getInt16(bytes, ref offset);
            var startStock = getUInt32(bytes, ref offset);

            skipEmptyBytes(bytes, 3, ref offset);

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


        private ICollection<CpuCommand> parseCpus(Span<byte> bytes, ICollection<Command> commands, ref int offset)
        {
            var list = new List<CpuCommand>();
            for (int i = 0; i < 100; i++)
            {
                var name = getString(bytes, 0x20, ref offset);
                if (name == string.Empty)
                {
                    getWord(bytes, 0x4F, ref offset);
                    continue;
                }

                var cpu = new CpuCommand
                {
                    Name = name,
                };
                var airFlag = getByte(bytes, ref offset);
                var characterInAir = isFlagOn(airFlag, 0);
                var enemyInAir = isFlagOn(airFlag, 1);

                var probability = getByte(bytes, ref offset);
                var close = getUInt16(bytes, ref offset);
                var far = getUInt16(bytes, ref offset);


                cpu.Probability = probability;
                cpu.Close = close;
                cpu.Far = far;
                cpu.CharacterInAir = characterInAir;
                cpu.EnemyInAir = enemyInAir;

                skipEmptyBytes(bytes, 3, ref offset);

                var steps = new List<CpuCommandStep>();
                for (int s = 0; s < 10; s++)
                {
                    skipEmptyBytes(bytes, 1, ref offset);

                    var directionFlag = getByte(bytes, ref offset);
                    var activeFlag = getByte(bytes, ref offset);
                    var commandIndex = getUInt16(bytes, ref offset);
                    var amount = getUInt16(bytes, ref offset);

                    var step = new CpuCommandStep
                    {
                        Continue = isFlagOn(activeFlag, 4),
                        Active = isFlagOn(activeFlag, 5),
                        Direction = (ComDirection)(directionFlag & 0b00001111),
                        Amount = amount,
                        Command = commandIndex > 0 ? new SkillReference
                        {
                            Number = commandIndex,
                            Name = commands.Skip(commandIndex - 1).First().Name
                        } : null,
                    };
                    steps.Add(step);
                }
                cpu.Steps = steps;
                list.Add(cpu);
            }
            return list;
        }

        private void skipNumberSequenceUInt16(Span<byte> bytes, int from, int to, ref int offset)
        {
            for (int i = from; i < to + 1; i++)
            {
                var unknown = getUInt16(bytes, ref offset);
                Debug.Assert(unknown == i);
            }
        }
    }
}