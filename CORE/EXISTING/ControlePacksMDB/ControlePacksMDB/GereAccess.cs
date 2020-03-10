using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlePacksMDB
{
    class GereAccess
    {
        //--------------------------------------------------------------------
        // Retourne la chaîne de connection pour un fichier Access.
        //
        public static string GetConnectionStringMDB(string sFichier)
        {
            return "Driver={Microsoft Access Driver (*.mdb)};Dbq="
                + sFichier
                + ";Uid=Admin;Pwd=;";
        }
    }
}
