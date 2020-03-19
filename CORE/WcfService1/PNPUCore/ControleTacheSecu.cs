using System;
using System.Collections.Generic;
using System.Text;
using PNPUTools.DataManager;
using System.Data;


namespace PNPUCore.Controle
{
    class ControleTacheSecu : IControle
    {
        private string sPathMdb = string.Empty;

        ControleTacheSecu(string sPPathMdb)
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
                dmaManagerAccess = new DataManagerAccess("D:\\PNPU\\8.1_HF2003_PLFR_PAY.mdb");
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_BP FROM M4RBP_DEF WHERE SECURITY_TYPE <> 2");
                if (dsDataSet.Tables["M4RBP_DEF"].Rows.Count >= 0)
                {
                    bResultat = false;
                    foreach (DataRow drRow in dsDataSet.Tables["M4RBP_DEF"].Rows)
                    {
                        sTest = drRow["ID_BP"].ToString() ;
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


