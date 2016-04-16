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
    public class UIScreen : JigExtension
    {
        private int lastScreenWidth;
        private int lastScreenHeight;
        private bool lastFullScreen;
        
        void UpdateScreenSize()
        {
            Value v;
            
            if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height && lastFullScreen != Screen.fullScreen)
            {
                return;
            }
            
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastFullScreen = Screen.fullScreen;
            
            v = Variables.Read("UIScreen.width", 1);
            v.I = Screen.width;
            Variables.Store(v, 1, new Value(v));
            
            v = Variables.Read("UIScreen.height", 1);
            v.I = Screen.height;
            Variables.Store(v, 1, new Value(v));
    
            v = Variables.Read("UIScreen.fullScreen", 1);
            v.B = Screen.fullScreen;
            Variables.Store(v, 1, new Value(v));
        }
        
        IEnumerator SetResolution(List<Value> values)
        {
            if ( values.Count != 3 )
            {
                Debug.LogError("SetResolution(width, height, [true=fullscreen | false=windowed]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                values[1].ConvertTo(Value.ValueType.Integer);
                values[2].ConvertTo(Value.ValueType.Bool);
    
                Screen.SetResolution(values[0].I, values[1].I, values[2].B);
            }
            yield return 0;
        }
        
        public override void Initialize(JigCompiler compiler)
        {
            this.lastScreenWidth = Screen.width;
            this.lastScreenHeight = Screen.height;
            
            Variables.Create("UIScreen.width", new Value(Screen.width, "UIScreen.width"));
            Variables.Create("UIScreen.height", new Value(Screen.height, "UIScreen.height"));
            Variables.Create("UIScreen.fullScreen", new Value(Screen.fullScreen, "UIScreen.fullScreen"));
            
            int index = Variables.Create("UIScreen.supported.width", new Value(0, "UIScreen.supported.width"));
            Value w = Variables.Read(index, 0);
            
            index = Variables.Create("UIScreen.supported.height", new Value(0, "UIScreen.supported.height"));
            Value h = Variables.Read(index, 0);
    
            index = Variables.Create("UIScreen.supported.refreshRate", new Value(0, "UIScreen.supported.refreshRate"));
            Value r = Variables.Read(index, 0);
            
            Resolution[] resolutions = Screen.resolutions;
            
            for(int ii=0; ii<resolutions.Length; ++ii)
            {
                Variables.Store(w, ii+1, new Value(resolutions[ii].width, w.name));
                Variables.Store(h, ii+1, new Value(resolutions[ii].height, h.name));
                Variables.Store(r, ii+1, new Value(resolutions[ii].refreshRate, r.name));
            }
    
            compiler.AddFunction("UIScreen.SetResolution", SetResolution);
    
            InvokeRepeating("UpdateScreenSize", .1f, .1f);
        }
    }
}