using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ControlePacksMDB
{
    class GereSQLServer
    {
        //--------------------------------------------------------------------
        // Retourne la chaîne de connection pour un serveur SQL Server.
        //
        public string GetConnectionStringSQL(string sNomServeur)
        {
            try
            {
                return ConfigurationManager.AppSettings["CONNEXION_" + sNomServeur];
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        // MHUM le 06/02/2019 - Gestion des clients désynchro
        public string GetConnectionStringSQL(string sNomServeur, string sClient)
        {
            string sServeur = string.Empty;
            string sResultat = string.Empty;
            bool bTrouve = false; 
            try
            {
                sServeur = sClient.Substring(sClient.Length - 4, 3);
                sResultat = ConfigurationManager.AppSettings["BASEDESYNC_" + sNomServeur];
                sResultat = sResultat.Replace("[BASE]", sServeur);
                for (int i = 0; (i < ConfigurationManager.AppSettings.Count) && (bTrouve == false); i++)
                {
                    string s;
                    s = ConfigurationManager.AppSettings.AllKeys[i].ToString();
                    if ((s.Length > 10) && (s.Substring(0, 10) == "SERVDESYNC"))
                    {
                        if (ConfigurationManager.AppSettings[s].IndexOf("*" + sServeur + "*") > -1)
                        {
                            bTrouve = true;
                            sResultat = sResultat.Replace("[SERVEUR]", s.Substring(11));
                        }

                    }

                }
                if (bTrouve == false)
                    sResultat = string.Empty;

                return sResultat;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
    }
}
