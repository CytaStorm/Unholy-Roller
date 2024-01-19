namespace CollabProject
{
    internal class Program
    {
        
        // Ramon Miland
        // Jeff Chen

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
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

            // Reverse the order of characters in the string
            string reversed = "";
            for (int i = lowerS.Length - 1; i >=0; i--)
            {
                reversed += lowerS.Substring(i, 1);
            }

            // Check if the string is the same forward and backward
            return reversed == lowerS;
        }
    }
}