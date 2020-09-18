using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace X.MAX.Container
{
    /// <summary>
    /// Red Black Tree (left-leaning)
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">value type</typeparam>
    public sealed class RedBlackTree<TKey, TValue>
    {
        /// <summary>
        /// compare element
        /// </summary>
        public Func<TKey, TKey, int> Comparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = (a, b) => (a as IComparable<TKey>).CompareTo(b);
                return _comparer;
            }
            set
            {
                _comparer = value;
            }
        }
        private Func<TKey, TKey, int> _comparer;

        private RedBlackTreeNode<TKey, TValue> _root;

        /// <summary>
        /// .ctor
        /// </summary>
        public RedBlackTree() { }

        /// <summary>
        /// .ctor
        /// </summary>
        public RedBlackTree(IEnumerable<KeyValuePair<TKey, TValue>> kvs)
        {
            foreach (var kv in kvs)
            {
                Insert(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// insert k-v
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Insert(TKey key, TValue value)
        {
            if (_root == null)
            {
                _root = new RedBlackTreeNode<TKey, TValue>(key, value, RedOrBlack.Black, null);
                return;
            }

            var node = _root;
            InsertInner(node, key, value);
        }

        private void InsertInner(RedBlackTreeNode<TKey, TValue> node, TKey key, TValue value)
        {
            Split4node(node);

            RedBlackTreeNode<TKey, TValue> child;
            var c = Comparer(key, node.Key);
            if (c == 0)
            {
                node.Key = key;
                node.Value = value;
                return;
            }
            else if (c < 0)
            {
                child = node.LeftChild;
                if (child != null)
                {
                    InsertInner(child, key, value);
                    return;
                }

                child = new RedBlackTreeNode<TKey, TValue>(key, value, RedOrBlack.Red, node);
                node.LeftChild = child;
            }
            else
            {
                child = node.RightChild;
                if (child != null)
                {
                    InsertInner(child, key, value);
                    return;
                }

                child = new RedBlackTreeNode<TKey, TValue>(key, value, RedOrBlack.Red, node);
                node.RightChild = child;
            }
            FixUp(child);
        }

        private void FixUp(RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.Color == RedOrBlack.Black) return;
            var parent = node.Parent;
            if (parent == null)
            {
                //root color is black
                node.Color = RedOrBlack.Black;
                return;
            }

            if (node == parent.LeftChild && parent.Color == RedOrBlack.Red)
            {
                RightRotate(parent);
            }
            else if (node == parent.RightChild)
            {
                if (parent.Color == RedOrBlack.Red)
                {
                    LeftRotate(node);
                    RightRotate(node);
                }
                else
                {
                    if (parent.LeftChild?.Color == RedOrBlack.Red) return;
                    LeftRotate(node);
                }
            }
        }

        private void Split4node(RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.LeftChild?.Color == RedOrBlack.Red && node.RightChild?.Color == RedOrBlack.Red)
            {
                node.Color = RedOrBlack.Red;
                node.LeftChild.Color = RedOrBlack.Black;
                node.RightChild.Color = RedOrBlack.Black;
                FixUp(node);
            }
        }

        private void LeftRotate(RedBlackTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            parent.RightChild = node.LeftChild;
            node.LeftChild = parent;
            SwithParent(node);
            SwitchColor(node, parent);
        }

        private void RightRotate(RedBlackTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            parent.LeftChild = node.RightChild;
            node.RightChild = parent;
            SwithParent(node);
            SwitchColor(node, parent);
        }

        private void SwithParent(RedBlackTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            var ancestor = parent.Parent;
            node.Parent = ancestor;
            parent.Parent = node;
            if (ancestor != null)
            {
                if (ancestor.LeftChild == parent)
                {
                    ancestor.LeftChild = node;
                }
                else
                {
                    ancestor.RightChild = node;
                }
            }
            else
            {
                _root = node;
            }
        }

        private void SwitchColor(RedBlackTreeNode<TKey, TValue> a, RedBlackTreeNode<TKey, TValue> b)
        {
            var tmp = a.Color;
            a.Color = b.Color;
            b.Color = tmp;
        }

        /// <summary>
        /// get value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value, if key was not found, default of TValue</param>
        /// <returns>key was found</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            var node = _root;
            while (node != null)
            {
                var c = Comparer(key, node.Key);
                if (c == 0)
                {
                    value = node.Value;
                    return true;
                }
                else if (c < 0)
                {
                    node = node.LeftChild;
                }
                else
                {
                    node = node.RightChild;
                }
            }
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Red Black Tree Node
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">value type</typeparam>
    sealed class RedBlackTreeNode<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public RedOrBlack Color { get; set; }
        public RedBlackTreeNode<TKey, TValue> LeftChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> RightChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public RedBlackTreeNode() { }
        public RedBlackTreeNode(TKey key, TValue value, RedOrBlack color, RedBlackTreeNode<TKey, TValue> parent) => (Key, Value, Color, Parent) = (key, value, color, parent);
    }

    /// <summary>
    /// Red Or Black
    /// </summary>
    public enum RedOrBlack
    {
        /// <summary>
        /// Red
        /// </summary>
        Red,

        /// <summary>
        /// Black
        /// </summary>
        Black
    }
}
