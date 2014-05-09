using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WUApiLib;

namespace WindowsUpdateCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parse(args);
        }

        private static void Parse(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("/Install", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Searching for Updates...");
                UpdateSession UpdateSession = new UpdateSession();
                IUpdateSearcher UpdateSearch = UpdateSession.CreateUpdateSearcher();
                UpdateSearch.Online = true;
                ISearchResult SearchResults = UpdateSearch.Search("IsInstalled=0 AND IsPresent=0");

                UpdateCollection UpdateCollection = new UpdateCollection();
                foreach (IUpdate x in SearchResults.Updates)
                {
                    if (!x.Title.Equals("Bing", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!x.EulaAccepted)
                        {
                            x.AcceptEula();
                        }
                        UpdateCollection.Add(x);
                    }
                }
                
                if (UpdateCollection.Count == 0)
                {
                    return;
                }

                Console.WriteLine("Downloading Updates");
                UpdateDownloader Downloader = UpdateSession.CreateUpdateDownloader();

                Downloader.Updates = UpdateCollection;

                try
                {
                    IDownloadResult DownloadResult = Downloader.Download();
                }
                catch
                {
                    Console.WriteLine("Program does not have Admin Access.");
                    return;
                }

                UpdateCollection InstallCollection = new UpdateCollection();

                foreach (IUpdate update in UpdateCollection)
                {
                    if (update.IsDownloaded)
                    {
                        InstallCollection.Add(update);
                    }
                }

                UpdateInstaller InstallAgent = (UpdateInstaller)UpdateSession.CreateUpdateInstaller();
                InstallAgent.Updates = InstallCollection;
                Console.WriteLine("Installing Updates...");
                IInstallationResult InstallResult = InstallAgent.Install();
                Console.WriteLine("Updates Completed.");
            }
            else
            {
                Console.WriteLine("\nIncorrect Usage.");
                Console.WriteLine("Example: WindowsUpdate.exe /Install\n");
            }
        }
    }
}
