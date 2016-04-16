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
    public class UIControlsBase
    {
        public enum Location : int
        {
            None = 0,
            XY = 1,
            Left = 2,
            Right = 3,
            Top = 4,
            Bottom = 5,
            CenterHorizontal = 6,
            CenterVertical = 7,
            CenterBoth = 8,
            TopCenter = 9,
            BottomCenter = 10,
            LeftCenter = 11,
            RightCenter = 12
        }
        ;
        
        public Rect rect;
        public bool active;
        public Location location;
        private bool clicked;
        
        public bool Clicked
        {
            get
            {
                bool c = this.clicked;
                this.clicked = false;
                
                return c;
            }

            set
            {
                this.clicked = value;
            }
        }

        private bool changed;
        
        public bool Changed
        {
            get
            {
                bool c = this.changed;
                this.changed = false;
                
                return c;
            }
            
            set
            {
                this.changed = value;
            }
        }
        
        public UIControlsBase()
        {
            this.rect = new Rect(0, 0, 100, 100);
            this.active = false;
            this.location = Location.XY;
            this.Clicked = false;
            this.Changed = false;
        }

        public UIControlsBase( Location L, Rect rect, bool active )
        {
            this.rect = new Rect(rect);
            this.active = active;
            this.location = L;
            this.Clicked = false;
        }

        public Rect GetRect()
        {
            Rect rc = new Rect(rect);
            
            if ( active )
            {
                switch( location )
                {
                    case Location.None:
                        break;
                    case Location.XY:
                        break;
                    case Location.Left:
                        rc.x = 0;
                        break;
                    case Location.Right:
                        rc.x = Screen.width - rc.width;
                        break;
                    case Location.Top:
                        rc.y = 0;
                        break;
                    case Location.Bottom:
                        rc.y = Screen.height - rc.height;
                        break;
                    case Location.CenterHorizontal:
                        rc.x = Screen.width / 2 - rc.width / 2;
                        break;
                    case Location.CenterVertical:
                        rc.y = Screen.height / 2 - rc.height / 2;
                        break;
                    case Location.CenterBoth:
                        rc.x = Screen.width / 2 - rc.width / 2;
                        rc.y = Screen.height / 2 - rc.height / 2;
                        break;
                    case Location.TopCenter:
                        rc.x = Screen.width / 2 - rc.width / 2;
                        rc.y = 0;
                        break;
                    case Location.BottomCenter:
                        rc.x = Screen.width / 2 - rc.width / 2;
                        rc.y = Screen.height - rc.height;
                        break;
                    case Location.LeftCenter:
                        rc.x = 0;
                        rc.y = Screen.height / 2 - rc.height / 2;
                        break;
                    case Location.RightCenter:
                        rc.x = Screen.width - rc.width;
                        rc.y = Screen.height / 2 - rc.height / 2;
                        break;
                }
            }
            return rc;
        }

        public GUIContent GetContent( Value v )
        {
            GUIContent guiContent;

            if ( v.T == Value.ValueType.GObject )
            {
                SpriteRenderer sr = v.G.GetComponent<SpriteRenderer>();
                if ( sr != null )
                {
                    Color[] pix = sr.sprite.texture.GetPixels((int)sr.sprite.textureRect.x, (int)sr.sprite.textureRect.y,
                                                              (int)sr.sprite.textureRect.width, (int)sr.sprite.textureRect.height);
                    Texture2D tx = new Texture2D((int)sr.sprite.textureRect.width, (int)sr.sprite.textureRect.height);
                    tx.SetPixels(pix);
                    tx.Apply();
                    guiContent = new GUIContent(tx);
                }
                else
                {
                    GUITexture gt = v.G.GetComponent<GUITexture>();
                    if ( gt != null )
                    {
                        guiContent = new GUIContent(gt.texture);
                    }
                    else
                    {
                        guiContent = new GUIContent(v.S);
                    }
                }
            }
            else
            {
                guiContent = new GUIContent(v.S);
            }

            return guiContent;
        }
    };
}
