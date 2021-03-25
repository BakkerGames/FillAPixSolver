using JsonLibrary;
using System;

namespace FillAPixSolver
{
    public class FillAPixEngine
    {
        private const char UNKNOWN = '.';
        private const char FILLED = '#';
        private const char NOTFILLED = ' ';

        public static void InitPuzzle(JObject puzzle)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            int[,] values = new int[width, height];
            char[,] answer = new char[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = puzzle.GetArray("grid")[y].ToString()[x];
                    if (c >= '0' && c <= '9')
                    {
                        values[x, y] = c - '0'; // use actual int value, not char value
                    }
                    else
                    {
                        values[x, y] = UNKNOWN; // use char value UNKNOWN as integer
                    }
                    answer[x, y] = UNKNOWN;
                }
            }
            puzzle["values"] = values;
            puzzle["answer"] = answer;
            puzzle["steps"] = new JArray();
        }

        public static void Solve(JObject puzzle)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            bool changed;
            do
            {
                changed = false;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int value = ((int[,])puzzle["values"])[x, y];
                        if (value == UNKNOWN) continue;
                        // check for basic logic
                        if (SolveBasic(puzzle, x, y))
                        {
                            changed = true;
                        }

                        //                        GetCounts(puzzle, x, y, out int unknown, out int filled, out int notFilled);
                        //                        if (unknown == 0) continue;
                        //                        if (value == filled)
                        //                        {
                        //                            MarkNotFilled(puzzle, x, y);
                        //                            changed = true;
                        //                            continue;
                        //                        }
                        //                        if (value == 9 - notFilled)
                        //                        {
                        //                            MarkFilled(puzzle, x, y);
                        //                            changed = true;
                        //                            continue;
                        //                        }
                        //                        // check for advanced logic
                        //                        if (SolveAdvanced(puzzle, x, y))
                        //                        {
                        //                            changed = true;
                        //                        }
                    }
                }
            } while (changed);
        }

        private static bool SolveBasic(JObject puzzle, int x, int y)
        {
            bool changed = false;
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            JObject info = GetInfo(puzzle, x, y);
            if ((int)info["unknown"] > 0)
            {
                if ((int)info["value"] == (int)info["filled"])
                {
                    for (int y1 = y - 1; y1 <= y + 1; y1++)
                    {
                        for (int x1 = x - 1; x1 <= x + 1; x1++)
                        {
                            if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height)
                            {
                                // beyond edges of grid
                                continue;
                            }
                            if (((char[,])puzzle["answer"])[x1, y1] == UNKNOWN)
                            {
                                ((char[,])puzzle["answer"])[x1, y1] = NOTFILLED;
                                //todo add step
                                changed = true;
                            }
                        }
                    }
                }
                else if (9 - (int)info["value"] == (int)info["notfilled"])
                {
                    for (int y1 = y - 1; y1 <= y + 1; y1++)
                    {
                        for (int x1 = x - 1; x1 <= x + 1; x1++)
                        {
                            if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height)
                            {
                                // beyond edges of grid
                                continue;
                            }
                            if (((char[,])puzzle["answer"])[x1, y1] == UNKNOWN)
                            {
                                ((char[,])puzzle["answer"])[x1, y1] = FILLED;
                                //todo add step
                                changed = true;
                            }
                        }
                    }
                }
            }
            return changed;
        }

        private static JObject GetInfo(JObject puzzle, int x, int y)
        {
            JObject result = new();
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            result["value"] = ((int[,])puzzle["values"])[x, y];
            result["unknown"] = 0;
            result["notfilled"] = 0;
            result["filled"] = 0;
            char[,] neighbors = new char[3, 3];
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height)
                    {
                        // beyond edges of grid
                        neighbors[x1 - x + 1, y1 - y + 1] = NOTFILLED;
                        result["notfilled"] = (int)result["notfilled"] + 1;
                    }
                    else
                    {
                        char cell = ((char[,])puzzle["answer"])[x1, y1];
                        neighbors[x1 - x + 1, y1 - y + 1] = cell;
                        switch (cell)
                        {
                            case UNKNOWN:
                                result["unknown"] = (int)result["unknown"] + 1;
                                break;
                            case NOTFILLED:
                                result["notfilled"] = (int)result["notfilled"] + 1;
                                break;
                            case FILLED:
                                result["filled"] = (int)result["filled"] + 1;
                                break;
                        }
                    }
                }
            }
            result["neighbors"] = neighbors;
            return result;
        }


        //public static bool IsSolveFinished(JObject puzzle)
        //{
        //            int height = (int)puzzle["height"];
        //            for (int y = 0; y < height; y++)
        //            {
        //###  string line = puzzle.GetArray("answer")[y].ToString();
        //                if (line.Contains(UNKNOWN))
        //                {
        //                    return false;
        //                }
        //            }
        //return true;
        //}

        #region private routines

        //private static bool SolveAdvanced(JObject puzzle, int x, int y)
        //{
        //            int width = (int)puzzle["width"];
        //            int height = (int)puzzle["height"];
        //            bool changed = false;
        //### char c1 = puzzle.GetArray("grid")[y].ToString()[x];
        //            int value1 = c1 - '0';
        //            JArray cells1 = GetCells(puzzle, x, y);
        //            for (int y1 = y - 1; y1 <= y + 1; y1++)
        //            {
        //                for (int x1 = x - 1; x1 <= x + 1; x1++)
        //                {
        //                    if (x == x1 && y == y1) continue;
        //                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width) continue;
        //### char c2 = puzzle.GetArray("grid")[y1].ToString()[x1];
        //                    if (c2 < '0' || c2 > '9') continue;
        //                    int value2 = c2 - '0';
        //                    JArray cells2 = GetCells(puzzle, x1, y1);
        //
        //                    JArray common = GetCommon(cells1, cells2);
        //                    JArray leftOnly = GetLeftOnly(cells1, cells2);
        //                    JArray rightOnly = GetRightOnly(cells1, cells2);
        //
        //                    int minCommon = CountFilled(common);
        //                    minCommon = Math.Max(minCommon, value1 - (leftOnly.Count() - CountNotFilled(leftOnly)));
        //                    minCommon = Math.Max(minCommon, value2 - (rightOnly.Count() - CountNotFilled(rightOnly)));
        //
        //                    int maxCommon = common.Count() - CountNotFilled(common);
        //                    maxCommon = Math.Min(maxCommon, value1 - (leftOnly.Count() - CountFilled(leftOnly)));
        //                    maxCommon = Math.Min(maxCommon, value2 - (rightOnly.Count() - CountFilled(rightOnly)));
        //
        //                    if (CountUnknown(common) > 0 && minCommon == CountFilled(common) + CountUnknown(common))
        //                    {
        //                        MarkCellsFilled(puzzle, common);
        //                        changed = true;
        //                        continue;
        //                    }
        //                    if (CountUnknown(common) > 0 && maxCommon == CountNotFilled(common) + CountUnknown(common))
        //                    {
        //                        MarkCellsNotFilled(puzzle, common);
        //                        changed = true;
        //                        continue;
        //                    }
        //
        //
        //                    if (CountUnknown(leftOnly) > 0 && value1 -
        //                    {
        //                        MarkCellsFilled(puzzle, leftOnly);
        //                        changed = true;
        //                    }
        //                    //                    if (value2 > 0 && CountUnknown(rightOnly) > 0 && value2 == maxCommon + CountFilled(rightOnly) + CountUnknown(rightOnly))
        //                    //                    {
        //                    //                        MarkCellsFilled(puzzle, rightOnly);
        //                    //                        changed = true;
        //                    //                    }
        //                }
        //            }
        //return changed;
        //}

        //private static void MarkNotFilled(JObject puzzle, int x, int y)
        //{
        //            int width = (int)puzzle["width"];
        //            int height = (int)puzzle["height"];
        //            for (int y1 = y - 1; y1 <= y + 1; y1++)
        //            {
        //                for (int x1 = x - 1; x1 <= x + 1; x1++)
        //                {
        //                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
        //                    {
        //                        continue;
        //                    }
        //### char c = puzzle.GetArray("answer")[y1].ToString()[x1];
        //                    if (c == UNKNOWN)
        //                    {
        //### string line = puzzle.GetArray("answer")[y1].ToString();
        //                        line = line[..x1] + NOTFILLED + line[(x1 + 1)..];
        //### puzzle.GetArray("answer")[y1] = line;
        //                        puzzle.GetArray("steps").Add($"{x1},{y1},0");
        //                    }
        //                }
        //            }
        //}

        //private static void MarkFilled(JObject puzzle, int x, int y)
        //{
        //            int width = (int)puzzle["width"];
        //            int height = (int)puzzle["height"];
        //            for (int y1 = y - 1; y1 <= y + 1; y1++)
        //            {
        //                for (int x1 = x - 1; x1 <= x + 1; x1++)
        //                {
        //                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
        //                    {
        //                        continue;
        //                    }
        //### char c = puzzle.GetArray("answer")[y1].ToString()[x1];
        //                    if (c == UNKNOWN)
        //                    {
        //### string line = puzzle.GetArray("answer")[y1].ToString();
        //                        line = line[..x1] + FILLED + line[(x1 + 1)..];
        //### puzzle.GetArray("answer")[y1] = line;
        //                        puzzle.GetArray("steps").Add($"{x1},{y1},1");
        //                    }
        //                }
        //            }
        //}

        //private static void GetCounts(JObject puzzle, int x, int y, out int unknown, out int filled, out int notFilled)
        //{
        //            int width = (int)puzzle["width"];
        //            int height = (int)puzzle["height"];
        //            unknown = 0;
        //            filled = 0;
        //            notFilled = 0;
        //            for (int y1 = y - 1; y1 <= y + 1; y1++)
        //            {
        //                for (int x1 = x - 1; x1 <= x + 1; x1++)
        //                {
        //                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
        //                    {
        //                        notFilled++;
        //                    }
        //                    else
        //                    {
        //### char c = puzzle.GetArray("answer")[y1].ToString()[x1];
        //                        switch (c)
        //                        {
        //                            case NOTFILLED:
        //                                notFilled++;
        //                                break;
        //                            case FILLED:
        //                                filled++;
        //                                break;
        //                            default:
        //                                unknown++;
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //}

        //private static JArray GetCells(JObject puzzle, int x, int y)
        //{
        //            int width = (int)puzzle["width"];
        //            int height = (int)puzzle["height"];
        //            JArray result = new();
        //            for (int y1 = y - 1; y1 <= y + 1; y1++)
        //            {
        //                for (int x1 = x - 1; x1 <= x + 1; x1++)
        //                {
        //                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
        //                    {
        //                        result.Add(new JArray { x1, y1, NOTFILLED });
        //                    }
        //                    else
        //                    {
        //### char c = puzzle.GetArray("answer")[y1].ToString()[x1];
        //                        result.Add(new JArray { x1, y1, c });
        //                    }
        //
        //                }
        //            }
        //            return result;
        //}

        //private static JArray GetCommon(JArray cells1, JArray cells2)
        //{
        //            JArray result = new();
        //            foreach (JArray cell1 in cells1)
        //            {
        //                foreach (JArray cell2 in cells2)
        //                {
        //                    if (((int)cell1[0] == (int)cell2[0]) && ((int)cell1[1] == (int)cell2[1]))
        //                    {
        //                        result.Add(cell1);
        //                        break;
        //                    }
        //                }
        //            }
        //            return result;
        //}

        //private static JArray GetLeftOnly(JArray cells1, JArray cells2)
        //{
        //            JArray result = new();
        //            foreach (JArray cell1 in cells1)
        //            {
        //                bool found = false;
        //                foreach (JArray cell2 in cells2)
        //                {
        //                    if (((int)cell1[0] == (int)cell2[0]) && ((int)cell1[1] == (int)cell2[1]))
        //                    {
        //                        found = true;
        //                        break;
        //                    }
        //                }
        //                if (!found)
        //                {
        //                    result.Add(cell1);
        //                }
        //            }
        //            return result;
        //}

        //private static JArray GetRightOnly(JArray cells1, JArray cells2)
        //{
        //            JArray result = new();
        //            foreach (JArray cell2 in cells2)
        //            {
        //                bool found = false;
        //                foreach (JArray cell1 in cells1)
        //                {
        //                    if (((int)cell1[0] == (int)cell2[0]) && ((int)cell1[1] == (int)cell2[1]))
        //                    {
        //                        found = true;
        //                        break;
        //                    }
        //                }
        //                if (!found)
        //                {
        //                    result.Add(cell2);
        //                }
        //            }
        //            return result;
        //}

        //private static int CountFilled(JArray cells)
        //{
        //            int result = 0;
        //            foreach (JArray cell in cells)
        //            {
        //                if ((char)cell[2] == FILLED)
        //                    result++;
        //            }
        //            return result;
        //}

        //private static int CountNotFilled(JArray cells)
        //{
        //            int result = 0;
        //            foreach (JArray cell in cells)
        //            {
        //                if ((char)cell[2] == NOTFILLED)
        //                    result++;
        //            }
        //            return result;
        //}

        //private static int CountUnknown(JArray cells)
        //{
        //            int result = 0;
        //            foreach (JArray cell in cells)
        //            {
        //                if ((char)cell[2] == UNKNOWN)
        //                    result++;
        //            }
        //            return result;
        //}

        private static void MarkCellsFilled(JObject puzzle, JArray cells)
        {
            //            foreach (JArray cell in cells)
            //            {
            //                if ((char)cell[2] == UNKNOWN)
            //                {
            //                    int x1 = (int)cell[0];
            //                    int y1 = (int)cell[1];
            //### string line = puzzle.GetArray("answer")[y1].ToString();
            //                    line = line[..x1] + FILLED + line[(x1 + 1)..];
            //### puzzle.GetArray("answer")[y1] = line;
            //                    puzzle.GetArray("steps").Add($"{x1},{y1},1");
            //                    cell[2] = FILLED;
            //                }
            //            }
        }

        private static void MarkCellsNotFilled(JObject puzzle, JArray cells)
        {
            //            foreach (JArray cell in cells)
            //            {
            //                if ((char)cell[2] == UNKNOWN)
            //                {
            //                    int x1 = (int)cell[0];
            //                    int y1 = (int)cell[1];
            //### string line = puzzle.GetArray("answer")[y1].ToString();
            //                    line = line[..x1] + NOTFILLED + line[(x1 + 1)..];
            //### puzzle.GetArray("answer")[y1] = line;
            //                    puzzle.GetArray("steps").Add($"{x1},{y1},0");
            //                    cell[2] = NOTFILLED;
            //                }
            //            }
        }

        #endregion
    }
}
