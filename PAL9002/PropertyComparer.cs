using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;

namespace PAL9002
{


    public class PropertyComparer<T> : System.Collections.Generic.IComparer<T>
    {
        private PropertyDescriptor _property;
        private ListSortDirection _direction;

        public PropertyComparer(PropertyDescriptor property,
            ListSortDirection direction)
        {
            _property = property;
            _direction = direction;
        }

        #region IComparer<T>

        public int Compare(T xWord, T yWord)
        {
            // Get property values
            object xValue = GetPropertyValue(xWord, _property.Name);
            object yValue = GetPropertyValue(yWord, _property.Name);

            // Determine sort order
            if (_direction == ListSortDirection.Ascending)
            {
                return CompareAscending(xValue, yValue);
            }
            else
            {
                return CompareDescending(xValue, yValue);
            }
        }

        public bool Equals(T xWord, T yWord)
        {
            return xWord.Equals(yWord);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        // Compare two property values of any type
        private int CompareAscending(object x, object y)
        {
            int result;

            // If values implement IComparer
            if (x is IComparable)
            {
                result = ((IComparable)x).CompareTo(y);
            }
            // If values don't implement IComparer but are equivalent
            else if (x.Equals(y))
            {
                result = 0;
            }
            // Values don't implement IComparer and are not equivalent,
            // so compare as typed values
            else result = ((IComparable)x).CompareTo(y);

            // Return result
            return result;
        }

        private int CompareDescending(object x, object y)
        {
            // Return result adjusted for ascending or descending sort order ie
            // multiplied by 1 for ascending or -1 for descending
            return -CompareAscending(x, y);
        }

        private object GetPropertyValue(T value, string property)
        {
            // Get property
            PropertyInfo propertyInfo = value.GetType().GetProperty(property);

            // Return value
            return propertyInfo.GetValue(value, null);
        }
    }
}