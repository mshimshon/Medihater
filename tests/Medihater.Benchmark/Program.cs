using BenchmarkDotNet.Running;

namespace Medihater.Benchmark;
public class Program
{
    public Program()
    {
    }

    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<BenchmarkGeneric>();
        BenchmarkRunner.Run<BenchmarkEager>();
        BenchmarkRunner.Run<BenchmarkLazy>();
    }
}
