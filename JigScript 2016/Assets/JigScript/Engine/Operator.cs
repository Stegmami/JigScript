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
    public class Operator
    {
        public string symbol { get; private set; }

        public int precedence { get; private set; }

        public delegate void Handler();

        public Handler handler { get; private set; }

        public Operator( string symbol, int precedence, Handler handler )
        {
            this.symbol = symbol;
            this.precedence = precedence;
            this.handler = handler;
        }
        
        public Operator( Operator op )
        {
            this.symbol = op.symbol;
            this.precedence = op.precedence;
            this.handler = op.handler;
        }
    };
}