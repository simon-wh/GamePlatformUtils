using GamePlatformUtils.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GPUTest
{
    class Program
    {
        static void Main(string[] args)
        {
	    Steam steam;
	    try {
            	steam = new Steam();
		Console.WriteLine(steam.InstallPath);
	    }
	    catch (FileNotFoundException exc){
		Console.WriteLine("Steam install not found!");
	    }

            Console.ReadKey();
        }
    }
}
