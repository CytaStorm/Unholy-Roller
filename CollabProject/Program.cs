namespace CollabProject
{
    internal class Program
    {
 
        // Ramon Miland
        // Jeff Chen
        // Tony Qiu
        // Gunnar Dickey 

        static void Main(string[] args)
        {
            //Dice
            PromptSum();
            Console.WriteLine();

            //Last first
            lastFirst("Wedge Antilles");
            Console.WriteLine();

            //Palindrome
            isPalindrome("Racecar");
            isPalindrome("Chewy");
            Console.WriteLine();
        }

        /// <summary>
        /// Checks whether or not the entered string in lowercase
        /// is read the same forward and backward
        /// </summary>
        /// <param name="s"> the string to check </param>
        static void isPalindrome(string s)
        {
            // Make the entered string lowercase
            string lowerS = s.ToLower();

            // Reverse the order of characters
            string reversed = "";
            for (int i = lowerS.Length - 1; i >=0; i--)
            {
                reversed += lowerS.Substring(i, 1);
            }

            // Check if the string is the same forward and backward
            if (reversed == lowerS)
            {
                Console.WriteLine($"{s} is a palindrome");
                return;
            };

            Console.WriteLine($"{s} is not a palindrome");

            //first duplicate array, should output 10
            int[] numbers1 = { 2, 2, 4, 10, 10, 10, 10, 4, 2, 2, 2, 4 };
            Console.Write("The array ");
            for (int i = 0; i < numbers1.Length - 1; i++)
            {
                Console.Write(numbers1[i] + ", ");
            }
            Console.Write(numbers1[numbers1.Length - 1]);
            Console.WriteLine(" has a duplicate chain of " + GetLongestDuplicate(numbers1));

            //second duplicate array, should output 7
            int[] numbers2 = { 5, 2, 4, 4, 6, 6, 6, 7, 7, 7, 1, 2 };
            Console.Write("The array ");
            for (int i = 0; i < numbers2.Length - 1; i++)
            {
                Console.Write(numbers2[i] + ", ");
            }
            Console.Write(numbers2[numbers2.Length - 1]);
            Console.WriteLine(" has a duplicate chain of " + GetLongestDuplicate(numbers2));
        }
    }

        /// <summary>
        /// Takes input of a first and last name, and outputs
        /// the name in the format {Last}. {firstInitial}.
        /// </summary>
        /// <param name="str"></param>
        static void lastFirst(string str)
        {
            string lastFirst = "";
            int indexOfSpace = str.IndexOf(' ') + 1;
            lastFirst  = str.Substring(indexOfSpace) + ", " + str.Substring(0,1) + ".";

            Console.WriteLine(lastFirst);
        }

        /// <summary>
        /// Ask for target dice sum and continuously
        /// rolls two dice until target is reached.
        /// </summary>
        public static void PromptSum()
        {
            int targetSum = 0;
            do
            {
                //Prompt for target number.
                Console.Write("Desired dice sum: ");
                targetSum = int.Parse(Console.ReadLine());
                if (targetSum < 2 || targetSum > 12)
                {
                    Console.WriteLine("Invalid sum");
                    continue;
                }
                Random random = new Random();

                //Roll dice until targetSum is reached.
                int currentSum = 0;
                while (currentSum != targetSum)
                {
                    currentSum = 0;
                    int roll1 = random.Next(1, 7);
                    int roll2 = random.Next(1, 7);
                    Console.WriteLine($"{roll1} + {roll2} = {roll1 + roll2}");
                    currentSum += roll1;
                    currentSum += roll2;
                }

            } while (targetSum < 2 || targetSum > 12);
        }
    static int GetLongestDuplicate(int[] array)
    {
        int current = 0;
        int count = 1;
        int largestCount = 0;
        int savedI = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == current)
            {
                count++;


            }
            else
            {
                current = array[i];
                if (count >= largestCount)
                {
                    largestCount = count;
                    savedI = i - 1;

                }
                count = 1;
            }
        }

        return array[savedI];
    }
}
}