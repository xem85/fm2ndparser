using Fm2ndParser.Blocks;
using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Kgt;
using Fm2ndParser.Utility;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fm2ndParser.Compilers
{
    public abstract class BaseCompiler<T>
        where T : FMFile, new()
    {
        protected T _fmFile;
        protected BinaryWriter _writer;
        protected KGTFile _kgtFile;

        public BaseCompiler(T fmFile, KGTFile kgtFile)
        {
            _fmFile = fmFile;
            _kgtFile = kgtFile;
        }

        public void Compile(string outputFilename)
        {
            using var stream = File.OpenWrite(outputFilename);
            Compile(stream);
        }

        public void Compile(Stream stream)
        {
            using var writer = _writer = new BinaryWriter(stream);
            CompileInternal();
        }

        protected virtual void CompileInternal()
        {
            writeHeader();

            writeName();
            writeSkills();
            writeBlocks();
            writeImages();
            writeGlobalPalettes();
            writeSounds();
        }

        // 16 byte header
        private void writeHeader()
        {
            writeString("2DKGT2K");
            writeZeros(5);
            writeBytes([_kgtFile.Loaded ? (byte)1 : (byte)0]);
            writeZeros(3);
        }

        private void writeBytes(object value)
        {
            throw new NotImplementedException();
        }

        // 256 byte name
        private void writeName()
        {
            writeString(_fmFile.Name, 256);
        }

        private void writeSkills()
        {
            writeUInt32((uint)_fmFile.Skills.Count());

            ushort position = 0;
            foreach (var skill in _fmFile.Skills)
            {
                writeString(skill.Name, 32);
                writeUInt16(position);
                writeZeros(1);
                writeUInt32((uint)skill.Type);
                position += (ushort)skill.Blocks.Count();
            }
        }
        private void writeBlocks()
        {
            var blocksCount = (uint)_fmFile.Skills.SelectMany(x => x.Blocks).Count();
            writeUInt32(blocksCount);
            foreach (var skill in _fmFile.Skills)
            {
                foreach (var block in skill.Blocks)
                {
                    var startPosition = _writer.BaseStream.Position;

                    writeBytes([(byte)toBlockType(block.Type)]);
                    switch (toBlockType(block.Type))
                    {
                        case BlockType.Settings:
                            writeSettiningsBlock((SettingsBlock)block);
                            break;
                        case BlockType.M:
                            writeMBlock((MBlock)block);
                            break;
                        case BlockType.DS:
                            writeDSBlock((DSBlock)block);
                            break;
                        case BlockType.S:
                            writeSBlock((SBlock)block);
                            break;
                        case BlockType.O:
                            writeOBlock((OBlock)block);
                            break;
                        case BlockType.E:
                            writeEBlock((EBlock)block);
                            break;
                        case BlockType.RC:
                            writeRCBlock((RCBlock)block);
                            break;
                        case BlockType.SF:
                            writeSFBlock((SFBlock)block);
                            break;
                        case BlockType.SG:
                            writeSGBlock((SGBlock)block);
                            break;
                        case BlockType.SC:
                            writeSCBlock((SCBlock)block);
                            break;
                        case BlockType.I:
                            writeIBlock((IBlock)block);
                            break;
                        case BlockType.EB:
                            writeEBBlock((EBBlock)block);
                            break;
                        case BlockType.GS:
                            writeGSBlock((GSBlock)block);
                            break;
                        case BlockType.GL:
                            writeGLBlock((GLBlock)block);
                            break;
                        case BlockType.RP:
                            writeRPBlock((RPBlock)block);
                            break;
                        case BlockType.GC:
                            writeGCBlock((GCBlock)block);
                            break;
                        case BlockType.DB:
                            writeDBBlock((DBBlock)block);
                            break;
                        case BlockType.R:
                            writeRBlock((RBlock)block);
                            break;
                        case BlockType.FA:
                            writeFABlock((FABlock)block);
                            break;
                        case BlockType.FD:
                            writeFDBlock((FDBlock)block);
                            break;
                        case BlockType.PS:
                            writePSBlock((PSBlock)block);
                            break;
                        case BlockType.C:
                            writeCBlock((CBlock)block);
                            break;
                        case BlockType.V:
                            writeVBlock((VBlock)block);
                            break;
                        case BlockType.Rnd:
                            writeRndBlock((RndBlock)block);
                            break;
                        case BlockType.COLOR:
                            writeColorBlock((ColorBlock)block);
                            break;
                        case BlockType.COM:
                            writeComBlock((ComBlock)block);
                            break;
                        case BlockType.AI:
                            writeAIBlock((AIBlock)block);
                            break;
                    }

                    var wroteBytes = _writer.BaseStream.Position - startPosition;
                    writeZeros(16 - (int)wroteBytes);
                }
            }
        }

        private void writeImages()
        {
            writeUInt32((uint)_fmFile.Images.Count());
            foreach (var image in _fmFile.Images)
            {
                var isPacked = image.PackedSize >= 0;
                var enableRePacking = false;
                if (enableRePacking && isPacked && (image.PackedData == null || image.PackedData.Length == 0))
                {
                    // update packedData
                    image.PackedData = ImageCompression.Compress(image.Data);
                    image.PackedSize = (uint)image.PackedData.LongLength;
                }

                writeBytes(image.Pointer); // 4 bytes
                writeUInt32(image.Width);
                writeUInt32(image.Height);
                writeUInt32((uint)image.PaletteType);
                writeUInt32(image.PackedSize);

                var paletteSize = (image.PaletteType == PaletteType.Private ? 0x400u : 0u);
                var rawSize = checked((int)(image.Width * image.Height + paletteSize));

                if (isPacked)
                    writeBytes(image.PackedData);
                else
                    writeBytes(image.Data);
            }
        }

        private void writeGlobalPalettes()
        {
            foreach (var palette in _fmFile.GlobalPalettes)
            {
                var data = ParseCommand.ToFM2kPalette(palette.Colors);
                writeBytes(data);
            }
        }

        private void writeSounds()
        {
            writeUInt32((uint)_fmFile.Sounds.Count());
            foreach (var sound in _fmFile.Sounds)
            {
                writeBytes(sound.Pointer); // 4 bytes
                writeString(sound.Name, 0x20);
                writeUInt32(sound.Size);

                byte flags = 0;

                if (sound.EndlessLoop) flags |= 1 << 4;
                flags |= (byte)((byte)sound.Type & 0b00000011);
                writeBytes([flags]);

                writeBytes([sound.CDDATrack]);

                writeBytes(sound.Data);
            }
        }

        #region Write blocks
        protected void writeSettiningsBlock(SettingsBlock block)
        {
            switch (block.SettingsType)
            {
                case SettingsType.None:
                    break;
                case SettingsType.HitMark:
                    writeBytes([(byte)block.Position]);
                    writeBytes([block.NumberWidth]);
                    break;
                case SettingsType.Time:
                    writeUInt32(block.Time);
                    break;
                case SettingsType.Position:
                    writeInt16(block.X);
                    writeInt16(block.Y);
                    writeBytes([(byte)block.Width]);
                    break;
                case SettingsType.MarkPosition:
                    writeInt16(block.X);
                    writeInt16(block.Y);
                    writeInt8((sbyte)block.Width);
                    writeInt8((sbyte)block.Height);
                    break;
                case SettingsType.Character:
                    writeZeros(1);
                    writeBytes([(byte)block.Level]);
                    break;
                case SettingsType.Stage:
                    byte flags = 0;
                    if (block.ConnectLtRt) flags |= 1 << 1;
                    if (block.ConnectUpDw) flags |= 1 << 2;
                    if (block.WidthEnabled) flags |= 1 << 3;
                    if (block.HeightEnabled) flags |= 1 << 4;
                    writeBytes([flags]);

                    writeInt16(block.Width);
                    writeInt16(block.Height);

                    break;
                default:
                    throw new Exception("Unknown SettingsType: " + block.SettingsType);
            }
        }

        private void writeMBlock(MBlock block)
        {
            writeInt16(block.GravityX);
            writeInt16(block.MoveX);
            writeInt16(block.MoveY);
            writeInt16(block.GravityY);

            byte flags = 0;

            if (block.Add) flags |= 1 << 0;
            if (block.StopMoveX) flags |= 1 << 1;
            if (block.StopMoveY) flags |= 1 << 2;
            if (block.StopGravityX) flags |= 1 << 3;
            if (block.StopGravityY) flags |= 1 << 4;

            writeBytes([flags]);
        }

        private void writeDSBlock(DSBlock block)
        {
            writeBytes([(byte)block.When]);
            writeSkillBlockReference(block.Skill);
        }
        private void writeSBlock(SBlock block)
        {
            writeZeros(1);  // unknown byte
            writeUInt16(block.Sound.Number);    // todo get number from sound reference
        }

        private void writeOBlock(OBlock block)
        {
            byte flags = 0;
            if (block.Out) flags |= 1 << 0;
            if (block.Point) flags |= 1 << 1;
            if (block.UnCond) flags |= 1 << 2;
            if (block.Shadow) flags |= 1 << 3;
            if (block.Parent) flags |= 1 << 5;
            if (block.PicXY) flags |= 1 << 6;

            writeBytes(new byte[] { flags });

            writeSkillBlockReference(block.Skill);
            writeSkillBlockReference(block.OutSkill);
            writeInt16(block.X);
            writeInt16(block.Y);
            writeBytes([block.Number]);
            writeBytes([block.Depth]);
        }

        private void writeEBlock(EBlock block)
        {
        }

        private void writeRCBlock(RCBlock block)
        {
            byte flags = 0;
            if (block.In) flags |= 1 << 0;
            if (block.TurnX) flags |= 1 << 2;
            if (block.TurnY) flags |= 1 << 3;
            if (block.Same) flags |= 1 << 4;
            writeBytes(new byte[] { flags });

            writeUInt16(block.CommonImage.Number);
            writeInt16(block.X);
            writeInt16(block.Y);
        }

        private void writeSFBlock(SFBlock block)
        {
            writeBytes([block.Loop]);
            writeSkillBlockReference(block.Skill);
        }

        private void writeSGBlock(SGBlock block)
        {
            writeSkillBlockReference(block.Skill);
        }

        private void writeSCBlock(SCBlock block)
        {
            writeSkillBlockReference(block.Skill);
        }

        private void writeIBlock(IBlock block)
        {
            writeUInt16(block.Wait);
            byte flags = 0;

            if (block.TurnX) { flags |= 1 << 6; }
            if (block.TurnY) { flags |= 1 << 7; }

            writeBytes(setSplittedData(flags, block.I));

            writeInt16(block.X);
            writeInt16(block.Y);

            byte ignoreDirection = block.IgnoreDirection ? (byte)0x01 : (byte)0x00;

            writeBytes([ignoreDirection]);
        }

        protected byte[] setSplittedData(byte flags, ushort value)
        {
            var iMask = (byte)ByteUtility.CreateBitMask(0, 5);

            // flags può occupare solo i 3 bit alti del secondo byte
            if ((flags & iMask) != 0)
                throw new ArgumentOutOfRangeException(nameof(flags), "flags deve usare solo i 3 bit alti del byte.");

            // value usa 8 bit del primo byte + 5 bit bassi del secondo byte = 13 bit totali
            if ((value & ~0x1FFF) != 0)
                throw new ArgumentOutOfRangeException(nameof(value), "value deve essere compreso tra 0 e 8191.");

            var word = new byte[2];

            // Primo byte: 8 bit bassi del valore
            word[0] = (byte)(value & 0xFF);

            // Secondo byte:
            // - 5 bit bassi = parte alta di value
            // - 3 bit alti = flags
            word[1] = (byte)(((value >> 8) & iMask) | flags);

            return word;
        }

        private void writeEBBlock(EBBlock block)
        {
            writeBytes([(byte)block.FadingType]);
            writeRgba(block.Rgba);
            writeUInt16(block.Duration);

            byte flags = 0;

            if (block.Player) flags |= 1 << 0;
            if (block.Enemy) flags |= 1 << 1;
            if (block.BG) flags |= 1 << 2;
            if (block.System) flags |= 1 << 3;
            writeBytes([flags]);

            writeEBShakeBG(block.ShakeBgX);
            writeEBShakeBG(block.ShakeBgY);
        }


        private void writeGSBlock(GSBlock block)
        {
            writeZeros(1);

            writeSkillBlockReference(block.Skill);
            writeBytes([block.IsMore ? (byte)1 : (byte)0]);
            writeBytes([block.Level]);
            writeInt16(block.Add);
        }

        private void writeGLBlock(GLBlock block)
        {
            writeZeros(1);

            writeSkillBlockReference(block.Skill);
            writeBytes([block.IsMore ? (byte)1 : (byte)0]);
            writeInt16(block.Add);
        }

        private void writeRPBlock(RPBlock block)
        {
            byte flags = 0;

            if (block.In) flags |= 1 << 0;
            if (block.TurnX) flags |= 1 << 2;
            writeBytes([flags]);

            writeHitJunctionBlock(block.HitJunction);

            writeInt16(block.X);
            writeInt16(block.Y);
        }

        private void writeGCBlock(GCBlock block)
        {
            writeZeros(1);

            writeInt16(block.PlayerLifeGauge);
            writeInt16(block.PlayerSpecialGauge);
            writeInt16(block.EnemyLifeGauge);
            writeInt16(block.EnemySpecialGauge);
        }
        private void writeDBBlock(DBBlock block)
        {
            writeBytes([block.Fail ? (byte)1 : (byte)0]);
            writeSkillBlockReference(block.Skill);
            writeZeros(2);
            writeBytes([(byte)block.Condition]);
        }

        private void writeRBlock(RBlock block)
        {
            writeHitJunctionBlock(block.HitsStand);
            writeHitJunctionBlock(block.HitsCrouched);
            writeHitJunctionBlock(block.HitsAir);
            writeHitJunctionBlock(block.GuardStand);
            writeHitJunctionBlock(block.GuardCrouched);
            writeHitJunctionBlock(block.GuardAir);
        }

        private void writeFABlock(FABlock block)
        {
            writeInt16(block.X);
            writeInt16(block.Y);

            writeInt16(block.Width);
            writeInt16(block.Height);
            writeBytes([block.Number]);

            byte flags = 0;

            if (block.Cancel) flags |= 1 << 0;
            if (block.Combo) flags |= 1 << 1;
            if (block.Halfed) flags |= 1 << 2;
            if (block.NoSkyDetection) flags |= 1 << 3;
            if (block.NoDetection) flags |= 1 << 4;
            if (block.DuringGuard) flags |= 1 << 5;
            if (block.GuardFail) flags |= 1 << 6;
            if (block.DuringReceipt) flags |= 1 << 7;

            writeBytes([flags]);

            writeZeros(1);

            writeBytes([block.Power]);
        }

        private void writeFDBlock(FDBlock block)
        {
            writeInt16(block.X);
            writeInt16(block.Y);
            writeInt16(block.Width);
            writeInt16(block.Height);
            writeBytes([block.Number]);

            byte flags = 0;
            if (block.Collide) flags |= 1 << 0;
            if (block.Damaged) flags |= 1 << 1;
            if (block.Throw) flags |= 1 << 2;
            writeBytes([flags]);
            writeBytes([block.DamageRate]);
        }

        private void writePSBlock(PSBlock block)
        {
            writeBytes([block.PlayerTime]);
            writeBytes([block.EnemyTime]);
        }

        private void writeCBlock(CBlock block)
        {
            byte flags = 0;
            if (block.Hits) flags |= 1 << 0;
            if (block.Uncond) flags |= 1 << 1;
            if (block.SkillCancelCondition) flags |= 1 << 3;
            writeBytes([flags]);

            writeBytes([block.From]);
            writeSkillReference(block.Skill);
            writeBytes([block.To]);
        }

        private void writeVBlock(VBlock block)
        {
            writeSkillBlockReference(block.MultiCondSkill);
            writeBytes([block.Var]);

            byte flags = 0;
            if (block.Replace) flags |= 1 << 0;
            if (block.Add) flags |= 1 << 1;
            if (block.ItsTheSame || block.ItsBelow) flags |= 1 << 2;
            if (block.ItsAbove || block.ItsBelow) flags |= 1 << 3;
            if (block.UseEven) flags |= 1 << 7;

            writeBytes([flags]);

            writeBytes([block.UseEvenVar]); // todo: write by var name ?
            writeInt16(block.Value);

            writeInt16(block.MultiCondValue);
        }

        private void writeRndBlock(RndBlock block)
        {
            writeUInt16(block.RandomNum);
            writeUInt16(block.WhenItsAbove);
            writeZeros(1);
            writeSkillBlockReference(block.Skill);
        }

        private void writeColorBlock(ColorBlock block)
        {
            writeBytes([(byte)block.Option]);
            writeRgba(block.Rgba);
        }

        private void writeComBlock(ComBlock block)
        {
            writeSkillBlockReference(block.Skill);
            writeBytes([block.Time]);
            for (int i = 0; i < 5; i++)
            {
                var step = block.Steps.ElementAt(i);
                writeCommandStep(step);
            }
        }

        protected void writeCommandStep(CommandStep step)
        {
            byte flags1 = 0;
            byte flags2 = 0;

            if (step.A) flags1 |= 0b00010000;
            if (step.B) flags1 |= 0b00100000;
            if (step.C) flags1 |= 0b01000000;
            if (step.D) flags1 |= 0b10000000;

            if (step.E) flags2 |= 0b00000001;
            if (step.F) flags2 |= 0b00000010;
            if (step.Continue) flags2 |= 0b00010000;
            if (step.Active) flags2 |= 0b00100000;

            flags1 |= (byte)step.Direction;
            flags2 |= (byte)(((byte)step.Type) << 6);

            writeBytes([flags1, flags2]);
        }

        private void writeAIBlock(AIBlock block)
        {
            writeZeros(2);

            writeBytes([block.Num]);
            writeBytes([block.Time]);
            writeBytes([(byte)block.Option]);
            writeBytes([(byte)block.FadingType]);
            writeRgba(block.Rgba);
        }

        #endregion

        #region write base
        protected void writeSkillReference(SkillReference skill)
        {
            writeUInt16(skill.Number);  // todo get number from skill reference
        }

        protected void writeSkillBlockReference(SkillBlockReference skill)
        {
            writeUInt16(skill.Number);  // todo get number from skill reference
            writeBytes([skill.Block]);
        }

        protected void writeHitJunctionBlock(SkillReference skillReference)
        {
            writeUInt16(skillReference.Number);
        }

        protected void writeRgba(Rgba rgba)
        {
            writeBytes([rgba.R, rgba.G, rgba.B, rgba.A]);
        }

        private void writeEBShakeBG(EBShakeBg shakeBgX)
        {
            writeBytes([(byte)shakeBgX.Type]);
            writeBytes([shakeBgX.Shake]);
            writeBytes([shakeBgX.Duration]);
        }

        protected void writeInt16(short value)
        {
            _writer.Write(value);
        }

        protected void writeZeros(int count)
        {
            var array = Array.CreateInstance(typeof(byte), count);
            writeBytes((byte[])array);
        }

        private BlockType toBlockType(string type)
        {
            return Enum.Parse<BlockType>(type, true);
        }

        protected void writeUInt16(ushort value)
        {
            _writer.Write(value);
        }

        protected void writeUInt32(uint value)
        {
            _writer.Write(value);
        }

        protected void writeString(string value, int? length = null)
        {
            var bytes = Encoding.GetEncoding(932).GetBytes(value);
            if (length.HasValue)
            {
                Array.Resize(ref bytes, length.Value);
            }
            _writer.Write(bytes);
        }

        protected void writeInt8(sbyte value)
        {
            _writer.Write(value);
        }

        protected void writeBytes(byte[] value)
        {
            _writer.Write(value);
        }
        #endregion
    }
}
