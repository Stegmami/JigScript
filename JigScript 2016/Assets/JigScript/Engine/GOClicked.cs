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
    public class GOClicked : MonoBehaviour
    {
        private bool clickable;
        private bool clicked;
        private bool mouseOver;
        private bool mouseDown;
        private bool isDragging;
        private bool dragBegin;
        private bool dragEnd;
        private Vector3 mouseCurrentPos;
        private Vector3 mouseStartPos;
        private float dragDistance;
        
        public bool IsClicked()
        {
            bool rc = false;
    
            if ( this.clickable && this.clicked )
            {
                rc = true;
            }
    
            this.clicked = false;
    
            return rc;
        }
    
        public bool IsSelected()
        {
            bool rc = false;
            
            if ( this.clickable && this.mouseDown )
            {
                rc = true;
            }
            
            return rc;
        }
    
        public bool IsDragging()
        {
            bool rc = false;
            
            if ( this.clickable && this.isDragging )
            {
                rc = true;
            }
            
            return rc;
        }
    
        public bool IsDragBegin()
        {
            bool rc = false;
            
            if ( this.clickable && this.dragBegin )
            {
                rc = true;
                this.dragBegin = false;
            }
            
            return rc;
        }
    
        public bool IsDragEnd()
        {
            bool rc = false;
            
            if ( this.clickable && this.dragEnd )
            {
                rc = true;
                this.dragEnd = false;
                this.isDragging = false;
            }
            
            return rc;
        }
    
        public bool IsHover()
        {
            CPU.whensNeeded = true;
            return mouseOver;
        }
    
        public void OnMouseDrag()
        {
            float distance = Vector3.Distance(mouseStartPos, Input.mousePosition);
    
            if ( distance>=dragDistance )
            {
                if ( this.isDragging == false )
                {
                    this.dragBegin = true;
                }
                this.isDragging = true;
                CPU.whensNeeded = true;
            }
        }
    
        public void OnMouseOver()
        {
            mouseOver = true;
        }
        
        public void OnMouseExit()
        {
            mouseOver = false;
        }
        
        public void SetClickable( bool clickable )
        {
            this.clickable = clickable;
        }
    
        public void SetMinDragDistance( float distance )
        {
            this.dragDistance = distance;
            this.isDragging = false;
        }
    
        void OnMouseUp()
        {
            this.mouseDown = false;
            if ( this.isDragging )
            {
                this.dragEnd = true;
                CPU.whensNeeded = true;
            }
            this.isDragging = false;
            this.dragBegin = false;
        }
    
        void OnMouseDown()
        {
            this.mouseStartPos = Input.mousePosition;
            this.mouseDown = true;
            this.dragBegin = false;
            this.dragEnd = false;
            this.isDragging = false;
            CPU.whensNeeded = true;
        }
    
        void OnMouseUpAsButton()
        {
        
            if ( this.isDragging == false )
            {
                this.clicked = true;
                CPU.whensNeeded = true;
            }
        }
    
        void Start()
        {
            this.clickable = false;
            this.clicked = false;
            this.mouseDown = false;
            this.dragBegin = false;
            this.dragEnd = false;
            this.dragDistance = 1.2f;
        }
    }
}