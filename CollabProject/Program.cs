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
            Console.WriteLine(lastFirst("Wedge Antilles"));
          
            Console.WriteLine(isPalindrome("Racecar"));
            Console.WriteLine(isPalindrome("Chewy"));
        }

        /// <summary>
        /// Checks whether or not the entered string in lowercase
        /// is read the same forward and backward
        /// </summary>
        /// <param name="s"> the string to check </param>
        /// <returns> True if reversed is same, False otherwise </returns>
        static bool isPalindrome(string s)
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
            return reversed == lowerS;
        }

        static string lastFirst(string str)
        {
            string lastFirst = "";
            int indexOfSpace = str.IndexOf(' ');
            lastFirst  = str.Substring(indexOfSpace) + ", " + str.Substring(0,1) + ".";

            return lastFirst;
        }
    }
}