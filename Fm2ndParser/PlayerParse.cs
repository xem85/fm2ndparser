using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Fm2ndParser
{
    internal partial class PlayerParse
    {
        public PlayerParse()
        {
        }

        public Player Parse(string filename)
        {
            var file = File.ReadAllBytes(filename);
            Span<byte> bytes = file;

            var offset = 0;

            var type = getString(bytes, 16, ref offset);

            var name = getString(bytes, 256, ref offset);

            var skills = readSkills(bytes, ref offset);

            getWord(bytes, 4, ref offset);

            var blocksCount = getInt16(bytes, ref offset);

            getWord(bytes, 2, ref offset);

            readSkillsBlocks(skills.ToList(), blocksCount, bytes, ref offset);

            var result = new Player
            {
                Type = type,
                Name = name,
                Skills = skills,
            };

            return result;
        }

        private string getString(Span<byte> bytes, int length, ref int offset)
        {
            var word = getWord(bytes, length, ref offset);
            var result = Encoding.Default.GetString(word).Replace("\0", "").Trim();
            return result;
        }

        private void readSkillsBlocks(IList<Skill> skills, int blocksCount, Span<byte> bytes, ref int offset)
        {
            var blocks = new List<Block>();
            var blocksData = new List<byte[]>();
            for (int i = 0; i < blocksCount - 1; i++)
            {
                blocksData.Add(getWord(bytes, 16, ref offset).ToArray());
            }

            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                skill.Blocks = new List<Block>();
                var endPosition = skill != skills.Last() ? skills[i + 1].Position : blocksCount;
                var skillBlocksCount = endPosition - skill.Position;

                foreach (var blockData in blocksData.Skip(skill.Position).Take(skillBlocksCount))
                {
                    if (!skill.Blocks.Any())
                    {
                        var settingsBlock = parseSkillSettings(skill.Type, blockData);
                        skill.Blocks.Add(settingsBlock);
                    }
                    else
                    {
                        var block = parseBlock(blockData);
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
                case 12:
                    return parseIBlock(data, ref offset);
                case 23:
                    return parseRBlock(data, ref offset);
                case 24:
                    return parseFABlock(data, ref offset);
                case 25:
                    return parseFDBlock(data, ref offset);
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

                default:
                    return new UnknownBlock
                    {
                        Type = "Unknown",
                        Data = data.ToArray(),
                    };
            }
        }


        #region Blocks Parsing
        private MBlock parseMBlock(Span<byte> data, ref int offset)
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
                Skill = getUInt16(data, ref offset),
                SkillBlock = getByte(data, ref offset),
            };
            return block;
        }

        private Block parseSBlock(Span<byte> data, ref int offset)
        {
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

            block.Skill = getUInt16(data, ref offset);
            block.SkillBlock = getByte(data, ref offset);
            block.OutSkill = getUInt16(data, ref offset);
            block.OutSkillBlock = getByte(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);
            block.Number = getByte(data, ref offset);
            block.Depth = getByte(data, ref offset);

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
            block.Skill = isFlagOn(flags, 3);

            block.From = getByte(data, ref offset);
            block.SkillNumber = getUInt16(data, ref offset);
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
                MultiCondSkill = getUInt16(data, ref offset),
                MultiCondSkillBlock = getByte(data, ref offset),
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
            else if (var >= 64 && var <= 69)
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

            block.Skill = getUInt16(data, ref offset);
            block.SkillBlock = getByte(data, ref offset);

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
                R = getByte(data, ref offset),
                G = getByte(data, ref offset),
                B = getByte(data, ref offset),
                A = getByte(data, ref offset),
            };

            return block;
        }

        private Block parseComBlock(Span<byte> data, ref int offset)
        {
            var block = new ComBlock
            {
                // 36
                Type = "COM",
                Data = data.ToArray(),
                Skill = getUInt16(data, ref offset),
                SkillBlock = getByte(data, ref offset),
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

        #endregion
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

        private ICollection<Skill> readSkills(Span<byte> bytes, ref int offset)
        {
            var skills = new List<Skill>();

            var complete = false;
            do
            {
                var skill = readSkill(bytes, ref offset);

                if (!string.IsNullOrWhiteSpace(skill.Name))
                {
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
                Type = getInt32(bytes, ref offset),
                Name = getString(bytes, 32, ref offset),
                Position = getInt16(bytes, ref offset),
            };

            word = getWord(bytes, 1, ref offset);

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