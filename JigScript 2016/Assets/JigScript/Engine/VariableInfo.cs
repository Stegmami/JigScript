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
using System.Collections;

namespace NightPen.JigScript
{
    internal class VariableInfo
    {
        public string key { get; private set; }

        public bool isGlobal { get; private set; }

        public int index { get; private set; }

        public int arrayIndex { get; private set; }

        public bool inuse { get; set; }

        public int braces { get; set; }
        
        public int field { get; set; }

        public VariableInfo( string key, bool isGlobal, int index, int arrayIndex, bool inuse, int braces, int field )
        {
            this.key = key;
            this.isGlobal = isGlobal;
            this.index = index;
            this.arrayIndex = arrayIndex;
            this.inuse = inuse;
            this.braces = braces;
            this.field = field;
        }

        public VariableInfo( VariableInfo v )
        {
            this.key = v.key;
            this.isGlobal = v.isGlobal;
            this.index = v.index;
            this.arrayIndex = v.arrayIndex;
            this.inuse = v.inuse;
            this.braces = v.braces;
            this.field = v.field;
        }
    };
}