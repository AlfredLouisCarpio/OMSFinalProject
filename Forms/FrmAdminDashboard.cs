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

namespace OMSFinal.Forms
{
    public partial class FrmAdminDashboard : Form
    {
        SqlCommand cmd;
        SqlConnection cn;
        SqlDataReader dr;
        private const string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";
        public FrmAdminDashboard()
        {
            InitializeComponent();
            LoadUserData();
        }
        private void LoadUserData()
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserID, Username, Password, Role FROM UserAccount";
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    dataGridViewUser.Rows.Clear();

                    while (reader.Read())
                    {
                        dataGridViewUser.Rows.Add(reader["UserID"], reader["Username"], reader["Password"], reader["Role"]);
                    }
                }

                reader.Close();
            }
        }
        private void FrmAdminDashboard_Load(object sender, EventArgs e)
        {
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("Seller");
            cmbRole.Items.Add("Customer");

            

            LoadUserData();
        }
        private void loadDataGrid()
        {
            using (SqlCommand cmd = new SqlCommand("SELECT username, password, role FROM dbo.usertable ORDER BY username DESC", cn))
            {
                SqlDataAdapter adap = new SqlDataAdapter(cmd);
                DataTable tab = new DataTable();

                adap.Fill(tab);
                dataGridViewUser.DataSource = tab;
                
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(cmbRole.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please enter a value in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!char.IsUpper(txtUsername.Text[0]))
            {
                MessageBox.Show("The first letter of the username must be a capital letter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM UserAccount WHERE Username = @username AND Role = @role", connection))
                {
                    checkCmd.Parameters.AddWithValue("@username", txtUsername.Text);
                    checkCmd.Parameters.AddWithValue("@role", cmbRole.Text);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Account is already used. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {

                        using (SqlCommand insertCmd = new SqlCommand("INSERT INTO UserAccount (Username, Password, Role) VALUES (@username, @password, @role)", connection))
                        {
                            insertCmd.Parameters.AddWithValue("@username", txtUsername.Text);
                            insertCmd.Parameters.AddWithValue("@password", txtPassword.Text);
                            insertCmd.Parameters.AddWithValue("@role", cmbRole.Text);

                            insertCmd.ExecuteNonQuery();
                            MessageBox.Show("Account has been created", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUserData();
                        }
                    }
                }
            }
        }

            private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrEmpty(cmbRole.Text))
            {
                MessageBox.Show("Please enter values in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!char.IsUpper(txtUsername.Text[0]))
            {
                MessageBox.Show("The first letter of the username must be a capital letter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string username = txtUsername.Text;
                string role = cmbRole.Text.ToUpper();

                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM UserAccount WHERE UPPER(Username) = @username AND UPPER(Role) = @role", connection))
                {
                    checkCmd.Parameters.AddWithValue("@username", username.ToUpper());
                    checkCmd.Parameters.AddWithValue("@role", role);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE UserAccount SET Password = @password WHERE UPPER(Username) = @username AND UPPER(Role) = @role", connection))
                        {
                            cmd.Parameters.AddWithValue("@username", username.ToUpper());
                            cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                            cmd.Parameters.AddWithValue("@role", role);

                            try
                            {
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Successfully Updated!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Update failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error updating the account: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        LoadUserData();
                    }
                    else
                    {
                        MessageBox.Show("No account found for the specified username and role.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrEmpty(cmbRole.Text))
            {
                MessageBox.Show("Please enter values in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!char.IsUpper(txtUsername.Text[0]))
            {
                MessageBox.Show("The first letter of the username must be a capital letter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string usernameToDelete = txtUsername.Text;
            string passwordToDelete = txtPassword.Text;
            string roleToDelete = cmbRole.Text;

            DialogResult drs = MessageBox.Show("Are you sure you want to delete this?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (drs == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM UserAccount WHERE Username = @username AND Password = @password AND Role = @role", connection))
                    {
                        cmd.Parameters.AddWithValue("@username", usernameToDelete);
                        cmd.Parameters.AddWithValue("@password", passwordToDelete);
                        cmd.Parameters.AddWithValue("@role", roleToDelete);

                        try
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Successfully Deleted!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No account found for the specified username, password, and role.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting the account: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("CANCELLED!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LoadUserData();
        }

        private void lblLO_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }

        private void lblExi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new frmReports().Show();
            this.Hide();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
 }
 

