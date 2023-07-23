// LFInteractive LLC. 2021-2024﻿
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace Windows_Super_Delete;

internal class Program
{
    private static void Main(string[] args)
    {
        if (!IsRunningAsAdmin())
        {
            Console.WriteLine("This program requires to be run as admin!");
            ProcessStartInfo startInfo = new()
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName,
                Arguments = string.Join(' ', args),
                Verb = "runas",
                UseShellExecute = true,
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {
                Console.ReadKey();
            }
            Environment.Exit(0);
        }
        else
        {
            string path;
            if (!args.Any())
            {
                Console.Write("Enter Path to Super Delete: ");
                path = Path.GetFullPath(Console.ReadLine() ?? "./");
            }
            else
            {
                path = args[0];
            }

            Console.WriteLine($"Selected Directory: \"{path}\"");
            Console.Write("Are you sure [y/N]: ");
            string response = Console.ReadLine() ?? "N";

            if (response.ToLower().StartsWith('y'))
            {
                if (OperatingSystem.IsWindows())
                {
                    Destroy(path);
                }
                else
                {
                    Console.WriteLine("Only Windows is currently supported!");
                }
            }
        }
    }

    private static void Destroy(string path)
    {
        FileInfo info = new(path);
        System.Security.AccessControl.FileSecurity control = info.GetAccessControl();
        SecurityIdentifier? currentUser = WindowsIdentity.GetCurrent().User;
        control.SetOwner(currentUser);
        control.SetAccessRuleProtection(false, false);
        control.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(currentUser, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
        info.SetAccessControl(control);

        if (info.Attributes.HasFlag(FileAttributes.Directory))
        {
            Directory.Delete(path, true);
        }
        else
        {
            File.Delete(path);
        }
    }

    private static bool IsRunningAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}