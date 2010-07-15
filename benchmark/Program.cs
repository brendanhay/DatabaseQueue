using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using DatabaseQueue.Serialization;
using DatabaseQueue.Tests;

namespace DatabaseQueue.Benchmark
{
    internal static class Program
    {
        private const int ITERATIONS = 5, 
            COLLECTION_SIZE = 1000;
        
        private static readonly string _directory = Path.Combine(Environment.CurrentDirectory, 
            "queue_benchmarks");

        public static void Main(string[] args)
        {
            Clean();

            Console.WriteLine("Starting{0}", Environment.NewLine);

            var formats = Enum.GetValues(typeof(FormatType));
            var databases = Enum.GetValues(typeof(DatabaseType));

            var serializerFactory = new SerializerFactory<Entity>();

            foreach (FormatType format in formats)
            {
                var serializer = serializerFactory.Create(format);

                BenchmarkSerializer(serializer, Entity.Create, ITERATIONS);
            }

            Thread.Sleep(250);

            var queueFactory = new DatabaseQueueFactory<Entity>(serializerFactory);

            foreach (DatabaseType database in databases)
            {
                foreach (FormatType format in formats)
                {
                    var name = database + format.ToString();
                    var path = name + (database == DatabaseType.SqlCompact ? ".sdf" : ".db");

                    var closureFormat = format;
                    var closureDatabase = database;

                    BenchmarkQueue(name, () => queueFactory.Create(path, closureDatabase, closureFormat),
                        () => Entity.CreateCollection(COLLECTION_SIZE), ITERATIONS);
                }
            }

            Console.WriteLine("Finished{0}", Environment.NewLine);
        }

        private static void BenchmarkSerializer<T>(ISerializer<T> serializer,
            Func<T> factory, int iterations)
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

            WriteAverages("Serialization", 1, serialization);
            WriteAverages("Deserialization", 1, deserialization);

            //WriteMeasurement("Entity Size", size(serialized).LongLength, "bytes");
            Console.WriteLine();
        }

        private static void BenchmarkQueue<T>(string name, Func<IQueue<T>> queueFactory,
            Func<ICollection<T>> collectionFactory, int iterations)
        {
            var watch = new Stopwatch();
            var enqueued = new List<long>(iterations);
            var dequeued = new List<long>(iterations);

            var collection = collectionFactory();
            WriteTitle(name, collection.Count);

            watch.Reset();
            watch.Start();

            // Queue Initialization
            var queue = queueFactory();

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

            WriteAverages("Enqueue", collection.Count, enqueued);
            WriteAverages("Dequeue", collection.Count, dequeued);

            WriteCount(queue.Count);

            // Dispose
            watch.Reset();
            watch.Start();

            queue.Dispose();

            watch.Stop();

            WriteMeasurement("Dispose", watch.ElapsedMilliseconds, "ms");
            Console.WriteLine();
        }

        private static void Clean()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        private static string GetPath(string name)
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            return Path.Combine(_directory, name);
        }

        private static void WriteTitle(string name)
        {
            WriteTitle(name, null);
        }

        private static void WriteTitle(string name, int? itemCount)
        {
            Console.WriteLine("{0,-19} {1} {2}", name, itemCount,
                itemCount.HasValue ? "items" : null);
        }

        private static void WriteMeasurement(string method, long measurement, string suffix)
        {
            Console.WriteLine(" {0,-18} {1} {2}", string.Format("<{0}>", method), measurement, 
                suffix);
        }

        private static void WriteAverages(string method, int? total, IEnumerable<long> events)
        {
            var average = Math.Round(events.Average(), 1);

            var formattedAverage = string.Format("Average (ms): {0}", average);
            var formattedThroughput = total.HasValue
                ? string.Format("Throughput (per second): {0}",
                Math.Round((total.Value / average) * 1000, 1)) : null;
            var formattedEvents = string.Format("Events (ms): {0} ", 
                string.Join(", ", events.Select(i => i.ToString()).ToArray()));
            Console.WriteLine(" {0,-18} {1}, {2}, {3}", string.Format("<{0}>", method), 
                              formattedAverage, formattedThroughput, formattedEvents);
        }

        private static void WriteCount(int count)
        {
            Console.WriteLine(" <Count> {0,-18}", count);
        }
    }
}
