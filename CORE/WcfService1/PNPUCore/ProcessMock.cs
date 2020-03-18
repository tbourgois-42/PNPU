using PNPUCore.Control;
using PNPUCore.Controle;
using System;
using System.Collections.Generic;
using System.Text;

namespace PNPUCore.Process
{
    class ProcessMock : IProcess
    {
        private readonly object listOfMockControl;

        public void executeMainProcess()
        {
            List<IControle> listControl =  ListControls.listOfMockControl;

            foreach (IControle controle in listControl)
            {
                controle.makeControl();
            }

        }

        public string formatReport()
        {
            return "{OUAH MAIS QUEL TALENT!}";
        }

        internal static IProcess createProcess()
        {
            return new ProcessMock();
        }
    }
}
