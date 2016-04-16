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
    public class CustomProperty
    {
        public delegate Value GetFunction( Value dest );

        public delegate void SetFunction( Value dest, int arrayIndex, Value source );

        public string name { get; protected set; }

        public int fieldId { get; protected set; }

        public GetFunction getFn { get; protected set; }

        public SetFunction setFn { get; protected set; }
        
        public bool poll;
        
        public CustomProperty()
        {
            this.name = string.Empty;
            this.fieldId = 0;
            this.getFn = null;
            this.setFn = null;
        }
        
        public CustomProperty( string name, int field, GetFunction getFn, SetFunction setFn, bool poll = false )
        {
            this.name = name;
            this.fieldId = field;
            this.getFn = getFn;
            this.setFn = setFn;
            this.poll = poll;
        }
    };
}