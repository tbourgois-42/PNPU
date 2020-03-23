using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;

namespace PNPUTools.DataManager
{
    public class DataManagerAccess : IDataManager
    {
        private string sMdbPath = string.Empty;

        public DataManagerAccess(string sPMdbPath)
        {
            sMdbPath = sPMdbPath;
        }


        string GetConnectionString()
        {
            return "Driver={Microsoft Access Driver (*.mdb)};Dbq="
                + sMdbPath
                + ";Uid=Admin;Pwd=;";
        }

        public DataSet GetData(string sRequest)
        {
            DataSet dataSet = null;

             using (OdbcConnection connection =
              new OdbcConnection(GetConnectionString()))
            {
                OdbcDataAdapter adapter =
                    new OdbcDataAdapter(sRequest, connection);

                // Open the connection and fill the DataSet.
                try
                {
                    connection.Open();
                    dataSet = new DataSet();
                    adapter.Fill(dataSet);
                }
                catch (Exception ex)
                {
                    // A gérer la mise à jour du log
                    //Console.WriteLine(ex.Message);
                    
                }

            }

             return dataSet;
        }

    }
}
