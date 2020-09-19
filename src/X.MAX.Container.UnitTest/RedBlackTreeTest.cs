using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace X.MAX.Container.UnitTest
{
    [TestClass]
    public class RedBlackTreeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dic = new Dictionary<int, string>
            {
                { 10, "10" },
                { 20, "20" },
                { 30, "30" },
                { 40, "40" },
                { 50, "50" },
                { 60, "60" },
                { 70, "70" },
                { 80, "80" },
                { 90, "90" },
                { 100, "100" },
                { 110, "110" },
                { 120, "120" }
            };
            var tree = new RedBlackTree<int, string>(dic);
            string value;
            Assert.IsTrue(tree.TryGet(10, out value));
            Assert.AreEqual(value, "10");
            Assert.IsTrue(tree.TryGet(70, out value));
            Assert.AreEqual(value, "70");
            Assert.IsTrue(tree.TryGet(100, out value));
            Assert.AreEqual(value, "100");
            Assert.IsFalse(tree.TryGet(55, out value));

            Assert.IsTrue(tree.Remove(80));

            var sb = PrintTree(tree._root);
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
    }
}
