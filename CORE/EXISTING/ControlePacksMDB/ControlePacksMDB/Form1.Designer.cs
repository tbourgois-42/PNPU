namespace ControlePacksMDB
{
    partial class FormePrincipale
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSelDosRAMDL = new System.Windows.Forms.Button();
            this.tbRAMDL = new System.Windows.Forms.TextBox();
            this.lDossierRamdl = new System.Windows.Forms.Label();
            this.cacAnalyseRAMDL = new System.Windows.Forms.CheckBox();
            this.btnAucun = new System.Windows.Forms.Button();
            this.btnTous = new System.Windows.Forms.Button();
            this.clbOrga = new System.Windows.Forms.CheckedListBox();
            this.cacClientsDesynchro = new System.Windows.Forms.CheckBox();
            this.cacControlerListeTables = new System.Windows.Forms.CheckBox();
            this.cbBaseSQL = new System.Windows.Forms.ComboBox();
            this.lBaseSql = new System.Windows.Forms.Label();
            this.cbStandard = new System.Windows.Forms.CheckBox();
            this.btnChoixDossierResultat = new System.Windows.Forms.Button();
            this.tbDossierResultat = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvListePacks = new System.Windows.Forms.DataGridView();
            this.cChemin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnChoixDossierPacks = new System.Windows.Forms.Button();
            this.tbDossierPacks = new System.Windows.Forms.TextBox();
            this.lDossierPacks = new System.Windows.Forms.Label();
            this.btnEffacerListePacks = new System.Windows.Forms.Button();
            this.btnChargerPacks = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lbInfos = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbActionLocalisation = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbErreurs = new System.Windows.Forms.ListBox();
            this.btnControler = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListePacks)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(967, 657);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnSelDosRAMDL);
            this.tabPage1.Controls.Add(this.tbRAMDL);
            this.tabPage1.Controls.Add(this.lDossierRamdl);
            this.tabPage1.Controls.Add(this.cacAnalyseRAMDL);
            this.tabPage1.Controls.Add(this.btnAucun);
            this.tabPage1.Controls.Add(this.btnTous);
            this.tabPage1.Controls.Add(this.clbOrga);
            this.tabPage1.Controls.Add(this.cacClientsDesynchro);
            this.tabPage1.Controls.Add(this.cacControlerListeTables);
            this.tabPage1.Controls.Add(this.cbBaseSQL);
            this.tabPage1.Controls.Add(this.lBaseSql);
            this.tabPage1.Controls.Add(this.cbStandard);
            this.tabPage1.Controls.Add(this.btnChoixDossierResultat);
            this.tabPage1.Controls.Add(this.tbDossierResultat);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.dgvListePacks);
            this.tabPage1.Controls.Add(this.btnChoixDossierPacks);
            this.tabPage1.Controls.Add(this.tbDossierPacks);
            this.tabPage1.Controls.Add(this.lDossierPacks);
            this.tabPage1.Controls.Add(this.btnEffacerListePacks);
            this.tabPage1.Controls.Add(this.btnChargerPacks);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(959, 631);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Chargement des packs";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnSelDosRAMDL
            // 
            this.btnSelDosRAMDL.Location = new System.Drawing.Point(826, 133);
            this.btnSelDosRAMDL.Name = "btnSelDosRAMDL";
            this.btnSelDosRAMDL.Size = new System.Drawing.Size(83, 23);
            this.btnSelDosRAMDL.TabIndex = 31;
            this.btnSelDosRAMDL.Tag = "";
            this.btnSelDosRAMDL.Text = "Sélectionner";
            this.btnSelDosRAMDL.UseVisualStyleBackColor = true;
            this.btnSelDosRAMDL.Visible = false;
            this.btnSelDosRAMDL.Click += new System.EventHandler(this.btnSelDosRAMDL_Click);
            // 
            // tbRAMDL
            // 
            this.tbRAMDL.Location = new System.Drawing.Point(198, 135);
            this.tbRAMDL.Name = "tbRAMDL";
            this.tbRAMDL.Size = new System.Drawing.Size(614, 20);
            this.tbRAMDL.TabIndex = 30;
            this.tbRAMDL.Tag = "";
            this.tbRAMDL.Text = "C:\\Program Files (x86)\\meta4\\M4DevClient\\Bin\\RamDL.exe";
            this.tbRAMDL.Visible = false;
            // 
            // lDossierRamdl
            // 
            this.lDossierRamdl.AutoSize = true;
            this.lDossierRamdl.Location = new System.Drawing.Point(71, 138);
            this.lDossierRamdl.Name = "lDossierRamdl";
            this.lDossierRamdl.Size = new System.Drawing.Size(119, 13);
            this.lDossierRamdl.TabIndex = 29;
            this.lDossierRamdl.Text = "Exécutable RamDL.exe";
            this.lDossierRamdl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lDossierRamdl.Visible = false;
            // 
            // cacAnalyseRAMDL
            // 
            this.cacAnalyseRAMDL.AutoSize = true;
            this.cacAnalyseRAMDL.Location = new System.Drawing.Point(826, 107);
            this.cacAnalyseRAMDL.Name = "cacAnalyseRAMDL";
            this.cacAnalyseRAMDL.Size = new System.Drawing.Size(104, 17);
            this.cacAnalyseRAMDL.TabIndex = 28;
            this.cacAnalyseRAMDL.Text = "Analyse RAMDL";
            this.cacAnalyseRAMDL.UseVisualStyleBackColor = true;
            this.cacAnalyseRAMDL.CheckedChanged += new System.EventHandler(this.cacAnalyseRAMDL_CheckedChanged);
            // 
            // btnAucun
            // 
            this.btnAucun.Location = new System.Drawing.Point(676, 385);
            this.btnAucun.Name = "btnAucun";
            this.btnAucun.Size = new System.Drawing.Size(51, 23);
            this.btnAucun.TabIndex = 27;
            this.btnAucun.Text = "Aucun";
            this.btnAucun.UseVisualStyleBackColor = true;
            this.btnAucun.Click += new System.EventHandler(this.btnAucun_Click);
            // 
            // btnTous
            // 
            this.btnTous.Location = new System.Drawing.Point(676, 343);
            this.btnTous.Name = "btnTous";
            this.btnTous.Size = new System.Drawing.Size(51, 23);
            this.btnTous.TabIndex = 26;
            this.btnTous.Text = "Tous";
            this.btnTous.UseVisualStyleBackColor = true;
            this.btnTous.Click += new System.EventHandler(this.btnTous_Click);
            // 
            // clbOrga
            // 
            this.clbOrga.FormattingEnabled = true;
            this.clbOrga.Location = new System.Drawing.Point(752, 185);
            this.clbOrga.Name = "clbOrga";
            this.clbOrga.Size = new System.Drawing.Size(201, 409);
            this.clbOrga.TabIndex = 25;
            // 
            // cacClientsDesynchro
            // 
            this.cacClientsDesynchro.AutoSize = true;
            this.cacClientsDesynchro.Location = new System.Drawing.Point(325, 107);
            this.cacClientsDesynchro.Name = "cacClientsDesynchro";
            this.cacClientsDesynchro.Size = new System.Drawing.Size(101, 17);
            this.cacClientsDesynchro.TabIndex = 20;
            this.cacClientsDesynchro.Text = "Désynchronisés";
            this.cacClientsDesynchro.UseVisualStyleBackColor = true;
            this.cacClientsDesynchro.CheckedChanged += new System.EventHandler(this.cacClientsDesynchro_CheckedChanged);
            // 
            // cacControlerListeTables
            // 
            this.cacControlerListeTables.AutoSize = true;
            this.cacControlerListeTables.Checked = true;
            this.cacControlerListeTables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cacControlerListeTables.Location = new System.Drawing.Point(553, 107);
            this.cacControlerListeTables.Name = "cacControlerListeTables";
            this.cacControlerListeTables.Size = new System.Drawing.Size(248, 17);
            this.cacControlerListeTables.TabIndex = 22;
            this.cacControlerListeTables.Text = "Contrôler la mise à jour du catalogue des tables";
            this.cacControlerListeTables.UseVisualStyleBackColor = true;
            // 
            // cbBaseSQL
            // 
            this.cbBaseSQL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBaseSQL.FormattingEnabled = true;
            this.cbBaseSQL.Location = new System.Drawing.Point(198, 107);
            this.cbBaseSQL.Name = "cbBaseSQL";
            this.cbBaseSQL.Size = new System.Drawing.Size(121, 21);
            this.cbBaseSQL.TabIndex = 21;
            // 
            // lBaseSql
            // 
            this.lBaseSql.AutoSize = true;
            this.lBaseSql.Location = new System.Drawing.Point(135, 111);
            this.lBaseSql.Name = "lBaseSql";
            this.lBaseSql.Size = new System.Drawing.Size(55, 13);
            this.lBaseSql.TabIndex = 22;
            this.lBaseSql.Text = "Base SQL";
            this.lBaseSql.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbStandard
            // 
            this.cbStandard.AutoSize = true;
            this.cbStandard.Checked = true;
            this.cbStandard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStandard.Location = new System.Drawing.Point(442, 107);
            this.cbStandard.Name = "cbStandard";
            this.cbStandard.Size = new System.Drawing.Size(105, 17);
            this.cbStandard.TabIndex = 21;
            this.cbStandard.Text = "Packs standards";
            this.cbStandard.UseVisualStyleBackColor = true;
            this.cbStandard.CheckedChanged += new System.EventHandler(this.cbStandard_CheckedChanged);
            // 
            // btnChoixDossierResultat
            // 
            this.btnChoixDossierResultat.Location = new System.Drawing.Point(826, 69);
            this.btnChoixDossierResultat.Name = "btnChoixDossierResultat";
            this.btnChoixDossierResultat.Size = new System.Drawing.Size(83, 23);
            this.btnChoixDossierResultat.TabIndex = 6;
            this.btnChoixDossierResultat.Tag = "Sélectionner le dossier contenant les packs";
            this.btnChoixDossierResultat.Text = "Sélectionner";
            this.btnChoixDossierResultat.UseVisualStyleBackColor = true;
            this.btnChoixDossierResultat.Click += new System.EventHandler(this.btnChoixDossierResultat_Click);
            // 
            // tbDossierResultat
            // 
            this.tbDossierResultat.Location = new System.Drawing.Point(198, 69);
            this.tbDossierResultat.Name = "tbDossierResultat";
            this.tbDossierResultat.Size = new System.Drawing.Size(614, 20);
            this.tbDossierResultat.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(187, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Dossier contenant les fichiers résultats";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dgvListePacks
            // 
            this.dgvListePacks.AllowUserToAddRows = false;
            this.dgvListePacks.AllowUserToDeleteRows = false;
            this.dgvListePacks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvListePacks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cChemin});
            this.dgvListePacks.Location = new System.Drawing.Point(10, 174);
            this.dgvListePacks.Name = "dgvListePacks";
            this.dgvListePacks.Size = new System.Drawing.Size(646, 426);
            this.dgvListePacks.TabIndex = 18;
            this.dgvListePacks.TabStop = false;
            // 
            // cChemin
            // 
            this.cChemin.HeaderText = "Chemin du pack";
            this.cChemin.Name = "cChemin";
            this.cChemin.ReadOnly = true;
            this.cChemin.Width = 800;
            // 
            // btnChoixDossierPacks
            // 
            this.btnChoixDossierPacks.Location = new System.Drawing.Point(826, 10);
            this.btnChoixDossierPacks.Name = "btnChoixDossierPacks";
            this.btnChoixDossierPacks.Size = new System.Drawing.Size(83, 23);
            this.btnChoixDossierPacks.TabIndex = 2;
            this.btnChoixDossierPacks.Tag = "Sélectionner le dossier contenant les packs";
            this.btnChoixDossierPacks.Text = "Sélectionner";
            this.btnChoixDossierPacks.UseVisualStyleBackColor = true;
            this.btnChoixDossierPacks.Click += new System.EventHandler(this.btnChoixDossierPacks_Click);
            // 
            // tbDossierPacks
            // 
            this.tbDossierPacks.Location = new System.Drawing.Point(198, 15);
            this.tbDossierPacks.Name = "tbDossierPacks";
            this.tbDossierPacks.Size = new System.Drawing.Size(614, 20);
            this.tbDossierPacks.TabIndex = 1;
            // 
            // lDossierPacks
            // 
            this.lDossierPacks.AutoSize = true;
            this.lDossierPacks.Location = new System.Drawing.Point(49, 15);
            this.lDossierPacks.Name = "lDossierPacks";
            this.lDossierPacks.Size = new System.Drawing.Size(141, 13);
            this.lDossierPacks.TabIndex = 15;
            this.lDossierPacks.Text = "Dossier contenant les packs";
            this.lDossierPacks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnEffacerListePacks
            // 
            this.btnEffacerListePacks.Location = new System.Drawing.Point(541, 40);
            this.btnEffacerListePacks.Name = "btnEffacerListePacks";
            this.btnEffacerListePacks.Size = new System.Drawing.Size(115, 23);
            this.btnEffacerListePacks.TabIndex = 4;
            this.btnEffacerListePacks.Text = "Effacer la liste";
            this.btnEffacerListePacks.UseVisualStyleBackColor = true;
            this.btnEffacerListePacks.Click += new System.EventHandler(this.btnEffacerListePacks_Click);
            // 
            // btnChargerPacks
            // 
            this.btnChargerPacks.Location = new System.Drawing.Point(198, 40);
            this.btnChargerPacks.Name = "btnChargerPacks";
            this.btnChargerPacks.Size = new System.Drawing.Size(194, 23);
            this.btnChargerPacks.TabIndex = 3;
            this.btnChargerPacks.Text = "Charger la liste de packs";
            this.btnChargerPacks.UseVisualStyleBackColor = true;
            this.btnChargerPacks.Click += new System.EventHandler(this.btnChargerPacks_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lbInfos);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.lbActionLocalisation);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.lbErreurs);
            this.tabPage2.Controls.Add(this.btnControler);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(959, 631);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Résultats de l\'analyse";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lbInfos
            // 
            this.lbInfos.AutoSize = true;
            this.lbInfos.Location = new System.Drawing.Point(285, 32);
            this.lbInfos.Name = "lbInfos";
            this.lbInfos.Size = new System.Drawing.Size(0, 13);
            this.lbInfos.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Informations de localisation";
            // 
            // lbActionLocalisation
            // 
            this.lbActionLocalisation.FormattingEnabled = true;
            this.lbActionLocalisation.Location = new System.Drawing.Point(6, 49);
            this.lbActionLocalisation.Name = "lbActionLocalisation";
            this.lbActionLocalisation.Size = new System.Drawing.Size(908, 316);
            this.lbActionLocalisation.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 377);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Liste des erreurs";
            // 
            // lbErreurs
            // 
            this.lbErreurs.ForeColor = System.Drawing.Color.Black;
            this.lbErreurs.FormattingEnabled = true;
            this.lbErreurs.HorizontalScrollbar = true;
            this.lbErreurs.Location = new System.Drawing.Point(6, 400);
            this.lbErreurs.Name = "lbErreurs";
            this.lbErreurs.Size = new System.Drawing.Size(908, 225);
            this.lbErreurs.TabIndex = 7;
            // 
            // btnControler
            // 
            this.btnControler.Location = new System.Drawing.Point(6, 6);
            this.btnControler.Name = "btnControler";
            this.btnControler.Size = new System.Drawing.Size(75, 23);
            this.btnControler.TabIndex = 6;
            this.btnControler.Text = "Contrôler";
            this.btnControler.UseVisualStyleBackColor = true;
            this.btnControler.Click += new System.EventHandler(this.btnControler_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FormePrincipale
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(989, 681);
            this.Controls.Add(this.tabControl1);
            this.Name = "FormePrincipale";
            this.Text = "Contrôle des packs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormePrincipale_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListePacks)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvListePacks;
        private System.Windows.Forms.DataGridViewTextBoxColumn cChemin;
        private System.Windows.Forms.Button btnChoixDossierPacks;
        private System.Windows.Forms.TextBox tbDossierPacks;
        private System.Windows.Forms.Label lDossierPacks;
        private System.Windows.Forms.Button btnEffacerListePacks;
        private System.Windows.Forms.Button btnChargerPacks;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnControler;
        private System.Windows.Forms.ListBox lbErreurs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbActionLocalisation;
        private System.Windows.Forms.Button btnChoixDossierResultat;
        private System.Windows.Forms.TextBox tbDossierResultat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbInfos;
        private System.Windows.Forms.CheckBox cbStandard;
        private System.Windows.Forms.ComboBox cbBaseSQL;
        private System.Windows.Forms.Label lBaseSql;
        private System.Windows.Forms.CheckBox cacControlerListeTables;
        private System.Windows.Forms.CheckBox cacClientsDesynchro;
        private System.Windows.Forms.CheckedListBox clbOrga;
        private System.Windows.Forms.Button btnAucun;
        private System.Windows.Forms.Button btnTous;
        private System.Windows.Forms.Button btnSelDosRAMDL;
        private System.Windows.Forms.TextBox tbRAMDL;
        private System.Windows.Forms.Label lDossierRamdl;
        private System.Windows.Forms.CheckBox cacAnalyseRAMDL;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

