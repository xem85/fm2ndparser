using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fm2ndParser
{
    public class BaseParser
    {
        protected string _filename;
        protected KGT _kgt;
        protected IList<Skill> _skills;
        protected IList<SkillBlockReference> _skillBlockRefs = new List<SkillBlockReference>();

        public BaseParser(string filename, KGT kgt)
        {
            _filename = filename;
            _kgt = kgt;
        }

        protected T parse<T>() where T : FMFile, new()
        {
            var file = File.ReadAllBytes(_filename);
            Span<byte> bytes = file;

            var offset = 0;

            return ParseInternal<T>(bytes, ref offset);
        }

        protected virtual T ParseInternal<T>(Span<byte> bytes, ref int offset) where T : FMFile, new()
        {
            var type = getString(bytes, 16, ref offset);
            if (type.StartsWith("2DKGT2G"))
                throw new LockedFileException();

            var name = getString(bytes, 256, ref offset);

            _skills = readSkills(bytes, ref offset);

            readSkillsBlocks(_skills, bytes, ref offset);
            setSkillBlockTypes();

            var images = readImages(bytes, ref offset);

            var palettes = readGlobalPalettes(bytes, ref offset);

            var sounds = readSounds(bytes, ref offset);

            setSoundBlockNames(sounds);

            var result = new T
            {
                Type = type,
                Name = name,
                Skills = _skills,
                Images = images,
                GlobalPalettes = palettes,
                Sounds = sounds,
            };

            return result;
        }

        protected string getString(Span<byte> bytes, int length, ref int offset)
        {
            var word = getWord(bytes, length, ref offset);
            var zeroIndex = word.IndexOf((byte)0);
            var slice = zeroIndex >= 0 ? word.Slice(0, zeroIndex) : word;

            // CP932 (Shift_JIS Microsoft variant)
            var result = Encoding.GetEncoding(932).GetString(slice);
            //var result = Encoding.Default.GetString(slice).Trim();
            //var result = Encoding.Default.GetString(word).Replace("\0", "").Trim();
            return result;
        }

        protected void readSkillsBlocks(IList<Skill> skills, Span<byte> bytes, ref int offset)
        {
            var blocksCount = getInt32(bytes, ref offset);

            var blocks = new List<Block>();
            var blocksData = new List<byte[]>();
            for (int i = 0; i < blocksCount; i++)
            {
                blocksData.Add(getWord(bytes, 16, ref offset).ToArray());
            }

            for (int i = 0; i < skills.Count - 1; i++)  // .Count - 1 because the last skill is empty
            {
                var skill = skills[i];
                var skillBlocksCount = skills[i + 1].Position - skill.Position;

                foreach (var blockData in blocksData.Skip(skill.Position).Take(skillBlocksCount))
                {
                    // todo, the settings can be simpli parsed as a block (?)
                    if (!skill.Blocks.Any())
                    {
                        var settingsBlock = parseSkillSettings(blockData);
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

        protected Settings parseSkillSettings(Span<byte> data)
        {
            var offset = 0;

            // 0: user
            // 1: none / cursor
            // 3: cursor position
            // 9: stage layout / life, special bars
            // 97: hit mark
            // 33: default
            // 57: timed
            // 65: time number
            // 129: special bar
            // 193: vitory mark
            // 131: Position: timer
            // 195: Pos: p1 Icon
            // 259: Pos: p2 Icon
            // 323: Pos: Special Stock 1P
            // 387: Pos: Special Stock 2P
            // 451: Pos: Victory Mark 1P
            // 515: Pos: Victory Mark 2P

            var type = getByte(data, ref offset);

            var unk = getByte(data, ref offset);

            var level = getByte(data, ref offset);

            var result = new Settings
            {
                Data = data.ToArray(),
                Type = "Settings",
                Level = level,
            };
            return result;
        }

        protected Block parseBlock(Span<byte> data)
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
        protected Block parseMBlock(Span<byte> data, ref int offset)
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

        protected Block parseDSBlock(Span<byte> data, ref int offset)
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

        protected Block parseSBlock(Span<byte> data, ref int offset)
        {
            var unknown = getByte(data, ref offset);
            var sound = getUInt16(data, ref offset);

            var block = new SBlock
            {
                // 3
                Type = "S",
                Data = data.ToArray(),
                Sound = new SkillReference
                {
                    Number = sound,
                },
            };

            return block;
        }

        protected Block parseOBlock(Span<byte> data, ref int offset)
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

        protected Block parseEBlock(Span<byte> data, ref int offset)
        {
            var block = new EBlock
            {
                // 5
                Type = "E",
                Data = data.ToArray(),
            };

            return block;
        }


        protected Block parseRCBlock(Span<byte> data, ref int offset)
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

            //block.CommonImage =  getUInt16(data, ref offset);
            block.CommonImage = getCommonImageBlock(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);

            return block;
        }

        protected Block parseSFBlock(Span<byte> data, ref int offset)
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

        protected Block parseSGBlock(Span<byte> data, ref int offset)
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

        protected Block parseSCBlock(Span<byte> data, ref int offset)
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

        protected Block parseIBlock(Span<byte> data, ref int offset)
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

        protected Block parseEBBlock(Span<byte> data, ref int offset)
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

        protected EBShakeBg getEBShakeBG(Span<byte> data, ref int offset)
        {
            var result = new EBShakeBg
            {
                Type = (EBShakeBgType)getByte(data, ref offset),
                Shake = getByte(data, ref offset),
                Duration = getByte(data, ref offset),
            };
            return result;
        }

        protected Block parseGSBlock(Span<byte> data, ref int offset)
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

        protected Block parseGLBlock(Span<byte> data, ref int offset)
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

        protected Block parseRPBlock(Span<byte> data, ref int offset)
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

            block.HitJunction = getHitJunctionBlock(data, ref offset);
            block.X = getInt16(data, ref offset);
            block.Y = getInt16(data, ref offset);

            return block;
        }

        protected Block parseGCBlock(Span<byte> data, ref int offset)
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


        protected Block parseFABlock(Span<byte> data, ref int offset)
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

        protected Block parseFDBlock(Span<byte> data, ref int offset)
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

        protected Block parsePSBlock(Span<byte> data, ref int offset)
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

        protected Block parseRBlock(Span<byte> data, ref int offset)
        {
            var block = new RBlock
            {
                // 23
                Type = "R",
                Data = data.ToArray(),
                HitsStand = getHitJunctionBlock(data, ref offset),
                HitsCrouched = getHitJunctionBlock(data, ref offset),
                HitsAir = getHitJunctionBlock(data, ref offset),
                GuardStand = getHitJunctionBlock(data, ref offset),
                GuardCrouched = getHitJunctionBlock(data, ref offset),
                GuardAir = getHitJunctionBlock(data, ref offset),
            };

            return block;
        }

        protected Block parseCBlock(Span<byte> data, ref int offset)
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

        protected Block parseVBlock(Span<byte> data, ref int offset)
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

        protected string getVarName(byte var)
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

        protected Block parseRndBlock(Span<byte> data, ref int offset)
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

        protected Block parseColorBlock(Span<byte> data, ref int offset)
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

        protected Rgba getRgba(Span<byte> data, ref int offset)
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

        protected Block parseComBlock(Span<byte> data, ref int offset)
        {
            var block = new ComBlock
            {
                // 36
                Type = "COM",
                Data = data.ToArray(),
                Skill = getSkillBlock(data, ref offset),
                Time = getByte(data, ref offset),
            };

            var steps = new List<BlockCommandStep>();
            for (int i = 0; i < 5; i++)
            {
                var step = getCommandStep(data, ref offset);
                steps.Add(step);
            }
            block.Steps = steps;

            return block;
        }

        protected Block parseAIBlock(Span<byte> data, ref int offset)
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

        protected BlockCommandStep getCommandStep(Span<byte> data, ref int offset)
        {
            var step = new BlockCommandStep();
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

        protected ICollection<ImageResource> readImages(Span<byte> bytes, ref int offset)
        {
            var count = getUInt32(bytes, ref offset);

            var images = new List<ImageResource>();

            for (int i = 0; i < count; i++)
            {
                var entryOffset = (uint)offset;

                var unknown = getUInt32(bytes, ref offset);
                var width = getUInt32(bytes, ref offset);
                var height = getUInt32(bytes, ref offset);
                var paletteType = getUInt32(bytes, ref offset); // 0: common, 1: private
                var packedSize = getUInt32(bytes, ref offset);

                byte[] imageData = Array.Empty<byte>();

                if (packedSize != 0)
                {
                    var packed = getWord(bytes, (int)packedSize, ref offset).ToArray();
                    var unpackedSize = checked((int)(width * height + (paletteType == 1 ? 0x400u : 0u)));
                    imageData = extractSprite(packed, unpackedSize);
                }
                else if (width != 0 && height != 0)
                {
                    var rawSize = checked((int)(width * height + (paletteType == 1 ? 0x400u : 0u)));
                    imageData = getWord(bytes, rawSize, ref offset).ToArray();
                }

                images.Add(new ImageResource
                {
                    Width = width,
                    Height = height,
                    PaletteType = paletteType,
                    PackedSize = packedSize,
                    Offset = entryOffset,
                    Data = imageData,
                });
            }

            return images;
        }

        protected ICollection<byte[]> readGlobalPalettes(Span<byte> bytes, ref int offset)
        {
            var result = new List<byte[]>();

            for (int i = 0; i < 8; i++)
            {
                var palette = getWord(bytes, 0x420, ref offset).ToArray();
                result.Add(palette);
            }

            return result;
        }

        protected ICollection<SoundResource> readSounds(Span<byte> bytes, ref int offset)
        {
            var count = getInt32(bytes, ref offset);
            var result = new List<SoundResource>();

            for (int i = 0; i < count; i++)
            {
                var unknown = getWord(bytes, 0x4, ref offset);
                var name = getString(bytes, 0x20, ref offset);
                var size = getUInt32(bytes, ref offset);
                var flags = getByte(bytes, ref offset);
                assertUnusedFlags(flags, 0b11101100);

                var endlessLoop = isFlagOn(flags, 5);
                var type = (SoundType)(flags & 0b00000011);

                var cddaTrack = getByte(bytes, ref offset);

                var soundData = size != 0 ? getWord(bytes, (int)size, ref offset).ToArray() : Array.Empty<byte>();

                result.Add(new SoundResource
                {
                    Name = name,
                    Size = size,
                    Type = type,
                    EndlessLoop = endlessLoop,
                    CDDATrack = cddaTrack,
                    Data = soundData,
                });
            }

            return result;
        }

        protected byte[] extractSprite(byte[] source, int destinationSize)
        {
            var destination = new byte[destinationSize];

            var pos = 0;
            var pos2 = 0;

            while (pos < source.Length && pos2 < destinationSize)
            {
                uint tmp = source[pos];
                var tmp2 = tmp >> 6;
                tmp = tmp & 0x3f;

                if (tmp == 0)
                {
                    pos = pos + 1;
                    if (pos >= source.Length)
                        break;

                    tmp = source[pos];
                    if (tmp != 0)
                    {
                        tmp = tmp + 0x3f;
                    }
                    else
                    {
                        if (pos + 3 >= source.Length)
                            break;

                        tmp = BitConverter.ToUInt16(source, pos + 1);
                        var tmp3 = (uint)(source[pos + 3] << 0x10);
                        tmp = tmp + tmp3 + 0x13f;
                        pos = pos + 3;
                    }
                }

                switch (tmp2)
                {
                    case 0:
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            destination[pos2++] = 0;
                        }
                        break;

                    case 1:
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            pos = pos + 1;
                            if (pos >= source.Length)
                                break;

                            destination[pos2++] = source[pos];
                        }
                        break;

                    case 2:
                        pos = pos + 1;
                        if (pos >= source.Length)
                            break;

                        var repeatedByte = source[pos];
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            destination[pos2++] = repeatedByte;
                        }
                        break;

                    case 3:
                        pos = pos + 1;
                        if (pos >= source.Length)
                            break;

                        var copyDistance = (int)source[pos];
                        if (copyDistance == 0)
                        {
                            pos = pos + 1;
                            if (pos >= source.Length)
                                break;

                            copyDistance = (source[pos] + 1) << 8;
                            pos = pos + 1;
                        }

                        var readPos = pos2 - copyDistance;
                        for (int i = 0; i < tmp && pos2 < destinationSize; i++)
                        {
                            if (readPos < 0 || readPos >= destinationSize)
                                break;

                            destination[pos2++] = destination[readPos++];
                        }
                        break;
                }

                pos = pos + 1;
            }

            return destination;
        }

        protected SkillReference getSkill(Span<byte> data, ref int offset)
        {
            var result = new SkillReference
            {
                Number = getUInt16(data, ref offset),
            };
            result.Name = _skills[result.Number].Name;
            return result;
        }

        protected SkillBlockReference getSkillBlock(Span<byte> data, ref int offset)
        {
            var result = new SkillBlockReference
            {
                Number = getUInt16(data, ref offset),
                Block = getByte(data, ref offset),
            };
            if (_skills.Count > result.Number)
            {
                result.Name = _skills[result.Number].Name;
                _skillBlockRefs.Add(result);
            }
            else
            {
                Console.WriteLine($"Parse Error. Skill {result.Number} not found");
            }
            return result;
        }

        private void setSoundBlockNames(ICollection<SoundResource> sounds)
        {
            foreach (var sBlock in _skills.SelectMany(x => x.Blocks).Where(x => x is SBlock).Cast<SBlock>())
            {
                var sound = sBlock.Sound;
                sound.Name = sounds.Skip(sound.Number).First().Name;
            }
        }

        private void setSkillBlockTypes()
        {
            foreach (var skillRef in _skillBlockRefs)
            {
                setSkillBlockType(skillRef);
            }
        }

        private void setSkillBlockType(SkillBlockReference skillBlockReference)
        {
            var blocks = _skills[skillBlockReference.Number].Blocks;
            if (blocks?.Count() > skillBlockReference.Block)
                skillBlockReference.BlockType = blocks.Skip(skillBlockReference.Block).First().Type;
        }

        protected SkillReference getHitJunctionBlock(Span<byte> data, ref int offset)
        {
            var result = new SkillReference
            {
                Number = getUInt16(data, ref offset),
            };
            if (_kgt?.HitJunctions.Count > result.Number)
            {
                result.Name = _kgt.HitJunctions[result.Number];
            }
            else
            {
                Console.WriteLine($"Parse Error. Skill {result.Number} not found");
            }
            return result;
        }
        protected SkillReference getCommonImageBlock(Span<byte> data, ref int offset)
        {
            var result = new SkillReference
            {
                Number = getUInt16(data, ref offset),
            };
            if (_kgt?.CommonImages.Count > result.Number)
            {
                result.Name = _kgt.CommonImages[result.Number];
            }
            else
            {
                Console.WriteLine($"Parse Error. Skill {result.Number} not found");
            }
            return result;
        }


        protected void getSplittedData(Span<byte> data, ref int offset, out byte flags, out ushort value)
        {
            var word = getWord(data, 2, ref offset);

            var iMask = CreateBitMask(0, 5);
            flags = (byte)(word[1] & ~iMask);
            var iWord = new byte[2];
            word.CopyTo(iWord);
            iWord[1] = (byte)(word[1] & iMask);
            value = BitConverter.ToUInt16(iWord);
        }

        protected void getSplittedData2(Span<byte> data, ref int offset, out byte flags, out ushort value)
        {
            var b = getByte(data, ref offset);

            var iMask = CreateBitMask(0, 4);
            flags = (byte)((b & ~iMask) / 16);
            value = (byte)(b & iMask);
        }

        protected static uint CreateBitMask(int start, int length)
        {
            uint mask = 0xffffffff;
            mask >>= 32 - length;
            mask <<= start;
            return mask;
        }

        protected bool isFlagOn(byte flags, byte position)
        {
            return (flags & (1 << position)) > 0;
        }

        protected byte getByte(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 1, ref offset);
            return word[0];
        }

        protected sbyte getInt8(Span<byte> data, ref int offset)
        {
            var result = getByte(data, ref offset);
            return (sbyte)result;
        }

        protected short getInt16(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 2, ref offset);
            return BitConverter.ToInt16(word);
        }

        protected ushort getUInt16(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 2, ref offset);
            return BitConverter.ToUInt16(word);
        }


        protected int getInt32(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 4, ref offset);
            return BitConverter.ToInt32(word);
        }

        protected uint getUInt32(Span<byte> data, ref int offset)
        {
            var word = getWord(data, 4, ref offset);
            return BitConverter.ToUInt32(word);
        }

        protected IList<Skill> readSkills(Span<byte> bytes, ref int offset)
        {
            var count = getUInt32(bytes, ref offset);

            var skills = new List<Skill>();

            for (int i = 0; i < count; i++)
            {
                var skill = readSkill(bytes, ref offset);
                skill.Index = skills.Count();
                skills.Add(skill);
            }

            return skills;
        }

        // 39 bytes
        // 0x20 ansichar: name
        // 0x02 uint16: position
        // 0x01 unknown
        // 0x04 uint32: type
        protected Skill readSkill(Span<byte> bytes, ref int offset)
        {

            var result = new Skill
            {
                Name = getString(bytes, 32, ref offset),
                Position = getUInt16(bytes, ref offset),
            };

            // unknown, always 0
            var word = getByte(bytes, ref offset);
            Debug.Assert(word == 0);

            result.Type = getUInt32(bytes, ref offset);

            return result;
        }

        protected IList<Command> parseCommands(Span<byte> bytes, ref int offset)
        {
            var count = getInt32(bytes, ref offset);

            var commands = new List<Command>();

            for (int i = 0; i < count; i++)
            {
                var command = parseCommand(bytes, ref offset);
                commands.Add(command);
            }

            return commands;
        }

        protected Command parseCommand(Span<byte> bytes, ref int offset)
        {
            var result = new Command
            {
                Name = getString(bytes, 32, ref offset),
                Time = getUInt16(bytes, ref offset),
                AirSkill = getSkill(bytes, ref offset),
                StandSkill = getSkill(bytes, ref offset),
                StandFarSkill = getSkill(bytes, ref offset),
                CrouchedSkill = getSkill(bytes, ref offset),
            };

            var steps = new List<CommandStep>();
            for (int i = 0; i < 10; i++)
            {

                var flags1 = getByte(bytes, ref offset);
                var flags2 = getByte(bytes, ref offset);

                var step = new CommandStep()
                {
                    A = isFlagOn(flags1, 3),
                    B = isFlagOn(flags1, 2),
                    C = isFlagOn(flags1, 1),
                    D = isFlagOn(flags1, 0),
                    E = isFlagOn(flags2, 7),
                    F = isFlagOn(flags2, 6),
                    Continue = isFlagOn(flags2, 4),
                    Active = isFlagOn(flags2, 5),
                    Direction = (ComDirection)(flags1 & 0b00001111),
                    Type = (CommandStepType)(flags1 & 0b11100000),
                };
                steps.Add(step);
            }
            for (int i = 0; i < 10; i++)
            {
                steps[i].Amount = getUInt16(bytes, ref offset);
            }

            result.Steps = steps;

            return result;
        }

        protected static Span<byte> getWord(Span<byte> bytes, int length, ref int offset)
        {
            Span<byte> word = bytes.Slice(offset, length);
            offset += length;
            return word;
        }

        protected void skipEmptyBytes(Span<byte> bytes, int count, ref int offset)
        {
            for (int i = 0; i < count; i++)
            {
                var b = getByte(bytes, ref offset);
                Debug.Assert(b == 0);
            }
        }

        protected void skiRemaningEmptyBytes(Span<byte> bytes, ref int offset)
        {
            // the remaning are all 0s
            skipEmptyBytes(bytes, bytes.Length - offset, ref offset);
        }

        protected void assertUnusedFlags(byte flags, byte bitMask)
        {
            Debug.Assert((flags & bitMask) == 0);
        }
    }
}