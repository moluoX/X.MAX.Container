using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X.MAX.Container.UnitTest
{
    [TestClass]
    public class RedBlackTreeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var arr = new[] { 3, 28, 43, -43, 75, -28, 68, 96, };
            var tree = new RedBlackTree<int, int>();
            foreach (var b in arr)
            {
                if (b >= 0) tree.Add(b, b);
                else tree.Remove(-b);
                var flag = IsRedBlackTree(tree.Root);
                Assert.AreEqual(flag, "");
            }
        }

        [TestMethod]
        public void TestRandom()
        {
            var random = new Random();
            int max = 10000;
            int times = 10000;
            var cache = new List<int>();
            var tree = new RedBlackTree<int, int>();
            var sb = new StringBuilder();
            try
            {
                for (int i = 0; i < times; i++)
                {
                    var oper = random.Next(4);
                    int key, index;
                    switch (oper)
                    {
                        case 0:
                        case 1:
                            key = random.Next(max);
                            sb.Append(key + ", ");
                            if (tree.Add(key, key))
                                cache.Add(key);
                            break;
                        case 2:
                            if (cache.Count == 0) break;
                            index = random.Next(cache.Count);
                            key = cache[index];
                            sb.Append("-" + key + ", ");
                            Assert.IsTrue(tree.Remove(key));
                            cache.RemoveAt(index);
                            break;
                        case 3:
                            if (cache.Count == 0) break;
                            index = random.Next(cache.Count);
                            key = cache[index];
                            int value;
                            Assert.IsTrue(tree.TryGet(key, out value));
                            Assert.AreEqual(value, key);
                            break;
                    }
                    var flag = IsRedBlackTree(tree.Root);
                    Assert.AreEqual(flag, "");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private StringBuilder PrintTree<TKey, TValue>(RedBlackTreeNode<TKey, TValue> node)
        {
            var sb = new StringBuilder();
            var queue = new Queue<RedBlackTreeNode<TKey, TValue>>();
            queue.Enqueue(node);
            var queue2 = new Queue<RedBlackTreeNode<TKey, TValue>>();
            while (queue.Count > 0 || queue2.Count > 0)
            {
                if (queue.Count == 0)
                {
                    sb.Append("\n");
                    queue = queue2;
                    queue2 = new Queue<RedBlackTreeNode<TKey, TValue>>();
                    continue;
                }

                var n = queue.Dequeue();
                if (n == null)
                {
                    //sb.Append("--\t");
                    continue;
                }
                sb.Append(n.Key).Append(n.IsRed ? "_" : "").Append("\t");
                queue2.Enqueue(n.LeftChild);
                queue2.Enqueue(n.RightChild);
            }
            return sb;
        }

        private string IsRedBlackTree<TKey, TValue>(RedBlackTreeNode<TKey, TValue> root)
        {
            if (root == null) return "";
            var bh = -1;
            if (root.IsRed) return "根是红色";

            var stack = new Stack<RedBlackTreeNode<TKey, TValue>>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var n = stack.Pop();
                //if (n.RightChild?.IsRed == true && (n.LeftChild == null || !n.LeftChild.IsRed))
                //{
                //    return $"非左倾 {n.RightChild.Key}";
                //}
                if (n.LeftChild == null || n.RightChild == null)
                {
                    //叶子
                    var nbh = CalBH(n);
                    if (bh == -1) bh = nbh;
                    else if (bh != nbh)
                    {
                        return $"黑高不对 {bh}-{nbh} {n.Key}";
                    }
                    continue;
                }
                if (n.RightChild != null)
                {
                    if (n.IsRed && n.RightChild.IsRed) return $"父 {n.Key} 子 {n.RightChild.Key} 皆红";
                    stack.Push(n.RightChild);
                }
                if (n.LeftChild != null)
                {
                    if (n.IsRed && n.LeftChild.IsRed) return $"父 {n.Key} 子 {n.LeftChild.Key} 皆红";
                    stack.Push(n.LeftChild);
                }
            }
            return "";
        }

        private int CalBH<TKey, TValue>(RedBlackTreeNode<TKey, TValue> node)
        {
            var bh = 1;
            var up = node;
            while (up != null)
            {
                if (!up.IsRed) bh++;
                up = up.Parent;
            }
            return bh;
        }
    }
}
