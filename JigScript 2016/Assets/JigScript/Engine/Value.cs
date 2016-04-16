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
    public class Value
    {
        public enum ValueType
        {
            None = 0,
            Integer = 1,
            Float = 2,
            String = 3,
            Bool = 4,
            Function = 5,
            UserFunction = 6,
            GObject = 7,
            Vector = 8,
            Empty = 9,
            MObject = 10,
            Table = 11,
            Sound = 12,
            Quaternion = 13
        };


        public string name;
        public ValueType T;
        public int I;
        public float F;
        public string S;
        public bool B;
        public FUN N;
        public UserFunction U;
        public GameObject G;
        public Vector3 V;
        public Quaternion Q;
        public bool moveTo;                 //Signals when a move has been completed.
        public bool scaleTo;                //Signals when a scale has been completed.
        public bool rotateTo;               //Signals when a rotation has been completed.
        public bool collision;              //true if a collision has happened.
        public Value other;                 //If a collision has happened then this contains the game object variable that collided.
        public int arrayIndex;              //used for assignments, set on retrival.
        public int index;                   //used for assignments, set on retrival.
        public bool isVariable;             //if true then this is a variable and not an immediate value.
        public int field;                   //sub field is one is present, 0 means none.
        public bool isReference;            //if true then this variable is the left side of an assignment.
        public UIControl.Type controlType;  //If a control this is the type.
        public int sound;                   //If a sound this is the index of the sound clip.
        
        //used by when statement to determine if this value has changed.
        public int changed;

        private void Set( ValueType T, string name )
        {
            this.index = 0;
            this.arrayIndex = 0;
            this.T = T;
            this.name = name;
            this.I = 0;
            this.F = 0.0f;
            this.S = "";
            this.B = false;
            this.N = new FUN(0);
            this.U = null;
            this.G = null;
            this.V = Vector3.zero;
            this.Q = Quaternion.identity;

            this.moveTo = false;
            this.scaleTo = false;
            this.rotateTo = false;
            this.collision = false;
            this.isVariable = false;
            this.isReference = false;
            this.controlType = UIControl.Type.None;
            this.sound = 0;

            this.field = 0;
            this.changed = 0;
        }

        public Value( int I, string name )
        {
            this.Set(ValueType.Integer, name);
            this.I = I;
        }

        public Value( float F, string name )
        {
            this.Set(ValueType.Float, name);
            this.F = F;
        }

        public Value( string S, string name )
        {
            this.Set(ValueType.String, name);
            this.S = S;
        }

        public Value( bool B, string name )
        {
            this.Set(ValueType.Bool, name);
            this.B = B;
        }

        public Value( FUN N, string name )
        {
            this.Set(ValueType.Function, name);
            this.N = N;
        }

        public Value( UserFunction U, string name )
        {
            this.Set(ValueType.UserFunction, name);
            this.U = U;
        }

        public Value( GameObject G, string name )
        {
            this.Set(ValueType.GObject, name);
            this.G = G;
        }

        public Value( Vector3 V, string name )
        {
            this.Set(ValueType.Vector, name);
            this.V = V;
        }

        public Value()
        {
            Set(ValueType.Integer, Variables.GetUniqueName());
            this.isVariable = false;
        }

        public Value( Value v1 )
        {
            this.index = v1.index;
            this.arrayIndex = v1.arrayIndex;
            this.name = v1.name;
            
            this.T = v1.T;
            this.I = v1.I;
            this.F = v1.F;
            this.S = v1.S;
            this.B = v1.B;
            this.N = v1.N;
            this.U = v1.U;
            this.G = v1.G;
            this.V = v1.V;
            this.Q = v1.Q;
            
            this.moveTo = v1.moveTo;
            this.scaleTo = v1.scaleTo;
            this.rotateTo = v1.rotateTo;
            this.isVariable = v1.isVariable;
            this.field = v1.field;
            this.isReference = v1.isReference;
            this.controlType = v1.controlType;
            this.sound = v1.sound;
            this.changed = v1.changed;
        }
        
        public Value( Value vt, int arrayIndex, Value vf )
        {
            this.T = vf.T;
            this.I = vf.I;
            this.F = vf.F;
            this.S = vf.S;
            this.B = vf.B;
            this.N = vf.N;
            this.U = vf.U;
            this.G = vf.G;
            this.V = vf.V;
            this.Q = vf.Q;
            
            this.index = vt.index;
            this.arrayIndex = arrayIndex;
            this.isVariable = vt.isVariable;
            this.name = vt.name;
            this.moveTo = vf.moveTo;
            this.scaleTo = vf.scaleTo;
            this.rotateTo = vf.rotateTo;
            this.field = vf.field;
            this.isReference = vf.isReference;
            this.controlType = vf.controlType;
            this.sound = vf.sound;
            this.changed = vf.changed;
        }
        
        public Value( ValueType T, string name )
        {
            this.Set(T, name);
        }

        public override string ToString()
        {
            string s;

            s = this.T.ToString() + ",";
            s += this.I.ToString() + ",";
            s += this.F.ToString() + ",";
            s += "\"" + this.S + "\"" + ",";
            s += this.B.ToString() + ",";
            s += this.V.x.ToString() + ",";
            s += this.V.y.ToString() + ",";
            s += this.V.z.ToString();
            
            return s;
        }
        
        public static Value FromString( string str )
        {
            string[] fields = str.Split(new char[]
            {
                ','
            });

            Value v = new Value(0, "str");

            if ( fields.Length>0 )
            {
                switch( fields[0] )
                {
                    case "None":
                        v.T = ValueType.None;
                        break;
                    case "Integer":
                        v.T = ValueType.Integer;
                        break;
                    case "Float":
                        v.T = ValueType.Float;
                        break;
                    case "String":
                        v.T = ValueType.String;
                        break;
                    case "Bool":
                        v.T = ValueType.Bool;
                        break;
                    case "Vector":
                        v.T = ValueType.Vector;
                        break;
                }
            }
            if ( fields.Length>1 )
            {
                int.TryParse(fields[1], out v.I);
            }
            if ( fields.Length>2 )
            {
                float.TryParse(fields[2], out v.F);
            }
            if ( fields.Length>3 )
            {
                v.S = fields[3].Trim(new char[]
                {
                    '\"',
                    ','
                });
            }
            if ( fields.Length>4 )
            {
                v.B = (fields[4].ToLower() == "true");
            }
            if ( fields.Length>5 )
            {
                float.TryParse(fields[5], out v.V.x);
            }
            if ( fields.Length>6 )
            {
                float.TryParse(fields[6], out v.V.y);
            }
            if ( fields.Length>7 )
            {
                float.TryParse(fields[7], out v.V.z);
            }

            return v;
        }

        public static void Promote(ref Value result, ref Value tmp)
        {
            if ( result.T == tmp.T )
            {
                return;
            }
            
            //1. If an operation involves two operands and one of them is of type string, the other one is converted to string.
            if ( result.T == ValueType.String || tmp.T == ValueType.String )
            {
                result.ConvertTo(ValueType.String);
                tmp.ConvertTo(ValueType.String);
                return;
            }
            
            //2. If an operation involves two operands and one of them is of type float, the other one is converted to float.
            if ( result.T == ValueType.Float || tmp.T == ValueType.Float )
            {
                result.ConvertTo(ValueType.Float);
                tmp.ConvertTo(ValueType.Float);
                return;
            }

            //3. If an operation involves two operands and one of them is of type int, the other one is converted to int.
            if ( result.T == ValueType.Integer && tmp.T != ValueType.Integer )
            {
                tmp.ConvertTo(ValueType.Integer);
                tmp.ConvertTo(ValueType.Integer);
                return;
            }
            
            if ( result.T == ValueType.Empty && (tmp.T == ValueType.Integer || tmp.T == ValueType.Float) )
            {
                result.ConvertTo(tmp.T);
                return;
            }
            
            if ( tmp.T == ValueType.Empty && (result.T == ValueType.Integer || result.T == ValueType.Float) )
            {
                tmp.ConvertTo(result.T);
                return;
            }
            
            result.ConvertTo(tmp.T);
        }

        public static Value operator+( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.I += tmp.I;
                    break;
                case ValueType.Float:
                    result.F += tmp.F;
                    break;
                case ValueType.String:
                    result.S += tmp.S;
                    break;
                case ValueType.Bool:
                    result.B = (result.B || tmp.B);
                    break;
                case ValueType.GObject:
                    v2.G.transform.parent = v1.G.transform;
                    break;
                case ValueType.Vector:
                    result.V += tmp.V;
                    break;
            }
            return result;
        }
        
        public static Value operator-( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            
            if ( result.T != ValueType.Integer || result.T != ValueType.Float )
            {
                result.ConvertTo(ValueType.Float);
            }
            
            if ( tmp.T != ValueType.Integer || tmp.T != ValueType.Float )
            {
                tmp.ConvertTo(ValueType.Float);
            }
            
            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.I -= tmp.I;
                    break;
                case ValueType.Float:
                    result.F -= tmp.F;
                    break;
            }
            
            return result;
        }
        
        public static Value operator*( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            if ( result.T != ValueType.Integer || result.T != ValueType.Float )
            {
                result.ConvertTo(ValueType.Float);
            }
            
            if ( tmp.T != ValueType.Integer || tmp.T != ValueType.Float )
            {
                tmp.ConvertTo(ValueType.Float);
            }
            
            Promote(ref result, ref tmp);
            
            switch( result.T )
            {
                case ValueType.Integer:
                    result.I *= tmp.I;
                    break;
                case ValueType.Float:
                    result.F *= tmp.F;
                    break;
            }
            
            return result;
        }

        public static Value operator%( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);
            
            if ( result.T != ValueType.Integer )
            {
                result.ConvertTo(ValueType.Integer);
            }
            
            if ( tmp.T != ValueType.Integer )
            {
                tmp.ConvertTo(ValueType.Integer);
            }

            Promote(ref result, ref tmp);
            
            result.I %= tmp.I;

            return result;
        }
        
        public static Value operator/( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            if ( result.T != ValueType.Integer || result.T != ValueType.Float )
            {
                result.ConvertTo(ValueType.Float);
            }
            
            if ( tmp.T != ValueType.Integer || tmp.T != ValueType.Float )
            {
                tmp.ConvertTo(ValueType.Float);
            }
            
            Promote(ref result, ref tmp);
            
            switch( result.T )
            {
                case ValueType.Integer:
                    result.I /= tmp.I;
                    break;
                case ValueType.Float:
                    result.F /= tmp.F;
                    break;
            }
            
            return result;
        }

        public static Value operator&( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            result.ConvertTo(ValueType.Integer);
            tmp.ConvertTo(ValueType.Integer);
            
            result.I &= tmp.I;

            return result;
        }

        public static Value operator|( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            result.ConvertTo(ValueType.Integer);
            tmp.ConvertTo(ValueType.Integer);
            
            result.I |= tmp.I;

            return result;
        }

        public static Value operator^( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            result.ConvertTo(ValueType.Integer);
            tmp.ConvertTo(ValueType.Integer);
            
            result.I ^= tmp.I;

            return result;
        }

        public static Value operator<( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I<tmp.I;
                    break;
                case ValueType.Float:
                    result.B = result.F<tmp.F;
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S)<0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B == false && tmp.B == true);
                    break;
            }
            result.T = ValueType.Bool;

            return result;
        }

        public static Value operator<=( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);
            
            Promote(ref result, ref tmp);
            
            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I<=tmp.I;
                    break;
                case ValueType.Float:
                    result.B = result.F<=tmp.F;
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S)<=0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B == false && tmp.B == true || result.B == tmp.B);
                    break;
            }
            
            result.T = ValueType.Bool;
            
            return result;
        }

        public static Value operator>( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I>tmp.I;
                    break;
                case ValueType.Float:
                    result.B = result.F>tmp.F;
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S)>0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B == true && tmp.B == false);
                    break;
            }

            result.T = ValueType.Bool;

            return result;
        }

        public static Value operator>=( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I>tmp.I;
                    break;
                case ValueType.Float:
                    result.B = result.F>tmp.F;
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S)>0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B == true && tmp.B == false || result.B == tmp.B);
                    break;
            }

            result.T = ValueType.Bool;

            return result;
        }

        private static bool CompareVectors( Vector3 a, Vector3 b, float angleError )
        {
            bool rc = false;

            if ( Mathf.Approximately(a.magnitude, b.magnitude) != false )
            {
                float cosAngleError = Mathf.Cos(angleError * Mathf.Deg2Rad);

                float cosAngle = Vector3.Dot(a.normalized, b.normalized);
            
                if ( cosAngle>=cosAngleError )
                {
                    rc = true;
                }
            }

            return rc;
        }
        
        public static Value operator==( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I == tmp.I;
                    break;
                case ValueType.Float:
                    result.B = Mathf.Approximately(result.F, tmp.F);
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S) == 0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B == tmp.B);
                    break;
                case ValueType.GObject:
                    result.B = (result.G == tmp.G);
                    break;
                case ValueType.Vector:
                    result.B = CompareVectors(result.V, tmp.V, .01f);
                    break;
            }

            result.T = ValueType.Bool;

            return result;
        }

        public static Value operator!=( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            Promote(ref result, ref tmp);

            switch( result.T )
            {
                case ValueType.Integer:
                    result.B = result.I != tmp.I;
                    break;
                case ValueType.Float:
                    result.B = Mathf.Approximately(result.F, tmp.F);
                    break;
                case ValueType.String:
                    result.B = result.S.CompareTo(tmp.S) != 0;
                    break;
                case ValueType.Bool:
                    result.B = (result.B != tmp.B);
                    break;
                case ValueType.GObject:
                    result.B = (result.G == tmp.G);
                    break;
                case ValueType.Vector:
                    result.B = CompareVectors(result.V, tmp.V, .01f) == false;
                    break;
            }
            
            result.T = ValueType.Bool;

            return result;
        }
        
        public override bool Equals( System.Object obj )
        {
            Value v = obj as Value;
            if ( (object)v == null )
            {
                return false;
            }

            Value tmp = new Value(v);
            tmp.ConvertTo(this.T);
            
            bool b = false;
            
            switch( this.T )
            {
                case ValueType.Integer:
                    b = this.I == tmp.I;
                    break;
                case ValueType.Float:
                    b = Mathf.Approximately(this.F, tmp.F);
                    break;
                case ValueType.String:
                    b = this.S.CompareTo(tmp.S) == 0;
                    break;
                case ValueType.Bool:
                    b = (this.B == tmp.B);
                    break;
                case ValueType.Vector:
                    b = CompareVectors(this.V, tmp.V, .01f);
                    break;
                case ValueType.GObject:
                    b = (this.G == tmp.G);
                    break;
                case ValueType.Function:
                    b = (this.N == tmp.N);
                    break;
                case ValueType.UserFunction:
                    b = (this.U == tmp.U);
                    break;
                case ValueType.Empty:
                    b = (this.index == tmp.index && this.arrayIndex == tmp.arrayIndex);
                    break;
                case ValueType.MObject:
                    JigMat jm1 = this.G.GetComponent<JigMat>();
                    JigMat jm2 = tmp.G.GetComponent<JigMat>();
                    if ( jm1 != null && jm2 != null )
                    {
                        b = (jm1 == jm2);
                    }
                    break;
            }
            
            return b;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Value operator>>( Value v1, int v2 )
        {
            Value result = new Value(v1);
            result.ConvertTo(ValueType.Integer);
            result.I = result.I>>v2;
            
            return result;
        }

        public static Value operator<<( Value v1, int v2 )
        {
            Value result = new Value(v1);
            result.ConvertTo(ValueType.Integer);
            result.I = result.I<<v2;
            
            return result;
        }

        public static Value LogicalAnd( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            result.ConvertTo(ValueType.Bool);
            tmp.ConvertTo(result.T);
            
            result.B = result.B && tmp.B;
            
            return result;
        }

        public static Value LogicalOr( Value v1, Value v2 )
        {
            Value tmp = new Value(v2);
            Value result = new Value(v1);

            result.ConvertTo(ValueType.Bool);
            tmp.ConvertTo(result.T);
            
            result.B = result.B || tmp.B;
            
            return result;
        }

        public static Value Negate(Value v1)
        {
            Value result = new Value(v1);
            
            result.ConvertTo(ValueType.Bool);
            
            result.B = !result.B;
            
            return result;
        }
        
        private float FloatFromString(string s)
        {
            float f = 0.0f;
            
            float.TryParse(s, out f);
            
            return f;
        }
        
        private Vector3 VectorFromString(string s)
        {
            string[] elements = s.Split(new char[]{','});
            
            return new Vector3(elements.Length > 0 ? FloatFromString(elements[0]) : 0,
                               elements.Length > 1 ? FloatFromString(elements[1]) : 0,
                               elements.Length > 2 ? FloatFromString(elements[2]) : 0);
        }
        
        private Quaternion QuaternionFromString(string s)
        {
            string[] elements = s.Split(new char[]{','});
            
            return new Quaternion(elements.Length > 0 ? FloatFromString(elements[0]) : 0,
                               elements.Length > 1 ? FloatFromString(elements[1]) : 0,
                               elements.Length > 2 ? FloatFromString(elements[2]) : 0,
                               elements.Length > 3 ? FloatFromString(elements[3]) : 0);
        }
        
        public void ConvertTo( ValueType to )
        {
            if ( this.T != ValueType.Function && to != ValueType.Function && this.T != to )
            {
                switch( this.T )
                {
                    case ValueType.GObject:
                        switch( to )
                        {
                            case ValueType.String:
                                T = to;
                                S = G.name;
                                break;
                            case ValueType.Vector:
                                T = to;
                                V = new Vector3(G.transform.position.x, G.transform.position.y, G.transform.position.z);
                                break;
                        }
                        break;
                    case ValueType.Vector:
                        switch( to )
                        {
                            case ValueType.String:
                                T = to;
                                S = V.x.ToString() + "," + V.y.ToString() + "," + V.z.ToString();
                                break;
                            case ValueType.Bool:
                                T = to;
                                B = Mathf.Approximately(V.x, 0.0f) && Mathf.Approximately(V.y, 0.0f) && Mathf.Approximately(V.z, 0.0f);
                                break;
                            case ValueType.GObject:
                                T = to;
                                G.transform.position = V;
                                break;
                            case ValueType.Quaternion:
                                T = to;
                                S = V.x.ToString() + "," + V.y.ToString() + "," + V.z.ToString() + ", 0";
                                break;
                        }
                        break;
                    case ValueType.Integer:
                        switch( to )
                        {
                            case ValueType.Float:
                                T = to;
                                F = (float)I;
                                break;
                            case ValueType.String:
                                T = to;
                                S = I.ToString();
                                break;
                            case ValueType.Bool:
                                T = to;
                                B = (I != 0);
                                break;
                        }
                        break;
                    case ValueType.Float:
                        switch( to )
                        {
                            case ValueType.Integer:
                                T = to;
                                I = (int)F;
                                break;
                            case ValueType.String:
                                T = to;
                                S = F.ToString();
                                break;
                            case ValueType.Bool:
                                T = to;
                                B = ((int)F != 0);
                                break;
                        }
                        break;
                    case ValueType.String:
                        switch( to )
                        {
                            case ValueType.Integer:
                                T = to;
                                I = 0;
                                int.TryParse(S, out I);
                                break;
                            case ValueType.Float:
                                T = to;
                                F = 0.0f;
                                float.TryParse(S, out F);
                                break;
                            case ValueType.Bool:
                                T = to;
                                B = (S.ToLower() != "false");
                                break;
                            case ValueType.Vector:
                                T = to;
                                V = VectorFromString(S);
                                break;
                            case ValueType.Quaternion:
                                T = to;
                                Q = QuaternionFromString(S);
                                break;
                        }
                        break;
                    case ValueType.Bool:
                        switch( to )
                        {
                            case ValueType.Integer:
                                T = to;
                                I = (B) ? 1 : 0;
                                break;
                            case ValueType.Float:
                                T = to;
                                F = (B) ? 1.0f : 0.0f;
                                break;
                            case ValueType.String:
                                T = to;
                                S = B.ToString().ToLower();
                                break;
                        }
                        break;
                }
            }
        }
    };
}