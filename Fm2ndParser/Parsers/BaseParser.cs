using Fm2ndParser.Blocks;
using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Kgt;
using Fm2ndParser.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fm2ndParser.Parsers
{
    public abstract class BaseParser<T>
        where T : FMFile, new()
    {
        protected string _filename;
        protected KGTFile _kgt;
        protected IList<Skill> _skills;
        protected IList<SkillBlockReference> _skillBlockRefs = new List<SkillBlockReference>();
        private BinaryReader _reader;

        public abstract string FileExtension { get; }

        public BaseParser(string filename, KGTFile kgt)
        {
            _filename = filename;
            _kgt = kgt;
        }

        public T Parse()
        {
            using var stream = File.OpenRead(_filename);
            using var reader = _reader = new BinaryReader(stream);
            return ParseInternal();
        }

        protected virtual T ParseInternal()
        {
            var type = getString(12);
            var loadedFlags = getUInt32();
            var loaded = loadedFlags == 1;

            if (type.StartsWith("2DKGT2G"))
                throw new LockedFileException(_filename);
            if (!type.StartsWith("2DKGT2K"))
                throw new InvalidDataException($"Not a valid Fighter Maker 2nd file: {_filename}");

            var name = getString(256);

            _skills = readSkills();

            readSkillsBlocks(_skills);
            setSkillReferenceBlockTypes();

            var images = readImages();

            var palettes = readGlobalPalettes();

            var sounds = readSounds();

            setSoundBlockNames(sounds);

            var result = new T
            {
                Type = Path.GetExtension(_filename).ToLowerInvariant().Substring(1),
                Loaded = loaded,
                Name = name,
                Skills = _skills,
                Images = images,
                GlobalPalettes = palettes,
                Sounds = sounds,
            };

            return result;
        }

        protected void readSkillsBlocks(IList<Skill> skills)
        {
            var blocksCount = getUInt32();

            for (int i = 0; i < skills.Count - 1; i++)  // .Count - 1 because the last skill is empty
            {
                var skill = skills[i];
                var skillBlocksCount = skills[i + 1].Position - skill.Position;

                for (int j = 0; j < skillBlocksCount; j++)
                {
                    var block = parseBlock();
                    block.Index = skill.Blocks.Count();
                    skill.Blocks.Add(block);
                }
            }
        }

        protected SettingsBlock parseSettingsBlock()
        {
            var data = getBytes(15);

            var result = new SettingsBlock
            {
                Data = data,
                Type = "Settings",
            };
            return result;
        }

        protected void setSettingsBlocksData()
        {
            foreach (var skill in _skills)
            {
                // should always be the first block, but just in case
                var settinsBlock = skill.Blocks.FirstOrDefault(x => x is SettingsBlock) as SettingsBlock;
                if (settinsBlock != null)
                    setSettingsBlockData(settinsBlock, getSettingsType((uint)skill.Index));
            }
        }

        protected void setSettingsBlockData(SettingsBlock settings, SettingsType settingsType)
        {
            using var reader = new BinaryReader(new MemoryStream(settings.Data));

            settings.SettingsType = settingsType;

            if (settingsType == SettingsType.None)
            {
            }
            else if (settingsType == SettingsType.HitMark)
            {
                settings.Position = (HitMarkPosition)reader.ReadByte();
                settings.NumberWidth = reader.ReadByte();
            }
            else if (settingsType == SettingsType.Time)
            {
                settings.Time = reader.ReadUInt32();
            }
            else if (settingsType == SettingsType.Position)
            {
                settings.X = reader.ReadInt16();
                settings.Y = reader.ReadInt16();
                settings.Width = reader.ReadByte();
            }
            else if (settingsType == SettingsType.MarkPosition)
            {
                settings.X = reader.ReadInt16();
                settings.Y = reader.ReadInt16();
                settings.Width = reader.ReadSByte();
                settings.Height = reader.ReadSByte();
            }
            else if (settingsType == SettingsType.Character)
            {
                var empty = reader.ReadByte();
                Debug.Assert(empty == 0);
                settings.Level = reader.ReadByte();
            }
            else if (settingsType == SettingsType.Stage)
            {
                // todo verify
                var flags = reader.ReadByte();
                settings.ConnectLtRt = isFlagOn(flags, 1);
                settings.ConnectUpDw = isFlagOn(flags, 2);
                settings.WidthEnabled = isFlagOn(flags, 3);
                settings.HeightEnabled = isFlagOn(flags, 4);
                settings.Width = reader.ReadInt16();
                settings.Height = reader.ReadInt16();
            }

        }

        protected Block parseBlock()
        {
            var startPosition = _reader.BaseStream.Position;
            var type = getByte();
            Block block = null;
            switch (type)
            {
                case 0:
                    block = parseSettingsBlock();
                    break;
                case 1:
                    block = parseMBlock();
                    break;
                case 2:
                    block = parseDSBlock();
                    break;
                case 3:
                    block = parseSBlock();
                    break;
                case 4:
                    block = parseOBlock();
                    break;
                case 5:
                    block = parseEBlock();
                    break;
                case 7:
                    block = parseRCBlock();
                    break;
                case 9:
                    block = parseSFBlock();
                    break;
                case 10:
                    block = parseSGBlock();
                    break;
                case 11:
                    block = parseSCBlock();
                    break;
                case 12:
                    block = parseIBlock();
                    break;
                case 14:
                    block = parseEBBlock();
                    break;
                case 16:
                    block = parseGSBlock();
                    break;
                case 17:
                    block = parseGLBlock();
                    break;
                case 20:
                    block = parseRPBlock();
                    break;
                case 21:
                    block = parseGCBlock();
                    break;
                case 22:
                    block = parseDBBlock();
                    break;
                case 23:
                    block = parseRBlock();
                    break;
                case 24:
                    block = parseFABlock();
                    break;
                case 25:
                    block = parseFDBlock();
                    break;
                case 26:
                    block = parsePSBlock();
                    break;
                case 30:
                    block = parseCBlock();
                    break;
                case 31:
                    block = parseVBlock();
                    break;
                case 32:
                    block = parseRndBlock();
                    break;
                case 35:
                    block = parseColorBlock();
                    break;
                case 36:
                    block = parseComBlock();
                    break;
                case 37:
                    block = parseAIBlock();
                    break;

                default:
                    throw new NotImplementedException($"Block type {type} not implemented");
            }

            var readBytes = _reader.BaseStream.Position - startPosition;
            skipEmptyBytes(16 - readBytes);

            return block;
        }

        protected abstract SettingsType getSettingsType(uint skillIdx);

        #region Blocks Parsing
        protected Block parseMBlock()
        {
            var block = new MBlock
            {
                // 1
                Type = "M",

                GravityX = getInt16(),
                MoveX = getInt16(),
                MoveY = getInt16(),
                GravityY = getInt16(),
            };
            var flags = getByte();
            block.Add = isFlagOn(flags, 0);
            block.StopMoveX = isFlagOn(flags, 1);
            block.StopMoveY = isFlagOn(flags, 2);
            block.StopGravityX = isFlagOn(flags, 3);
            block.StopGravityY = isFlagOn(flags, 4);
            return block;
        }

        protected Block parseDSBlock()
        {
            var block = new DSBlock
            {
                // 2
                Type = "DS",

                When = (DSSkill)getByte(),
                Skill = getSkillBlock(),
            };
            return block;
        }

        protected Block parseSBlock()
        {
            var unknown = getByte();
            var sound = getUInt16();

            var block = new SBlock
            {
                // 3
                Type = "S",

                Sound = new SkillReference
                {
                    Number = sound,
                },
            };

            return block;
        }

        protected Block parseOBlock()
        {
            var block = new OBlock
            {
                // 4
                Type = "O",

            };
            var flags = getByte();
            block.Out = isFlagOn(flags, 0);
            block.Point = isFlagOn(flags, 1);
            block.UnCond = isFlagOn(flags, 2);
            block.Shadow = isFlagOn(flags, 3);
            block.Parent = isFlagOn(flags, 5);
            block.PicXY = isFlagOn(flags, 6);

            block.Skill = getSkillBlock();
            block.OutSkill = getSkillBlock();
            block.X = getInt16();
            block.Y = getInt16();
            block.Number = getByte();
            block.Depth = getByte();

            return block;
        }

        protected Block parseEBlock()
        {
            var block = new EBlock
            {
                // 5
                Type = "E",

            };

            return block;
        }


        protected Block parseRCBlock()
        {
            var block = new RCBlock
            {
                // 7
                Type = "RC",

            };

            var flags = getByte();
            block.In = isFlagOn(flags, 0);
            block.TurnX = isFlagOn(flags, 2);
            block.TurnY = isFlagOn(flags, 3);
            block.Same = isFlagOn(flags, 4);

            //block.CommonImage =  getUInt16();
            block.CommonImage = getCommonImageBlock();
            block.X = getInt16();
            block.Y = getInt16();

            return block;
        }

        protected Block parseSFBlock()
        {
            var block = new SFBlock
            {
                // 9
                Type = "SF",

                Loop = getByte(),
                Skill = getSkillBlock(),
            };
            return block;
        }

        protected Block parseSGBlock()
        {
            var block = new SGBlock
            {
                // 10
                Type = "SG",

                Skill = getSkillBlock(),
            };
            return block;
        }

        protected Block parseSCBlock()
        {
            var block = new SCBlock
            {
                // 11
                Type = "SC",

                Skill = getSkillBlock(),
            };
            return block;
        }

        protected Block parseIBlock()
        {
            var block = new IBlock
            {
                // 12
                Type = "I",

                Wait = getUInt16(),
            };

            byte flags;
            ushort value;

            getSplittedData(out flags, out value);

            block.I = value;
            block.TurnX = isFlagOn(flags, 6);
            block.TurnY = isFlagOn(flags, 7);

            block.X = getInt16();
            block.Y = getInt16();

            flags = getByte();
            block.IgnoreDirection = isFlagOn(flags, 0);

            return block;
        }

        protected Block parseEBBlock()
        {
            var block = new EBBlock
            {
                // 14
                Type = "EB",

                FadingType = (EBFadingType)getByte(),
                Rgba = getRgba(),
                Duration = getUInt16(),
            };
            var flags = getByte();
            block.Player = isFlagOn(flags, 0);
            block.Enemy = isFlagOn(flags, 1);
            block.BG = isFlagOn(flags, 2);
            block.System = isFlagOn(flags, 3);

            block.ShakeBgX = getEBShakeBG();
            block.ShakeBgY = getEBShakeBG();
            return block;
        }

        protected EBShakeBg getEBShakeBG()
        {
            var result = new EBShakeBg
            {
                Type = (EBShakeBgType)getByte(),
                Shake = getByte(),
                Duration = getByte(),
            };
            return result;
        }

        protected Block parseGSBlock()
        {
            getByte();
            var block = new GSBlock
            {
                // 16
                Type = "GS",

                Skill = getSkillBlock(),
                IsMore = getByte() == 1,
                Level = getByte(),
                Add = getInt16(),
            };
            return block;
        }

        protected Block parseGLBlock()
        {
            getByte();
            var block = new GLBlock
            {
                // 17
                Type = "GL",

                Skill = getSkillBlock(),
                IsMore = getByte() == 1,
                Add = getInt16(),
            };
            return block;
        }

        protected Block parseRPBlock()
        {
            var block = new RPBlock
            {
                // 20
                Type = "RP",

            };

            var flags = getByte();
            block.In = isFlagOn(flags, 0);
            block.TurnX = isFlagOn(flags, 2);

            block.HitJunction = getHitJunctionBlock();
            block.X = getInt16();
            block.Y = getInt16();

            return block;
        }

        protected Block parseGCBlock()
        {
            getByte();
            var block = new GCBlock
            {
                // 21
                Type = "GC",

                PlayerLifeGauge = getInt16(),
                PlayerSpecialGauge = getInt16(),
                EnemyLifeGauge = getInt16(),
                EnemySpecialGauge = getInt16(),
            };
            return block;
        }

        protected Block parseDBBlock()
        {
            var ifFail = getByte() == 1;
            var skillRef = getSkillBlock();
            skipEmptyBytes(2);
            var condition = getByte();

            var block = new DBBlock
            {
                // 22
                Type = "DB",
                Fail = ifFail,
                Skill = skillRef,
                Condition = (DBCondition)condition,
            };
            return block;
        }
        protected Block parseFABlock()
        {
            var block = new FABlock
            {
                // 24
                Type = "FA",

                X = getInt16(),
                Y = getInt16(),
                Width = getInt16(),
                Height = getInt16(),
                Number = getByte(),
            };

            var flags = getByte();
            block.Cancel = isFlagOn(flags, 0);
            block.NoDetection = isFlagOn(flags, 4);
            block.Combo = isFlagOn(flags, 1);
            block.NoSkyDetection = isFlagOn(flags, 3);
            block.GuardFail = isFlagOn(flags, 6);
            block.DuringGuard = isFlagOn(flags, 5);
            block.DuringReceipt = isFlagOn(flags, 7);
            block.Halfed = isFlagOn(flags, 2);

            // empty (maybe other flags)
            getByte();

            block.Power = getByte();

            return block;
        }

        protected Block parseFDBlock()
        {
            var block = new FDBlock
            {
                // 25
                Type = "FD",

                X = getInt16(),
                Y = getInt16(),
                Width = getInt16(),
                Height = getInt16(),
                Number = getByte(),
            };

            var flags = getByte();
            block.Collide = isFlagOn(flags, 0);
            block.Damaged = isFlagOn(flags, 1);
            block.Throw = isFlagOn(flags, 2);

            block.DamageRate = getByte();

            return block;
        }

        protected Block parsePSBlock()
        {
            var block = new PSBlock
            {
                // 26
                Type = "PS",

                PlayerTime = getByte(),
                EnemyTime = getByte(),
            };
            return block;
        }

        protected Block parseRBlock()
        {
            var block = new RBlock
            {
                // 23
                Type = "R",

                HitsStand = getHitJunctionBlock(),
                HitsCrouched = getHitJunctionBlock(),
                HitsAir = getHitJunctionBlock(),
                GuardStand = getHitJunctionBlock(),
                GuardCrouched = getHitJunctionBlock(),
                GuardAir = getHitJunctionBlock(),
            };

            return block;
        }

        protected Block parseCBlock()
        {
            var block = new CBlock
            {
                // 30
                Type = "C",

            };
            var flags = getByte();

            block.Hits = isFlagOn(flags, 0);
            block.Uncond = isFlagOn(flags, 1);

            block.SkillCancelCondition = isFlagOn(flags, 3);

            block.From = getByte();
            block.Skill = getSkill();
            block.To = getByte();

            return block;
        }

        protected Block parseVBlock()
        {
            var block = new VBlock
            {
                // 31
                Type = "V",

                MultiCondSkill = getSkillBlock(),
                Var = getByte(),
            };
            var flags = getByte();
            block.Replace = isFlagOn(flags, 0);
            block.Add = isFlagOn(flags, 1);

            var itsTheSameFlag = isFlagOn(flags, 2);
            var itsAboveFlag = isFlagOn(flags, 3);

            block.ItsTheSame = itsTheSameFlag && !itsAboveFlag;
            block.ItsAbove = itsAboveFlag && !itsTheSameFlag;
            block.ItsBelow = itsAboveFlag && itsTheSameFlag;

            block.UseEven = isFlagOn(flags, 7);

            block.UseEvenVar = getByte();
            block.Value = getInt16();
            block.MultiCondValue = getInt16();

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
                throw new Exception($"Unknown variable name for {var}");
            }
        }

        protected Block parseRndBlock()
        {
            var block = new RndBlock
            {
                // 32
                Type = "Rnd",

                RandomNum = getUInt16(),
                WhenItsAbove = getUInt16(),
            };
            getByte();

            block.Skill = getSkillBlock();

            return block;
        }

        protected Block parseColorBlock()
        {
            var block = new ColorBlock
            {
                // 35
                Type = "COLOR",

                Option = (ColorOption)getByte(),
                Rgba = getRgba(),
            };

            return block;
        }

        protected Rgba getRgba()
        {
            var result = new Rgba
            {
                R = getByte(),
                G = getByte(),
                B = getByte(),
                A = getByte(),
            };
            return result;
        }

        protected Block parseComBlock()
        {
            var block = new ComBlock
            {
                // 36
                Type = "COM",

                Skill = getSkillBlock(),
                Time = getByte(),
            };

            var steps = new List<CommandStep>();
            for (int i = 0; i < 5; i++)
            {
                var step = getCommandStep();
                steps.Add(step);
            }
            block.Steps = steps;

            return block;
        }

        protected Block parseAIBlock()
        {
            getByte();
            getByte();
            var block = new AIBlock
            {
                // 37
                Type = "AI",

                Num = getByte(),
                Time = getByte(),
                Option = (ColorOption)getByte(),
                FadingType = (AIFadingType)getByte(),
                Rgba = getRgba(),
            };
            return block;
        }

        protected CommandStep getCommandStep()
        {
            var flags1 = getByte();
            var flags2 = getByte();

            var step = new CommandStep()
            {
                Direction = (ComDirection)(flags1 & 0b00001111),
                A = isFlagOn(flags1, 4),
                B = isFlagOn(flags1, 5),
                C = isFlagOn(flags1, 6),
                D = isFlagOn(flags1, 7),
                E = isFlagOn(flags2, 0),
                F = isFlagOn(flags2, 1),
                Continue = isFlagOn(flags2, 4),
                Active = isFlagOn(flags2, 5),
                Type = (CommandStepType)((flags2 & 0b11000000) >> 6),
            };
            return step;
        }

        #endregion

        protected ICollection<ImageResource> readImages()
        {
            var count = getUInt32();

            var images = new List<ImageResource>();

            for (int i = 0; i < count; i++)
            {
                var entryOffset = _reader.BaseStream.Position;

                var pointer = getBytes(0x4);
                var width = getUInt32();
                var height = getUInt32();
                var paletteType = getUInt32(); // 0: common, 1: private
                var packedSize = getUInt32();
                var isPacked = packedSize != 0;

                byte[] imageData = Array.Empty<byte>();

                var unpackedSize = checked((uint)(width * height + (paletteType == 1 ? 0x400u : 0u)));

                var sourceSize = isPacked ? packedSize : unpackedSize;

                if (width == 0 && height == 0)
                    sourceSize = 0;

                var sourceData = getBytes((int)sourceSize);

                if (isPacked)
                {
                    imageData = ImageCompression.Extract(sourceData, unpackedSize);
                }
                else
                    imageData = sourceData;

                images.Add(new ImageResource
                {
                    Pointer = pointer.ToArray(),
                    Width = width,
                    Height = height,
                    PaletteType = (PaletteType)paletteType,
                    PackedSize = packedSize,
                    Offset = (uint)entryOffset,
                    PackedData = isPacked ? sourceData : imageData,
                    Data = imageData,
                });
            }

            return images;
        }

        protected ICollection<Palette> readGlobalPalettes()
        {
            var result = new List<Palette>();

            for (int i = 0; i < 8; i++)
            {
                var palette = parsePalette();
                result.Add(palette);
            }

            return result;
        }

        protected ICollection<SoundResource> readSounds()
        {
            var count = getUInt32();
            var result = new List<SoundResource>();

            for (int i = 0; i < count; i++)
            {
                var pointer = getBytes(0x4);
                var name = getString(0x20);
                var size = getUInt32();
                var flags = getByte();
                assertUnusedFlags(flags, 0b11101100);

                var endlessLoop = isFlagOn(flags, 4);
                var type = (SoundType)(flags & 0b00000011);

                var cddaTrack = getByte();

                var soundData = size != 0 ? getBytes((int)size) : Array.Empty<byte>();

                result.Add(new SoundResource
                {
                    Pointer = pointer,
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

        #region palette
        private Palette parsePalette()
        {
            var position = _reader.BaseStream.Position;
            var data = getBytes(0x420);
            var colors = data.Chunk(4).Select(c => parseFM2kColor(c.ToArray())).ToArray();

            var result = new Palette
            {
                Position = position,
                Colors = colors,
            };

            return result;
        }

        private Color parseFM2kColor(byte[] color)
        {
            if (color.Count() != 4 && color[3] != 1 && color[3] != 0)
                throw new Exception("Wrong format");

            var b = color[0];
            var g = color[1];
            var r = color[2];

            //if (r % 8 != 0 || g % 8 != 0 || b % 8 != 0)
            //    throw new Exception("Wrong format");

            var a = color[3] == 0 ? 0 : 255;

            var result = Color.FromArgb(a, r, g, b);
            return result;
        }


        #endregion

        protected SkillReference getSkill()
        {
            var result = new SkillReference
            {
                Number = getUInt16(),
            };
            result.Name = _skills[result.Number].Name;
            return result;
        }

        protected SkillBlockReference getSkillBlock()
        {
            var result = new SkillBlockReference
            {
                Number = getUInt16(),
                Block = getByte(),
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

        private void setSkillReferenceBlockTypes()
        {
            foreach (var skillRef in _skillBlockRefs)
            {
                setSkillReferenceBlockType(skillRef);
            }
        }

        private void setSkillReferenceBlockType(SkillBlockReference skillBlockReference)
        {
            var blocks = _skills[skillBlockReference.Number].Blocks;
            if (blocks?.Count() > skillBlockReference.Block)
                skillBlockReference.BlockType = blocks.Skip(skillBlockReference.Block).First().Type;
        }

        protected SkillReference getHitJunctionBlock()
        {
            var result = new SkillReference
            {
                Number = getUInt16(),
            };
            if (_kgt?.HitJunctions.Count > result.Number)
            {
                result.Name = _kgt.HitJunctions[result.Number].Name;
            }
            else
            {
                Console.WriteLine($"Parse Error. Skill {result.Number} not found");
            }
            return result;
        }
        protected SkillReference getCommonImageBlock()
        {
            var result = new SkillReference
            {
                Number = getUInt16(),
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

        protected IList<Skill> readSkills()
        {
            var count = getUInt32();

            var skills = new List<Skill>();

            for (int i = 0; i < count; i++)
            {
                var skill = readSkill();
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
        protected Skill readSkill()
        {

            var result = new Skill
            {
                Name = getString(0x20),
                Position = getUInt16(),
            };

            // unknown, always 0
            var word = getByte();
            Debug.Assert(word == 0);

            result.Type = (SkillType)getUInt32();

            return result;
        }

        #region Parse utility
        /// <summary>
        /// Legge 2 byte consecutivi dallo stream binario e li divide in due parti logiche:
        /// i bit alti del secondo byte vengono restituiti in <paramref name="flags"/>,
        /// mentre i 5 bit bassi del secondo byte, insieme al primo byte, vengono usati
        /// per ricostruire un valore numerico a 16 bit restituito in <paramref name="value"/>.
        /// </summary>
        /// <param name="data">
        /// Buffer binario sorgente da cui leggere i dati.
        /// </param>
        /// <param name="offset">
        /// Posizione corrente nel buffer. Viene avanzata di 2 byte da <c>getWord</c>.
        /// </param>
        /// <param name="flags">
        /// Restituisce i flag contenuti nei bit alti del secondo byte.
        /// I 5 bit bassi vengono esclusi perché appartengono al valore numerico.
        /// </param>
        /// <param name="value">
        /// Restituisce il valore numerico ottenuto dai 2 byte letti,
        /// ma con il secondo byte mascherato in modo da conservare solo i suoi 5 bit bassi.
        /// </param>
        protected void getSplittedData(out byte flags, out ushort value)
        {
            var word = getBytes(2);

            var iMask = ByteUtility.CreateBitMask(0, 5);
            flags = (byte)(word[1] & ~iMask);
            var iWord = new byte[2];
            word.CopyTo(iWord);
            iWord[1] = (byte)(word[1] & iMask);
            value = BitConverter.ToUInt16(iWord);
        }

        protected bool isFlagOn(byte flags, byte position)
        {
            return (flags & (1 << position)) > 0;
        }

        protected byte getByte()
        {
            return _reader.ReadByte();
        }

        protected sbyte getInt8()
        {
            return _reader.ReadSByte();
        }

        protected short getInt16()
        {
            return _reader.ReadInt16();
        }

        protected ushort getUInt16()
        {
            return _reader.ReadUInt16();
        }

        protected int getInt32()
        {
            return _reader.ReadInt32();
        }

        protected uint getUInt32()
        {
            return _reader.ReadUInt32();
        }


        protected string getString(int length)
        {
            var word = getBytes(length);
            var zeroIndex = Array.IndexOf(word, (byte)0);
            var slice = zeroIndex >= 0 ? word.AsSpan(0, zeroIndex).ToArray() : word;

            // CP932 (Shift_JIS Microsoft variant)
            var result = Encoding.GetEncoding(932).GetString(slice);
            //var result = Encoding.Default.GetString(slice).Trim();
            //var result = Encoding.Default.GetString(word).Replace("\0", "").Trim();
            return result;
        }

        protected byte[] getBytes(int length)
        {
            return _reader.ReadBytes(length);
        }

        protected void skipEmptyBytes(long count)
        {
            for (long i = 0; i < count; i++)
            {
                var b = getByte();
                Debug.Assert(b == 0);
            }
        }

        protected void skipRemaningEmptyBytes()
        {
            // the remaning are all 0s
            skipEmptyBytes(_reader.BaseStream.Length - _reader.BaseStream.Position);
        }

        #endregion
        protected void assertUnusedFlags(byte flags, byte bitMask)
        {
            Debug.Assert((flags & bitMask) == 0);
        }
    }
}