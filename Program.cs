using System.Buffers.Binary;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
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

        static ProcessModule baseProcessModule;

        static void Main(string[] args)
        {
            //displayProcessList();

            while (RUN_MAIN_LOOP)
            {
                //  Get the process id from the user
                /*  bool validEntry = false;
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
                 } */

                targetProcess = Process.GetProcessesByName("BfVietnam")[0];

                //  Here we can assume we have a valid process handle
                Console.WriteLine("\nLoaded Modules In " + targetProcess.ProcessName + '\n');

                //  Hook the process
                targetProcessPtr = Utils.Pinvoke.OpenProcess(Utils.Pinvoke.ProcessAccessFlags.VMOperation | Utils.Pinvoke.ProcessAccessFlags.VMWrite | Utils.Pinvoke.ProcessAccessFlags.VMRead, false, targetProcess.Id);
                if (targetProcessPtr.ToString() == "0")
                {
                    Console.WriteLine("Failed to hook. Code: " + Utils.Pinvoke.GetLastError().ToString());
                }
                else
                {
                    Console.WriteLine("Attached");
                    ATTACHED = true;
                }

                if (ATTACHED)
                {
                    // Attempt to get the health address
                    //  List all the running modules in the process
                    foreach (ProcessModule module in targetProcess.Modules)
                    {
                        Console.WriteLine(string.Format("Module:\t {0} - {1}", module.FileName, module.ModuleName));
                        if (module.ModuleName == "BfVietnam.exe")
                        {
                            baseProcessModule = module;
                        }
                    }

                    IntPtr gameBaseAddress = baseProcessModule.EntryPointAddress;
                    IntPtr healthAddress = GetPointerWithOffsets(gameBaseAddress, new int[] { 0x0097D01C, 0x54, 0xA4, 0xDC, 0x10, 0x38 });

                    WriteMem(targetProcessPtr, healthAddress, 3333f);
                    //WriteMemFloat(targetProcessPtr, new IntPtr(0x2A0485C8), 34237f).ToString();
                }


                // Die for now
                RUN_MAIN_LOOP = false;
            }
        }

        static void displayProcessList()
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

        public static void WriteMem(IntPtr hProc, IntPtr address, long val)
        {

            int bytesWritten = 0;
            byte[] buffer = BitConverter.GetBytes(val);

            bool result = Utils.Pinvoke.WriteProcessMemory(hProc, address, buffer, 4, out bytesWritten);
            if (!result)
            {
                Console.WriteLine("Failed to write into memory. Code: " + Utils.Pinvoke.GetLastError().ToString());
            }
            else
            {
                Console.WriteLine("\n" + bytesWritten.ToString() + " bytes written");
            }
        }

        public static void WriteMem(IntPtr hProc, IntPtr address, float val)
        {
            int bytesWritten = 0;
            byte[] buffer = BitConverter.GetBytes(val);

            bool result = Utils.Pinvoke.WriteProcessMemory(hProc, new IntPtr(0x2A0485C8), buffer, (uint)buffer.Length, out bytesWritten);
            if (!result)
            {
                Console.WriteLine("Failed to write into memory. Code: " + Utils.Pinvoke.GetLastError().ToString());
            }
            else
            {
                Console.WriteLine("\nWrote " + val.ToString() + " as float (" + bytesWritten.ToString() + " bytes)");
            }
        }

        public static IntPtr GetPointerWithOffsets(IntPtr basePointer, int[] offsets)
        {
            IntPtr resultingPointer = basePointer;
            for (int i = 0; i < offsets.Length; i++)
            {
                resultingPointer = IntPtr.Add(resultingPointer, offsets[i]);
            }
            return resultingPointer;
        }
    }
}
