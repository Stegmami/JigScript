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
using NightPen;

namespace NightPen.JigScript
{
    public class Variable : System.Object
    {
        private Value v;
        private Object rwLock = new Object();

        public Variable( Value v )
        {
            lock( rwLock )
            {
                this.v = new Value(v);
            }
        }

        public void IncrementChangedCount()
        {
            lock( rwLock )
            {
                ++this.v.changed;
            }
        }

        public void DecrementChangedCount()
        {
            lock( rwLock )
            {
                if ( this.v.changed > 0 )
                {
                    --this.v.changed;
                }
            }
        }
        
        public Value Set( Value v)
        {
            lock( rwLock )
            {
                int changed = v.changed;
                this.v = new Value(v);
                this.v.changed = changed;
            }
            
            return this.v;
        }

        public Value Get()
        {
            Value v;

            lock( rwLock )
            {
                v = new Value(this.v);
            }

            return v;
        }
    };
}