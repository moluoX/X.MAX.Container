using System;
using System.Collections.Generic;
using System.Linq;

namespace X.MAX.Container
{
    /// <summary>
    /// max heap or min heap
    /// </summary>
    /// <typeparam name="T">element type</typeparam>
    public sealed class MaxMinHeap<T>
    {
        private MaxOrMin _maxOrMin;
        private T[] _data = new T[100];
        private int _lastIndex = -1;

        /// <summary>
        /// compare element
        /// </summary>
        public Func<T, T, int> Comparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = (a, b) => (a as IComparable<T>).CompareTo(b);
                return _comparer;
            }
            set
            {
                _comparer = value;
            }
        }
        private Func<T, T, int> _comparer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxOrMin">max heap or min heap</param>
        public MaxMinHeap(MaxOrMin maxOrMin) => _maxOrMin = maxOrMin;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxOrMin">max heap or min heap</param>
        /// <param name="data">elements</param>
        public MaxMinHeap(MaxOrMin maxOrMin, IEnumerable<T> data) => Init(maxOrMin, data, 0, data.Count() - 1, null);

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxOrMin">max heap or min heap</param>
        /// <param name="data">elements</param>
        /// <param name="comparer">compare element</param>
        public MaxMinHeap(MaxOrMin maxOrMin, IEnumerable<T> data, Func<T, T, int> comparer) => Init(maxOrMin, data, 0, data.Count() - 1, comparer);

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxOrMin">max heap or min heap</param>
        /// <param name="data">elements</param>
        /// <param name="start">elements start index</param>
        /// <param name="end">elements end index, include this</param>
        public MaxMinHeap(MaxOrMin maxOrMin, IEnumerable<T> data, int start, int end) => Init(maxOrMin, data, start, end, null);

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="maxOrMin">max heap or min heap</param>
        /// <param name="data">elements</param>
        /// <param name="start">elements start index</param>
        /// <param name="end">elements end index, include this</param>
        /// <param name="comparer">compare element</param>
        public MaxMinHeap(MaxOrMin maxOrMin, IEnumerable<T> data, int start, int end, Func<T, T, int> comparer) => Init(maxOrMin, data, start, end, comparer);

        private void Init(MaxOrMin maxOrMin, IEnumerable<T> data, int start, int end, Func<T, T, int> comparer)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (start < 0 || start >= data.Count()) throw new ArgumentOutOfRangeException(nameof(start));
            if (end < 0 || end < start || end >= data.Count()) throw new ArgumentOutOfRangeException(nameof(end));

            _comparer = comparer;
            _maxOrMin = maxOrMin;
            IncreaseArrayTo(end - start + 1);
            Array.Copy(data.ToArray(), start, _data, 0, end - start + 1);
            _lastIndex = end - start;
            AdjustAll();
        }

        /// <summary>
        /// elements length
        /// </summary>
        public int Length { get { return _lastIndex + 1; } }

        /// <summary>
        /// the element top of heap
        /// </summary>
        public T Top { get { return _data[0]; } }

        /// <summary>
        /// push a element and self-adjust
        /// </summary>
        /// <param name="a"></param>
        public void Push(T a)
        {
            IncreaseArray();
            _data[++_lastIndex] = a;
            AdjustUp(_lastIndex);
        }

        /// <summary>
        /// pop the element top of heap and self-adjust
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (_lastIndex < 0) throw new IndexOutOfRangeException("the heap is empty");

            var root = _data[0];
            _data[0] = _data[_lastIndex];
            _lastIndex--;
            AdjustDown(0);
            return root;
        }

        /// <summary>
        /// try pop the element top of heap and self-adjust
        /// </summary>
        /// <param name="r">the element top of heap. if the heap is empty, default of T</param>
        /// <returns>the heap is not empty</returns>
        public bool TryPop(out T r)
        {
            if (_lastIndex >= 0)
            {
                r = Pop(); return true;
            }
            r = default;
            return false;
        }

        private void AdjustUp(int i)
        {
            var p = GetParent(i);
            if (p == null) return;
            if (Compare(_data[p.Value], _data[i]) < 0)
            {
                Swap(p.Value, i);
                AdjustUp(p.Value);
            }
        }

        private void AdjustDown(int i)
        {
            var left = GetLeftChild(i);
            var right = GetRightChild(i);
            int? child;
            if (left == null) child = right;
            else if (right == null) child = left;
            else
            {
                child = Compare(_data[left.Value], _data[right.Value]) > 0 ? left : right;
            }
            if (child == null) return;
            if (Compare(_data[i], _data[child.Value]) < 0)
            {
                Swap(child.Value, i);
                AdjustDown(child.Value);
            }
        }

        private void AdjustAll()
        {
            for (int i = (_lastIndex - 1) / 2; i >= 0; i--)
            {
                AdjustDown(i);
            }
        }

        private int? GetParent(int i) => i <= 0 ? default(int?) : (i - 1) / 2;
        private int? GetLeftChild(int i) => 2 * i + 1 > _lastIndex ? default(int?) : 2 * i + 1;
        private int? GetRightChild(int i) => 2 * i + 2 > _lastIndex ? default(int?) : 2 * i + 2;

        private void Swap(int i, int j)
        {
            var tmp = _data[i];
            _data[i] = _data[j];
            _data[j] = tmp;
        }

        private void IncreaseArray(int amount = 1)
        {
            if (_data.Length > _lastIndex + amount) return;
            var newData = new T[_data.Length * 2];
            Array.Copy(_data, newData, _lastIndex + 1);
            _data = newData;
        }

        private void IncreaseArrayTo(int target)
        {
            if (_data.Length >= target) return;
            var newData = new T[target];
            Array.Copy(_data, newData, _lastIndex + 1);
            _data = newData;
        }

        private int Compare(T a, T b)
        {
            var r = Comparer(a, b);
            return _maxOrMin == MaxOrMin.Max ? r : -r;
        }
    }

    /// <summary>
    /// max heap or min heap
    /// </summary>
    public enum MaxOrMin
    {
        /// <summary>
        /// max heap
        /// </summary>
        Max,

        /// <summary>
        /// min heap
        /// </summary>
        Min
    }
}
