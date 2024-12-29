using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using NBody;

//using BenchmarkDotNet.Toolchains.InProcess.Emit;
//using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using NBody.Extensions;
using CustomVector = NBody.old.Vector;
using NumericsVector = System.Runtime.Intrinsics.Vector256<double>;

int N = 4;
for (int i = 0; i < N; i++) {
  for (int j = 0; j < N; j++) {
    if (i == j)
      Console.ForegroundColor = ConsoleColor.Red;
    else
      Console.ForegroundColor = ConsoleColor.DarkGreen;

    Console.WriteLine($"i: {i}, j: {j}, i^j: {i ^ j}");
  }
  Console.WriteLine();
}

//var config = DefaultConfig.Instance
//    .AddJob(Job
//         .MediumRun
//         .WithLaunchCount(1)
//         .WithToolchain(InProcessNoEmitToolchain.Instance));

//BenchmarkRunner.Run<ChildBenchmark>();
//BenchmarkRunner.Run<VectorBenchmark>();

public class ChildBenchmark
{
  NumericsVector Center = NumericsVector.Zero;
  double size = 10.0;
  NumericsVector[] Positions = [
    Vector256.Create(1.0, 1.0, 1.0, 0.0),
    Vector256.Create(1.0, 1.0, -1.0, 0.0),
    Vector256.Create(1.0, -1.0, 1.0, 0.0),
    Vector256.Create(1.0, -1.0, -1.0, 0.0),
    Vector256.Create(-1.0, 1.0, 1.0, 0.0),
    Vector256.Create(-1.0, 1.0, 1.0, 0.0),
    Vector256.Create(-1.0, -1.0, 1.0, 0.0),
    Vector256.Create(-1.0, -1.0, -1.0, 0.0),
  ];

  [Benchmark]
  public (int index, NumericsVector position) UseIntrinsics()
  {
    int index = 0;
    NumericsVector position = NumericsVector.Zero;
    for (int i = 0; i < 8; i++) {
      var comparison = Vector256.GreaterThanOrEqual(Center, Positions[i]);
      index = OcTreeCellExtensions.ChildIndex(comparison.AsInt64());
      position = Center.GetChildCenter(comparison, size);
    }
    return (index, position);
  }

}

public class VectorBenchmark
{
  const int OperationsPerInvoke = 4096;
  public CustomVector leftC = new CustomVector(1.0, 2.0, 3.0);
  public CustomVector rightC = new CustomVector(4.0, 5.0, 6.0);
  public NumericsVector leftN = Vector256.Create(1.0, 2.0, 3.0, 4.0);
  public NumericsVector rightN = Vector256.Create(5.0, 6.0, 7.0, 8.0);
  public CustomVector[] CustomVectorResult = new CustomVector[OperationsPerInvoke];
  public NumericsVector[] NumericsVectorResult = new NumericsVector[OperationsPerInvoke];
  public double[] DoubleResult = new double[OperationsPerInvoke];

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void CustomVectorMultiply()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    CustomVectorResult[i] = leftC * rightC;
  //  }
  //}
  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVectorMultiply()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    NumericsVectorResult[i] = leftN * rightN;
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void CustomVectorAdd()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    CustomVectorResult[i] = leftC + rightC;
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVectorAdd()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    NumericsVectorResult[i] = leftN + rightN;
  //  }
  //}

  [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  public void CustomVectorCross()
  {
    for (int i = 0; i < OperationsPerInvoke; i++) {
      CustomVectorResult[i] = CustomVector.Cross(leftC, rightC);
    }
  }

  [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  public void NumericsVectorCross()
  {
    for (int i = 0; i < OperationsPerInvoke; i++) {
      NumericsVectorResult[i] = leftN.Cross(rightN);
    }
  }

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void CustomVector256<double>ot()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    DoubleResult[i] = CustomVector.Dot(leftC, rightC);
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVector256<double>ot()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    DoubleResult[i] = leftN.Dot(rightN);
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVector256<double>ot2()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    DoubleResult[i] = leftN.Dot2(rightN);
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVector256<double>ot3()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    DoubleResult[i] = leftN.Dot3(rightN);
  //  }
  //}

  //[Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
  //public void NumericsVector256<double>otAlternative()
  //{
  //  for (int i = 0; i < OperationsPerInvoke; i++) {
  //    DoubleResult[i] = leftN.Dot2(rightN);
  //  }
  //}
}