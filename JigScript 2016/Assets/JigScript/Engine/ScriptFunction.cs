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
using System.Collections.Generic;

namespace NightPen.JigScript
{
    internal class ScriptFunction
    {
        public class Parameter
        {
            public string name { get; private set; }

            public int parameterVariableIndex { get; private set; }

            public int arrayIndex { get; private set; }
            
            public Parameter( string name, int index, int arrayIndex )
            {
                this.name = name;
                this.parameterVariableIndex = index;
                this.arrayIndex = arrayIndex;
            }
        };
        
        public string name { get; private set; }    //function name
        public List<Parameter> parameters { get; private set; } //cpu local value addresses
        public int cpuAddress { get; private set; }

        public int variableIndex { get; private set; }
        
        public ScriptFunction( ScriptFunction scriptFunction )
        {
            this.name = scriptFunction.name;
            this.cpuAddress = scriptFunction.cpuAddress;
            this.variableIndex = scriptFunction.variableIndex;
            this.parameters = new List<Parameter>();
            foreach( Parameter p in scriptFunction.parameters )
            {
                this.parameters.Add(p);
            }
        }
        
        public ScriptFunction( string name, int variableIndex, int cpuAddress, List<Parameter>parameters )
        {
            this.name = name;
            this.cpuAddress = cpuAddress;
            this.variableIndex = variableIndex;
            this.parameters = new List<Parameter>();
            foreach( Parameter p in parameters )
            {
                this.parameters.Add(p);
            }
        }
    };
}