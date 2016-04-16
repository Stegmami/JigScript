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
    public class UIControl
    {
        public enum Type
        {
            None = 0,
            Button = 1,
            Label = 2,
            Edit = 3,
            List = 4,
            Toggle = 5,
            Box = 6
        }
        ;
        
        public Type T;
        public int index;
        public UIButton button;
        public UILabel label;
        public UIEdit edit;
        public UIList list;
        public UIToggle toggle;
        public UIBox box;
        
        public UIControl( Type T, int index )
        {
            this.T = T;
            this.index = index;
        }
        
        public bool Active( bool state )
        {
            bool previous = false;
            
            switch( T )
            {
                case Type.Button:
                    previous = button.active;
                    button.active = state;
                    break;
                case Type.Label:
                    previous = label.active;
                    label.active = state;
                    break;
                case Type.Edit:
                    previous = edit.active;
                    edit.active = state;
                    break;
                case Type.List:
                    previous = list.active;
                    list.active = state;
                    break;
                case Type.Toggle:
                    previous = toggle.active;
                    toggle.active = state;
                    break;
                case Type.Box:
                    previous = box.active;
                    box.active = state;
                    break;
            }

            return previous;
        }

        public void Update()
        {
            switch( T )
            {
                case Type.Label:
                    label.Update();
                    break;
                case Type.Button:
                    button.Update();
                    break;
                case Type.Toggle:
                    toggle.Update();
                    break;
                case Type.Edit:
                    edit.Update();
                    break;
                case Type.List:
                    list.Update();
                    break;
                case Type.Box:
                    box.Update();
                    break;
            }
        }
    };
};