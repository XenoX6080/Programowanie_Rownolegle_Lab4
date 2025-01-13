using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScottPlot;

namespace Lab4
{
    internal class Program
    {
        private static string fileName = null;
        private static readonly ManualResetEvent fileReadyEvent = new ManualResetEvent(false);


        public static double GetFunction(double x)
        {
            return Math.Sin(x) + Math.Log(Math.Abs(x) + 0.1) + Math.Tan(x);
        }

        public static double CalculateIntegral(double start, double end, int steps)
        {
            double stepSize = (end - start) / steps;
            double result = 0;

            OrderablePartitioner<Tuple<int, int>> orderablePartitioner = Partitioner.Create(0, steps);

            Parallel.ForEach(orderablePartitioner, (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double x = start + i * stepSize;
                    double y = GetFunction(x);
                    result += y * stepSize;
                    Thread.Sleep(1);
                }
            });

            return result;
        }

        public static void SaveAndDisplayGraph()
        {
            double[] dataX = Enumerable.Range(1, 100).Select(i => (double)i / 10).ToArray();
            double[] dataY = dataX.Select(x => GetFunction(x)).ToArray();

            var plt = new Plot();
            plt.Add.Scatter(dataX, dataY);

            fileReadyEvent.WaitOne();

            bool success = false;
            while (!success)
            {

                try
                {
                    plt.SavePng(fileName, 400, 300);
                    success = true;

                    var p = new System.Diagnostics.Process();
                    p.StartInfo = new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true };
                    p.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd zapisywania pliku: {ex.Message}");
                    Console.Write("Wprowadź ponownie nazwę pliku żeby zapisać wykres: : ");
                    fileName = Console.ReadLine() + ".png";
                    fileReadyEvent.Set();
                }
            }
        }

        public static void GetFileNameFromUser()
        {
            Console.Write("Wprowadź nazwę pliku żeby zapisać wykres: ");
            fileName = Console.ReadLine() + ".png";
            fileReadyEvent.Set();

        }

        static void Main(string[] args)
        {
            double start = 0;
            double end = 10;
            int steps = 10000;

            Console.WriteLine($"Początek przedziału: {start}\nKoniec przedziału: {end}\nIlość kroków: {steps}\n");

            Thread inputThread = new Thread(GetFileNameFromUser);
            inputThread.Start();

            double result = CalculateIntegral(start, end, steps);
            
            Console.WriteLine($"\nCałka funkcji y = sin(x)+log(∣x∣+0.1)+tan(x) wynosi {result}");

            SaveAndDisplayGraph();
        }
    }
}
