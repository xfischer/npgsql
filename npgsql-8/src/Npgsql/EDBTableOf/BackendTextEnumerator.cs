using System.Collections.Generic;
using System.Text;

namespace EnterpriseDB.EDBClient
{
    internal static class BackendTextEnumerator
    {

        public static IEnumerable<string> EnumerateTokens(string BackendData)
        {
            /* Examples :
             * Points: "{\"(0,0)\",\"(-4.2,43.5)\"}"
             * Table of real,real: "{\"(-5.2,43.5)\",\"(-5.2,43.5)\"}"
             * Array of domain types :  {7369,7499,7521}
             * Array of tuples with space-escaped ones : {\"(ACCOUNTING,\\\"NEW YORK\\\")\",\"(OPERATIONS,BOSTON)\",\"(RESEARCH,DALLAS)\",\"(SALES,CHICAGO)\"}
             * Nested composite : {""(\"(7369,SMITH)\",\"(ACCOUNTING,\"\"NEW YORK\"\")\")","(\"(7369,SMITH)\",\"(OPERATIONS,BOSTON)\")","(\"(7369,SMITH)\",\"(RESEARCH,D
            */

            bool firstBraceFound = false;
            bool inTuple = false;
            bool hasTuples = false;
            Stack<char> escapedChars = new Stack<char>();
            StringBuilder sb = new StringBuilder(BackendData.Length / 2);
            int idx = 0;
            while (idx < BackendData.Length)
            {
                char c = BackendData[idx];
                switch (c)
                {
                case '{':
                    if (firstBraceFound)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        firstBraceFound = true;
                    }
                    break;

                case '\\':
                    char next = BackendData[idx + 1];
                    if (escapedChars.Count > 0 && escapedChars.Peek() == next)
                    {
                        escapedChars.Pop();
                        idx += 1;
                    }
                    else
                    {
                        escapedChars.Push(next);
                        idx += 1;
                    }
                    break;

                case ',':

                    if (!hasTuples || sb.Length > 0)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    break;

                case '}':
                    if (idx != BackendData.Length - 1) // EOF
                    {
                        sb.Append(c);
                    }
                    break;
                case '"':
                    if (escapedChars.Count > 0 && escapedChars.Peek() == c)
                    {
                        escapedChars.Pop();
                    }
                    else
                    {
                        escapedChars.Push(c);
                    }
                    break;
                case '(':
                    if (!inTuple)
                    {
                        inTuple = true;
                        hasTuples = true;
                        escapedChars.Push('(');
                    }
                    else
                    {
                        if (escapedChars.Count > 0 && escapedChars.Peek() == '"') // escaped (
                        {
                            escapedChars.Push(c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    break;
                case ')':
                    if (escapedChars.Count > 0 && escapedChars.Peek() == '(')
                    {
                        escapedChars.Pop();
                        inTuple = false;
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                            sb.Clear();
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
                default:
                    sb.Append(c);
                    break;
                }
                idx++;
            }

            if (sb.Length == 0)
            {
                yield break;
            }
            else
            {
                yield return sb.ToString();
            }
        }
    }
}
