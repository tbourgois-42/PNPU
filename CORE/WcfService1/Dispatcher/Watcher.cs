using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using PNPUCore;

namespace PNUDispatcher
{
    /// <summary>  
    /// Cette classe sert à communiquer avec le webservice via un PIPE nommé.  
    /// </summary>  
    public class Watcher
    {
        private static NamedPipeServerStream npssPipeClient = null;
        private static StreamString ssStreamString = null;
        private static Thread tListen = null;

        /// <summary>  
        /// Constructeur de la classe. Lance juste la fonction ListenRequest dans un thread.  
        /// </summary>  
        public Watcher()
        {
            tListen = new Thread(ListenRequest);
            tListen.Start();
        }

        /// <summary>  
        /// Destructeur de la classe. Ferme le PIPE nommé s'il est ouvert.  
        /// </summary> 
        ~Watcher()
        {
            if (npssPipeClient != null)
                npssPipeClient.Close();

        }

        /// <summary>  
        /// Fonction d'écoute lancée dans un thread. Elle attend les messages envoyés par le webservice via le PIPE et lui renvoie un résultat.
        /// </summary> 
        private static void ListenRequest()
        {
            string sMessage = string.Empty;
            string sMessageResultat = string.Empty;

            npssPipeClient = new NamedPipeServerStream("PNPU_PIPE");
            npssPipeClient.WaitForConnection();
            ssStreamString = new StreamString(npssPipeClient);

            while(true)
            {
                sMessage = ssStreamString.ReadString();
                Console.WriteLine(sMessage);

                if (IsValideJSON(sMessage) == false)
                    sMessageResultat = "KO";
                else {
                    sMessageResultat = "OK";
                    Thread thread = new Thread(new ThreadStart(LaunchProcessFunction));
                    thread.Start();
                }

                ssStreamString.WriteString(sMessageResultat);
            }
            
        }

        private static void LaunchProcessFunction()
        {
            var launcher = new Launcher();
            launcher.Launch("toto", "Unnomdeprocess");
        }

        /// <summary>  
        /// Fonction de controle de validité du JSON.
        /// </summary> 
        private static bool IsValideJSON(string sJSON)
        {
            if (sJSON == string.Empty)
                return false;

            return true;
        }



    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
