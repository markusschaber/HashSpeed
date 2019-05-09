
namespace HashSpeed
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;

    [Config(typeof(Config))]
    public class HashBenchmark
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(DefaultConfig.Instance);
                Add(new ThroughputColumn());
            }
        }

        private HashAlgorithm sha256 = new SHA256CryptoServiceProvider();
        private HashAlgorithm sha384 = new SHA384CryptoServiceProvider();
        private HashAlgorithm sha512 = new SHA512CryptoServiceProvider();
        private HashAlgorithm mha256 = new SHA256Managed();
        private HashAlgorithm mha384 = new SHA384Managed();
        private HashAlgorithm mha512= new SHA512Managed();
        private HashAlgorithm md5 = new MD5CryptoServiceProvider();

        private byte[] data;

        [Params(16, 256, 1024, 8192, 1000000)]
        public int DataSize;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[DataSize];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark(Baseline = true)]
        public byte[] Sha384() => sha384.ComputeHash(data);

        [Benchmark]
        public byte[] Sha512() => sha512.ComputeHash(data);

        [Benchmark]
        public byte[] Managed256() => mha256.ComputeHash(data);

        [Benchmark]
        public byte[] Managed384() => mha384.ComputeHash(data);

        [Benchmark]
        public byte[] Managed512() => mha512.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);

    }

    public class ThroughputColumn : AbstractColumn
    {
        public override string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            int dataSize = (int)benchmarkCase.Parameters[nameof(HashBenchmark.DataSize)];

            double kilobytes = dataSize / 1024.0;

            var report = summary.Reports.Single(r => r.BenchmarkCase == benchmarkCase);

            var nanos = report.ResultStatistics.Mean ;

            var throughput = kilobytes * 1000000000 / nanos;

            return FormattableString.Invariant($"{throughput:N2} kb/s");
        }
    }

    public class NanoColumn : AbstractColumn
    { // just a test class...
        public override string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var report = summary.Reports.Single(r => r.BenchmarkCase == benchmarkCase);

            var nanos = report.ResultStatistics.Mean;

            return nanos.ToString(CultureInfo.InvariantCulture);
        }
    }


    public abstract class AbstractColumn : IColumn
    {
        public abstract string GetValue(Summary summary, BenchmarkCase benchmarkCase);
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
                => GetValue(summary, benchmarkCase);

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;

        public string Id => GetType().AssemblyQualifiedName;

        public string ColumnName => GetType().Name;

        public bool AlwaysShow => true;

        public ColumnCategory Category => ColumnCategory.Custom;

        public int PriorityInCategory => 0;

        public bool IsNumeric => true;

        public UnitType UnitType => UnitType.Size;

        public string Legend => "Throughput in kilobytes (kibibytes) per second";
    }
}
