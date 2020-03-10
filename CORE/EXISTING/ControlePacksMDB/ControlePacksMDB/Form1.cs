/*---------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/* Programmé par Michaël HUMBERT le 13/03/2014                                                                                                                   */
/* Programme de contrôle des packs.                                                                                                                              */
/* MDH 14/01/2015 : Gestion d'une liste de commandes interdites                                                                                                  */
/* MDH 16/06/2015 : Gestion des tables absentes des packs                                                                                                        */
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.Odbc;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace ControlePacksMDB
{
    public partial class FormePrincipale : Form
    {
        public FormePrincipale()
        {

            InitializeComponent();
            lbErreurs.ForeColor = Color.Red;
            cbBaseSQL.Text = string.Empty;

            //=> MHUM le 06/02/2019 - Gestion clients désynchros
            // Chargement de la liste des serveur SQL configurés
            for (int i = 0; i < ConfigurationManager.AppSettings.Count; i++)
            {
                string s;

                s = ConfigurationManager.AppSettings.AllKeys[i].ToString();
               /* if (s.Length > 10 && s.Substring(0, 10) == "CONNEXION_")
                {
                    cbBaseSQL.Items.Add(s.Substring(10));
                }
                if (s == "BASEDEFAUT")
                    cbBaseSQL.Text = ConfigurationManager.AppSettings[s];*/
                if ((s == "CLIENTSDESYNCHRO") && (ConfigurationManager.AppSettings[s] == "O"))
                    cacClientsDesynchro.Checked = true;
                //=> MHUM le 19/09/2019 - Gestion chemin RAMDL
                if (s == "FICHIER_RAMDL")
                    tbRAMDL.Text = ConfigurationManager.AppSettings.GetValues(i)[0];
                //<= MHUM le 19/09/2019
                //=> MHUM le 19/09/2019 - Gestion chemin RAMDL
            }
            /*if (cbBaseSQL.Text == string.Empty)
                cbBaseSQL.Text = "SAASSN305";

            */
            //<= MHUM le 06/02/2019 - Gestion clients désynchros

            GereClientsDesynchro();
        }

        // => MHUM le 06/02/2019 - Gestion clients désynchros
        //------------------------------------------------------------------------------------------
        // Chargement des paramètres si clients désynchro.
        //
        //
        private void GereClientsDesynchro()
        {

            try
            {
                cbBaseSQL.Items.Clear();
                clbOrga.Items.Clear();

                if (cacClientsDesynchro.Checked == true)
                {
                    clbOrga.Visible = true;
                    btnAucun.Visible = true;
                    btnTous.Visible = true;
                }
                else
                {
                    clbOrga.Visible = false;
                    btnAucun.Visible = false;
                    btnTous.Visible = false;
                }


                for (int i = 0; i < ConfigurationManager.AppSettings.Count; i++)
                {
                    string s;
                    s = ConfigurationManager.AppSettings.AllKeys[i].ToString();

                    if (cacClientsDesynchro.Checked == true)
                    {
                        if (s.Length > 8 && s.Substring(0, 8) == "DESORGA_")
                        {
                            clbOrga.Items.Add(ConfigurationManager.AppSettings[s] + " [" + s.Substring(s.Length - 3, 3) + "]",true);
                        }
                        if (s.Length > 10 && s.Substring(0, 11) == "BASEDESYNC_")
                            cbBaseSQL.Items.Add(s.Substring(11));
                        if (s == "DESYNCDEF")
                            cbBaseSQL.Text = ConfigurationManager.AppSettings[s];

                    }
                    else
                    {
                        if (s.Length > 10 && s.Substring(0, 10) == "CONNEXION_")
                        {
                            cbBaseSQL.Items.Add(s.Substring(10));
                        }
                        if (s == "BASEDEFAUT")
                            cbBaseSQL.Text = ConfigurationManager.AppSettings[s];

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Erreur - Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        // <= MHUM le 06/02/2019 - Gestion clients désynchros
        
        //--------------------------------------------------------------------
        // Sélection du dossier contenant les packs à vérifier
        //
        private void btnChoixDossierPacks_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = tbDossierPacks.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDossierPacks.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //--------------------------------------------------------------------
        // Chargement des packs contenus dans le dossier indiqué
        //
        private void btnChargerPacks_Click(object sender, EventArgs e)
        {
            if (tbDossierPacks.Text == String.Empty)
                MessageBox.Show("Veuillez indiquer le dossier contenant les packs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {

                foreach (string sFichier in Directory.GetFiles(tbDossierPacks.Text, "*.mdb"))
                {
                    bool bTrouve = false;
                    int i;

                    for (i = 0; i < dgvListePacks.Rows.Count && bTrouve == false; i++)
                    {
                        // Contrôle que le fichier n'est pas déja dans la liste
                        if (dgvListePacks.Rows[i].Cells[0].Value.ToString() == sFichier)
                            bTrouve = true;
                    }

                    if (bTrouve == false)
                    {
                        dgvListePacks.Rows.Add(sFichier);
                        dgvListePacks.SelectAll();
                    }
                }
            }

        }

        //--------------------------------------------------------------------
        // Effacement de la liste des fichiers
        //
        private void btnEffacerListePacks_Click(object sender, EventArgs e)
        {
            dgvListePacks.Rows.Clear();
            lbErreurs.Items.Clear();
            lbActionLocalisation.Items.Clear();

        }

        //--------------------------------------------------------------------
        // Contrôle des plages des ID_SYNONYM
        //
        private int ControlID_SYNONYM(List<int> LIM_INF, List<int> LIM_SUP)
        {
            string sConnection;
            int iResultat = 0;
            int iID_SYNONYM;

            bool bTableExiste;

            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);


                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {

                        bTableExiste = CtrlExisteTableMDB(connection, "M4RCH_ITEMS");
                        if (bTableExiste == true)
                        {

                            OdbcCommand command = connection.CreateCommand();

                            command.CommandText = "select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE ID_TI LIKE '%HR%CALC' AND ID_TI NOT LIKE '%DIF%' AND ID_SYNONYM <> 0";
                            try
                            {
                                connection.Open();

                                OdbcDataReader reader = command.ExecuteReader();

                                while (reader.Read())
                                {
                                    bool bPlageOK = true;
                                    iID_SYNONYM = Int32.Parse(reader[1].ToString());
                                    for (int j = 0; j < LIM_INF.Count && bPlageOK == true; j++)
                                    {
                                        if (iID_SYNONYM >= LIM_INF[j] && iID_SYNONYM <= LIM_SUP[j])
                                            bPlageOK = false;
                                    }
                                    if (bPlageOK == false)
                                        lbErreurs.Items.Add(sCheminMDB + " : L'ID_SYNONYM de l'item " + reader[0].ToString() + " (" + reader[1].ToString() + ") est dans les plages réservées client.");
                                }
                                reader.Close();
                            }
                            catch (Exception ex)
                            {
                                lbErreurs.Items.Add("ControlID_SYNONYM (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                                return -999;
                            }
                        }
                        else
                        {
                            AjoutMessageListes("Avertissement : table M4RCH_ITEMS inexistante dans le MDB. Contrôle des ID_SYNONYM impossible.");
                        }
                    }
                       
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControlID_SYNONYM - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }
            }
            return iResultat;
        }


        //--------------------------------------------------------------------
        // Contrôle des ID_SYNONYM déja exisant en prod
        //
        private int ControleID_SYNONYMExistant(SqlConnection sqlConn)
        {

            string sConnection;
            int iResultat = 0;
            string sID_SYNONYM;
            string sRequeteSqlServer = String.Empty;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            Dictionary<string, string> dicListItems = new Dictionary<string, string>();
            bool bTableExiste;

            try
            {

                for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        bTableExiste = CtrlExisteTableMDB(connection, "M4RCH_ITEMS");
                        if (bTableExiste == true)
                        {
                            OdbcCommand command = connection.CreateCommand();

                            command.CommandText = "select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE (ID_TI LIKE '%HRPERIOD%CALC' OR ID_TI LIKE '%HRROLE%CALC') AND ID_TI NOT LIKE '%DIF%' AND ID_SYNONYM <> 0";
                            try
                            {
                                bool bItemAControler = false;
                                connection.Open();

                                OdbcDataReader reader = command.ExecuteReader();

                                sRequeteSqlServer = "select ID_ITEM, ID_SYNONYM FROM M4RCH_ITEMS WHERE (ID_TI LIKE '%HRPERIOD%CALC' OR ID_TI LIKE '%HRROLE%CALC') AND ID_TI NOT LIKE '%DIF%' AND (";

                                while (reader.Read()) /* Parcours des items contenus dans le MDB */
                                {
                                    sID_SYNONYM = reader[1].ToString();

                                    try
                                    {
                                        string sTestExistence;

                                        sTestExistence = dicListItems[sID_SYNONYM].ToString();
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        if (bItemAControler == false)
                                            bItemAControler = true;
                                        else
                                            sRequeteSqlServer += "OR ";

                                        sRequeteSqlServer += " (ID_SYNONYM = " + reader[1].ToString() + " AND ID_ITEM <> '" + reader[0].ToString() + "') ";

                                        dicListItems.Add(sID_SYNONYM, reader[0].ToString());
                                    }
                                }

                                if (bItemAControler == true)
                                {
                                    sRequeteSqlServer += ")";
                                    sqlComm = new SqlCommand(sRequeteSqlServer, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read())
                                    {
                                        lbErreurs.Items.Add(sCheminMDB + " : L'ID_SYNONYM de l'item " + dicListItems[sqlDR["ID_SYNONYM"].ToString()] + " (" + sqlDR["ID_SYNONYM"].ToString() + ") est déja utilisé pour l'item " + sqlDR["ID_ITEM"] + ".");
                                    }
                                    sqlDR.Close();
                                    sqlDR = null;
                                }
                                reader.Close();
                            }
                            catch (Exception ex)
                            {
                                lbErreurs.Items.Add("ControleID_SYNONYMExistant (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                                return -999;
                            }

                        }
                        else
                        {
                            AjoutMessageListes("Avertissement : table M4RCH_ITEMS inexistante dans le MDB. Contrôle des ID_SYNONYM impossible.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lbErreurs.Items.Add("ControleID_SYNONYMExistant - Erreur d'exécution (exception) : " + ex.Message);
                return -999;
            }

            return iResultat;
        }

        //--------------------------------------------------------------------
        // Lancement des contrôles
        //
        private void btnControler_Click(object sender, EventArgs e)
        {
            bool bErreur = false;
            string sListeErreurs = string.Empty;
            string sNomFichierResultat;
            StreamWriter swFichierResultat;
            GereSQLServer gsGereSQLServer = null;
            string sChaineDeConnexionSQLServer;
            SqlConnection sqlConn = null;

            List<string> CMD_L = new List<string>();
            List<string> CMD_D = new List<string>();
            List<string> CMD_F = new List<string>();
            List<string> CMD_B = new List<string>();
            List<int> LIM_INF = new List<int>();
            List<int> LIM_SUP = new List<int>();
            // MDH 14/01/2015 - Ajout des commandes interdites
            List<string> L_INTERDIT = new List<string>();
            // MHUM 11/01/2017 - Gestion des ID_ORGAS
            List<string> L_ORGA = new List<string>();
            // MHUM 18/12/2017 - Clés et sections des paramètres applicatifs interdites
            string s_CLE = string.Empty;
            string s_SECTION = string.Empty;

            // MHUM 16/01/2018 - Gestion des statuts des tickets liés aux packs
            List<string[]> pListePackTickets;
            // MHUM 16/01/2018 - Gestion des statuts des tickets liés aux packs
            
            // MHUM le 06/02/2019 - Gestion clients désynchros
            int iIndexOrgaDesync = 0;
            string sEnvironnement = string.Empty;
            bool bBoucle = true;
            
            // MHUM le 20/09/2019 - Saisie des identifiants pour PN pour lancer l'analyse RAMDL
            fSaisieIdentifiants SaisieIdentifiant = null;
            string sLogin = string.Empty;
            string sMdp = string.Empty;



            try
            {
                lbErreurs.Items.Clear();
                lbActionLocalisation.Items.Clear();

                if (dgvListePacks.SelectedRows.Count == 0) /* contrôle qu'au moins un fichier est sélectionné */
                {
                    sListeErreurs += "Vous devez sélectionner au moins un pack.\n\n";
                    bErreur = true;
                }

                 // MHUM le 31/01/2019 - Gestion environnement désynchro - Il faut avoir coché au moins 1 environement désynchro
                if ((cacClientsDesynchro.Checked == true) && (clbOrga.CheckedItems.Count == 0))
                {
                    sListeErreurs += "Vous devez sélectionner au moins un environnement désynchronisé.\n\n";
                    bErreur = true;
                }

                // MHUM le 20/09/2019 - Gestion de l'analyse RAMDL
                if (cacAnalyseRAMDL.Checked == true)
                {
                    if (tbRAMDL.Text == string.Empty)
                    {
                        sListeErreurs += "Vous devez indiquer le chemin du fichier RAMDL.EXE.\n\n";
                        bErreur = true;
                    }
                    else
                    {
                        if (tbRAMDL.Text.ToUpper().IndexOf("RAMDL.EXE") == -1)
                        {
                            sListeErreurs += "Vous devez indiquer le chemin du fichier RAMDL.EXE.\n\n";
                            bErreur = true;
                        }
                        else
                        {
                            if (File.Exists(tbRAMDL.Text) == false)
                            {
                                sListeErreurs += "Le chemin du fichier RAMDL.EXE est incorrect.\n\n";
                                bErreur = true;
                            }
                            else
                            {
                                if (bErreur == false)
                                {
                                    bool bBoucleIdent = true;
                                    SaisieIdentifiant = new fSaisieIdentifiants();
                                    while (bBoucleIdent == true)
                                    {
                                        if (SaisieIdentifiant.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                                        {
                                            if (MessageBox.Show("Confirmez-vous l'annulation de l'analyse RAMDL ?", "Confirmation", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                            {
                                                cacAnalyseRAMDL.Checked = false;
                                                bBoucleIdent = false;
                                            }

                                        }
                                        else
                                        {
                                            SaisieIdentifiant.LitIndentifiants(out sLogin, out sMdp);
                                            if (LoginMdpPNValide(sLogin, sMdp) == true)
                                                bBoucleIdent = false;
                                            else
                                                MessageBox.Show("Login ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }


                }

                if (bErreur == false) /* s'il n'y a pas d'erreur */
                {
                    Cursor.Current = Cursors.WaitCursor;
                    /*ControleSecuTaches();
                    ControleSecuTables();*/
                    lbInfos.Text = "Contrôles des types des packs en cours...";

                    gsGereSQLServer = new GereSQLServer();
                    // MHUM le 31/01/2019 - Gestion environnement désynchro
                    while (bBoucle == true)
                    {
                        if (cacClientsDesynchro.Checked == true)
                        {
                            sChaineDeConnexionSQLServer = gsGereSQLServer.GetConnectionStringSQL(cbBaseSQL.SelectedItem.ToString(), clbOrga.CheckedItems[iIndexOrgaDesync].ToString());
                        }
                        else
                            sChaineDeConnexionSQLServer = gsGereSQLServer.GetConnectionStringSQL(cbBaseSQL.SelectedItem.ToString());


                        sqlConn = new SqlConnection(sChaineDeConnexionSQLServer);
                        sqlConn.Open();

                        // MHUM le 31/01/2019 - Gestion environnement désynchro - On ne fait qu'une seule fois le contrôle
                        if (iIndexOrgaDesync == 0)
                        {
                            /* Chargement des commandes autorisées dans les différents types de packs */
                            //ChargeParametrage(ref CMD_L, ref CMD_D, ref CMD_F, ref CMD_B, ref LIM_INF, ref LIM_SUP, ref L_INTERDIT, ref L_ORGA, ref s_CLE, ref s_SECTION);
                            ChargeParametrage(ref CMD_L, ref CMD_D, ref CMD_F, ref CMD_B, ref LIM_INF, ref LIM_SUP, ref L_INTERDIT, ref L_ORGA, ref s_CLE, ref s_SECTION,sqlConn);

                            ControleTypePack(CMD_L, CMD_D, CMD_F, CMD_B, L_INTERDIT, s_CLE, s_SECTION);
                        }

                        // MHUM le 31/01/2019 - Gestion environnement désynchro - On ne traite que l'orga du client
                        if (cacClientsDesynchro.Checked == true)
                        {
                            L_ORGA.Clear();
                            L_ORGA.Add(clbOrga.CheckedItems[iIndexOrgaDesync].ToString().Substring(0, 4));
                            sEnvironnement = clbOrga.CheckedItems[iIndexOrgaDesync].ToString().Substring(clbOrga.CheckedItems[iIndexOrgaDesync].ToString().Length - 4, 3);

                            if (Directory.Exists(tbDossierResultat.Text + "\\" + sEnvironnement + "\\RD") == false)
                                Directory.CreateDirectory(tbDossierResultat.Text + "\\" + sEnvironnement + "\\RD");
                        }

                        lbInfos.Text = "Contrôles des informations de localisation en cours...";
                        ActionsLocalisation(sqlConn, L_ORGA, sEnvironnement);

                        lbInfos.Text = "Contrôles des ID_SYNONYM en cours...";
                        this.Refresh();

                        if (cbStandard.Checked == true)
                        {
                            // MHUM le 31/01/2019 - Gestion environnement désynchro - On ne fait qu'une seule fois le contrôle
                            if (iIndexOrgaDesync == 0)
                            {
                                /* On contrôle les plages des ID_SYNONYM si pack standard */
                                ControlID_SYNONYM(LIM_INF, LIM_SUP);
                            }

                            /* Controle si les Id_SYNONYM livrés existent déja sur la base */
                            ControleID_SYNONYMExistant(sqlConn);

                            // MHUM le 31/01/2019 - Gestion environnement désynchro - On ne fait qu'une seule fois le contrôle
                            if (iIndexOrgaDesync == 0)
                            {
                                //=> MHUM 07/04/2016 - Ajout du contrôle sur le catalogue des tables
                                if (cacControlerListeTables.Checked == true)
                                    ControleCatalogueTables(sqlConn, sEnvironnement);

                                ControleTousM4OModifies(sqlConn);
                            }

                        }

                        // MHUM le 31/01/2019 - Gestion environnement désynchro - On ne fait qu'une seule fois le contrôle
                        if (iIndexOrgaDesync == 0)
                        {
                            lbInfos.Text = "Contrôles des dépendances en cours...";
                            this.Refresh();

                            // MHUM 16/01/2018 - Gestion des statuts des tickets liés aux packs
                            pListePackTickets = new List<string[]>();
                            LectureStatuts(ref pListePackTickets);
                            // MHUM 16/01/2018 - Gestion des statuts des tickets liés aux packs

                            ControleDependances(pListePackTickets);

                            pListePackTickets.Clear();


                            //=> MHUM le 20/03/2018 - Ajout du contrôle qu'un objet ou une présentation hérité en plateforme n'est pas déja hérité en standard
                            lbInfos.Text = "Contrôles des héritages en cours...";
                            this.Refresh();
                            ControleNiveauHeritage(sqlConn);
                            //<= MHUM le 20/03/2018

                        }
                        
                        //=> MHUM le 11/07/2018 - Contrôle des changements du niveau de saisie
                        ControleNiveauxSaisieItems(sqlConn, sEnvironnement);
                        //<= MHUM le 11/07/2018

                        //=> MHUM le 18/09/2019 - Lancement de l'analyse RAMDL
                        lbInfos.Text = "Analyse RAMDL en cours...";
                        this.Refresh();
                        if (cacAnalyseRAMDL.Checked == true)
                            AnalyseMdbRAMDL(sEnvironnement, sChaineDeConnexionSQLServer, sLogin, sMdp);
                        //<= MHUM le 18/09/2019

                        /* Fermeture de la connexion sql */
                        sqlConn.Close();

                        //=> MHUM le 06/02/2019 - Gestion clients désynchro
                        if (cacClientsDesynchro.Checked == true)
                        {
                            iIndexOrgaDesync++;
                            if (iIndexOrgaDesync >= clbOrga.CheckedItems.Count)
                                bBoucle = false;
                        }
                        else
                            bBoucle = false;
                    }

                    lbInfos.Text = "Ecriture du résultat en cours...";
                    this.Refresh();

                    // Ecriture du fichier log
                    if (tbDossierResultat.Text != string.Empty)
                        sNomFichierResultat = tbDossierResultat.Text + "\\";
                    else
                        sNomFichierResultat = string.Empty;

                    sNomFichierResultat += "ControlePackMDB_LOG.TXT";
                    swFichierResultat = new StreamWriter(sNomFichierResultat, false, Encoding.UTF8);

                    foreach (string s in lbActionLocalisation.Items)
                    {
                        swFichierResultat.WriteLine(s);
                    }

                    if (lbErreurs.Items.Count > 0)
                    {
                        swFichierResultat.WriteLine("\n!!!!!!!!! ERREURS !!!!!!!!!");
                        foreach (string s in lbErreurs.Items)
                        {
                            swFichierResultat.WriteLine(s);
                        }

                    }
                    swFichierResultat.Close();

                    lbInfos.Text = string.Empty;
                    Cursor.Current = Cursors.Default;

                    if (lbErreurs.Items.Count > 0)
                    {
                        if (lbErreurs.Items.Count == 1)
                            MessageBox.Show("Traitement terminé avec 1 erreur.","Erreur",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        else
                            MessageBox.Show("Traitement terminé avec " + lbErreurs.Items.Count.ToString() + " erreurs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        MessageBox.Show("Traitement terminé sans erreur.","Information",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show(sListeErreurs,"Erreur",MessageBoxButtons.OK,MessageBoxIcon.Error);
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Erreur - Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        //--------------------------------------------------------------------
        // Contrôles de la sécurisation des tâches
        //
        private int ControleSecuTaches()
        {
            string sConnection;
            int iResultat = 0;

            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();

                        command.CommandText = "select ID_BP FROM M4RBP_DEF WHERE SECURITY_TYPE <> 2";
                        try
                        {
                            connection.Open();

                            OdbcDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                iResultat = -1;
                                lbErreurs.Items.Add(sCheminMDB + " : Tâche " + reader[0].ToString() + " non sécurisée.");
                            }
                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleSecuTaches (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }

                    }
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleSecuTaches - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }
            }
            return iResultat;
        }

        //--------------------------------------------------------------------
        // Contrôles de la sécurisation des tables
        //
        private int ControleSecuTables()
        {
            string sConnection;
            int iResultat = 0;

            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();

                        command.CommandText = "select ID_OBJECT FROM M4RDC_LOGIC_OBJECT WHERE HAVE_SECURITY <> 1";
                        try
                        {
                            connection.Open();

                            OdbcDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                iResultat = -1;
                                lbErreurs.Items.Add(sCheminMDB + " : Table " + reader[0].ToString() + " non sécurisée.");
                            }
                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleSecuTables (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }

                    }
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleSecuTables - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }
            }
            return iResultat;
        }

 
        //--------------------------------------------------------------------
        // Chargement du paramétrage
        //
        private int ChargeParametrage(ref List<string> CMD_L, ref List<string> CMD_D, ref List<string> CMD_F, ref List<string> CMD_B, ref List<int> LIM_INF, ref List<int> LIM_SUP, ref List<string> L_INTERDIT, ref List<string> L_ORGA, ref string s_CLE, ref string s_SECTION, SqlConnection sqlConn)
        {
            // MHUM 18/12/2017 - Gestion des clés et sections interdites
            s_CLE = string.Empty;
            s_SECTION = string.Empty;

            // MHUM le 21/05/2019 - Je charge les plages des ID_SYNONYM client depuis la table M4CFR_PLAGES_ID_SYNONYM au lieu du fichier de config */
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR;

            /* Chargement des commandes autorisées dans les différents types de packs */
            for (int i = 0; i < ConfigurationManager.AppSettings.Count; i++)
            {
                string s;

                s = ConfigurationManager.AppSettings.AllKeys[i].ToString();
                switch (s.Substring(0, 6))
                {
                    case "PACK_L":
                        CMD_L.Add(ConfigurationManager.AppSettings[s]);
                        break;

                    case "PACK_D":
                        CMD_D.Add(ConfigurationManager.AppSettings[s]);
                        break;

                    case "PACK_F":
                        CMD_F.Add(ConfigurationManager.AppSettings[s]);
                        break;

                    case "PACK_B":
                        CMD_B.Add(ConfigurationManager.AppSettings[s]);
                        break;

                    /* MHUM le 21/05/2019 - Maintenant lon cherche les plages ID_SYNONYM dans la table M4CFR_PLAGES_ID_SYNONYM    
                    case "LIMINF":
                        LIM_INF.Add(Int32.Parse(ConfigurationManager.AppSettings[s]));
                        break;

                    case "LIMSUP":
                        LIM_SUP.Add(Int32.Parse(ConfigurationManager.AppSettings[s]));
                        break;
                        */
                    // MDH 14/01/2015 - Ajout des commandes interdites
                    case "INTERD":
                        L_INTERDIT.Add(ConfigurationManager.AppSettings[s]);
                        break;
                    // FIN MDH 14/01/2015

                    // MHUM 11/01/2017 - Gestion des ID_ORGAS
                    case "IDORGA":
                        L_ORGA.Add(ConfigurationManager.AppSettings[s]);
                        break;

                    // MHUM 18/12/2017 - Gestion des clés interdites
                    case "PARKEY":
                        if (s_CLE == string.Empty)
                            s_CLE = "'";
                        else
                            s_CLE += ",'";

                        s_CLE += ConfigurationManager.AppSettings[s] + "'";
                        break;

                    // MHUM 18/12/2017 - Gestion des sections interdites
                    case "PARSEC":
                        if (s_SECTION == string.Empty)
                            s_SECTION = "'";
                        else
                            s_SECTION += ",'";

                        s_SECTION += ConfigurationManager.AppSettings[s] + "'";
                        break;

                }
            }

            // MHUM le 21/05/2019 - Je charge les plages des ID_SYNONYM client depuis la table M4CFR_PLAGES_ID_SYNONYM au lieu du fichier de config */
            sqlComm = new SqlCommand("SELECT CFR_PLAGE_DEBUT, CFR_PLAGE_FIN  FROM  M4CFR_PLAGES_ID_SYNONYM WHERE ID_ORGANIZATION ='0000' and CFR_ID_TYPE = 'CLIENT'", sqlConn);
            sqlDR = sqlComm.ExecuteReader();
            while (sqlDR.Read()) 
            {
                LIM_INF.Add(Int32.Parse(sqlDR["CFR_PLAGE_DEBUT"].ToString()));
                LIM_SUP.Add(Int32.Parse(sqlDR["CFR_PLAGE_FIN"].ToString()));
            }
            sqlDR.Close();
            
            return 0;
        }

        //--------------------------------------------------------------------
        // Contrôles du contenu de chaque type de pack
        //
        private int ControleTypePack(List<string> CMD_L, List<string> CMD_D, List<string> CMD_F, List<string> CMD_B, List<string> L_INTERDIT, string s_CLE, string s_SECTION)
        {
            string sConnection;
            int iResultat = 0;
            // MHUM le 18/12/2017 - Ajout contrôle sur paramètres applicatifs
            string sCmdCtrlParam = "SELECT DISTINCT ID_SECTION + '\\'+ID_KEY AS CLE FROM  M4RAV_APP_VAL_LG1 WHERE ";
            OdbcDataReader reader;

            if (s_CLE != string.Empty) 
            {
                sCmdCtrlParam += "ID_KEY IN (" + s_CLE +")";
                if (s_SECTION != string.Empty)
                    sCmdCtrlParam += " OR ";
            }
            if (s_SECTION != string.Empty)
            {
                sCmdCtrlParam += "ID_SECTION IN (" + s_SECTION + ")";
            }
            // FIN MHUM le 18/12/2017

            AjoutMessageListes("Controle des types de pack.",3);

            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;

                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    AjoutMessageListes("Traitement du fichier " + sCheminMDB,1);

                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();

                        try
                        {
                            connection.Open();

                            // MHUM le 18/12/2017 - Ajout contrôle sur paramètres applicatifs
                            if (CtrlExisteTableMDB(connection, "M4RAV_APP_VAL_LG1") == true)
                            {
                                command.CommandText = sCmdCtrlParam;
                                reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    lbErreurs.Items.Add(sCheminMDB + " : Livraison du paramètre appicatif " + reader[0].ToString() + " interdite.");
                                }
                                reader.Close();
                            }
                            // FIN MHUM le 18/12/2017

                            // Controle du contenu des packs L
                            ControleUnType(command, "_L", CMD_L, sCheminMDB, L_INTERDIT);

                            // Controle du contenu des packs D
                            ControleUnType(command, "_D", CMD_D, sCheminMDB, L_INTERDIT);

                            // Controle du contenu des packs F
                            ControleUnType(command, "_F", CMD_F, sCheminMDB, L_INTERDIT);

                            // Controle du contenu des packs B
                            ControleUnType(command, "_B", CMD_B, sCheminMDB, L_INTERDIT);

                            connection.Close();
                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleTypePack (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }

                    }
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleTypePack - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }
            }
            return iResultat;
        }

        private void ControleUnType(OdbcCommand pCommand, string psTypePack, List<string> pListCMD, string pCheminMDB, List<string> L_INTERDIT)
        {
            bool bTrouve;
            int iCpt;
            string sCommandPack;
            OdbcDataReader reader;


            // MDH 11/03/2015 - je ne traite que les lignes actives
            //pCommand.CommandText = "select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%" + psTypePack + "'";
            pCommand.CommandText = "select ID_PACKAGE,CMD_SEQUENCE,CMD_CODE FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%" + psTypePack + "' AND CMD_ACTIVE =-1";
            reader = pCommand.ExecuteReader();

            while (reader.Read())
            {
                iCpt = 0;
                bTrouve = false;
                sCommandPack = reader[2].ToString().ToUpper().Trim();
                
                // Je remplace les espaces et tabulation par un seul espace.
                sCommandPack = System.Text.RegularExpressions.Regex.Replace(sCommandPack, "\\s+", " ");
                
                
                
                // MDH 14/01/2015 - Ajout des commandes interdites
                while ((iCpt < L_INTERDIT.Count()) && (bTrouve == false))
                {
                    if (sCommandPack.IndexOf(L_INTERDIT[iCpt++]) >= 0)
                    {
                        double dConv;

                        try
                        {
                            dConv = Convert.ToDouble(reader[1].ToString());
                        }
                        catch
                        {
                            dConv = 0;
                        }
                        bTrouve = true;
                        lbErreurs.Items.Add(pCheminMDB + " : La commande " + dConv.ToString("###0") + " du pack " + reader[0].ToString() + " est interdite.");
                    }
                }
                iCpt = 0;
                // FIN MDH 14/01/2015

                while ((iCpt < pListCMD.Count()) && (bTrouve == false))
                {
                    if (sCommandPack.IndexOf(pListCMD[iCpt++]) >= 0)
                        bTrouve = true;
                }

                if (bTrouve == false)
                {
                    double dConv;
                    
                    try
                    {
                        dConv = Convert.ToDouble(reader[1].ToString());
                    }
                    catch
                    {
                        dConv = 0;
                    }

                    lbErreurs.Items.Add(pCheminMDB + " : La commande " + dConv.ToString("###0") + " du pack " + reader[0].ToString() + " ne doit pas être dans un pack " + psTypePack + ".");
                }
            }
            reader.Close();

        }


        //--------------------------------------------------------------------
        // Ajout d'un élément dans la liste des actions à faire pour la localisation
        //
        private int AjoutElementListeActions(ref SortedDictionary<string, List<string>> dActions, string sPack, string sChaine)
        {
            if (dActions.ContainsKey(sPack) == false)
                dActions.Add(sPack, new List<string> { sChaine });
            else
                dActions[sPack].Add(sChaine);
            return 0;
        }

        //--------------------------------------------------------------------
        // Liste des actions à faire pour la localisation
        //
        private int ActionsLocalisation(SqlConnection sqlConn, List<string> L_ORGA, string sEnvironnement)
        {
            string sConnection;
            int iResultat = 0;
            string sNomFichierActions;
            StreamWriter swFichierActions;
            List<string[]> lListeAControler = new List<string[]>();
            string sRequeteControle = string.Empty;
            bool bPremierElement;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            bool ExisteTable = true;
            // MHUM 10/01/2017 - Modification du fichier des actions
            SortedDictionary<string, List<string>> dActions = new SortedDictionary<string, List<string>>();
            string sORGA_COPY = String.Empty; 
            string sORGA_SCRIPT = String.Empty;
            StreamReader srFichierTemplate;
            string sTemplateSEC_LOBJ = String.Empty;
            string sTemplateM4RCH_VT_TPL_OV = String.Empty;
            string sNomFichierActionsCmd;
            
            string sTempo = String.Empty;
            // => MHUM le 25/09/2019 - Demande de Guilain, plus de fichier CMD on met directement les insert dans le fichier de commande
            //SortedDictionary<string, List<string>> dActionsCmd = new SortedDictionary<string, List<string>>();
            //StreamWriter swFichierActionsCmd;
            // <= MHUM le 25/09/2019

            srFichierTemplate = new StreamReader("Copie Sécu SEC_LOBJ.SQL");
            sTemplateSEC_LOBJ = srFichierTemplate.ReadToEnd();
            srFichierTemplate.Close();

            srFichierTemplate = new StreamReader("MAJ M4RCH_VT_TPL_OV.sql");
            sTemplateM4RCH_VT_TPL_OV = srFichierTemplate.ReadToEnd();
            srFichierTemplate.Close();

            bPremierElement = true;
            foreach (string orga in L_ORGA)
            {
                if (bPremierElement == true)
                {
                    bPremierElement = false;
                    sORGA_COPY = "'";
                    sORGA_SCRIPT = "'";
                }
                else
                {
                    sORGA_COPY += ",";
                    sORGA_SCRIPT += ",'";
                }
                sORGA_COPY += orga;
                sORGA_SCRIPT += orga + "'";
            }
            if (sORGA_COPY.Length > 0)
                sORGA_COPY += "'";

            AjoutMessageListes("Action de localisation",3);
            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    if (tbDossierResultat.Text != string.Empty)
                        sNomFichierActions = tbDossierResultat.Text + "\\";
                    else
                        sNomFichierActions = string.Empty;

                    // => MHUM le 06/02/2019 - Gestion clients désynchro
                    if (sEnvironnement != string.Empty)
                        sNomFichierActions += sEnvironnement + "\\RD\\";
                    // <= MHUM le 06/02/2019

                    //=> MHUM 11/01/2017 - Fichier de commande pour sécurités tables et nouveaux items
                    sNomFichierActionsCmd = sNomFichierActions + Path.GetFileNameWithoutExtension(sCheminMDB) + "_CMD.SQL";
                    //<= MHUM 11/01/2017

                    

                    sNomFichierActions += Path.GetFileNameWithoutExtension(sCheminMDB) + "_ACT.CSV";
                    swFichierActions = new StreamWriter(sNomFichierActions, false/*, Encoding.UTF8*/);
                    swFichierActions.WriteLine("TYPE;PACK;ELEMENT");

                    AjoutMessageListes("Traitement du fichier " + sCheminMDB,1);

                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();
                        OdbcCommand command2 = connection.CreateCommand();


                        try
                        {
                            OdbcDataReader reader = null;
                            connection.Open();

                            ExisteTable = CtrlExisteTableMDB(connection, "M4RBP_DEF");
                            if (ExisteTable == true)
                            {
                                // Contrôle des tâches
                                command.CommandText = "select ID_PACKAGE, ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_CLASS = 'BUSINESS PROCESS' AND RIGHT(ID_PACKAGE,2) = '_L' AND CMD_ACTIVE =-1";
                                reader = command.ExecuteReader();


                                while (reader.Read()) // Contrôle des tâches
                                {
                                    command2.CommandText = "SELECT SECURITY_TYPE FROM M4RBP_DEF WHERE ID_BP = '" + reader[1].ToString() + "'";
                                    OdbcDataReader reader2 = command2.ExecuteReader();


                                    if (reader2.Read())
                                    {
                                        if (reader2[0].ToString() == "2")
                                            lListeAControler.Add(new string[] { reader[0].ToString(), reader[1].ToString() });
                                        else
                                            lbErreurs.Items.Add(sCheminMDB + " : Tâche " + reader[1].ToString() + " non sécurisée.");
                                    }
                                    else
                                        lbErreurs.Items.Add(sCheminMDB + " : Tâche " + reader[1].ToString() + " inexistante dans la table M4RBP_DEF.");
                                    reader2.Close();

                                }
                                reader.Close();

                                //---------------------------------------------------------------------------------------------------------------------------
                                // Pour les tâches sécurisées, vérification de l'existance et du paramétrage sur la base de référence.

                                sRequeteControle = "select A.ID_BP, (select COUNT(*) from M4RBP_APPROLE B where B.ID_BP=A.ID_BP) AS DROIT from M4RBP_DEF A where ID_BP IN (";
                                bPremierElement = true;
                                foreach (string[] t in lListeAControler)
                                {
                                    if (bPremierElement == true)
                                        bPremierElement = false;
                                    else
                                        sRequeteControle += ",";
                                    sRequeteControle += "'" + t[1] + "'";
                                }
                                sRequeteControle += ")";

                                if (bPremierElement == false)
                                {
                                    sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read()) // Traitement des tâches trouvées sur la base de référence
                                    {
                                        foreach (string[] t in lListeAControler)
                                        {
                                            if (t[1] == sqlDR["ID_BP"].ToString()) // On trouve le code tâche
                                            {
                                                if (Int16.Parse(sqlDR["DROIT"].ToString()) > 0) // Des droits sont définis
                                                {
                                                    AjoutMessageListes(t[0] + " : Tâche " + t[1] + " existante, droits définis (M4RBP_APPROLE).");
                                                    swFichierActions.WriteLine("SECU TACHE (EXISTE + DROITS);" + t[0] + ";" + t[1]);
                                                    AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RBP_APPROLE from origin to destination where \" ID_BP = '" + t[1] + "'\"\\ /* EXISTE + DROITS */");
                                                }
                                                else
                                                {
                                                    AjoutMessageListes(t[0] + " : Tâche " + t[1] + " existante, droits NON définis (M4RBP_APPROLE).");
                                                    swFichierActions.WriteLine("SECU TACHE (EXISTE);" + t[0] + ";" + t[1]);
                                                    AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RBP_APPROLE from origin to destination where \" ID_BP = '" + t[1] + "'\"\\ /* EXISTE */");
                                                }
                                                t[0] = "Traité"; // Indique l'enregistrement traité
                                            }

                                        }
                                    }
                                    sqlDR.Close();
                                    sqlDR = null;
                                }
                                foreach (string[] t in lListeAControler)
                                {
                                    if (t[0] != "Traité")
                                    {
                                        AjoutMessageListes(t[0] + " : Tâche " + t[1] + " inexistante, droits à définir (M4RBP_APPROLE).");
                                        swFichierActions.WriteLine("SECU TACHE (N EXISTE PAS);" + t[0] + ";" + t[1]);
                                        AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RBP_APPROLE from origin to destination where \" ID_BP = '" + t[1] + "'\"\\");
                                    }
                                }
                                lListeAControler.Clear();

                                // Controle l'existance de tâche non transférée
                                command.CommandText = "select ID_BP FROM M4RBP_DEF WHERE ID_BP NOT IN (select ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_CLASS = 'BUSINESS PROCESS')";
                                reader = command.ExecuteReader();

                                while (reader.Read()) // Contrôle des tâches
                                {
                                    lbErreurs.Items.Add(sCheminMDB + " : Tâche " + reader[0].ToString() + " présente dans le MDB mais non transférée.");
                                }
                                reader.Close();

                            }
                            else
                            {
                                lbActionLocalisation.Items.Add("Avertissement : table M4RBP_DEF inexistante dans le MDB. Contrôle des tâches impossible.");
                            }


                            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
                            // Contrôle des tables

                            ExisteTable = CtrlExisteTableMDB(connection, "M4RDC_LOGIC_OBJECT");
                            if (ExisteTable == true)
                            {
                                command.CommandText = "select ID_PACKAGE, ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_CLASS = 'LOGICAL TABLE' AND RIGHT(ID_PACKAGE,2) = '_L' AND CMD_ACTIVE =-1";
                                reader = command.ExecuteReader();

                                while (reader.Read()) // Contrôle des tables
                                {
                                    command2.CommandText = "SELECT HAVE_SECURITY FROM M4RDC_LOGIC_OBJECT WHERE ID_OBJECT = '" + reader[1].ToString() + "'";
                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                    if (reader2.Read())
                                    {
                                        if (reader2[0].ToString() == "1")
                                            lListeAControler.Add(new string[] { reader[0].ToString(), reader[1].ToString() });
                                        else
                                            lbErreurs.Items.Add(sCheminMDB + " : Table " + reader[1].ToString() + " non sécurisée.");
                                    }
                                    else
                                        lbErreurs.Items.Add(sCheminMDB + " : Table " + reader[1].ToString() + " inexistante dans la table M4RDC_LOGIC_OBJECT.");
                                    reader2.Close();
                                }
                                reader.Close();


                                //---------------------------------------------------------------------------------------------------------------------------
                                // Pour les tables sécurisées, vérification de l'existance et du paramétrage sur la base de référence.

                                sRequeteControle = "select A.ID_OBJECT, (select COUNT(*) from M4RDC_SEC_LOBJ B where B.ID_OBJECT=A.ID_OBJECT) AS DROIT from M4RDC_LOGIC_OBJECT A where ID_OBJECT IN (";
                                bPremierElement = true;
                                foreach (string[] t in lListeAControler)
                                {
                                    if (bPremierElement == true)
                                        bPremierElement = false;
                                    else
                                        sRequeteControle += ",";
                                    sRequeteControle += "'" + t[1] + "'";
                                }
                                sRequeteControle += ")";

                                if (bPremierElement == false)
                                {
                                    sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read()) // Traitement des tables trouvées sur la base de référence
                                    {
                                        foreach (string[] t in lListeAControler)
                                        {
                                            if (t[1] == sqlDR["ID_OBJECT"].ToString()) // On trouve le code table
                                            {
                                                if (Int16.Parse(sqlDR["DROIT"].ToString()) > 0) // Des droits sont définis
                                                {
                                                    AjoutMessageListes(t[0] + " : Table " + t[1] + " existante, droits définis (M4RDC_SEC_LOBJ).");
                                                    swFichierActions.WriteLine("SECU TABLE (EXISTE + DROITS);" + t[0] + ";" + t[1]);
                                                    //=> MHUM le 25/09/2019 - Demande de Guilain, on écrit directement les insert dans fichier de commande 
                                                    //AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RDC_SEC_LOBJ FROM ORIGIN TO DESTINATION WHERE \" ID_OBJECT = '" + t[1] + "'\"\\ /* EXISTE + DROITS */");
                                                    //sTempo = "/* EXISTE + DROITS */\n" + sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]);
                                                    //AjoutElementListeActions(ref dActionsCmd, t[0], sTempo);
                                                    sTempo = "/* EXISTE + DROITS */\n" + sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]) + "\\";
                                                    AjoutElementListeActions(ref dActions, t[0], sTempo);
                                                    //<= MHUM le 25/09/2019
                                                }
                                                else
                                                {
                                                    AjoutMessageListes(t[0] + " : Table " + t[1] + " existante, droits NON définis (M4RDC_SEC_LOBJ).");
                                                    swFichierActions.WriteLine("SECU TABLE (EXISTE);" + t[0] + ";" + t[1]);
                                                    //=> MHUM le 25/09/2019 - Demande de Guilain, on écrit directement les insert dans fichier de commande 
                                                    //AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RDC_SEC_LOBJ FROM ORIGIN TO DESTINATION WHERE \" ID_OBJECT = '" + t[1] + "'\"\\ /* EXISTE */");
                                                    //sTempo = "/* EXISTE */\n" + sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]);
                                                    //AjoutElementListeActions(ref dActionsCmd, t[0], sTempo);
                                                    sTempo = "/* EXISTE */\n" + sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]) + "\\";
                                                    AjoutElementListeActions(ref dActions, t[0], sTempo);
                                                    //<= MHUM le 25/09/2019
                                                }
                                                t[0] = "Traité"; // Indique l'enregistrement traité
                                            }

                                        }
                                    }

                                    sqlDR.Close();
                                    sqlDR = null;
                                }
                                foreach (string[] t in lListeAControler)
                                {
                                    if (t[0] != "Traité")
                                    {
                                        AjoutMessageListes(t[0] + " : Table " + t[1] + " inexistante, droits à définir (M4RDC_SEC_LOBJ).");
                                        swFichierActions.WriteLine("SECU TABLE (N EXISTE PAS);" + t[0] + ";" + t[1]);
                                        //=> MHUM le 25/09/2019 - Demande de Guilain, on écrit directement les insert dans fichier de commande 
                                        /*AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RDC_SEC_LOBJ FROM ORIGIN TO DESTINATION WHERE \" ID_OBJECT = '" + t[1] + "'\"\\ ");
                                        sTempo = sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]);
                                        AjoutElementListeActions(ref dActionsCmd, t[0], sTempo);*/
                                        sTempo = sTemplateSEC_LOBJ.Replace("#TABLE#", t[1]) + "\\";
                                        AjoutElementListeActions(ref dActions, t[0], sTempo);
                                        //<= MHUM le 25/09/2019
                                    }
                                }
                                lListeAControler.Clear();


                                // Controle l'existance de table non transférée
                                command.CommandText = "select ID_OBJECT FROM M4RDC_LOGIC_OBJECT WHERE ID_OBJECT NOT IN (select ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_CLASS = 'LOGICAL TABLE')";
                                reader = command.ExecuteReader();

                                while (reader.Read()) // Contrôle des tâches
                                {
                                    lbErreurs.Items.Add(sCheminMDB + " : Table " + reader[0].ToString() + " présente dans le MDB mais non transférée.");
                                }
                                reader.Close();
                            }
                            else
                            {
                                lbActionLocalisation.Items.Add("Avertissement : table M4RDC_LOGIC_OBJECT inexistante dans le MDB. Contrôle des tables impossible.");
                            }

                            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
                            // Contrôle des sécurités des concepts de paie

                            ExisteTable = CtrlExisteTableMDB(connection, "M4RTM_VT_CNCPT_HS");
                            if (ExisteTable == true)
                            {
                                command.CommandText = "select DISTINCT ID_DMD_COMPONENT FROM M4RTM_VT_CNCPT_HS WHERE HAVE_SECURITY=1";
                                reader = command.ExecuteReader();

                                while (reader.Read()) // Contrôle des sécurités de concept
                                {
                                    command2.CommandText = "SELECT ID_PACKAGE,CMD_SEQUENCE FROM  M4RDL_PACK_CMDS WHERE CMD_CODE LIKE '%M4RTM_VT_CNCPT_HS%' AND CMD_CODE LIKE '%" + reader[0].ToString() + "%' AND CMD_ACTIVE = -1";
                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                    if (reader2.Read())
                                    {
                                        // Pour l'instant on considère que c'est une erreur de livrer des sécurités sur les concepts
                                        lbErreurs.Items.Add(sCheminMDB + " : Livraison de sécurité sur le concept " + reader[0].ToString() + " dans le pack " + reader2[0].ToString() + ".");
                                    }
                                    reader2.Close();
                                }
                                reader.Close();

                            }
                            else
                            {
                                lbActionLocalisation.Items.Add("Avertissement : table M4RTM_VT_CNCPT_HS inexistante dans le MDB. Contrôle des sécurités des concepts impossible.");
                            }

                            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
                            // Propagation de données
                            
                            // MHUM le 12/06/2019 - Je filtre sur %COPY_DATA_9999 et uniquement les commandes actives
                            command.CommandText = "select ID_PACKAGE, CMD_CODE FROM M4RDL_PACK_CMDS WHERE ID_OBJECT LIKE '%COPY_DATA_9999' AND CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();

                            while (reader.Read()) // Récupération des propagations à prévoir
                            {
                                string stempo = reader[1].ToString();
                                AjoutMessageListes("Données à propager pour le pack " + reader[0].ToString());
                                swFichierActions.WriteLine("PROPAGATION;" + reader[0].ToString() + ";");

                                stempo = System.Text.RegularExpressions.Regex.Replace(stempo, "@id_orgas_dest(|\\s+)=(|\\s+)'9999'", "@id_orgas_dest = " + sORGA_COPY,System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                stempo = System.Text.RegularExpressions.Regex.Replace(stempo, "ID_ORGANIZATION(|\\s+)=(|\\s+)'9999'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                stempo = System.Text.RegularExpressions.Regex.Replace(stempo, "ID_ORGANIZATION\\s+LIKE(|\\s+)'9999'", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                stempo = System.Text.RegularExpressions.Regex.Replace(stempo, "ID_ORGANIZATION\\s+IN(|\\s+)\\((|\\s+)'9999'(|\\s+)\\)", "ID_ORGANIZATION IN (" + sORGA_SCRIPT + ")", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (System.Text.RegularExpressions.Regex.IsMatch(stempo, "SFR_APPEL_ID_ORGA",System.Text.RegularExpressions.RegexOptions.IgnoreCase) == true)
                                {
                                    
                                    System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(stempo, "EXECUTE_METHOD.+9999.+DESTINATION(|\\s)\\\\", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    foreach (System.Text.RegularExpressions.Match m in mc)
                                    {
                                        string sOrigine = m.Value;
                                        string sModifie = string.Empty;
                                        foreach (string sOrg in L_ORGA)
                                        {
                                            if (sModifie != String.Empty)
                                                sModifie += "\n";
                                            sModifie += sOrigine.Replace("\"9999\"", "\"" + sOrg + "\"");
                                        }
                                        stempo = stempo.Replace(sOrigine, sModifie);
                                    }
                                }
                                AjoutElementListeActions(ref dActions, reader[0].ToString(), stempo);
                            }
                            reader.Close();

//=> MHUM 19/06/2017 - Controle de la présence de données pour les replace
                            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
                            // Controle de la présence de données pour les replace
                            command.CommandText = "select ID_PACKAGE, CMD_CODE FROM M4RDL_PACK_CMDS WHERE UCase(CMD_CODE) LIKE '%REPLACE%' AND CMD_ACTIVE=-1 ";
                            reader = command.ExecuteReader();

                            while (reader.Read()) // Récupération des propagations à prévoir
                            {
                                string stempo = reader[1].ToString();
                                string sTable = string.Empty;
                                string sWhere = string.Empty;
                                int iIndex = 0;
                                int iIndex2 = 0;
                                int iIndex3 = 0;

                                iIndex = stempo.ToUpper().IndexOf("REPLACE ", iIndex);
                                while (iIndex >= 0)
                                {
                                    iIndex += "REPLACE".Length + 1;
                                    iIndex2 = stempo.ToUpper().IndexOf("FROM", iIndex);
                                    if (iIndex2 >= 0)
                                    {
                                        // Récupréation du nom de la table
                                        sTable = stempo.ToUpper().Substring(iIndex, iIndex2 - iIndex).Trim();
                                        sWhere = string.Empty;
                                        
                                            // Recherche s'il y a une clause where
                                        iIndex = stempo.IndexOf('\"', iIndex2);
                                        if (iIndex >= 0)
                                        {
                                            // Vérifie qu'il ne s'agit pas du WHERE d'une commande replace suivante
                                            iIndex3 = stempo.ToUpper().IndexOf("REPLACE ", iIndex2);
                                            if ((iIndex3 < 0) || (iIndex3 > iIndex))
                                            {

                                                iIndex2 = stempo.IndexOf('\"', iIndex + 1);
                                                if (iIndex2 < 0)
                                                    iIndex2 = stempo.Length - 1;
                                                else
                                                    sWhere = stempo.Substring(iIndex + 1, iIndex2 - iIndex - 1);
                                            }
                                            else
                                            {
                                                if ((iIndex3 >= 0) && (iIndex3 < iIndex))
                                                    iIndex = iIndex3;
                                            }
                                        }
                                        ExisteTable = CtrlExisteTableMDB(connection, sTable);
                                        if (ExisteTable == true)
                                        {
                                            command2.CommandText = "SELECT COUNT(*) FROM " + sTable;
                                            if (sWhere != string.Empty)
                                                command2.CommandText += " WHERE " + sWhere;

                                            OdbcDataReader reader2 = command2.ExecuteReader();
                                            if (reader2.Read() == false)
                                            {
                                                lbErreurs.Items.Add(sCheminMDB + " : Replace sur la table " + sTable + " dans le pack " + reader[0].ToString() + " sans donnée dans le mdb (filtre : " + sWhere + ")" );
                                            }
                                            else
                                            {
                                                if (reader2[0].ToString() == "0")
                                                    lbErreurs.Items.Add(sCheminMDB + " : Replace sur la table " + sTable + " dans le pack " + reader[0].ToString() + " sans donnée dans le mdb (filtre : " + sWhere + ")");
                                            }

                                            reader2.Close();
                                        }
                                        else
                                        {
                                            lbErreurs.Items.Add(sCheminMDB + " : Replace sur la table " + sTable + " dans le pack " + reader[0].ToString() + " mais la table n'existe pas dans le mdb.");
                                        }
                                        
                                    }
                                    if (iIndex >= 0)
                                        iIndex = stempo.ToUpper().IndexOf("REPLACE ", iIndex);

                                }
                            }
                            reader.Close();

//<= MHUM 19/06/2017 - Controle de la présence de données pour les replace
    
                            //-----------------------------------------------------------------------------------------------------------------------------------------------------------
                            // MHUM 07/07/2016 - Ajout de la liste des nouveaux items

                            ExisteTable = CtrlExisteTableMDB(connection, "M4RCH_ITEMS");
                            if (ExisteTable == true)
                            {
                                // MHUM 11/05/2018 - il manque les items de niveau rôle
                                //command.CommandText = "select ID_PACKAGE,A.ID_TI+'.'+A.ID_ITEM,B.ID_PACKAGE from M4RCH_ITEMS A, M4RDL_PACK_CMDS B where A.ID_TI LIKE '%HRPERIOD_CALC' AND A.ID_TI NOT LIKE '%DIF_HRPERIOD_CALC' AND B.ID_PACKAGE LIKE '%L' AND B.ID_OBJECT = A.ID_TI+'.'+A.ID_ITEM AND B.CMD_ACTIVE=-1";
                                command.CommandText = "select ID_PACKAGE,A.ID_TI+'.'+A.ID_ITEM,B.ID_PACKAGE from M4RCH_ITEMS A, M4RDL_PACK_CMDS B where ((A.ID_TI LIKE '%HRPERIOD_CALC' AND A.ID_TI NOT LIKE '%DIF_HRPERIOD_CALC') OR (A.ID_TI LIKE '%HRROLE_CALC' AND A.ID_TI NOT LIKE '%DIF_HRROLE_CALC')) AND B.ID_PACKAGE LIKE '%L' AND B.ID_OBJECT = A.ID_TI+'.'+A.ID_ITEM AND B.CMD_ACTIVE=-1";
                                reader = command.ExecuteReader();

                                sRequeteControle = "select ID_TI +'.' + ID_ITEM AS C_ITEM from M4RCH_ITEMS where ID_TI +'.' + ID_ITEM IN (";
                                bPremierElement = true;

                                while (reader.Read()) // Lecture des items de paie livrés
                                {
                                    lListeAControler.Add(new string[] { reader[0].ToString(), reader[1].ToString() });
                                    if (bPremierElement == true)
                                        bPremierElement = false;
                                    else
                                        sRequeteControle += ",";
                                    sRequeteControle += "'" + reader[1].ToString() + "'";

                                }
                                reader.Close();
                                sRequeteControle += ")";

                                if (bPremierElement == false)
                                {
                                    sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read()) // Traitement des items trouvés sur la base de référence
                                    {
                                        foreach (string[] t in lListeAControler)
                                        {
                                            if (t[1] == sqlDR["C_ITEM"].ToString()) // On trouve l'item
                                            {
                                                t[0] = "Traité";
                                            }
                                        }
                                    }
                                    sqlDR.Close();
                                    sqlDR = null;
                                }

                                foreach (string[] t in lListeAControler)
                                {
                                    if (t[0] != "Traité")
                                    {
                                        AjoutMessageListes("Nouvel item de paie " + t[1] + " livré dans le pack " + t[0]);
                                        swFichierActions.WriteLine("NOUVEL ITEM PAIE;" + t[0] + ";" + t[1]);
                                        //=> MHUM le 25/09/2019 - Demande de Guilain, on écrit directement les insert dans fichier de commande 
                                        /*AjoutElementListeActions(ref dActions, t[0], "REPLACE M4RCH_VT_TPL_OV FROM ORIGIN TO DESTINATION WHERE \" ID_DMD_COMPONENT = '" + t[1].Substring(t[1].IndexOf('.') + 1) + "'\"\\ ");
                                        sTempo = sTemplateM4RCH_VT_TPL_OV.Replace("#ID_DMD_COMPONENT#", t[1].Substring(t[1].IndexOf('.')+1));
                                        AjoutElementListeActions(ref dActionsCmd, t[0], sTempo);*/
                                        sTempo = sTemplateM4RCH_VT_TPL_OV.Replace("#ID_DMD_COMPONENT#", t[1].Substring(t[1].IndexOf('.') + 1)) + "\\";
                                        AjoutElementListeActions(ref dActions, t[0], sTempo);
                                        //<= MHUM le 25/09/2019
                                    }
                                }
                                lListeAControler.Clear();
                                
                            }
                            connection.Close();
                            swFichierActions.Close();

                            // MHUM 11/01/2017 - Essai nouveau fichier action
                            if (dActions.Keys.Count > 0)
                            {
                                if (tbDossierResultat.Text != string.Empty)
                                    sNomFichierActions = tbDossierResultat.Text + "\\";
                                else
                                    sNomFichierActions = string.Empty;

                                // => MHUM le 06/02/2019 - Gestion clients désynchro
                                if (sEnvironnement != string.Empty)
                                    sNomFichierActions += sEnvironnement + "\\RD\\";
                                // <= MHUM le 06/02/2019


                                sNomFichierActions += Path.GetFileNameWithoutExtension(sCheminMDB) + "_ACT.SQL";
                                swFichierActions = new StreamWriter(sNomFichierActions, false/*, Encoding.UTF8*/);

                                foreach (string s in dActions.Keys)
                                {
                                    swFichierActions.WriteLine("/*" + s + "*/");
                                    foreach (string e in dActions[s])
                                        swFichierActions.WriteLine(e);
                                    swFichierActions.WriteLine("/*" + s + "*/");
                                    swFichierActions.WriteLine();
                                }
                                swFichierActions.Close();
                                dActions.Clear();
                            }

                            // => MHUM le 25/09/2019 - Demande de Guilain, plus de fichier CMD on met directement les insert dans le fichier de commande
                            /*if (dActionsCmd.Keys.Count > 0)
                            {
                                swFichierActionsCmd = new StreamWriter(sNomFichierActionsCmd, false,Encoding.UTF8);
                                foreach (string s in dActionsCmd.Keys)
                                {*/
                                   // swFichierActionsCmd.WriteLine("/*" + s + "*/");
                                    /*foreach (string e in dActionsCmd[s])
                                        swFichierActionsCmd.WriteLine(e);*/
                                    //swFichierActionsCmd.WriteLine("/*" + s + "*/");
                                    /*swFichierActionsCmd.WriteLine();
                                }
                                swFichierActionsCmd.Close();
                                dActionsCmd.Clear();
                            }*/
                            // <= MHUM le 25/09/2019

                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ActionsLocalisation (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }


                    }
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ActionsLocalisation - Erreur d'exécution (exception) : " + ex.Message); 
                    return -999;
                }
            }
            return iResultat;
        }

        //--------------------------------------------------------------------
        // Contrôles des dépendances
        //
        private int ControleDependances(List<string[]> pListePackTickets)
        {
            string sConnection;
            string sConnection2;
            int iResultat = 0;
            string sNomFichierDependance;
            StreamWriter swFichierDependance;
            string[] tElement1 = new string[3];
            string[] tElement2 = new string[3];

            AjoutMessageListes("Contrôle des dépendances",3);
            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    string sCheminMDB;
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    if (tbDossierResultat.Text != string.Empty)
                        sNomFichierDependance = tbDossierResultat.Text + "\\";
                    else
                        sNomFichierDependance = string.Empty;
                    
                    sNomFichierDependance += Path.GetFileNameWithoutExtension(sCheminMDB) + "_DEP.CSV";
                    swFichierDependance = new StreamWriter(sNomFichierDependance, false, Encoding.UTF8);
                    // MHUM 22/05/2018 - Je précise les 2 éléments
                    //swFichierDependance.WriteLine("CODE PACK;TICKET;STATUT;CODE PACK 2;TICKET2;STATUT2;CLASSE ELEMENT;ELEMENT;FICHIER MDB");
                    //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                    //swFichierDependance.WriteLine("CODE PACK;TICKET;STATUT;CODE PACK 2;TICKET2;STATUT2;CLASSE ELEMENT1 / CLASSE ELEMENT2;ELEMENT1 / ELEMENT2;FICHIER MDB");
                    swFichierDependance.WriteLine("CODE PACK;TICKET;LIB;STATUT;CODE PACK 2;TICKET2;LIB2;STATUT2;CLASSE ELEMENT1 / CLASSE ELEMENT2;ELEMENT1 / ELEMENT2;FICHIER MDB");
                    //<= MHUM 10/09/2018
                    AjoutMessageListes("Traitement du fichier " + sCheminMDB,1);

                    
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();
                        OdbcDataReader reader;

                        try
                        {
                            connection.Open();

                            // Recherche des dépendances dans le même mdb.
                            
                            // Recherche de la livraison du même élément dans différents packs
                            command.CommandText = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT, B.ID_PACKAGE FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS = B.ID_CLASS AND A.ID_OBJECT = B.ID_OBJECT AND A.ID_PACKAGE <> B.ID_PACKAGE AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                foreach (string[] tElem1 in pListePackTickets)
                                {
                                    if (tElem1[0] == reader[0].ToString())
                                    {
                                        foreach (string[] tElem2 in pListePackTickets)
                                        {
                                            if (tElem2[0] == reader[3].ToString())
                                            {
                                                //MHUM 14/06/2018 - Pas de dépendance si c'est la présentation du BP électronique
                                                if ((reader[1].ToString() == "PRESENTATION") && (reader[2].ToString().IndexOf("DP_PAYROLL_CHANNEL")>0))
                                                    break;
                                                // MHUM 22/05/2018 - Je précise les 2 éléments
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //=> MHUM le 10/09/2018 - Ajout du libellé du ticket
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader[1].ToString() + ";" + reader[2].ToString() + "/" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader[1].ToString() + ";" + reader[2].ToString() + "/" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //<= MHUM le 10/09/2018
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                /*int cpt1 = 0;
                                
                                while ((cpt1 < pListePackTickets.Count-1) && (pListePackTickets[cpt1][0] != reader[0].ToString())) cpt1++;
                                if (cpt1 < pListePackTickets.Count - 1)
                                {
                                    tElement1[0] = pListePackTickets[cpt1][0];
                                    tElement1[1] = pListePackTickets[cpt1][1];
                                    tElement1[2] = pListePackTickets[cpt1][2];
                                    cpt1 = 0;
                                    while ((cpt1 < pListePackTickets.Count - 1) && (pListePackTickets[cpt1][0] != reader[3].ToString())) 
                                        cpt1++;
                                    if (cpt1 < pListePackTickets.Count - 1)
                                    {
                                        tElement2[0] = pListePackTickets[cpt1][0];
                                        tElement2[1] = pListePackTickets[cpt1][1];
                                        tElement2[2] = pListePackTickets[cpt1][2];
                                        swFichierDependance.WriteLine(tElement1[0] + ";" + tElement1[1] + ";" + tElement1[2] + ";" + tElement2[0] + ";" + tElement2[1] + ";" + tElement2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                    }
                                }*/

                                //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader[3].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader[3].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                            }
                            reader.Close();

                            // MHUM 06/08/2018 - Ajout test existance table M4RCH_PICOMPONENTS
                            if (CtrlExisteTableMDB(connection, "M4RCH_PICOMPONENTS") == true)
                            {

                                // MHUM le 09/07/2018 - Recherche de la livraison d'un payroll item dans un pack et de d'un item de ce payroll item dans un autre pack.
                                command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B, M4RCH_PICOMPONENTS P WHERE A.ID_CLASS='PAYROLL ITEM' AND P.ID_T3 + '.' + P.ID_PAYROLL_ITEM= A.ID_OBJECT AND B.ID_CLASS='ITEM' AND B.ID_OBJECT=P.ID_TI + '.' + P.ID_ITEM AND A.ID_PACKAGE <> B.ID_PACKAGE AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE= -1";
                                reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    foreach (string[] tElem1 in pListePackTickets)
                                    {
                                        if (tElem1[0] == reader[0].ToString())
                                        {
                                            foreach (string[] tElem2 in pListePackTickets)
                                            {
                                                if (tElem2[0] == reader[3].ToString())
                                                {
                                                    // MHUM le 09/07/2018 - Je précise les 2 éléments
                                                    //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                    //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                    swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                    // MHUM le 09/07/2018 - Ajout de la dépendance dans l'autre sens
                                                    //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                    swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                    //<= MHUM 10/09/2018
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }

                                    AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader[3].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                    AjoutMessageListes("Dépendance entre " + reader[3].ToString() + " et " + reader[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                }
                                reader.Close();
                            }

                            // MDH 19/06/2015 - Recherche de la livraison d'un item dans un pack et de la NS de l'item dans un autre pack.
                            // MHUM 22/05/2018 - Je précise les 2 éléments
                            //command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='ITEM' AND B.ID_CLASS='NODE STRUCTURE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE";
                            command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='ITEM' AND B.ID_CLASS='NODE STRUCTURE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                foreach (string[] tElem1 in pListePackTickets)
                                {
                                    if (tElem1[0] == reader[0].ToString())
                                    {
                                        foreach (string[] tElem2 in pListePackTickets)
                                        {
                                            if (tElem2[0] == reader[3].ToString())
                                            {
                                                // MHUM 22/05/2018 - Je précise les 2 éléments
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //<= MHUM 10/09/2018
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }

                                //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader[3].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader[3].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                AjoutMessageListes("Dépendance entre " + reader[3].ToString() + " et " + reader[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                            }
                            reader.Close();

                            // MDH 19/06/2015 - Recherche de la livraison d'un "FIELD" dans un pack et d'un "LOGICAL TABLE"  de l'item dans un autre pack.
                            // MHUM 22/05/2018 - Je précise les 2 éléments
                            //command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='FIELD' AND B.ID_CLASS='LOGICAL TABLE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE";
                            command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='FIELD' AND B.ID_CLASS='LOGICAL TABLE' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                foreach (string[] tElem1 in pListePackTickets)
                                {
                                    if (tElem1[0] == reader[0].ToString())
                                    {
                                        foreach (string[] tElem2 in pListePackTickets)
                                        {
                                            if (tElem2[0] == reader[3].ToString())
                                            {
                                                // MHUM 22/05/2018 - Je précise les 2 éléments
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //<= MHUM 10/09/2018
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader[3].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader[3].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                AjoutMessageListes("Dépendance entre " + reader[3].ToString() + " et " + reader[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                            }
                            reader.Close();

                            // MHUM 17/10/206 - Recherche de la livraison d'un noeud dans un pack et du Meta4Objet dans un autre pack.
                            // MHUM 22/05/2018 - Je précise les 2 éléments
                            //command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='NODE' AND B.ID_CLASS='META4OBJECT' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE";
                            command.CommandText = "SELECT A.ID_PACKAGE, B.ID_CLASS, B.ID_OBJECT, B.ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT1, A.ID_CLASS AS ID_CLASS1 FROM M4RDL_PACK_CMDS A, M4RDL_PACK_CMDS B WHERE A.ID_CLASS='NODE' AND B.ID_CLASS='META4OBJECT' AND B.ID_OBJECT = LEFT(A.ID_OBJECT,INSTR(A.ID_OBJECT,'.')-1) AND A.ID_PACKAGE <> B.ID_PACKAGE  AND A.CMD_ACTIVE = -1 AND B.CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                foreach (string[] tElem1 in pListePackTickets)
                                {
                                    if (tElem1[0] == reader[0].ToString())
                                    {
                                        foreach (string[] tElem2 in pListePackTickets)
                                        {
                                            if (tElem2[0] == reader[3].ToString())
                                            {
                                                // MHUM 22/05/2018 - Je précise les 2 éléments
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + " / " + reader[5].ToString() + ";" + reader[2].ToString() + " / " + reader[4].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + reader[5].ToString() + " / " + reader[1].ToString() + ";" + reader[4].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                //<= MHUM 10/09/2018
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader[3].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader[3].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                AjoutMessageListes("Dépendance entre " + reader[3].ToString() + " et " + reader[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                            }
                            reader.Close();

                            // Recherche des dépendances dans les autres mdb.
                            command.CommandText = "select ID_PACKAGE, ID_CLASS, ID_OBJECT FROM M4RDL_PACK_CMDS WHERE ID_PACKAGE LIKE '%_L' OR ID_PACKAGE LIKE '%_B' AND CMD_ACTIVE = -1";
                            reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int j = 0; j < dgvListePacks.SelectedRows.Count; j++)
                                {
                                    if (i != j)
                                    {
                                        string sCheminMDB2 = dgvListePacks.SelectedRows[j].Cells[0].Value.ToString();
                                        sConnection2 = GereAccess.GetConnectionStringMDB(sCheminMDB2);
                                        using (OdbcConnection connection2 = new OdbcConnection(sConnection2))
                                        {
                                            OdbcCommand command2 = connection2.CreateCommand();

                                            command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS = '" + reader[1].ToString() + "' AND A.ID_OBJECT = '" + reader[2].ToString() + "' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                            try
                                            {
                                                connection2.Open();

                                                OdbcDataReader reader2 = command2.ExecuteReader();

                                                while (reader2.Read())
                                                {
                                                    foreach (string[] tElem1 in pListePackTickets)
                                                    {
                                                        if (tElem1[0] == reader[0].ToString())
                                                        {
                                                            foreach (string[] tElem2 in pListePackTickets)
                                                            {
                                                                if (tElem2[0] == reader2[0].ToString())
                                                                {
                                                                    //MHUM 14/06/2018 - Pas de dépendance si c'est la présentation du BP électronique
                                                                    if ((reader[1].ToString() == "PRESENTATION") && (reader[2].ToString().IndexOf("DP_PAYROLL_CHANNEL") > 0))
                                                                        break;
                                                                    // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                    //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                    //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                    //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                    swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                    //<= MHUM 10/09/2018
                                                                    // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                    // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                    //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                    break;
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    } 
                                                    //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                    AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                }
                                                reader2.Close();
                                                connection2.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 +") - Erreur d'exécution (exception) : " + ex.Message);
                                                return -999;
                                            }

                                            
                                            // MDH 19/06/2015 - Recherche dépendance ITEM -> NODE STRUCTURE
                                            if (reader[1].ToString() == "ITEM")
                                            {
                                                string sNodeStructure = reader[2].ToString().Substring(0,reader[2].ToString().IndexOf("."));

                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'NODE STRUCTURE' AND A.ID_OBJECT = '" + sNodeStructure + "' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        }
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                               
                                                   
                                                    // MHUM le 09/07/2018 - Recherche dépendance ITEM -> PAYROLL ITEM
                                                    command2.CommandText = "select A.ID_PACKAGE, A.ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A, M4RCH_PICOMPONENTS P WHERE A.ID_CLASS = 'PAYROLL ITEM' AND P.ID_T3 + '.' + P.ID_PAYROLL_ITEM= A.ID_OBJECT AND P.ID_TI + '.' + P.ID_ITEM = '" + reader[2].ToString() + "' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                    connection2.Open();
                                                    
                                                     // MHUM 06/08/2018 - Ajout test existance table M4RCH_PICOMPONENTS
                                                    if (CtrlExisteTableMDB(connection2, "M4RCH_PICOMPONENTS") == true)
                                                    {
                                                        reader2 = command2.ExecuteReader();

                                                        while (reader2.Read())
                                                        {
                                                            foreach (string[] tElem1 in pListePackTickets)
                                                            {
                                                                if (tElem1[0] == reader[0].ToString())
                                                                {
                                                                    foreach (string[] tElem2 in pListePackTickets)
                                                                    {
                                                                        if (tElem2[0] == reader2[0].ToString())
                                                                        {
                                                                            // MHUM le 09/07/2018 - Je précise les 2 éléments
                                                                            //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                            //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                            swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                            //<= MHUM 10/09/2018
                                                                            break;
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                            AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                        }
                                                        reader2.Close();
                                                    }
                                                    connection2.Close();


                                                    
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }
                                            else if (reader[1].ToString() == "NODE STRUCTURE")
                                            {
                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'ITEM' AND A.ID_OBJECT LIKE '" + reader[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        } 
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }

                                            // MDH 19/06/2015 - Recherche dépendance FIELD -> LOGICAL TABLE
                                            if (reader[1].ToString() == "FIELD")
                                            {
                                                string sTable = reader[2].ToString().Substring(0, reader[2].ToString().IndexOf("."));

                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'LOGICAL TABLE' AND A.ID_OBJECT = '" + sTable + "' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        } 
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }
                                            else if (reader[1].ToString() == "LOGICAL TABLE")
                                            {
                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'FIELD' AND A.ID_OBJECT LIKE '" + reader[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        } 
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }

                                            // MHUM 17/10/2016 - Recherche dépendance NODE -> META4OBJECT
                                            if (reader[1].ToString() == "NODE")
                                            {
                                                string sNode = reader[2].ToString().Substring(0, reader[2].ToString().IndexOf("."));

                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'META4OBJECT' AND A.ID_OBJECT = '" + sNode + "' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        } 
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }
                                            else if (reader[1].ToString() == "META4OBJECT")
                                            {
                                                command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'NODE' AND A.ID_OBJECT LIKE '" + reader[2].ToString() + ".%' AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                try
                                                {
                                                    connection2.Open();

                                                    OdbcDataReader reader2 = command2.ExecuteReader();

                                                    while (reader2.Read())
                                                    {
                                                        foreach (string[] tElem1 in pListePackTickets)
                                                        {
                                                            if (tElem1[0] == reader[0].ToString())
                                                            {
                                                                foreach (string[] tElem2 in pListePackTickets)
                                                                {
                                                                    if (tElem2[0] == reader2[0].ToString())
                                                                    {
                                                                        // MHUM 22/05/2018 - Je précise les 2 éléments
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                        //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                        //<= MHUM 10/09/2018
                                                                        // MHUM 22/05/2018 - Ajout de la dépendance dans l'autre sens
                                                                        // MHUM 09/07/2018 - Finalement je ne garde que ce qui concerne le mdb en cours
                                                                        //swFichierDependance.WriteLine(tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + reader2[1].ToString() + "/" + reader[1].ToString() + ";" + reader2[2].ToString() + " / " + reader[2].ToString() + ";" + Path.GetFileName(sCheminMDB));
                                                                        break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        } 
                                                        //swFichierDependance.WriteLine(reader[0].ToString() + ";" + reader2[0].ToString() + ";" + reader[1].ToString() + ";" + reader[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                        AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                    }
                                                    reader2.Close();
                                                    connection2.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                    return -999;
                                                }

                                            }

                                            // MHUM le 09/07/2018 - Gestion de la livraison d'un payroll item dans un pack et d'un item dans un autre pack d'un autre mdb
                                            if (reader[1].ToString() == "PAYROLL ITEM")
                                            {
                                                // Il faut récupérer les composant du payroll item
                                                OdbcCommand command3 = connection.CreateCommand();
                                                OdbcDataReader reader3 = null;
                                                string sListeComposant = string.Empty;

                                                command3.CommandText = "select ID_TI +'.' + ID_ITEM FROM M4RCH_PICOMPONENTS WHERE ID_T3 + '.' + ID_PAYROLL_ITEM = '" + reader[2].ToString() + "'";
                                                reader3 = command3.ExecuteReader();
                                                while (reader3.Read())
                                                {
                                                    if (sListeComposant == string.Empty)
                                                        sListeComposant = "('" + reader3[0].ToString() + "'";
                                                    else
                                                        sListeComposant += ",'" + reader3[0].ToString() + "'";
                                                }
                                                reader3.Close();
                                                
                                                if (sListeComposant != string.Empty)
                                                {

                                                    sListeComposant += ")";

                                                    command2.CommandText = "select ID_PACKAGE, ID_CLASS, A.ID_OBJECT FROM M4RDL_PACK_CMDS A WHERE A.ID_CLASS = 'ITEM' AND A.ID_OBJECT IN " + sListeComposant + " AND A.ID_PACKAGE <> '" + reader[0].ToString() + "' AND A.CMD_ACTIVE = -1";
                                                    try
                                                    {
                                                        connection2.Open();

                                                        OdbcDataReader reader2 = command2.ExecuteReader();

                                                        while (reader2.Read())
                                                        {
                                                            foreach (string[] tElem1 in pListePackTickets)
                                                            {
                                                                if (tElem1[0] == reader[0].ToString())
                                                                {
                                                                    foreach (string[] tElem2 in pListePackTickets)
                                                                    {
                                                                        if (tElem2[0] == reader2[0].ToString())
                                                                        {
                                                                            //=> MHUM 10/09/2018 - Ajout du libellé du ticket
                                                                            //swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                            swFichierDependance.WriteLine(tElem1[0] + ";" + tElem1[1] + ";" + tElem1[3] + ";" + tElem1[2] + ";" + tElem2[0] + ";" + tElem2[1] + ";" + tElem2[3] + ";" + tElem2[2] + ";" + reader[1].ToString() + "/" + reader2[1].ToString() + ";" + reader[2].ToString() + " / " + reader2[2].ToString() + ";" + Path.GetFileName(dgvListePacks.SelectedRows[j].Cells[0].Value.ToString()));
                                                                            //<= MHUM 10/09/2018
                                                                            break;
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                            AjoutMessageListes("Dépendance entre " + reader[0].ToString() + " et " + reader2[0].ToString() + " sur l'élément " + reader[1].ToString() + " " + reader[2].ToString());
                                                        }
                                                        reader2.Close();
                                                        connection2.Close();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        lbErreurs.Items.Add("ControleDependances (" + sCheminMDB2 + ") - Erreur d'exécution (exception) : " + ex.Message);
                                                        return -999;
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            reader.Close(); 
                            connection.Close();

                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleDependances (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message); 
                            return -999;
                        }

                    }

                    // Recherche des dépendances dans les autres mdb.
                    
                    swFichierDependance.Close();
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleDependances - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }
            }
            return iResultat;
        }

        //--------------------------------------------------------------------
        // Contrôle de la présence de la liste des tables dans le cas de création d'une table
        //
        private int ControleCatalogueTables(SqlConnection sqlConn, string sEnvironnement)
        {
            string sConnection;
            string sCheminMDB;
            List<string> lListeTables = new List<string>();
            string sNomFichierActions;
            StreamWriter swFichierActions;
            string sRequeteControle = string.Empty;
            List<string[]> lListeAControler = new List<string[]>();
            bool bPremierElement;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;


            AjoutMessageListes("Contrôle du catalogue des tables",3);
            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    AjoutMessageListes("Traitement du fichier " + sCheminMDB,1);

                    
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();
                        OdbcDataReader reader;
                        OdbcCommand command2 = connection.CreateCommand();
                        OdbcDataReader reader2;

                        
                        try
                        {
                            if (CtrlExisteTableMDB(connection, "M4RDC_LOGIC_OBJECT") == true)
                            {

                                if (tbDossierResultat.Text != string.Empty)
                                    sNomFichierActions = tbDossierResultat.Text + "\\";
                                else
                                    sNomFichierActions = string.Empty;

                                // => MHUM le 06/02/2019 - Gestion clients désynchro
                                if (sEnvironnement != string.Empty)
                                    sNomFichierActions += sEnvironnement + "\\RD\\";
                                // <= MHUM le 06/02/2019

                                sNomFichierActions += Path.GetFileNameWithoutExtension(sCheminMDB) + "_ACT.CSV";
                                swFichierActions = new StreamWriter(sNomFichierActions, true, Encoding.UTF8);

                                connection.Open();

                                // Recherche des tables livrées dans le mdb.

                                command.CommandText = "select A.ID_PACKAGE, A.ID_OBJECT,B.REAL_NAME FROM M4RDL_PACK_CMDS A INNER JOIN M4RDC_LOGIC_OBJECT B ON (A.ID_OBJECT = B.ID_OBJECT) ";
                                //command.CommandText += "WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS = 'LOGICAL TABLE' AND A.CMD_ACTIVE = -1 AND A.CMD_COMMENTS LIKE 'NEW%' AND (B.ID_OBJECT_TYPE = 1 OR B.ID_OBJECT_TYPE = 3) AND ID_ORG_TYPE <> 1";
                                command.CommandText += "WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS = 'LOGICAL TABLE' AND A.CMD_ACTIVE = -1 AND (B.ID_OBJECT_TYPE = 1 OR B.ID_OBJECT_TYPE = 3) AND ID_ORG_TYPE <> 1";
                                reader = command.ExecuteReader();

                                sRequeteControle = "SELECT ID_OBJECT FROM M4RDC_LOGIC_OBJECT WHERE ID_OBJECT IN (";
                                bPremierElement = true;

                                while (reader.Read())
                                {
                                    command2.CommandText = "select * from M4RDL_PACK_CMDS WHERE CMD_CODE LIKE '%M4CFR_X_DATA_TABLES%' + CHR(39) + '" + reader["REAL_NAME"].ToString() + "' + CHR(39) + '%' OR CMD_CODE LIKE '%M4CFR_X_DATA_TABLES%' + CHR(39) + '" + reader["ID_OBJECT"].ToString() + "' + CHR(39) + '%'"; 
                                    reader2 = command2.ExecuteReader();

                                    if (reader2.HasRows == false)
                                    {
                                        lListeAControler.Add(new string[] { reader["ID_PACKAGE"].ToString(), reader["ID_OBJECT"].ToString() });
                                        if (bPremierElement == true)
                                            bPremierElement = false;
                                        else
                                            sRequeteControle += ",";
                                        sRequeteControle += "'" + reader["ID_OBJECT"].ToString() + "'";
                                    }
                                    else
                                        swFichierActions.WriteLine("LISTE TABLES;" + reader["ID_PACKAGE"].ToString() + ";" + reader["ID_OBJECT"].ToString());

                                    reader2.Close();
                                }
                                reader.Close();

                                sRequeteControle += ")";

                                if (bPremierElement == false) // il y a au moins une table
                                {
                                    sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read()) // Traitement des tables trouvées sur la base de référence
                                    {
                                        foreach (string[] t in lListeAControler)
                                        {
                                            if (t[1] == sqlDR["ID_OBJECT"].ToString()) // On trouve le code table
                                                t[0] = "Traité"; // Indique l'enregistrement traité

                                        }
                                    }
                                    sqlDR.Close();
                                    foreach (string[] t in lListeAControler)
                                    {
                                        if (t[0] != "Traité")
                                        {
                                            lbErreurs.Items.Add(sCheminMDB + " : Livraison de la table " + t[1] + " dans le pack " + t[0] + " sans mise à jour du catalogue des tables.");
                                        }
                                    }

                                }

                                swFichierActions.Close();
                            }
                            else
                            {
                                lbActionLocalisation.Items.Add("Avertissement : table M4RDC_LOGIC_OBJECT inexistante dans le MDB. Contrôle des sécurités des concepts impossible.");
                            }

                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleCatalogueTables (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }
                    }

                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleCatalogueTables - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }

            }
            return 0;
        }

        //--------------------------------------------------------------------
        // Contrôle des Meta4 objets modifiés par les packs
        //
        private int ControleTousM4OModifies(SqlConnection sqlConn)
        {
            string sConnection;
            string sCheminMDB;
            string sIDPackageCourant;
            List<string> lListeM4O = new List<string>();
            List<string> lListeNODESTRUCTURE = new List<string>();
            string sTempo;
            StreamWriter swFichierMeta4Objets;
            string sNomFichierMeta4Objets;
            SortedDictionary<string, string> dOwnerFlag = new SortedDictionary<string, string>();
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            string sRequeteControle;

            try
            {
                // Chargement de la liste des owner flag
                sRequeteControle = "select ID_OWNER_FLAG,ISNULL(N_OWNER_FLAGFRA,N_OWNER_FLAGENG) AS LIB from M4RDC_LU_OWNR_FLG";
                sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                sqlDR = sqlComm.ExecuteReader();
                while (sqlDR.Read())
                {
                    dOwnerFlag.Add(sqlDR["ID_OWNER_FLAG"].ToString(), sqlDR["LIB"].ToString());
                }
                sqlDR.Close();

                AjoutMessageListes("Liste des Meta4 objets modifiés par les packs", 3);
                for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
                {
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    if (tbDossierResultat.Text != string.Empty)
                        sNomFichierMeta4Objets = tbDossierResultat.Text + "\\";
                    else
                        sNomFichierMeta4Objets = string.Empty;

                    sNomFichierMeta4Objets += Path.GetFileNameWithoutExtension(sCheminMDB) + "_M4O.CSV";
                    swFichierMeta4Objets = new StreamWriter(sNomFichierMeta4Objets, false, Encoding.UTF8);
                    swFichierMeta4Objets.WriteLine("Pack;Code M4O;Libellé M4O;Owner flag;Livré dans le mdb");

                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();
                        OdbcDataReader reader;

                        try
                        {
                            connection.Open();

                            // Recherche des tables livrées dans le mdb.
                            command.CommandText = "select A.ID_PACKAGE, A.ID_CLASS,A.ID_OBJECT FROM M4RDL_PACK_CMDS A ";
                            command.CommandText += "WHERE (A.ID_PACKAGE LIKE '%_L' OR A.ID_PACKAGE LIKE '%_B') AND A.ID_CLASS IN ('META4OBJECT','NODE STRUCTURE','NODE','ITEM') AND A.CMD_ACTIVE = -1 ";
                            command.CommandText += "ORDER BY ID_PACKAGE ";
                            reader = command.ExecuteReader();
                            sIDPackageCourant = String.Empty;
                            
                            while (reader.Read())
                            {
                                if (reader["ID_PACKAGE"].ToString() != sIDPackageCourant)
                                {
                                    if ((sIDPackageCourant != String.Empty) && (lListeM4O.Count + lListeNODESTRUCTURE.Count > 0))
                                    {
                                        ControleM4OModifiesPack(sqlConn, connection, lListeM4O, lListeNODESTRUCTURE, sIDPackageCourant, swFichierMeta4Objets,sCheminMDB,dOwnerFlag);
                                        lListeM4O.Clear();
                                        lListeNODESTRUCTURE.Clear();
                                    }
                                    sIDPackageCourant = reader["ID_PACKAGE"].ToString();
                                }
                                switch (reader["ID_CLASS"].ToString())
                                {
                                    case "META4OBJECT": if (lListeM4O.Contains(reader["ID_OBJECT"].ToString()) == false)
                                                            lListeM4O.Add(reader["ID_OBJECT"].ToString());
                                                        break;

                                    case "NODE":    sTempo = reader["ID_OBJECT"].ToString();
                                                    sTempo = sTempo.Substring(0, sTempo.IndexOf('.'));
                                                    if (lListeM4O.Contains(sTempo) == false)
                                                        lListeM4O.Add(sTempo);
                                                        break;

                                    case "NODE STRUCTURE": if (lListeNODESTRUCTURE.Contains(reader["ID_OBJECT"].ToString()) == false)
                                                            lListeNODESTRUCTURE.Add(reader["ID_OBJECT"].ToString());
                                                        break;

                                    case "ITEM":    sTempo = reader["ID_OBJECT"].ToString();
                                                    sTempo = sTempo.Substring(0,sTempo.IndexOf('.'));
                                                    if (lListeNODESTRUCTURE.Contains(sTempo) == false)
                                                        lListeNODESTRUCTURE.Add(sTempo);
                                                        break;
                                }
                                
                            }
                            if (lListeM4O.Count + lListeNODESTRUCTURE.Count > 0)
                            {
                                ControleM4OModifiesPack(sqlConn, connection, lListeM4O, lListeNODESTRUCTURE, sIDPackageCourant, swFichierMeta4Objets,sCheminMDB,dOwnerFlag);
                                lListeM4O.Clear();
                                lListeNODESTRUCTURE.Clear();
                            }

                            reader.Close();
                            connection.Close();
                            swFichierMeta4Objets.Close();
                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleTousM4OModifies (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                lbErreurs.Items.Add("ControleTousM4OModifies - Erreur d'exécution (exception) : " + ex.Message);
                return -999;
            }
            return 0;
        }

        private int ControleM4OModifiesPack(SqlConnection sqlConn, OdbcConnection connection, List<string> lListeM4O, List<string> lListeNODESTRUCTURE, string sIDPack, StreamWriter swFichierMeta4Objets, string sCheminMDB, SortedDictionary<string, string> dOwnerFlag)
        {
            OdbcCommand command = connection.CreateCommand();
            OdbcDataReader reader;
            bool bPremier = true;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            string sRequeteControle;
            string sBuffer;
            try
            {
                if (lListeNODESTRUCTURE.Count > 0)
                {
                    if ((CtrlExisteTableMDB(connection, "M4RCH_NODES") == true) && (CtrlExisteTableMDB(connection, "M4RCH_OVERWRITE_NO") == true))
                    {
                        command.CommandText = "select ID_T3,ID_TI from M4RCH_NODES where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                command.CommandText += ",";

                            command.CommandText += "'" + s + "'";
                        }
                        command.CommandText += ") UNION select ID_NODE_T3 AS ID_T3,ID_TI from M4RCH_OVERWRITE_NO where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                command.CommandText += ",";

                            command.CommandText += "'" + s + "'";
                        }
                        command.CommandText += ")";
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            lListeNODESTRUCTURE.Remove(reader["ID_TI"].ToString()); // Je supprime les NS dont on trouve le M4O dans le MDB
                            if (lListeM4O.Contains(reader["ID_T3"].ToString()) == false)
                                lListeM4O.Add(reader["ID_T3"].ToString());
                        }
                        reader.Close();
                    }
                    // Il reste des nodes structures non trouvées dans le MDB. On cherche sur la base SQL.
                    if (lListeNODESTRUCTURE.Count > 0)
                    {
                        sRequeteControle = "select ID_T3,ID_TI from M4RCH_NODES where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                sRequeteControle += ",";

                            sRequeteControle += "'" + s + "'";
                        }
                        sRequeteControle += ") UNION select ID_NODE_T3 AS ID_T3,ID_TI from M4RCH_OVERWRITE_NO where ID_TI IN (";
                        bPremier = true;
                        foreach (string s in lListeNODESTRUCTURE)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                sRequeteControle += ",";

                            sRequeteControle += "'" + s + "'";
                        }
                        sRequeteControle += ")";
                        sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                        sqlDR = sqlComm.ExecuteReader();
                        while (sqlDR.Read())
                        {
                            if (lListeM4O.Contains(sqlDR["ID_T3"].ToString()) == false)
                                lListeM4O.Add(sqlDR["ID_T3"].ToString());
                        }
                        sqlDR.Close();
                    }
                }

                if (lListeM4O.Count > 0)
                {
                    if (CtrlExisteTableMDB(connection, "M4RCH_T3S") == true)
                    {
                        // Recherche des M4O directement dans le MDB
                        // MHUM 01/09/2017 - Ajout du contrôle si l'objet est hérité 
                        command.CommandText = "select ID_T3, N_T3FRA, OWNER_FLAG from M4RCH_T3S where ID_T3 IN (";
                        //command.CommandText = "select [M4RCH_T3S].[ID_T3], [M4RCH_T3S].[N_T3FRA], [M4RCH_T3S].[OWNER_FLAG], [M4RCH_T3_INHERIT].[ID_T3_BASE], [M4RDM_OS_PROJ_MEMS].[ID_PROJECT] from [M4RCH_T3S] LEFT JOIN [M4RCH_T3_INHERIT] ON [M4RCH_T3_INHERIT].[ID_T3]=[M4RCH_T3S].[ID_T3] AND [M4RCH_T3_INHERIT].[ID_T3_BASE]<>[M4RCH_T3S].[ID_T3] LEFT JOIN [M4RDM_OS_PROJ_MEMS] ON [M4RDM_OS_PROJ_MEMS].[ID_INSTANCE]=[M4RCH_T3S].[ID_T3] AND [M4RDM_OS_PROJ_MEMS].[ID_CLASS]='DIN_OBJECT' where [M4RCH_T3S].[ID_T3] IN (";
                        // MHUM 01/09/2017 - Ajout du contrôle si l'objet est hérité 
                        bPremier = true;
                        foreach (string s in lListeM4O)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                command.CommandText += ",";

                            command.CommandText += "'" + s + "'";
                        }
                        command.CommandText += ")";
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            lListeM4O.Remove(reader["ID_T3"].ToString()); // Je supprime les M4O trouvés dans le MDB
                            sBuffer = reader["OWNER_FLAG"].ToString();
                            try
                            {
                                sBuffer = dOwnerFlag[sBuffer].ToString() + " - (" + sBuffer + ")";
                            }
                            catch (Exception ex)
                            {
                                sBuffer = reader["OWNER_FLAG"].ToString();
                            }
                            swFichierMeta4Objets.WriteLine(sIDPack + ";" + reader["ID_T3"].ToString() + ";" + reader["N_T3FRA"].ToString() + ";" + sBuffer +";Oui");
                            // Controle si c'est un objet techno
                            if ((reader["OWNER_FLAG"].ToString() =="1") || (reader["ID_T3"].ToString().Substring(0,4) == "SRTC"))
                                lbErreurs.Items.Add(sCheminMDB + " : Modification de l'objet techno " + reader["ID_T3"].ToString() +" dans le pack "+ sIDPack +".");
                        }
                        reader.Close();
                    }
                    // S'il reste des Meta4 Objets non trouvés dans le mdb, on cherche sur la base SQL
                    if (lListeM4O.Count > 0)
                    {
                        // MHUM 01/09/2017 - Ajout du contrôle si l'objet est hérité 
                        sRequeteControle = "select ID_T3, N_T3FRA, OWNER_FLAG from M4RCH_T3S where ID_T3 IN (";
                        //sRequeteControle = "select A.ID_T3, A.N_T3FRA, A.OWNER_FLAG, B.ID_T3_BASE, C.ID_PROJECT from M4RCH_T3S AS A LEFT JOIN M4RCH_T3_INHERIT AS B ON (B.ID_T3=A.ID_T3 AND B.ID_T3_BASE<>A.ID_T3) LEFT JOIN M4RDM_OS_PROJ_MEMS AS C ON (C.ID_INSTANCE=A.ID_T3 AND C.ID_CLASS='DIN_OBJECT') where A.ID_T3 IN (";
                        // FIN MHUM 01/09/2017 - Ajout du contrôle si l'objet est hérité 
                        bPremier = true;
                        foreach (string s in lListeM4O)
                        {
                            if (bPremier == true)
                                bPremier = false;
                            else
                                sRequeteControle += ",";

                            sRequeteControle += "'" + s + "'";
                        }
                        sRequeteControle += ")";
                        sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                        sqlDR = sqlComm.ExecuteReader();
                        while (sqlDR.Read())
                        {
                            sBuffer = sqlDR["OWNER_FLAG"].ToString();
                            try
                            {
                                sBuffer = dOwnerFlag[sBuffer].ToString() + " - (" + sBuffer + ")";
                            }
                            catch (Exception ex)
                            {
                                sBuffer = sqlDR["OWNER_FLAG"].ToString();
                            }
                            swFichierMeta4Objets.WriteLine(sIDPack + ";" + sqlDR["ID_T3"].ToString() + ";" + sqlDR["N_T3FRA"].ToString() + ";" + sBuffer + ";Non"); 
                            // Controle si c'est un objet techno
                            if ((sqlDR["OWNER_FLAG"].ToString() == "1") || (sqlDR["ID_T3"].ToString().Substring(0, 4) == "SRTC"))
                                lbErreurs.Items.Add(sCheminMDB + " : Modification de l'objet techno " + sqlDR["ID_T3"].ToString() + " dans le pack " + sIDPack + ".");
                        }
                        sqlDR.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                lbErreurs.Items.Add("ControleM4OModifiesPack - Erreur d'exécution (exception) : " + ex.Message);
                return -999;
            }
            return 0;
        }

        // MHUM le 11/07/2018
        //--------------------------------------------------------------------
        // Contrôle du changement des niveaux de saisie des items
        //
        private int ControleNiveauxSaisieItems(SqlConnection sqlConn, string sEnvironnement)
        {
            string sConnection;
            string sCheminMDB;
            string sNomFichierNiveauSaisie;
            StreamWriter swFichierNiveauSaisie;
            string sRequeteControle = string.Empty;
            List<string[]> lListeITEMS = new List<string[]>();
            bool bPremierElement;
            bool bItemTrouve;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;


            AjoutMessageListes("Contrôle des niveaux de saisie", 3);
            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);

                    AjoutMessageListes("Traitement du fichier " + sCheminMDB, 1);


                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();
                        OdbcDataReader reader;

                        try
                        {
                            if (tbDossierResultat.Text != string.Empty)
                                sNomFichierNiveauSaisie = tbDossierResultat.Text + "\\";
                            else
                                sNomFichierNiveauSaisie = string.Empty;

                            // => MHUM le 06/02/2019 - Gestion clients désynchro
                            if (sEnvironnement != string.Empty)
                                sNomFichierNiveauSaisie += sEnvironnement + "\\RD\\";
                            // <= MHUM le 06/02/2019


                            sNomFichierNiveauSaisie += Path.GetFileNameWithoutExtension(sCheminMDB) + "_SAIS.CSV";
                            swFichierNiveauSaisie = new StreamWriter(sNomFichierNiveauSaisie, false, Encoding.UTF8);
                            swFichierNiveauSaisie.WriteLine("Pack(s);Item;ID_DMD_COMPONENT;ID_DMD_GROUP Pack;ID_DMD_GROUP Base");

                            connection.Open();

                            //MHUM le 07/12/2018 - La table M4RCH_ITEMS n'est pas forcémernt présente dans le mdb
                            if (CtrlExisteTableMDB(connection, "M4RCH_ITEMS") == true)
                            {
                                // Recherche des items livrés dans le mdb.

                                command.CommandText = "SELECT A.ID_PACKAGE AS ID_PACKAGE, A.ID_OBJECT AS ID_OBJECT, B.ID_DMD_COMPONENT AS ID_DMD_COMPONENT FROM M4RDL_PACK_CMDS A, M4RCH_ITEMS B WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS='ITEM' AND (A.ID_OBJECT LIKE '%HRPERIOD_CALC.%' OR A.ID_OBJECT LIKE '%HRROLE_CALC.%') AND A.CMD_ACTIVE = -1 AND B.ID_TI + '.' + B.ID_ITEM = A.ID_OBJECT ";
                                reader = command.ExecuteReader();

                                sRequeteControle = "SELECT ID_DMD_COMPONENT, ID_DMD_GROUP FROM M4RCH_DMD_GRP_CMP WHERE ID_DMD_COMPONENT IN (";
                                bPremierElement = true;

                                while (reader.Read())
                                {
                                    if (bPremierElement == true)
                                    {
                                        bPremierElement = false;
                                    }
                                    else
                                    {
                                        sRequeteControle += ",";
                                    }


                                    // MHUM le 09/08/2018 - Gestion du cas où il n'y a pas de saisie paramétrée, donc pas de DMD_COMPONENT
                                    if (reader["ID_DMD_COMPONENT"].ToString() != string.Empty)
                                        sRequeteControle += "'" + reader["ID_DMD_COMPONENT"].ToString() + "'";
                                    else
                                        sRequeteControle += "'" + reader["ID_OBJECT"].ToString().Substring(reader["ID_OBJECT"].ToString().LastIndexOf(".") + 1) + "'";


                                    bItemTrouve = false;
                                    for (int elt = 0; elt < lListeITEMS.Count && bItemTrouve == false; elt++)
                                    {
                                        if (lListeITEMS[elt][0] == reader["ID_OBJECT"].ToString())
                                        {
                                            lListeITEMS[elt][1] += " / " + reader["ID_PACKAGE"].ToString();
                                            bItemTrouve = true;
                                        }
                                    }
                                    if (bItemTrouve == false)
                                    {
                                        // MHUM le 09/08/2018 - Gestion du cas où il n'y a pas de saisie paramétrée, donc pas de DMD_COMPONENT
                                        string sDMD_COMPONENT = string.Empty;


                                        if (reader["ID_DMD_COMPONENT"].ToString() != string.Empty)
                                            sDMD_COMPONENT = reader["ID_DMD_COMPONENT"].ToString();
                                        else
                                            sDMD_COMPONENT = reader["ID_OBJECT"].ToString().Substring(reader["ID_OBJECT"].ToString().LastIndexOf(".") + 1);

                                        lListeITEMS.Add(new string[] { reader["ID_OBJECT"].ToString(), reader["ID_PACKAGE"].ToString(), sDMD_COMPONENT, string.Empty, string.Empty });
                                    }
                                }
                                reader.Close();

                                // MHUM le 12/11/2018 - Déplacement du contrôle de l'existence de la table. Elle n'est utilisée qu'ici
                                if (CtrlExisteTableMDB(connection, "M4RCH_PICOMPONENTS") == true)
                                {
                                    command.CommandText = "SELECT A.ID_PACKAGE AS ID_PACKAGE, B.ID_TI + '.' + B.ID_ITEM AS ID_OBJECT, B.ID_DMD_COMPONENT AS ID_DMD_COMPONENT FROM M4RDL_PACK_CMDS A, M4RCH_ITEMS B, M4RCH_PICOMPONENTS C WHERE A.ID_PACKAGE LIKE '%_L' AND A.ID_CLASS='PAYROLL ITEM' AND A.CMD_ACTIVE = -1 AND C.ID_T3 + '.' + C.ID_PAYROLL_ITEM = A.ID_OBJECT AND B.ID_TI =C.ID_TI AND B.ID_ITEM=C.ID_ITEM";
                                    reader = command.ExecuteReader();

                                    while (reader.Read())
                                    {
                                        if (bPremierElement == true)
                                        {
                                            bPremierElement = false;
                                        }
                                        else
                                        {
                                            sRequeteControle += ",";
                                        }
                                        // MHUM le 09/08/2018 - Gestion du cas où il n'y a pas de saisie paramétrée, donc pas de DMD_COMPONENT
                                        if (reader["ID_DMD_COMPONENT"].ToString() != string.Empty)
                                            sRequeteControle += "'" + reader["ID_DMD_COMPONENT"].ToString() + "'";
                                        else
                                            sRequeteControle += "'" + reader["ID_OBJECT"].ToString().Substring(reader["ID_OBJECT"].ToString().LastIndexOf(".") + 1) + "'";


                                        bItemTrouve = false;
                                        for (int elt = 0; elt < lListeITEMS.Count && bItemTrouve == false; elt++)
                                        {
                                            if (lListeITEMS[elt][0] == reader["ID_OBJECT"].ToString())
                                            {
                                                lListeITEMS[elt][1] += " / " + reader["ID_PACKAGE"].ToString();
                                                bItemTrouve = true;
                                            }
                                        }
                                        if (bItemTrouve == false)
                                        {
                                            // MHUM le 09/08/2018 - Gestion du cas où il n'y a pas de saisie paramétrée, donc pas de DMD_COMPONENT
                                            string sDMD_COMPONENT = string.Empty;


                                            if (reader["ID_DMD_COMPONENT"].ToString() != string.Empty)
                                                sDMD_COMPONENT = reader["ID_DMD_COMPONENT"].ToString();
                                            else
                                                sDMD_COMPONENT = reader["ID_OBJECT"].ToString().Substring(reader["ID_OBJECT"].ToString().LastIndexOf(".") + 1);

                                            lListeITEMS.Add(new string[] { reader["ID_OBJECT"].ToString(), reader["ID_PACKAGE"].ToString(), sDMD_COMPONENT, string.Empty, string.Empty });

                                        }
                                    }
                                    reader.Close();
                                }
                                if (bPremierElement == false)
                                {
                                    sRequeteControle += ") ORDER BY ID_DMD_COMPONENT, ID_DMD_GROUP";
                                    command.CommandText = sRequeteControle;
                                    reader = command.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        bItemTrouve = false;
                                        for (int elt = 0; elt < lListeITEMS.Count && bItemTrouve == false; elt++)
                                        {
                                            if (lListeITEMS[elt][2] == reader["ID_DMD_COMPONENT"].ToString())
                                            {
                                                if (lListeITEMS[elt][3] != string.Empty)
                                                    lListeITEMS[elt][3] += " - ";
                                                lListeITEMS[elt][3] += reader["ID_DMD_GROUP"].ToString();
                                                bItemTrouve = true;
                                            }
                                        }
                                    }
                                    reader.Close();

                                    sqlComm = new SqlCommand(sRequeteControle, sqlConn);
                                    sqlDR = sqlComm.ExecuteReader();
                                    while (sqlDR.Read())
                                    {
                                        bItemTrouve = false;
                                        for (int elt = 0; elt < lListeITEMS.Count && bItemTrouve == false; elt++)
                                        {
                                            if (lListeITEMS[elt][2] == sqlDR["ID_DMD_COMPONENT"].ToString())
                                            {
                                                if (lListeITEMS[elt][4] != string.Empty)
                                                    lListeITEMS[elt][4] += " - ";
                                                lListeITEMS[elt][4] += sqlDR["ID_DMD_GROUP"].ToString();
                                                bItemTrouve = true;
                                            }
                                        }
                                    }
                                    sqlDR.Close();
                                }

                                for (int elt = 0; elt < lListeITEMS.Count; elt++)
                                {
                                    if (lListeITEMS[elt][3] != lListeITEMS[elt][4])
                                    {
                                        if (lListeITEMS[elt][3] == string.Empty) lListeITEMS[elt][3] = "Aucun";
                                        if (lListeITEMS[elt][4] == string.Empty) lListeITEMS[elt][4] = "Aucun";

                                        swFichierNiveauSaisie.WriteLine(lListeITEMS[elt][1] + ";" + lListeITEMS[elt][0] + ";" + lListeITEMS[elt][2] + ";" + lListeITEMS[elt][3] + ";" + lListeITEMS[elt][4]);
                                    }
                                }

                                swFichierNiveauSaisie.WriteLine();
                                swFichierNiveauSaisie.WriteLine();
                                swFichierNiveauSaisie.WriteLine();
                                swFichierNiveauSaisie.WriteLine("Référentiel :");

                                sqlComm = new SqlCommand("select ID_DMD_GROUP, N_DMD_GROUPFRA from M4RCH_DMD_GROUPS where ID_DMD = 'DMD1'", sqlConn);
                                sqlDR = sqlComm.ExecuteReader();
                                while (sqlDR.Read())
                                    swFichierNiveauSaisie.WriteLine(sqlDR["ID_DMD_GROUP"].ToString() + " - " + sqlDR["N_DMD_GROUPFRA"].ToString());

                                // MHUM le 06/08/2018 - Ajout du close.
                                sqlDR.Close();

                                swFichierNiveauSaisie.Close();
                            }
                            else
                            {
                                lbActionLocalisation.Items.Add("Avertissement : table M4RCH_ITEMS inexistante dans le MDB. Contrôle des niveaux de saisie impossible.");
                            }
                        }
                        catch (Exception ex)
                        {
                            lbErreurs.Items.Add("ControleNiveauxSaisieItems (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                            return -999;
                        }
                    }

                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleNiveauxSaisieItems - Erreur d'exécution (exception) : " + ex.Message);
                    return -999;
                }

            }
            return 0;
        }

        //--------------------------------------------------------------------
        // Ajout de messages généraux dans les listes (nouveau fichier mdb, controle...)
        //
        void AjoutMessageListes(string pMessage,int pNbTab=0)
        {
            string sMessageAffiche = string.Empty;

            Application.DoEvents(); // Pour raffraichir l'affichage
            if (pNbTab > 0)
            {
                for (int i = 0; i < pNbTab; i++) sMessageAffiche += "\t";
                sMessageAffiche += "----------------------------- " + pMessage + " ------------------------------";
                lbActionLocalisation.Items.Add(string.Empty);
                lbActionLocalisation.Items.Add(sMessageAffiche);
            }
            else
                lbActionLocalisation.Items.Add(pMessage);

            
        }

        //--------------------------------------------------------------------
        // Sélection du dossier contenant les fichiers résultats
        //
        private void btnChoixDossierResultat_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = tbDossierResultat.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDossierResultat.Text = folderBrowserDialog1.SelectedPath;
            }

        }
       
        //--------------------------------------------------------------------
        // controle l'existance d'une table dans un  mdb
        //
        private bool CtrlExisteTableMDB(OdbcConnection connection, string sNomDeTable)
        {
            bool bOuvert = true;
            try
            {
                bOuvert = (connection.State == ConnectionState.Open);

                if (bOuvert == false)
                    connection.Open();

                OdbcCommand command = connection.CreateCommand();

                command.CommandText = "select TOP 1 * FROM " + sNomDeTable;
                OdbcDataReader reader = command.ExecuteReader();

                reader.Close();

                return (true);
            }
            catch (Exception ex)
            {
                return (false);
            }
            finally
            {
                if (bOuvert == false)
                    connection.Close();
            }
        }

        //--------------------------------------------------------------------
        // Gestion de la case à cocher du controle de la liste des tables en fonction
        //de la case à cocher pack standard.
        private void cbStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (cbStandard.Checked == true)
                cacControlerListeTables.Checked = true;
            else
                cacControlerListeTables.Checked = false;
        }

        //--------------------------------------------------------------------
        // Lecture des statuts des tickets associés aux tâches CCT
        //
        private void LectureStatuts(ref List<string[]> pListePackTickets)
        {
            string sCheminMDB;
            string sConnection;
            string sFiltreRequete = string.Empty;
            string sFiltreRequeteSQL = string.Empty;
            bool bPremier;
            string sChaineConnexionSQL = "server=192.168.221.1;uid=pmtalk-ro;pwd=RPV380Z2ATc8353uSTK;database=PMTalkv4_Repl;";
            SqlConnection sqlConn = null;
            string sRequeteSQL = string.Empty;
            List<string[]> lListeTachePack = new List<string[]>();
            StreamWriter swFichierStatuts = new StreamWriter(tbDossierResultat.Text + "\\StatutsTickets.csv", false, Encoding.UTF8);
            List<string> lTaches = new List<string>();
            List<string> lPacks = new List<string>();
            MatchCollection mcMatchCol = null;
            string pattern = @"_(\d{5})_"; // pour rexpression régulière pour récupérer le code du ticket à partir du nom du pack
            string pattern2 = @"_(\d{6})_"; // pour rexpression régulière pour récupérer le code du ticket à partir du nom du pack
            string sNumticket = string.Empty;
            string sStatutTicket = string.Empty;
            string sChargeAncVal;
            List<string> lTachesDiffStat = new List<string>(); // Pour stocker les taches liées à des tickets ayant des statuts différents


            try
            {
                swFichierStatuts.WriteLine("Fichier MDB;Tâche CCT;Ticket;Libelle ticket;Statut ticket");

                sqlConn = new SqlConnection(sChaineConnexionSQL);
                sqlConn.Open();
                sRequeteSQL = "SELECT A.item_id AS TICKET,A.value_text AS TACHE_CCT, B.value_choice AS STATUT, C.value_text AS PACKAGE, D.name AS LIBELLE FROM PMT_CF_values A";
                sRequeteSQL += " INNER JOIN PMT_CF_values B ON (A.item_id = B.item_id AND B.cf_id = 7 AND B.ded_id='PMTSM0001v3.00ded')";
                sRequeteSQL += " INNER JOIN PMT_CF_values C ON (A.item_id = C.item_id AND C.cf_id = 10 AND C.ded_id='PMTSM0001v3.00ded')";
                sRequeteSQL += " INNER JOIN PSM_Incident_Idx D ON (A.item_id = D.ID)";
                sRequeteSQL += "  WHERE A.cf_id = 8 AND A.ded_id='PMTSM0001v3.00ded' AND (";

                for (int i = 0; i < dgvListePacks.Rows.Count; i++)
                {
                    sFiltreRequete = string.Empty;
                    sFiltreRequeteSQL = string.Empty;
                    if (dgvListePacks.Rows[i].Selected == true)
                    {
                        sCheminMDB = dgvListePacks.Rows[i].Cells[0].Value.ToString();
                        sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                        using (OdbcConnection connection = new OdbcConnection(sConnection))
                        {
                            OdbcCommand command = connection.CreateCommand();

                            // Recherche des packs contenus dans le mdb
                            command.CommandText = "select ID_PACKAGE, CCT_TASK_ID FROM M4RDL_PACKAGES";
                            connection.Open();
                            OdbcDataReader reader = command.ExecuteReader();
                            bPremier = true;

                            while (reader.Read())
                            {
                                if (bPremier == true)
                                    bPremier = false;
                                else
                                {
                                    sFiltreRequete += ",";
                                    sFiltreRequeteSQL += " OR ";
                                }

                                sFiltreRequete += "'" + reader[0].ToString() + "'";

                                // Récupération du ticket à partir du code du pack
                                mcMatchCol = Regex.Matches(reader[0].ToString(), pattern);
                                if (mcMatchCol.Count > 0)
                                    sNumticket = reader[0].ToString().Substring(mcMatchCol[0].Index + 1, 5);
                                else
                                {
                                    // Gestion des numéros de tickets sur 6 caractères.
                                    mcMatchCol = Regex.Matches(reader[0].ToString(), pattern2);
                                    if (mcMatchCol.Count > 0)
                                        sNumticket = reader[0].ToString().Substring(mcMatchCol[0].Index + 1, 6);
                                    else
                                        sNumticket = string.Empty;
                                }

                                if (reader[1].ToString() != string.Empty) // si le code tache CCT est renseigné
                                    sFiltreRequeteSQL += "A.value_text LIKE '%" + reader[1].ToString() + "%'";
                                else // Si pas de code tache on récupère le numéro du ticket dans le code du pack
                                    sFiltreRequeteSQL += "A.item_id = '" + sNumticket + "'";

                                lListeTachePack.Add(new string[] { reader[0].ToString(), reader[1].ToString(), "N", sNumticket });
                            }
                            reader.Close();

                            SqlCommand sqlComm = new SqlCommand(sRequeteSQL + sFiltreRequeteSQL + ")", sqlConn);

                            sqlComm.CommandTimeout = 180;
                            SqlDataReader sqlDR = sqlComm.ExecuteReader();
                            if (sqlDR.HasRows == true)
                            {
                                while (sqlDR.Read())
                                {
                                    foreach (string[] tElem in lListeTachePack)
                                    {
                                        if ((sqlDR["TACHE_CCT"].ToString() != string.Empty && sqlDR["TACHE_CCT"].ToString().IndexOf(tElem[1]) >= 0) || (sqlDR["TICKET"].ToString() == string.Empty && sqlDR["TACHE_CCT"].ToString() == tElem[3]))
                                        {
                                            if (lTaches.Contains("*" + sCheminMDB + "*" + tElem[1] + "*" + sqlDR["TICKET"].ToString()) == false)
                                            {
                                                swFichierStatuts.WriteLine(sCheminMDB + ";" + tElem[1] + ";" + sqlDR["TICKET"].ToString() + ";" + sqlDR["LIBELLE"].ToString() + ";" + sqlDR["STATUT"].ToString());

                                                lTaches.Add("*" + sCheminMDB + "*" + tElem[1] + "*" + sqlDR["TICKET"].ToString());
                                            }
                                            if (lPacks.Contains("*" + sCheminMDB + "*" + tElem[0] + "*" + sqlDR["TICKET"].ToString()) == false)
                                            {
                                                //=> MHUM le 10/09/2018 - Ajout du libellé
                                                //pListePackTickets.Add(new string[] { tElem[0], sqlDR["TICKET"].ToString(), sqlDR["STATUT"].ToString() });
                                                pListePackTickets.Add(new string[] { tElem[0], sqlDR["TICKET"].ToString(), sqlDR["STATUT"].ToString(), sqlDR["LIBELLE"].ToString() });
                                                //<= MHUM le 10/09/2018
                                                lPacks.Add("*" + sCheminMDB + "*" + tElem[0] + "*" + sqlDR["TICKET"].ToString());
                                            }
                                            sChargeAncVal = tElem[2]; // Pour tester les cas où pour une tâche CCT on a plusieurs tickets avec des statuts différents.
                                            tElem[2] = "O"; // Flag pour les tâches trouvées dans les tickets
                                            sStatutTicket = sqlDR["STATUT"].ToString();


                                            // Test des tâches CCT liés à des tickets avec des statuts différents.
                                            if ((sChargeAncVal != "N") && (sChargeAncVal != tElem[2]))
                                            {
                                                if (lTachesDiffStat.IndexOf(sqlDR["TACHE_CCT"].ToString()) < 0)
                                                    lTachesDiffStat.Add(sqlDR["TACHE_CCT"].ToString());
                                            }
                                        }
                                    }

                                }
                                // Parcours pour indiquer les tâches qui n'ont pas été trouvées dans les tickets.
                                foreach (string[] tElem in lListeTachePack)
                                {
                                    if (tElem[2] == "N")
                                    {
                                        swFichierStatuts.WriteLine(sCheminMDB + ";" + tElem[1] + ";;Aucun ticket;");
                                        pListePackTickets.Add(new string[] { tElem[0], "Aucun ticket", "Pas de statut", "" });
                                    }
                                }
                            }
                            else
                            { // Ca où aucun ticket n'est trouvé pour la liste des tâches.
                                foreach (string[] tElem in lListeTachePack)
                                {
                                    swFichierStatuts.WriteLine(sCheminMDB + ";" + tElem[1] + ";;Aucun ticket;");
                                    pListePackTickets.Add(new string[] { tElem[0], "Aucun ticket", "Pas de statut","" });
                                }

                            }

                            sqlDR.Close();
                            lListeTachePack.Clear();
                        }
                    }
                }

                if (sqlConn != null)
                {
                    sqlConn.Close();
                }

                // Test des tâches CCT liés à des tickets avec des statuts différents.
                if (lTachesDiffStat.Count > 0)
                {
                    string sMessage = string.Empty;

                    foreach (string s in lTachesDiffStat)
                    {
                        if (sMessage != string.Empty)
                            sMessage += ", ";
                        sMessage += s;
                    }
                    if (lTachesDiffStat.Count == 1)
                        MessageBox.Show("Plusieurs tickets avec des statuts plateforme différents existent pour la tâche suivante : " + sMessage, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show("Plusieurs tickets avec des statuts plateforme différents existent pour les tâches suivantes :\n" + sMessage, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception dans LectureStatuts : " + ex.Message, "Erreur - Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                swFichierStatuts.Close();
            }

        }
        
//=> MHUM le 20/03/2018 - Ajout du contrôle qu'un objet ou une présentation hérité en plateforme n'est pas déja hérité en standard
        //--------------------------------------------------------------------
        // Contrôle de la présence de la liste des tables dans le cas de création d'une table
        //
        private void ControleNiveauHeritage(SqlConnection sqlConn)
        {
            string sConnection;
            string sCheminMDB;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            string sRequete = string.Empty;
            List<string[]> lObjetsHeritesSTD = new List<string[]>();
            List<string[]> lPresentsHeritesSTD = new List<string[]>();
            string sListeID_T3 = string.Empty;
            string sListeID_PRES = string.Empty;

            
            /* Récupération des M4O hérités au niveau standard */
            sRequete = "select OS.ID_T3 AS ID_T3, PMS.ID_PROJECT AS ID_PROJECT, PMS.ID_INSTANCE AS ID_INSTANCE FROM SPR_DIN_OBJECTS OS inner join M4RDM_OS_PROJ_MEMS PMS on (OS.ID = PMS.ID_INSTANCE and PMS.ID_CLASS = 'DIN_OBJECT' AND PMS.ID_PROJECT IN ('STANDARD','_M4ROOT'))";
            sqlComm = new SqlCommand(sRequete, sqlConn);
            sqlDR = sqlComm.ExecuteReader();

            while (sqlDR.Read())
            {
                lObjetsHeritesSTD.Add(new string[] { sqlDR[0].ToString(), sqlDR[1].ToString(), sqlDR[2].ToString() });
                if (sListeID_T3 != string.Empty)
                    sListeID_T3 += ",";
                sListeID_T3 += "'" + sqlDR[0].ToString() + "'";
            }
            sqlDR.Close();

            /* Récupération des présentations héritées au niveau standard */
            sRequete = "select PS.ID_PRESENTATION,PMS.ID_PROJECT,PMS.ID_INSTANCE FROM SPR_DIN_PRESENTS  PS inner join M4RDM_OS_PROJ_MEMS PMS on (PS.ID = PMS.ID_INSTANCE and PMS.ID_CLASS = 'DIN_PRESENT' AND PMS.ID_PROJECT IN ('STANDARD','_M4ROOT'))";
            sqlComm = new SqlCommand(sRequete, sqlConn);
            sqlDR = sqlComm.ExecuteReader();

            while (sqlDR.Read())
            {
                lPresentsHeritesSTD.Add(new string[] { sqlDR[0].ToString(), sqlDR[1].ToString(), sqlDR[2].ToString() });
                if (sListeID_PRES != string.Empty)
                    sListeID_PRES += ",";
                sListeID_PRES += "'" + sqlDR[0].ToString() + "'";
            }
            sqlDR.Close();

            for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
            {
                try
                {
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        if (CtrlExisteTableMDB(connection, "SPR_DIN_OBJECTS") == true && CtrlExisteTableMDB(connection, "SPR_DIN_PRESENTS") == true && CtrlExisteTableMDB(connection,"M4RDM_OS_PROJ_MEMS") == true)
                        {
                            connection.Open();

                            OdbcCommand command = connection.CreateCommand();
                                                        
                            try
                            {
                                command.CommandText = "select OC.ID_T3, PMP.ID_PROJECT,PMP.ID_INSTANCE FROM SPR_DIN_OBJECTS OC inner join M4RDM_OS_PROJ_MEMS PMP on (OC.ID = PMP.ID_INSTANCE and PMP.ID_CLASS = 'DIN_OBJECT' AND PMP.ID_PROJECT NOT IN ('STANDARD','_M4ROOT','PLATFORM')) ";
                                command.CommandText += "WHERE OC.ID_T3 IN (" + sListeID_T3 + ")";

                                OdbcDataReader reader = command.ExecuteReader();

                                while (reader.Read())
                                {
                                    int j = 0;

                                    while (j < (lObjetsHeritesSTD.Count - 1) && (lObjetsHeritesSTD[j][0] != reader[0].ToString())) j++;
                                    if (lObjetsHeritesSTD[j][0] != reader[0].ToString())
                                        lbErreurs.Items.Add(sCheminMDB + " : Héritage de l'objet " + reader[0].ToString() + " au niveau " + reader[1].ToString() + " (" + reader[2].ToString() + ") alors qu'il est hérité au niveau standard.");
                                    else
                                        lbErreurs.Items.Add(sCheminMDB + " : Héritage de l'objet " + reader[0].ToString() + " au niveau " + reader[1].ToString() + " (" + reader[2].ToString() + ") alors qu'il est hérité au niveau " + lObjetsHeritesSTD[j][1] + " (" + lObjetsHeritesSTD[j][2] + ").");
                                }
                                reader.Close();

                                command.CommandText = "select  PC.ID_PRESENTATION,PMP.ID_PROJECT,PMP.ID_INSTANCE from SPR_DIN_PRESENTS PC  inner join M4RDM_OS_PROJ_MEMS PMP on (PC.ID = PMP.ID_INSTANCE and PMP.ID_CLASS = 'DIN_PRESENT' AND PMP.ID_PROJECT NOT IN ('STANDARD','_M4ROOT','PLATFORM')) ";
                                command.CommandText += "WHERE PC.ID_PRESENTATION IN (" + sListeID_PRES + ")";
                                reader = command.ExecuteReader();

                                while (reader.Read())
                                {
                                    int j = 0;

                                    while (j < (lPresentsHeritesSTD.Count - 1) && (lPresentsHeritesSTD[j][0] != reader[0].ToString())) j++;
                                    if (lPresentsHeritesSTD[j][0] != reader[0].ToString())
                                        lbErreurs.Items.Add(sCheminMDB + " : Héritage de la présentation " + reader[0].ToString() + " au niveau " + reader[1].ToString() + " (" + reader[2].ToString() + ") alors qu'elle est héritée au niveau standard.");
                                    else
                                        lbErreurs.Items.Add(sCheminMDB + " : Héritage de la présentation " + reader[0].ToString() + " au niveau " + reader[1].ToString() + " (" + reader[2].ToString() + ") alors qu'elle est héritée au niveau " + lPresentsHeritesSTD[j][1] + " (" + lPresentsHeritesSTD[j][2] +").");
                                }
                                reader.Close();

                                connection.Close();
                            }
                            catch (Exception ex)
                            {
                                lbErreurs.Items.Add("ControleNiveauHeritage (" + sCheminMDB + ") - Erreur d'exécution (exception) : " + ex.Message);
                                return;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    lbErreurs.Items.Add("ControleNiveauHeritage - Erreur d'exécution (exception) : " + ex.Message);
                    return;
                }
            }

        }
        //<= MHUM le 20/03/2018

        // => MHUM le 06/02/2019 gestion des clients désynchro
        //--------------------------------------------------------------------
        // Si on change la case à cocher Clients désynchros
        //
        private void cacClientsDesynchro_CheckedChanged(object sender, EventArgs e)
        {
            GereClientsDesynchro();
        }
        
        
        //--------------------------------------------------------------------
        // On veut cocher tous les clients désynchros
        //
        private void btnTous_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbOrga.Items.Count; i++)
                clbOrga.SetItemChecked(i, true);
        }

        //--------------------------------------------------------------------
        // On veut décocher tous les clients désynchros
        //
        private void btnAucun_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbOrga.Items.Count; i++)
                clbOrga.SetItemChecked(i, false);
        }
        // <= MHUM le 06/02/2019 gestion des clients désynchro


        //=> MHUM le 22/11/2019 - Gestion création des dossiers nécessaires à l'analyse RAMDL
        //--------------------------------------------------------------------
        // Récupération du paramétrage dans la chaine
        //
        private string LitValeurParam(string pChaineEntiere, string pParam)
        {
            string sResultat = string.Empty;
            int iIndexDeb = 0;
            int iIndexFin = 0;

            try
            {
                iIndexDeb = pChaineEntiere.IndexOf(pParam);
                if (iIndexDeb > -1)
                {
                    iIndexDeb = pChaineEntiere.IndexOf("'", iIndexDeb);
                    iIndexFin = pChaineEntiere.IndexOf("'", iIndexDeb+1);
                    sResultat = pChaineEntiere.Substring(iIndexDeb + 1, iIndexFin - iIndexDeb - 1);
                }
            }
            catch (Exception ex)
            {
                lbErreurs.Items.Add("LitValeurParam - Erreur d'exécution (exception) : " + ex.Message);
            }
            return (sResultat);
        }

        //=> MHUM le 18/09/2019 - Gestion du lancement de l'analyse RAMDL
        //--------------------------------------------------------------------
        // Analyse RAMDL des mdb sélectionnés
        //
        private void AnalyseMdbRAMDL(string sEnvironnement, string sChaineDeConnexion, string sLogin, string sMdp)
        {
            StreamWriter swFichierIni = null;
            string sConnection;
            OdbcDataReader reader;
            string sCheminMDB;
            ProcessStartInfo psiStartInfo = null;
            Process pProcess = null;
            string sDossierFichiersRAMDL;
            StreamReader srResultat;
            string sContenuFichierLog;
            StreamWriter swFichierRegroupAnalyse = null;
            StreamReader srFichierAnalyse = null;
            string[] sListeFichier;


            try
            {
                //=> MHUM le 22/11/2019 - Lecture du paramétrage de RAMDL et création des dossiers si nécessaire 
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)+"\\META4\\regmeta4.xml") == true)
                {
                    StreamReader srRegMeta4 = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)+"\\META4\\regmeta4.xml");
                    bool bContinue = !srRegMeta4.EndOfStream;
                    string sBuffer = string.Empty;
                    string sBuffer2 = string.Empty;
                    

                    while (bContinue == true)
                    {
                        sBuffer = srRegMeta4.ReadLine();
                        if (sBuffer.IndexOf("<RAMDL ") > -1)
                        {
                            //if (Directory.Exists("V:\\M4Temp\\PM\\Cache") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Cache");
                            sBuffer2 = LitValeurParam(sBuffer, "LastCacheDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            //if (Directory.Exists("V:\\M4Temp\\PM\\Log") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Log");
                            sBuffer2 = LitValeurParam(sBuffer, "LastLogDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            //if (Directory.Exists("V:\\M4Temp\\PM\\CVS") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\CVS");
                            sBuffer2 = LitValeurParam(sBuffer, "LastCVSDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            //if (Directory.Exists("V:\\M4Temp\\PM\\Client") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Client");
                            sBuffer2 = LitValeurParam(sBuffer, "LastClientDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            //if (Directory.Exists("V:\\M4Temp\\PM\\Package") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Package");
                            sBuffer2 = LitValeurParam(sBuffer, "LastPackageDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            //if (Directory.Exists("V:\\M4Temp\\PM\\Standard") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Standard");
                            sBuffer2 = LitValeurParam(sBuffer, "LastStandardDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);


                            //if (Directory.Exists("V:\\M4Temp\\PM\\Backup\\client") == false) Directory.CreateDirectory("V:\\M4Temp\\PM\\Backup\\client");
                            sBuffer2 = LitValeurParam(sBuffer, "LastBackupDirectory=");
                            if (sBuffer2 != string.Empty)
                                if (Directory.Exists(sBuffer2) == false) Directory.CreateDirectory(sBuffer2);

                            
                            bContinue = false;
                        }
                        else
                            bContinue = !srRegMeta4.EndOfStream;
                    }

                }
                //<= MHUM le 22/11/2019

                if (Directory.Exists(tbDossierResultat.Text + "\\TempoRAMDL") == false) Directory.CreateDirectory(tbDossierResultat.Text + "\\TempoRAMDL");

                if (sEnvironnement == string.Empty)
                    sDossierFichiersRAMDL = tbDossierResultat.Text;
                else
                    sDossierFichiersRAMDL = tbDossierResultat.Text + "\\" + sEnvironnement + "\\RD";



                for (int i = 0; i < dgvListePacks.SelectedRows.Count; i++)
                {
                    sCheminMDB = dgvListePacks.SelectedRows[i].Cells[0].Value.ToString();
                    if (sEnvironnement == string.Empty)
                        AjoutMessageListes("Analyse RAMDL du fichier " + sCheminMDB, 1);
                    else
                        AjoutMessageListes("Analyse RAMDL du fichier " + Path.GetFileName(sCheminMDB) + " sur " + sEnvironnement, 1);

                    MiseAJourMDB(sCheminMDB);


                    sConnection = GereAccess.GetConnectionStringMDB(sCheminMDB);
                    using (OdbcConnection connection = new OdbcConnection(sConnection))
                    {
                        OdbcCommand command = connection.CreateCommand();

                        connection.Open();
                        command.CommandText = "SELECT ID_PACKAGE FROM M4RDL_PACKAGES WHERE ID_PACKAGE LIKE '%_L'";
                        reader = command.ExecuteReader();
                        if (reader.HasRows == true)
                        {
                            swFichierIni = new StreamWriter(tbDossierResultat.Text + "\\TempoRAMDL\\CmdRAMDL.ini", false);

                            swFichierIni.WriteLine("<LOG_FILE>");
                            swFichierIni.WriteLine(sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                            swFichierIni.WriteLine("<ORIGIN_CONN>");
                            swFichierIni.WriteLine("DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + sCheminMDB);
                            swFichierIni.WriteLine("<TARGET_CONN>");
                            if (sEnvironnement != string.Empty)
                                swFichierIni.WriteLine("DSN=" + cbBaseSQL.Text.Replace("XXX", sEnvironnement));
                            else
                                swFichierIni.WriteLine(MiseEnformeChaineConnexion(sChaineDeConnexion, cbBaseSQL.Text));

                            swFichierIni.WriteLine("<USER_CVM>");
                            swFichierIni.WriteLine(sLogin);
                            swFichierIni.WriteLine("<PWD_CVM>");
                            swFichierIni.WriteLine(sMdp);//swFichierIni.WriteLine("Logan%Celya09");
                            swFichierIni.WriteLine("<CLEAR_PREVIOUS_ANALYSIS>");
                            swFichierIni.WriteLine("YES");
                            swFichierIni.WriteLine("<PACK_ANALYSIS>");

                            while (reader.Read())
                            {
                                swFichierIni.WriteLine(reader[0].ToString());
                            }
                            reader.Close();
                            connection.Close();
                            swFichierIni.WriteLine("<ANALYSE_RESULTS_FILE>");
                            swFichierIni.WriteLine(sDossierFichiersRAMDL + "\\Analyse_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".TXT");
                            
                            swFichierIni.Close();

                            // Encodage du mot de passe
                            psiStartInfo = new ProcessStartInfo();
                            psiStartInfo.FileName = tbRAMDL.Text;
                            psiStartInfo.Arguments = "ENC " + tbDossierResultat.Text + "\\TempoRAMDL\\CmdRAMDL.ini";
                            pProcess = Process.Start(psiStartInfo);
                            pProcess.WaitForExit();
                            pProcess.Close();

                            // Suppression des fichiers d'analyse et de log précédents
                            sListeFichier = Directory.GetFiles(sDossierFichiersRAMDL,"RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + "*.log");
                            foreach (string sFichier in sListeFichier)
                                File.Delete(sFichier);
                            sListeFichier = Directory.GetFiles(sDossierFichiersRAMDL, "Analyse_" + Path.GetFileNameWithoutExtension(sCheminMDB) + "*.TXT");
                            foreach (string sFichier in sListeFichier)
                                File.Delete(sFichier);

                            // Lancement de l'analyse
                            psiStartInfo = new ProcessStartInfo();
                            psiStartInfo.FileName = tbRAMDL.Text;
                            psiStartInfo.Arguments = tbDossierResultat.Text + "\\TempoRAMDL\\CmdRAMDL.ini";
                            pProcess = Process.Start(psiStartInfo);

                            //=> MHUM le 19/12/2019 - Controunement du problème de plantage dur les RDP
                            /*pProcess.WaitForExit();
                            pProcess.Close();*/
                            bool bStop = false;

                            while (bStop == false)
                            {
                                if (pProcess.HasExited == true)
                                    bStop = true;
                                else
                                {
                                    if (File.Exists(sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log") == true)
                                    {
                                        try
                                        {
                                            StreamReader srTestFichierLog = new StreamReader(sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                                            string sTestFichierLog = srTestFichierLog.ReadToEnd();
                                            srTestFichierLog.Close();

                                            // Si dans le log je vois que l'analyse est terminé je fais un kill.
                                            if (sTestFichierLog.IndexOf("End execution log") > -1)
                                                pProcess.Kill();
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                    }

                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
                            if (File.Exists(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb") == true)
                                File.Delete(Path.GetDirectoryName(sCheminMDB) + "\\" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".ldb");



                            // Contrôle du résultat
                            srResultat = new StreamReader(sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");
                            sContenuFichierLog = srResultat.ReadToEnd();
                            srResultat.Close();

                            int iIndexErreur = sContenuFichierLog.IndexOf("[Error");
                            if ((iIndexErreur > -1) && (sContenuFichierLog.IndexOf("[Error 429] ActiveX component can't create object") != iIndexErreur))
                                lbErreurs.Items.Add("Erreur lors de l'analyse du mdb " + Path.GetFileNameWithoutExtension(sCheminMDB) + ". Voir le fichier de log " + sDossierFichiersRAMDL + "\\RAMDL_" + Path.GetFileNameWithoutExtension(sCheminMDB) + ".log");

                        }
                        else
                            connection.Close();
                    }
                }
                swFichierRegroupAnalyse = new StreamWriter(sDossierFichiersRAMDL + "\\Regroupement_Analyse.TXT", false);
                if (swFichierRegroupAnalyse != null)
                {
                    bool bPremier = true;
                    string sBuffer = string.Empty;
                    sListeFichier = Directory.GetFiles(sDossierFichiersRAMDL, "Analyse_*.TXT");
                    foreach (string sFichier in sListeFichier)
                    {
                        srFichierAnalyse = new StreamReader(sFichier);
                        // On garde la ligne d'entête si c'est le premier fichier
                        sBuffer = srFichierAnalyse.ReadLine() + Environment.NewLine;
                        if (bPremier == true)
                        {
                            sBuffer += Path.GetFileNameWithoutExtension(sFichier).Substring(8) + ".MDB" + Environment.NewLine;
                            bPremier = false;
                        }
                        else
                            sBuffer = Path.GetFileNameWithoutExtension(sFichier).Substring(8) + ".MDB" + Environment.NewLine;
                        sBuffer += srFichierAnalyse.ReadToEnd();
                        srFichierAnalyse.Close();
                        swFichierRegroupAnalyse.Write(sBuffer);
                    }
                    swFichierRegroupAnalyse.Close();
                }
            }

            catch (Exception ex)
            {
                lbErreurs.Items.Add("AnalyseUnMdbRAMDL - Erreur d'exécution (exception) : " + ex.Message);
                return;
            }
        }

        private void cacAnalyseRAMDL_CheckedChanged(object sender, EventArgs e)
        {
            if (cacAnalyseRAMDL.Checked == true) 
            {
                lDossierRamdl.Visible=true;
                tbRAMDL.Visible = true;
                btnSelDosRAMDL.Visible = true;
            }
            else
            {
                lDossierRamdl.Visible = false;
                tbRAMDL.Visible = false;
                btnSelDosRAMDL.Visible = false;
            }
        }

        private void btnSelDosRAMDL_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(tbRAMDL.Text);
            openFileDialog1.FileName = Path.GetFileName(tbRAMDL.Text);
            openFileDialog1.Filter = "exécutables|*.exe";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbRAMDL.Text = openFileDialog1.FileName;
            }
        }

        private void FormePrincipale_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("FICHIER_RAMDL");
            config.AppSettings.Settings.Add("FICHIER_RAMDL", tbRAMDL.Text);
            config.Save(ConfigurationSaveMode.Modified);

        }

        private string MiseEnformeChaineConnexion(string sChaineConnexion, string sNomSourceODBC)
        {
            string sResultat = string.Empty;
            int iIndex = 0;

            if (sChaineConnexion.ToUpper().Substring(0, 6) == "SERVER")
            {
                iIndex = sChaineConnexion.IndexOf(";");
                if (iIndex > -1)
                    sResultat = "DSN=" + sNomSourceODBC + sChaineConnexion.Substring(iIndex);
            }
            return (sResultat);
        }

        // MHUM le 24/09/2019 - Test MAJ MDB pour supprimer les CRLF des commentaires
        private void MiseAJourMDB(string sCheminMDB)
        {
            OdbcConnection dbConn = new OdbcConnection();
            dbConn.ConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=" + sCheminMDB +";Uid=Admin;Pwd=;";
            dbConn.Open();

            OdbcCommand objCmd = new OdbcCommand();
            
            objCmd.Connection = dbConn;
            objCmd.CommandType = CommandType.Text;
            objCmd.CommandText = "UPDATE M4RDL_PACK_CMDS SET CMD_COMMENTS = ' ' WHERE CMD_COMMENTS LIKE '%'+CHR(13)+CHR(10)+'%'";

            objCmd.ExecuteNonQuery();
            dbConn.Close();

        }

        // MHUM le 26/09/2019 - Controle la validité du login et du mot de passe PN
        private bool LoginMdpPNValide(string sLogin, string sMdp)
        {
            bool bResultat = false;
            string sPWDEnc = string.Empty;
            string sBuffer = string.Empty;
            string sChaineDeConnexion = string.Empty;
            GereSQLServer gsGereSQLServer = null;
            SqlConnection sqlConn = null;
            SqlCommand sqlComm = null;
            SqlDataReader sqlDR = null;
            bool bUtiliseLDAP = false;


            try
            {
                gsGereSQLServer = new GereSQLServer();
                if (cacClientsDesynchro.Checked == true)
                {
                    sChaineDeConnexion = gsGereSQLServer.GetConnectionStringSQL(cbBaseSQL.SelectedItem.ToString(),clbOrga.CheckedItems[0].ToString());
                }
                else
                    sChaineDeConnexion = gsGereSQLServer.GetConnectionStringSQL(cbBaseSQL.SelectedItem.ToString());


                if (sChaineDeConnexion != string.Empty)
                {
                    sqlConn = new SqlConnection(sChaineDeConnexion);
                    sqlComm = sqlConn.CreateCommand();
                    sqlConn.Open();
                    
                    // On vérifie d'abord si l'utilisateur est paramétré pour le LDAP
                    sqlComm.CommandText = "select * from M4RAV_APP_VAL_LG1 WHERE ID_APLICATION='LOGON' AND ID_SECTION='SCH_SESSION' AND ID_KEY='ID_REPOSITORY' AND VALUE_CONCEPT='" + sLogin + "' AND CONVERT(VARCHAR(MAX),APP_VALUE)='0002'";
                    sqlDR = sqlComm.ExecuteReader();
                    // Si utilise LDAP je schinte le contrôle                    
                    if (sqlDR.HasRows == true)
                    {
                        bUtiliseLDAP = true;
                        bResultat = true;
                    }
                    sqlDR.Close();

                    if (bUtiliseLDAP == false)
                    {
                        sqlComm.CommandText = "select * from M4RSC_APPUSER where ID_APP_USER='" + sLogin + "' AND N_PASSWORD=lower(convert(varchar(max),hashbytes('sha2_512',convert(varchar(max),'" + sLogin + "'+'" + sMdp + "')),2))";
                        sqlDR = sqlComm.ExecuteReader();
                        if (sqlDR.HasRows == true)
                            bResultat = true;
                        sqlDR.Close();
                    }
                    sqlConn.Close();
                }

            }
             catch (Exception ex)
            {
                lbErreurs.Items.Add("LoginMdpPNValide - Erreur d'exécution (exception) : " + ex.Message);
                return false;
            }
            return bResultat;
        }
    }

}
        