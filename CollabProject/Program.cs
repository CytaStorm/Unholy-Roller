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
            int[] numbers = { 5, 2, 4, 4, 6, 6, 6, 7, 7, 7, 1, 2 };
            Console.WriteLine(GetLongestDuplicate(numbers));

        }
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