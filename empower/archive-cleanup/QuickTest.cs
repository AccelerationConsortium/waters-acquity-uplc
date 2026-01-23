using System;
using WatersEmpowerToolkit;

namespace WatersEmpowerQuickTest
{
    /// <summary>
    /// Quick test application for Waters Empower Toolkit
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waters Empower Toolkit - Quick Test");
            Console.WriteLine("===================================");
            Console.WriteLine();

            try
            {
                // Quick diagnostics test
                Console.WriteLine("Running quick diagnostics...");
                string diagnostics = EmpowerHelper.RunDiagnostics();
                Console.WriteLine(diagnostics);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
    }
}
