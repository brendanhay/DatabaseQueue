using System.Collections.Generic;
using System.IO;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseQueue.Tests.Collections
{
    public abstract class QueueTestBase : MockFactory
    {
        #region Assertions

        protected void Assert_FileExists(string path)
        {
            Assert.IsTrue(File.Exists(path), "File does not exist");
        }

        protected void Assert_TryEnqueueMultiple_IsTrue(IQueue<Entity> queue)
        {
            Assert.IsTrue(queue.TryEnqueueMultiple(Items), "TryEnqueueMultiple failed");
        }

        protected void Assert_TryEnqueueMultiple_NullItems_IsFalse(IQueue<Entity> queue)
        {
            Assert.IsFalse(queue.TryEnqueueMultiple(null), "TryEnqueueMultiple succeeded");
        }

        protected void Assert_TryDequeueMultiple_IsTrue(IQueue<Entity> queue)
        {
            ICollection<Entity> items;

            Assert.IsTrue(queue.TryEnqueueMultiple(Items), "TryEnqueueMultiple failed");
            Assert.IsTrue(queue.TryDequeueMultiple(out items, Items.Count), 
                "TryDequeueMultiple failed");
        }
        
        protected void Assert_TryDequeueMultiple_RemovesItemsFromQueue(IQueue<Entity> queue)
        {
            ICollection<Entity> items;

            Assert_TryEnqueueMultiple_IsTrue(queue);
            Assert.IsTrue(queue.TryDequeueMultiple(out items, int.MaxValue), 
                "TryDequeueMultiple failed");
            Assert.IsTrue(queue.Count == 0, "Queue was expected to have 0 items");
        }

        protected void Assert_TryDequeueMultiple_0Max_IsFalse(IQueue<Entity> queue)
        {
            ICollection<Entity> items;

            Assert.IsFalse(queue.TryDequeueMultiple(out items, 0), "TryDequeueMultiple succeeded");
            Assert.IsTrue(items.Count == 0, "TryDequeueMultiple returned more than 0 items");
        }

        #endregion


    }
}
