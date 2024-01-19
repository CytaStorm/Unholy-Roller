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