using PNPUCore.Database;
using PNPUCore.Process;
using System;
using System.Collections.Generic;

namespace PNPUCore
{
    class Launcher
    {
        List<InfoClient> listClient;


        void launchProcess(IProcess process)
        {
            process.executeMainProcess();
            String json = process.formatReport();
            Console.WriteLine(json);
        }


        void Launch(String clientName, String processName)
        {
            IProcess process = createProcess(processName, clientName);

            launchProcess(process);
        }


        IProcess createProcess(String process, String client)
        {
            return ProcessMock.createProcess();
        }

        static void Main(string[] args)
        {
            Launcher test = new Launcher();
            test.Launch("toto", "Unnomdeprocess");
        }
    }
}
