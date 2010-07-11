using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DatabaseQueue.Tests;

namespace DatabaseQueue.Benchmark
{
    internal static class Program
    {
        private const int ITERATIONS = 5;

        public static void Main(string[] args)
        {

            Console.WriteLine("Starting{0}", Environment.NewLine);

            var serializers = new Dictionary<ISerializer<DummyEntity>, Func<object, byte[]>> { 
                { new BinarySerializer<DummyEntity>(), bytes => ((byte[])bytes) },
                { new XmlSerializer<DummyEntity>(), xml => Encoding.UTF8.GetBytes(xml.ToString()) },
                { new JsonSerializer<DummyEntity>(), json => Encoding.UTF8.GetBytes(json.ToString()) }
            };

            foreach (var pair in serializers)
            {
                BenchmarkSerializer(pair.Key, pair.Value, DummyEntity.Create, ITERATIONS);

                Thread.Sleep(250);
            }

            var queues = new Dictionary<string, IQueue<DummyEntity>> {
                { "BinaryQueue", SqliteQueue.CreateBinaryQueue<DummyEntity>("BinaryBenchmark.queue") },
                { "XmlQueue", SqliteQueue.CreateXmlQueue<DummyEntity>("XmlBenchmark.queue") },
                { "JsonQueue", SqliteQueue.CreateJsonQueue<DummyEntity>("JsonBenchmark.queue") }
            };

            foreach (var pair in queues)
            {
                var queue = pair.Value as DatabaseQueueBase<DummyEntity>;

                BenchmarkQueue(pair.Key, queue, () => DummyEntity.CreateCollection(1000), ITERATIONS);

                Thread.Sleep(250);
            }

            Console.WriteLine("Finished{0}", Environment.NewLine);
        }

        private static void BenchmarkSerializer<T>(ISerializer<T> serializer, 
            Func<object, byte[]> size, Func<T> factory, int iterations)
        {
            var watch = new Stopwatch();
            var serialization = new List<long>(iterations);
            var deserialization = new List<long>(iterations);
            
            object serialized = null;
            T deserialized;

            for (var i = 0; i < iterations; i++)
            {
                // Serialization
                watch.Reset();
                watch.Start();

                if (!serializer.TrySerialize(factory(), out serialized))
                    Debug.Assert(false);

                watch.Stop();
                serialization.Add(watch.ElapsedMilliseconds);

                // Deserialization
                watch.Reset();
                watch.Start();

                if (!serializer.TryDeserialize(serialized, out deserialized))
                    Debug.Assert(false);

                watch.Stop();
                deserialization.Add(watch.ElapsedMilliseconds);
            }

            WriteTitle(serializer.GetType().Name);
            
            WriteAverages("Serialization", serialization);
            WriteAverages("Deserialization", deserialization);
            
            WriteMeasurement("Entity Size", size(serialized).LongLength, "bytes");
            Console.WriteLine();
        }

        private static void BenchmarkQueue<T>(string name, DatabaseQueueBase<T> queue, 
            Func<ICollection<T>> factory, int iterations)
        {
            var watch = new Stopwatch();
            var enqueued = new List<long>(iterations);
            var dequeued = new List<long>(iterations);

            // Queue Initialization
            var collection = factory();

            WriteTitle(name, collection.Count);

            watch.Reset();
            watch.Start();

            queue.Initialize();

            watch.Stop();

            WriteMeasurement("Initialize", watch.ElapsedMilliseconds, "ms");

            for (var i = 0; i < 5; i++)
            {
                // Enqueue
                watch.Reset();
                watch.Start();

                if (!queue.TryEnqueueMultiple(collection))
                    Debug.Assert(false);

                watch.Stop();
                enqueued.Add(watch.ElapsedMilliseconds);

                // Deuque
                watch.Reset();
                watch.Start();

                ICollection<T> items;

                if (!queue.TryDequeueMultiple(out items, collection.Count))
                    Debug.Assert(false);

                watch.Stop();
                dequeued.Add(watch.ElapsedMilliseconds);
            }

            WriteAverages("Enqueue", enqueued);
            WriteAverages("Dequeue", dequeued);

            // Dispose
            watch.Reset();
            watch.Start();

            queue.Dispose();

            watch.Stop();

            WriteMeasurement("Dispose", watch.ElapsedMilliseconds, "ms");
            Console.WriteLine();
        }

        private static void WriteTitle(string name)
        {
            WriteTitle(name, null);
        }

        private static void WriteTitle(string name, int? itemCount)
        {
            Console.WriteLine("[{0}] {1} {2}", name, itemCount, 
                itemCount.HasValue ? "items" : null);
        }

        private static void WriteMeasurement(string method, long measurement, string suffix)
        {
            Console.WriteLine(" {1}{0}  -> {2} {3}", Environment.NewLine, method, measurement, suffix);   
        }

        private static void WriteAverages(string method, IEnumerable<long> events)
        {
            Console.WriteLine(" {1}{0}  -> Average: {2} ms{0}  -> Events: {3}",
                Environment.NewLine, method, events.Average(),
                string.Join(", ", events.Select(i => i.ToString()).ToArray()));
        }
    }
}
