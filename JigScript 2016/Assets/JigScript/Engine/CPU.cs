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
using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class CPU: MonoBehaviour
    {
        internal class UpdateInformation
        {
            public int address;
            public float deltaTime;
            public float nextUpdate;
            public bool running;
            
            public UpdateInformation( float deltaTime, int address )
            {
                this.address = address;
                this.deltaTime = deltaTime;
                this.nextUpdate = deltaTime + Time.time;
                this.running = false;
            }
        };
        
        internal class WhenInformation
        {
            public int address;
            private List < Value > values;
            public bool changed;
            public bool function;
            public bool running;
            public float count;
            
            public WhenInformation( int address )
            {
                this.address = address;
                this.values = new List < Value >();
                this.changed = false;
                this.function = false;
                this.running = false;
                this.count = 0;
            }
            
            public void AddVariableInformation( int index, int arrayIndex, int customProperty )
            {
                Value v = Variables.Read(index, arrayIndex);
                v.field = customProperty;
                values.Add(v);
            }
            
            public void SetWhenFunctionType( int index )
            {
                function = true;
            }
            
            /// <summary>
            /// Decrements the changed count for the variables that are part of this when statement.
            /// </summary>
            public void DecrementChangedCount()
            {
                if ( this.function == false )
                {
                    for( int ii = 0; ii < values.Count; ++ii )
                    {
                        if ( values[ii].isVariable == false )
                        {
                            continue;
                        }
                        Variables.DecrementChangedCount(values[ii]);
                    }
                }
            }
            
            public void Update()
            {
                if ( this.function )
                {
                    this.changed = true;
                }
                else
                {
                    for( int ii = 0; ii < values.Count; ++ii )
                    {
                        if ( values[ii].isVariable == false )
                        {
                            continue;
                        }

                        List <Value> va = Variables.Expand(values[ii], false);
                        for(int tt=1; tt<va.Count; ++tt)
                        {
                            if ( va[tt].changed > 0 || Variables.IsPollingProperty(va[tt].field) )
                            {
                                this.changed = true;
                                break;
                            }
                        }
                    }
                }
            }
        };
        
        public static bool whensNeeded = false;
        public const int ACC1 = 0; //AX
        
        private bool exitNow = false; //if true then the program exists immediately.
        
        public List < Instruction > program
        {
            get;
            private set;
        }

        public List < Instruction > functions
        {
            get;
            private set;
        }
        
        public Dictionary < int, List < int >> scriptFunctionParameterInfo;
        private bool isFunction;
        private List < WhenInformation > whenInformation;
        private List < UpdateInformation > updateInformation;
        private bool stopPlaying = false;
        public bool isPlaying { get; private set; }
        public const int functionsBase = 1000000000;
        
        public void SetCompileType( bool isFunction )
        {
            this.isFunction = isFunction;
        }
        
        public void Awake()
        {
            program = new List < Instruction >();
            functions = new List < Instruction >();
            whenInformation = new List < WhenInformation >();
            updateInformation = new List < UpdateInformation >();
            
            scriptFunctionParameterInfo = new Dictionary < int, List < int >>();
            
            isFunction = false;
            
            stopPlaying = true;
            isPlaying = false;
            
            ResetCPU();
        }
        
        public IEnumerator ResetCPU()
        {
            stopPlaying = true;
            while( isPlaying )
            {
                yield
                    return new WaitForEndOfFrame();
            }
            
            whenInformation.Clear();
            updateInformation.Clear();
            
            program.Clear();
            functions.Clear();
            scriptFunctionParameterInfo.Clear();
            
            exitNow = false;
        }
        
        public int CurrentAddress
        {
            get
            {
                if ( isFunction )
                {
                    return functions.Count + functionsBase;
                }
                else
                {
                    return program.Count;
                }
            }
        }
        
        public void ReplaceInstruction( int address, Instruction ins )
        {
            if ( isFunction )
            {
                functions[address - functionsBase] = ins;
            }
            else
            {
                program[address] = ins;
            }
        }
        
        public int AppendInstruction( Instruction ins )
        {
            int address;
            
            if ( isFunction )
            {
                address = functions.Count + functionsBase;
                functions.Add(ins);
            }
            else
            {
                address = program.Count;
                program.Add(ins);
            }
            return address;
        }
        
        public void AddFunctionParameter( int functionAddress, int index )
        {
            if ( scriptFunctionParameterInfo.ContainsKey(functionAddress) == false )
            {
                scriptFunctionParameterInfo.Add(functionAddress, new List < int >());
            }
            scriptFunctionParameterInfo[functionAddress].Add(index);
        }
        
        public void Run()
        {
            StartCoroutine("InternalRun");
        }
        
        public void ExitNow()
        {
            exitNow = true;
        }
        
        private string FormatOperand( Instruction ins )
        {
            string s = string.Empty;
            
            if ( ins.code == OpCode.PUSHI )
            {
                Value v1 = new Value(ins.v);
                v1.ConvertTo(Value.ValueType.String);
                s += v1.S;
            }
            else
            {
                string name = Variables.Name(ins.operand1);
                if ( name[0] == '$' || name[0] == '#' || name[0] == '~' )
                {
                    if ( name == "#Zero" )
                    {
                        s = "0";
                    }
                    else
                    {
                        s = name.Substring(1);
                    }
                }
                else
                {
                    s = name;
                    if ( ins.arrayIndex>0 )
                    {
                        s += "[" + ins.arrayIndex + "]";
                    }
                }
                if ( ins.field > 0 )
                {
                    s += "." + Variables.CustomPropertyName(ins.field);
                }
            }
            
            return s;
        }
        
        private string FormatInstructionValue( Instruction ins )
        {
            string s = string.Empty;
            
            ins.v.ConvertTo(Value.ValueType.String);
            s = ins.v.S;
            return s;
        }
        
        private string DissassembleInstruction( int offset, int ic, Instruction ins )
        {
            string s = string.Empty;
            
            if ( ins.code>OpCode.GROUP1 && ins.code<OpCode.GROUP2 )
            { //opcode only group
                s += (offset + ic).ToString("0000") + " " + ins.code.ToString();
            }
            else if ( ins.code>OpCode.GROUP2 && ins.code<OpCode.GROUP3 )
            { //arithmetic group
                s += (offset + ic).ToString("0000") + " " + ins.code.ToString() + ";   POP 2 BX AX, PUSH AX";
            }
            else if ( ins.code>OpCode.GROUP3 && ins.code<OpCode.GROUP4 )
            { //one variable or value group
                s += (offset + ic).ToString("0000") + " " + ins.code.ToString() + " " + FormatOperand(ins);
            }
            else if ( ins.code>OpCode.GROUP4 && ins.code<OpCode.GROUP5 )
            { //address group
                s += (offset + ic).ToString("0000") + " " + ins.code.ToString() + " ";
                if ( ins.code == OpCode.CALL )
                {
                    s += Variables.Name(ins.address);
                }
                else
                {
                    s += ins.address.ToString("0000");
                }
            }
            else if ( ins.code>OpCode.GROUP5 && ins.code<OpCode.GROUP6 )
            { //move group
                switch( ins.code )
                {
                    case OpCode.MOVM:
                        s += (offset + ic).ToString("0000") + " MOV [AX], BX";
                        break;
                    case OpCode.MOVA:
                        s += (offset + ic).ToString("0000") + " MOV AX, [BX][IX];   POP 2 BX IX, PUSH AX";
                        break;
                    case OpCode.LDA:
                        s += (offset + ic).ToString("0000") + " LDA " + FormatInstructionValue(ins);
                        break;
                    case OpCode.LDB:
                        s += (offset + ic).ToString("0000") + " LDB " + FormatInstructionValue(ins);
                        break;
                    case OpCode.LDI:
                        s += (offset + ic).ToString("0000") + " LDI " + FormatInstructionValue(ins);
                        break;
                }
            }
            s += "\r\n";
            return s;
        }
        
        public string Dissassemble()
        {
            string s = string.Empty;
            
            for( int ii = 0; ii < program.Count; ++ii )
            {
                Instruction ins = new Instruction(program[ii]);
                s += DissassembleInstruction(0, ii, ins);
            }
            
            for( int ii = 0; ii < functions.Count; ++ii )
            {
                Instruction ins = new Instruction(functions[ii]);
                s += DissassembleInstruction(1000000000, ii, ins);
            }
            
            return s;
        }
        
        IEnumerator CopyArray( Value to, Value from )
        {
            Variables.Copy(to, from);
            yield
                return 0;
        }
        
        public void ProcessWhens()
        {
            UpdateWhens(true);
            if ( isPlaying )
            {
                if ( WhensNeeded(true) )
                {
                    if ( isPlaying )
                    {
                        StartCoroutine(ProcessWhens(true));
                    }
                }
            }
        }
                
        private void UpdateWhens(bool whensEnabled)
        {
            if ( whensEnabled == false )
            {
                return;
            }
            
            for( int ii = 0; ii < whenInformation.Count; ++ii )
            {
                if( CPU.whensNeeded )
                {
                    whenInformation[ii].changed = true;
                }
                else
                {
                    whenInformation[ii].Update();
                }
            }
        }

        public bool WhensNeeded(bool whensEnabled)
        {
            bool rc = false;
            if ( whensEnabled )
            {
                for( int ii = 0; ii < whenInformation.Count; ++ii )
                {
                    if ( whenInformation[ii].changed )
                    {
                        rc = true;
                        break;
                    }
                }
            }            
            return rc;
        }

        public IEnumerator ProcessWhens(bool whensEnabled)
        {
            if ( whensEnabled )
            {
                for( int ii = 0; ii < whenInformation.Count; ++ii )
                {
                    if ( stopPlaying )
                    {
                        break;
                    }
                    if ( whenInformation[ii].running == true )
                    {
                        continue;
                    }
                    if ( whenInformation[ii].changed )
                    {
                        whenInformation[ii].changed = false;
                        whenInformation[ii].DecrementChangedCount();
                        whenInformation[ii].running = true;
                        StartCoroutine(Run1(whenInformation[ii].address, ii));
                    }
                }
                CPU.whensNeeded = false;
            }
            yield return 0;
        }
        
        private Value PopSafe( ref Stack < Value > valueStack )
        {
            return (valueStack.Count == 0) ? Variables.Read(0, 0) : valueStack.Pop();
        }
        
        private void PushSafe( ref Stack < Value > valueStack, Value v )
        {
            valueStack.Push(new Value(v));
        }
        
        public int AddWhen( int address )
        {
            int index = whenInformation.Count;
            
            whenInformation.Add(new WhenInformation(address));
            
            return index;
        }
        
        public void AddWhenVariableInfo( int whenIndex, bool isFunction, int index, int arrayIndex, int customProperty )
        {
            if ( isFunction )
            {
                whenInformation[whenIndex].function = true;
            }
            else if ( whenInformation[whenIndex].function == false )
            {
                whenInformation[whenIndex].AddVariableInformation(index, arrayIndex, customProperty);
            }
        }
        
        public int AddUpdate( float deltaTime, int address )
        {
            int index = updateInformation.Count;
            
            updateInformation.Add(new UpdateInformation(deltaTime, address));
            
            return index;
        }
        
        void MOVM( Value AX, Value BX )
        {
            if ( AX.arrayIndex == 0 )
            {
                if ( BX.isVariable )
                {
                    Variables.Copy(AX, BX);
                }
                else
                {
                    Variables.FillArray(AX, 0, BX);
                }
            }
            else if ( BX.arrayIndex == 0 && BX.isVariable == true )
            {
                BX = Variables.Read(BX.index, AX.arrayIndex, BX.field);
                Variables.Store(AX, AX.arrayIndex, BX);
            }
            else
            {
                Variables.Store(AX, AX.arrayIndex, BX);
            }
        }

        private Value ProcessMathCode( OpCode code, Value AX, Value BX )
        {
            switch( code )
            {
                case OpCode.ADD:
                    AX = AX + BX;
                    break;
                case OpCode.SUB:
                    AX = AX - BX;
                    break;
                case OpCode.MUL:
                    AX = AX * BX;
                    break;
                case OpCode.MOD:
                    AX = AX % BX;
                    break;
                case OpCode.DIV:
                    AX = AX / BX;
                    break;
                case OpCode.AND:
                    AX = AX & BX;
                    break;
                case OpCode.OR:
                    AX = AX | BX;
                    break;
                case OpCode.XOR:
                    AX = AX ^ BX;
                    break;
                case OpCode.SHR:
                    AX = AX>>BX.I;
                    break;
                case OpCode.SHL:
                    AX = AX<<BX.I;
                    break;
                case OpCode.LT:
                    AX = AX<BX;
                    break;
                case OpCode.GT:
                    AX = AX>BX;
                    break;
                case OpCode.LTE:
                    AX = AX<=BX;
                    break;
                case OpCode.GTE:
                    AX = AX>=BX;
                    break;
                case OpCode.EQ:
                    AX = AX == BX;
                    break;
                case OpCode.NEQ:
                    AX = AX != BX;
                    break;
                case OpCode.LAND:
                    AX = Value.LogicalAnd(AX, BX);
                    break;
                case OpCode.LOR:
                    AX = Value.LogicalOr(AX, BX);
                    break;
                case OpCode.INC:
                    AX.ConvertTo(Value.ValueType.Integer);
                    AX.I++;
                    break;
                case OpCode.DEC:
                    AX.ConvertTo(Value.ValueType.Integer);
                    AX.I--;
                    break;
                case OpCode.NEG:
                    //BX is ignored.
                    AX = Value.Negate(AX);
                    break;
            }
            return AX;
        }

        private Value ProcessMathCodes( OpCode code, Value AX, Value BX )
        {
            int count;
            
            count = Variables.Length(AX);
            if ( BX.isVariable == false )
            {
                for( int arrayIndex = 1; arrayIndex <= count; ++arrayIndex )
                {
                    if ( code >= OpCode.LOGICS && code <= OpCode.LOGICE )
                    {
                        Value vax = Variables.Read(AX.index, arrayIndex, AX.field);
                        AX = ProcessMathCode(code, vax, BX);
                        if ( AX.B == false )
                        {
                            break;
                        }
                    }
                    else
                    {
                        Value vax = Variables.Read(AX.index, arrayIndex, AX.field);
                        vax = ProcessMathCode(code, vax, BX);
                        Variables.Store(AX, arrayIndex, vax);
                    }
                }
            }
            else
            {
                for( int arrayIndex = 1; arrayIndex <= count; ++arrayIndex )
                {
                    if ( code >= OpCode.LOGICS && code <= OpCode.LOGICE )
                    {
                        Value vax = Variables.Read(AX.index, arrayIndex, AX.field);
                        Value vbx = Variables.Read(BX.index, arrayIndex, BX.field);
                        AX = ProcessMathCode(code, vax, vbx);
                        if ( AX.B == false )
                        {
                            break;
                        }
                    }
                    else
                    {
                        Value vax = Variables.Read(AX.index, arrayIndex, AX.field);
                        Value vbx = Variables.Read(BX.index, arrayIndex, BX.field);
                        vax = ProcessMathCode(code, vax, vbx);
                        Variables.Store(AX, arrayIndex, vax);
                    }
                        
                }
            }
            
            return AX;
        }
        
        IEnumerator InternalRun()
        {
            isPlaying = true;
            stopPlaying = false;
            
            yield
                return StartCoroutine(Run1(0, -1));
            
            isPlaying = false;
            
            yield
                return 0;
        }
        
        IEnumerator Run1( int ic, int index )
        {
            Value v1;
            Value AX = new Value();
            Value BX = new Value();
            Value IX = new Value();
            int CX = 0; //counting register
            int repeatCount = 0; //current count set to 0 when CX is set and incremented for each loop. Accessible in script.
            
            Stack < Value > valueStack = new Stack < Value >();
            Stack < int > icStack = new Stack < int >();
            Stack < List < Value >> parameters = new Stack < List < Value >>();
            
            bool exit = exitNow;
            int countPerYield = 0;
            
            while( exit == false && isPlaying == true && stopPlaying == false )
            {
                //yield only applies to main routine, basically this keeps
                //unity from pausing a long time if nothing else happens.
                
                if ( index == -1 )
                {
                    countPerYield++;
                    if ( countPerYield > 10000 )
                    {
                        countPerYield = 0;
                        yield return 0;
                        continue;
                    }
                }
                
                Instruction ins;
                
                if ( ic>=functionsBase )
                {
                    ins = functions[ic - functionsBase];
                }
                else
                {
                    ins = program[ic];
                }
                
                if ( isPlaying == false )
                {
                    break;
                }

                if ( index == -1 )
                {
                    v1 = Variables.Read("whens.enable", 1);
                    if ( v1.changed > 0 && v1.B )
                    {
                        Variables.ClearChanged();
                    }

                    UpdateWhens(v1.B);
                    if ( WhensNeeded(v1.B) )
                    {
                        if ( isPlaying )
                        {
                            StartCoroutine(ProcessWhens(v1.B));
                        }
                    }
                }
                
                if ( isPlaying == false )
                {
                    break;
                }
                    
                if ( index == -1 )
                {
                    for( int ii = 0; ii < updateInformation.Count; ++ii )
                    {
                        if ( isPlaying )
                        {
                            if ( updateInformation[ii].running )
                            {
                                continue;
                            }
                            if ( updateInformation[ii].nextUpdate<=Time.time )
                            {
                                updateInformation[ii].running = true;
                                updateInformation[ii].nextUpdate = updateInformation[ii].deltaTime + Time.time;
                                StartCoroutine(Run1(updateInformation[ii].address, ii));
                            }
                        }
                    }
                }

                if ( isPlaying == false )
                {
                    break;
                }
                //post inc dec only, pre inc dec is handled by adding or subtracting 1
                if ( ins.code == OpCode.INC || ins.code == OpCode.DEC )
                {
                    v1 = Variables.Read(ins.operand1, ins.arrayIndex);
                    Value v2 = new Value(v1);
                    AX = ProcessMathCode(ins.code, v1, v1);
                    if ( v1.arrayIndex == 0 )
                    {
                        AX = ProcessMathCodes(ins.code, v1, v1);
                        v2.I = AX.I + 1; //force changed for arrays
                    }
                    Variables.Store(v2, v2.arrayIndex, AX);
                }
                else if ( ins.code>OpCode.GROUP2 && ins.code<OpCode.GROUP3 )
                {
                    BX = PopSafe(ref valueStack);
                    v1 = PopSafe(ref valueStack);
                    
                    AX = ProcessMathCode(ins.code, v1, BX);
                    
                    if ( v1.isVariable == true && v1.arrayIndex == 0 )
                    {
                        AX = ProcessMathCodes(ins.code, v1, BX);
                    }
                    
                    PushSafe(ref valueStack, AX);
                }
                
                if ( ins.code == OpCode.END )
                {
                    if ( whenInformation.Count>0 || updateInformation.Count>0 )
                    {
                        if ( isPlaying == true )
                        {
                            yield return 0;
                            continue;
                        }
                    }
                    exit = true;
                    break;
                }
                
                ++ic;
                
                if ( ins.code == OpCode.RFU )
                {
                    exit = true;
                    if ( index>=0 && index<updateInformation.Count )
                    {
                        updateInformation[index].running = false;
                        updateInformation[index].nextUpdate = updateInformation[index].deltaTime + Time.time;
                    }
                    break;
                }
                
                if ( ins.code == OpCode.RTI )
                {
                    exit = true;
                    if ( index>=0 && index<whenInformation.Count )
                    {
                        whenInformation[index].running = false;
                        whenInformation[index].count = 0;
                    }
                    break;
                }

                switch( ins.code )
                {
                    case OpCode.NOP:
                        break;
                    case OpCode.POPI:
                        IX = new Value(PopSafe(ref valueStack));
                        IX.ConvertTo(Value.ValueType.Integer);
                        break;
                    case OpCode.DNR:
                        v1 = PopSafe(ref valueStack);
                        PushSafe(ref valueStack, v1);
                        v1 = Variables.Read(v1.index, v1.arrayIndex, v1.field);
                        PushSafe(ref valueStack, v1);
                        break;
                    case OpCode.MOVA:
                    //MOV AX, [BX][IX]
                        BX = PopSafe(ref valueStack);
                        IX = PopSafe(ref valueStack);

                        if ( BX.arrayIndex != IX.I )
                        {
                            BX.arrayIndex = IX.I;
                            if ( BX.isReference )
                            {
                                AX = Variables.Read(BX.index, BX.arrayIndex, 0);
                                AX.arrayIndex = BX.arrayIndex;
                                AX.field = BX.field;
                                AX.isReference = BX.isReference;
                            }
                            else
                            {
                                AX = Variables.Read(BX.index, BX.arrayIndex, BX.field);
                            }
                        }
                        else
                        {
                            if ( BX.isReference == false )
                            {
                                AX = Variables.Read(BX.index, BX.arrayIndex, BX.field);
                            }
                            else
                            {
                                AX = BX;
                            }
                        }
                    
                        PushSafe(ref valueStack, AX);
                        break;
                    case OpCode.MOVM:
                        //MOV [AX], BX
                        MOVM(AX, BX);
                        break;
                    case OpCode.LDA:
                        AX = new Value(ins.v);
                        break;
                    case OpCode.LDB:
                        BX = new Value(ins.v);
                        break;
                    case OpCode.LDI:
                        IX = new Value(ins.v);
                        break;
                    case OpCode.JCX:
                        if ( CX>0 )
                        {
                            --CX;
                            ++repeatCount;
                            ic = ins.address;
                        }
                        break;
                    case OpCode.JA:
                        ic = ins.address;
                        break;
                    case OpCode.JF:
                        AX.ConvertTo(Value.ValueType.Bool);
                        if ( AX.B == false )
                        {
                            ic = ins.address;
                        }
                        break;
                    case OpCode.JR:
                        v1 = PopSafe(ref valueStack);
                        int pc = v1.I;
                        while( --pc >= 0 )
                        {
                            if ( pc<scriptFunctionParameterInfo[ins.address].Count )
                            {
                                BX = PopSafe(ref valueStack);
                                AX = Variables.Read(scriptFunctionParameterInfo[ins.address][pc], 0);
                                MOVM(AX, BX);
                            }
                        }
                        icStack.Push(ic);
                        ic = ins.address;
                        break;
                    case OpCode.RET:
                        if ( icStack.Count != 0 )
                        {
                            ic = icStack.Pop();
                        }
                        else
                        {
                            exit = true;
                        }
                        break;
                    case OpCode.CPS:
                        parameters.Push(new List < Value >());
                        break;
                    case OpCode.PUSHP:
                        parameters.Peek().Add(new Value(AX));
                        break;
                    case OpCode.CALL:
                        countPerYield = 0;
                        List < Value > p = parameters.Pop();
                        p.Reverse();
                        v1 = Variables.Read(ins.address, 0);
                        if ( v1.T == Value.ValueType.UserFunction )
                        {
                            int count = p.Count;
                            yield return StartCoroutine(v1.U.fn(p));
                            if ( p.Count>count )
                            {
                                AX = p[count];
                            }
                            else
                            {
                                AX = new Value(0, "AX");
                            }
                            PushSafe(ref valueStack, AX);
                        }
                        break;
                    case OpCode.YIELD:
                        countPerYield = 0;
                        yield return 0;
                        break;
                    case OpCode.PUSHI:
                        PushSafe(ref valueStack, ins.v);
                        break;
                    case OpCode.PUSHV:
                        v1 = Variables.Read(ins.operand1, ins.arrayIndex);
                        v1.field = ins.field;
                        v1.isReference = ins.isReference;
                        PushSafe(ref valueStack, v1);
                        break;
                    case OpCode.PUSHA:
                        PushSafe(ref valueStack, AX);
                        break;
                    case OpCode.PRC:
                        AX.T = Value.ValueType.Integer;
                        AX.I = repeatCount;
                        PushSafe(ref valueStack, AX);
                        break;
                    case OpCode.POPC:
                        v1 = PopSafe(ref valueStack);
                        v1.ConvertTo(Value.ValueType.Integer);
                        CX = v1.I;
                        repeatCount = 0;
                        break;
                    case OpCode.POPA:
                        AX = PopSafe(ref valueStack);
                        break;
                    case OpCode.PUSHB:
                        PushSafe(ref valueStack, BX);
                        break;
                    case OpCode.POPB:
                        BX = PopSafe(ref valueStack);
                        break;
                    case OpCode.POP:
                        PopSafe(ref valueStack);
                        break;
                }
            }
        }
    };
}