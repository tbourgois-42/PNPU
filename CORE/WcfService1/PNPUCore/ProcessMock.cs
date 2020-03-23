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

            //Pour test MHUM
            listControl.Clear();
            string sCheminMDB = "D:\\PNPU\\02_8.1_HF2003_PLFR_HP.mdb";
            ControleTacheSecu ctsControleTacheSecu = new ControleTacheSecu(sCheminMDB);
            listControl.Add(ctsControleTacheSecu);
            ControleTableSecu ctsControleTableSecu = new ControleTableSecu(sCheminMDB);
            listControl.Add(ctsControleTableSecu);

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
