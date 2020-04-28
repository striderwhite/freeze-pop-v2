using System.Threading;
using System.Runtime.InteropServices;
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
        static bool RUN_HACK = true;
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

            //  Here we can assume we have a valid process handle
            targetProcess = Process.GetProcessesByName("BfVietnam")[0];


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
                //  List all the running modules in the process
                Console.WriteLine("\nLoaded Modules In " + targetProcess.ProcessName + '\n');
                foreach (ProcessModule module in targetProcess.Modules)
                {
                    Console.WriteLine(string.Format("Module:\t {0} - {1}", module.FileName, module.ModuleName));
                    if (module.ModuleName == "BfVietnam.exe")
                    {
                        baseProcessModule = module;
                    }
                }

                IntPtr gameBaseAddress = baseProcessModule.BaseAddress;
;
                Utils.Vec3 playerVec = new Utils.Vec3();
                Random r = new Random();

                while (RUN_HACK)
                {
                    IntPtr ptrVecPosition = GetPointerWithOffsets(targetProcessPtr, IntPtr.Add(gameBaseAddress, Utils.Offsets.V_VECTOR_BASE_OFFSET), Utils.Offsets.X_VECTOR_OFFSETS);

                    playerVec.x = readMemToFloat(targetProcessPtr, ptrVecPosition, 4);
                    playerVec.y = readMemToFloat(targetProcessPtr, IntPtr.Add(ptrVecPosition, 4), 4);
                    playerVec.z = readMemToFloat(targetProcessPtr, IntPtr.Add(ptrVecPosition, 8), 4);
                    Console.WriteLine("before \tx: " + playerVec.x + " y: " + playerVec.y + " z: " + playerVec.z);

                    WriteMem(targetProcessPtr, ptrVecPosition, Convert.ToSingle(r.Next(250, 2500)));
                    WriteMem(targetProcessPtr, IntPtr.Add(ptrVecPosition, 4), Convert.ToSingle(r.Next(250, 255)));
                    WriteMem(targetProcessPtr, IntPtr.Add(ptrVecPosition, 8), Convert.ToSingle(r.Next(250, 2500)));
                    Console.WriteLine("after \tx: " + playerVec.x + " y: " + playerVec.y + " z: " + playerVec.z);
                    Thread.Sleep(1000);
                }

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
                Console.WriteLine("\nWrote " + val.ToString() + " to " + IntPtrToHex(address) + " (" + bytesWritten.ToString() + " bytes)");
            }
        }

        public static void WriteMem(IntPtr hProc, IntPtr address, float val)
        {
            int bytesWritten = 0;
            byte[] buffer = BitConverter.GetBytes(val);

            bool result = Utils.Pinvoke.WriteProcessMemory(hProc, address, buffer, (uint)buffer.Length, out bytesWritten);
            if (!result)
            {
                Console.WriteLine("Failed to write into memory. Code: " + Utils.Pinvoke.GetLastError().ToString());
            }
            else
            {
                Console.WriteLine("\nWrote " + val.ToString() + " as float to " + IntPtrToHex(address) + " (" + bytesWritten.ToString() + " bytes)");
            }
        }

        public static IntPtr GetPointerWithOffsets(IntPtr hProc, IntPtr basePointer, int[] offsets)
        {
            // https://guidedhacking.com/threads/c-multilevel-pointer-function-c-version-of-finddmaaddy.11874/
            IntPtr resultingPointer = basePointer;
            var buffer = new byte[IntPtr.Size];
            //Console.WriteLine("Starting with address " + basePointer.ToInt64().ToString("X8"));
            for (int i = 0; i < offsets.Length; i++)
            {
                Utils.Pinvoke.ReadProcessMemory(hProc, resultingPointer.ToInt64(), buffer, (ulong)buffer.Length, out var read);
                //Console.WriteLine("Value Of Pointed Address  " + resultingPointer.ToInt64().ToString("X8"));
                resultingPointer = IntPtr.Add(new IntPtr(BitConverter.ToInt32(buffer, 0)), offsets[i]);
                //Console.WriteLine("New Pointed Address " + resultingPointer.ToInt64().ToString("X8"));
            }
            return resultingPointer;
        }

        public static float readMemToFloat(IntPtr hproc, IntPtr basePointer, int size)
        {
            var buffer = new byte[size];
            Utils.Pinvoke.ReadProcessMemory(hproc, basePointer.ToInt64(), buffer, (ulong)buffer.Length, out var read);
            return BitConverter.ToSingle(buffer);
        }

        public static String IntPtrToHex(IntPtr num)
        {
            return string.Format("0x{0:X8}", num.ToInt64());
        }
    }
}
