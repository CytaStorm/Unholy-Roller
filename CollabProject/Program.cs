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
            Console.WriteLine("Hello, World!");
            PromptSum();
        }

        /// <summary>
        /// Dice Sum function!
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
            Console.WriteLine();
        } 
    }
}