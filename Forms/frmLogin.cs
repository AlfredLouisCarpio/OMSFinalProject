using OMSFinal.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OMSFinal
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }
        SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True");
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private void frmLogin_Load(object sender, EventArgs e)
        {
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("Seller");
            cmbRole.Items.Add("Customer");
        }

        private void chbkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (chbkShowPass.Checked)
            {
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            cmbRole.Text = "";
            txtPassword.Text = "";
            txtUsername.Focus();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            new frmRegister().Show();
            this.Hide();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            con.Open();
            string login = "SELECT * FROM UserAccount WHERE username = @Username AND password = @Password AND role = @Role";
            SqlCommand cmd = new SqlCommand(login, con);
            cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
            cmd.Parameters.AddWithValue("@Role", cmbRole.Text);
            SqlDataReader dr = cmd.ExecuteReader();

            if (txtUsername.Text == "" || txtPassword.Text == "" || cmbRole.Text == "")
            {
                MessageBox.Show("Please fill in all the fields.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (dr.Read())
            {
                string storedUsername = dr["Username"].ToString();
                string storedPassword = dr["Password"].ToString();
                string storedRole = dr["Role"].ToString();

                if (storedUsername != txtUsername.Text && storedPassword != txtPassword.Text && storedRole != cmbRole.Text)
                {
                    MessageBox.Show("Incorrect username, password, and role. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedUsername != txtUsername.Text && storedPassword != txtPassword.Text)
                {
                    MessageBox.Show("Incorrect username and password. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedPassword != txtPassword.Text && storedRole != cmbRole.Text)
                {
                    MessageBox.Show("Incorrect password and role. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedUsername != txtUsername.Text && storedRole != cmbRole.Text)
                {
                    MessageBox.Show("Incorrect username and role. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedUsername != txtUsername.Text)
                {
                    MessageBox.Show("Incorrect username. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedPassword != txtPassword.Text)
                {
                    MessageBox.Show("Incorrect password. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (storedRole != cmbRole.Text)
                {
                    MessageBox.Show("Incorrect role. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (storedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Hide();
                        FrmAdminDashboard AD = new FrmAdminDashboard();
                        AD.Show();
                    }
                    else if (storedRole.Equals("Seller", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Hide();
                        FrmSellerDashboard SD = new FrmSellerDashboard();
                        SD.Show();
                    }
                    else if (storedRole.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Hide();
                        FrmCustomerDashboard CD = new FrmCustomerDashboard();
                        CD.Show();
                    }
                }
            }
            else
            {
                MessageBox.Show("Incorrect username, password, or role. Please try again.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            con.Close();
        }

        private void cmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
 

