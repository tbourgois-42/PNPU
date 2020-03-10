using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ControlePacksMDB
{
    public partial class fSaisieIdentifiants : Form
    {
        public fSaisieIdentifiants()
        {
            InitializeComponent();
        }

        public void LitIndentifiants(out string sLogin, out string sMdp)
        {
            sLogin = tbUtilisateur.Text;
            sMdp = tbMdp.Text;
        }

        private void fSaisieIdentifiants_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((this.DialogResult == System.Windows.Forms.DialogResult.OK) && ((tbUtilisateur.Text == string.Empty) || (tbMdp.Text == string.Empty)))
            {
                MessageBox.Show("Veuillez indiquer le code utilisateur et le mot de passe PN.");
                e.Cancel = true;
            }
        }
    }
}
