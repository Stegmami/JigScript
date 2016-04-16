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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class SceneObject
    {
        public GameObject go;
        
        private Vector3 position;
        private Vector3 localScale;
        private Vector3 eulerAngles;
        private Material m;
        private bool active;
        
        public SceneObject(GameObject go)
        {
            this.go = go;
            
            this.position = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z);
            this.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
            this.eulerAngles = new Vector3(go.transform.eulerAngles.x, go.transform.eulerAngles.y, go.transform.eulerAngles.z);
            this.active = go.activeSelf;
        }
        
        public void Reset()
        {
            this.go.transform.position = new Vector3(this.position.x, this.position.y, this.position.z);
            this.go.transform.localScale = new Vector3(this.localScale.x, this.localScale.y, this.localScale.z);
            this.go.transform.eulerAngles = new Vector3(this.eulerAngles.x, this.eulerAngles.y, this.eulerAngles.z);
            this.go.SetActive(this.active);
        }
    };
}