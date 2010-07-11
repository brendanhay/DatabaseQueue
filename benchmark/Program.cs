using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseQueue.Tests;

namespace DatabaseQueue.Benchmark
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting{0}", Environment.NewLine);

            var binarySerializer = new BinarySerializer<DummyEntity>();
            var jsonSerializer = new JsonSerializer<DummyEntity>();

            var binaryQueue = CreateQueue("BinaryBenchmark.queue", StorageSchema.Binary(), binarySerializer);
            var jsonQueue = CreateQueue("JsonBenchmark.queue", StorageSchema.Json(), jsonSerializer);

            const int iterations = 5;

            BenchmarkSerializer("Binary Serializer", binarySerializer, 
                serialized => ((byte[])serialized), iterations);
            BenchmarkSerializer("Json Serializer", jsonSerializer,
                serialized => Encoding.UTF8.GetBytes(serialized.ToString()), iterations);

            BenchmarkQueue("Binary Queue", binaryQueue, () => DummyEntity.CreateCollection(1000), iterations);
            BenchmarkQueue("Json Queue", jsonQueue, () => DummyEntity.CreateCollection(1000), iterations);
            
            Console.WriteLine("Finished{0}", Environment.NewLine);
        }

        private static DatabaseQueueBase<T> CreateQueue<T>(string name, IStorageSchema schema, 
            ISerializer<T> serializer)
        {
            return new SqliteQueue<T>(Path.Combine(Environment.CurrentDirectory, name), schema, 
                serializer);
        }

        private static void BenchmarkSerializer<T>(string name, ISerializer<T> serializer, 
            Func<object, byte[]> size, int iterations) where T : new()
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

                if (!serializer.TrySerialize(new T(), out serialized))
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

            WriteTitle(name);
            
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

                if (!queue.TryEnqueueMultiple(collection, 0))
                    Debug.Assert(false);

                watch.Stop();
                enqueued.Add(watch.ElapsedMilliseconds);

                // Deuque
                watch.Reset();
                watch.Start();

                ICollection<T> items;

                if (!queue.TryDequeueMultiple(out items, collection.Count, 0))
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
            Console.WriteLine(" {1}{0}  -> {2} ms", Environment.NewLine, method, measurement, suffix);   
        }

        private static void WriteAverages(string method, IEnumerable<long> events)
        {
            Console.WriteLine(" {1}{0}  -> Average: {2} ms{0}  -> Events: {3}",
                Environment.NewLine, method, events.Average(),
                string.Join(", ", events.Select(i => i.ToString()).ToArray()));
        }
    }
}
