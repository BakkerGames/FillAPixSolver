using JsonLibrary;
using System;
using System.IO;

namespace FillAPixSolver
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Syntax: <filepath.json>");
                return 1;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File not found: {args[0]}");
                return 2;
            }
            string puzzleText = File.ReadAllText(args[0]);
            JObject puzzle = JObject.Parse(puzzleText);
            FillAPixEngine.InitPuzzle(puzzle);
            FillAPixEngine.Solve(puzzle);
            for (int y = 0; y < (int)puzzle["height"]; y++)
            {
                for (int x = 0; x < (int)puzzle["width"]; x++)
                {
                    Console.Write(((char[,])puzzle["answer"])[x, y]);
                }
                Console.WriteLine();
            }
            //            if (!FillAPixEngine.IsSolveFinished(puzzle))
            //            {
            //                Console.WriteLine("Could not solve puzzle!");
            //            }
            //            else
            //            {
            //                Console.WriteLine(puzzle["clue"]);
            //            }
            return 0;
        }
    }
}
