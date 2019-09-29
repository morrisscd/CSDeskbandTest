using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeskbandCallingApp
{
    class CallingProgram
    {
        // All COM DLLs must export the DllRegisterServer()
        // and the DllUnregisterServer() APIs for self-registration/unregistration.
        // They both have the same signature and so only one
        // delegate is required.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate UInt32 DllRegUnRegAPI();

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string strLibraryName);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern Int32 FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        static void DisplayUsage()
        {
            Console.WriteLine("Usage : COMRegistration [/r or /u] [Path to DLL].");
        }

        static bool CheckArguments(string[] args, out bool bRegistration)
        {
            // Ensure that there are at least 2 arguments.
            if (args.Length < 2)
            {
                DisplayUsage();
                bRegistration = false;
                return false;
            }

            // We take the first argument as the path to the COM DLL to register/unregister.
            if (args[0] == "/r")
            {
                bRegistration = true;
                return true;
            }
            else if (args[0] == "/u")
            {
                bRegistration = false;
                return true;
            }
            else
            {
                Console.WriteLine("Invalid flag : {0:S}.", args[0]);
                DisplayUsage();
                bRegistration = false;
                return false;
            }
        }

        static void Main()
        {
            // string[] args
            // Boolean to determine if registration or unregistration
            // is required.
            bool bRegistration;

            string[] args = new string[2];

            args[0] = "/r";

            //args[1] = @"C:\Users\kryte\source\repos\WinFormsEmailNotifier\bin\Debug\WinFormsEmailNotifier.dll";
            //args[1] = @"C:\temp\WinFormsEmailNotifier.dll";

            args[1] = @"C:\Users\kryte\source\repos\CSDeskBand\src\CSDeskBand\bin\Debug\CSDeskBand.dll";

            // Check the arguments for correctness.
            // Note that if the path to the DLL contains spaces,
            // the path must be enclosed in quotations, e.g. :
            // "c:\my path\my dll.dll"
            if (CheckArguments(args, out bRegistration) == false)
            {
                return;
            }

            // Load the DLL.
            IntPtr hModuleDLL = LoadLibrary(args[1]);

            if (hModuleDLL == IntPtr.Zero)
            {
                Console.WriteLine("Unable to load DLL : {0:S}.", args[1]);
                DisplayUsage();
                return;
            }

            // Obtain the required exported API.
            IntPtr pExportedFunction = IntPtr.Zero;

            if (bRegistration)
            {
                pExportedFunction = GetProcAddress(hModuleDLL, "Register");
            }
            else
            {
                pExportedFunction = GetProcAddress(hModuleDLL, "Unregister");
            }

            if (pExportedFunction == IntPtr.Zero)
            {
                Console.WriteLine("Unable to get required API from DLL.");
                return;
            }

            // Obtain the delegate from the exported function, whether it be
            // DllRegisterServer() or DllUnregisterServer().
            DllRegUnRegAPI pDelegateRegUnReg =
              (DllRegUnRegAPI)(Marshal.GetDelegateForFunctionPointer(pExportedFunction, typeof(DllRegUnRegAPI)))
              as DllRegUnRegAPI;

            // Invoke the delegate.
            UInt32 hResult = pDelegateRegUnReg();

            if (hResult == 0)
            {
                if (bRegistration)
                {
                    Console.WriteLine("Registration Successful.");
                }
                else
                {
                    Console.WriteLine("Unregistration Successful.");
                }
            }
            else
            {
                Console.WriteLine("Error occurred : {0:X}.", hResult);
            }

            FreeLibrary(hModuleDLL);
            hModuleDLL = IntPtr.Zero;
        }
    }
}
