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
    public class GOScriptTrigger : MonoBehaviour
    {
        bool collisionActive;
    
        void Start()
        {
            this.collisionActive = true;
        }
    
        public void SetCollision(bool state)
        {
            this.collisionActive = state;
        }
        
        void OnTriggerEnter( Collider other )
        {
            if ( this.collisionActive )
            {
                GOValue gov = gameObject.GetComponent<GOValue>();
                GOValue goOther = other.gameObject.GetComponent<GOValue>();
                if ( gov != null && goOther != null )
                {
                    if ( gov.v.G.activeSelf && goOther.v.G.activeSelf )
                    {
                        gov.v.collision = true;
                        gov.v.other = new Value(goOther.v);
                        Variables.StoreDirect(gov.v, gov.v.arrayIndex, gov.v, true);
                    }
                }
            }
        }
    
        void OnTriggerExit( Collider other )
        {
            if ( this.collisionActive )
            {
                GOValue gov = gameObject.GetComponent<GOValue>();
                GOValue goOther = other.gameObject.GetComponent<GOValue>();
                if ( gov != null && goOther != null )
                {
                    gov.v.collision = false;
                    Variables.StoreDirect(gov.v, gov.v.arrayIndex, gov.v, true);
                }
            }
        }
    }
}