using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
//using JRPC_Client;
//using XDevkitPlusPlus;
//using XRPCLib;
//using XDevkit;

namespace GSCDecompiler
{
    public partial class Form1 : Form
    {
        //XRPC XRPC = new XRPC();
        Dictionary<int, int> OPCodeDict;
        static public Dictionary<long, int> FunctionDict; //function start, hashed name
        static public Dictionary<long, string> StringTableDict; //stringreference offset, actual string
        static public Dictionary<long, Tuple<long, long>> FixupDict; //getfunction psoition, FuncName hash, GSCFile Name hash
        static public Dictionary<long, string> IncludeDict; //gsc file name hash, gsc file name
        static public List<long> DevStringList;
        int CodeStart = 0;
        int ExportsOffset = 0;
        StringBuilder CurrentOffsetText = new StringBuilder("");
        public Form1()
        {
            InitializeComponent();
            InitDictionary();
        }

        string SeperateNameFromPath(string Path) //goes from scripts/shared/blah.gsc to blah.gsc
        {
            if (Path.Length == 1)
                return Path;
            var FullStr = new List<char>();
            for (int i = 0; i < Path.Length - 1; i++)
            {
                char c = Path[Path.Length - 1 - i];
                if (c == '/') break;
                FullStr.Add(c);
            }
            var buffer = FullStr.ToArray();
            Array.Reverse(buffer);
            return new string(buffer);
        }

        public static void AppendToOffset(long number)
        {
            var form = Form.ActiveForm as Form1; //damn c# static class
            form.CurrentOffsetText.Append(number.ToString("X") + System.Environment.NewLine);
        }

        string ReadNullTerminatedString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            for (; ; )
            {
                byte b = (byte)reader.ReadByte();
                if (b == 0) break;
                bytes.Add(b);
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        string CreateCanonicalFilename(string input) //used to guess what gsc file is being called, just removes the _shared at the end and _ at the start wich is what most gsc canoncial names have done
        {
            if (input == "")
                return "null string";
            string JustName = SeperateNameFromPath(input);
            if (JustName.EndsWith("_shared") == true)
                JustName = JustName.Substring(0, JustName.Length - 7);
            if (JustName[0] == '_')
                JustName = JustName.Substring(1, JustName.Length - 1);
            return JustName;
        }

        public static int ShortReverseEndian(short value)
        {
            return ((int)((int)((value << 24) & 0xff000000) | ((value << 8) & 0xff0000)) >> 16) & 0xffff;
        }

        public static int ReverseEndian(int value)
        {
            return (int)((int)((value << 24) & 0xff000000) | ((value << 8) & 0xff0000) | ((value >> 8) & 0xff00) | ((value >> 24) & 0xff));
        }

        private void GetHeaderData_Click(object sender, EventArgs e) //check opcodedefs.cs for what all the structs are, might help you
        {
            StringBuilder HeaderDataText = new StringBuilder("");
            StringBuilder IncludesText = new StringBuilder("");
            StringBuilder StringText = new StringBuilder("");
            StringBuilder FunctionText = new StringBuilder("");
            HeaderDataTextOutput.Text = "";
            IncludesTextOutput.Text = "";
            StringTextOutput.Text = "";
            FunctionTextOutput.Text = "";
            NameHash.Text = "";

            IncludeDict = new Dictionary<long, string>();
            StringTableDict = new Dictionary<long, string>();
            FixupDict = new Dictionary<long, Tuple<long, long>>();

            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                BinaryReader reader = new BinaryReader(File.Open(openFileDialog1.FileName, FileMode.Open));
                int Magic1 = ReverseEndian(reader.ReadInt32());                        //0x00
                int Magic2 = ReverseEndian(reader.ReadInt32());                        //0x04
                if (Magic1 - Magic2 == 1933398848)
                {
                    int CRC = ReverseEndian(reader.ReadInt32());                       //0x08
                    int IncludeOffset = ReverseEndian(reader.ReadInt32());             //0x0C
                    int AnimtreeOffset = ReverseEndian(reader.ReadInt32());            //0x10 i can't bothered reversing animtrees, of the couple that have animtrees they don't even ref anything or just ref to op_getint
                    CodeStart = ReverseEndian(reader.ReadInt32());                     //0x14
                    int StringtableOffset = ReverseEndian(reader.ReadInt32());         //0x18
                    int DevStringOffset = ReverseEndian(reader.ReadInt32());           //0x1C
                    ExportsOffset = ReverseEndian(reader.ReadInt32());                 //0x20
                    int ImportsOffset = ReverseEndian(reader.ReadInt32());             //0x24
                    int FixupOffset = ReverseEndian(reader.ReadInt32());             //0x28
                    int ProfileOffset = ReverseEndian(reader.ReadInt32());                 //0x2C
                    int CodeSize = ReverseEndian(reader.ReadInt32());                    //0x30
                    int NameOffset = ShortReverseEndian(reader.ReadInt16());           //0x34
                    int StringCount = ShortReverseEndian(reader.ReadInt16());          //0x36
                    int ExportCount = ShortReverseEndian(reader.ReadInt16());          //0x38
                    int ImportsCount = ShortReverseEndian(reader.ReadInt16());         //0x3A
                    int FixupCount = ShortReverseEndian(reader.ReadInt16());         //0x3C
                    int ProfileCount = ShortReverseEndian(reader.ReadInt16());         //0x3E both this value and 0x30 isn't referenced at all during linking in the xex, so i'd assume they don't do anything
                    int DevStringCount = ShortReverseEndian(reader.ReadInt16());       //0x40
                    byte IncludeCount = reader.ReadByte();                             //0x42
                    byte AnimtreeCount = reader.ReadByte();                            //0x43 
                    HeaderDataText.Append("Magic: " + Magic1.ToString("X") + Magic2.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("CRC: " + CRC.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Include Offset: " + IncludeOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Animtree Offset: " + AnimtreeOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Code Start: " + CodeStart.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Stringtable Offset: " + StringtableOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Dev String Offset: " + DevStringOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Exports Offset: " + ExportsOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Imports Offset: " + ImportsOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Fixup Offset: " + FixupOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Profile Offset: " + ProfileOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Code Size: " + CodeSize.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Name Offset: " + NameOffset.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("String Count: " + StringCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Export Count: " + ExportCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Import Count: " + ImportsCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Fixup Count: " + FixupCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Profile Count: " + ProfileCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Dev String Count: " + DevStringCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Include Count: " + IncludeCount.ToString("X") + System.Environment.NewLine);
                    HeaderDataText.Append("Animtree Count: " + AnimtreeCount.ToString("X") + System.Environment.NewLine);

                    //get includes
                    if (IncludeCount != 0)
                    {
                        for (int i = 0; i < IncludeCount; i++)
                        {
                            reader.BaseStream.Seek(IncludeOffset + (i * 4), 0);
                            int offset = ReverseEndian(reader.ReadInt32());
                            reader.BaseStream.Seek(offset, 0);
                            string text = ReadNullTerminatedString(reader).Replace(@"\", "/");
                            try
                            {
                                IncludeDict.Add(OPCodeFunction.CanonicalHash(CreateCanonicalFilename(text)) & 0x00000000FFFFFFFF, text);
                            }
                            catch (System.ArgumentException)
                            {

                            }
                            IncludesText.Append(text + System.Environment.NewLine);
                        }
                    }

                    //get name
                    reader.BaseStream.Seek(NameOffset, 0);
                    string GSCNameString = ReadNullTerminatedString(reader);
                    string SubGSCNameString = GSCNameString.Substring(0, GSCNameString.Length - 4);
                    try
                    {
                        IncludeDict.Add(OPCodeFunction.CanonicalHash(CreateCanonicalFilename(SubGSCNameString)) & 0x00000000FFFFFFFF, SubGSCNameString);
                    }
                    catch (System.ArgumentException)
                    {

                    }
                    GSCName.Text = GSCNameString;

                    //get Functions (exports)
                    FunctionDict = new Dictionary<long, int>();
                    if (ExportCount != 0)
                    {
                        for (int i = 0; i < ExportCount; i++)
                        {
                            try
                            {
                                reader.BaseStream.Seek(ExportsOffset + (i * 20), 0);
                                reader.ReadInt32();
                                int FuncStart = ReverseEndian(reader.ReadInt32());
                                int Name = ReverseEndian(reader.ReadInt32());
                                reader.ReadInt32();
                                byte Params = reader.ReadByte();
                                byte flags = reader.ReadByte();
                                string rp = ", ";
                                if (flags == 0x02)
                                    FunctionText.Append(FuncStart.ToString("X") + rp + Name.ToString("X") + rp + Params.ToString("X") + rp + "autoexec" + System.Environment.NewLine);
                                else
                                    FunctionText.Append(FuncStart.ToString("X") + rp + Name.ToString("X") + rp + Params.ToString("X") + System.Environment.NewLine);

                                FunctionDict.Add(FuncStart, Name);
                            }
                            catch (EndOfStreamException)
                            {
                                FunctionText.Append("EndOfStreamException");
                            }
                        }
                        reader.BaseStream.Seek(ExportsOffset + 0xC, 0);
                        NameHash.AppendText(ReverseEndian(reader.ReadInt32()).ToString("X")); //get the name hash of the gsc
                    }

                    //stringtable read
                    if (StringCount != 0)
                    {
                        byte count = 0x00;
                        reader.BaseStream.Seek(StringtableOffset, 0);
                        for (int i = 0; i < StringCount; i++)
                        {
                            int offset = ShortReverseEndian(reader.ReadInt16());
                            count = reader.ReadByte();
                            reader.ReadByte();
                            long CurrentPos = reader.BaseStream.Position;
                            reader.BaseStream.Seek(offset, 0);
                            string text = ReadNullTerminatedString(reader);
                            reader.BaseStream.Seek(CurrentPos, 0);
                            for (int ii = 0; ii < count; ii++)
                            {
                                int strpos = ReverseEndian(reader.ReadInt32());
                                StringTableDict.Add(strpos, text);
                            }
                            StringText.Append(text + System.Environment.NewLine);
                        }
                    }

                    //imports read
                    if (ImportsCount != 0)
                    {
                        int REFcount = 0;
                        reader.BaseStream.Seek(ImportsOffset, 0);
                        Tuple<long, long> combined;
                        for (int i = 0; i < ImportsCount; i++)
                        {
                            long hashname = ReverseEndian(reader.ReadInt32());
                            long GSCFilename = ReverseEndian(reader.ReadInt32());
                            combined = new Tuple<long, long>(hashname, GSCFilename);
                            REFcount = ShortReverseEndian(reader.ReadInt16());
                            byte numParams = reader.ReadByte();
                            byte flags = reader.ReadByte();
                            for (int ii = 0; ii < REFcount; ii++)
                            {
                                int HashRef = ReverseEndian(reader.ReadInt32());
                                FixupDict.Add(HashRef, combined);
                            }
                            string rp = ", ";
                            FunctionText.Append((hashname & 0xFFFFFFFF).ToString("X") + rp + flags.ToString("X") + System.Environment.NewLine);
                        }
                    }

                    //DevStringRead
                    DevStringList = new List<long>();
                    if (DevStringCount != 0)
                    {
                        reader.BaseStream.Seek(DevStringOffset, 0);
                        for (int i = 0; i < DevStringCount; i++)
                        {
                            reader.ReadInt16();
                            byte count = reader.ReadByte();
                            reader.ReadByte();
                            for (int ii = 0; ii < count; ii++)
                            {
                                int DevRef = ReverseEndian(reader.ReadInt32());
                                DevStringList.Add(DevRef);
                            }
                        }
                    }
                    HeaderDataTextOutput.Text = HeaderDataText.ToString();
                    IncludesTextOutput.Text = IncludesText.ToString();
                    StringTextOutput.Text = StringText.ToString();
                    FunctionTextOutput.Text = FunctionText.ToString();


                    reader.Close();
                }
                else
                    MessageBox.Show("Incorrect Magic!");
            }
            else
                MessageBox.Show("No file selected!");
        }

        private void DissasembleButton_Click(object sender, EventArgs e)
        {
            StringBuilder DecompiledText = new StringBuilder("");
            CurrentOffsetText = new StringBuilder("");
            BinaryReader reader = new BinaryReader(File.Open(openFileDialog1.FileName, FileMode.Open));
            reader.BaseStream.Seek(((CodeStart + 3) & 0xfffffffc) + 4, 0);
            byte OPCode = 0x01;
            int ParamAmount = 0;
            int IntValue = 0;
            byte ByteValue = 0;
            float FloatValue = 0;
            string StringValue = "";
            int OPCodeFunc = 0;
            DecompiledText.Append("Funct_" + FunctionDict[reader.BaseStream.Position].ToString("X") + System.Environment.NewLine);
            CurrentOffsetText.AppendLine();
            try
            {
                while (true)
                {
                    Form1.AppendToOffset(reader.BaseStream.Position);
                    if (reader.BaseStream.Position >= ExportsOffset)
                    {
                        Form1.AppendToOffset(reader.BaseStream.Position);
                        break;
                    }
                    OPCode = reader.ReadByte();
                    OPCodeFunc = OPCodeDict[OPCode];
                    switch (OPCodeFunc)
                    {
                        case (int)OPCodeNames.OP_Invalid:
                            DecompiledText.Append("OP_Invalid()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ClearParams:
                            DecompiledText.Append("OP_ClearParams()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_End:
                            int vbalue = 0;
                            long newpos = ((reader.BaseStream.Position + 3) & 0xfffffffc) + 4;
                            if (!FunctionDict.TryGetValue(newpos, out vbalue))
                            {
                                DecompiledText.Append("OP_End()" + System.Environment.NewLine);
                                break;
                            }
                            reader.BaseStream.Seek(newpos, 0);
                            CurrentOffsetText.Append(System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine);
                            DecompiledText.Append("OP_End()" + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Funct_" + vbalue.ToString("X") + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CallBuiltin:
                            ParamAmount = reader.PeekChar();
                            if (RenameBuiltinCheckbox.Checked == true)
                            {
                                StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                                DecompiledText.Append("OP_CallBuiltin(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            }
                            else
                            {
                                IntValue = OPCodeFunction.OP_424(reader);
                                DecompiledText.Append("OP_CallBuiltin(" + ParamAmount + ", " + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            }

                            break;

                        case (int)OPCodeNames.OP_EvalLocalVariableRef:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalLocalVariableRef(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_PreScriptCall:
                            DecompiledText.Append("OP_PreScriptCall()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CheckClearParams:
                            DecompiledText.Append("OP_CheckClearParams()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalLocalVariable:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalLocalVariable(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetUIntPointer:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetUIntPointer(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_BoolNot:
                            DecompiledText.Append("OP_BoolNot()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CreateLocalVariable:
                            ParamAmount = reader.PeekChar();
                            StringValue = OPCodeFunction.OP_CreateLocalVariableFunction(reader);
                            DecompiledText.Append("OP_CreateLocalVariable(" + ParamAmount + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_JumpOnTrue:
                            IntValue = OPCodeFunction.OP_JumpFunction(reader);
                            DecompiledText.Append("OP_JumpOnTrue(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetSelfObject:
                            DecompiledText.Append("OP_GetSelfObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Inc:
                            DecompiledText.Append("OP_Inc()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetByte:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_GetByte(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Bit_And:
                            DecompiledText.Append("OP_Bit_And()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_WaitRealTime:
                            DecompiledText.Append("OP_WaitRealTime()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptThreadCallPointer:
                            DecompiledText.Append("OP_ScriptThreadCallPointer()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetString:
                            StringValue = OPCodeFunction.OP_GetStringFunction(reader);
                            DecompiledText.Append("OP_GetString(" + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetIString:
                            StringValue = OPCodeFunction.OP_GetStringFunction(reader);
                            DecompiledText.Append("OP_GetIString(" + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetWorld:
                            DecompiledText.Append("OP_GetWorld()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetZero:
                            DecompiledText.Append("OP_GetZero()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ShiftLeft:
                            DecompiledText.Append("OP_ShiftLeft()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetGame:
                            DecompiledText.Append("OP_GetGame()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SuperNotEqual:
                            DecompiledText.Append("OP_SuperNotEqual()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_JumpOnFalseExpr:
                            IntValue = OPCodeFunction.OP_JumpFunction(reader);
                            DecompiledText.Append("OP_JumpOnFalseExpr(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptFunctionCallPointer:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_ScriptFunctionCallPointer(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ShiftRight:
                            DecompiledText.Append("OP_ShiftRight()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetGameRef:
                            DecompiledText.Append("OP_GetGameRef()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ClassFunctionCall:
                            IntValue = OPCodeFunction.OP_424(reader);
                            DecompiledText.Append("OP_ClassFunctionCall(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetFunction:
                            StringValue = OPCodeFunction.OP_GetFunctionFunction3(reader);
                            DecompiledText.Append("OP_GetFunction(" + StringValue + ")" + System.Environment.NewLine);
                            break;
                        case (int)OPCodeNames.OP_EndSwitch:
                            IntValue = OPCodeFunction.OP_EndSwitchFunction(reader);
                            DecompiledText.Append("OP_EndSwitch(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ClassFunctionThreadCall:
                            IntValue = OPCodeFunction.OP_424(reader);
                            DecompiledText.Append("OP_ClassFunctionThreadCall(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetClassesObject:
                            DecompiledText.Append("OP_GetClassesObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_FirstArrayKey:
                            DecompiledText.Append("OP_FirstArrayKey()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Return:
                            int vvbalue = 0;
                            long nnewpos = ((reader.BaseStream.Position + 3) & 0xfffffffc) + 4;
                            if (!FunctionDict.TryGetValue(nnewpos, out vvbalue))
                            {
                                DecompiledText.Append("OP_Return()" + System.Environment.NewLine);
                                break;
                            }
                            reader.BaseStream.Seek(nnewpos, 0);
                            CurrentOffsetText.Append(System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine);
                            DecompiledText.Append("OP_Return()" + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + "Funct_" + vvbalue.ToString("X") + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Plus:
                            DecompiledText.Append("OP_Plus()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptFunctionCall:
                            ParamAmount = reader.PeekChar();
                            StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                            DecompiledText.Append("OP_ScriptFunctionCall(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_IsDefined:
                            DecompiledText.Append("OP_IsDefined()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptMethodCall:
                            ParamAmount = reader.PeekChar();
                            StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                            DecompiledText.Append("OP_ScriptMethodCall(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetLevel:
                            DecompiledText.Append("OP_GetLevel()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetNegUnsignedShort:
                            IntValue = OPCodeFunction.OP_112(reader);
                            DecompiledText.Append("OP_GetNegUnsignedShort(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetNegByte:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_GetNegByte(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetLongUndefined:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetLongUndefined(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Notify:
                            DecompiledText.Append("OP_Notify()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EndOn:
                            DecompiledText.Append("OP_EndOn()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_NextArrayKey:
                            DecompiledText.Append("OP_NextArrayKey()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetAnimation:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetAnimation(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_LessThan:
                            DecompiledText.Append("OP_LessThan()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalSelfFieldVariableRef:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalSelfFieldVariableRef(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_DecTop:
                            DecompiledText.Append("OP_DecTop()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetUndefined:
                            DecompiledText.Append("OP_GetUndefined()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SetVariableField:
                            DecompiledText.Append("OP_SetVariableField()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalLevelFieldVariable:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalLevelFieldVariable(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetEmptyArray:
                            DecompiledText.Append("OP_GetEmptyArray()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ClearArray:
                            DecompiledText.Append("OP_ClearArray()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_WaitTill:
                            DecompiledText.Append("OP_WaitTill()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetApiFunction:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetApiFunction(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_WaitTillFrameEnd:
                            DecompiledText.Append("OP_WaitTillFrameEnd()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetSelf:
                            DecompiledText.Append("OP_GetSelf()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetFloat:
                            FloatValue = OPCodeFunction.OP_F324(reader);
                            DecompiledText.Append("OP_GetFloat(" + FloatValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Wait:
                            DecompiledText.Append("OP_Wait()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetHash:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetHash(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetWorldObject:
                            DecompiledText.Append("OP_GetWorldObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SizeOf:
                            DecompiledText.Append("OP_SizeOf()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalArrayRef:
                            DecompiledText.Append("OP_EvalArrayRef()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptMethodCallPointer:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_ScriptMethodCallPointer(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_BoolComplement:
                            DecompiledText.Append("OP_BoolComplement()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Bit_Xor:
                            DecompiledText.Append("OP_Bit_Xor()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Modulus:
                            DecompiledText.Append("OP_Modulus()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalArray:
                            DecompiledText.Append("OP_EvalArray()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalFieldVariable:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalFieldVariable(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SuperEqual:
                            DecompiledText.Append("OP_SuperEqual()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetClasses:
                            DecompiledText.Append("OP_GetClasses()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Bit_Or:
                            DecompiledText.Append("OP_Bit_Or()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_LessThanOrEqualTo:
                            DecompiledText.Append("OP_LessThanOrEqualTo()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetObjectType:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetObjectType(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Multiply:
                            DecompiledText.Append("OP_Multiply()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Vector:
                            DecompiledText.Append("OP_Vector()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetTime:
                            DecompiledText.Append("OP_GetTime()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalLocalVariableRefCached:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_EvalLocalVariableRefCached(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Switch:
                            StringValue = OPCodeFunction.OP_SwitchFunction(reader);
                            DecompiledText.Append("OP_Switch(" + StringValue.Remove(StringValue.Length - 2) + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CastBool:
                            DecompiledText.Append("OP_CastBool()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_NotEqual:
                            DecompiledText.Append("OP_NotEqual()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GreaterThan:
                            DecompiledText.Append("OP_GreaterThan()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalSelfFieldVariable:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalSelfFieldVariable(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetAnim:
                            DecompiledText.Append("OP_GetAnim()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Jump:
                            IntValue = OPCodeFunction.OP_JumpFunction(reader);
                            DecompiledText.Append("OP_Jump(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalLocalVariableCached:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_EvalLocalVariableCached(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptMethodThreadCall:
                            ParamAmount = reader.PeekChar();
                            StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                            DecompiledText.Append("OP_ScriptMethodThreadCall(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Dec:
                            DecompiledText.Append("OP_Dec()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Equal:
                            DecompiledText.Append("OP_Equal()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SetWaittillVariableFieldCached:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_SetWaittillVariableFieldCached(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Minus:
                            DecompiledText.Append("OP_Minus()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CastFieldObject:
                            DecompiledText.Append("OP_CastFieldObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetVector:
                            IntValue = OPCodeFunction.OP_32C(reader);
                            DecompiledText.Append("OP_GetVector(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_Divide:
                            DecompiledText.Append("OP_Divide()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_JumpOnFalse:
                            IntValue = OPCodeFunction.OP_JumpFunction(reader);
                            DecompiledText.Append("OP_JumpOnFalse(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_CallBuiltinMethod:
                            ParamAmount = reader.PeekChar();
                            if (RenameBuiltinCheckbox.Checked == true)
                            {
                                StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                                DecompiledText.Append("OP_CallBuiltinMethod(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            }
                            else
                            {
                                IntValue = OPCodeFunction.OP_424(reader);
                                DecompiledText.Append("OP_CallBuiltinMethod(" + ParamAmount + ", " + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            }
                            break;

                        case (int)OPCodeNames.OP_EvalFieldVariableRef:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalFieldVariableRef(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptThreadCall:
                            ParamAmount = reader.PeekChar();
                            StringValue = OPCodeFunction.OP_GetFunctionFunction(reader);
                            DecompiledText.Append("OP_ScriptThreadCall(" + ParamAmount + ", " + StringValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_WaitTillMatch:
                            DecompiledText.Append("OP_WaitTillMatch()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GreaterThanOrEqualTo:
                            DecompiledText.Append("OP_GreaterThanOrEqualTo()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ClearFieldVariable:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_ClearFieldVariable(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_VectorConstant:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_VectorConstant(" + ByteValue + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_JumpOnTrueExpr:
                            IntValue = OPCodeFunction.OP_JumpFunction(reader);
                            DecompiledText.Append("OP_JumpOnTrueExpr(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetInteger:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_GetInteger(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SafeSetVariableFieldCached:
                            ByteValue = reader.ReadByte();
                            DecompiledText.Append("OP_SafeSetVariableFieldCached(" + ByteValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_VectorScale:
                            DecompiledText.Append("OP_VectorScale()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_EvalLevelFieldVariableRef:
                            IntValue = OPCodeFunction.OP_324(reader);
                            DecompiledText.Append("OP_EvalLevelFieldVariableRef(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetLevelObject:
                            DecompiledText.Append("OP_GetLevelObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetAnimObject:
                            DecompiledText.Append("OP_GetAnimObject()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_SafeDecTop:
                            DecompiledText.Append("OP_SafeDecTop()" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_GetUnsignedShort:
                            IntValue = OPCodeFunction.OP_112(reader);
                            DecompiledText.Append("OP_GetUnsignedShort(" + IntValue.ToString("X") + ")" + System.Environment.NewLine);
                            break;

                        case (int)OPCodeNames.OP_ScriptMethodThreadCallPointer:
                            DecompiledText.Append("OP_ScriptMethodThreadCallPointer()" + System.Environment.NewLine);
                            break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                DecompiledText.Append("end of stream error!!!");
            }
            DecompiledText.Append(System.Environment.NewLine + System.Environment.NewLine);
            DecompiledTextOutput.Text = DecompiledText.ToString();
            CurrentOffsetTextOutput.Text = CurrentOffsetText.ToString();
            reader.Close();
        }

        //private void DumpGSC_Click(object sender, EventArgs e) //need my gsc hook DLL to use this, the DLL ghetto hooks GscObjLink and printfs all gsc file offsets whcih are then parsed using this function.
        //{                                                      //this function (not the dll) is incredibly unstable due to xrpc over wifi and crashes regulary but it worked in the end with all gsc files being correct.
        //    XRPC.Connect();                                    //If you want the DLL + source or anything else, PM me on se7ensins - se7ensins com/members/craftycritter.1077585/ 
        //    StreamReader reader = File.OpenText(@"E:\coding\RTE tool creation\Projects\BO3 GSC\GSC Dump\MP TDM redwood orig.txt");
        //    FileStream fs;
        //    char[] c;
        //    for (int i = 0; i < 468; i++)
        //    {
        //        c = new char[8];
        //        reader.Read(c, 0, c.Length);
        //        uint offset = UInt32.Parse(new string(c), System.Globalization.NumberStyles.AllowHexSpecifier);
        //        if ((offset & 0x10000000) == 0)
        //        {
        //            Byte[] GSCSize = XRPC.GetMemory(offset + 0x2c, 4);
        //            Byte[] GSCNameLoc = XRPC.GetMemory(offset + 0x34, 2);
        //            Array.Reverse(GSCSize);
        //            Array.Reverse(GSCNameLoc);
        //            short NameLoc = BitConverter.ToInt16(GSCNameLoc, 0);
        //            Byte[] GSCFile = XRPC.GetMemory(offset, BitConverter.ToUInt32(GSCSize, 0));
        //            string name = OPCodeFunction.DecodeAsciiZ(GSCFile, NameLoc);
        //            DecompiledText.Append(name + ", " + i +System.Environment.NewLine);
        //            string dir = @"G:\_raw\" + name;
        //            Directory.CreateDirectory(Path.GetDirectoryName(dir));
        //            fs = File.Create(dir);
        //            fs.Write(GSCFile, 0, GSCFile.Length);
        //            fs.Close();
        //        }
        //        reader.Read();
        //        reader.Read();
        //    }
        //    reader.Close();
        //}

        private void CanonicalHash_Click(object sender, EventArgs e)
        {
            CanonicalHashOutput.Text = (OPCodeFunction.CanonicalHash(CanonicalHashInput.Text) & 0x00000000FFFFFFFF).ToString("X");
        }

    }

    public class SyncTextBox : TextBox //cheers Hans Passant and John Willemse for the synced scroll code
    {
        public SyncTextBox()
        {
            this.Multiline = true;
            this.ScrollBars = ScrollBars.Vertical;
        }
        public Control Buddy { get; set; }

        private static bool scrolling;   // In case buddy tries to scroll us
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // Trap WM_VSCROLL message and pass to buddy
            if ((m.Msg == 0x115 || m.Msg == 0x20a) && !scrolling && Buddy != null && Buddy.IsHandleCreated)
            {
                scrolling = true;
                SendMessage(Buddy.Handle, m.Msg, m.WParam, m.LParam);
                scrolling = false;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}
