using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNPUTools.DataManager;
using System.Data;

namespace PNPUCore.Controle
{
    class ControleIDSynonym : IControle
    {
        private string sPathMdb = string.Empty;
        private List<int> lLIM_INF;
        private List<int> lLIM_SUP;

        public ControleIDSynonym(string sPPathMdb, List<int> LIM_INF, List<int> LIM_SUP)
        {
            sPathMdb = sPPathMdb;
            lLIM_INF = LIM_INF;
            lLIM_SUP = LIM_SUP;
        }

        public bool makeControl()
        {
            bool bResultat = true;
            int iID_SYNONYM;

            DataManagerAccess dmaManagerAccess = null;
            try
            {
                dmaManagerAccess = new DataManagerAccess(sPathMdb);
                DataSet dsDataSet = dmaManagerAccess.GetData("select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE ID_TI LIKE '%HR%CALC' AND ID_TI NOT LIKE '%DIF%' AND ID_SYNONYM <> 0");

                if (dsDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drRow in dsDataSet.Tables[0].Rows)
                    {
                        bool bPlageOK = true;

                        iID_SYNONYM = Int32.Parse(drRow[1].ToString());
                        for (int j = 0; j < lLIM_INF.Count && bPlageOK == true; j++)
                        {
                             if (iID_SYNONYM >= lLIM_INF[j] && iID_SYNONYM <= lLIM_SUP[j])
                                bPlageOK = false;

                        }
                        if (bPlageOK == false)
                        {
                            bResultat = false;
                            // TODO logger l'item et l'ID_SYNONYM
                        }
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
