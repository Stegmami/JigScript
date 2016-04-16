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
using NightPen.JigScript;

public class GOUserFunctions : JigExtension
{

    JigCompiler jigCompiler;
    
    List<GameObject> createdGameObjects;
    List<SceneObject> sceneObjects;
    
    internal enum TweenType
    {
        None = 0,
        MoveTo = 1,
        ScaleTo = 2,
        RotateTo = 3
    }
    
    private bool GOExists( GameObject go, bool CreatedOnly )
    {
        bool rc = false;
        
        if ( CreatedOnly == false )
        {
            for( int ii=0; ii<sceneObjects.Count; ++ii )
            {
                if ( sceneObjects[ii].go == go )
                {
                    rc = true;
                    break;
                }
            }
        }
        if ( rc == false )
        {
            for( int ii=0; ii<createdGameObjects.Count; ++ii )
            {
                if ( createdGameObjects[ii] == null )
                {
                    continue;
                }
                if ( createdGameObjects[ii] == go )
                {
                    rc = true;
                    break;
                }
            }
        }
        return rc;
    }
    
    private GameObject SingleGameObjectFromValue( Value v, bool createdOnly )
    {
        GameObject go = null;
        
        if ( v.T == Value.ValueType.GObject )
        {
            if ( GOExists(v.G, createdOnly) )
            {
                go = v.G;
            }
        }
        else
        {
            Debug.LogError("GameObject " + v.S + " does not exist or is .");
        }
        
        return go;
    }
    
    private GameObject[] GameObjectFromValue( Value v, bool createdOnly )
    {
        List<GameObject> gos = new List<GameObject>();
        
        if ( v.T == Value.ValueType.GObject )
        {
            List<Value> ea = Variables.Expand(v, false);
            
            for( int ii=1; ii<ea.Count; ++ii )
            {
                if ( ea[ii].T == Value.ValueType.GObject )
                {
                    if ( GOExists(ea[ii].G, createdOnly) )
                    {
                        GOValue gov = ea[ii].G.GetComponent<GOValue>();
                        if ( gov )
                        {
                            gov.v.index = v.index;
                            gov.v.arrayIndex = ea[ii].arrayIndex;
                            gos.Add(ea[ii].G);
                        }
                    }
                }
            }
        }
        
        return gos.ToArray();
    }
    
    private Value InternalCreateGameObject(string name, GameObject go, bool active)
    {
        if ( string.IsNullOrEmpty(name) )
        {
            name = Variables.GetUniqueName();
        }
        
        if ( go == null )
        {
            go = new GameObject();
        }
        
        Value vRc = new Value(Value.ValueType.GObject, name);
        vRc.G = (GameObject)Instantiate(go);
        vRc.G.SetActive(active);

        Variables.CreateUnique(ref vRc);
        
        GOValue gov = vRc.G.GetComponent<GOValue>();
        if ( gov == null )
        {
            vRc.G.AddComponent<GOValue>();
            gov = vRc.G.GetComponent<GOValue>();
        }
        gov.v = vRc;
        
        bool found = false;
        for( int ii=0; ii<createdGameObjects.Count; ii++ )
        {
            if ( createdGameObjects[ii] == null )
            {
                createdGameObjects[ii] = vRc.G;
                found = true;
                break;
            }
        }
        if ( found == false )
        {
            createdGameObjects.Add(vRc.G);
        }
        
        return vRc;
    }
    
    private IEnumerator CreateFunction( List<Value> values )
    {
        if ( values.Count != 1 )
        {
            Debug.LogError("GameObjects.Create(gameobject variable);");
        }
        else
        {
            if ( values[0].T != Value.ValueType.GObject )
            {
                Debug.LogError("Parameter " + values[0].S + "is not a gameobject.");
            }
            else
            {
                Value vRc = InternalCreateGameObject(values[0].G.name, values[0].G, true);
                values.Add(vRc);
            }
        }
        
        yield return 0;
    }
    
    private IEnumerator DestroyFunction( List<Value> values )
    {
        if ( values.Count != 1 )
        {
            Debug.LogError("GameObjects.Destroy(gameobject);");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], true);
            if ( gos != null )
            {
                foreach( GameObject go in gos )
                {
                    for( int ii=0; ii<createdGameObjects.Count; ii++ )
                    {
                        if ( createdGameObjects[ii] == null )
                        {
                            continue;
                        }
                        if ( createdGameObjects[ii] == go )
                        {
                            createdGameObjects[ii] = null;
                            DestroyImmediate(go);
                            break;
                        }
                    }
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator ProcessGOTransform( List<Value>values, TweenType tweenType )
    {
        GameObject[] gos = GameObjectFromValue(values[0], false);
        if ( gos != null )
        {
            values[1].ConvertTo(Value.ValueType.Bool);
            values[2].ConvertTo(Value.ValueType.Float);
            values[3].ConvertTo(Value.ValueType.Float);
            values[4].ConvertTo(Value.ValueType.Float);
            values[5].ConvertTo(Value.ValueType.Float);
            Vector3 v = new Vector3(values[3].F, values[4].F, values[5].F);
            foreach( GameObject go in gos )
            {
                if ( go.name[0] == '@' )
                {
                    continue;
                }
                
                Vector3 vto = v;
                
                GOValue gov = go.GetComponent<GOValue>();
                if ( gov )
                {
                    switch( tweenType )
                    {
                        case TweenType.MoveTo:
                            if ( values[1].B )
                            {
                                vto = vto + go.transform.position;
                            }
                            StartCoroutine(gov.TweenPosition(values[2].F, vto));
                            break;
                        case TweenType.ScaleTo:
                            if ( values[1].B )
                            {
                                vto = vto + go.transform.localScale;
                            }
                            StartCoroutine(gov.TweenScale(values[2].F, vto));
                            break;
                        case TweenType.RotateTo:
                            if ( values[1].B )
                            {
                                vto = vto + go.transform.eulerAngles;
                            }
                            StartCoroutine(gov.TweenRotate(values[2].F, vto));
                            break;
                    }
                }
            }
        }
        yield return 0;
    }
    
    IEnumerator MoveFunction( List<Value> values )
    {
        if ( values.Count != 6 )
        {
            Debug.LogError("GameObjects.MoveTo(gameobject, [MoveTo.Relative | MoveTo.Absolute], time, x, y, z);");
        }
        else
        {
            StartCoroutine(ProcessGOTransform(values, TweenType.MoveTo));
        }
        yield return 0;
    }
    
    IEnumerator ScaleFunction( List<Value> values )
    {
        if ( values.Count != 6 )
        {
            Debug.LogError("GameObjects.ScaleTo(gameobject, [ScaleTo.Relative | ScaleTo.Absolute], time, x, y, z);");
        }
        else
        {
            StartCoroutine(ProcessGOTransform(values, TweenType.ScaleTo));
        }
        yield return 0;
    }
    
    IEnumerator RotateFunction( List<Value> values )
    {
        if ( values.Count != 6 )
        {
            Debug.LogError("GameObjects.RotateTo(gameobject, [RotateTo.Relative | RotateTo.Absolute], time, x, y, z);");
        }
        else
        {
            StartCoroutine(ProcessGOTransform(values, TweenType.RotateTo));
        }
        yield return 0;
    }
    
    
    void SetState( List<Value> values )
    {
        GameObject[] gos = GameObjectFromValue(values[0], false);
        if ( gos != null )
        {
            foreach( GameObject go in gos )
            {
                Animator animator = go.GetComponent<Animator>();
                if ( animator == null )
                {
                    Debug.LogError("Game object " + go.name + " does not have an Animator component.");
                }
                else
                {
                    switch( values[2].T )
                    {
                        case Value.ValueType.Bool:
                            animator.SetBool(values[1].S, values[2].B);
                            break;
                        case Value.ValueType.Integer:
                            animator.SetInteger(values[1].S, values[2].I);
                            break;
                        case Value.ValueType.Float:
                            animator.SetFloat(values[1].S, values[2].F);
                            break;
                    }
                }
            }
        }
    }
    
    IEnumerator SetIntegerParameter( List<Value> values )
    {
        if ( values.Count != 3 )
        {
            Debug.LogError("GameObjects.Animator.SetInteger(gameobject, \"state name\", value);");
        }
        else
        {
            values[2].ConvertTo(Value.ValueType.Integer);
            SetState(values);
        }
        yield return 0;
    }
    
    IEnumerator SetFloatParameter( List<Value> values )
    {
        if ( values.Count != 3 )
        {
            Debug.LogError("GameObjects.Animator.SetFloat(gameobject, \"state name\", value);");
        }
        else
        {
            values[2].ConvertTo(Value.ValueType.Float);
            SetState(values);
        }
        yield return 0;
    }
    
    IEnumerator SetBoolParameter( List<Value> values )
    {
        if ( values.Count != 3 )
        {
            Debug.LogError("GameObjects.Animator.SetBool(gameobject, \"state name\", value);");
        }
        else
        {
            values[2].ConvertTo(Value.ValueType.Bool);
            SetState(values);
        }
        yield return 0;
    }
    
    Value GetAnimatorParameter( GameObject go, string name, Value.ValueType T )
    {
        Value vRc = new Value(Value.ValueType.None, "");
        
        Animator animator = go.GetComponent<Animator>();
        if ( animator != null )
        {
            switch( T )
            {
                case Value.ValueType.Integer:
                    vRc = new Value(animator.GetInteger(name), "AnimatorIntParameter");
                    break;
                case Value.ValueType.Float:
                    vRc = new Value(animator.GetFloat(name), "AnimatorFloatParameter");
                    break;
                case Value.ValueType.Bool:
                    vRc = new Value(animator.GetBool(name), "AnimatorBoolParameter");
                    break;
            }
        }
        else
        {
            Debug.LogError("Game object " + go.name + " does not have an Animator component.");
        }
        
        return vRc;
    }
    
    IEnumerator GetIntegerParameter( List<Value> values )
    {
        if ( values.Count != 2 )
        {
            Debug.LogError("state = GameObjects.Animator.GetInteger(gameobject, \"state name\");");
        }
        else
        {
            GameObject go = SingleGameObjectFromValue(values[0], false);
            if ( go != null )
            {
                Animator animator = go.GetComponent<Animator>();
                if ( animator != null )
                {
                    values[1].ConvertTo(Value.ValueType.String);
                    Value v = GetAnimatorParameter(go, values[1].S, Value.ValueType.Integer);
                    if ( v.T == Value.ValueType.Integer )
                    {
                        values.Add(v);
                    }
                }
                else
                {
                    Debug.LogError("Game object " + go.name + " does not have an Animator component.");
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator GetFloatParameter( List<Value> values )
    {
        if ( values.Count != 2 )
        {
            Debug.LogError("state = GameObjects.Animator.GetInteger(gameobject, \"state name\");");
        }
        else
        {
            GameObject go = SingleGameObjectFromValue(values[0], false);
            if ( go != null )
            {
                Animator animator = go.GetComponent<Animator>();
                if ( animator != null )
                {
                    values[1].ConvertTo(Value.ValueType.String);
                    Value v = GetAnimatorParameter(go, values[1].S, Value.ValueType.Float);
                    if ( v.T == Value.ValueType.Float )
                    {
                        values.Add(v);
                    }
                }
                else
                {
                    Debug.LogError("Game object " + go.name + " does not have an Animator component.");
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator GetBoolParameter( List<Value> values )
    {
        if ( values.Count != 2 )
        {
            Debug.LogError("state = GameObjects.Animator.GetInteger(gameobject, \"state name\");");
        }
        else
        {
            GameObject go = SingleGameObjectFromValue(values[0], false);
            if ( go != null )
            {
                Animator animator = go.GetComponent<Animator>();
                if ( animator != null )
                {
                    values[1].ConvertTo(Value.ValueType.String);
                    Value v = GetAnimatorParameter(go, values[1].S, Value.ValueType.Bool);
                    if ( v.T == Value.ValueType.Bool )
                    {
                        values.Add(v);
                    }
                }
                else
                {
                    Debug.LogError("Game object " + go.name + " does not have an Animator component.");
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator AddForceFunction( List<Value> values )
    {
        if ( values.Count != 4 )
        {
            Debug.LogError("GameObjects.AddForce(gameobject, xforce, yforce, zforce);");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                foreach( GameObject go in gos )
                {
                    Rigidbody rigidbody = go.GetComponent<Rigidbody>();
                    if ( rigidbody != null )
                    {
                        values[1].ConvertTo(Value.ValueType.Float);
                        values[2].ConvertTo(Value.ValueType.Float);
                        values[3].ConvertTo(Value.ValueType.Float);
                        rigidbody.AddForce(new Vector3(values[1].F, values[2].F, values[3].F));
                    }
                    else
                    {
                        Debug.LogError("Game object " + go.name + " does not have a Rigidbody component.");
                        break;
                    }
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator ComparePositionFunction( List<Value> values )
    {
        if ( values.Count != 2 )
        {
            Debug.LogError("bool = GameObject.ComparePositions(gameobject1, gameobject2);");
            yield return 0;
        }
        
        GameObject go = SingleGameObjectFromValue(values[0], false);
        GameObject go1 = SingleGameObjectFromValue(values[1], false);
        
        Value v = new Value(false, "GameObjects.ComparePositions");
        
        if ( go != null && go1 != null )
        {
            Collider c1 = go.GetComponent<Collider>();
            Collider c2 = go1.GetComponent<Collider>();
            if ( c1 != null && c2 != null )
            {
                v.B = c1.bounds.Intersects(c2.bounds);
            }
            else
            {
                if ( c1 == null )
                {
                    Debug.LogError("There is no collider component on the " + go.name + " game object.");
                }
                if ( c2 == null )
                {
                    Debug.LogError("There is no collider component on the " + go1.name + " game object.");
                }
            }
        }
        values.Add(v);
        
        yield return 0;
    }
    
    IEnumerator GetDistanceFunction( List<Value> values )
    {
        Value v = new Value(999999999.0f, "GameObjects.GetDistance");
        
        if ( values.Count != 2 )
        {
            Debug.LogError("bool = GameObjects.GetDistance(gameobject1, gameobject2);");
            yield return 0;
        }
        else
        {
            GameObject go = SingleGameObjectFromValue(values[0], false);
            GameObject go1 = SingleGameObjectFromValue(values[1], false);
            
            if ( go != null && go1 != null )
            {
                v.F = Vector3.Distance(go.transform.position, go1.transform.position);
            }
        }
        
        values.Add(v);
        
        yield return 0;
    }
    
    private void SetRigidbodyValues( GameObject go, string parameter, List<Value> values )
    {
        Rigidbody rigidbody = go.GetComponent<Rigidbody>();
        
        switch( parameter )
        {
            default:
                Debug.LogError("parameter " + parameter + " is not supported.");
                break;
            case "angularDrag":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.angularDrag = values[2].F;
                break;
            case "angularVelocity":
                values[2].ConvertTo(Value.ValueType.Float);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                rigidbody.angularVelocity = new Vector3(values[2].F, values[3].F, values[4].F);
                break;
            case "centerOfMass":
                values[2].ConvertTo(Value.ValueType.Float);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                rigidbody.centerOfMass = new Vector3(values[2].F, values[3].F, values[4].F);
                break;
            case "collisionDetectionMode":
                values[2].ConvertTo(Value.ValueType.Integer);
                rigidbody.collisionDetectionMode = (CollisionDetectionMode)values[2].I;
                break;
            case "constraints":
                values[2].ConvertTo(Value.ValueType.Integer);
                rigidbody.constraints = (RigidbodyConstraints)values[2].I;
                break;
            case "detectCollisions":
                values[2].ConvertTo(Value.ValueType.Bool);
                rigidbody.detectCollisions = values[2].B;
                break;
            case "drag":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.drag = values[2].F;
                break;
            case "freezeRotation":
                values[2].ConvertTo(Value.ValueType.Bool);
                rigidbody.freezeRotation = values[2].B;
                break;
            case "inertiaTensor":
                Debug.Log("Not Implemented");
                break;
            case "interpolation":
                values[2].ConvertTo(Value.ValueType.Integer);
                rigidbody.interpolation = (RigidbodyInterpolation)values[2].I;
                break;
            case "isKinematic":
                values[2].ConvertTo(Value.ValueType.Bool);
                rigidbody.isKinematic = values[2].B;
                break;
            case "mass":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.mass = values[2].F;
                break;
            case "maxAngularVelocity":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.maxAngularVelocity = values[2].F;
                break;
            case "position":
                values[2].ConvertTo(Value.ValueType.Float);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                rigidbody.position = new Vector3(values[2].F, values[3].F, values[4].F);
                break;
            case "rotation":
                rigidbody.rotation = values[2].Q;
                break;
            case "sleepAngularVelocity":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.sleepAngularVelocity = values[2].F;
                break;
            case "sleepVelocity":
                values[2].ConvertTo(Value.ValueType.Float);
                rigidbody.sleepVelocity = values[2].F;
                break;
            case "solverIterationCount":
                values[2].ConvertTo(Value.ValueType.Integer);
                rigidbody.solverIterationCount = values[2].I;
                break;
            case "useConeFriction":
                values[2].ConvertTo(Value.ValueType.Bool);
                rigidbody.useConeFriction = values[2].B;
                break;
            case "useGravity":
                values[2].ConvertTo(Value.ValueType.Bool);
                rigidbody.useGravity = values[2].B;
                break;
            case "velocity":
                values[2].ConvertTo(Value.ValueType.Float);
                values[3].ConvertTo(Value.ValueType.Float);
                values[4].ConvertTo(Value.ValueType.Float);
                rigidbody.isKinematic = false;
                rigidbody.velocity = new Vector3(values[2].F, values[3].F, values[4].F);
                break;
        }
    }
    
    IEnumerator SetRigidbodyFunction( List<Value> values )
    {
        if ( values.Count < 3 )
        {
            Debug.LogError("GameObjects.Rigidbody.Set(game object, parameter, values);");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                foreach( GameObject go in gos )
                {
                    Rigidbody rigidbody = go.GetComponent<Rigidbody>();
                    if ( rigidbody == null )
                    {
                        Debug.LogError("Game object " + go.name + " does not have a Rigidbody component.");
                        break;
                    }
                    values[1].ConvertTo(Value.ValueType.String);
                    SetRigidbodyValues(go, values[1].S, values);
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator ClearFunction( List<Value> values )
    {
        foreach( GameObject go in createdGameObjects )
        {
            if ( go == null )
            {
                continue;
            }
            DestroyImmediate(go);
        }
        createdGameObjects.Clear();
        
        yield return 0;
    }
    
    public IEnumerator ResetObjects()
    {
        foreach( SceneObject so in sceneObjects )
        {
            if ( so.go == null )
            {
                continue;
            }
            
            GOValue gov = so.go.GetComponent<GOValue>();
            if ( gov != null )
            {
                gov.StopTween = true;
            }
        }
        
        yield return new WaitForEndOfFrame();
        
        foreach( SceneObject so in sceneObjects )
        {
            if ( so.go == null )
            {
                continue;
            }
            so.Reset();
        }
        foreach( GameObject go in createdGameObjects )
        {
            if ( go == null )
            {
                continue;
            }
            DestroyImmediate(go);
        }
        createdGameObjects.Clear();
        
        yield return 0;
    }
    
    IEnumerator ResetGameObjectsFunction( List<Value> values )
    {
        yield return StartCoroutine(ResetObjects());
    }
    
    IEnumerator SetMinDragDistanceFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.SetMinDrag(gameobject variable or array, distance)");
        }
        else
        {
            values[1].ConvertTo(Value.ValueType.Float);
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                foreach( GameObject go in gos )
                {
                    GOClicked clicked = go.GetComponent<GOClicked>();
                    if ( clicked == null )
                    {
                        go.AddComponent<GOClicked>();
                        clicked = go.GetComponent<GOClicked>();
                    }
                    clicked.SetMinDragDistance(values[1].F);
                }
            }
        }
        
        yield return 0;
    }
    
    IEnumerator IsClickedFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsClicked(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = false;
                
                foreach( GameObject go in gos )
                {
                    GOClicked goc = go.GetComponent<GOClicked>();
                    if ( goc != null && goc.IsClicked() )
                    {
                        rc = true;
                        break;
                    }
                }
                values.Add(new Value(rc, "GameObjects.IsClicked"));
            }
        }
        yield return 0;
    }
    
    IEnumerator IsSelectedFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsSelected(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = false;
                
                foreach( GameObject go in gos )
                {
                    GOClicked goc = go.GetComponent<GOClicked>();
                    if ( goc != null && goc.IsSelected() )
                    {
                        rc = true;
                        break;
                    }
                }
                values.Add(new Value(rc, "GameObjects.IsSelected"));
            }
        }
        yield return 0;
    }
    
    IEnumerator IsDraggingFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsDragging(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = false;
                
                foreach( GameObject go in gos )
                {
                    GOClicked goc = go.GetComponent<GOClicked>();
                    if ( goc != null && goc.IsDragging() )
                    {
                        rc = true;
                        break;
                    }
                }
                values.Add(new Value(rc, "GameObjects.IsDragging"));
            }
        }
        yield return 0;
    }
    
    IEnumerator IsDragBeginFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsDragBegin(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = false;
                
                foreach( GameObject go in gos )
                {
                    GOClicked goc = go.GetComponent<GOClicked>();
                    if ( goc != null && goc.IsDragBegin() )
                    {
                        rc = true;
                        break;
                    }
                }
                values.Add(new Value(rc, "GameObjects.IsDragBegin"));
            }
        }
        yield return 0;
    }
    
    IEnumerator IsDragEndFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsDragEnd(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = false;
                
                foreach( GameObject go in gos )
                {
                    GOClicked goc = go.GetComponent<GOClicked>();
                    if ( goc != null && goc.IsDragEnd() )
                    {
                        rc = true;
                        break;
                    }
                }
                values.Add(new Value(rc, "GameObjects.IsDragEnd"));
            }
        }
        yield return 0;
    }
    
    IEnumerator MousePositionFunction( List<Value> values )
    {
        if ( values.Count != 1 )
        {
            Debug.LogError("vector = GameObjects.MousePosition(gameobject)");
        }
        else
        {
            GameObject go = SingleGameObjectFromValue(values[0], false);
            if ( go != null )
            {
                Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z) - Mathf.Abs(go.transform.position.z));
                pos = Camera.main.ScreenToWorldPoint(pos);
                Value v = new Value(Value.ValueType.Vector, "GameObjects.MousePosition");
                v.V = new Vector3(pos.x, pos.y, pos.z);
                values.Add(v);
            }
        }
        yield return 0;
    }
    
    IEnumerator IsMoveCompleteFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.IsMoveComplete(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = true;
                
                foreach( GameObject go in gos )
                {
                    GOValue gov = go.GetComponent<GOValue>();
                    if ( gov != null )
                    {
                        Value v = jigCompiler.ReadVariable(gov.v.name, gov.v.arrayIndex);
                        if ( v.moveTo == false )
                        {
                            rc = false;
                        }
                        else
                        {
                            v.moveTo = false;
                            Variables.Store(v, v.arrayIndex, new Value(v));
                        }
                    }
                }
                values.Add(new Value(rc, "allMoved"));
            }
        }
        yield return 0;
    }
    
    IEnumerator GetLocationFunction( List<Value>values )
    {
        if ( values.Count != 2 )
        {
            Debug.LogError("GameObjects.GetLocation(gameobject, forward gameobject)");
        }
        
        GameObject other = SingleGameObjectFromValue(values[0], false);
        GameObject forward = SingleGameObjectFromValue(values[1], false);
        
        if ( forward != null && other != null )
        {
            Vector3 f = forward.transform.position.normalized;
            Vector3 o = other.transform.position.normalized;
            Value v = new Value(Vector3.Dot(f, o), "GameObjects.GetLocation");
            values.Add(v);
        }
        
        yield return 0;
    }
    
    IEnumerator IsScaleCompleteFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.isScaleComplete(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = true;
                
                foreach( GameObject go in gos )
                {
                    GOValue gov = go.GetComponent<GOValue>();
                    if ( gov != null )
                    {
                        Value v = jigCompiler.ReadVariable(gov.v.name, gov.v.arrayIndex);
                        if ( v.scaleTo == false )
                        {
                            rc = false;
                        }
                        else
                        {
                            v.scaleTo = false;
                            Variables.Store(v, v.arrayIndex, new Value(v));
                        }
                    }
                }
                values.Add(new Value(rc, "allScaled"));
            }
        }
        yield return 0;
    }
    
    IEnumerator IsRotateCompleteFunction( List<Value> values )
    {
        if ( values.Count < 1 )
        {
            Debug.LogError("GameObjects.isRotateComplete(gameobject variable or array)");
        }
        else
        {
            GameObject[] gos = GameObjectFromValue(values[0], false);
            if ( gos != null )
            {
                bool rc = true;
                
                foreach( GameObject go in gos )
                {
                    GOValue gov = go.GetComponent<GOValue>();
                    if ( gov != null )
                    {
                        Value v = jigCompiler.ReadVariable(gov.v.name, gov.v.arrayIndex);
                        if ( v.rotateTo == false )
                        {
                            rc = false;
                        }
                        else
                        {
                            v.rotateTo = false;
                            Variables.Store(v, v.arrayIndex, new Value(v));
                        }
                    }
                }
                values.Add(new Value(rc, "allRotated"));
            }
        }
        yield return 0;
    }
    
    static string IsLoadableGameObject( GameObject go )
    {
        if ( go.name == "Main Camera" )
        {
            return "MainCamera";
        }
        
        if ( go.tag == "JigIgnore" )
        {
            return "";
        }
        
        #if UNITY_EDITOR
        if ( UnityEditor.PrefabUtility.GetPrefabParent(go) == null && UnityEditor.PrefabUtility.GetPrefabObject(go) != null )
        {
            return string.Empty;
        }
        #endif
        
        if ( string.IsNullOrEmpty(go.name) ||
            go.name == "HandlesGO" ||
            go.name == "Cylinder" || go.name == "cylinder" ||
            go.name == "Cone" || go.name == "cone" ||
            go.name == "Sphere" || go.name == "sphere" ||
            go.name == "Cube" || go.name == "cube" ||
            go.name == "Torus" || go.name == "torus" ||
            go.name == "PreviewMaterials" ||
            go.name == "dial_plane" ||
            go.name == "dial_arrow" ||
            go.name == "JigScript" ||
            go.name == "Preview Camera" ||
            go.name == "DefaultGeneric" ||
            go.name == "dial_flat" ||
            go.name == "arrow" || go.name == "root" )
        {
            return string.Empty;
        }
        
        if ( go.transform.parent == null )
        {
            return go.name;
        }
        return IsLoadableGameObject(go.transform.parent.gameObject) + "." + go.name;
    }

    //if game objects same name
    //add second object as array element.
    

    private void ImportSceneGameObjects()
    {
        GameObject[] gos = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
        
        foreach( GameObject go in gos )
        {
            string name = IsLoadableGameObject(go);

            if ( string.IsNullOrEmpty(name) == false )
            {
                if ( go.name[0] == '@' )
                {
                    Value v = new Value(Value.ValueType.GObject, go.name.Substring(1));
                    v.G = go;
                    name = name.Remove(name.IndexOf('@'), 1);
                    Variables.Create(name, new Value(v));
                }
            }
        }
        foreach( GameObject go in gos )
        {
            string name = IsLoadableGameObject(go);
            
            if ( string.IsNullOrEmpty(name) )
            {
                continue;
            }
            
            if ( go.name[0] == '@' )
            {
                continue;
            }
            
            int index = name.IndexOf('@');
            if ( index >= 0 )
            {
                name = name.Remove(name.IndexOf('@'), 1);
                index = name.LastIndexOf('.');
                if ( index >= 0 )
                {
                    string arrayName = name.Substring(0, index);
                    int arrayIndex = -1;
                    int.TryParse(name.Substring(index + 1), out arrayIndex);
                    
                    index = Variables.Index(arrayName);
                    
                    if ( index >= 0 && arrayIndex >= 0 )
                    {
                        Value v = new Value(Value.ValueType.GObject, arrayName);
                        v.G = go;
                        v.index = index;
                        v.arrayIndex = arrayIndex;
                        
                        GOValue gov = v.G.GetComponent<GOValue>();
                        if ( gov == null )
                        {
                            gov = v.G.AddComponent<GOValue>();
                        }
                        gov.v = v;
/*                        
                        if ( v.G.GetComponent<Collider>() == null )
                        {
                            v.G.AddComponent<BoxCollider>();
                        }
                        if ( v.G.GetComponent<GOClicked>() == null )
                        {
                            v.G.AddComponent<GOClicked>();
                        }
                        if ( v.G.GetComponent<Rigidbody>() == null )
                        {
                            v.G.AddComponent<Rigidbody>();
                            
                            Rigidbody rigidbody = v.G.GetComponent<Rigidbody>();
                            rigidbody.isKinematic = true;
                        }
*/
                        Variables.Store(v, v.arrayIndex, new Value(v));
                        sceneObjects.Add(new SceneObject(v.G));
                    }
                }
            }
            else
            {
                Value v = new Value(Value.ValueType.GObject, name);
                v.G = go;
                GOValue gov = v.G.GetComponent<GOValue>();
                if ( gov == null )
                {
                    gov = v.G.AddComponent<GOValue>();
                }
                if ( gov == null )
                {
                }
                else
                {
                    gov.v = v;
/*                
                    if ( go.name != "Main Camera" )
                    {
                        gov.v = v;
                        if ( v.G.GetComponent<Collider>() == null )
                        {
                            v.G.AddComponent<BoxCollider>();
                        }
                        if ( v.G.GetComponent<GOClicked>() == null )
                        {
                            v.G.AddComponent<GOClicked>();
                        }
                        if ( v.G.GetComponent<Rigidbody>() == null )
                        {
                            v.G.AddComponent<Rigidbody>();
                            
                            Rigidbody rigidbody = v.G.GetComponent<Rigidbody>();
                            rigidbody.isKinematic = true;
                        }
                    }
*/
                }
                if ( Variables.Exists(name) )
                {
                    Value v1 = Variables.Read(name, 0);
                    int count = Variables.Length(v1);
                    v1.arrayIndex = count+1;
                    v1.isVariable = true;
                    gov = v.G.GetComponent<GOValue>();
                    gov.v.arrayIndex = v1.arrayIndex;
                    gov.v = v;
                    Variables.Store(v1, v1.arrayIndex, new Value(v));
                }
                else
                {
                    Variables.Create(name, new Value(v));
                }
                sceneObjects.Add(new SceneObject(v.G));
            }
        }
    }
    
    private Value GetDragBegin( Value v )
    {
        if (v.T == Value.ValueType.GObject)
        {
            GOClicked goc = v.G.GetComponent<GOClicked>();
            if (goc)
            {
                v.T = Value.ValueType.Bool;
                v.B = goc.IsDragBegin();
            }
        }
        return v;
    }
    
    private Value GetCollision( Value v )
    {
        bool collision = false;
        
        if ( v.T == Value.ValueType.GObject )
        {
            GOValue gov = v.G.GetComponent<GOValue>();
            if ( gov )
            {
                collision = gov.v.collision;
                gov.v.collision = false;
                Variables.Store(gov.v, gov.v.arrayIndex, new Value(gov.v));
            }
        }
        
        v.T = Value.ValueType.Bool;
        v.B = collision;
        
        return v;
    }
    
    private void SetCollision(Value dest, int arrayIndex, Value source)
    {
        if (dest.T != Value.ValueType.GObject )
        {
            return;
        }
        
        source.ConvertTo(Value.ValueType.Bool);
        
        List<Value> va = Variables.Expand(dest, false);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            GOScriptTrigger gost = va[ii].G.GetComponent<GOScriptTrigger>();
            
            if ( va[ii].G.GetComponent<Rigidbody>() == null )
            {
                if ( va[ii].G.GetComponent<Rigidbody>() == null )
                {
                    va[ii].G.AddComponent<Rigidbody>();
                    Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                    rigidbody.isKinematic = true;
                }
            }
            if ( va[ii].G.GetComponent<Collider>() == null )
            {
                if ( va[ii].G.GetComponent<Collider>() == null )
                {
                    va[ii].G.AddComponent<BoxCollider>();
                }
            }
            
            if ( va[ii].G.GetComponent<GOScriptTrigger>() == null )
            {
                va[ii].G.AddComponent<GOScriptTrigger>();
                gost = va[ii].G.GetComponent<GOScriptTrigger>();
                
                Collider c = va[ii].G.GetComponent<Collider>();
                if ( c != null )
                {
                    c.isTrigger = true;
                }
            }
            else
            {
                gost.SetCollision(source.B);
            }
        }
    }
    
    private Value GetOther(Value v)
    {
        GOValue gov = v.G.GetComponent<GOValue>();
        if ( gov )
        {
            Value v1 = gov.v.other ?? gov.v;
            v = new Value(v1);
        }
        
        return v;
    }
    
    private Value GetSelected( Value v )
    {
        if (v.T == Value.ValueType.GObject)
        {
            GOClicked goc = v.G.GetComponent<GOClicked>();
            if (goc)
            {
                v.T = Value.ValueType.Bool;
                v.B = goc.IsSelected();
            }
        }
        return v;
    }
    
    private void SetMaterial( Value dest, int arrayIndex, Value source )
    {
        if ( source.T != Value.ValueType.GObject )
        {
            return;
        }
        
        JigMat jm = source.G.GetComponent<JigMat>();
        if ( jm == null )
        {
            return;
        }
        
        List<Value> va = Variables.Expand(dest, false);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject && va[ii].G.GetComponent<Renderer>() != null )
            {
                va[ii].G.GetComponent<Renderer>().material = jm.material;
            }
        }
    }
    
    private Value GetDraging( Value v )
    {
        if (v.T == Value.ValueType.GObject)
        {
            GOClicked goc = v.G.GetComponent<GOClicked>();
            if (goc)
            {
                v.T = Value.ValueType.Bool;
                v.B = goc.IsDragging();
            }
        }
        return v;
    }
    
    private Value GetDragEnd( Value v )
    {
        if (v.T == Value.ValueType.GObject)
        {
            GOClicked goc = v.G.GetComponent<GOClicked>();
            if (goc)
            {
                v.T = Value.ValueType.Bool;
                v.B = goc.IsDragEnd();
            }
        }
        return v;
    }
    
    private Value GetHover( Value v )
    {
        if (v.T == Value.ValueType.GObject)
        {
            GOClicked goc = v.G.GetComponent<GOClicked>();
            if (goc)
            {
                v.T = Value.ValueType.Bool;
                v.B = goc.IsHover();
            }
        }
        return v;
    }
    
    private Value GetPX( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.position.x;
        }
        return v;
    }
    
    private void SetPX( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.position = new Vector3(source.F, va[ii].G.transform.position.y, va[ii].G.transform.position.z);
            }
        }
    }
    
    private Value GetPY( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.position.y;
        }
        return v;
    }
    
    private void SetPY( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.position = new Vector3(va[ii].G.transform.position.x, source.F, va[ii].G.transform.position.z);
            }
        }
    }
    
    private Value GetPZ( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.position.z;
        }
        return v;
    }
    
    private void SetPZ( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.position = new Vector3(va[ii].G.transform.position.x, va[ii].G.transform.position.y, source.F);
            }
        }
    }
    
    private Value GetSX( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.localScale.x;
        }
        return v;
    }
    
    private void SetSX( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.localScale = new Vector3(source.F, va[ii].G.transform.localScale.y, va[ii].G.transform.localScale.z);
            }
        }
    }
    
    private Value GetSY( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.localScale.y;
        }
        return v;
    }
    
    private void SetSY( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.localScale = new Vector3(va[ii].G.transform.localScale.x, source.F, va[ii].G.transform.localScale.z);
            }
        }
    }
    
    private Value GetSZ( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.localScale.z;
        }
        
        return v;
    }
    
    private void SetSZ( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.localScale = new Vector3(va[ii].G.transform.localScale.x, va[ii].G.transform.localScale.y, source.F);
            }
        }
    }
    
    private Value GetRX( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.eulerAngles.x;
        }
        
        return v;
    }
    
    private void SetRX( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.eulerAngles = new Vector3(source.F, va[ii].G.transform.eulerAngles.y, va[ii].G.transform.eulerAngles.z);
            }
        }
    }
    
    private Value GetRY( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.eulerAngles.y;
        }
        
        return v;
    }
    
    private void SetRY( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.eulerAngles = new Vector3(va[ii].G.transform.eulerAngles.x, source.F, va[ii].G.transform.eulerAngles.z);
            }
        }
    }
    
    private Value GetRZ( Value v )
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Float;
            v.F = v.G.transform.eulerAngles.z;
        }
        
        return v;
    }
    
    private void SetRZ( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.transform.eulerAngles = new Vector3(va[ii].G.transform.eulerAngles.x, va[ii].G.transform.eulerAngles.y, source.F);
            }
        }
    }
    
    private Value GetVX( Value v )
    {
        v.T = Value.ValueType.Float;
        v.F = v.V.x;
        return v;
    }
    
    private void SetVX( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.Vector )
            {
                va[ii].V.x = source.F;
                Variables.Store(dest, ii, va[ii]);
            }
        }
    }
    
    private Value GetVY( Value v )
    {
        v.T = Value.ValueType.Float;
        v.F = v.V.y;
        return v;
    }
    
    private void SetVY( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.Vector )
            {
                va[ii].V.y = source.F;
                Variables.Store(dest, ii, va[ii]);
            }
        }
    }
    
    private Value GetVZ( Value v )
    {
        v.T = Value.ValueType.Float;
        v.F = v.V.z;
        return v;
    }
    
    private void SetVZ( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.Vector )
            {
                va[ii].V.z = source.F;
                Variables.Store(dest, ii, va[ii]);
            }
        }
    }
    
    private void SetClickable( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Bool);
                
                if ( va[ii].G.GetComponent<Collider>() == null )
                {
                    va[ii].G.AddComponent<BoxCollider>();
                }
                
                GOClicked goc = va[ii].G.GetComponent<GOClicked>();
                if (goc)
                {
                    goc.SetClickable(source.B);
                }
            }
        }
    }
    
    private void SetPhysics( Value dest, int arrayIndex, Value source )
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Bool);
                if ( va[ii].G.GetComponent<Collider>() == null )
                {
                    va[ii].G.AddComponent<BoxCollider>();
                }
                if ( va[ii].G.GetComponent<Rigidbody>() == null )
                {
                    va[ii].G.AddComponent<Rigidbody>();
                }
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                rigidbody.isKinematic = source.B;
            }
        }
    }
    
    private Value GetActiveProperty(Value v)
    {
        if ( v.T == Value.ValueType.GObject )
        {
            v.T = Value.ValueType.Bool;
            v.B = v.G.activeSelf;
        }
        
        return v;
    }
    
    private void SetActiveProperty(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Bool);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                va[ii].G.SetActive(source.B);
            }
        }
    }
    
    private Value GetMoveTransformStatus(Value v)
    {
        v.B = false;
        
        if ( v.T == Value.ValueType.GObject )
        {
            if ( v.arrayIndex == 0 )
            {
                v.moveTo = true;
                List<Value> ea = Variables.Expand(v, false);
                foreach(Value v1 in ea)
                {
                    if ( v1.moveTo == false )
                    {
                        v.moveTo = false;
                        break;
                    }
                }
                if ( v.moveTo )
                {
                    foreach(Value v1 in ea)
                    {
                        v1.moveTo = false;
                        Variables.StoreDirect(v1, v1.arrayIndex, v1, false);
                    }
                }
            }
            if ( v.moveTo )
            {
                v.moveTo = false;
                Variables.StoreDirect(v, v.arrayIndex, v, false);
                v.B = true;
            }
        }
        v.T = Value.ValueType.Bool;
        
        return v;
    }
    
    private Value GetScaleTransformStatus(Value v)
    {
        v.B = false;
        
        if ( v.T == Value.ValueType.GObject )
        {
            if ( v.arrayIndex == 0 )
            {
                v.scaleTo = true;
                List<Value> ea = Variables.Expand(v, false);
                foreach(Value v1 in ea)
                {
                    if ( v1.scaleTo == false )
                    {
                        v.scaleTo = false;
                        break;
                    }
                }
                if ( v.scaleTo )
                {
                    foreach(Value v1 in ea)
                    {
                        v1.scaleTo = false;
                        Variables.StoreDirect(v1, v1.arrayIndex, v1, false);
                    }
                }
            }
            if ( v.scaleTo )
            {
                v.scaleTo = false;
                Variables.StoreDirect(v, v.arrayIndex, v, false);
                v.B = true;
            }
        }
        v.T = Value.ValueType.Bool;
        
        return v;
    }
    
    private Value GetRotateTransformStatus(Value v)
    {
        v.B = false;
        
        if ( v.T == Value.ValueType.GObject )
        {
            if ( v.arrayIndex == 0 )
            {
                v.rotateTo = true;
                List<Value> ea = Variables.Expand(v, false);
                foreach(Value v1 in ea)
                {
                    if ( v1.rotateTo == false )
                    {
                        v.rotateTo = false;
                        break;
                    }
                }
                if ( v.rotateTo )
                {
                    foreach(Value v1 in ea)
                    {
                        v1.rotateTo = false;
                        Variables.StoreDirect(v1, v1.arrayIndex, v1, false);
                    }
                }
            }
            if ( v.rotateTo )
            {
                v.rotateTo = false;
                Variables.StoreDirect(v, v.arrayIndex, v, false);
                v.B = true;
            }
        }
        v.T = Value.ValueType.Bool;
        
        return v;
    }
    
    private void SetAngularDrag(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.angularDrag = source.F;
                }
            }
        }
    }
    
    private void SetAngularVelocity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Vector);
                    rigidbody.angularVelocity = source.V;
                }
            }
        }
    }
    
    private void SetcenterOfMass(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Vector);
                    rigidbody.centerOfMass = source.V;
                }
            }
        }
    }
    
    private void SetCollisionDetectionMode(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Integer);
                    rigidbody.collisionDetectionMode = (CollisionDetectionMode)source.I;
                }
            }
        }
    }
    
    private void SetConstraints(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Integer);
                    rigidbody.constraints = (RigidbodyConstraints)source.I;
                }
            }
        }
    }
    
    private void SetDetectCollisions(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    rigidbody.detectCollisions = source.B;
                }
            }
        }
    }
    
    private void SetDrag(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.drag = source.F;
                }
            }
        }
    }
    
    private void SetFreezeRotation(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    rigidbody.freezeRotation = source.B;
                }
            }
        }
    }
    
    private void SetInertiaTensor(Value dest, int arrayIndex, Value source)
    {
    }
    
    private void SetInterpolation(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Integer);
                    rigidbody.interpolation = (RigidbodyInterpolation)source.I;
                }
            }
        }
    }
    
    private void SetIsKinematic(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    rigidbody.isKinematic = source.B;
                }
            }
        }
    }
    
    private void SetMass(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.mass = source.F;
                }
            }
        }
    }
    
    private void SetMaxAngularVelocity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.maxAngularVelocity = source.F;
                }
            }
        }
    }
    
    private void SetPosition(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Vector);
                    rigidbody.position = source.V;
                }
            }
        }
    }
    
    private void SetRotation(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Quaternion);
                    rigidbody.rotation = source.Q;
                }
            }
        }
    }
    
    private void SetSleepAngularVelocity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.sleepAngularVelocity = source.F;
                }
            }
        }
    }
    
    private void SetSleepVelocity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Float);
                    rigidbody.sleepVelocity = source.F;
                }
            }
        }
    }
    
    private void SetSolverIterationCount(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Integer);
                    rigidbody.solverIterationCount = source.I;
                }
            }
        }
    }
    
    private void SetUseConeFriction(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    rigidbody.useConeFriction = source.B;
                }
            }
        }
    }
    
    private void SetUseGravity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Bool);
                    rigidbody.useGravity = source.B;
                }
            }
        }
    }
    
    private void SetVelocity(Value dest, int arrayIndex, Value source)
    {
        List<Value> va = Variables.Expand(dest, false);
        
        source.ConvertTo(Value.ValueType.Float);
        
        for(int ii=1; ii<va.Count; ++ii)
        {
            if ( va[ii].T == Value.ValueType.GObject )
            {
                source.ConvertTo(Value.ValueType.Float);
                Rigidbody rigidbody = va[ii].G.GetComponent<Rigidbody>();
                if ( rigidbody == null )
                {
                    Debug.Log("Physics is not enabled for game object " + va[ii].G.name);
                }
                else
                {
                    source.ConvertTo(Value.ValueType.Vector);
                    rigidbody.velocity = source.V;
                }
            }
        }
    }
    
    public override void Initialize(JigCompiler compiler)
    {
        this.jigCompiler = compiler;
        
        createdGameObjects = new List<GameObject>();
        sceneObjects = new List<SceneObject>();
        
        Variables.Create("CollisionDetectionMode.Continuous", new Value((int)CollisionDetectionMode.Continuous, "CollisionDetectionMode.Continuous"));
        Variables.Create("CollisionDetectionMode.ContinuousDynamic", new Value((int)CollisionDetectionMode.ContinuousDynamic, "CollisionDetectionMode.ContinuousDynamic"));
        Variables.Create("CollisionDetectionMode.Discrete", new Value((int)CollisionDetectionMode.Discrete, "CollisionDetectionMode.Discrete"));
        Variables.Create("RigidbodyConstraints.FreezeAll", new Value((int)RigidbodyConstraints.FreezeAll, "RigidbodyConstraints.FreezeAll"));
        Variables.Create("RigidbodyConstraints.FreezePosition", new Value((int)RigidbodyConstraints.FreezePosition, "RigidbodyConstraints.FreezePosition"));
        Variables.Create("RigidbodyConstraints.FreezePositionX", new Value((int)RigidbodyConstraints.FreezePositionX, "RigidbodyConstraints.FreezePositionX"));
        Variables.Create("RigidbodyConstraints.FreezePositionY", new Value((int)RigidbodyConstraints.FreezePositionX, "RigidbodyConstraints.FreezePositionY"));
        Variables.Create("RigidbodyConstraints.FreezePositionZ", new Value((int)RigidbodyConstraints.FreezePositionZ, "RigidbodyConstraints.FreezePositionZ"));
        Variables.Create("RigidbodyConstraints.FreezeRotation", new Value((int)RigidbodyConstraints.FreezeRotation, "RigidbodyConstraints.FreezeRotation"));
        Variables.Create("RigidbodyConstraints.FreezeRotationX", new Value((int)RigidbodyConstraints.FreezeRotation, "RigidbodyConstraints.FreezeRotationX"));
        Variables.Create("RigidbodyConstraints.FreezeRotationY", new Value((int)RigidbodyConstraints.FreezeRotation, "RigidbodyConstraints.FreezeRotationY"));
        Variables.Create("RigidbodyConstraints.FreezeRotationZ", new Value((int)RigidbodyConstraints.FreezeRotation, "RigidbodyConstraints.FreezeRotationZ"));
        Variables.Create("RigidbodyConstraints.None", new Value((int)RigidbodyConstraints.None, "RigidbodyConstraints.None"));
        Variables.Create("RigidbodyInterpolation.Extrapolate", new Value((int)RigidbodyInterpolation.Extrapolate, "RigidbodyInterpolation.Extrapolate"));
        Variables.Create("RigidbodyInterpolation.Interpolate", new Value((int)RigidbodyInterpolation.Interpolate, "RigidbodyInterpolation.Interpolate"));
        Variables.Create("RigidbodyInterpolation.None", new Value((int)RigidbodyInterpolation.None, "RigidbodyInterpolation.None"));
        //DOC
        Variables.Create("Tween.Relative", new Value(true, "Tween.Relative"));
        Variables.Create("Tween.Absolute", new Value(false, "Tween.Absolute"));

        Variables.Create("MoveTo.Relative", new Value(true, "MoveTo.Relative"));
        Variables.Create("MoveTo.Absolute", new Value(false, "MoveTo.Absolute"));
        Variables.Create("RotateTo.Relative", new Value(true, "RotateTo.Relative"));
        Variables.Create("RotateTo.Absolute", new Value(false, "RotateTo.Absolute"));
        Variables.Create("ScaleTo.Relative", new Value(true, "ScaleTo.Relative"));
        Variables.Create("ScaleTo.Absolute", new Value(false, "ScaleTo.Absolute"));
        Variables.Create("Location.SameDirection", new Value(1, "Location.SameDirection"));
        Variables.Create("Location.OppositeDirection", new Value(-1, "Location.OppositeDirection"));
        Variables.Create("Location.Perpendicular", new Value(0, "Location.Perpendicular"));
        
        compiler.AddFunction("GameObjects.Create", CreateFunction, true);
        compiler.AddFunction("GameObjects.Destroy", DestroyFunction, true);
        compiler.AddFunction("GameObjects.MoveTo", MoveFunction);
        compiler.AddFunction("GameObjects.ScaleTo", ScaleFunction);
        compiler.AddFunction("GameObjects.RotateTo", RotateFunction);
        
        
        compiler.AddFunction("GameObjects.SetMinDragDistance", SetMinDragDistanceFunction);
        
        compiler.AddFunction("GameObjects.MousePosition", MousePositionFunction);
        
        compiler.AddFunction("GameObjects.IsMoveComplete", IsMoveCompleteFunction);
        compiler.AddFunction("GameObjects.IsScaleComplete", IsScaleCompleteFunction);
        compiler.AddFunction("GameObjects.IsRotateComplete", IsRotateCompleteFunction);
        
        compiler.AddFunction("GameObjects.GetLocation", GetLocationFunction);
        
        compiler.AddFunction("GameObjects.Rigidbody.AddForce", AddForceFunction);
        compiler.AddFunction("GameObjects.Rigidbody.Set", SetRigidbodyFunction);
        compiler.AddFunction("GameObjects.ComparePositions", ComparePositionFunction);
        compiler.AddFunction("GameObjects.GetDistance", GetDistanceFunction);
        compiler.AddFunction("GameObjects.Clear", ClearFunction, true);
        compiler.AddFunction("GameObjects.Reset", ResetGameObjectsFunction, true);
        
        compiler.AddFunction("GameObjects.Animator.SetInteger", SetIntegerParameter);
        compiler.AddFunction("GameObjects.Animator.SetFloat", SetFloatParameter);
        compiler.AddFunction("GameObjects.Animator.SetBool", SetBoolParameter);
        compiler.AddFunction("GameObjects.Animator.GetInteger", GetIntegerParameter);
        compiler.AddFunction("GameObjects.Animator.GetFloat", GetFloatParameter);
        compiler.AddFunction("GameObjects.Animator.GetBool", GetBoolParameter);
        
        //custom properties
        
        Variables.CreateCustomProperty("PhysicsAngularDrag", null, SetAngularDrag);
        Variables.CreateCustomProperty("PhysicsAngularVelocity", null, SetAngularVelocity);
        Variables.CreateCustomProperty("PhysicsSetcenterOfMass", null, SetcenterOfMass);
        Variables.CreateCustomProperty("PhysicsSetCollisionDetectionMode", null, SetCollisionDetectionMode);
        Variables.CreateCustomProperty("PhysicsConstraints", null, SetConstraints);
        Variables.CreateCustomProperty("PhysicsDetectCollisions", null, SetDetectCollisions);
        Variables.CreateCustomProperty("PhysicsDrag", null, SetDrag);
        Variables.CreateCustomProperty("PhysicsFreezeRotation", null, SetFreezeRotation);
        Variables.CreateCustomProperty("PhysicsInertiaTensor", null, SetInertiaTensor);
        Variables.CreateCustomProperty("PhysicsInterpolation", null, SetInterpolation);
        Variables.CreateCustomProperty("PhysicsIsKinematic", null, SetIsKinematic);
        Variables.CreateCustomProperty("PhysicsMass", null, SetMass);
        Variables.CreateCustomProperty("PhysicsMaxAngularVelocity", null, SetMaxAngularVelocity);
        Variables.CreateCustomProperty("PhysicsPosition", null, SetPosition);
        Variables.CreateCustomProperty("PhysicsRotation", null, SetRotation);
        Variables.CreateCustomProperty("PhysicsSleepAngularVelocity", null, SetSleepAngularVelocity);
        Variables.CreateCustomProperty("PhysicsSleepVelocity", null, SetSleepVelocity);
        Variables.CreateCustomProperty("PhysicsSolverIterationCount", null, SetSolverIterationCount);
        Variables.CreateCustomProperty("PhysicsUseConeFriction", null, SetUseConeFriction);
        Variables.CreateCustomProperty("PhysicsUseGravity", null, SetUseGravity);
        Variables.CreateCustomProperty("PhysicsVelocity", null, SetVelocity);
        
        
        
        Variables.CreateCustomProperty("dragBegin", GetDragBegin, null);
        Variables.CreateCustomProperty("collision", GetCollision, SetCollision);
        Variables.CreateCustomProperty("selected", GetSelected, null);
        Variables.CreateCustomProperty("material", null, SetMaterial);
        Variables.CreateCustomProperty("other", GetOther, null);
        
        Variables.CreateCustomProperty("dragging", GetDraging, null);
        Variables.CreateCustomProperty("dragEnd", GetDragEnd, null);
        Variables.CreateCustomProperty("hover", GetHover, null);
        
        Variables.CreateCustomProperty("px", GetPX, SetPX);
        Variables.CreateCustomProperty("py", GetPY, SetPY);
        Variables.CreateCustomProperty("pz", GetPZ, SetPZ);
        
        Variables.CreateCustomProperty("sx", GetSX, SetSX);
        Variables.CreateCustomProperty("sy", GetSY, SetSY);
        Variables.CreateCustomProperty("sz", GetSZ, SetSZ);
        
        Variables.CreateCustomProperty("rx", GetRX, SetRX);
        Variables.CreateCustomProperty("ry", GetRY, SetRY);
        Variables.CreateCustomProperty("rz", GetRZ, SetRZ);
        
        Variables.CreateCustomProperty("vx", GetVX, SetVX);
        Variables.CreateCustomProperty("vy", GetVY, SetVY);
        Variables.CreateCustomProperty("vz", GetVZ, SetVZ);
        
        Variables.CreateCustomProperty("MoveComplete", GetMoveTransformStatus, null);
        Variables.CreateCustomProperty("ScaleComplete", GetScaleTransformStatus, null);
        Variables.CreateCustomProperty("RotateComplete", GetRotateTransformStatus, null);
        
        Variables.CreateCustomProperty("clickable", null, SetClickable);
        Variables.CreateCustomProperty("physics", null, SetPhysics);
        
        Variables.CreateCustomProperty("active", GetActiveProperty, SetActiveProperty);
        
        ImportSceneGameObjects();
    }
}