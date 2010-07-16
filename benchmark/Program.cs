using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DatabaseQueue.Collections;
using DatabaseQueue.Data;
using DatabaseQueue.Diagnostics;
using DatabaseQueue.Serialization;

namespace DatabaseQueue.Benchmark
{
    internal static class Program
    {
        private static readonly Func<IList<int>> Counts
            = () => new List<int> { 100, 100, 500, 500, 1500, 1500, 3000, 3000, 5000 };
            //= () => Enumerable.Range(1, 5).Select(i => (int)Math.Pow(2, i) * 100).ToList();

        private static readonly string _directory = Path.Combine(Environment.CurrentDirectory,
            "queue_benchmarks");

        public static void Main(string[] args)
        {
            Clean();

            Console.WriteLine("Starting{0}", Environment.NewLine);

            var formats = Enum.GetValues(typeof(FormatType));
            var databases = Enum.GetValues(typeof(DatabaseType));

            var serializerFactory = new SerializerFactory();

            foreach (FormatType format in formats)
            {
                var serializer = serializerFactory.Create<Entity>(format);

                BenchmarkSerializer(serializer, Entity.CreateCollection);
            }

            Thread.Sleep(250);

            var queueFactory = new DatabaseQueueFactory<Entity>(serializerFactory);

            foreach (DatabaseType database in databases)
            {
                foreach (FormatType format in formats)
                {
                    var name = database + format.ToString();
                    var path = GetPath(name + (database == DatabaseType.SqlCompact ? ".sdf" : ".db"));

                    var closureFormat = format;
                    var closureDatabase = database;

                    BenchmarkQueue(name, () => queueFactory.Create(path, closureDatabase, 
                        closureFormat, new QueuePerformanceCounter(name)), Entity.CreateCollection);
                }
            }

            Console.WriteLine("Finished{0}", Environment.NewLine);
        }

        private static void BenchmarkQueue<T>(string name, Func<IQueue<T>> queueFactory,
            Func<int, ICollection<T>> collectionFactory)
        {
            var watch = new Stopwatch();
            var enqueued = new List<long>();
            var dequeued = new List<long>();

            var counts = Counts();
            var total = counts.Sum();

            WriteTitle(name);

            watch.Reset();
            watch.Start();

            // Queue Initialization
            var queue = queueFactory();

            watch.Stop();

            WriteMeasurement("Initialize", watch.ElapsedMilliseconds, "ms");

            WriteMethod("Items");

            foreach (var count in counts)
            {
                var collection = collectionFactory(count);

                Console.Write("{0} ", count);

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

            Console.WriteLine();

            WriteAverage("Enqueue", total, enqueued);
            WriteAverage("Dequeue", total, dequeued);
            WriteThroughput(total, enqueued, dequeued);

            WriteCount(queue.Count);

            // Dispose
            watch.Reset();
            watch.Start();

            queue.Dispose();

            watch.Stop();

            WriteMeasurement("Dispose", watch.ElapsedMilliseconds, "ms");
            Console.WriteLine();
        }

        private static void BenchmarkSerializer<T>(ISerializer<T> serializer,
            Func<int, ICollection<T>> collectionFactory)
        {
            var watch = new Stopwatch();
            var serialization = new List<long>();
            var deserialization = new List<long>();

            var counts = Counts();
            var total = counts.Sum();

            object serialized = null;
            T deserialized;

            WriteTitle(serializer.GetType().Name);
            WriteMethod("Items");

            foreach (var count in counts)
            {
                var collection = collectionFactory(count);

                Console.Write("{0} ", count);

                foreach (var item in collection)
                {
                    // Serialization
                    watch.Reset();
                    watch.Start();

                    if (!serializer.TrySerialize(item, out serialized))
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
            }

            Console.WriteLine();

            WriteAverage("Serialization", total, serialization);
            WriteAverage("Deserialization", total, deserialization);
            WriteThroughput(total, serialization, deserialization);

            //WriteMeasurement("Entity Size", size(serialized).LongLength, "bytes");
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

        private static void WriteMethod(string method)
        {
            Console.Write(" {0,-18} ", string.Format("<{0}>", method));
        }

        private static void WriteMeasurement(string method, long measurement, string suffix)
        {
            WriteMethod(method);

            Console.WriteLine("{0} {1}", measurement, suffix);
        }

        private static void WriteCount(int count)
        {
            WriteMethod("Count");

            Console.Write(count);
            Console.WriteLine();
        }

        private static void WriteAverage(string method, int total, IEnumerable<long> events)
        {
            WriteMethod(method);

            var averageTime = Math.Round(events.Average(), 2);
            var averageItems =  Math.Round((double)(total / events.Count()), 2);

            Console.Write("{0} ms per {1} items", averageTime, averageItems);
            Console.WriteLine();
        }

        private static void WriteThroughput(int total, IEnumerable<long> @in, IEnumerable<long> @out)
        {
            WriteMethod("Throughput");

            Console.Write("{0} in, {1} out per second", CalculateThroughput(total, @in),
                CalculateThroughput(total, @out));
            Console.WriteLine();
        }

        private static double CalculateThroughput(int total, IEnumerable<long> events)
        {
            var averageTime = events.Average();
            var averageItems = total / events.Count();

            return Math.Round((1000 / averageTime) * averageItems, 0);
        }
    }
}
