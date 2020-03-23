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

        public ControleTacheSecu(string sPPathMdb)
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
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_BP FROM M4RBP_DEF  WHERE SECURITY_TYPE <> 2"); 
                
                if (dsDataSet.Tables[0].Rows.Count > 0)
                {
                    bResultat = false;
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        // TODO Loguer les taches non sécurisées
                        sTest = drRow[0].ToString() ;
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


