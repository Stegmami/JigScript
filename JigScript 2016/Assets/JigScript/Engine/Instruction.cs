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
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class Instruction
    {
        public OpCode code;
        public int operand1;
        public int arrayIndex;
        public int address;
        public int field;
        public bool isReference;
        public Value v;

        public Instruction( OpCode code )
        {
            this.code = code;
            this.operand1 = 0;
            this.arrayIndex = 0;
            this.address = 0;
            this.field = 0;
            this.v = null;
            this.isReference = false;
        }

        public Instruction( OpCode code, int operand1, int arrayIndex, int field, bool isReference )
        {
            this.code = code;
            this.operand1 = operand1;
            this.arrayIndex = arrayIndex;
            this.address = 0;
            this.v = null;
            this.field = field;
            this.isReference = isReference;
        }
            
        public Instruction( OpCode code, int operand1, int arrayIndex, int address )
        {
            this.code = code;
            this.operand1 = operand1;
            this.arrayIndex = arrayIndex;
            this.address = address;
            this.field = 0;
            this.v = null;
            this.isReference = false;
        }

        public Instruction( OpCode code, Value v )
        {
            this.code = code;
            this.operand1 = 0;
            this.arrayIndex = 0;
            this.address = 0;
            this.field = 0;
            this.v = v;
            this.isReference = false;
        }

        public Instruction( Instruction ins )
        {
            this.code = ins.code;
            this.operand1 = ins.operand1;
            this.arrayIndex = ins.arrayIndex;
            this.address = ins.address;
            this.field = ins.field;
            this.v = ins.v;
            this.isReference = ins.isReference;
        }
    };
}