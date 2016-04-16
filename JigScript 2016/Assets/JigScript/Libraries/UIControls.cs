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
    public class UIControls : JigExtension
    {
        private GUISkin guiSkin;
        static List<UIControl> controls;
        private bool initialized;
    
        public void Awake()
        {
            initialized = false;
        }
                
        public static Value GetControlVariable( Value v )
        {
            Value v1 = new Value(0, Variables.GetUniqueName());
            
            if ( controls != null )
            {
                if ( v.I>0 && v.I<controls.Count )
                {
                    v1 = Variables.Read(controls[v.I].index, 1);
                    if ( v1.controlType != UIControl.Type.Button &&
                        v1.controlType != UIControl.Type.Edit &&
                        v1.controlType != UIControl.Type.Label &&
                        v1.controlType != UIControl.Type.List &&
                        v1.controlType != UIControl.Type.Toggle &&
                        v1.controlType != UIControl.Type.Box )
                    {
                        v1.I = -1;
                    }
                }
                else
                {
                    v1.I = -1;
                }
            }
            
            return v1;
        }
        
        IEnumerator ButtonFunction( List<Value> values )
        {
            if ( values.Count != 6 && values.Count != 7)
            {
                Debug.LogError("button = UIControls.Button(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                values[1].ConvertTo(Value.ValueType.Bool);
                if ( values[2].T != Value.ValueType.GObject )
                {
                    values[2].ConvertTo(Value.ValueType.String);
                }
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                values[5].ConvertTo(Value.ValueType.Float);
                values[6].ConvertTo(Value.ValueType.Float);
                
                Value v = new Value(values[2]);
                v.I = controls.Count;
                v.controlType = UIControl.Type.Button;
                Variables.CreateUnique(ref v);
                
                UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                
                UIControl uiControl = new UIControl(UIControl.Type.Button, v.index);
                
                uiControl.button = new UIButton(L, rect, v, values[1].B);
                
                if ( values.Count == 8 )
                {
                    foreach(GUIStyle gs in guiSkin.customStyles)
                    {
                        if ( gs.name == values[7].S )
                        {
                            uiControl.button.SetStyle(gs);
                            break;
                        }
                    }
                }
                
                values.Add(v);
                controls.Add(uiControl);
            }
            yield return 0;
        }
        
        IEnumerator ToggleFunction( List<Value> values )
        {
            if ( values.Count != 7 && values.Count != 8)
            {
                Debug.LogError("toggle = UIControls.Toggle(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                if ( values[2].T != Value.ValueType.GObject )
                {
                    values[2].ConvertTo(Value.ValueType.String);
                }
                values[0].ConvertTo(Value.ValueType.Integer);
                values[1].ConvertTo(Value.ValueType.Bool);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                values[5].ConvertTo(Value.ValueType.Float);
                values[6].ConvertTo(Value.ValueType.Float);
                
                Value v = new Value(values[2]);
                v.I = controls.Count;
                v.controlType = UIControl.Type.Toggle;
                Variables.CreateUnique(ref v);
                
                UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                
                UIControl uiControl = new UIControl(UIControl.Type.Toggle, v.index);
                uiControl.toggle = new UIToggle(L, rect, v, values[1].B);
                
                if ( values.Count == 8 )
                {
                    values[7].ConvertTo(Value.ValueType.String);
                    foreach(GUIStyle gs in guiSkin.customStyles)
                    {
                        if ( gs.name == values[7].S )
                        {
                            uiControl.toggle.SetStyle(gs);
                            break;
                        }
                    }
                }
                
                values.Add(v);
                controls.Add(uiControl);
            }
            yield return 0;
        }
        
        IEnumerator EditFunction( List<Value> values )
        {
            if ( values.Count != 7 && values.Count != 8)
            {
                Debug.LogError("edit = UIControls.Edit(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                if ( values[2].T != Value.ValueType.GObject )
                {
                    values[2].ConvertTo(Value.ValueType.String);
                }
                values[0].ConvertTo(Value.ValueType.Integer);
                values[1].ConvertTo(Value.ValueType.Bool);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                values[5].ConvertTo(Value.ValueType.Float);
                values[6].ConvertTo(Value.ValueType.Float);
                
                Value v = new Value(values[2]);
                v.I = controls.Count;
                v.controlType = UIControl.Type.Edit;
                Variables.CreateUnique(ref v);
                
                UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                
                UIControl uiControl = new UIControl(UIControl.Type.Edit, v.index);
                uiControl.edit = new UIEdit(L, rect, v, values[1].B);
                if ( values.Count == 8 )
                {
                    values[7].ConvertTo(Value.ValueType.String);
                    foreach(GUIStyle gs in guiSkin.customStyles)
                    {
                        if ( gs.name == values[7].S )
                        {
                            uiControl.edit.SetStyle(gs);
                            break;
                        }
                    }
                }
                
                values.Add(v);
                controls.Add(uiControl);
            }
            yield return 0;
        }
        
        IEnumerator LabelFunction( List<Value> values )
        {
            if ( values.Count != 7 && values.Count != 8)
            {
                Debug.LogError("label = UIControls.Label(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                if ( values[2].T != Value.ValueType.GObject )
                {
                    values[2].ConvertTo(Value.ValueType.String);
                }
                values[0].ConvertTo(Value.ValueType.Integer);
                values[1].ConvertTo(Value.ValueType.Bool);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                values[5].ConvertTo(Value.ValueType.Float);
                values[6].ConvertTo(Value.ValueType.Float);
                
                Value v = new Value(values[2]);
                v.I = controls.Count;
                v.controlType = UIControl.Type.Label;
                Variables.CreateUnique(ref v);
                
                UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                
                UIControl uiControl = new UIControl(UIControl.Type.Label, v.index);
                uiControl.label = new UILabel(L, rect, v, values[1].B);
                if ( values.Count == 8 )
                {
                    values[7].ConvertTo(Value.ValueType.String);
                    foreach(GUIStyle gs in guiSkin.customStyles)
                    {
                        if ( gs.name == values[7].S )
                        {
                            uiControl.toggle.SetStyle(gs);
                            break;
                        }
                    }
                }
                
                values.Add(v);
                controls.Add(uiControl);
            }
            yield return 0;
        }
        
        IEnumerator ListFunction( List<Value> values )
        {
            if ( values.Count != 7 && values.Count != 8)
            {
                Debug.LogError("label = UIControls.Label(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                
                if ( values[2].isVariable == false )
                {
                    Debug.LogError("Initial content parameter 3, must be a variable.");
                }
                else
                {
                    values[0].ConvertTo(Value.ValueType.Integer);
                    values[3].ConvertTo(Value.ValueType.Float);
                    values[4].ConvertTo(Value.ValueType.Float);
                    values[5].ConvertTo(Value.ValueType.Float);
                    values[6].ConvertTo(Value.ValueType.Float);
                    
                    Value v = new Value(values[2]);
                    v.I = controls.Count;
                    v.controlType = UIControl.Type.List;
                    Variables.CreateUnique(ref v);
                    
                    v.I = controls.Count;
                    
                    List<Value> va = Variables.Expand(values[2], false);
                    for( int ii=1; ii<va.Count; ++ii )
                    {
                        va[ii].I = v.I;
                        va[ii].controlType = UIControl.Type.List;
                        Variables.Store(v, ii, va[ii]);
                    }
                    
                    UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                    Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                    
                    UIControl uiControl = new UIControl(UIControl.Type.List, v.index);
                    uiControl.list = new UIList(L, rect, v, values[1].B);
                    
                    values.Add(v);
                    controls.Add(uiControl);
                }
            }
            yield return 0;
        }
        
        IEnumerator BoxFunction( List<Value> values )
        {
            if ( values.Count != 7 && values.Count != 8)
            {
                Debug.LogError("box = UIControls.Box(location, [show = true | hide = false], content, x, y, width, height, [style]);");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.Integer);
                if ( values[2].T != Value.ValueType.GObject )
                {
                    values[2].ConvertTo(Value.ValueType.String);
                }
                values[0].ConvertTo(Value.ValueType.Integer);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                values[5].ConvertTo(Value.ValueType.Float);
                values[6].ConvertTo(Value.ValueType.Float);
                
                Value v = new Value(values[2]);
                v.I = controls.Count;
                v.controlType = UIControl.Type.Box;
                Variables.CreateUnique(ref v);
                
                UIControlsBase.Location L = (UIControlsBase.Location)values[0].I;
                Rect rect = new Rect(values[3].F, values[4].F, values[5].F, values[6].F);
                
                UIControl uiControl = new UIControl(UIControl.Type.Box, v.index);
                uiControl.box = new UIBox(L, rect, v, values[1].B);
                if ( values.Count == 8 )
                {
                    values[7].ConvertTo(Value.ValueType.String);
                    foreach(GUIStyle gs in guiSkin.customStyles)
                    {
                        if ( gs.name == values[7].S )
                        {
                            uiControl.box.SetStyle(gs);
                            break;
                        }
                    }
                }
                
                values.Add(v);
                controls.Add(uiControl);
            }
            yield return 0;
        }
        
        private int FindControl( Value v )
        {
            int index = -1;
            
            for( int ii=1; ii<controls.Count; ++ii )
            {
                if ( controls[ii].index == v.index )
                {
                    index = ii;
                    break;
                }
            }
            
            return index;
        }
        
        IEnumerator SetSkinFunction( List<Value>values )
        {
            if ( values.Count != 1 )
            {
                Debug.LogError("UIControls.SetSkin(\"skin file\");");
            }
            else
            {
                values[0].ConvertTo(Value.ValueType.String);
                guiSkin = (GUISkin)Resources.Load(values[0].S);
                if ( guiSkin == null )
                {
                    guiSkin = (GUISkin)Resources.Load("JigScript/Skins/JigSkin");
                }
                if ( this.guiSkin.customStyles.Length>0 && this.guiSkin.customStyles[0].name == "ListBox" )
                {
                    UIList.listStyle = this.guiSkin.customStyles[0];
                }
                else
                {
                    UIList.listStyle = null;
                }
            }
            yield return 0;
        }
        
        public IEnumerator ClearFunction( List<Value> values )
        {
            for( int ii=0; ii<controls.Count; ++ii )
            {
                controls[ii].Active(false);
                Value v = Variables.Read(controls[ii].index, 0);
                v.T = Value.ValueType.None;
                Variables.Store(v, v.arrayIndex, new Value(v));
            }
            yield return new WaitForEndOfFrame();
            controls.Clear();
            controls.Add(new UIControl(UIControl.Type.None, 0));
        }
        
        private Value GetListIndex( Value v )
        {
    		Value v1 = UIControls.GetControlVariable(v);
    		if ( v1.I > 0 && v1.controlType == UIControl.Type.List )
    		{
                v = new Value((controls[v1.I].list.selected + 1), Variables.GetUniqueName());
    		}
            return v;
        }
        
        private void SetListIndex( Value dest, int arrayIndex, Value source )
        {
            Value v1 = UIControls.GetControlVariable(dest);
            if ( v1.I > 0 && v1.controlType == UIControl.Type.List )
            {
                controls[v1.I].list.selected = v1.I;
            }
        }
        
        private Value GetContent( Value v )
        {
            Value v1 = UIControls.GetControlVariable(v);
            if ( v1.I > 0 )
            {
                switch( v.controlType )
                {
                    case UIControl.Type.Button:
                        v = controls[v1.I].button.Get();
                        break;
                    case UIControl.Type.Edit:
                        v = controls[v1.I].edit.Get();
                        break;
                    case UIControl.Type.Label:
                        v = controls[v1.I].label.Get();
                        break;
                    case UIControl.Type.List:
                        v = controls[v1.I].list.Get(v.arrayIndex);
                        break;
                    case UIControl.Type.Toggle:
                        v = controls[v1.I].toggle.Get();
                        break;
                    case UIControl.Type.Box:
                        v = controls[v1.I].box.Get();
                        break;
                }
            }
            return v;
        }
        
        private void SetContent( Value dest, int arrayIndex, Value source )
        {
            List<Value> va = Variables.Expand(dest, false);
            
            for(int ii=1; ii<va.Count; ++ii)
            {
                Value v1 = UIControls.GetControlVariable(va[ii]);
                if ( v1.I > 0 )
                {
                    switch( v1.controlType )
                    {
                        case UIControl.Type.Button:
                            controls[v1.I].button.Set(source);
                            break;
                        case UIControl.Type.Edit:
                            controls[v1.I].edit.Set(source);
                            break;
                        case UIControl.Type.Label:
                            controls[v1.I].label.Set(source);
                            break;
                        case UIControl.Type.List:
                            controls[v1.I].list.Set(arrayIndex, source);
                            break;
                        case UIControl.Type.Toggle:
                            controls[v1.I].toggle.Set(source);
                            break;
                        case UIControl.Type.Box:
                            controls[v1.I].box.Set(source);
                            break;
                    }
                }
            }
        }
        
        private Value GetChanged( Value v )
        {
            Value v1 = UIControls.GetControlVariable(v);
            if ( v1.I > 0 )
            {
                switch( v.controlType )
                {
                    case UIControl.Type.Button:
                        v.B = controls[v1.I].button.Changed;
                        break;
                    case UIControl.Type.Edit:
                        v.B = controls[v1.I].edit.Changed;
                        break;
                    case UIControl.Type.Label:
                        v.B = controls[v1.I].label.Changed;
                        break;
                    case UIControl.Type.List:
                        v.B = controls[v1.I].list.Changed;
                        break;
                    case UIControl.Type.Toggle:
                        v.B = controls[v1.I].toggle.Changed;
                        break;
                    case UIControl.Type.Box:
                        v.B = controls[v1.I].box.Changed;
                        break;
                }
            }
            return v;
        }
        
        private Value GetClicked( Value v )
        {
            if ( v.T == Value.ValueType.GObject )
            {
                GOClicked goc = v.G.GetComponent < GOClicked >();
                if ( goc )
                {
                    v.T = Value.ValueType.Bool;
                    v.B = goc.IsClicked();
                }
            }
            else
            {
                Value v1 = UIControls.GetControlVariable(v);
                if ( v1.I > 0 )
                {
                    switch( v1.controlType )
                    {
                        case UIControl.Type.Button:
                            v.T = Value.ValueType.Bool;
                            v.B  = controls[v1.I].button.Clicked;
                            break;
                        case UIControl.Type.Edit:
                            v.T = Value.ValueType.Bool;
                            v.B = controls[v1.I].edit.Clicked;
                            break;
                        case UIControl.Type.Label:
                            v.T = Value.ValueType.Bool;
                            v.B = controls[v1.I].label.Clicked;
                            break;
                        case UIControl.Type.List:
                            v.T = Value.ValueType.Bool;
                            v.B = controls[v1.I].list.Clicked;
                            break;
                        case UIControl.Type.Toggle:
                            v.T = Value.ValueType.Bool;
                            v.B = controls[v1.I].toggle.Clicked;
                            break;
                        case UIControl.Type.Box:
                            v.T = Value.ValueType.Bool;
                            v.B = controls[v1.I].box.Clicked;
                            break;
                    }
                }
            }
            return v;
        }
        
        Value GetShow(Value v)
        {
            Value v1 = UIControls.GetControlVariable(v);
            if ( v1.I > 0 )
            {
                switch( v1.controlType )
                {
                    case UIControl.Type.Button:
                        v.T = Value.ValueType.Bool;
                        v.B  = controls[v1.I].button.active;
                        break;
                    case UIControl.Type.Edit:
                        v.T = Value.ValueType.Bool;
                        v.B = controls[v1.I].edit.active;
                        break;
                    case UIControl.Type.Label:
                        v.T = Value.ValueType.Bool;
                        v.B = controls[v1.I].label.active;
                        break;
                    case UIControl.Type.List:
                        v.T = Value.ValueType.Bool;
                        v.B = controls[v1.I].list.active;
                        break;
                    case UIControl.Type.Toggle:
                        v.T = Value.ValueType.Bool;
                        v.B = controls[v1.I].toggle.active;
                        break;
                    case UIControl.Type.Box:
                        v.T = Value.ValueType.Bool;
                        v.B = controls[v1.I].box.active;
                        break;
                }
            }
            return v;
        }
        
        void SetShow(Value dest, int arrayIndex, Value source)
        {
            List<Value> va = Variables.Expand(dest, false);
            
            source.ConvertTo(Value.ValueType.Bool);
            
            for(int ii=1; ii<va.Count; ++ii)
            {
                Value v1 = UIControls.GetControlVariable(va[ii]);
                if ( v1.I > 0 )
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    
                    switch( v1.controlType )
                    {
                        case UIControl.Type.Button:
                            controls[v1.I].button.active = source.B;
                            break;
                        case UIControl.Type.Edit:
                            v1.B = controls[v1.I].edit.active = source.B;
                            break;
                        case UIControl.Type.Label:
                            v1.B = controls[v1.I].label.active = source.B;
                            break;
                        case UIControl.Type.List:
                            v1.B = controls[v1.I].list.active = source.B;
                            break;
                        case UIControl.Type.Toggle:
                            v1.B = controls[v1.I].toggle.active = source.B;
                            break;
                        case UIControl.Type.Box:
                            v1.B = controls[v1.I].box.active = source.B;
                            break;
                    }
                }
            }
        }
        
        void OnGUI()
        {
            if ( initialized )
            {
                GUI.skin = guiSkin;
                
                if ( controls != null )
                {
                    for( int ii=1; ii<controls.Count; ++ii )
                    {
                        controls[ii].Update();
                    }
                }
            }
        }
        
        public override void Initialize(JigCompiler compiler)
        {
            useGUILayout = false;
            
            this.guiSkin = null;
            GUISkin skin = (GUISkin)Resources.Load("Skins/JigSkin");
            if ( skin != null )
            {
                this.guiSkin = skin;
                if ( this.guiSkin.customStyles.Length>0)
                {
                    if ( this.guiSkin.customStyles[0].name == "ListBox" )
                    {
                        UIList.listStyle = this.guiSkin.customStyles[0];
                    }
                    else
                    {
                        UIList.listStyle = null;
                    }
                }
            }
    
            UIList.GUIListContent.fontSize = 20;
            
            controls = new List<UIControl>();
            controls.Add(new UIControl(UIControl.Type.None, 0));
            
            Variables.Create("UIControls.XY", new Value((int)UIControlsBase.Location.XY, "UIControls.XY"));
            Variables.Create("UIControls.Left", new Value((int)UIControlsBase.Location.Left, "UIControls.Left"));
            Variables.Create("UIControls.Right", new Value((int)UIControlsBase.Location.Right, "UIControls.Right"));
            Variables.Create("UIControls.Top", new Value((int)UIControlsBase.Location.Top, "UIControls.Top"));
            Variables.Create("UIControls.Bottom", new Value((int)UIControlsBase.Location.Bottom, "UIControls.Bottom"));
            Variables.Create("UIControls.CenterHorizontal", new Value((int)UIControlsBase.Location.CenterHorizontal, "UIControls.CenterHorizontal"));
            Variables.Create("UIControls.CenterVertical", new Value((int)UIControlsBase.Location.CenterVertical, "UIControls.CenterVertical"));
            Variables.Create("UIControls.CenterBoth", new Value((int)UIControlsBase.Location.CenterBoth, "UIControls.CenterBoth"));
            Variables.Create("UIControls.Visible", new Value(true, "UIControls.Visible"));
            Variables.Create("UIControls.NotVisible", new Value(false, "UIControls.NotVisible"));
            
            Variables.Create("UIControls.TopCenter", new Value((int)UIControlsBase.Location.TopCenter, "UIControls.TopCenter"));
            Variables.Create("UIControls.BottomCenter", new Value((int)UIControlsBase.Location.BottomCenter, "UIControls.BottomCenter"));
            Variables.Create("UIControls.LeftCenter", new Value((int)UIControlsBase.Location.LeftCenter, "UIControls.LeftCenter"));
            Variables.Create("UIControls.RightCenter", new Value((int)UIControlsBase.Location.RightCenter, "UIControls.RightCenter"));
                    
            compiler.AddFunction("UIControls.Button", ButtonFunction);
            compiler.AddFunction("UIControls.Toggle", ToggleFunction);
            compiler.AddFunction("UIControls.Edit", EditFunction);
            compiler.AddFunction("UIControls.Label", LabelFunction);
            compiler.AddFunction("UIControls.List", ListFunction);
            compiler.AddFunction("UIControls.Box", BoxFunction);
            compiler.AddFunction("UIControls.SetSkin", SetSkinFunction);
            compiler.AddFunction("UIControls.Clear", ClearFunction);
            
            Variables.CreateCustomProperty("index", GetListIndex, SetListIndex);
            Variables.CreateCustomProperty("content", GetContent, SetContent);
            Variables.CreateCustomProperty("changed", GetChanged, null);
            Variables.CreateCustomProperty("clicked", GetClicked, null);
            
            Variables.CreateCustomProperty("show", GetShow, SetShow);
            
            useGUILayout = false;
            initialized = true;
        }
    }
}