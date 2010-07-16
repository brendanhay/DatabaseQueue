using System.Collections.Generic;
using System.IO;
using DatabaseQueue.Benchmark;
using DatabaseQueue.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DatabaseQueue.Tests
{
    /// <summary>
    /// Contains inheritable uninitialized properties that can be bound
    /// in the test/class initialization phase and static factory methods to create
    /// valid recursive mocks
    /// </summary>
    public class MockFactory
    {
        public static readonly ICollection<Entity> Items = Entity.CreateCollection(100);
        
        #region Uninitialized Inheritable Mock Properties

        protected Mock<IQueue<Entity>> QueueMock { get; set; }

        #endregion

        #region Static Factory Methods

        public static Mock<IQueue<Entity>> CreateIQueueMock()
        {
            var items = Entity.CreateCollection(100);

            var queueMock = new Mock<IQueue<Entity>>();
            queueMock.Setup(mock => mock.TryDequeueMultiple(out items, It.IsAny<int>())).Returns(true);
            queueMock.Setup(mock => mock.TryEnqueueMultiple(It.IsAny<ICollection<Entity>>())).Returns(true);
            queueMock.SetupGet(mock => mock.SyncRoot).Returns(queueMock.Object);

            return queueMock;
        }

        #endregion

        #region Static Helpers

        public static string GetFilePath(TestContext context, string fileName)
        {
            return Path.Combine(context.TestDeploymentDir, fileName);
        }

        #endregion
    }
}
