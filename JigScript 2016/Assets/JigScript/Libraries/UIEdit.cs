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
    public class UIEdit : UIControlsBase
    {
        public Value v;
        
        private GUIStyle style;
        
        public UIEdit(Value v)
        {
            this.v = new Value(v);
            style = new GUIStyle();
        }
        
        public void SetStyle(GUIStyle style)
        {
            this.style = style;
        }
        
        public void Set(Value vNew)
        {
            vNew.ConvertTo(Value.ValueType.String);
            v.S = vNew.S;
            Variables.Store(v, 1, v);
        }
        
        public Value Get()
        {
            return new Value(v.S, Variables.GetUniqueName());
        }
        
        public UIEdit(UIControlsBase.Location L, Rect rect, Value v, bool active)  : base(L, rect, active)
        {
            this.v = v;
        }
        
        private string Process(string s)
        {
            if ( style != null )
            {
                s = GUI.TextField(GetRect(), v.S, style);
            }
            else
            {
                s = GUI.TextField(GetRect(), v.S);
            }
            
            return s;
        }
        
        public void Update()
        {
            if (active)
            {
                v = Variables.Read(v.index, 1);
                string s = Process(v.S);
                if (s != v.S)
                {
                    v.S = s;
                    Variables.Store(v, v.arrayIndex, v);
                    Changed = true;
                    CPU.whensNeeded = true;
                }
            }
        }               
    };
}