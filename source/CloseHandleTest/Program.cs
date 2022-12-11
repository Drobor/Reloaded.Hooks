using System;
using System.IO;
using System.Runtime.InteropServices;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;

[Function(CallingConventions.Stdcall)]

public static unsafe class Program
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    [Function(CallingConventions.Stdcall)]
    public delegate int CloseHandle_Delegate(int hObject);
    
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    //private static IHook<CloseHandle_Delegate> s_closeHandleHook;
    private static IHook<CloseHandle_Delegate> s_closeHandleHook;

    public static void Main(string[] args)
    {
        var address = GetProcAddress(LoadLibraryW("kernel32.dll"), "CloseHandle");

        Console.WriteLine($"Address:{address}");
        //Console.ReadLine();
            
        s_closeHandleHook = ReloadedHooks.Instance.CreateHook<CloseHandle_Delegate>(CloseHandle_Hook, (long)address);
        
        Console.WriteLine("hook created");
        Console.WriteLine($"OriginalFunctionAddress:{s_closeHandleHook.OriginalFunctionAddress}");
        Console.WriteLine($"OriginalFunctionWrapperAddress:{s_closeHandleHook.OriginalFunctionWrapperAddress}");
        //Console.ReadLine();

        s_closeHandleHook.Activate();

        Console.WriteLine("hook activated, Program should crash by itself in ~5 seconds if its netcore");
        
        //Console.ReadLine();

        for (int i = 0; i < 5; i++)
        {
            File.WriteAllText($"test{i}.txt", "just some stuff that opens and closes handles");
            File.Delete($"test{i}.txt");//Deleting file to verify that original CloseHandle function was properly called. Delete fails it is wasn't 
        }
        Console.ReadLine();
    }

    private static int CloseHandle_Hook(int hObject)
    {
        Console.WriteLine($"CloseHandle was called {hObject}.");
        
        var result = s_closeHandleHook.OriginalFunction(hObject);
        
        Console.WriteLine("CloseHandle original was called");
        return result;
    }
}