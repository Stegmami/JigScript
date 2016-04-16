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
    public class UIList : UIControlsBase
    {
        internal class GUIListContent
        {
            public GUIContent guiContent;
            public int height;
            public int width;
            public static int fontSize;

            public bool IsEmpty { get; private set; }
            
            public GUIListContent()
            {
                IsEmpty = true;
            }
            
            public GUIListContent(GUIContent guiContent)
            {
                this.guiContent = guiContent;
                if (this.guiContent.image != null)
                {
                    this.height = this.guiContent.image.height;
                    this.width = this.guiContent.image.width;
                }
                else
                {
                    this.height = fontSize;
                    this.width = this.guiContent.text.Length * GUIListContent.fontSize;
                }
                IsEmpty = false;
            }
        };

        public static GUIStyle listStyle = null;
        public Vector2 scrollPosition;
        public int selected;
        public Value v;
        
        public UIList(Value v)
        {
            Clicked = false;
            scrollPosition = new Vector2(0, 0);
        }

        public UIList(UIControlsBase.Location L, Rect rect, Value v, bool active) : base(L, rect, active)
        {
            this.v = v;
        }
        
        public Value Get(int arrayIndex)
        {
            Value v2 = new Value(v);
            
            if (arrayIndex > 0 && arrayIndex < Variables.Length(v))
            {
                v2 = Variables.Read(v.index, arrayIndex);
            }
            else
            {
                v2 = new Value(Value.ValueType.Empty, Variables.GetUniqueName());
            }
            return v2;
        }
        
        public void Set(int element, Value vNew)
        {
            if (element >= 0 && element < Variables.Length(v))
            {
                if (element == 0)
                {
                    Variables.Copy(v, vNew);
                    Changed = true;
                }
                else
                {
                    Variables.Store(v, element, vNew);
                    Changed = true;
                }
            }
        }
        
        public void Update()
        {
            if (active)
            {
                Value v1 = new Value(v);
                v1.arrayIndex = 0;
                List<Value> valueArray = Variables.Expand(v1, false);
                
                List<GUIListContent> guiContentList = new List<GUIListContent>();
                int totalWidth = 0;
                int totalHeight = 0;
                GUIListContent.fontSize = 20;
                if (listStyle != null)
                {
                    GUIListContent.fontSize = listStyle.fontSize;
                }
                for (int ii=1; ii<valueArray.Count; ++ii)
                {
                    if (valueArray[ii].T == Value.ValueType.Empty)
                    {
                        guiContentList.Add(new GUIListContent());
                        continue;
                    }
                    GUIContent gc = GetContent(valueArray [ii]);
                    GUIListContent glc = new GUIListContent(gc);
                    
                    totalHeight += glc.height;
                    if (glc.width > totalWidth)
                    {
                        totalWidth = glc.width;
                    }
                    guiContentList.Add(glc);
                }
                Rect viewRect = new Rect(0, 0, totalWidth, totalHeight);
                scrollPosition = GUI.BeginScrollView(GetRect(), scrollPosition, viewRect);
                Rect rcItem = new Rect(0, 0, viewRect.width, GUIListContent.fontSize);
                
                int oldSelect = selected;
                
                for (int ii=0; ii<guiContentList.Count; ii++)
                {
                    rcItem.height = guiContentList [ii].height;
                    if ( listStyle != null )
                    {
                        if (GUI.Toggle(rcItem, selected == ii, guiContentList [ii].guiContent, listStyle))
                        {
                            selected = ii;
                        }
                    }
                    else
                    {
                        if (GUI.Toggle(rcItem, selected == ii, guiContentList [ii].guiContent))
                        {
                            selected = ii;
                        }
                    }
                    rcItem.y += guiContentList [ii].height;
                }
                GUI.EndScrollView();
                if ( oldSelect != selected )
                {
                    Clicked = true;
                    Changed = true;
                    CPU.whensNeeded = true;
                }
            }
        }               
    };
}