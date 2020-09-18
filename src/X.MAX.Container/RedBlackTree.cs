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
                _root = new RedBlackTreeNode<TKey, TValue>(key, value, false, null);
                return;
            }

            var node = _root;
            InsertInner(node, key, value);
        }

        private void InsertInner(RedBlackTreeNode<TKey, TValue> node, TKey key, TValue value)
        {
            Split4node(node);

            var c = Comparer(key, node.Key);
            if (c == 0)
            {
                node.Key = key;
                node.Value = value;
                return;
            }

            RedBlackTreeNode<TKey, TValue> child;
            if (c < 0)
            {
                if (node.LeftChild != null)
                {
                    InsertInner(node.LeftChild, key, value);
                    return;
                }
                child = new RedBlackTreeNode<TKey, TValue>(key, value, true, node);
                node.LeftChild = child;
            }
            else
            {
                if (node.RightChild != null)
                {
                    InsertInner(node.RightChild, key, value);
                    return;
                }
                child = new RedBlackTreeNode<TKey, TValue>(key, value, true, node);
                node.RightChild = child;
            }
            FixUp(child);
        }

        private void FixUp(RedBlackTreeNode<TKey, TValue> node)
        {
            if (!node.IsRed) return;
            var parent = node.Parent;
            if (parent == null)
            {
                //root color is black
                node.IsRed = false;
                return;
            }

            if (node == parent.LeftChild && parent.IsRed)
            {
                RightRotate(parent);
            }
            else if (node == parent.RightChild)
            {
                if (parent.IsRed)
                {
                    LeftRotate(node);
                    RightRotate(node);
                }
                else
                {
                    if (parent.LeftChild?.IsRed == true) return;
                    LeftRotate(node);
                }
            }
        }

        private void Split4node(RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.LeftChild?.IsRed == true && node.RightChild?.IsRed == true)
            {
                node.IsRed = true;
                node.LeftChild.IsRed = false;
                node.RightChild.IsRed = false;
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
            var tmp = a.IsRed;
            a.IsRed = b.IsRed;
            b.IsRed = tmp;
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
        public bool IsRed { get; set; }
        public RedBlackTreeNode<TKey, TValue> LeftChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> RightChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public RedBlackTreeNode() { }
        public RedBlackTreeNode(TKey key, TValue value, bool isRed, RedBlackTreeNode<TKey, TValue> parent) => (Key, Value, IsRed, Parent) = (key, value, isRed, parent);
    }
}
