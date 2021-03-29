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
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            // arrays must be height, width
            int[,] values = new int[height, width];
            char[,] answer = new char[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = puzzle.GetArray("grid")[y].ToString()[x];
                    if (c >= '0' && c <= '9')
                    {
                        values[y, x] = c - '0'; // use actual int value, not char value
                    }
                    else
                    {
                        values[y, x] = UNKNOWN; // use char value UNKNOWN as integer
                    }
                    answer[y, x] = UNKNOWN;
                }
            }
            puzzle["values"] = values;
            puzzle["answer"] = answer;
            puzzle["steps"] = new JArray();
        }

        public static void Solve(JObject puzzle)
        {
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            bool changed;
            do
            {
                changed = false;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int value = ((int[,])puzzle["values"])[y, x];
                        if (value == UNKNOWN) continue;
                        // check for basic logic
                        if (SolveBasic(puzzle, y, x))
                        {
                            changed = true;
                            continue;
                        }
                        // check for advanced logic
                        if (SolveAdvanced(puzzle, y, x))
                        {
                            changed = true;
                            continue;
                        }
                    }
                }
            } while (changed);
        }

        private static bool SolveBasic(JObject puzzle, int y, int x)
        {
            JObject info = GetInfo(puzzle, y, x);
            if ((int)info["unknown"] == 0)
            {
                return false;
            }
            bool changed = false;
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            if ((int)info["value"] == (int)info["filled"])
            {
                for (int y1 = y - 1; y1 <= y + 1; y1++)
                {
                    for (int x1 = x - 1; x1 <= x + 1; x1++)
                    {
                        if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                        {
                            // beyond edges of grid
                            continue;
                        }
                        if (((char[,])puzzle["answer"])[y1, x1] == UNKNOWN)
                        {
                            ((char[,])puzzle["answer"])[y1, x1] = NOTFILLED;
                            ((JArray)puzzle["steps"]).Add($"{y1},{x1},0");
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
                        if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                        {
                            // beyond edges of grid
                            continue;
                        }
                        if (((char[,])puzzle["answer"])[y1, x1] == UNKNOWN)
                        {
                            ((char[,])puzzle["answer"])[y1, x1] = FILLED;
                            ((JArray)puzzle["steps"]).Add($"{y1},{x1},1");
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        public static bool IsSolveFinished(JObject puzzle)
        {
            for (int y = 0; y < (int)puzzle["height"]; y++)
            {
                for (int x = 0; x < (int)puzzle["width"]; x++)
                {
                    if (((char[,])puzzle["answer"])[y, x] == UNKNOWN)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool SolveAdvanced(JObject puzzle, int y, int x)
        {
            JObject info1 = GetInfo(puzzle, y, x);
            int value1 = (int)info1["value"];
            if ((int)info1["unknown"] == 0 || (int)info1["filled"] == value1)
            {
                return false;
            }
            bool changed = false;
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    if (x == x1 && y == y1) continue;
                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width) continue;
                    if (((int[,])puzzle["values"])[y1, x1] == UNKNOWN)
                    {
                        continue;
                    }

                    JObject info2 = GetInfo(puzzle, y1, x1);
                    int value2 = (int)info2["value"];

                    JObject common = GetCommon(puzzle, info1, info2);
                    JObject leftOnly = GetLeftOnly(puzzle, info1, info2);
                    JObject rightOnly = GetRightOnly(puzzle, info1, info2);

                    int minCommon = (int)common["filled"];
                    minCommon = Math.Max(minCommon, value1 - (int)leftOnly["count"] + (int)leftOnly["notfilled"]);
                    minCommon = Math.Max(minCommon, value2 - (int)rightOnly["count"] + (int)rightOnly["notfilled"]);

                    int maxCommon = (int)common["count"] - (int)common["notfilled"];
                    maxCommon = Math.Min(maxCommon, value1 - (int)leftOnly["filled"]);
                    maxCommon = Math.Min(maxCommon, value2 - (int)rightOnly["filled"]);

                    if (minCommon == maxCommon)
                    {
                        if ((int)common["unknown"] > 0)
                        {
                            if ((int)common["unknown"] + (int)common["filled"] == minCommon)
                            {
                                MarkFilled(puzzle, common);
                                changed = true;
                            }
                            else if ((int)common["count"] - (int)common["unknown"] - (int)common["notfilled"] == maxCommon)
                            {
                                MarkNotFilled(puzzle, common);
                                changed = true;
                            }
                        }
                        if ((int)leftOnly["unknown"] > 0)
                        {
                            if (value1 - minCommon == (int)leftOnly["unknown"] + (int)leftOnly["filled"])
                            {
                                MarkFilled(puzzle, leftOnly);
                                changed = true;
                            }
                            else if (value1 - minCommon == (int)leftOnly["count"] - (int)leftOnly["unknown"] - (int)leftOnly["notfilled"])
                            {
                                MarkNotFilled(puzzle, leftOnly);
                                changed = true;
                            }
                        }
                        if ((int)rightOnly["unknown"] > 0)
                        {
                            if (value2 - minCommon == (int)rightOnly["unknown"] + (int)rightOnly["filled"])
                            {
                                MarkFilled(puzzle, rightOnly);
                                changed = true;
                            }
                            else if (value2 - minCommon == (int)rightOnly["count"] - (int)rightOnly["unknown"] - (int)rightOnly["notfilled"])
                            {
                                MarkNotFilled(puzzle, rightOnly);
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                    {
                        // exit early, as info1 and/or info2 have changed
                        return true;
                    }
                }
            }
            return false;
        }

        private static void MarkNotFilled(JObject puzzle, JObject info)
        {
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            foreach (JArray cell in (JArray)info["cells"])
            {
                int y = (int)cell[0];
                int x = (int)cell[1];
                if (y < 0 || y >= height || x < 0 || x >= width) continue;
                if (((char[,])puzzle["answer"])[y, x] == UNKNOWN)
                {
                    ((char[,])puzzle["answer"])[y, x] = NOTFILLED;
                    ((JArray)puzzle["steps"]).Add($"{y},{x},0");
                }
            }
        }

        private static void MarkFilled(JObject puzzle, JObject info)
        {
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            foreach (JArray cell in (JArray)info["cells"])
            {
                int y = (int)cell[0];
                int x = (int)cell[1];
                if (y < 0 || y >= height || x < 0 || x >= width) continue;
                if (((char[,])puzzle["answer"])[y, x] == UNKNOWN)
                {
                    ((char[,])puzzle["answer"])[y, x] = FILLED;
                    ((JArray)puzzle["steps"]).Add($"{y},{x},1");
                }
            }
        }

        private static JObject GetInfo(JObject puzzle, int y, int x)
        {
            JObject result = new();
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            result["value"] = ((int[,])puzzle["values"])[y, x];
            result["unknown"] = 0;
            result["notfilled"] = 0;
            result["filled"] = 0;
            result["cells"] = new JArray();
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    ((JArray)result["cells"]).Add(new JArray { y1, x1 });
                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                    {
                        // beyond edges of grid
                        result["notfilled"] = (int)result["notfilled"] + 1;
                    }
                    else
                    {
                        char neighborValue = ((char[,])puzzle["answer"])[y1, x1];
                        switch (neighborValue)
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
                            default:
                                throw new SystemException("Logic error!");
                        }
                    }
                }
            }
            return result;
        }

        private static JObject GetCommon(JObject puzzle, JObject info1, JObject info2)
        {
            JObject result = new();
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            result["unknown"] = 0;
            result["notfilled"] = 0;
            result["filled"] = 0;
            result["cells"] = new JArray();
            foreach (JArray cellLeft in (JArray)info1["cells"])
            {
                int y = (int)cellLeft[0];
                int x = (int)cellLeft[1];
                foreach (JArray cellRight in (JArray)info2["cells"])
                {
                    if (y == (int)cellRight[0] && x == (int)cellRight[1])
                    {
                        ((JArray)result["cells"]).Add(new JArray { y, x });
                        if (y < 0 || y >= height || x < 0 || x >= width)
                        {
                            // beyond edges of grid
                            result["notfilled"] = (int)result["notfilled"] + 1;
                        }
                        else
                        {
                            char neighborValue = ((char[,])puzzle["answer"])[y, x];
                            switch (neighborValue)
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
                                default:
                                    throw new SystemException("Logic error!");
                            }
                        }
                    }
                }
            }
            result["count"] = ((JArray)result["cells"]).Count();
            return result;
        }

        private static JObject GetLeftOnly(JObject puzzle, JObject info1, JObject info2)
        {
            JObject result = new();
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            result["unknown"] = 0;
            result["notfilled"] = 0;
            result["filled"] = 0;
            result["cells"] = new JArray();
            foreach (JArray cellLeft in (JArray)info1["cells"])
            {
                int y = (int)cellLeft[0];
                int x = (int)cellLeft[1];
                bool found = false;
                foreach (JArray cellRight in (JArray)info2["cells"])
                {
                    if (y == (int)cellRight[0] && x == (int)cellRight[1])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ((JArray)result["cells"]).Add(new JArray { y, x });
                    if (y < 0 || y >= height || x < 0 || x >= width)
                    {
                        // beyond edges of grid
                        result["notfilled"] = (int)result["notfilled"] + 1;
                    }
                    else
                    {
                        char neighborValue = ((char[,])puzzle["answer"])[y, x];
                        switch (neighborValue)
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
                            default:
                                throw new SystemException("Logic error!");
                        }
                    }
                }
            }
            result["count"] = ((JArray)result["cells"]).Count();
            return result;
        }

        private static JObject GetRightOnly(JObject puzzle, JObject info1, JObject info2)
        {
            JObject result = new();
            int height = (int)puzzle["height"];
            int width = (int)puzzle["width"];
            result["unknown"] = 0;
            result["notfilled"] = 0;
            result["filled"] = 0;
            result["cells"] = new JArray();
            foreach (JArray cellRight in (JArray)info2["cells"])
            {
                int y = (int)cellRight[0];
                int x = (int)cellRight[1];
                bool found = false;
                foreach (JArray cellLeft in (JArray)info1["cells"])
                {
                    if (y == (int)cellLeft[0] && x == (int)cellLeft[1])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ((JArray)result["cells"]).Add(new JArray { y, x });
                    if (y < 0 || y >= height || x < 0 || x >= width)
                    {
                        // beyond edges of grid
                        result["notfilled"] = (int)result["notfilled"] + 1;
                    }
                    else
                    {
                        char neighborValue = ((char[,])puzzle["answer"])[y, x];
                        switch (neighborValue)
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
                            default:
                                throw new SystemException("Logic error!");
                        }
                    }
                }
            }
            result["count"] = ((JArray)result["cells"]).Count();
            return result;
        }
    }
}
