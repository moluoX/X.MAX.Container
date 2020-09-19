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

        public RedBlackTreeNode<TKey, TValue> _root;

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
                Add(kv.Key, kv.Value);
            }
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
                else if (c < 0) node = node.LeftChild;
                else node = node.RightChild;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// add k-v
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Add(TKey key, TValue value)
        {
            if (_root == null)
            {
                _root = new RedBlackTreeNode<TKey, TValue>(key, value, false, null);
                return;
            }

            var node = _root;
            AddInner(node, key, value);
        }

        private void AddInner(RedBlackTreeNode<TKey, TValue> node, TKey key, TValue value)
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
                    AddInner(node.LeftChild, key, value);
                    return;
                }
                child = new RedBlackTreeNode<TKey, TValue>(key, value, true, node);
                node.LeftChild = child;
            }
            else
            {
                if (node.RightChild != null)
                {
                    AddInner(node.RightChild, key, value);
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
                RightRotate(parent.Parent);
            }
            else if (node == parent.RightChild)
            {
                if (parent.IsRed)
                {
                    LeftRotate(parent);
                    RightRotate(parent.Parent);
                }
                else
                {
                    if (parent.LeftChild?.IsRed == true) return;
                    LeftRotate(parent);
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
            var child = node.RightChild;
            var parent = node.Parent;

            if (parent == null) _root = child;
            else if (parent.LeftChild == node) parent.LeftChild = child;
            else parent.RightChild = child;

            child.Parent = parent;
            var childLeft = child.LeftChild;
            child.LeftChild = node;

            node.Parent = child;
            node.RightChild = childLeft;

            SwitchColor(node, child);
        }

        private void RightRotate(RedBlackTreeNode<TKey, TValue> node)
        {
            var child = node.LeftChild;
            var parent = node.Parent;

            if (parent == null) _root = child;
            else if (parent.LeftChild == node) parent.LeftChild = child;
            else parent.RightChild = child;

            child.Parent = parent;
            var childRight = child.RightChild;
            child.RightChild = node;

            node.Parent = child;
            node.LeftChild = childRight;

            SwitchColor(node, child);
        }

        private void SwitchColor(RedBlackTreeNode<TKey, TValue> a, RedBlackTreeNode<TKey, TValue> b)
        {
            var tmp = a.IsRed;
            a.IsRed = b.IsRed;
            b.IsRed = tmp;
        }

        /// <summary>
        /// remove key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key was found</returns>
        public bool Remove(TKey key)
        {
            var node = _root;
            while (node != null)
            {
                var c = Comparer(key, node.Key);
                if (c == 0) break;
                else if (c < 0) node = node.LeftChild;
                else node = node.RightChild;
            }
            if (node == null) return false;
            RemoveNode(node);
            return true;
        }

        private void RemoveNode(RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.LeftChild != null && node.RightChild != null)
            {
                //node has 2 child, find successor and copy kv to node. then remove successor
                var successor = FindSuccessor(node);
                node.CopyKV(successor);
                node = successor;
            }
            //now node has 0 or 1 child
            RemoveFix(node);
            DeleteNode(node);
        }

        private void RemoveFix(RedBlackTreeNode<TKey, TValue> node)
        {
            //now node has 0 or 1 child

            if (node.Parent == null)
            {
                //node is root. then delete it
                return;
            }

            if (node.IsRed)
            {
                //red node has 0 or 2 child, so has 0 child now. then delete node
                return;
            }
            //now node is black

            if (node.LeftChild != null)
            {
                //black node has 1 child, so child is red. then replace node by child, set child black
                node.LeftChild.IsRed = false;
                return;
            }
            if (node.RightChild != null)
            {
                //black node has 1 child, so child is red. then replace node by child, set child black
                node.RightChild.IsRed = false;
                return;
            }
            //now node is black and has 0 child

            //because node is black, it must has sibling
            if (node.Parent.RightChild == node && node.Parent.LeftChild.IsRed == true)
            {
                //node has left red sibling
                RightRotate(node.Parent);
            }
            else if (node.Parent.LeftChild == node && node.Parent.RightChild.IsRed == true)
            {
                //node has right red sibling
                LeftRotate(node.Parent);
            }
            //now sibling is black

            //black sibling has red child
            if (node.Parent.RightChild == node && node.Parent.LeftChild.LeftChild?.IsRed == true)
            {
                //node has left black sibling, and black sibling has red child. because left-leaning, child must be left child
                RightRotate(node.Parent);
                node.Parent.Parent.LeftChild.IsRed = false;
                return;
            }
            if (node.Parent.LeftChild == node && node.Parent.RightChild.LeftChild?.IsRed == true)
            {
                //node has right black sibling, and black sibling has red child. because left-leaning, child must be left child
                RightRotate(node.Parent.RightChild);
                LeftRotate(node.Parent);
                node.Parent.Parent.RightChild.IsRed = false;
                return;
            }
            //now sibling is black, and black sibling only has black child

            if (node.Parent.IsRed)
            {
                //parent is red
                if (node.Parent.RightChild == node)
                {
                    //node has left black sibling, switch color with sibling and parent
                    node.Parent.IsRed = false;
                    node.Parent.LeftChild.IsRed = true;
                    return;
                }
                if (node.Parent.LeftChild == node)
                {
                    //node has left black sibling, switch color with sibling and parent
                    node.Parent.IsRed = false;
                    node.Parent.RightChild.IsRed = true;
                    //LeftRotate, because left-leaning
                    LeftRotate(node.Parent);
                    return;
                }
            }
            //now parent is black, sibling is black, and black sibling only has black child

            if (node.Parent.RightChild == node)
            {
                //node has left black sibling, set black sibling red
                node.Parent.LeftChild.IsRed = true;
                RemoveFix(node.Parent);
                return;
            }
            if (node.Parent.LeftChild == node)
            {
                //node has right black sibling, set black sibling red
                node.Parent.RightChild.IsRed = true;
                //LeftRotate, because left-leaning
                LeftRotate(node.Parent);
                RemoveFix(node.Parent.Parent);
                return;
            }
        }

        private RedBlackTreeNode<TKey, TValue> FindSuccessor(RedBlackTreeNode<TKey, TValue> node)
        {
            var n = node.RightChild;
            while (true)
            {
                if (n.LeftChild == null) return n;
                n = n.LeftChild;
            }
        }

        private RedBlackTreeNode<TKey, TValue> DeleteNode(RedBlackTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            var child = node.LeftChild == null ? node.RightChild : node.LeftChild;
            if (parent == null)
            {
                _root = child;
                child.IsRed = false;
            }
            else if (parent.LeftChild == node) parent.LeftChild = child;
            else parent.RightChild = child;
            if (child != null) child.Parent = parent;
            node.RemoveLink();
            return child;
        }
    }

    /// <summary>
    /// Red Black Tree Node
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">value type</typeparam>
    public sealed class RedBlackTreeNode<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public bool IsRed { get; set; }
        public RedBlackTreeNode<TKey, TValue> LeftChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> RightChild { get; set; }
        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public RedBlackTreeNode() { }
        public RedBlackTreeNode(TKey key, TValue value, bool isRed, RedBlackTreeNode<TKey, TValue> parent) => (Key, Value, IsRed, Parent) = (key, value, isRed, parent);

        public void CopyKV(RedBlackTreeNode<TKey, TValue> src)
        {
            Key = src.Key;
            Value = src.Value;
        }

        public void RemoveLink()
        {
            Parent = null;
            LeftChild = null;
            RightChild = null;
        }
    }
}
