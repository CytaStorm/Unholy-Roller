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
            //first duplicate array, should output 10
            int[] numbers1 = { 2, 2, 4, 10, 10, 10, 10, 4, 2, 2, 2, 4 };
            Console.Write("The array ");
            for (int i = 0; i < numbers1.Length-1; i++)
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
        /// <summary>
        /// Gets the number of the longest duplicate chain
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static int GetLongestDuplicate(int[] array)
        {
            int current = 0;
            int count = 1;
            int largestCount = 0;
            int savedI = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == current) {
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