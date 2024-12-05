using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Dolly.Benchmarks;

new CloneBenchmarks().TestCloneExtensions();

BenchmarkRunner.Run<CloneBenchmarks>(ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)); //FastDeepCloner is not optimized