// -----------------------------------------------------------------------
// <copyright file="FastCgiHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace HttpServer.Addons.FastCgi
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class FastCgiHelper
    {
        public static bool CheckProcessExists(string name)
        {
            Process[] processlist = Process.GetProcesses();

            foreach (var p in processlist)
            {
                if (p.ProcessName == name)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TerminateProcess(string name)
        {
            Process[] processlist = Process.GetProcesses();

            foreach (var p in processlist)
            {
                if (p.ProcessName == name)
                {
                    try
                    {
                        p.Kill();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
