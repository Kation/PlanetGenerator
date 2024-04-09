// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;

class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            //.AddJob(Job.Default
            //    .WithPlatform(BenchmarkDotNet.Environments.Platform.X64)
            //    .WithJit(Jit.RyuJit)
            //    .WithRuntime(CoreRuntime.Core31)
            //    .WithWarmupCount(1)
            //    .WithLaunchCount(1)
            //    .WithIterationCount(20))
            //.AddJob(Job.Default
            //    .WithPlatform(BenchmarkDotNet.Environments.Platform.X64)
            //    .WithJit(Jit.RyuJit)
            //    .WithRuntime(CoreRuntime.Core50)
            //    .WithWarmupCount(1)
            //    .WithLaunchCount(1)
            //    .WithIterationCount(20))
            .AddJob(Job.Default
                .WithPlatform(BenchmarkDotNet.Environments.Platform.X64)
                .WithJit(Jit.Default)
                .WithRuntime(CoreRuntime.Core80));

        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, config);
        Console.Read();
    }

}