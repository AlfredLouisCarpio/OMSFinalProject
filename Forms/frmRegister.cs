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
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
        }
        SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True");
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();

        private void frmRegister_Load(object sender, EventArgs e)
        {
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("Seller");
            cmbRole.Items.Add("Customer");
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "" || cmbRole.Text == "" || txtPassword.Text == "" || txtConPass.Text == "")
            {
                MessageBox.Show("Username, Role, or Password fields are empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!char.IsUpper(txtUsername.Text, 0))
            {
                MessageBox.Show("Username must start with a capital letter", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (txtPassword.Text == txtConPass.Text)
            {
                using (SqlConnection con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True"))
                {
                    con.Open();


                    string checkUsernameQuery = "SELECT COUNT(*) FROM UserAccount WHERE Username = @Username AND Role = @Role";
                    using (SqlCommand checkUsernameCmd = new SqlCommand(checkUsernameQuery, con))
                    {
                        checkUsernameCmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                        checkUsernameCmd.Parameters.AddWithValue("@Role", cmbRole.Text);

                        int userCount = (int)checkUsernameCmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Username already exists", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {

                            string register = "INSERT INTO UserAccount (Username, Password, Role) VALUES (@Username, @Password, @Role)";
                            using (SqlCommand cmd = new SqlCommand(register, con))
                            {
                                cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                                cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                                cmd.Parameters.AddWithValue("@Role", cmbRole.Text);
                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Your Account has been Successfully Created", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Password does not match, Please Re-enter", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                txtConPass.Text = "";
                txtPassword.Focus();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            cmbRole.Text = "";
            txtPassword.Text = "";
            txtConPass.Text = "";
            txtUsername.Focus();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }

        private void chbkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (chbkShowPass.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtConPass.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
                txtConPass.PasswordChar = '*';
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

}
