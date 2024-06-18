using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace GSCDecompiler
{
    public static class OPCodeFunction
    {
        public unsafe static string DecodeAsciiZ(this byte[] buffer, int index) //used for dumping the gscs, not needed anymore
        {
            fixed (byte* bytes = &buffer[index])
            {
                return new string((sbyte*)bytes);
            }
        }

        public static int OP_112(BinaryReader reader) //112 is a ref to the powerPC code, means add 1 to the stream position, clrrwi by 1, add 2
        {
            long offset = reader.BaseStream.Position + 1;
            offset = offset & 0xfffffffe;
            reader.BaseStream.Seek(offset, 0);
            return Form1.ShortReverseEndian(reader.ReadInt16());
        }

        public static int OP_324(BinaryReader reader) //324 is a ref to the powerPC code, means add 3 to the stream position, clrrwi by 2, add 4
        {
            long offset = reader.BaseStream.Position + 3;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            return Form1.ReverseEndian(reader.ReadInt32());
        }

        public static float OP_F324(BinaryReader reader) //add 3 to the stream position, clrrwi by 2, add 4, but with a float
        {
            long offset = reader.BaseStream.Position + 3;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            byte[] buffer = new byte[4];
            reader.Read(buffer, 0, 4);
            Array.Reverse(buffer);
            return System.BitConverter.ToSingle(buffer, 0);
        }

        public static int OP_424(BinaryReader reader) //add 4 to the stream position, clrrwi by 2, add 4
        {
            long offset = reader.BaseStream.Position + 4;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            return Form1.ReverseEndian(reader.ReadInt32());
        }

        public static int OP_32C(BinaryReader reader) //add 3 to the stream position, clrrwi by 2, add 4 * 3 for each vector, used by getvector only
        {
            long offset = reader.BaseStream.Position + 3;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            int value = Form1.ReverseEndian(reader.ReadInt32());
            reader.ReadInt32();
            reader.ReadInt32();
            return value;
        }

        public static string OP_SwitchFunction(BinaryReader reader) //code for the switch function
        {
            long origoffset = reader.BaseStream.Position + 3;
            long offset = origoffset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            int switchpos = Form1.ReverseEndian(reader.ReadInt32());
            reader.BaseStream.Seek((offset + switchpos + 7) & 0xfffffffc, 0);
            int switchcount = Form1.ReverseEndian(reader.ReadInt32());
            string sto = "";
            for (int i = 0; i < switchcount; i++)
            {
                reader.ReadInt16();
                int index = Form1.ShortReverseEndian(reader.ReadInt16());
                int codeoffset = Form1.ReverseEndian(reader.ReadInt32());
                sto += index.ToString("X") + ": " + (codeoffset + reader.BaseStream.Position).ToString("X") + ", ";
            }
            reader.BaseStream.Seek(offset + 4, 0);
            return sto;
        }

        public static int OP_EndSwitchFunction(BinaryReader reader) //code for the endswitch function
        {
            long offset = reader.BaseStream.Position + 3;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            int OrigValue = Form1.ReverseEndian(reader.ReadInt32());
            int value = OrigValue << 3;
            offset = reader.BaseStream.Position;
            offset += 3;
            offset = offset & 0xfffffffc;
            offset += value;
            reader.BaseStream.Seek(offset, 0);
            return OrigValue;
        }

        public static string OP_CreateLocalVariableFunction(BinaryReader reader) //code for the CreateLocalVariable Function, struct: variable count, (alligndword), uint var_name_hash, unknown
        {
            string OutputString = "";
            byte LoopCount = reader.ReadByte();
            for (int i = 0; i < LoopCount; i++)
            {
                long offset = reader.BaseStream.Position + 3;
                offset = offset & 0xfffffffc;
                reader.BaseStream.Seek(offset, 0);
                OutputString += ", " + Form1.ReverseEndian(reader.ReadInt32()).ToString("X");
                reader.ReadByte();
            }
            return OutputString;
        }


        public static string OP_GetStringFunction(BinaryReader reader) //code for the OP_Get(i)String Function
        {
            long offset = reader.BaseStream.Position + 1;
            offset = offset & 0xfffffffe;
            reader.BaseStream.Seek(offset, 0);
            int buffer = Form1.ShortReverseEndian(reader.ReadInt16());
            try
            {
                return Form1.StringTableDict[offset];
            }
            catch (KeyNotFoundException)
            {
                if(Form1.DevStringList.Contains(offset))
                    return "Dev strings are not supported";
                else
                    return "0x" + buffer.ToString("X");
            }
        }

        public static int OP_JumpFunction(BinaryReader reader) //code for the jump Function
        {
            long offset = reader.BaseStream.Position + 1;
            offset = offset & 0xfffffffe;
            reader.BaseStream.Seek(offset, 0);
            return Form1.ShortReverseEndian(reader.ReadInt16()) + (int)offset + 2;
        }

        public static string OP_GetFunctionFunction(BinaryReader reader) //code for functions that reference functions, names them based on the guessed function name
        {
            long OPOffset = reader.BaseStream.Position - 1;
            long offset = reader.BaseStream.Position + 4;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            reader.ReadInt32();
            string wer = "";
            try
            {
                wer += Form1.IncludeDict[Form1.FixupDict[OPOffset].Item2 & 0x00000000FFFFFFFF] + "::" + (Form1.FixupDict[OPOffset].Item1 & 0x00000000FFFFFFFF).ToString("X");
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    wer += (Form1.FixupDict[OPOffset].Item2 & 0x00000000FFFFFFFF).ToString("X") + "::" + (Form1.FixupDict[OPOffset].Item1 & 0x00000000FFFFFFFF).ToString("X");
                }
                catch (KeyNotFoundException)
                {
                    wer += "unknown function!";
                }
            }
            return wer;
        }

        public static string OP_GetFunctionFunction3(BinaryReader reader) //literally the exact same as above but adding 3 instead of 4 because op_getfunction is stupid
        {
            long OPOffset = reader.BaseStream.Position - 1;
            long offset = reader.BaseStream.Position + 3;
            offset = offset & 0xfffffffc;
            reader.BaseStream.Seek(offset, 0);
            reader.ReadInt32();
            string wer = "";
            try
            {
                wer += Form1.IncludeDict[Form1.FixupDict[OPOffset].Item2 & 0x00000000FFFFFFFF] + "::" + (Form1.FixupDict[OPOffset].Item1 & 0x00000000FFFFFFFF).ToString("X"); //works for functs in the gsc, not out of it
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    wer += (Form1.FixupDict[OPOffset].Item2 & 0x00000000FFFFFFFF).ToString("X") + "::" + (Form1.FixupDict[OPOffset].Item1 & 0x00000000FFFFFFFF).ToString("X");
                }
                catch(KeyNotFoundException)
                {
                    wer += "unknown function!";
                }
            }
            return wer;
        }

        internal unsafe static Int64 CanonicalHash(string fname) //reversed sl_getcanonicalstring from xex
        {
            string x = fname + "\0";
            fixed (char* c = x)
            {
                Int64 hash = (char.ToLower(*c) ^ 0xE5770569) * 0x1000193;
                for (int z = 1; z < x.Length; z++)
                    hash = (char.ToLower(x[z]) ^ hash) * 0x1000193;
                return hash;
            }
            return 0;
        }
    }
}
