using System.Linq;
using System.Diagnostics;
using System;
using Utils;
using System.Collections.Generic;

namespace freeze_pop_v2
{
    class Program
    {
        static bool RUN_MAIN_LOOP = true;
        static bool ATTACHED = false;  // Determines if we are activly hooking 
        static List<Process> listOfProcesses = new List<Process>(); // Snapshot of processes
        static Process targetProcess = null;   //  Process targeted for hooking
        static IntPtr targetProcessPtr; //  Target process pointer
        static int targetProcessId; //  Target process id
        static String userInput = ""; //    Holds what the user inputs

        static void Main(string[] args)
        {
            getProcessList();

            while (RUN_MAIN_LOOP)
            {

                //  Get the process id from the user
                bool validEntry = false;
                while (!validEntry)
                {
                    Console.Write("Enter Target Process ID: ");
                    userInput = Console.ReadLine();

                    if (int.TryParse(userInput, out targetProcessId))
                    {
                        try
                        {
                            targetProcess = Process.GetProcessById(targetProcessId);
                            validEntry = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message.ToString());
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid Process ID");
                    }
                }

                //  Here we can assume we have a valid process handle
                Console.WriteLine("Process valid -  attempting to hook");

                targetProcessPtr = Utils.Pinvoke.OpenProcess(Utils.Pinvoke.ProcessAccessFlags.All, false, 1);


                RUN_MAIN_LOOP = false;
            }

            Console.WriteLine("Bye");

        }

        static void getProcessList()
        {
            Console.WriteLine("==== Process List ====");
            Process.GetProcesses().ToList().ForEach(process =>
            {
                if (!Utils.Constants.PROCESSES_TO_IGNORE.Contains(process.ProcessName))
                {
                    listOfProcesses.Add(process);
                    Console.WriteLine(String.Format("{0} {1}", process.ProcessName, process.Id));
                }
            });
        }
    }
}
