using Fm2ndParser.Blocks;
using Fm2ndParser.Character;
using Fm2ndParser.Common;
using Fm2ndParser.Parsers;
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
    public abstract class BaseCompiler<T> where T : FMFile, new()
    {
        string _filename;
        protected T _fmFile;

        public BaseCompiler(string filename, T fmFile)
        {
            _filename = filename;
            _fmFile = fmFile;
        }
        public void Compile()
        {
            using var stream = File.OpenWrite(_filename);
            using var writer = new BinaryWriter(stream);

            CompileInternal(writer);
        }

        protected virtual void CompileInternal(BinaryWriter writer)
        {
            writeHeader(writer);

            writeName(writer);
            writeSkills(writer);
            writeBlocks(writer);
            writeImages(writer);
            writeGlobalPalettes(writer);
            writeSounds(writer);
        }

        // 16 byte header
        private void writeHeader(BinaryWriter writer)
        {
            writeString(writer, "2DKGT2K");
            writeZeros(writer, 5);
            writeBytes(writer, [0x01]);
            writeZeros(writer, 3);
        }

        // 256 byte name
        private void writeName(BinaryWriter writer)
        {
            writeString(writer, _fmFile.Name, 256);
        }

        private void writeSkills(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.Skills.Count());

            ushort position = 0;
            foreach (var skill in _fmFile.Skills)
            {
                writeString(writer, skill.Name, 32);
                writeUInt16(writer, position);
                writeZeros(writer, 1);
                writeUInt32(writer, skill.Type);
                position += (ushort)skill.Blocks.Count();
            }
        }
        private void writeBlocks(BinaryWriter writer)
        {
            var blocksCount = (uint)_fmFile.Skills.SelectMany(x => x.Blocks).Count();
            writeUInt32(writer, blocksCount);
            foreach (var skill in _fmFile.Skills)
            {
                foreach (var block in skill.Blocks)
                {
                    var startPosition = writer.BaseStream.Position;

                    writeBytes(writer, [(byte)toBlockType(block.Type)]);
                    switch (toBlockType(block.Type))
                    {
                        case BlockType.Settings:
                            writeSettiningsBlock(writer, (SettingsBlock)block);
                            break;
                        case BlockType.M:
                            writeMBlock(writer, (MBlock)block);
                            break;
                        case BlockType.DS:
                            writeDSBlock(writer, (DSBlock)block);
                            break;
                        case BlockType.S:
                            writeSBlock(writer, (SBlock)block);
                            break;
                        case BlockType.O:
                            writeOBlock(writer, (OBlock)block);
                            break;
                        case BlockType.E:
                            writeEBlock(writer, (EBlock)block);
                            break;
                        case BlockType.RC:
                            writeRCBlock(writer, (RCBlock)block);
                            break;
                        case BlockType.SF:
                            writeSFBlock(writer, (SFBlock)block);
                            break;
                        case BlockType.SG:
                            writeSGBlock(writer, (SGBlock)block);
                            break;
                        case BlockType.SC:
                            writeSCBlock(writer, (SCBlock)block);
                            break;
                        case BlockType.I:
                            writeIBlock(writer, (IBlock)block);
                            break;
                        case BlockType.EB:
                            writeEBBlock(writer, (EBBlock)block);
                            break;
                        case BlockType.GS:
                            writeGSBlock(writer, (GSBlock)block);
                            break;
                        case BlockType.GL:
                            writeGLBlock(writer, (GLBlock)block);
                            break;
                        case BlockType.RP:
                            writeRPBlock(writer, (RPBlock)block);
                            break;
                        case BlockType.GC:
                            writeGCBlock(writer, (GCBlock)block);
                            break;
                        case BlockType.R:
                            writeRBlock(writer, (RBlock)block);
                            break;
                        case BlockType.FA:
                            writeFABlock(writer, (FABlock)block);
                            break;
                        case BlockType.FD:
                            writeFDBlock(writer, (FDBlock)block);
                            break;
                        case BlockType.PS:
                            writePSBlock(writer, (PSBlock)block);
                            break;
                        case BlockType.C:
                            writeCBlock(writer, (CBlock)block);
                            break;
                        case BlockType.V:
                            writeVBlock(writer, (VBlock)block);
                            break;
                        case BlockType.Rnd:
                            writeRndBlock(writer, (RndBlock)block);
                            break;
                        case BlockType.COLOR:
                            writeColorBlock(writer, (ColorBlock)block);
                            break;
                        case BlockType.COM:
                            writeComBlock(writer, (ComBlock)block);
                            break;
                        case BlockType.AI:
                            writeAIBlock(writer, (AIBlock)block);
                            break;
                    }

                    var byteCount = writer.BaseStream.Position - startPosition;
                    Debug.Assert(byteCount == 16);
                }
            }
        }

        private void writeImages(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.Images.Count());
            foreach (var image in _fmFile.Images)
            {
                writeBytes(writer, image.Pointer); // 4 bytes
                writeUInt32(writer, image.Width);
                writeUInt32(writer, image.Height);
                writeUInt32(writer, (uint)image.PaletteType);
                writeUInt32(writer, image.PackedSize);

                var paletteSize = (image.PaletteType == PaletteType.Private ? 0x400u : 0u);
                var rawSize = checked((int)(image.Width * image.Height + paletteSize));

                // todo: replace with image
                if (image.PackedSize >= 0)
                {
                    if (image.PackedData == null || image.PackedData.Length == 0)
                    {
                        var packedData = ImageCompression.Compress(image.Data);
                        writeBytes(writer, packedData);
                    }
                    else
                    {
                        writeBytes(writer, image.PackedData);
                    }
                }
                else
                    writeBytes(writer, image.Data);
            }
        }

        private void writeGlobalPalettes(BinaryWriter writer)
        {
            foreach (var palette in _fmFile.GlobalPalettes)
            {
                var data = ParseCommand.ToFM2kPalette(palette.Colors);
                writeBytes(writer, data);
            }
        }

        private void writeSounds(BinaryWriter writer)
        {
            writeUInt32(writer, (uint)_fmFile.Sounds.Count());
            foreach (var sound in _fmFile.Sounds)
            {
                writeBytes(writer, sound.Pointer); // 4 bytes
                writeString(writer, sound.Name, 0x20);
                writeUInt32(writer, sound.Size);

                byte flags = 0;

                if (sound.EndlessLoop) flags |= 1 << 5;
                flags |= (byte)((byte)sound.Type & 0b00000011);
                writeBytes(writer, [flags]);

                writeBytes(writer, [sound.CDDATrack]);

                writeBytes(writer, sound.Data);
            }
        }

        #region Write blocks
        protected void writeSettiningsBlock(BinaryWriter writer, SettingsBlock block)
        {
            switch (block.SettingsType)
            {
                case SettingsType.None:
                    writeZeros(writer, 15);
                    break;
                case SettingsType.HitMark:
                    writeBytes(writer, [(byte)block.Position]);
                    writeBytes(writer, [block.NumberWidth]);
                    writeZeros(writer, 13);
                    break;
                case SettingsType.Time:
                    writeUInt32(writer, block.Time);
                    writeZeros(writer, 11);
                    break;
                case SettingsType.Position:
                    writeInt16(writer, block.X);
                    writeInt16(writer, block.Y);
                    writeBytes(writer, [(byte)block.Width]);
                    writeZeros(writer, 10);
                    break;
                case SettingsType.MarkPosition:
                    writeInt16(writer, block.X);
                    writeInt16(writer, block.Y);
                    writeInt8(writer, (sbyte)block.Width);
                    writeInt8(writer, (sbyte)block.Height);
                    writeZeros(writer, 9);
                    break;
                case SettingsType.Character:
                    writeZeros(writer, 1);
                    writeBytes(writer, [(byte)block.Level]);
                    writeZeros(writer, 13);
                    break;
                case SettingsType.Stage:
                    byte flags = 0;
                    if (block.ConnectLtRt) flags |= 1 << 1;
                    if (block.ConnectUpDw) flags |= 1 << 2;
                    if (block.WidthEnabled) flags |= 1 << 3;
                    if (block.HeightEnabled) flags |= 1 << 4;
                    writeBytes(writer, [flags]);

                    writeInt16(writer, block.Width);
                    writeInt16(writer, block.Height);

                    writeZeros(writer, 10);
                    break;
                default:
                    throw new Exception("Unknown SettingsType: " + block.SettingsType);
            }
        }

        private void writeMBlock(BinaryWriter writer, MBlock block)
        {
            writeInt16(writer, block.GravityX);
            writeInt16(writer, block.MoveX);
            writeInt16(writer, block.MoveY);
            writeInt16(writer, block.GravityY);

            byte flags = 0;

            if (block.Add) flags |= 1 << 0;
            if (block.StopMoveX) flags |= 1 << 1;
            if (block.StopMoveY) flags |= 1 << 2;
            if (block.StopGravityX) flags |= 1 << 3;
            if (block.StopGravityY) flags |= 1 << 4;

            writeBytes(writer, [flags]);

            writeZeros(writer, 6);
        }

        private void writeDSBlock(BinaryWriter writer, DSBlock block)
        {
            writeBytes(writer, [(byte)block.When]);
            writeSkillBlockReference(writer, block.Skill);
            writeZeros(writer, 11);
        }
        private void writeSBlock(BinaryWriter writer, SBlock block)
        {
            writeZeros(writer, 1);  // todo write unknown byte
            writeUInt16(writer, block.Sound.Number);    // todo get number from sound reference
            writeZeros(writer, 12);
        }

        private void writeOBlock(BinaryWriter writer, OBlock block)
        {
            byte flags = 0;
            if (block.Out) flags |= 1 << 0;
            if (block.Point) flags |= 1 << 1;
            if (block.UnCond) flags |= 1 << 2;
            if (block.Shadow) flags |= 1 << 3;
            if (block.Parent) flags |= 1 << 5;
            if (block.PicXY) flags |= 1 << 6;

            writeBytes(writer, new byte[] { flags });

            writeSkillBlockReference(writer, block.Skill);
            writeSkillBlockReference(writer, block.OutSkill);
            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);
            writeBytes(writer, [block.Number]);
            writeBytes(writer, [block.Depth]);
            writeZeros(writer, 2);
        }

        private void writeEBlock(BinaryWriter writer, EBlock block)
        {
            writeZeros(writer, 15);
        }

        private void writeRCBlock(BinaryWriter writer, RCBlock block)
        {
            byte flags = 0;
            if (block.In) flags |= 1 << 0;
            if (block.TurnX) flags |= 1 << 2;
            if (block.TurnY) flags |= 1 << 3;
            if (block.Same) flags |= 1 << 4;
            writeBytes(writer, new byte[] { flags });

            writeUInt16(writer, block.CommonImage.Number);
            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);

            writeZeros(writer, 8);
        }

        private void writeSFBlock(BinaryWriter writer, SFBlock block)
        {
            writeBytes(writer, [block.Loop]);
            writeSkillBlockReference(writer, block.Skill);
            writeZeros(writer, 11);
        }

        private void writeSGBlock(BinaryWriter writer, SGBlock block)
        {
            writeSkillBlockReference(writer, block.Skill);
            writeZeros(writer, 12);
        }

        private void writeSCBlock(BinaryWriter writer, SCBlock block)
        {
            writeSkillBlockReference(writer, block.Skill);
            writeZeros(writer, 12);
        }

        private void writeIBlock(BinaryWriter writer, IBlock block)
        {
            writeUInt16(writer, block.Wait);
            byte flags = 0;

            if (block.TurnX) { flags |= 1 << 6; }
            if (block.TurnY) { flags |= 1 << 7; }

            writeBytes(writer, setSplittedData(flags, block.I));

            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);

            byte ignoreDirection = block.IgnoreDirection ? (byte)0x01 : (byte)0x00;

            writeBytes(writer, [ignoreDirection]);

            writeZeros(writer, 6);
        }

        protected byte[] setSplittedData(byte flags, ushort value)
        {
            var iMask = (byte)BaseParser.CreateBitMask(0, 5);

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

        private void writeEBBlock(BinaryWriter writer, EBBlock block)
        {
            writeBytes(writer, [(byte)block.FadingType]);
            writeRgba(writer, block.Rgba);
            writeUInt16(writer, block.Duration);

            byte flags = 0;

            if (block.Player) flags |= 1 << 0;
            if (block.Enemy) flags |= 1 << 1;
            if (block.BG) flags |= 1 << 2;
            if (block.System) flags |= 1 << 3;
            writeBytes(writer, [flags]);

            writeEBShakeBG(writer, block.ShakeBgX);
            writeEBShakeBG(writer, block.ShakeBgY);
            writeZeros(writer, 1);
        }


        private void writeGSBlock(BinaryWriter writer, GSBlock block)
        {
            writeZeros(writer, 1);

            writeSkillBlockReference(writer, block.Skill);
            writeBytes(writer, [block.IsMore ? (byte)1 : (byte)0]);
            writeBytes(writer, [block.Level]);
            writeInt16(writer, block.Add);

            writeZeros(writer, 7);
        }

        private void writeGLBlock(BinaryWriter writer, GLBlock block)
        {
            writeZeros(writer, 1);

            writeSkillBlockReference(writer, block.Skill);
            writeBytes(writer, [block.IsMore ? (byte)1 : (byte)0]);
            writeInt16(writer, block.Add);

            writeZeros(writer, 8);
        }

        private void writeRPBlock(BinaryWriter writer, RPBlock block)
        {
            byte flags = 0;

            if (block.In) flags |= 1 << 0;
            if (block.TurnX) flags |= 1 << 2;
            writeBytes(writer, [flags]);

            writeHitJunctionBlock(writer, block.HitJunction);

            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);
            writeZeros(writer, 8);
        }

        private void writeGCBlock(BinaryWriter writer, GCBlock block)
        {
            writeZeros(writer, 1);

            writeInt16(writer, block.PlayerLifeGauge);
            writeInt16(writer, block.PlayerSpecialGauge);
            writeInt16(writer, block.EnemyLifeGauge);
            writeInt16(writer, block.EnemySpecialGauge);

            writeZeros(writer, 6);
        }

        private void writeRBlock(BinaryWriter writer, RBlock block)
        {
            writeHitJunctionBlock(writer, block.HitsStand);
            writeHitJunctionBlock(writer, block.HitsCrouched);
            writeHitJunctionBlock(writer, block.HitsAir);
            writeHitJunctionBlock(writer, block.GuardStand);
            writeHitJunctionBlock(writer, block.GuardCrouched);
            writeHitJunctionBlock(writer, block.GuardAir);

            writeZeros(writer, 3);
        }

        private void writeFABlock(BinaryWriter writer, FABlock block)
        {
            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);

            writeInt16(writer, block.Width);
            writeInt16(writer, block.Height);
            writeBytes(writer, [block.Number]);

            byte flags = 0;

            if (block.Cancel) flags |= 1 << 0;
            if (block.Combo) flags |= 1 << 1;
            if (block.Halfed) flags |= 1 << 2;
            if (block.NoSkyDetection) flags |= 1 << 3;
            if (block.NoDetection) flags |= 1 << 4;
            if (block.DuringGuard) flags |= 1 << 5;
            if (block.GuardFail) flags |= 1 << 6;
            if (block.DuringReceipt) flags |= 1 << 7;

            writeBytes(writer, [flags]);

            writeZeros(writer, 1);

            writeBytes(writer, [block.Power]);
            writeZeros(writer, 3);
        }

        private void writeFDBlock(BinaryWriter writer, FDBlock block)
        {
            writeInt16(writer, block.X);
            writeInt16(writer, block.Y);
            writeInt16(writer, block.Width);
            writeInt16(writer, block.Height);
            writeBytes(writer, [block.Number]);

            byte flags = 0;
            if (block.Collide) flags |= 1 << 0;
            if (block.Damaged) flags |= 1 << 1;
            if (block.Throw) flags |= 1 << 2;
            writeBytes(writer, [flags]);
            writeBytes(writer, [block.DamageRate]);

            writeZeros(writer, 4);
        }

        private void writePSBlock(BinaryWriter writer, PSBlock block)
        {
            writeBytes(writer, [block.PlayerTime]);
            writeBytes(writer, [block.EnemyTime]);

            writeZeros(writer, 13);
        }

        private void writeCBlock(BinaryWriter writer, CBlock block)
        {
            byte flags = 0;
            if (block.Hits) flags |= 1 << 0;
            if (block.Uncond) flags |= 1 << 1;
            if (block.SkillCancelCondition) flags |= 1 << 3;
            writeBytes(writer, [flags]);

            writeBytes(writer, [block.From]);
            writeSkillReference(writer, block.Skill);
            writeBytes(writer, [block.To]);

            writeZeros(writer, 10);
        }

        private void writeVBlock(BinaryWriter writer, VBlock block)
        {
            writeSkillBlockReference(writer, block.MultiCondSkill);
            writeBytes(writer, [block.Var]);

            byte flags = 0;
            if (block.Replace) flags |= 1 << 0;
            if (block.Add) flags |= 1 << 1;
            if (block.ItsTheSame || block.ItsBelow) flags |= 1 << 2;
            if (block.ItsAbove || block.ItsBelow) flags |= 1 << 3;
            if (block.UseEven) flags |= 1 << 7;

            writeBytes(writer, [flags]);

            writeBytes(writer, [block.UseEvenVar]); // todo: write by var name ?
            writeInt16(writer, block.Value);

            writeInt16(writer, block.MultiCondValue);

            writeZeros(writer, 5);
        }

        private void writeRndBlock(BinaryWriter writer, RndBlock block)
        {
            writeUInt16(writer, block.RandomNum);
            writeUInt16(writer, block.WhenItsAbove);
            writeZeros(writer, 1);
            writeSkillBlockReference(writer, block.Skill);
            writeZeros(writer, 7);
        }

        private void writeColorBlock(BinaryWriter writer, ColorBlock block)
        {
            writeBytes(writer, [(byte)block.Option]);
            writeRgba(writer, block.Rgba);
            writeZeros(writer, 10);
        }

        private void writeComBlock(BinaryWriter writer, ComBlock block)
        {
            writeSkillBlockReference(writer, block.Skill);
            writeBytes(writer, [block.Time]);
            for (int i = 0; i < 5; i++)
            {
                var step = block.Steps.ElementAt(i);
                writeCommandStep(writer, step);
            }

            writeZeros(writer, 1);
        }

        protected void writeCommandStep(BinaryWriter writer, CommandStep step)
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

            writeBytes(writer, [flags1, flags2]);
        }

        private void writeAIBlock(BinaryWriter writer, AIBlock block)
        {
            writeZeros(writer, 2);

            writeBytes(writer, [block.Num]);
            writeBytes(writer, [block.Time]);
            writeBytes(writer, [(byte)block.Option]);
            writeBytes(writer, [(byte)block.FadingType]);
            writeRgba(writer, block.Rgba);

            writeZeros(writer, 5);
        }

        #endregion

        #region write base
        protected void writeSkillReference(BinaryWriter writer, SkillReference skill)
        {
            writeUInt16(writer, skill.Number);  // todo get number from skill reference
        }

        protected void writeSkillBlockReference(BinaryWriter writer, SkillBlockReference skill)
        {
            writeUInt16(writer, skill.Number);  // todo get number from skill reference
            writeBytes(writer, [skill.Block]);
        }

        protected void writeHitJunctionBlock(BinaryWriter writer, SkillReference skillReference)
        {
            writeUInt16(writer, skillReference.Number);
        }

        protected void writeRgba(BinaryWriter writer, Rgba rgba)
        {
            writeBytes(writer, [rgba.R, rgba.G, rgba.B, rgba.A]);
        }

        private void writeEBShakeBG(BinaryWriter writer, EBShakeBg shakeBgX)
        {
            writeBytes(writer, [(byte)shakeBgX.Type]);
            writeBytes(writer, [shakeBgX.Shake]);
            writeBytes(writer, [shakeBgX.Duration]);
        }

        protected void writeInt16(BinaryWriter writer, short value)
        {
            writer.Write(value);
        }

        protected void writeZeros(BinaryWriter writer, int count)
        {
            var array = Array.CreateInstance(typeof(byte), count);
            writeBytes(writer, (byte[])array);
        }

        private BlockType toBlockType(string type)
        {
            return Enum.Parse<BlockType>(type, true);
        }

        protected void writeUInt16(BinaryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        protected void writeUInt32(BinaryWriter writer, uint value)
        {
            writer.Write(value);
        }

        protected void writeString(BinaryWriter writer, string value, int? length = null)
        {
            var bytes = Encoding.GetEncoding(932).GetBytes(value);
            if (length.HasValue)
            {
                Array.Resize(ref bytes, length.Value);
            }
            writer.Write(bytes);
        }

        protected void writeInt8(BinaryWriter writer, sbyte value)
        {
            writer.Write(value);
        }

        protected void writeBytes(BinaryWriter writer, byte[] value)
        {
            writer.Write(value);
        }
        #endregion
    }
}
