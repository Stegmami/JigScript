using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NightPen.JigScript
{
    public class DValue : Comparer<List<Value>>
    {
        public override int Compare(List<Value> a, List<Value> b)
        {
            Value v1 = a[0];
            Value v2 = b[0];
            
            if ( (v1 < v2).B )
            {
                return -1;
            }
            if ( (v1 > v2).B )
            {
                return 1;
            }
            return 0;
        }        
    }

    public class JigTable : System.Object
    {
        public enum SelectCondition
        {
            None = 0,
            Less = 1,
            Greater = 2,
            LessOrEqual = 3,
            GreaterOrEqual = 4,
            Equal = 5,
            NotEqual = 6
        };
        
        List<List<DValue>> data;
        List<string> columnNames;
        
        public JigTable(List<string> columns)
        {
            this.columnNames = new List<string>();
            this.columnNames.AddRange(columns);
            this.data = new List<List<DValue>>();
        }
        
        public void Insert(List<DValue> values)
        {
            int index = data.BinarySearch(values);
            if ( index < 0 )
            {
                index = ~index;
            }
            data.Insert(index, values);
        }
        
        public void Update(Value v, List<DValue> values)
        {
            int index = data.BinarySearch(values);
            if ( index >= 0 )
            {
                data[index] = values;
            }
        }
        
        public List<Value> Select(Value v1, SelectCondition cond, Value v2)
        {
            
            
            return new List<Value>();
        }
        
        public bool Save()
        {
            return false;
        }
        
        public bool Load()
        {
            return false;
        }
        
        public bool Send()
        {
            return false;
        }
    }
}