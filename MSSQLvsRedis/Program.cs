/*
(C) 2024 Serguei Tarassov <serge@arbinada.com>
Performance testing of SQL Server and Redis

Dependencies:
$ dotnet add package Microsoft.Data.SqlClient
$ dotnet add package NRedisStack
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using System.Collections;
using System.Linq;


namespace SqlRedisTestApp
{
    using TestDataItem = KeyValuePair<Guid, string>;

    public class TestData : IEnumerable<TestDataItem>
    {
        public const int MAX_COUNT = 1000000;
        List<TestDataItem> m_data = new List<TestDataItem>();

        public void Clear()
        {
            m_data.Clear();
        }

        public int Count {
            get { return m_data.Count; }
        }

        public void AddKey(Guid key, string value)
        {
            m_data.Add(new TestDataItem(key, value));
        }

        public IEnumerator<TestDataItem> GetEnumerator()
        {
            return m_data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_data.GetEnumerator();
        }

        public TestDataItem this[int index] {
            get { return m_data[index]; }
        }
    }

    class RequestProfile
    {
        public long MinTimeMksec = 0;
        public long MaxTimeMksec = 0;
        public long AvgTimeMksec = 0;
        public long ElapsedTimeMsec = 0;
        public long RequestsPerSec = 0;
    }

    class TestResult
    {
        public List<RequestProfile> ResultByThread;
        public TestResult(int thread_count)
        {
            ResultByThread = new List<RequestProfile>();
            for (int i = 0; i < thread_count; i++)
            {
                ResultByThread.Add(new RequestProfile());
            }
        }
    }

    abstract class Tester
    {
        protected TestData m_data;
        protected TestResult m_result;
        protected int m_thread_count;
        protected int m_query_count_per_thread;

        public Tester(TestData data, int thread_count, int query_count_per_thread)
        {
            m_data = data;
            m_thread_count = thread_count;
            m_query_count_per_thread = query_count_per_thread;
            m_result = new TestResult(thread_count);
        }

        public TestResult Result {
            get { return m_result; }
        }

        public void Run()
        {
            var threads = new List<Thread>();
            for (int i = 0; i < m_thread_count; i++)
            {
                var thread = new Thread(RunInThread);
                // var thread = new Thread(() => this.RunInThread(i));
                // thread.Name = $"Consumer{i}";
                threads.Add(thread);
                thread.Start(i);
            }
            foreach (var thread in threads)
                thread.Join();
        }

        protected abstract void RunInThread(object data);
    }

    class SqlTester : Tester
    {
        public SqlTester(TestData data, int thread_count, int query_count_per_thread)
            : base(data, thread_count, query_count_per_thread)
        {}

        public const string CONNECTION_STRING = "Data Source=127.0.0.1;Initial Catalog=kv_db;User id=sa;Password=PassW0rd;TrustServerCertificate=Yes;";

        public void LoadData()
        {
            var cn = new SqlConnection(CONNECTION_STRING);
            cn.Open();
            var cmd = new SqlCommand("SELECT COUNT(1) FROM kv", cn);
            var count = cmd.ExecuteScalar();
            Console.WriteLine($"Table row count: {count}");
            cmd = new SqlCommand("SELECT k, v FROM kv", cn);
            var reader  = cmd.ExecuteReader();
            m_data.Clear();
            while (reader.Read())
            {
                m_data.AddKey(reader.GetGuid(0), reader.GetString(1));
            }
        }

        protected override void RunInThread(object data)
        {
            var thread_num = (int)data;
            Console.WriteLine($"Started SQLServer thread: {thread_num}");
            var cn = new SqlConnection(CONNECTION_STRING);
            cn.Open();
            var cmd = new SqlCommand("SELECT v FROM kv WHERE k = @k", cn);
            cmd.Parameters.Add("@k", SqlDbType.UniqueIdentifier);
            cmd.Prepare();
            Random rnd = new Random();
            int max_count = m_data.Count + 1;
            long min_time_mksec = long.MaxValue;
            long max_time_mksec = 0;
            long elapsed_mksec = 0;
            for (int i = 0; i < m_query_count_per_thread; i++)
            {
                int idx = rnd.Next(0, max_count);
                var item = m_data[idx];
                cmd.Parameters["@k"].Value = item.Key;
                var watch = Stopwatch.StartNew();
                var v = (string)cmd.ExecuteScalar();
                watch.Stop();
                if (v != item.Value)
                    throw new ApplicationException($"Unexpected value {item.Value}. Key: {item.Key}");
                long time_mksec = watch.ElapsedTicks / (Stopwatch.Frequency / 1000000);
                elapsed_mksec += time_mksec;
                if (time_mksec < min_time_mksec)
                    min_time_mksec = time_mksec;
                if (time_mksec > max_time_mksec)
                    max_time_mksec = time_mksec;
            }
            m_result.ResultByThread[thread_num].ElapsedTimeMsec = elapsed_mksec / 1000;
            m_result.ResultByThread[thread_num].MinTimeMksec = min_time_mksec;
            m_result.ResultByThread[thread_num].MaxTimeMksec = max_time_mksec;
            m_result.ResultByThread[thread_num].AvgTimeMksec = elapsed_mksec / m_query_count_per_thread;
            m_result.ResultByThread[thread_num].RequestsPerSec = 1000000 / m_result.ResultByThread[thread_num].AvgTimeMksec;
        }
    }


    class RedisTester : Tester
    {
        public RedisTester(TestData data, int thread_count, int query_count_per_thread)
            : base(data, thread_count, query_count_per_thread)
        {}

        public bool ResetData { get; set; } = false;

        const string SERVER_NAME = "localhost:6379";
        ConfigurationOptions CONNECTION_CONFIG_OPTIONS = new ConfigurationOptions {
            EndPoints = { SERVER_NAME }
        };
        public void InitData()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(CONNECTION_CONFIG_OPTIONS);
            IDatabase db = redis.GetDatabase();
            var server = redis.GetServer(SERVER_NAME);
            bool force_reset_data = ResetData;
            if (server.Keys().Count() != m_data.Count)
            {
                Console.Write($"Key count {server.Keys().Count()} is different. Expected: {m_data.Count}");
                force_reset_data = true;
            }
            if (force_reset_data)
            {
                Console.Write("Deleting existing keys...");
                int i = 1;
                foreach (var key in server.Keys())
                {
                    db.KeyDelete(key);
                    if (i++ % 10000 == 0)
                        Console.Write(".");
                }
                Console.WriteLine("OK");
                Console.Write("Writing Redis test data...");
                i = 1;
                foreach (var item in m_data)
                {
                    db.StringSet(item.Key.ToString(), item.Value);
                    if (i++ % 10000 == 0)
                        Console.Write($".");
                }
                Console.WriteLine("OK");
            }
            Console.WriteLine($"Redis database contains {m_data.Count} key-value pairs");
        }

        protected override void RunInThread(object data)
        {
            var thread_num = (int)data;
            Console.WriteLine($"Started Redis thread: {thread_num}");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(CONNECTION_CONFIG_OPTIONS);
            IDatabase db = redis.GetDatabase();
            Random rnd = new Random();
            int max_count = m_data.Count + 1;
            long min_time_mksec = long.MaxValue;
            long max_time_mksec = 0;
            long elapsed_mksec = 0;
            for (int i = 0; i < m_query_count_per_thread; i++)
            {
                int idx = rnd.Next(0, max_count);
                var item = m_data[idx];
                var watch = Stopwatch.StartNew();
                var v = db.StringGet(item.Key.ToString());
                watch.Stop();
                if (v != item.Value)
                    throw new ApplicationException($"Unexpected value {item.Value}. Key: {item.Key}");
                long time_mksec = watch.ElapsedTicks / (Stopwatch.Frequency / 1000000);
                elapsed_mksec += time_mksec;
                if (time_mksec < min_time_mksec)
                    min_time_mksec = time_mksec;
                if (time_mksec > max_time_mksec)
                    max_time_mksec = time_mksec;
            }
            m_result.ResultByThread[thread_num].ElapsedTimeMsec = elapsed_mksec / 1000;
            m_result.ResultByThread[thread_num].MinTimeMksec = min_time_mksec;
            m_result.ResultByThread[thread_num].MaxTimeMksec = max_time_mksec;
            m_result.ResultByThread[thread_num].AvgTimeMksec = elapsed_mksec / m_query_count_per_thread;
            m_result.ResultByThread[thread_num].RequestsPerSec = 1000000 / m_result.ResultByThread[thread_num].AvgTimeMksec;
        }
    }

    internal class Program
    {
        static void PrintResults(TestResult result)
        {
            long total_min_time = long.MaxValue;
            long total_max_time = 0;
            long total_avg_time = 0;
            long total_req_sec = 0;
            var r = result.ResultByThread;
            for (int i = 0; i < r.Count; i++)
            {
                Console.WriteLine(
                    $"Thread {i}:" +
                    $"\tMin time, mksec: {r[i].MinTimeMksec}" +
                    $"\tMax time, mksec: {r[i].MaxTimeMksec}" +
                    $"\tAvg time, mksec: {r[i].AvgTimeMksec}" +
                    $"\tElapsed time, msec: {r[i].ElapsedTimeMsec}" +
                    $"\tRequests per sec: {r[i].RequestsPerSec}"
                );
                total_req_sec += r[i].RequestsPerSec;
                total_avg_time += r[i].AvgTimeMksec;
                if (total_min_time > r[i].MinTimeMksec)
                    total_min_time = r[i].MinTimeMksec;
                if (total_max_time < r[i].MaxTimeMksec)
                    total_max_time = r[i].MaxTimeMksec;
            }
            Console.WriteLine($"Min time, mksec: {total_min_time}");
            Console.WriteLine($"Max time, mksec: {total_max_time}");
            Console.WriteLine($"Avg time, mksec: {total_avg_time / r.Count}");
            Console.WriteLine($"Avg requests per sec: {total_req_sec / r.Count}");

        }
        static void Main(string[] args)
        {
            // Test parameters
            int thread_count           = args.Length > 0 ? int.Parse(args[0]) : 10;
            int query_count_per_thread = args.Length > 1 ? int.Parse(args[1]) : 1000;
            bool reset_redis_data       = args.Length > 2 ? bool.Parse(args[2]) : false;
            //
            var data = new TestData();
            var tester = new SqlTester(data, thread_count, query_count_per_thread);
            tester.LoadData();
            Console.WriteLine($"Read key count: {data.Count}");
            Console.WriteLine($"Query per thread: {query_count_per_thread}");
            tester.Run();
            PrintResults(tester.Result);
            var redis_tester = new RedisTester(data, thread_count, query_count_per_thread);
            redis_tester.ResetData = reset_redis_data;
            redis_tester.InitData();
            redis_tester.Run();
            PrintResults(redis_tester.Result);
        }
    }
}
