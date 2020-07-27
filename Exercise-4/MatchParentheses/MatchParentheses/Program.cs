using System;
using System.Collections.Generic;
using System.Linq;

namespace MatchParentheses
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Validate Parentheses");
            string strInput = Console.ReadLine();

            if (IsParenthesesMatched(strInput))
                Console.WriteLine("Matched Parentheses");
            else
                Console.WriteLine("MisMatched Parentheses");

        }

        public static bool IsParenthesesMatched(string sExpression)
        {
            Dictionary<char, char> bracketList = new Dictionary<char, char>() {
                { '(', ')' },
                { '{', '}' },
                { '[', ']' },
                { '<', '>' }
            };

            Stack<char> brackets = new Stack<char>();

            try
            {
                // run through each character in the input string
                foreach (char c in sExpression)
                {
                    // check if a 'opening' bracket from bracketList
                    if (bracketList.Keys.Contains(c))
                    {
                        // push to stack
                        brackets.Push(c);
                    }
                    else
                        // check if a 'closing' brackets from bracketList
                        if (bracketList.Values.Contains(c))
                    {
                        // check if the closing bracket matches the 'latest' 'opening' bracket
                        if (c == bracketList[brackets.First()])
                        {
                            brackets.Pop();
                        }
                        else
                            //its an unbalanced string
                            return false;
                    }
                    else
                        continue;
                }
            }
            catch
            {
                // if an exception. Return false
                return false;
            }

            // Ensure all brackets are closed
            return brackets.Count() == 0 ? true : false;
        }
    }
}
