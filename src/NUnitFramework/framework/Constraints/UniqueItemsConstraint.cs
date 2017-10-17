// ***********************************************************************
// Copyright (c) 2007 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace NUnit.Framework.Constraints
{ 
    /// <summary>
    /// UniqueItemsConstraint tests whether all the items in a 
    /// collection are unique.
    /// </summary>
    public class UniqueItemsConstraint : CollectionItemsEqualConstraint, IEqualityComparer<object>
    {
        /// <summary>
        /// The Description of what this constraint tests, for
        /// use in messages and in the ConstraintResult.
        /// </summary>
        public override string Description
        {
            get { return "all items unique"; }
        }

        #region IEqualityComparer Implementation

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ItemsEqual(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        /// <summary>
        /// Check that all items are unique.
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        protected override bool Matches(IEnumerable actual)
        {
            // This algorithm handles a range of 100000 ints,
            // which previously took 105 seconds in 24 to 35
            // milliseconds on my laptop (Debug). However,
            // it does not use NUnit equality at all, so 
            // may give different results if the members
            // of the enumerable are handled specially by
            // NUnitEqualityComparer.
            var hash = new HashSet<object>();

            foreach (object o in actual)
            {
                object item = o;
                if (IgnoringCase)
                    if (o is string)
                        item = ((string)o).ToLower();
                    else if (o is char)
                        item = char.ToLower((char)o);

                // As far as I can determine, the Contains
                // method doesn't make any use of a supplied
                // IEqualityComparer<object>. If we could 
                // make that happen, we could use NUnit
                // equality for all items.
                if (hash.Contains(item))
                    return false;

                hash.Add(item);
            }

            return true;
        }

        // This version of Matches was my first fix. On
        // a test using a range of ints from 0 to 10000,
        // it reduced time from about 105 seconds to 97.
        private bool LegacyMatches(IEnumerable actual)
        {
            if (actual is IList)
                return IsUnique((IList)actual);

            var list = new List<object>();
            foreach (object item in actual)
                list.Add(item);
            return IsUnique(list);
        }

        private bool IsUnique(IList list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                object item = list[i];
                for (int j = 0; j < i; j++)
                {
                    if (ItemsEqual(item, list[j]))
                        return false;
                }
            }

            return true;
        }
    }
}