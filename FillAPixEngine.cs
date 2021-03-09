using JsonLibrary;

namespace FillAPixSolver
{
    class FillAPixEngine
    {
        private const char EMPTY = '.';
        private const char FILLED = '#';
        private const char NOTFILLED = ' ';

        public static void InitPuzzle(JObject puzzle)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            JArray answer = new();
            for (int y = 0; y < height; y++)
            {
                answer.Add(new string(EMPTY, width));
            }
            puzzle["answer"] = answer;
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
                        char c = puzzle.GetArray("grid")[y].ToString()[x];
                        if (c < '0' || c > '9') continue;
                        int value = c - '0';
                        GetCounts(puzzle, x, y, out int empty, out int filled, out int notFilled);
                        if (empty == 0) continue;
                        if (value == filled)
                        {
                            MarkNotFilled(puzzle, x, y);
                            changed = true;
                        }
                        else if (value == filled + empty)
                        {
                            MarkFilled(puzzle, x, y);
                            changed = true;
                        }
                    }
                }
            } while (changed);
        }

        private static void MarkNotFilled(JObject puzzle, int x, int y)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                    {
                        continue;
                    }
                    char c = puzzle.GetArray("answer")[y1].ToString()[x1];
                    if (c == EMPTY)
                    {
                        string line = puzzle.GetArray("answer")[y1].ToString();
                        line = line[..x1] + NOTFILLED + line[(x1 + 1)..];
                        puzzle.GetArray("answer")[y1] = line;
                    }
                }
            }
        }

        private static void MarkFilled(JObject puzzle, int x, int y)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                    {
                        continue;
                    }
                    char c = puzzle.GetArray("answer")[y1].ToString()[x1];
                    if (c == EMPTY)
                    {
                        string line = puzzle.GetArray("answer")[y1].ToString();
                        line = line[..x1] + FILLED + line[(x1 + 1)..];
                        puzzle.GetArray("answer")[y1] = line;
                    }
                }
            }
        }

        private static void GetCounts(JObject puzzle, int x, int y, out int empty, out int filled, out int notFilled)
        {
            int width = (int)puzzle["width"];
            int height = (int)puzzle["height"];
            empty = 0;
            filled = 0;
            notFilled = 0;
            for (int y1 = y - 1; y1 <= y + 1; y1++)
            {
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    if (y1 < 0 || y1 >= height || x1 < 0 || x1 >= width)
                    {
                        notFilled++;
                    }
                    else
                    {
                        char c = puzzle.GetArray("answer")[y1].ToString()[x1];
                        switch (c)
                        {
                            case NOTFILLED:
                                notFilled++;
                                break;
                            case FILLED:
                                filled++;
                                break;
                            default:
                                empty++;
                                break;
                        }
                    }
                }
            }
        }
    }
}
