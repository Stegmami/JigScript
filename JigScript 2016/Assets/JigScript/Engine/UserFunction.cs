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
using System;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class UserFunction : System.Object
    {
        public delegate IEnumerator Function( List<Value> values );

        public Function fn;
        public bool wait;

        public UserFunction( Function fn, bool wait )
        {
            this.fn = fn;
            this.wait = wait;
        }
    }
};