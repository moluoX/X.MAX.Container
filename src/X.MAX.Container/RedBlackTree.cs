using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace X.MAX.Container
{
    /// <summary>
    /// Red Black Tree
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

        /// <summary>
        /// tree root
        /// </summary>
        public RedBlackTreeNode<TKey, TValue> Root { get; private set; }

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
            var node = Root;
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
        public bool Add(TKey key, TValue value)
        {
            if (Root == null)
            {
                Root = new RedBlackTreeNode<TKey, TValue>(key, value, false, null);
                return true;
            }

            var node = Root;
            return AddInner(node, key, value);
        }

        private bool AddInner(RedBlackTreeNode<TKey, TValue> node, TKey key, TValue value)
        {
            var c = Comparer(key, node.Key);
            if (c == 0)
            {
                node.Key = key;
                node.Value = value;
                return false;
            }

            RedBlackTreeNode<TKey, TValue> child;
            if (c < 0)
            {
                if (node.LeftChild != null)
                {
                    return AddInner(node.LeftChild, key, value);
                }
                child = new RedBlackTreeNode<TKey, TValue>(key, value, true, node);
                node.LeftChild = child;
            }
            else
            {
                if (node.RightChild != null)
                {
                    return AddInner(node.RightChild, key, value);
                }
                child = new RedBlackTreeNode<TKey, TValue>(key, value, true, node);
                node.RightChild = child;
            }
            AddFix(child);
            return true;
        }

        private void AddFix(RedBlackTreeNode<TKey, TValue> node)
        {
            var parent = node.Parent;
            if (parent == null)
            {
                //root color is black
                node.IsRed = false;
                return;
            }
            if (!parent.IsRed) return;
            //now parent is red

            if ((parent.Parent.LeftChild == parent && parent.Parent.RightChild?.IsRed == true)
                || (parent.Parent.RightChild == parent && parent.Parent.LeftChild?.IsRed == true))
            {
                //has red uncle
                Split4node(parent.Parent);
                AddFix(parent.Parent);
                return;
            }
            //now only has black uncle or no uncle

            if (parent.Parent.LeftChild == parent)
            {
                //parent on left
                if (parent.LeftChild == node) RightRotate(parent.Parent, true);
                else
                {
                    LeftRotate(parent);
                    RightRotate(node.Parent);
                    node.IsRed = false;
                    node.RightChild.IsRed = true;
                }
            }
            else
            {
                //parent on right
                if (parent.RightChild == node) LeftRotate(parent.Parent, true);
                else
                {
                    RightRotate(parent);
                    LeftRotate(node.Parent);
                    node.IsRed = false;
                    node.LeftChild.IsRed = true;
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
                AddFix(node);
            }
        }

        private void LeftRotate(RedBlackTreeNode<TKey, TValue> node, bool switchColor = false)
        {
            var child = node.RightChild;
            var parent = node.Parent;

            if (parent == null) Root = child;
            else if (parent.LeftChild == node) parent.LeftChild = child;
            else parent.RightChild = child;

            child.Parent = parent;
            var childLeft = child.LeftChild;
            child.LeftChild = node;

            node.Parent = child;
            node.RightChild = childLeft;
            if (childLeft != null) childLeft.Parent = node;

            if (switchColor) SwitchColor(node, child);
        }

        private void RightRotate(RedBlackTreeNode<TKey, TValue> node, bool switchColor = false)
        {
            var child = node.LeftChild;
            var parent = node.Parent;

            if (parent == null) Root = child;
            else if (parent.LeftChild == node) parent.LeftChild = child;
            else parent.RightChild = child;

            child.Parent = parent;
            var childRight = child.RightChild;
            child.RightChild = node;

            node.Parent = child;
            node.LeftChild = childRight;
            if (childRight != null) childRight.Parent = node;

            if (switchColor) SwitchColor(node, child);
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
            var node = Root;
            while (node != null)
            {
                var c = Comparer(key, node.Key);
                if (c == 0) break;
                else if (c < 0) node = node.LeftChild;
                else node = node.RightChild;
            }
            if (node == null) return false;
            RemoveFix(ref node);
            DeleteNode(node);
            return true;
        }

        private void RemoveFix(ref RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.LeftChild != null && node.RightChild != null)
            {
                //node has 2 child, find successor and copy kv to node. then remove successor
                var successor = FindSuccessor(node);
                node.CopyKV(successor);
                node = successor;
            }
            //now node has 0 or 1 child

            //red node has 0 or 2 child, so has 0 child now. done
            if (node.IsRed) return;
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

            RemoveFixBlackLeaf(node);
        }

        private void RemoveFixBlackLeaf(RedBlackTreeNode<TKey, TValue> node)
        {
            //now node is black and has 0 child
            //node is root. done
            if (node.Parent == null) return;
            var parent = node.Parent;

            //because node is black, it must has sibling
            //node has left red sibling
            if (parent.RightChild == node && parent.LeftChild.IsRed == true) RightRotate(parent, true);
            //node has right red sibling
            else if (parent.LeftChild == node && parent.RightChild.IsRed == true) LeftRotate(parent, true);
            //now sibling is black

            //black sibling has red child
            if (parent.RightChild == node)
            {
                if (parent.LeftChild.LeftChild?.IsRed == true)
                {
                    //node has left black sibling, and black sibling has red left child
                    RightRotate(parent, true);
                    parent.Parent.LeftChild.IsRed = false;
                    return;
                }
                if (parent.LeftChild.RightChild?.IsRed == true)
                {
                    //node has left black sibling, and black sibling has red right child
                    LeftRotate(parent.LeftChild);
                    RightRotate(parent);
                    parent.Parent.IsRed = parent.IsRed;
                    parent.IsRed = false;
                    return;
                }
            }
            if (parent.LeftChild == node)
            {
                if (parent.RightChild.RightChild?.IsRed == true)
                {
                    //node has right black sibling, and black sibling has red right child
                    LeftRotate(parent, true);
                    parent.Parent.RightChild.IsRed = false;
                    return;
                }
                if (parent.RightChild.LeftChild?.IsRed == true)
                {
                    //node has right black sibling, and black sibling has red left child
                    RightRotate(parent.RightChild);
                    LeftRotate(parent);
                    parent.Parent.IsRed = parent.IsRed;
                    parent.IsRed = false;
                    return;
                }
            }
            //now sibling is black, and black sibling only has black child

            if (parent.IsRed)
            {
                //parent is red
                if (parent.RightChild == node)
                {
                    //node has left black sibling, switch color with sibling and parent
                    parent.IsRed = false;
                    parent.LeftChild.IsRed = true;
                    return;
                }
                if (parent.LeftChild == node)
                {
                    //node has left black sibling, switch color with sibling and parent
                    parent.IsRed = false;
                    parent.RightChild.IsRed = true;
                    return;
                }
            }
            //now parent is black, sibling is black, and black sibling only has black child

            //node has left black sibling, set black sibling red
            if (parent.RightChild == node) parent.LeftChild.IsRed = true;
            //node has right black sibling, set black sibling red
            else if (parent.LeftChild == node) parent.RightChild.IsRed = true;
            RemoveFixBlackLeaf(parent);
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
                Root = child;
                if (child != null) child.IsRed = false;
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

        internal RedBlackTreeNode() { }
        internal RedBlackTreeNode(TKey key, TValue value, bool isRed, RedBlackTreeNode<TKey, TValue> parent) => (Key, Value, IsRed, Parent) = (key, value, isRed, parent);

        internal void CopyKV(RedBlackTreeNode<TKey, TValue> src)
        {
            Key = src.Key;
            Value = src.Value;
        }

        internal void RemoveLink()
        {
            Parent = null;
            LeftChild = null;
            RightChild = null;
        }
    }
}
