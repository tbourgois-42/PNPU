namespace ControlePacksMDB
{
    partial class fSaisieIdentifiants
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
            this.lUtilisateur = new System.Windows.Forms.Label();
            this.lMotDePasse = new System.Windows.Forms.Label();
            this.tbUtilisateur = new System.Windows.Forms.TextBox();
            this.tbMdp = new System.Windows.Forms.TextBox();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnValider = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lUtilisateur
            // 
            this.lUtilisateur.AutoSize = true;
            this.lUtilisateur.Location = new System.Drawing.Point(12, 23);
            this.lUtilisateur.Name = "lUtilisateur";
            this.lUtilisateur.Size = new System.Drawing.Size(53, 13);
            this.lUtilisateur.TabIndex = 0;
            this.lUtilisateur.Text = "Utilisateur";
            // 
            // lMotDePasse
            // 
            this.lMotDePasse.AutoSize = true;
            this.lMotDePasse.Location = new System.Drawing.Point(12, 57);
            this.lMotDePasse.Name = "lMotDePasse";
            this.lMotDePasse.Size = new System.Drawing.Size(71, 13);
            this.lMotDePasse.TabIndex = 1;
            this.lMotDePasse.Text = "Mot de passe";
            // 
            // tbUtilisateur
            // 
            this.tbUtilisateur.Location = new System.Drawing.Point(133, 20);
            this.tbUtilisateur.Name = "tbUtilisateur";
            this.tbUtilisateur.Size = new System.Drawing.Size(185, 20);
            this.tbUtilisateur.TabIndex = 2;
            // 
            // tbMdp
            // 
            this.tbMdp.Location = new System.Drawing.Point(133, 57);
            this.tbMdp.Name = "tbMdp";
            this.tbMdp.PasswordChar = '*';
            this.tbMdp.Size = new System.Drawing.Size(185, 20);
            this.tbMdp.TabIndex = 3;
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAnnuler.Location = new System.Drawing.Point(133, 99);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(75, 23);
            this.btnAnnuler.TabIndex = 4;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            // 
            // btnValider
            // 
            this.btnValider.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnValider.Location = new System.Drawing.Point(243, 99);
            this.btnValider.Name = "btnValider";
            this.btnValider.Size = new System.Drawing.Size(75, 23);
            this.btnValider.TabIndex = 5;
            this.btnValider.Text = "Valider";
            this.btnValider.UseVisualStyleBackColor = true;
            // 
            // fSaisieIdentifiants
            // 
            this.AcceptButton = this.btnValider;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAnnuler;
            this.ClientSize = new System.Drawing.Size(351, 136);
            this.Controls.Add(this.btnValider);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.tbMdp);
            this.Controls.Add(this.tbUtilisateur);
            this.Controls.Add(this.lMotDePasse);
            this.Controls.Add(this.lUtilisateur);
            this.Name = "fSaisieIdentifiants";
            this.Text = "Identifiant Peoplenet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fSaisieIdentifiants_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lUtilisateur;
        private System.Windows.Forms.Label lMotDePasse;
        private System.Windows.Forms.TextBox tbUtilisateur;
        private System.Windows.Forms.TextBox tbMdp;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.Button btnValider;
    }
}