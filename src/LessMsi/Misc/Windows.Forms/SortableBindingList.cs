﻿// Permission is hereby granted, free of charge, to any person obtaining
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
//
// Copyright (c) 2004 Scott Willeke (http://scott.willeke.com)
//
// Authors:
//	Scott Willeke (scott@willeke.com)
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Misc.Windows.Forms
{
    /// <summary>
    /// Implements a simple <see cref="System.ComponentModel.IBindingList"/> that provides sorting ability.
    /// </summary>
    /// <remarks>Useful to provide a sortable <see cref="System.Windows.Forms.DataGridView"/>.</remarks>
    class SortableBindingList<TItem> : BindingList<TItem>
    {
        private readonly IEnumerable<TItem> _originalItems;
        private PropertyDescriptor _sortProperty;
        private ListSortDirection _sortDirection;

        public SortableBindingList(IEnumerable<TItem> items)
        {
            _originalItems = items;
            ResetItems();
        }

        protected void ResetItems()
        {
            RaiseListChangedEvents = false;
            if (Items.Count > 0)
                Items.Clear();
            foreach (var i in _originalItems)
            {
                Add(i);
            }
            RaiseListChangedEvents = true;
            RaiseListSortingChanged();
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;
            Debug.WriteLine("ApplySortCore");
            RaiseListChangedEvents = false;
            Anculus.Core.Sort.QuickSort(Items, new PropertyComparer<TItem>(prop, direction));
            RaiseListChangedEvents = true;
            RaiseListSortingChanged();
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { Debug.WriteLine("SortPropertyCore"); return _sortProperty; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return _sortDirection; }
        }
        protected override bool IsSortedCore
        {
            get { return _sortProperty != null; }
        }

        private void RaiseListSortingChanged()
        {
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            ResetItems();
            _sortProperty = null;
            RaiseListSortingChanged();
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

		private sealed class PropertyComparer<T> : IComparer<T>
		{
			private PropertyDescriptor _prop;
			private ListSortDirection _direction;

			public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
			{
				if (prop == null)
					throw new ArgumentNullException("prop");
				_prop = prop;
				_direction = direction;
			}

			public int Compare(T x, T y)
			{
				var xValue = _prop.GetValue(x);
				var yValue = _prop.GetValue(y);
				var result = Comparer<object>.Default.Compare(xValue, yValue);
				if (_direction == ListSortDirection.Descending)
					result *= -1;
				return result;
			}
		}
    }
}