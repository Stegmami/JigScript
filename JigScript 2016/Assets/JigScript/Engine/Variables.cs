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
    public static class Variables : System.Object
    {
        private static List<CustomProperty> customPropertiesList;
        private static Dictionary<string, int> customPropertiesDB;
        private static Dictionary<string, int> variablesDB;
        private static List<List<Variable>> variableLists;
        private static Object rwLock;
        public const int clicked = 1;
        public const int changed = 2;
        private static int count;   //used to keep names unique

        static Variables()
        {
            rwLock = new Object();
            variablesDB = new Dictionary<string, int>();
            variableLists = new List<List<Variable>>();
            customPropertiesList = new List<CustomProperty>();
            customPropertiesDB = new Dictionary<string, int>();
            CreateCustomProperty("#NoProperty", null, null);
            count = 11220;
        }

        public static bool CustomPropertyExist(string id)
        {
            return customPropertiesDB.ContainsKey(id);
        }
        
        public static int CustomPropertyIndex(string id)
        {
            return customPropertiesDB[id];
        }
        
        public static string CustomPropertyName(int index)
        {
            string name = string.Empty;
            
            if ( index > 0 && index < customPropertiesList.Count )
            {
                name = customPropertiesList[index].name;
            }
            
            return name;
        }
        
        public static bool IsPollingProperty(int index)
        {
            return customPropertiesList[index].poll;
        }
        
        public static string UniqueName()
        {
            ++count;
            return "`" + count.ToString();
        }
                                                
        public static bool Exists( string name )
        {
            return variablesDB.ContainsKey(name);
        }
        
        public static int Index( string name )
        {
            int index = -1;
            if ( Exists(name) )
            {
                index = variablesDB[name];
            }
            
            return index;
        }
    
        public static string Name( int index )
        {
            string name = "#PlaceHolder";
            
            if ( index>=0 && index<variableLists.Count )
            {
                name = variableLists[index][0].Get().name;
            }
    
            return name;
        }
    
        public static int Length( Value v )
        {
            int rc = -1;

            if ( v.isVariable )
            {
                if ( v.index>0 && v.index<variableLists.Count )
                {
                    rc = variableLists[v.index].Count - 1;
                }
            }
            return rc;
        }
        
        public static void ClearLocals()
        {
            for( int index = 2; index < variableLists.Count; ++index )
            {
                Value vt = variableLists[index][0].Get();
                if ( vt.name[0] == '~' )
                {
                    Clear(vt);
                }
            }
        }
        //changed is not preserved.
        
        public static List<Value> Expand( Value v, bool CallGetFn )
        {
            List<Value> vrc = new List<Value>();

            Value v2 = Read(v.index, v.arrayIndex);
            
            v.changed = v2.changed; //preserve the changed state flag
            
            if ( v.isVariable && v.field == 0 )
            {
                v = v2;
            }

            vrc.Add(new Value(v));
            
            Value v1 = new Value(v);
            
            if ( v.isVariable == false )
            {
                vrc.Add(v1);
            }
            else
            {
                if ( v.arrayIndex == 0 )
                {
                    for(int ii=1; ii<variableLists[v.index].Count; ++ii)
                    {
                        v1 = variableLists[v.index][ii].Get();
                        v1.field = v.field;
                        if ( CallGetFn == false )
                        {
                            vrc.Add(v1);
                        }
                        else
                        {
                            vrc.Add(GetValue(v1));
                        }
                    }
                }
                else
                {
                    if ( CallGetFn == false )
                    {
                        vrc.Add(v1);
                    }
                    else
                    {
                        vrc.Add(GetValue(v1));
                    }
                }
            }
            
            return vrc;
        }
        
        public static void Copy( Value to, Value from )
        {
            if ( to.isVariable && from.isVariable )
            {
                int copyLen = variableLists[from.index].Count;
                
                List<Value> vf = Expand(from, false);
                
                for( int ii=1; ii<copyLen; ++ii )
                {
                    if ( ii < vf.Count )
                    {
                        Variables.Store(to, ii, vf[ii]);
                    }
                    else
                    {
                        Variables.Store(to, ii, vf[vf.Count-1]);
                    }
                }
            }
        }
        
        public static void FillArray( Value vt, int start, Value vf )
        {
            if ( vt.isVariable )
            {
                int length = variableLists[vt.index].Count;
                if ( start>length )
                {
                    length = start;
                }

                for( int ii=start; ii<length; ++ii )
                {
                    Store(vt, ii, new Value(vf));
                }
            }
        }
        
        public static void Clear( Value vt )
        {
            if ( vt.isVariable  )
            {
                lock( rwLock )
                {
                    variableLists[vt.index].Clear();
                }
                Store(vt, 0, new Value(vt));
                Store(vt, 1, new Value(vt));
            }
        }

        public static void Delete( Value vt, int firstRow, int rowsToDelete )
        {
            if ( vt.isVariable == false )
            {
                return;
            }
            
            int count = variableLists[vt.index].Count;
            
            if ( firstRow < 0 || firstRow >= count || rowsToDelete <= 0 )
            {
                return;
            }
            
            if ( firstRow == 0 || (firstRow == 1 && firstRow + rowsToDelete == count) )
            {
                Clear(vt);
                return;
            }
            
            if ( firstRow + rowsToDelete > count )
            {
                rowsToDelete = count - firstRow;
            }

            lock( rwLock )
            {
                while( rowsToDelete > 0 )
                {
                    variableLists[vt.index].RemoveAt(firstRow + rowsToDelete);
                    rowsToDelete--;
                }
            }
            if ( variableLists[vt.index].Count == 1 )
            {
                Store(vt, 1, new Value(vt));
            }
        }

        public static Value Read( string name, int arrayIndex )
        {
            return Read(Index(name), arrayIndex);
        }

        private static Value GetValue( Value v )
        {
            if ( v.isReference == false && v.field > 0 && v.field<customPropertiesList.Count )
            {
                if ( customPropertiesList[v.field].getFn == null )
                {
                    Debug.LogError(customPropertiesList[v.field].name + " is not readable.");
                }
                else
                {
                    v = customPropertiesList[v.field].getFn(v);
                }
            }
            return new Value(v);
        }

        public static Value Read( int index, int arrayIndex, int field = 0 )
        {
            if ( index < 0 || index >= variableLists.Count )
            {
                return new Value(variableLists[0][0].Get());
            }
            int last = variableLists[index].Count - 1;
            if ( last < 0 )
            {
                last = 0;
            }
            Value vf = GetValue(variableLists[index][last].Get());
            while( arrayIndex >= variableLists[index].Count )
            {
                if ( arrayIndex>=variableLists[index].Count )
                {
                    Store(vf, arrayIndex, vf);
                }
            }
            
            Value vr = variableLists[index][arrayIndex].Get();
            vr.field = field;
            vr.arrayIndex = arrayIndex;

            return new Value( GetValue(vr) );
        }

        public static void ClearChanged()
        {
            lock( rwLock )
            {
                for(int index=0; index < variableLists[index].Count; ++index)
                {
                    for(int arrayIndex=0; arrayIndex < variableLists[index].Count; ++arrayIndex)
                    {
                        Value v = variableLists[index][arrayIndex].Get();
                        v.changed = 0;
                        variableLists[index][arrayIndex].Set(v);
                    }
                }
            }
        }
        
        public static void DecrementChangedCount(Value value)
        {
            List<Value> va = Variables.Expand(value, false);
            
            foreach(Value v in va)
            {
                variableLists[v.index][v.arrayIndex].DecrementChangedCount();
            }
        }
        
        private static bool IsChanged( Value dest, int arrayIndex, Value source )
        {
            bool rc = false;
            
            if ( dest.isVariable && dest.index > 0 && dest.index < variableLists.Count )
            {
                if ( arrayIndex >= variableLists[dest.index].Count )
                {
                    rc = true;
                }
                else
                {
                    Value v1 = variableLists[dest.index][arrayIndex].Get();
                    if ( v1.field > 0 && v1.field < customPropertiesList.Count )
                    {
                        if ( customPropertiesList[v1.field].setFn != null )
                        {
                            v1 = customPropertiesList[v1.field].getFn(v1);
                        }
                    }
                    rc = v1.Equals(source) == false;
                }
            }
            return rc;
        }

        //If not in a Set property method use this call is it handles
        //custom property calls. This method is fine to call in a get
        //property method.
        public static void Store( Value dest, int arrayIndex, Value source )
        {
            Value v1 = new Value(dest);
            
            bool changed = IsChanged(dest, arrayIndex, source);
            
            if ( v1.field>0 && v1.field<customPropertiesList.Count )
            {
                if ( customPropertiesList[v1.field].setFn == null )
                {
                    Debug.LogError(customPropertiesList[v1.field].name + " is not writable.");
                }
                else
                {
                    customPropertiesList[v1.field].setFn(v1, arrayIndex, source);
                }
            }
            else
            {
                StoreDirect(dest, arrayIndex, source, changed);
            }
        }
        
        //Use this method when in a set property method as it does not call the
        //custom property Set method.
        public static void StoreDirect( Value dest, int arrayIndex, Value source, bool changed )
        {
            Value vt = new Value(source);
            vt.index = dest.index;
            vt.isVariable = true;
            
            while( arrayIndex >= variableLists[vt.index].Count )
            {
                lock( rwLock )
                {
                    if ( arrayIndex>=variableLists[vt.index].Count )
                    {
                        variableLists[vt.index].Add(new Variable(new Value(vt, variableLists[vt.index].Count, source)));
                    }
                }
            }
            
            Value vn = new Value(vt, variableLists[vt.index].Count, source);
            vn.arrayIndex = arrayIndex;
            variableLists[vt.index][arrayIndex].Set(vn);
            if ( changed )
            {
                variableLists[vt.index][arrayIndex].IncrementChangedCount();
            }
        }
            
        public static string GetUniqueName()
        {
            ++count;
            return "#" + (variableLists.Count + 100000).ToString();
        }

        public static void CreateUnique( ref Value v )
        {
            int index;
            
            lock( rwLock )
            {
                index = variableLists.Count;

                bool found = false;

                for( int ii=100; ii<variableLists.Count; ++ii )
                {
                    Value v1 = variableLists[ii][0].Get();
                    if ( v1.T == Value.ValueType.None )
                    {
                        v.name = v1.name;
                        v.index = v1.index;
                        
                        Store(v, 0, v);
                        Store(v, 1, v);
                        v.arrayIndex = 1;
                        found = true;
                        break;
                    }
                }
                if ( found == false )
                {
                    variableLists.Add(new List<Variable>());
                    v.index = index;
                    v.name = "#" + index;
                    variablesDB.Add(v.name, index);
                    Store(v, 0, v);
                    Store(v, 1, v);
                    v.arrayIndex = 1;
                }
            }
        }

        public static int Create( string name, Value v )
        {
            int index;

            if ( Exists(name) )
            {
                return Index(name);
            }
            else
            {
                lock( rwLock )
                {
                    index = variableLists.Count;
        
                    variableLists.Add(new List<Variable>());
                    v.index = index;
                    v.name = name;
                    variableLists[index].Add(new Variable(new Value(0, name)));
                    variablesDB.Add(name, index);
                    Store(v, 0, v);
                    Store(v, 1, v);
                }
            }

            return index;
        }
        
        public static int CreateCustomProperty( string name, CustomProperty.GetFunction getFn, CustomProperty.SetFunction setFn, bool poll = false )
        {
            int index;
            
            if ( customPropertiesDB.ContainsKey(name) )
            {
                index = customPropertiesDB[name];
            }
            else
            {
                index = customPropertiesList.Count;
                customPropertiesList.Add(new CustomProperty(name, index, getFn, setFn, poll));
                customPropertiesDB.Add(name, index);
            }
            return index;
        }
    }
}