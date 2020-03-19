using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleTableSecu : IControle
    {
        private string sPathMdb = string.Empty;

        public ControleTableSecu(string sPPathMdb)
        {
            sPathMdb = sPPathMdb;
        }

        public bool makeControl()
        {
            string sTest;
            bool bResultat = true;

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess(sPathMdb);
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_OBJECT FROM M4RDC_LOGIC_OBJECT WHERE HAVE_SECURITY <> 1");

                if (dsDataSet.Tables[0].Rows.Count > 0)
                {
                    bResultat = false;
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        // TODO Loguer les tables non sécurisées
                        sTest = drRow[0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO, loguer l'exception
                bResultat = false;
            }

            return bResultat;

        }
    }
}
