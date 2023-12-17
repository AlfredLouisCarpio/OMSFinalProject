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
    public partial class FrmCustomerDashboard : Form
    {
        private const string ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;

        public FrmCustomerDashboard()
        {
            InitializeComponent();
            LoadData();
            LoadProducts();
            LoadOrders();
            LoadProductInventory();

        }
        private void LoadOrders()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT OrderID, CustomerName, ProductID, Quantity, OrderStatus FROM dbo.Orders", connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable ordersTable = new DataTable();
                    adapter.Fill(ordersTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadProducts()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT ProductID, ProductName FROM dbo.Products", connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable productTable = new DataTable();
                    adapter.Fill(productTable);

                    cmbProducts.DisplayMember = "ProductName";
                    cmbProducts.ValueMember = "ProductID"; 

                    cmbProducts.DataSource = productTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadProductInventory()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT ProductName, QuantityInStock FROM dbo.Products", connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable inventoryTable = new DataTable();
                    adapter.Fill(inventoryTable);

                    dataGridViewProducts.DataSource = inventoryTable;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading product inventory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                dataAdapter = new SqlDataAdapter("SELECT * FROM Products", connection);
                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dataGridViewProducts.DataSource = dataTable;
                dataGridViewProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
        }

        private void FrmCustomerDashboard_Load(object sender, EventArgs e)
        {
            cmbProducts.DropDownStyle = ComboBoxStyle.DropDownList;
            dataGridViewProducts.ReadOnly = true;
            dataGridViewProducts.Enabled = false;
            dataGridViewProducts.ClearSelection();
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {

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

        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerName.Text) || numericQuantity.Value <= 0)
            {
                MessageBox.Show("Please enter valid customer name and quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!char.IsUpper(txtCustomerName.Text.FirstOrDefault()))
            {
                MessageBox.Show("Customer name must start with a capital letter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int productId = (int)cmbProducts.SelectedValue;
            int quantity = (int)numericQuantity.Value;
            string orderStatus = "Pending";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand checkCustomerCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.Orders WHERE CustomerName = @customerName", connection))
                    {
                        checkCustomerCommand.Parameters.AddWithValue("@customerName", txtCustomerName.Text);

                        int existingCustomerCount = Convert.ToInt32(checkCustomerCommand.ExecuteScalar());

                        if (existingCustomerCount > 0)
                        {
                            MessageBox.Show("Customer name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    int currentStock;
                    using (SqlCommand stockCommand = new SqlCommand("SELECT QuantityInStock FROM dbo.Products WHERE ProductID = @productId", connection))
                    {
                        stockCommand.Parameters.AddWithValue("@productId", productId);
                        currentStock = Convert.ToInt32(stockCommand.ExecuteScalar());
                    }

                    if (currentStock < quantity)
                    {
                        MessageBox.Show("Insufficient stock to fulfill the order.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DialogResult result = MessageBox.Show("Are you sure you want to place this order?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlCommand insertCommand = new SqlCommand("INSERT INTO dbo.Orders (CustomerName, ProductID, Quantity, OrderStatus) VALUES (@customerName, @productID, @quantity, @orderStatus)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@customerName", txtCustomerName.Text);
                            insertCommand.Parameters.AddWithValue("@productID", productId);
                            insertCommand.Parameters.AddWithValue("@quantity", quantity);
                            insertCommand.Parameters.AddWithValue("@orderStatus", orderStatus);

                            insertCommand.ExecuteNonQuery();
                        }

                        // Update stock in the Products table
                        using (SqlCommand updateStockCommand = new SqlCommand("UPDATE dbo.Products SET QuantityInStock = QuantityInStock - @quantity WHERE ProductID = @productId", connection))
                        {
                            updateStockCommand.Parameters.AddWithValue("@productId", productId);
                            updateStockCommand.Parameters.AddWithValue("@quantity", quantity);

                            updateStockCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("Order placed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

             
                LoadData();
                LoadOrders();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {

        }
        private void txtCustomerName_TextChanged_1(object sender, EventArgs e)
        {

        }
        
    }        
}
 

