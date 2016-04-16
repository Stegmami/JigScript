// Author: Kelly Rey Wilson kelly@MolecularJig.com
//
// Copyright (c) 2014, NightPen, LLC and MolecularJig
//
// All rights reserved.
//
// when() statement Patent Pending
//
// While the source to JigScript is copyrighted, any JigScript
// add on function libraries you create or any JigScript script
// files you create are yours to do with as you please! If you
// develop a really cool game or function library, we would
// love to see it. You can contact us at MolecularJig.com
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class JigCompiler : MonoBehaviour
    {
        private enum IdentifierType
        {
            None = 0,
            Number = 1,
            Bool = 2,
            Word = 3,
            String = 4,
            Operator = 5,
            Comment = 6,
            Line = 7,
            Other = 8,
            Variable = 9,
            Statement = 10,
            UserFunction = 11,
            PostOperation = 12,
            Empty = 13
        }
        ;

        internal class ContinueBreakInfo
        {
            public List<int> continueAddress;
            public List<int> breakAddress;
            
            public ContinueBreakInfo()
            {
                this.continueAddress = new List<int>();
                this.breakAddress = new List<int>();
            }
        };

        private Stack<ContinueBreakInfo> continueBreakStack;

        private delegate void StatementHandler();

        private Dictionary<string, Operator> operators;
        private Dictionary<string, VariableInfo> localVariableInfo;
        private static Dictionary<string, VariableInfo> globalVariableInfo;
        private Dictionary<string, ScriptFunction> scriptFunctions;
        private Dictionary<string, StatementHandler> statements;
        private Stack<int> localValuesStack;
        private int braces;
        private int squareBraces = 0;
        private IdentifierType idTypeLast;
        private string lastVariableName;
        private string idLast;
        private string currentFile;
        private int index;
        private int line;
        private int lineStartIndex;
        private int errorLine;
        private string errorFile;
        private string script;
        private string functionName = string.Empty;
        private const int maxImportRecurseLevel = 10;
        private int currentImportRecurseLevel;
        private bool compilingWhenCondition;
        private bool compilingWhen;
        
        private int whenIndex;
        private string error;
        private CPU cpu;

        public TextAsset ScriptFile;

        internal class ImportFile
        {
            public string file;
            public string script;
        };

        int PopValue()
        {
            int rc = 0;

            if ( localValuesStack.Count>0 )
            {
                rc = localValuesStack.Pop();
            }
            return rc;
        }

        List<ImportFile> importedFiles;
        public bool runOnImport;

        public Value ReadVariable( string name, int arrayIndex )
        {
            Value v = null;

            if ( Variables.Exists(name) )
            {
                v = Variables.Read(name, arrayIndex);
            }

            return v;
        }

        //Function that gets called when the Print user defined function is called. By
        //default this is set to the console.
        public delegate void PrintOutputHandler( string output );
        
        PrintOutputHandler printOutputHandler;

        //Call this method to reassign the print() user function to your own function.
        public void SetPrintOutput( PrintOutputHandler printOutputHandler )
        {
            this.printOutputHandler = printOutputHandler;
        }

        void Awake()
        {
            Initialize();
        }

        void Start()
        {
            JigExtension[] jigExtensions = (JigExtension[])Resources.FindObjectsOfTypeAll(typeof(JigExtension));
            
            foreach(JigExtension je in jigExtensions)
            {
                je.Initialize(this);
            } 
            
            if ( Application.isEditor == false )
            {
                if ( string.IsNullOrEmpty(ScriptFile.ToString()) ==  false )
                {
                    RunScript(ScriptFile.ToString(), 0);
                }
            }
        }
        
        private void ProcessImports( string fileName, string s )
        {
            string[] lines;
            
            if ( currentImportRecurseLevel>=maxImportRecurseLevel )
            {
                error = "import recursion level too deep. Max is " + maxImportRecurseLevel.ToString() + ".";
                errorFile = fileName;
                errorLine = 0;
                return;
            }

            currentImportRecurseLevel++;

            lines = s.Split(new char[]
            {
                '\n'
            });
            line = 0;
            
            foreach( string ls in lines )
            {
                line++;
                if ( ls.Length>6 && ls.Substring(0, 6) == "import" )
                {
                    string tmp = string.Empty;

                    int ii = 6;

                    while( ii < ls.Length && char.IsWhiteSpace(ls[ii]) )
                    {
                        ii++;
                    }
                    if ( ii == ls.Length )
                    {
                        error = "import statement is missing the name of the file to import.";
                        errorLine = line;
                        errorFile = fileName;
                        break;
                    }
                    if ( ls[ii] != '\"' )
                    {
                        error = "import missing open \"";
                        errorLine = line;
                        errorFile = fileName;
                        break;
                    }
                    ii++;
                    while( ii < ls.Length && ls[ii] != '\"' )
                    {
                        tmp += ls[ii];
                        ii++;
                    }
                    if ( ii == ls.Length || ls[ii] != '\"' )
                    {
                        error = "import missing close \"";
                        errorLine = line;
                        errorFile = fileName;
                        break;
                    }
                    ii++;
                    if ( ii == ls.Length || ls[ii] != ';' )
                    {
                        error = "import missing semicolon";
                        errorLine = line;
                        errorFile = fileName;
                        break;
                    }
                    ii++;
                    TextAsset tx = Resources.Load(tmp) as TextAsset;
                    if ( tx == null )
                    {
                        error = "import file " + tmp + "was not found. Make sure its at the correct resources path.";
                        errorLine = line;
                        errorFile = fileName;
                        break;
                    }
                    ProcessImports(tmp, tx.ToString());
                }
            }
            if ( errorLine == 0 )
            {
                importedFiles.Add(new ImportFile() { file = fileName, script = s });
            }

            currentImportRecurseLevel--;
        }

        public CPU[] availableCPUs;

        public void ReRun()
        {
            StartCoroutine(InternalRun(script, 0));
        }

        public void RunScript( string script, int cpuIndex )
        {
            StartCoroutine(InternalRun(script, cpuIndex));
        }
        
        IEnumerator InternalRun( string script, int cpuIndex )
        {
            yield return StartCoroutine(Compile(script, cpuIndex));
            if ( errorLine == 0 )
            {
                UIControls uic = GetComponent<UIControls>();
                if ( uic )
                {
                    yield return StartCoroutine(uic.ClearFunction(new List<Value>()));
                }
                SoundFunctions sf = GetComponent<SoundFunctions>();
                if ( sf != null )
                {
                    sf.Clear();
                }
                GOUserFunctions gof = GetComponent<GOUserFunctions>();
                if ( gof != null )
                {
                    yield return StartCoroutine(gof.ResetObjects());
                }
                
                availableCPUs[cpuIndex].Run();
            }
            else
            {
                Debug.LogError(GetErrors());
            }
        }

        public void Dissassemble( string script, int cpuIndex )
        {
            StartCoroutine(InternalDissassemble(script, cpuIndex));
        }

        IEnumerator InternalDissassemble( string script, int cpuIndex )
        {
            yield return StartCoroutine(Compile(script, cpuIndex));
            if ( errorLine == 0 )
            {
                Debug.Log(availableCPUs[cpuIndex].Dissassemble());
            }
            else
            {
                Debug.LogError(GetErrors());
            }
        }
        
        private string ToAscii( string input )
        {
            // Create two different encodings.
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;
            
            // Convert the string into a byte[].
            byte[] unicodeBytes = unicode.GetBytes(input);
            
            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);
            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            
            return new string(asciiChars);
        }

        public IEnumerator Compile( string script, int cpuIndex )
        {
            cpu = availableCPUs[cpuIndex];

            cpu.SetCompileType(false);

            compilingWhenCondition = false;
            compilingWhen = false;
            whenIndex = -1;

            yield return StartCoroutine(cpu.ResetCPU());
            currentImportRecurseLevel = 0;

            localValuesStack.Clear();

            localVariableInfo.Clear();
            Variables.ClearLocals();

            scriptFunctions.Clear();

            while( continueBreakStack.Count > 0 )
            {
                continueBreakStack.Peek().continueAddress.Clear();
                continueBreakStack.Peek().breakAddress.Clear();
                continueBreakStack.Pop();
            }

            error = string.Empty;
            errorLine = 0;

            importedFiles = new List<ImportFile>();

            ProcessImports("main_", script);

            if ( errorLine == 0 )
            {
                foreach( ImportFile file in importedFiles )
                {
                    currentFile = file.file;

                    braces = 0;
                    squareBraces = 0;
                    line = 1;
                    index = 0;
                    
                    this.script = ToAscii(file.script) + "\n";

                    while( index < this.script.Length && ProcessExpression() )
                    {
                    }
                    if ( errorLine != 0 )
                    {
                        errorFile = file.file;
                        break;
                    }
                }
                if ( errorLine == 0 )
                {
                    cpu.AppendInstruction(new Instruction(OpCode.END));
                }
            }
            yield return 0;
        }

        /// <summary>
        /// Returns a string containing any errors that occured in the last compile.
        /// </summary>
        /// <returns>Any errors that occured in the last compilation attempt.</returns>
        public string GetErrors()
        {
            string errors = string.Empty;
            
            if ( errorLine != 0 )
            {
                if ( errorLine<0 )
                {
                    errors = error;
                }
                else
                {
                    errors = errorFile + " : Error(" + errorLine + ") : " + error;
                }
            }
            return errors;
        }

        //Used to set a run time error. Mainly for debugging when using the game console for scripting a level.
        public void SetError( string msg )
        {
            errorLine = -1;
            error = msg;
        }
        
        public void ProcessWhens()
        {
            Value v1 = Variables.Read("whens.enable", 1);
            if ( v1.B )
            {
                for(int ii=0; ii<availableCPUs.Length; ++ii)
                {
                    if ( availableCPUs[ii].isPlaying == false )
                    {
                        break;
                    }
                    availableCPUs[ii].ProcessWhens();
                }
            }
        }
        
        public bool AddFunction( string name, UserFunction.Function fn, bool wait = false )
        {
            bool rc = false;

            if ( Variables.Exists(name) == false )
            {
                Value v = new Value(new UserFunction(fn, wait), name);

                Variables.Create(name, v);
                rc = true;
            }

            return rc;
        }

        //forces the value to be converted to int
        private Value GetToInt(Value v)
        {
            v.ConvertTo(Value.ValueType.Integer);
            return v;
        }
        
        //forces the value to be converted to int
        private Value GetToString(Value v)
        {
            v.ConvertTo(Value.ValueType.String);
            return v;
        }
        
        //forces the value to be converted to int
        private Value GetToFloat(Value v)
        {
            v.ConvertTo(Value.ValueType.Float);
            return v;
        }
        
        /// <summary>
        /// Initializes the compiler for use.
        /// </summary>

        public void Initialize()
        {
            idTypeLast = IdentifierType.None;
            idLast = string.Empty;

            operators = new Dictionary<string, Operator>();
            scriptFunctions = new Dictionary<string, ScriptFunction>();
            statements = new Dictionary<string, StatementHandler>();

            localVariableInfo = new Dictionary<string, VariableInfo>();
            localValuesStack = new Stack<int>();
            
            globalVariableInfo = new Dictionary<string, VariableInfo>();

            statements.Add("if", CompileIF);
            statements.Add("else", CompileElse);
            statements.Add("while", CompileWhile);
            statements.Add("var", CompileVar);
            statements.Add("local", CompileVar);
            statements.Add("fun", CompileScriptFunction);
            statements.Add("return", CompileReturn);
            statements.Add("global", CompileGlobalVariable);
            statements.Add("import", CompileImport);
            statements.Add("for", CompileFor);
            statements.Add("when", CompileWhen);
            statements.Add("update", CompileUpdate);
            statements.Add("yield", CompileYield);
            statements.Add("repeat", CompileRepeat);
            statements.Add("repeat.count", CompileRepeatCount);
            statements.Add("break", CompileBreak);
            statements.Add("continue", CompileContinue);
            
            statements.Add("DebugBreak", CompileDebugBreak);
            
            
            //unary operators needed.

//            operators.Add("!", new Operator("!", 100, NoHandler));
                                    
            operators.Add("++", new Operator("++", 100, NoHandler));
            operators.Add("--", new Operator("--", 100, NoHandler));
            
            operators.Add("*", new Operator("*", 30, Mul));
            operators.Add("/", new Operator("/", 29, Div));
            operators.Add("%", new Operator("%", 28, Mod));
            operators.Add("+", new Operator("+", 27, Add));
            operators.Add("-", new Operator("-", 26, Sub));
            operators.Add("<<", new Operator("<<", 23, ShiftLeft));
            operators.Add(">>", new Operator(">>", 23, ShiftRight));
            
            operators.Add("<", new Operator("<", 21, Less));
            operators.Add(">", new Operator(">", 21, Greater));
            operators.Add(">=", new Operator("<=", 21, LessOrEqual));
            operators.Add("<=", new Operator(">=", 21, GreaterOrEqual));
            operators.Add("==", new Operator("==", 21, Equal));
            operators.Add("!=", new Operator("!=", 21, NotEqual));
            
            operators.Add("&", new Operator("&", 15, And));
            operators.Add("|", new Operator("|", 14, Or));
            operators.Add("^", new Operator("^", 13, Xor));
            
            operators.Add("&&", new Operator("&&", 12, ConditionalAnd));
            operators.Add("||", new Operator("||", 11, ConditionalOr));
            
            operators.Add(",", new Operator(",", 10, NoHandler));
            
            operators.Add("=", new Operator("=", 9, Assignment));

            operators.Add("+=", new Operator("+=", 9, AddAssignment));
            operators.Add("-=", new Operator("-=", 9, SubAssignment));
            operators.Add("*=", new Operator("*=", 9, MulAssignment));
            operators.Add("/=", new Operator("/=", 9, DivAssignment));
            operators.Add("%=", new Operator("%=", 9, ModAssignment));
            operators.Add("&=", new Operator("&=", 9, AndAssignment));
            operators.Add("|=", new Operator("|=", 9, OrAssignment));
            operators.Add("^=", new Operator("^=", 9, XorAssignment));
            

            operators.Add("(", new Operator("(", 8, NoHandler));
            operators.Add(")", new Operator(")", 7, NoHandler));

            operators.Add("[", new Operator("[", 6, NoHandler));
            operators.Add("]", new Operator("]", 5, NoHandler));
            
            operators.Add(";", new Operator(";", 3, NoHandler));
            
            operators.Add("{", new Operator("{", 2, NoHandler));
            operators.Add("}", new Operator("}", 1, NoHandler));

            braces = 0; //The brace count determines if a variable is local or global.
            //Unless the key word global is used in which case it does
            //not matter where the variable is defined it is global.
                                                
            //Grouping symbols ( = precedence 8, { precedence 0
            //These operators act both like high precedence when encountered but like
            //a low precedence operator when on the operator stack.
            //the comma operator forces operator stack evaluation until the operator
            //stack is empty, or another comma or ( is encountered.

            Variables.Create("$0", new Value(0, "$0"));
            Variables.Create("whens.enable", new Value(false, "whens.enable"));
            
            continueBreakStack = new Stack<ContinueBreakInfo>();

            SetPrintOutput(DefaultPrintOutputHandler);
            
            AddFunction("print", PrintFunction);
            AddFunction("save", SaveFunction, true);
            AddFunction("load", LoadFunction, true);
            AddFunction("delete", DeleteFunction, true);
            
            Variables.CreateCustomProperty("toInt", GetToInt, null);
            Variables.CreateCustomProperty("toFloat", GetToFloat, null);
            Variables.CreateCustomProperty("toString", GetToString, null);
        }
        
        public delegate Value SetFunction( Value dest, int arrayIndex, Value source );
        
        private void DefaultPrintOutputHandler( string output )
        {
            Debug.Log(output);
        }
        
        private IEnumerator PrintFunction( List<Value> values )
        {
            string s = string.Empty;
            if ( values.Count<1 )
            {
                Debug.LogError("print(variable, ...);");
            }
            else
            {
                foreach( Value p in values )
                {
                    List<Value> expanded = Variables.Expand(p, true);
                    
                    for(int ii=1; ii<expanded.Count; ++ii)
                    {
                        expanded[ii].ConvertTo(Value.ValueType.String);
                        s += expanded[ii].S + "\r\n";
                    }
                }
    
                printOutputHandler(s);
            }

            yield return 0;
        }
        
        private IEnumerator SaveFunction( List<Value> values )
        {
            if ( values.Count != 2 )
            {
                Debug.LogError("save(key, variable);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                string s = values[1].ToString();
                PlayerPrefs.SetString(values[0].S, s);
                PlayerPrefs.Save();
            }
            yield return 0;
        }
        
        private IEnumerator LoadFunction( List<Value> values )
        {
            if ( values.Count<1 )
            {
                Debug.LogError("variable = load(key);");
            }

            values[0].ConvertTo(Value.ValueType.String);

            string s = PlayerPrefs.GetString(values[0].S);
            Value v = Value.FromString(s);
            values.Add(v);
            yield return 0;
        }

        private IEnumerator DeleteFunction( List<Value> values )
        {
            if ( values.Count<1 )
            {
                Debug.LogError("delete(key);");
            }
            
            values[0].ConvertTo(Value.ValueType.String);

            PlayerPrefs.DeleteKey(values[0].S);
            
            yield return 0;
        }

        private void ProcessAssignment(OpCode code = OpCode.NOP)
        {
            PopValue();
            PopValue();
            
            if ( code != OpCode.NOP )
            {
                cpu.AppendInstruction(new Instruction(code));
            }
            
            cpu.AppendInstruction(new Instruction(OpCode.POPB));
            cpu.AppendInstruction(new Instruction(OpCode.POPA));
            
            cpu.AppendInstruction(new Instruction(OpCode.MOVM));
        }
        
        private void Assignment()
        {
            ProcessAssignment(OpCode.NOP);
        }
        
        private void AddAssignment()
        {
            ProcessAssignment(OpCode.ADD);
        }
        
        private void SubAssignment()
        {
            ProcessAssignment(OpCode.SUB);
        }
        
        private void MulAssignment()
        {
            ProcessAssignment(OpCode.MUL);
        }
        
        private void DivAssignment()
        {
            ProcessAssignment(OpCode.DIV);
        }
        
        private void ModAssignment()
        {
            ProcessAssignment(OpCode.MOD);
        }
        
        private void AndAssignment()
        {
            ProcessAssignment(OpCode.AND);
        }
        
        private void OrAssignment()
        {
            ProcessAssignment(OpCode.OR);
        }
        
        private void XorAssignment()
        {
            ProcessAssignment(OpCode.XOR);
        }
        
        private void Add()
        {
            ProcessOpCode(OpCode.ADD);
        }
        
        private void Sub()
        {
            ProcessOpCode(OpCode.SUB);
        }

        private void ShiftRight()
        {
            ProcessOpCode(OpCode.SHR);
        }

        private void ShiftLeft()
        {
            ProcessOpCode(OpCode.SHL);
        }

        private void Mul()
        {
            ProcessOpCode(OpCode.MUL);
        }

        private void Mod()
        {
            ProcessOpCode(OpCode.MOD);
        }

        private void Div()
        {
            ProcessOpCode(OpCode.DIV);
        }

        private void And()
        {
            ProcessOpCode(OpCode.AND);
        }

        private void Or()
        {
            ProcessOpCode(OpCode.OR);
        }

        private void Xor()
        {
            ProcessOpCode(OpCode.XOR);
        }

        private void Shr()
        {
            ProcessOpCode(OpCode.SHR);
        }
        
        private void Shl()
        {
            ProcessOpCode(OpCode.SHL);
        }
        
        private void Less()
        {
            ProcessOpCode(OpCode.LT);
        }
        
        private void Greater()
        {
            ProcessOpCode(OpCode.GT);
        }
        
        private void LessOrEqual()
        {
            ProcessOpCode(OpCode.GTE);
        }
        
        private void GreaterOrEqual()
        {
            ProcessOpCode(OpCode.LTE);
        }
        
        private void Equal()
        {
            ProcessOpCode(OpCode.EQ);
        }
        
        private void NotEqual()
        {
            ProcessOpCode(OpCode.NEQ);
        }
        
        private void ConditionalAnd()
        {
            ProcessOpCode(OpCode.LAND);
        }
        
        private void ConditionalOr()
        {
            ProcessOpCode(OpCode.LOR);
        }

        private void PreIncrement( Stack<Operator> opStack )
        {
            UnaryOperators(OpCode.INC, opStack);
        }

        private void PreDecrement( Stack<Operator> opStack )
        {
            UnaryOperators(OpCode.DEC, opStack);
        }

        private void NegateOperator(Stack<Operator> opStack)
        {
            int lineStart;
            string id;
            
            lineStart = line;
            
            IdentifierType idType = IdentifierType.None;
            
            if ( GetIndentifier(out id, out idType) == false )
            {
                error = "end of file found.";
                errorLine = lineStart;
                return;
            }
            else if ( ProcessVariable(id, ref idType) )
            {
                ProcessVariable(id, ref idType);
                cpu.AppendInstruction(new Instruction(OpCode.NEG));
                ProcessAssignment(OpCode.NOP);
            }
            else if ( idType == IdentifierType.Number ||
                      idType == IdentifierType.Bool ||
                      idType == IdentifierType.String )
            {
                PushValue(id, idType);
                PushValue(id, idType);
                cpu.AppendInstruction(new Instruction(OpCode.NEG));
            }
            else
            {
                error = "Cannot negate the provided value." + idType;
                errorLine = lineStart;
                return;
            }
        }

        private void NoHandler()
        {
        }

        private VariableInfo GetVariableInformation( string id )
        {
            VariableInfo vInfo = null;
            string localId;
            
            if ( IsLocalVariable(id, out localId) )
            {
                vInfo = localVariableInfo[localId];
            }
            else if ( Variables.Exists(id) )
            {
                if ( globalVariableInfo.ContainsKey(id) == false )
                {
                    globalVariableInfo.Add(id, new VariableInfo(id, true, Variables.Index(id), 0, true, braces, 0));
                }
                vInfo = globalVariableInfo[id];
            }
            else
            {
                return null;
            }
            return new VariableInfo(vInfo);
        }
        
        private void UnaryOperators( OpCode code, Stack<Operator> opStack )
        {
            string id;
            int lineStart = line;
            
            int save = index;
            if ( GetIndentifier(out id) == false )
            {
                error = "++ and -- are only valid for variables.";
                errorLine = lineStart;
            }
            VariableInfo vInfo = GetVariableInformation(id);
            if ( vInfo == null )
            {
                error = "Variable " + id + " does not exist.";
                errorLine = lineStart;
                return;
            }
            cpu.AppendInstruction(new Instruction(code, vInfo.index, vInfo.arrayIndex, vInfo.field, false));
            
            if ( opStack.Count == 1 && (CheckNext(")") || CheckNext(",") || CheckNext(";")) )
            {
            }
            else
            {
                index = save;
            }
        }

        //v1 should always == the right most value.
        private void ProcessOpCode( OpCode code )
        {
            PopValue();
            PopValue();

            cpu.AppendInstruction(new Instruction(code));

            localValuesStack.Push(CPU.ACC1);
        }

        void CompileIF()
        {
            int lineStart = line;
            
            if ( CheckNext("(") )
            {
                if ( ProcessExpression() )
                {
                    PopValue();
                    
                    int condCheckAddr;
                    cpu.AppendInstruction(new Instruction(OpCode.POPA));
                    condCheckAddr = cpu.AppendInstruction(new Instruction(OpCode.JF, 0, 0, -1));
                    
                    if ( CheckNext("{") )
                    {
                        if ( ProcessExpression() )
                        {
                            if ( CheckNext("else") )
                            {
                                //Advance past the else keyword.
                                string id = string.Empty;
                                GetIndentifier(out id);
                                
                                int elseAddress = cpu.AppendInstruction(new Instruction(OpCode.JA, CPU.ACC1, 0, -1));
                                cpu.ReplaceInstruction(condCheckAddr, new Instruction(OpCode.JF, CPU.ACC1, 0, cpu.CurrentAddress));
                                if ( ProcessExpression() )
                                {
                                    cpu.ReplaceInstruction(elseAddress, new Instruction(OpCode.JA, CPU.ACC1, 0, cpu.CurrentAddress));
                                }
                            }
                            else
                            {
                                cpu.ReplaceInstruction(condCheckAddr, new Instruction(OpCode.JF, CPU.ACC1, 0, cpu.CurrentAddress));
                            }
                        }
                    }
                    else
                    {
                        error = "{ must follow if statement, syntax is if (condition) { }.";
                        errorLine = lineStart;
                    }
                }
            }
            else
            {
                error = "( must follow if statement, syntax is if (condition) { }.";
                errorLine = lineStart;
            }
        }

        void CompileElse()
        {
            error = "else without a corrisponding if.";
            errorLine = line;
        }

        private void CompileUserFunction( string id )
        {
            int lineStart = line;

            int vsTop = localValuesStack.Count;

            int fnId = Variables.Index(id);

            if ( CheckNext("(") )
            {
                if ( ProcessExpression() )
                {
                    cpu.AppendInstruction(new Instruction(OpCode.CPS));

                    while( localValuesStack.Count > vsTop )
                    {
                        PopValue();
                        cpu.AppendInstruction(new Instruction(OpCode.POPA));
                        cpu.AppendInstruction(new Instruction(OpCode.PUSHP));
                    }
                    
                    cpu.AppendInstruction(new Instruction(OpCode.CALL, 0, 0, fnId));
//                  cpu.AppendInstruction(new Instruction(OpCode.PUSHA));
                }
            }
            else
            {
                error = "( must follow function name.";
                errorLine = lineStart;
            }
        }

        void CompileWhile()
        {
            int lineStart = line;

            int condAddr = cpu.CurrentAddress;
            
            continueBreakStack.Push(new ContinueBreakInfo());
            
            if ( CheckNext("(") == false )
            {
                error = "( must follow while statement, syntax is if (condition) { }.";
                errorLine = lineStart;
                continueBreakStack.Pop();
                return;
            }
            if ( ProcessExpression() == false )
            {
                continueBreakStack.Pop();
                return;
            }
            PopValue();
            int condCheckAddr;

            cpu.AppendInstruction(new Instruction(OpCode.POPA));
            condCheckAddr = cpu.AppendInstruction(new Instruction(OpCode.JF, CPU.ACC1, 0, -1));

            if ( CheckNext("{") )
            {
                if ( ProcessExpression() )
                {
                    cpu.AppendInstruction(new Instruction(OpCode.JA, 0, 0, condAddr));
                    cpu.ReplaceInstruction(condCheckAddr, new Instruction(OpCode.JF, CPU.ACC1, 0, cpu.CurrentAddress));
                }
                else
                {
                    continueBreakStack.Pop();
                    return;
                }
            }
            else
            {
                error = "{ must follow while statement, syntax is if (condition) { }.";
                errorLine = lineStart;
                continueBreakStack.Pop();
                return;
            }
            ContinueBreakInfo cbi = continueBreakStack.Pop();
            foreach( int continueAddress in cbi.continueAddress )
            {
                cpu.ReplaceInstruction(continueAddress, new Instruction(OpCode.JA, CPU.ACC1, 0, condAddr));
            }
            foreach( int breakAddress in cbi.breakAddress )
            {
                cpu.ReplaceInstruction(breakAddress, new Instruction(OpCode.JA, CPU.ACC1, 0, cpu.CurrentAddress));
            }
        }

        void CompileImport()
        {
            while( index < script.Length && script[index] != '\n' )
            {
                index++;
            }
        }

        void CompileReturn()
        {
            if ( cpu.CurrentAddress<CPU.functionsBase )
            {
                error = "return statement can only be used inside of a function.";
                errorLine = line;
                return;
            }

            //if no expression then simply push 0
            if ( CheckNext(";") )
            {
                cpu.AppendInstruction(new Instruction(OpCode.PUSHI, new Value(0, Variables.GetUniqueName())));
                SkipNext();
            }
            else
            {
                if ( ProcessExpression() )
                {
                    cpu.AppendInstruction(new Instruction(OpCode.RET));
                }
            }
        }
        
        void CompileVar()
        {
            CompileVariable(true, Value.ValueType.Integer);
        }
        
        void CompileGlobalVariable()
        {
            CompileVariable(false, Value.ValueType.Integer);
        }

        private bool IsValidVariableName( string name )
        {
            bool rc = false;

            if ( name[0] == '_' || Char.IsLetter(name[0]) || name[0] == '.' )
            {
                int ii = 1;
                while( ii<name.Length )
                {
                    if ( name[ii] != '_' && Char.IsLetterOrDigit(name[ii]) == false && name[ii] != '.' )
                        break;
                    ii++;
                }
                if ( ii == name.Length )
                {
                    rc = true;
                }
            }
            return rc;
        }

        void CompileScriptFunction()
        {
            if ( braces>0 )
            {
                error = "Functions are always global scrope, Sub functions are not allowed.";
                errorLine = line;

                return;
            }

            int lineStart = line;

            if ( GetIndentifier(out functionName) == false )
            {
                error = "function name was not found. Syntax is, fun name(parameters) { }";
                errorLine = lineStart;
                return;
            }

            int variableIndex;
            
            cpu.SetCompileType(true);

            if ( Variables.Exists(functionName) )
            {
                variableIndex = Variables.Index(functionName);
            }
            else
            {
                variableIndex = Variables.Create(functionName, new Value(new FUN(cpu.CurrentAddress), functionName + "(){}"));
            }

            int cpuAddress = cpu.CurrentAddress;

            if ( CheckNext("(") == false )
            {
                error = "( must follow function name.";
                errorLine = lineStart;
                cpu.SetCompileType(false);
                return;
            }
            SkipNext();

            List<ScriptFunction.Parameter> parameters = new List<ScriptFunction.Parameter>();
            while( CheckNext(")") == false && errorLine == 0 )
            {
                string name;
                GetIndentifier(out name);
                string localId = GetLocalName(name, braces, functionName);
                if ( IsValidVariableName(name) == false )
                {
                    error = "Parameter " + name + " is not a valid parameter name.";
                    errorLine = lineStart;
                    cpu.SetCompileType(false);
                    return;
                }
                else if ( localVariableInfo.ContainsKey(localId) )
                {
                    error = name + " is not a valid parameter name, name must begin with an _ or letter and contain only _, letters, digits or periods.";
                    errorLine = lineStart;
                    cpu.SetCompileType(false);
                    return;
                }
                int parameterIndex;
                parameterIndex = Variables.Create(localId, new Value(0, localId));
                localVariableInfo.Add(localId, new VariableInfo(localId, false, parameterIndex, 0, true, -1, 0)); //parameter definations for functiosn cannot be properties.
                parameters.Add(new ScriptFunction.Parameter(name, parameterIndex, 0));
                cpu.AddFunctionParameter(cpuAddress, parameterIndex);

                if ( CheckNext(",") )
                {
                    SkipNext();
                }
            }

            if ( errorLine>0 || CheckNext(")") == false )
            {
                error = "Function defination must end with an )";
                cpu.SetCompileType(false);
                errorLine = lineStart;
                return;
            }
            SkipNext();

            if ( CheckNext("{") == false )
            {
                error = "{ must follow fun() statement, syntax is fun(parameters) { }.";
                errorLine = lineStart;
                cpu.SetCompileType(false);
                return;
            }
            
            if ( ProcessExpression(null) == false )
            {
                return;
            }
            
            string identifier = string.Empty;
            
            int current = index;
            
            GetIndentifier(out identifier);
            
            index = current;
            cpu.AppendInstruction(new Instruction(OpCode.PUSHI, new Value(0, Variables.GetUniqueName())));
            cpu.AppendInstruction(new Instruction(OpCode.RET));
            scriptFunctions.Add(functionName, new ScriptFunction(functionName, variableIndex, cpuAddress, parameters));
            cpu.SetCompileType(false);
        }
        
        private void CompileScriptFunctionCall( string functionName )
        {
            int lineStart = line;
            
            ScriptFunction fn = new ScriptFunction(scriptFunctions[functionName]);
            
            if ( CheckNext("(") == false )
            {
                error = "Function call requires an ( after the function name.";
                errorLine = lineStart;
                return;
            }
            int vsTop = localValuesStack.Count;
            if ( ProcessExpression() == false )
            {
                return;
            }
            
            int count = localValuesStack.Count - vsTop;
            cpu.AppendInstruction(new Instruction(OpCode.PUSHI, new Value(count, "%" + functionName + "Call_ParameterCount")));
            cpu.AppendInstruction(new Instruction(OpCode.JR, 0, 0, fn.cpuAddress));
            while( count > 0 )
            {
                localValuesStack.Pop();
                count--;
            }
        }

        string CompileVariable( bool isLocal, Value.ValueType variableType = Value.ValueType.Integer )
        {
            string id = string.Empty;
            int lineStart = line;

            int save = index;
            bool expressionAfter = false;
            if ( GetIndentifier(out id) == false || IsValidVariableName(id) == false )
            {
                error = "Invalid variable name. The name must begin with a letter or _. The variable can contain any number of letters or numbers.";
                errorLine = lineStart;
                return id;
            }

            int initialSize = 0;

            if ( CheckNext("[") )
            {
                SkipNext();
                string sid = string.Empty;
                IdentifierType idType = IdentifierType.None;
                if ( GetIndentifier(out sid, out idType) == false )
                {
                    error = "Array initializer must be a positive integer value.";
                    errorLine = lineStart;
                    return id;
                }
                if ( idType != IdentifierType.Number )
                {
                    error = "An array variable initializer must be a number.";
                    errorLine = lineStart;
                    return id;
                }
                if ( sid.IndexOf('.') > 0 )
                {
                    error = "Array initializer cannnot be a float value.";
                    errorLine = lineStart;
                    return id;
                }
                int.TryParse(sid, out initialSize);
                if ( initialSize == 0 )
                {
                    error = "Array initializer must be an integer value greater than 0.";
                    errorLine = lineStart;
                    return id;
                }
                if ( CheckNext("]") == false )
                {
                    error = "] is missing from array initializer.";
                    errorLine = lineStart;
                    return id;
                }
                SkipNext();
            }

            expressionAfter = CheckNext("=");

            if ( expressionAfter == false && CheckNext(";") == false )
            {
                error = "Variable declarations must end with a semi-colon or an assignment.";
                errorLine = lineStart;
                return id;
            }

            int varIndex;
            
            Value v = new Value(variableType, id);

            v.arrayIndex = 1;
            VariableInfo vInfo;

            if ( isLocal )
            {
                string localId = GetLocalName(id, braces);
                if ( localVariableInfo.ContainsKey(localId) )
                {
                    if ( localVariableInfo[localId].inuse )
                    {
                        error = "A variable named " + id + " has already been defined.";
                        errorLine = lineStart;
                        return id;
                    }
                    else
                    {
                        vInfo = localVariableInfo[localId];
                        varIndex = vInfo.index;
                    }
                }
                else
                {
                    v.name = localId;
                    varIndex = Variables.Create(localId, v);
                    //when variable is defined it cannot have a custom property.
                    vInfo = new VariableInfo(localId, false, varIndex, 1, true, braces, 0);
                    localVariableInfo.Add(localId, vInfo);
                }
            }
            else
            {
                if ( Variables.Exists(id) == false )
                {
                    varIndex = Variables.Create(id, v);
                    vInfo = new VariableInfo(id, true, varIndex, 1, true, braces, 0);
                    globalVariableInfo.Add(id, vInfo);
                }
                else
                {
                    varIndex = Variables.Index(id);
                    vInfo = new VariableInfo(globalVariableInfo[id]);
                }
            }
            localValuesStack.Push(varIndex);
            if ( expressionAfter )
            {
                index = save;   //parse rest of expression.
            }

            return id;
        }

        void CompileFor()
        {
            int lineStart = line;
            
            if ( CheckNext("(") == false )
            {
                error = "missing (";
                errorLine = lineStart;
                return;
            }
            SkipNext();
            if ( ProcessExpression() == false )
            {
                return;
            }

            int condAddr = cpu.CurrentAddress;
            continueBreakStack.Push(new ContinueBreakInfo());

            if ( ProcessExpression() == false )
            {
                continueBreakStack.Pop();
                return;
            }
            PopValue();
            int condCheckAddr;
            cpu.AppendInstruction(new Instruction(OpCode.POPA));
            condCheckAddr = cpu.AppendInstruction(new Instruction(OpCode.JF, CPU.ACC1, 0, -1));
            
            int updateExpression = index;
            
            while( index < script.Length && script[index] != '{' )
            {
                index++;
            }
            
            if ( index == script.Length )
            {
                error = "missing { in for statement, syntax is for(initialization;condition;update) { }";
                line = lineStart;
                continueBreakStack.Pop();
                return;
            }

            int cpuUpdateExpression;

            if ( ProcessExpression() )
            {
                int loopExit = index;
                
                index = updateExpression;
                
                //compile update expression
                cpuUpdateExpression = cpu.CurrentAddress;
                if ( ProcessExpression(new Operator(operators["("])) )
                {
                    cpu.AppendInstruction(new Instruction(OpCode.JA, 0, 0, condAddr));
                }
                
                cpu.ReplaceInstruction(condCheckAddr, new Instruction(OpCode.JF, CPU.ACC1, 0, cpu.CurrentAddress));
                
                index = loopExit;
            }
            else
            {
                continueBreakStack.Pop();
                return;
            }
            ContinueBreakInfo cbi = continueBreakStack.Pop();
            foreach( int continueAddress in cbi.continueAddress )
            {
                cpu.ReplaceInstruction(continueAddress, new Instruction(OpCode.JA, CPU.ACC1, 0, cpuUpdateExpression));
            }
            foreach( int breakAddress in cbi.breakAddress )
            {
                cpu.ReplaceInstruction(breakAddress, new Instruction(OpCode.JA, CPU.ACC1, 0, cpu.CurrentAddress));
            }
        }

        private void CompileBreak()
        {
            if ( continueBreakStack.Count == 0 )
            {
                error = "The break statement is not valid outside of a while or for loop.";
                errorLine = line;
                return;
            }
            if ( CheckNext(";") == false )
            {
                error = "A semi-colon must follow the break statement.";
                errorLine = line;
                return;
            }
            continueBreakStack.Peek().breakAddress.Add(cpu.CurrentAddress);
            cpu.AppendInstruction(new Instruction(OpCode.JA, 0, 0, -1));
        }
        
        private void CompileDebugBreak()
        {
            if ( CheckNext("(") == false )
            {
                error = "( must follow DebugBreak statement, syntax is DebugBreak();";
                errorLine = line;
                return;
            }
            SkipNext();
            if ( CheckNext(")") == false )
            {
                error = ") must follow DebugBreak statement, syntax is DebugBreak();";
                errorLine = line;
                return;
            }
            SkipNext();
            if ( CheckNext(";") == false )
            {
                error = "; must follow DebugBreak statement, syntax is DebugBreak();";
                errorLine = line;
                return;
            }
            SkipNext();
            cpu.AppendInstruction(new Instruction(OpCode.NOP));
        }

        private void CompileContinue()
        {
            if ( continueBreakStack.Count == 0 )
            {
                error = "The continue statement is not valid outside of a while or for loop.";
                errorLine = line;
                return;
            }
            if ( CheckNext(";") == false )
            {
                error = "A semi-colon must follow the continue statement.";
                errorLine = line;
                return;
            }
            continueBreakStack.Peek().continueAddress.Add(cpu.CurrentAddress);
            cpu.AppendInstruction(new Instruction(OpCode.JA, 0, 0, -1));
        }
        
        private void CompileRepeatCount()
        {
            cpu.AppendInstruction(new Instruction(OpCode.PRC));
        }
        
        private void CompileRepeat()
        {
            int lineStart = line;
            
            if ( CheckNext("(") == false )
            {
                error = "( must follow repeat statement, syntax is repeat (max repeat expression) { }.";
                errorLine = lineStart;
                return;
            }

            if ( ProcessExpression() )
            {
                PopValue();
                
                cpu.AppendInstruction(new Instruction(OpCode.POPC));
            }
            int repeatAddress = cpu.CurrentAddress;

            if ( CheckNext("{") == false )
            {
                error = "{ must follow repeat statement, syntax is repeat (max repeat expression) { }.";
                errorLine = lineStart;
                return;
            }
            if ( ProcessExpression() )
            {
                cpu.AppendInstruction(new Instruction(OpCode.JCX, 0, 0, repeatAddress));
            }
        }

        private void CompileYield()
        {
            if ( CheckNext(";") == false )
            {
                error = "; must follow yield statement, syntax is yield;";
                errorLine = line;
                return;
            }
            SkipNext();
        }

        private void CompileUpdate()
        {
            int lineStart = line;
            
            if ( CheckNext("(") == false )
            {
                error = "( must follow update statement, syntax is update (time) { }.";
                errorLine = lineStart;
                return;
            }

            SkipNext();

            cpu.SetCompileType(true);

            string id = string.Empty;
            IdentifierType idType = IdentifierType.None;

            if ( GetIndentifier(out id, out idType) == false )
            {
                error = "expected update time after ( syntax is update(time) { }.";
                errorLine = lineStart;
                return;
            }

            if ( idType != IdentifierType.Number )
            {
                error = "expected update time after ( syntax is update(time) { }.";
                errorLine = lineStart;
                return;
            }

            float deltaTime = 0.0f;
            
            if ( id.IndexOf('.')>=0 )
            {
                float.TryParse(id, out deltaTime);
            }
            else
            {
                int I = 0;
                int.TryParse(id, out I);
                deltaTime = (float)I;
            }

            if ( CheckNext(")") == false )
            {
                error = "expected ) after update time syntax is update(time) { }.";
                errorLine = lineStart;
                return;
            }

            SkipNext();

            if ( deltaTime<.005f )
            {
                error = "The time value must be >= .005 seconds, syntax is update(time) { }.";
                errorLine = lineStart;
                return;
            }

            cpu.AddUpdate(deltaTime, cpu.CurrentAddress);

            if ( CheckNext("{") == false )
            {
                error = "{ must follow update statement, syntax is update (time) { }.";
                errorLine = lineStart;
                cpu.SetCompileType(false);
                return;
            }
            if ( ProcessExpression() )
            {
                cpu.AppendInstruction(new Instruction(OpCode.RFU));
            }
            cpu.SetCompileType(false);
        }

        private void CompileWhen()
        {
            int lineStart = line;

            if ( CheckNext("(") == false )
            {
                error = "( must follow when statement, syntax is when (condition) { }.";
                errorLine = lineStart;
                return;
            }

            //when is an interrupt function called at periodic intervals. If the condition
            //evalues to true then the code between the { } is run.
            cpu.SetCompileType(true);
            int whenCondition = cpu.CurrentAddress;
            compilingWhenCondition = true;
            whenIndex = cpu.AddWhen(whenCondition);
            compilingWhen = true;
            if ( ProcessExpression() )
            {
                compilingWhenCondition = false;

                PopValue();

                int condCheckAddr;
                cpu.AppendInstruction(new Instruction(OpCode.POPA));
                condCheckAddr = cpu.AppendInstruction(new Instruction(OpCode.JF, 0, 0, -1));

                if ( CheckNext("{") == false )
                {
                    error = "{ must follow when statement, syntax is when (condition) { }.";
                    errorLine = lineStart;
                    cpu.SetCompileType(false);
                    compilingWhenCondition = false;
                    compilingWhen = false;
                    return;
                }
                if ( ProcessExpression() )
                {
                    cpu.ReplaceInstruction(condCheckAddr, new Instruction(OpCode.JF, CPU.ACC1, 0, cpu.CurrentAddress));
                    cpu.AppendInstruction(new Instruction(OpCode.RTI));
                }
            }
            compilingWhen = false;
            cpu.SetCompileType(false);
        }
        
        private void PushValue( string str, IdentifierType idType )
        {
            string id = "$" + str;

            Value value;
            
            switch( idType )
            {
                case IdentifierType.Number:
                    if ( id.IndexOf('.')>=0 )
                    {
                        float F = 0.0f;
                        float.TryParse(str, out F);
                        value = new Value(F, id);
                    }
                    else
                    {
                        int I = 0;
                        int.TryParse(str, out I);
                        value = new Value(I, id);
                    }
                    cpu.AppendInstruction(new Instruction(OpCode.PUSHI, value));
                    break;
                case IdentifierType.Bool:
                    value = (str == "false") ? new Value(false, id) : new Value(true, id);
                    cpu.AppendInstruction(new Instruction(OpCode.PUSHI, value));
                    break;
                case IdentifierType.Empty:
                    value = new Value();
                    value.T = Value.ValueType.Empty;
                    cpu.AppendInstruction(new Instruction(OpCode.PUSHI, value));
                    break;
                case IdentifierType.String:
                    value = new Value(str, id);
                    cpu.AppendInstruction(new Instruction(OpCode.PUSHI, value));
                    break;
            }

            localValuesStack.Push(CPU.ACC1);
        }
        
        private bool ProcessOperator( string id, ref Stack<Operator> opStack )
        {
            bool exit = false;
            
            int precedence = operators[id].precedence;
    
            //Preinc and Predec are never pushed as these simply increment
            //the next variable.        
            switch( id )
            {
                case "!":
                    NegateOperator(opStack);
                    break;
                case "++":
                    PreIncrement(opStack);
                    break;
                case "--":
                    PreDecrement(opStack);
                    break;
                case "(":
                    opStack.Push(new Operator(operators["("]));
                    break;
                case "{":
                    opStack.Push(new Operator(operators["{"]));
                    break;
                case "[":
                    opStack.Push(new Operator(operators["["]));
                    break;
                default:
                    if ( id == "," )
                    {
                        while( opStack.Count > 0 && opStack.Peek().symbol != "(" )
                        {
                            Operator op = opStack.Pop();
                            op.handler();
                        }
                        if ( opStack.Peek().symbol != "(" )
                        {
                            error = "Missing (";
                            errorLine = line;
                            exit = true;
                            break;
                        }
                    }
                    else if ( id == ")" || id == "}" || id == "]" )
                    {
                        string openSymbol = (id == ")") ? "(" : (id == "}") ? "{" : "[";
                        
                        while( opStack.Count > 0 && opStack.Peek().symbol != openSymbol )
                        {
                            Operator op = opStack.Pop();
                            op.handler();
                        }
                        if ( opStack.Count == 0 || opStack.Peek().symbol != openSymbol )
                        {
                            error = "Missing " + openSymbol;
                            errorLine = line;
                            exit = true;
                            break;
                        }
                        opStack.Pop();
                        if ( opStack.Count == 0 )
                        {
                            exit = true;
                        }
                    }
                    else
                    {
                        while( opStack.Count > 0 && opStack.Peek().precedence > precedence )
                        {
                            Operator op = opStack.Pop();
                            op.handler();
                        }
                        if ( id != ";" )
                        {
                            opStack.Push(new Operator(operators[id]));
                        }
                        if ( opStack.Count == 0 )
                        {
                            if ( id != ";" )
                            {
                                error = "Missing ;";
                                errorLine = line;
                                exit = true;
                                break;
                            }
                            exit = true;
                        }
                    }
                    break;
            }

            return exit;
        }

        private string GetLocalName( string id, int braces, string functionName = "" )
        {
            string name = string.Empty;

            if ( braces == 0 )
            {
                name = "~" + currentFile + functionName + "main_";
            }
            else
            {
                name = "~" + currentFile + functionName + braces.ToString();
                
                if ( compilingWhen == true && compilingWhenCondition == false)
                {
                    name = name + whenIndex.ToString() + "_";
                }
            }
            
            
            name = name + id;
            
            return name;
        }
        
        private bool IsLocalVariable( string id, out string localId )
        {
            bool rc = false;

            localId = string.Empty;

            localId = GetLocalName(id, 0);

            if ( localVariableInfo.ContainsKey(localId) )
            {
                rc = true;
            }
            else
            {
                for( int ii=braces; ii>0; --ii )
                {
                    localId = GetLocalName(id, ii);
                    if ( localVariableInfo.ContainsKey(localId) )
                    {
                        rc = true;
                        break;
                    }
                }
            }
            if ( rc == false )
            {
                localId = GetLocalName(id, 0, functionName);
                
                if ( localVariableInfo.ContainsKey(localId) )
                {
                    rc = true;
                }
            }
            
            return rc;
        }

        //needs to be recursive so v.other.stuff works
        private int GetCustomProperty(string id, int lineStart)
        {
            string p = id;
            int index = p.LastIndexOf('.');
            
            if ( index < 0 )
            {
                return 0;
            }
            
            p = p.Substring(index+1);
            
            if ( Variables.CustomPropertyExist(p) == false )
            {
                return 0;
            }
            
            if ( CheckNext("[") )
            {
                error = "Custom properties cannot be arrays.";
                errorLine = lineStart;
                return 0;
            }
            
            return Variables.CustomPropertyIndex(p);
        }

        //variable variable[0] variable.px variable[0].px

        private void PushVariable( string id, int lineStart, VariableInfo vInfo )
        {
            if ( vInfo == null )
            {
                error = "Variable " + id + " does not exist.";
                errorLine = lineStart;
                return;
            }
            
            int customProperty = 0;

            if ( CheckNext("[") )
            {
                if ( ProcessExpression() == false )
                {
                    return;
                }
                
                //array index has now been pushed onto the stack.

                //custom property if it exists is after the index.
                string next = CheckNext();
                if ( next[0] == '.' )
                {
                    SkipNext();
                    if ( GetIndentifier(out next) == false )
                    {
                        error = "Custom property " + next + " does not exist.";
                        errorLine = lineStart;
                        return;
                    }
                    next = "." + next;
                    customProperty = GetCustomProperty(next, lineStart);
                    if ( errorLine != 0 )
                    {
                        return;
                    }
                    if ( customProperty == 0 )
                    {
                        error = "Custom property " + next + " does not exist.";
                        errorLine = lineStart;
                        return;
                    }
                    //@@@@@ Recurse here? v.other.active = false;
                    //game variables need reset to initial positions on run.
                }
            }
            else
            {
                //no array so custom property is at the end of id if it exists at all
                customProperty = GetCustomProperty(id, lineStart);
                if ( errorLine != 0 )
                {
                    return;
                }
                
                //just variable name becomes variable[1] while variable[0] now means the whole array.
                cpu.AppendInstruction(new Instruction(OpCode.PUSHI, new Value(1, "$1")));
                localValuesStack.Push(1);
            }

            bool isMathAssignment = CheckNext("+=") || CheckNext("-=") || CheckNext("*=") || CheckNext("/=")
             || CheckNext("%=") || CheckNext("&=") || CheckNext("^=");
             
            bool isReference = isMathAssignment | CheckNext("=");
            
            //address is overloaded with the field
            cpu.AppendInstruction(new Instruction(OpCode.PUSHV, vInfo.index, vInfo.arrayIndex, customProperty, isReference));
            
            cpu.AppendInstruction(new Instruction(OpCode.MOVA));
            if ( isMathAssignment )
            {
                cpu.AppendInstruction(new Instruction(OpCode.DNR));
                localValuesStack.Push(vInfo.index);
            }
        }
        
        private bool ProcessUserFunction( string id, ref IdentifierType idType )
        {
            bool rc = false;

            if ( Variables.Exists(id) )
            {
                Value v = Variables.Read(id, 0);
                
                if ( v.T == Value.ValueType.UserFunction )
                {
                    CompileUserFunction(id);
                    cpu.AppendInstruction(new Instruction(OpCode.POPA));
                    rc = true;
                    lastVariableName = string.Empty;
                    idType = IdentifierType.UserFunction;
                    
                    if ( compilingWhenCondition )
                    {
                        cpu.AddWhenVariableInfo(whenIndex, true, 0, 0, 0);
                    }
                    rc = true;
                }
            }
            return rc;
        }
        
        private bool ProcessVariable( string id, ref IdentifierType idType )
        {
            int lineStart = line;
            bool rc = false;
            string localId = string.Empty;
            
            string vid = id;
            
            int customProperty = GetCustomProperty(id, lineStart);
            if ( errorLine != 0 )
            {
                return true;    //force error to pass though.
            }
            if ( customProperty > 0 )
            {
                int index = id.LastIndexOf('.');
                vid = id.Substring(0, index);
                //Currently only the other custom property acts like
                //a variable, this may be expanded to include any
                //property with an appropriate syntax in version 2.0.
                if ( GetCustomProperty(vid, lineStart) > 0 )
                {
                    error = "Sub properties are not allowed. Assign " + vid + " to a local variable in order to access the variables sub property.";
                    errorLine = lineStart;
                    return true;    //force error to pass though.
                }
            }
            
            if ( IsLocalVariable(vid, out localId) )
            {
                VariableInfo vInfo = GetVariableInformation(vid);
                if ( vInfo == null )
                {
                    error = "Variable " + id + " does not exist.";
                    errorLine = lineStart;
                }
                else
                {
                    PushVariable(id, lineStart, vInfo);
                    idType = IdentifierType.Variable;
                    lastVariableName = vid;
                    if ( compilingWhenCondition )
                    {
                        cpu.AddWhenVariableInfo(whenIndex, false, vInfo.index, vInfo.arrayIndex, customProperty);
                    }
                    rc = true;
                }
            }
            else if ( Variables.Exists(vid) )
            {
                Value v = Variables.Read(vid, 0);

                if ( v.T == Value.ValueType.UserFunction )
                {
                    CompileUserFunction(id);
                    cpu.AppendInstruction(new Instruction(OpCode.POPA));
                    rc = true;
                    lastVariableName = string.Empty;
                    idType = IdentifierType.UserFunction;
                    
                    if ( compilingWhenCondition )
                    {
                        cpu.AddWhenVariableInfo(whenIndex, true, 0, 0, customProperty);
                    }
                    rc = true;
                }
                else
                {
                    VariableInfo vInfo = GetVariableInformation(vid);
                    if ( vInfo == null )
                    {
                        error = "Variable " + id + " does not exist.";
                        errorLine = lineStart;
                    }
                    else
                    {
                        PushVariable(id, lineStart, vInfo);
                        idType = IdentifierType.Variable;
                        lastVariableName = id;
                        if ( compilingWhenCondition )
                        {
                            cpu.AddWhenVariableInfo(whenIndex, false, vInfo.index, vInfo.arrayIndex, customProperty);
                        }
                        rc = true;
                    }
                }
            }

            return rc;
        }

        //op is used when evaluating an expression that requires a closing
        //operator but the beginning open operator has already been
        //consumed. Currently this is only used in the for() statement.

        private bool ProcessExpression( Operator op = null )
        {
            int lineStart = line;

            string id = string.Empty;
            bool exit = false;

            Stack<Operator> opStack = new Stack<Operator>();
            if ( op != null )
            {
                opStack.Push(op);
            }

            IdentifierType idType = IdentifierType.None;
            
            while( errorLine == 0 && exit == false && GetIndentifier(out id, out idType) )
            {
                switch( idType )
                {
                    case IdentifierType.Number:
                        if ( idTypeLast == IdentifierType.PostOperation )
                        {
                            error = "Invalid operation after post increment or post decrement operation.";
                            errorLine = lineStart;
                            break;
                        }
                        PushValue(id, idType);
                        lastVariableName = string.Empty;
                        break;
                    case IdentifierType.String:
                        if ( idTypeLast == IdentifierType.PostOperation )
                        {
                            error = "Invalid operation after post increment or post decrement operation.";
                            errorLine = lineStart;
                            break;
                        }
                        PushValue(id, idType);
                        lastVariableName = string.Empty;
                        break;
                    case IdentifierType.Bool:
                        if ( idTypeLast == IdentifierType.PostOperation )
                        {
                            error = "Invalid operation after post increment or post decrement operation.";
                            errorLine = lineStart;
                            break;
                        }
                        PushValue(id, idType);
                        lastVariableName = string.Empty;
                        break;
                    case IdentifierType.Word:
                        if ( idTypeLast == IdentifierType.PostOperation )
                        {
                            error = "Invalid operation after post increment or post decrement operation.";
                            errorLine = lineStart;
                            break;
                        }
                        if ( statements.ContainsKey(id) )
                        {
                            statements[id]();
                            idType = IdentifierType.Statement;
                            lastVariableName = string.Empty;
                        }
                        else if ( scriptFunctions.ContainsKey(id) )
                        {
                            lastVariableName = string.Empty;
                            //Script functions are special in that
                            //they may return a result but not use the result.
                            CompileScriptFunctionCall(id);
                            if ( compilingWhenCondition )
                            {
                                cpu.AddWhenVariableInfo(whenIndex, true, 0, 0, 0);
                            }
                            if ( opStack.Count == 0 )
                            {
                                cpu.AppendInstruction(new Instruction(OpCode.POPA));
                            }
                            localValuesStack.Push(CPU.ACC1);
                        }
                        else if ( ProcessUserFunction(id, ref idType) )
                        {
                            cpu.AppendInstruction(new Instruction(OpCode.PUSHA));
                            localValuesStack.Push(CPU.ACC1);
                        }
                        else
                        {
                            if ( ProcessVariable(id, ref idType) == false )
                            {
                                error = "Unknown identifier " + id + " encountered.";
                                errorLine = lineStart;
                            }
                        }
                        break;
                    case IdentifierType.Operator:
                        if ( id == "=" && idTypeLast != IdentifierType.Variable )
                        {
                            error = "Cannot assign a value to a non-variable type.";
                            errorLine = lineStart;
                            break;
                        }
                        if ( id == "++" || id == "--" )
                        {
                            if ( idTypeLast == IdentifierType.PostOperation )
                            {
                                error = "Invalid operation after post increment or post decrement operation.";
                                errorLine = lineStart;
                                break;
                            }
                            if ( idTypeLast == IdentifierType.Variable )
                            {
                                //post increment or decrement current variable value is already on the stack.
                                VariableInfo vInfo = GetVariableInformation(lastVariableName);
                                if ( vInfo == null )
                                {
                                    error = "Variable " + id + " does not exist.";
                                    errorLine = lineStart;
                                    break;
                                }
                                if ( id == "++" )
                                {
                                    cpu.AppendInstruction(new Instruction(OpCode.INC, vInfo.index, vInfo.arrayIndex, vInfo.field, false));
                                }
                                else
                                {
                                    cpu.AppendInstruction(new Instruction(OpCode.DEC, vInfo.index, vInfo.arrayIndex, vInfo.field, false));
                                }
                                idType = IdentifierType.PostOperation;
                                break;
                            } //else this is a pre-increment so is processed normally.
                        }
                        if ( id == "[" )
                        {
                            squareBraces++;
                        }
                        if ( id == "]" )
                        {
                            squareBraces--;
                            if ( squareBraces<0 )
                            {
                                error = "Missing open [";
                                errorLine = lineStart;
                            }
                        }
                    
                        if ( id == "{" )
                        {
                            braces++;
                        }
                        if ( id == "}" )
                        {
                            foreach( KeyValuePair<string, VariableInfo> vInfo in localVariableInfo )
                            {
                                if ( vInfo.Value.braces == braces )
                                {
                                    localVariableInfo[vInfo.Key].inuse = false;
                                }
                            }

                            braces--;

                            if ( braces<0 )
                            {
                                error = "Missing open {";
                                errorLine = lineStart;
                            }
                        }
                        exit = ProcessOperator(id, ref opStack);
                        break;
                    case IdentifierType.Comment:
                        break;
                    case IdentifierType.Other:
                        error = "Unknown character " + id + " encountered.";
                        errorLine = lineStart;
                        break;
                }
                idTypeLast = idType;
                idLast = id;
            }
            return errorLine == 0;
        }

        /// <summary>
        /// Checks if the next identifier to be lexed is the same as id.
        /// </summary>
        /// <returns><c>true</c>, if the next identifier to be parsed is the same as id, <c>false</c> otherwise.</returns>
        /// <param name="id">Identifier to be checked.</param>
        private bool CheckNext( string id )
        {
            string identifier = string.Empty;

            int current = index;

            GetIndentifier(out identifier);
            
            index = current;

            return identifier == id;
        }
        
        /// <summary>
        /// returns the next identifier but does not advance the pointer.
        /// </summary>
        /// <returns><c>next identifier</c>, returns the next identifier to be encountered in the input stream.</returns>
        private string CheckNext( )
        {
            string id = string.Empty;
            
            int current = index;
            
            GetIndentifier(out id);
            
            index = current;
            
            return id;
        }
            
        /// <summary>
        /// Skips the next identifier.
        /// </summary>
        /// <returns><c>true</c>, if at the end of the script, <c>false</c> otherwise.</returns>
        /// <param name="script">Script.</param>
        /// <param name="index">Index.</param>
        private bool SkipNext()
        {
            string identifier = string.Empty;
            
            GetIndentifier(out identifier);
            
            return index == script.Length;
        }
        
        char GetNext()
        {
            char ch = '\0';

            if ( index<script.Length )
            {
                ch = script[index++];
            }
            return ch;
        }
        
        char PrevChar()
        {
            char ch = '\0';

            if ( index>1 )
            {
                return script[index - 2];
            }

            return ch;
        }
        
        private bool GetIndentifier( out string id )
        {
            IdentifierType idType = IdentifierType.None;
            
            return GetIndentifier(out id, out idType);
        }
        
        /// <summary>
        /// Gets an identifier from the string. An identifier is defined as a set of
        /// non white space characters, that can be either numbers, symbols or alphanumerics.
        /// The first character of the identifier determines what type it is. If the identifier
        /// begins with a number, then a number is returned. Special case is made for the period
        /// symbol as that determines integer or float. If the first character isn't a symbol or
        /// other identified type, 
        /// </summary>
        /// <returns><c>true</c>, if indentifier was returned, <c>false</c> if the end of the string was reached.</returns>
        /// <param name="id">returned Identifier if one was found.</param>
        private bool GetIndentifier( out string id, out IdentifierType idType )
        {
            idType = IdentifierType.None;
            id = string.Empty;
            
            if ( index<script.Length )
            {
                char ch = GetNext();
                
                if ( Char.IsWhiteSpace(ch) )
                {
                    while( index < script.Length && Char.IsWhiteSpace(ch) )
                    {
                        if ( ch == '\n' )
                        {
                            line++;
                        }
                        ch = script[index++];
                    }
                }
                if ( index<script.Length )
                {
                    id = ch.ToString();
                    
                    bool isNumber = false;
                    if ( ch == '-' || ch == '+' )
                    {
                        char ch1 = script[index];
                        if ( idTypeLast == IdentifierType.Operator && idLast != ")" && idLast != "]" && (Char.IsDigit(ch1) || ch1 == '.') )
                        {
                            isNumber = true;
                        }
                    }
                    else if ( Char.IsDigit(ch) )
                    {
                        isNumber = true;
                    }
                    else if ( ch == '.' )
                    {
                        if ( Char.IsLetterOrDigit(PrevChar()) == false && PrevChar() != '_' )
                        {
                            isNumber = true;
                        }
                    }
                    if ( isNumber )
                    {
                        idType = IdentifierType.Number;
                        
                        while( index < script.Length )
                        {
                            ch = script[index];
                            if ( Char.IsDigit(ch) == false && ch != '.' )
                            {
                                break;
                            }
                            id += ch;
                            index++;
                        }
                        if ( index<script.Length && script[index] == 'f' )
                        {
                            index++;
                        }
                    }
                    else if ( ch == '_' || Char.IsLetter(ch) || ch == '.' )
                    {
                        idType = IdentifierType.Word;
                        
                        while( index < script.Length )
                        {
                            ch = script[index];
                            if ( Char.IsLetterOrDigit(ch) == false && ch != '_' && ch != '.' )
                            {
                                break;
                            }
                            id += ch;
                            index++;
                        }
                        if ( id == "empty" )
                        {
                            idType = IdentifierType.Empty;
                        }
                        if ( id == "false" || id == "true" )
                        {
                            idType = IdentifierType.Bool;
                        }
                    }
                    else if ( ch == '\"' )
                    {
                        idType = IdentifierType.String;
                        id = string.Empty;
                        
                        while( index < script.Length )
                        {
                            ch = script[index++];
                            if ( ch == '\"' )
                            {
                                break;
                            }
                            if ( ch == '\n' )
                            {
                                line++;
                            }
                            if ( ch == '\\' )
                            {
                                if ( index<script.Length )
                                {
                                    ch = script[index];
                                    ch = (ch == 't') ? '\t' : ch;
                                    ch = (ch == 'r') ? '\r' : ch;
                                    ch = (ch == 'n') ? '\n' : ch;
                                    index++;
                                }
                            }
                            id += ch;
                        }
                    }
                    else if ( ch == '/' && (index<script.Length && script[index] == '/') )
                    {
                        idType = IdentifierType.Comment;
                        index++;
                        id = string.Empty;
                        while( index < script.Length && script[index] != '\r' && script[index] != '\n' )
                        {
                            index++;
                        }
                        if ( index<script.Length && script[index] == '\r' )
                        {
                            index++;
                        }
                        if ( index<script.Length && script[index] == '\n' )
                        {
                            line++;
                            index++;
                        }
                    }
                    else if ( ch == '/' && (index<script.Length && script[index] == '*') )
                    {
                        idType = IdentifierType.Comment;
                        index++;
                        while( index < script.Length )
                        {
                            if ( script[index] == '\n' )
                            {
                                line++;
                            }
                            if ( script[index] == '*' )
                            {
                                if ( index + 1<script.Length && script[index + 1] == '/' )
                                {
                                    break;
                                }
                            }
                            index++;
                        }
                        
                        if ( script[index] == '*' && index + 1<script.Length && script[index + 1] == '/' )
                        {
                            index += 2;
                        }
                    }
                    else
                    {
                        string id2 = string.Empty;
                        if ( index<script.Length )
                        {
                            id2 = id + script[index];
                        }
                        if ( id2 != string.Empty && operators.ContainsKey(id2) )
                        {
                            index++;
                            id = id2;
                            idType = IdentifierType.Operator;
                        }
                        else if ( operators.ContainsKey(id) )
                        {
                            idType = IdentifierType.Operator;
                        }
                        else
                        {
                            idType = IdentifierType.Other;
                        }
                    }
                }
            }
            return (index<script.Length);
        }
    }
}