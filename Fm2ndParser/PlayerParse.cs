using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Fm2ndParser
{
    public partial class PlayerParse
    {
        private string _filename;
        private IList<Skill> _skills;

        public PlayerParse(string filename)
        {
            _filename = filename;
        }

        public Player Parse()
        {
            var file = File.ReadAllBytes(_filename);
            Span<byte> bytes = file;

            var offset = 0;

            var type = getString(bytes, 16, ref offset);
            if (type.StartsWith("2DKGT2G"))
                throw new LockedFileException();

            var name = getString(bytes, 256, ref offset);

            var skillCount = getInt32(bytes, ref offset);   // not sure what this number is

            _skills = readSkills(bytes, ref offset);

            var blocksCount = getInt16(bytes, ref offset);

            getWord(bytes, 2, ref offset);

            readSkillsBlocks(blocksCount, bytes, ref offset);

            var result = new Player
            {
                Type = type,
                Name = name,
                Skills = _skills,
            };

            return result;
        }

        private string getString(Span<byte> bytes, int length, ref int offset)
        {
            var word = getWord(bytes, length, ref offset);
            var result = Encoding.Default.GetString(word).Replace("\0", "").Trim();
            return result;
        }

        private void readSkillsBlocks(int blocksCount, Span<byte> bytes, ref int offset)
        {
            var blocks = new List<Block>();
            var blocksData = new List<byte[]>();
            for (int i = 0; i < blocksCount; i++)
            {
                blocksData.Add(getWord(bytes, 16, ref offset).ToArray());
            }

            for (int i = 0; i < _skills.Count; i++)
            {
                var skill = _skills[i];
                skill.Blocks = new List<Block>();
                var endPosition = skill != _skills.Last() ? _skills[i + 1].Position : blocksCount;
                var skillBlocksCount = endPosition - skill.Position;

                foreach (var blockData in blocksData.Skip(skill.Position).Take(skillBlocksCount))
                {
                    if (!skill.Blocks.Any())
                    {
                        var settingsBlock = parseSkillSettings(skill.Type, blockData);
                        settingsBlock.Index = skill.Blocks.Count();
                        skill.Blocks.Add(settingsBlock);
                    }
                    else
                    {
                        var block = parseBlock(blockData);
                        block.Index = skill.Blocks.Count();
                        skill.Blocks.Add(block);
                    }
                }
            }
        }

        private Settings parseSkillSettings(int skillType, Span<byte> data)
        {
            var offset = 0;

            var type = getByte(data, ref offset);

            getByte(data, ref offset);

            var level = getByte(data, ref offset);

            var result = new Settings
            {
                Data = data.ToArray(),
                Type = "Settings",
                Level = level,
            };
            return result;
        }

        private Block parseBlock(Span<byte> data)
        {
            var offset = 0;
            var type = getByte(data, ref offset);

            switch (type)
            {
                case 0:
                    return null;
                case 1:
                    return parseMBlock(data, ref offset);
                case 2:
                    return parseDSBlock(data, ref offset);
                case 3:
                    return parseSBlock(data, ref offset);
                case 4:
                    return parseOBlock(data, ref offset);
                case 5:
                    return parseEBlock(data, ref offset);
                case 7:
                    return parseRCBlock(data, ref offset);
                case 9:
                    return parseSFBlock(data, ref offset);
                case 10:
                    return parseSGBlock(data, ref offset);
                case 11:
                    return parseSCBlock(data, ref offset);
                case 12:
                    return parseIBlock(data, ref offset);
                case 14:
                    return parseEBBlock(data, ref offset);
                case 16:
                    return parseGSBlock(data, ref offset);
                case 17:
                    return parseGLBlock(data, ref offset);
                case 20:
                    return parseRPBlock(data, ref offset);
                case 21:
                    return parseGCBlock(data, ref offset);
                case 23:
                    return parseRBlock(data, ref offset);
                case 24:
                    return parseFABlock(data, ref offset);
                case 25:
                    return parseFDBlock(data, ref offset);
                case 26:
                    return parsePSBlock(data, ref offset);
                case 30:
                    return parseCBlock(data, ref offset);
                case 31:
                    return parseVBlock(data, ref offset);
                case 32:
                    return parseRndBlock(data, ref offset);
                case 35:
                    return parseColorBlock(data, ref offset);
                case 36:
                    return parseComBlock(data, ref offset);
                case 37:
                    return parseAIBlock(data, ref offset);

                default:
                    return new UnknownBlock
                    {
                        Type = "Unknown",
                        Data = data.ToArray(),
                    };
            }
        }


        #region Blocks Parsing
        private Block parseMBlock(Span<byte> data, ref int offset)
        {
            var block = new MBlock
            {
                // 1
                Type = "M",
                Data = data.ToArray(),
                GravityX = getInt16(data, ref offset),
                MoveX = getInt16(data, ref offset),
                MoveY = getInt16(data, ref offset),
                GravityY = getInt16(data, ref offset),
            };
            var flags = getByte(data, ref offset);
            block.Add = isFlagOn(flags, 0);
            block.StopMoveX = isFlagOn(flags, 1);
            block.StopMoveY = isFlagOn(flags, 2);
            block.StopGravityX = isFlagOn(flags, 3);
            block.StopGravityY = isFlagOn(flags, 4);
            return block;
        }

        private Block parseDSBlock(Span<byte> data, ref int offset)
        {
            var block = new DSBlock
            {
                // 2
                Type = "DS",
                Data = data.ToArray(),
                When = (DSSkill)getByte(data, ref offset),
                Skill = getSkillBlock(data, ref offset),
            };
            return block;
        }

        private Block parseSBlock(Span<byte> data, ref int offset)
        {
            getByte(data, ref offset);
            var block = new SBlock
            {
                // 3
                Type = "S",
                Data = data.ToArray(),
                Sound = getInt16(data, ref offset),
            };

            return block;
        }

        private Block parseOBlock(Span<byte> data, ref int offset)
        {
            var block = new OBlock
            {
                // 4
                Type = "O",
                Data = data.ToArray(),
            };
            var flags = getByte(data, ref offset);
            block.Out = isFlagOn(flags, 0);
            block.Point = isFlagOn(flags, 1);
            block.UnCond = isFlagOn(flags, 2);
            block.Shadow = isFlagOn(flags, 3);
            block.Parent = isFlagOn(flags, 5);
            block.PicXY = isFlagOn(flags, 6);

            block.Skill = getSkillBlock(data, ref offset);
            block.OutSkill = getSkillBlock(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);
            block.Number = getByte(data, ref offset);
            block.Depth = getByte(data, ref offset);

            return block;
        }

        private Block parseEBlock(Span<byte> data, ref int offset)
        {
            var block = new EBlock
            {
                // 5
                Type = "E",
                Data = data.ToArray(),
            };

            return block;
        }


        private Block parseRCBlock(Span<byte> data, ref int offset)
        {
            var block = new RCBlock
            {
                // 7
                Type = "RC",
                Data = data.ToArray(),
            };

            var flags = getByte(data, ref offset);
            block.In = isFlagOn(flags, 0);
            block.TurnX = isFlagOn(flags, 2);
            block.TurnY = isFlagOn(flags, 3);
            block.Same = isFlagOn(flags, 4);

            block.CommonImage = getUInt16(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);

            return block;
        }

        private Block parseSFBlock(Span<byte> data, ref int offset)
        {
            var block = new SFBlock
            {
                // 9
                Type = "SF",
                Data = data.ToArray(),
                Loop = getByte(data, ref offset),
                Skill = getSkillBlock(data, ref offset),
            };
            return block;
        }

        private Block parseSGBlock(Span<byte> data, ref int offset)
        {
            var block = new SGBlock
            {
                // 10
                Type = "SG",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
            };
            return block;
        }

        private Block parseSCBlock(Span<byte> data, ref int offset)
        {
            var block = new SCBlock
            {
                // 11
                Type = "SC",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
            };
            return block;
        }

        private Block parseIBlock(Span<byte> data, ref int offset)
        {
            var block = new IBlock
            {
                // 12
                Type = "I",
                Data = data.ToArray(),
                Wait = getUInt16(data, ref offset),
            };

            byte flags;
            ushort value;

            getSplittedData(data, ref offset, out flags, out value);

            block.I = value;
            block.TurnX = isFlagOn(flags, 6);
            block.TurnY = isFlagOn(flags, 7);

            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);

            flags = getByte(data, ref offset);
            block.IgnoreDirection = isFlagOn(flags, 0);

            return block;
        }

        private Block parseEBBlock(Span<byte> data, ref int offset)
        {
            var block = new EBBlock
            {
                // 14
                Type = "EB",
                Data = data.ToArray(),
                FadingType = (EBFadingType)getByte(data, ref offset),
                Rgba = getRgba(data, ref offset),
                Duration = getUInt16(data, ref offset),
            };
            var flags = getByte(data, ref offset);
            block.Player = isFlagOn(flags, 0);
            block.Enemy = isFlagOn(flags, 1);
            block.BG = isFlagOn(flags, 2);
            block.System = isFlagOn(flags, 3);

            block.ShakeBgX = getEBShakeBG(data, ref offset);
            block.ShakeBgY = getEBShakeBG(data, ref offset);
            return block;
        }

        private EBShakeBg getEBShakeBG(Span<byte> data, ref int offset)
        {
            var result = new EBShakeBg
            {
                Type = (EBShakeBgType)getByte(data, ref offset),
                Shake = getByte(data, ref offset),
                Duration = getByte(data, ref offset),
            };
            return result;
        }

        private Block parseGSBlock(Span<byte> data, ref int offset)
        {
            getByte(data, ref offset);
            var block = new GSBlock
            {
                // 16
                Type = "GS",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
                IsMore = getByte(data, ref offset) == 1,
                Level = getByte(data, ref offset),
                Add = getInt16(data, ref offset),
            };
            return block;
        }

        private Block parseGLBlock(Span<byte> data, ref int offset)
        {
            getByte(data, ref offset);
            var block = new GLBlock
            {
                // 17
                Type = "GL",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
                IsMore = getByte(data, ref offset) == 1,
                Add = getInt16(data, ref offset),
            };
            return block;
        }

        private Block parseRPBlock(Span<byte> data, ref int offset)
        {
            var block = new RPBlock
            {
                // 20
                Type = "RP",
                Data = data.ToArray(),
            };

            var flags = getByte(data, ref offset);
            block.In = isFlagOn(flags, 0);
            block.TurnX = isFlagOn(flags, 2);

            block.HitJunction = getUInt16(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);

            return block;
        }

        private Block parseGCBlock(Span<byte> data, ref int offset)
        {
            getByte(data, ref offset);
            var block = new GCBlock
            {
                // 21
                Type = "GP",
                Data = data.ToArray(),
                PlayerLifeGauge = getInt16(data, ref offset),
                PlayerSpecialGauge = getInt16(data, ref offset),
                EnemyLifeGauge = getInt16(data, ref offset),
                EnemySpecialGauge = getInt16(data, ref offset),
            };
            return block;
        }


        private Block parseFABlock(Span<byte> data, ref int offset)
        {
            var block = new FABlock
            {
                // 24
                Type = "FA",
                Data = data.ToArray(),
                X = getInt16(data, ref offset),
                Y = getInt16(data, ref offset),
                Width = getInt16(data, ref offset),
                Height = getInt16(data, ref offset),
                Number = getByte(data, ref offset),
            };

            var flags = getByte(data, ref offset);
            block.Cancel = isFlagOn(flags, 0);
            block.NoDetection = isFlagOn(flags, 4);
            block.Combo = isFlagOn(flags, 1);
            block.NoSkyDetection = isFlagOn(flags, 3);
            block.GuardFail = isFlagOn(flags, 6);
            block.DuringGuard = isFlagOn(flags, 5);
            block.DuringReceipt = isFlagOn(flags, 7);
            block.Halfed = isFlagOn(flags, 2);

            // empty (maybe other flags)
            getByte(data, ref offset);

            block.Power = getByte(data, ref offset);

            return block;
        }

        private Block parseFDBlock(Span<byte> data, ref int offset)
        {
            var block = new FDBlock
            {
                // 25
                Type = "FD",
                Data = data.ToArray(),
                X = getInt16(data, ref offset),
                Y = getInt16(data, ref offset),
                Width = getInt16(data, ref offset),
                Height = getInt16(data, ref offset),
                Number = getByte(data, ref offset),
            };

            var flags = getByte(data, ref offset);
            block.Collide = isFlagOn(flags, 0);
            block.Damaged = isFlagOn(flags, 1);
            block.Throw = isFlagOn(flags, 2);

            block.DamageRate = getByte(data, ref offset);

            return block;
        }

        private Block parsePSBlock(Span<byte> data, ref int offset)
        {
            var block = new PSBlock
            {
                // 26
                Type = "PS",
                Data = data.ToArray(),
                PlayerTime = getByte(data, ref offset),
                EnemyTime = getByte(data, ref offset),
            };
            return block;
        }

        private Block parseRBlock(Span<byte> data, ref int offset)
        {
            var block = new RBlock
            {
                // 23
                Type = "R",
                Data = data.ToArray(),
                HitsStand = getInt16(data, ref offset),
                HitsCrouched = getInt16(data, ref offset),
                HitsAir = getInt16(data, ref offset),
                GuardStand = getInt16(data, ref offset),
                GuardCrouched = getInt16(data, ref offset),
                GuardAir = getInt16(data, ref offset),
            };

            return block;
        }

        private Block parseCBlock(Span<byte> data, ref int offset)
        {
            var block = new CBlock
            {
                // 30
                Type = "C",
                Data = data.ToArray(),
            };
            var flags = getByte(data, ref offset);

            block.Hits = isFlagOn(flags, 0);
            block.Uncond = isFlagOn(flags, 1);

            block.SkillCancelCondition = isFlagOn(flags, 3);

            block.From = getByte(data, ref offset);
            block.Skill = getSkill(data, ref offset);
            block.To = getByte(data, ref offset);

            return block;
        }

        private Block parseVBlock(Span<byte> data, ref int offset)
        {
            var block = new VBlock
            {
                // 31
                Type = "V",
                Data = data.ToArray(),
                MultiCondSkill = getSkillBlock(data, ref offset),
                Var = getByte(data, ref offset),
            };
            var flags = getByte(data, ref offset);
            block.Replace = isFlagOn(flags, 0);
            block.Add = isFlagOn(flags, 1);

            var itsTheSameFlag = isFlagOn(flags, 2);
            var itsAboveFlag = isFlagOn(flags, 3);

            block.ItsTheSame = itsTheSameFlag && !itsAboveFlag;
            block.ItsAbove = itsAboveFlag && !itsTheSameFlag;
            block.ItsBelow = itsAboveFlag && itsTheSameFlag;

            block.UseEven = isFlagOn(flags, 7);

            block.UseEvenVar = getByte(data, ref offset);
            block.Value = getInt16(data, ref offset);
            block.MultiCondValue = getInt16(data, ref offset);

            block.VarName = getVarName(block.Var);
            block.UseEvenVarName = getVarName(block.UseEvenVar);

            return block;
        }

        private string getVarName(byte var)
        {
            var aChar = 65;
            if (var >= 0 && var <= 16)
            {
                return $"Task Variable {(char)(aChar + var - 0)}";
            }
            else if (var >= 64 && var <= 79)
            {
                return $"Char Variable {(char)(aChar + var - 64)}";
            }
            else if (var >= 128 && var <= 143)
            {
                return $"System Variable {(char)(aChar + var - 128)}";
            }
            else if (var == 192)
            {
                return $"Data: X coor";
            }
            else if (var == 193)
            {
                return $"Data: Y coor";
            }
            else if (var == 194)
            {
                return $"Data: Map X coor";
            }
            else if (var == 195)
            {
                return $"Data: Map Y coor";
            }
            else if (var == 196)
            {
                return $"Data: Parent X";
            }
            else if (var == 197)
            {
                return $"Data: Parent Y";
            }
            else if (var == 198)
            {
                return $"Data: Time";
            }
            else if (var == 199)
            {
                return $"Data: No. Rounds";
            }
            else
            {
                return "Unknown";
            }
        }

        private Block parseRndBlock(Span<byte> data, ref int offset)
        {
            var block = new RndBlock
            {
                // 32
                Type = "Rnd",
                Data = data.ToArray(),
                RandomNum = getUInt16(data, ref offset),
                WhenItsAbove = getUInt16(data, ref offset),
            };
            getByte(data, ref offset);

            block.Skill = getSkillBlock(data, ref offset);

            return block;
        }

        private Block parseColorBlock(Span<byte> data, ref int offset)
        {
            var block = new ColorBlock
            {
                // 35
                Type = "COLOR",
                Data = data.ToArray(),
                Option = (ColorOption)getByte(data, ref offset),
                Rgba = getRgba(data, ref offset),
            };

            return block;
        }

        private Rgba getRgba(Span<byte> data, ref int offset)
        {
            var result = new Rgba
            {
                R = getByte(data, ref offset),
                G = getByte(data, ref offset),
                B = getByte(data, ref offset),
                A = getByte(data, ref offset),
            };
            return result;
        }

        private Block parseComBlock(Span<byte> data, ref int offset)
        {
            var block = new ComBlock
            {
                // 36
                Type = "COM",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
                Time = getByte(data, ref offset),
            };

            var steps = new List<CommandStep>();
            for (int i = 0; i < 5; i++)
            {
                var step = getCommandStep(data, ref offset);
                steps.Add(step);
            }
            block.Steps = steps;

            return block;
        }

        private Block parseAIBlock(Span<byte> data, ref int offset)
        {
            getByte(data, ref offset);
            getByte(data, ref offset);
            var block = new AIBlock
            {
                // 37
                Type = "AI",
                Data = data.ToArray(),
                Num = getByte(data, ref offset),
                Time = getByte(data, ref offset),
                Option = (ColorOption)getByte(data, ref offset),
                FadingType = (AIFadingType)getByte(data, ref offset),
                Rgba = getRgba(data, ref offset),
            };
            return block;
        }

        private CommandStep getCommandStep(Span<byte> data, ref int offset)
        {
            var step = new CommandStep();
            byte flags;
            ushort value;
            getSplittedData2(data, ref offset, out flags, out value);
            step.Direction = (ComDirection)value;

            step.A = isFlagOn(flags, 0);
            step.B = isFlagOn(flags, 1);
            step.C = isFlagOn(flags, 2);
            step.D = isFlagOn(flags, 3);

            flags = getByte(data, ref offset);
            step.E = isFlagOn(flags, 0);
            step.F = isFlagOn(flags, 1);

            step.Continue = isFlagOn(flags, 4);
            step.Active = isFlagOn(flags, 5);
            return step;
        }

        #endregion


        private SkillReference getSkill(Span<byte> data, ref int offset)
        {
            var result = new SkillReference
            {
                Number = getUInt16(data, ref offset),
            };
            result.Name = _skills[result.Number].Name;
            return result;
        }

        private SkillBlockReference getSkillBlock(Span<byte> data, ref int offset)
        {
            var result = new SkillBlockReference
            {
                Number = getUInt16(data, ref offset),
                Block = getByte(data, ref offset),
            };
            if (_skills.Count > result.Number)
            {
                result.Name = _skills[result.Number].Name;
            }
            else
            {
                Console.WriteLine($"Parse Error. Skill {result.Number} not found");
            }
            return result;
        }

        private void getSplittedData(Span<byte> data, ref int offset, out byte flags, out ushort value)
        {
            var word = getWord(data, 2, ref offset);

            var iMask = CreateBitMask(0, 5);
            flags = (byte)(word[1] & ~iMask);
            var iWord = new byte[2];
            word.CopyTo(iWord);
            iWord[1] = (byte)(word[1] & iMask);
            value = BitConverter.ToUInt16(iWord);
        }

        private void getSplittedData2(Span<byte> data, ref int offset, out byte flags, out ushort value)
        {
            var b = getByte(data, ref offset);

            var iMask = CreateBitMask(0, 4);
            flags = (byte)((b & ~iMask) / 16);
            value = (byte)(b & iMask);
        }

        private static uint CreateBitMask(int start, int length)
        {
            uint mask = 0xffffffff;
            mask >>= 32 - length;
            mask <<= start;
            return mask;
        }

        private bool isFlagOn(byte flags, byte position)
        {
            return (flags & (1 << position)) > 0;
        }

        private byte getByte(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 1, ref offset);
            return word[0];
        }

        private short getInt16(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 2, ref offset);
            return BitConverter.ToInt16(word);
        }

        private ushort getUInt16(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 2, ref offset);
            return BitConverter.ToUInt16(word);
        }


        private int getInt32(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 4, ref offset);
            return BitConverter.ToInt32(word);
        }

        private IList<Skill> readSkills(Span<byte> bytes, ref int offset)
        {
            var skills = new List<Skill>();

            var complete = false;
            do
            {
                var skill = readSkill(bytes, ref offset);

                if (!string.IsNullOrWhiteSpace(skill.Name))
                {
                    skill.Index = skills.Count();
                    skills.Add(skill);
                }
                else
                {
                    complete = true;
                }
            } while (!complete);

            return skills;
        }

        private Skill readSkill(Span<byte> bytes, ref int offset)
        {
            Span<byte> word;

            var result = new Skill
            {
                Name = getString(bytes, 32, ref offset),
                Position = getInt16(bytes, ref offset),
            };

            word = getWord(bytes, 1, ref offset);
            result.Type = getInt32(bytes, ref offset);

            return result;
        }

        private static Span<byte> getWord(Span<byte> bytes, int lenght, ref int offset)
        {
            Span<byte> word = bytes.Slice(offset, lenght);
            offset += lenght;
            return word;
        }
    }
}