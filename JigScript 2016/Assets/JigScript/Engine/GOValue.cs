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
    public class GOValue : MonoBehaviour
    {
        public Value v;
        public bool StopTween;
        
        public IEnumerator TweenPosition( float time, Vector3 to)
        {
            float ii = 0.0f;
            float rate = 1.0f / time;
            Vector3 start = new Vector3(v.G.transform.position.x, v.G.transform.position.y, v.G.transform.position.z);
            
            StopTween = false;
    
            Value v1 = Variables.Read(v.name, v.arrayIndex);
            v.rotateTo = v1.rotateTo;
            v.scaleTo = v1.scaleTo;
            v.moveTo = false;
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            while( ii < 1.0 && StopTween == false )
            {
                ii += Time.deltaTime * rate;
                v.G.transform.position = Vector3.Lerp(start, to, ii);
                yield return new WaitForEndOfFrame();
            }
            
            if ( ii >= 1.0f )
            {
                v.moveTo = true;
            }
    
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            yield return 0;
        }
        
        public IEnumerator TweenScale( float time, Vector3 to)
        {
            float ii = 0.0f;
            float rate = 1.0f / time;
            Vector3 start = new Vector3(v.G.transform.localScale.x, v.G.transform.localScale.y, v.G.transform.localScale.z);
            
            StopTween = false;
            
            Value v1 = Variables.Read(v.name, v.arrayIndex);
            v.moveTo = v1.moveTo;
            v.rotateTo = v1.rotateTo;
            v.scaleTo = false;
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            while( ii < 1.0 && StopTween == false )
            {
                ii += Time.deltaTime * rate;
                v.G.transform.localScale = Vector3.Lerp(start, to, ii);
                yield return new WaitForEndOfFrame();
            }
            
            if ( ii >= 1.0f )
            {
                v.scaleTo = true;
            }
            
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            yield return 0;
        }
        
        public IEnumerator TweenRotate( float time, Vector3 to)
        {
            float ii = 0.0f;
            float rate = 1.0f / time;
            Vector3 start = new Vector3(v.G.transform.eulerAngles.x, v.G.transform.eulerAngles.y, v.G.transform.eulerAngles.z);
            
            StopTween = false;
            
            Value v1 = Variables.Read(v.name, v.arrayIndex);
            v.moveTo = v1.moveTo;
            v.scaleTo = v1.scaleTo;
            v.rotateTo = false;
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            while( ii < 1.0 && StopTween == false )
            {
                ii += Time.deltaTime * rate;
                v.G.transform.eulerAngles = Vector3.Lerp(start, to, ii);
                yield return new WaitForEndOfFrame();
            }
            
            if ( ii >= 1.0f )
            {
                v.rotateTo = true;
            }
            
            Variables.StoreDirect(v, v.arrayIndex, v, true);
            
            yield return 0;
        }
    }
}