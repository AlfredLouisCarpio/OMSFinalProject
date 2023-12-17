using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OMSFinal.Forms
{
    public partial class FrmSellerDashboard : Form
    {
        private const string ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        public FrmSellerDashboard()
        {
            InitializeComponent();
            LoadProducts();
            LoadOrders();
            LoadProductInventory();
            LoadData();

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

                    dataGridViewOrders.DataSource = ordersTable;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadProductInventory()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT ProductID, ProductName, QuantityInStock FROM dbo.Products", connection))
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

        private void FrmSellerDashboard_Load(object sender, EventArgs e)
        {
            cmbProducts.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbStatus.Items.Add("Pending");
            cmbStatus.Items.Add("Done");


        }

        private void btnAddOrder_Click(object sender, EventArgs e)
        {
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
                string orderStatus = cmbStatus.Text;

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

                        using (SqlCommand insertCommand = new SqlCommand("INSERT INTO dbo.Orders (CustomerName, ProductID, Quantity, OrderStatus) VALUES (@customerName, @productID, @quantity, @orderStatus)", connection))
                        {
                            insertCommand.Parameters.AddWithValue("@customerName", txtCustomerName.Text);
                            insertCommand.Parameters.AddWithValue("@productID", productId);
                            insertCommand.Parameters.AddWithValue("@quantity", quantity);
                            insertCommand.Parameters.AddWithValue("@orderStatus", orderStatus);

                            insertCommand.ExecuteNonQuery();
                        }

                        using (SqlCommand updateStockCommand = new SqlCommand("UPDATE dbo.Products SET QuantityInStock = QuantityInStock - @quantity WHERE ProductID = @productId", connection))
                        {
                            updateStockCommand.Parameters.AddWithValue("@productId", productId);
                            updateStockCommand.Parameters.AddWithValue("@quantity", quantity);

                            updateStockCommand.ExecuteNonQuery();
                        }
                    }

                    LoadOrders();
                    LoadProductInventory();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int orderId = (int)dataGridViewOrders.SelectedRows[0].Cells["OrderID"].Value;
            string customerName = dataGridViewOrders.SelectedRows[0].Cells["CustomerName"].Value.ToString();
            int productId = (int)cmbProducts.SelectedValue;
            int quantity = (int)numericQuantity.Value;
            string orderStatus = cmbStatus.Text;


            int existingProductId;
            int existingQuantity;
            string existingCustomerName;
            string existingOrderStatus;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand selectCommand = new SqlCommand("SELECT ProductID, Quantity, CustomerName, OrderStatus FROM Orders WHERE OrderID = @orderID", connection))
                {
                    selectCommand.Parameters.AddWithValue("@orderID", orderId);

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            existingProductId = (int)reader["ProductID"];
                            existingQuantity = (int)reader["Quantity"];
                            existingCustomerName = reader["CustomerName"].ToString();
                            existingOrderStatus = reader["OrderStatus"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Selected order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }


            if (productId != existingProductId || quantity != existingQuantity || customerName != existingCustomerName)
            {
                MessageBox.Show("You can only update the status if the customer name, product, and quantity are exact.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand updateCommand = new SqlCommand("UPDATE Orders SET OrderStatus = @orderStatus WHERE OrderID = @orderID", connection))
                {
                    updateCommand.Parameters.AddWithValue("@orderStatus", orderStatus);
                    updateCommand.Parameters.AddWithValue("@orderID", orderId);

                    updateCommand.ExecuteNonQuery();
                }
            }

            LoadOrders();

        }


        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order to cancel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int orderId = (int)dataGridViewOrders.SelectedRows[0].Cells["OrderID"].Value;

            
            DialogResult result = MessageBox.Show("Are you sure you want to cancel the order?", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();


                        int productId;
                        int canceledQuantity;
                        string customerName;
                        using (SqlCommand getOrderDetailsCommand = new SqlCommand("SELECT ProductID, Quantity, CustomerName FROM Orders WHERE OrderID = @orderID", connection))
                        {
                            getOrderDetailsCommand.Parameters.AddWithValue("@orderID", orderId);
                            SqlDataReader reader = getOrderDetailsCommand.ExecuteReader();

                            if (reader.Read())
                            {
                                productId = (int)reader["ProductID"];
                                canceledQuantity = (int)reader["Quantity"];
                                customerName = reader["CustomerName"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Failed to retrieve order details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            reader.Close();
                        }


                        if (customerName != dataGridViewOrders.SelectedRows[0].Cells["CustomerName"].Value.ToString())
                        {
                            MessageBox.Show("You can only cancel the order if the customer name is exact.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }


                        using (SqlCommand deleteOrderCommand = new SqlCommand("DELETE FROM Orders WHERE OrderID = @orderID", connection))
                        {
                            deleteOrderCommand.Parameters.AddWithValue("@orderID", orderId);
                            deleteOrderCommand.ExecuteNonQuery();
                        }


                        using (SqlCommand updateStockCommand = new SqlCommand("UPDATE dbo.Products SET QuantityInStock = QuantityInStock + @quantity WHERE ProductID = @productId", connection))
                        {
                            updateStockCommand.Parameters.AddWithValue("@productId", productId);
                            updateStockCommand.Parameters.AddWithValue("@quantity", canceledQuantity);
                            updateStockCommand.ExecuteNonQuery();
                        }
                    }

                    LoadOrders();
                    LoadProductInventory();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error canceling order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void lblLO_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new frmLogin().Show();
            this.Close();
        }

        private void lblExi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }


        private void cmbProducts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool IsProductExists(string productName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM dbo.Products WHERE ProductName = @ProductName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", productName);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking product existence: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        private void AddProduct(string productName, int quantityToAdd)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO dbo.Products (ProductName, QuantityInStock) VALUES (@ProductName, @QuantityInStock)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", productName);
                        command.Parameters.AddWithValue("@QuantityInStock", quantityToAdd);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void UpdateProductQuantity(string productName, int quantityToAdd)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    string updateQuery = "UPDATE dbo.Products SET QuantityInStock = QuantityInStock + @QuantityToAdd WHERE ProductName = @ProductName";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@QuantityToAdd", quantityToAdd);
                        command.Parameters.AddWithValue("@ProductName", productName);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating product quantity: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnUpdateStocks_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            int quantityToAdd = (int)numericQuantityProduct.Value;

            
            if (!IsProductExists(productName))
            {
                MessageBox.Show("Product does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UpdateProductQuantity(productName, quantityToAdd);

            LoadData();

            MessageBox.Show("Product quantity updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            }
        }

        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT OrderID, CustomerName, ProductID, Quantity, OrderStatus FROM dbo.Orders WHERE CustomerName LIKE @CustomerName";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CustomerName", "%" + txtCustomerName.Text + "%");

                        SqlDataAdapter adap = new SqlDataAdapter(cmd);
                        DataTable tab = new DataTable();

                        adap.Fill(tab);
                        dataGridViewOrders.DataSource = tab;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void txtProductName_TextChanged(object sender, EventArgs e)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT ProductID, ProductName, QuantityInStock FROM dbo.Products WHERE ProductName LIKE @ProductName";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", "%" + txtProductName.Text + "%");

                        SqlDataAdapter adap = new SqlDataAdapter(cmd);
                        DataTable tab = new DataTable();

                        adap.Fill(tab);
                        dataGridViewProducts.DataSource = tab;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void DeleteProduct(string productName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM dbo.Products WHERE ProductName = @ProductName";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", productName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            string productNameToDelete = txtProductName.Text.Trim();

            if (!IsProductExists(productNameToDelete))
            {
                MessageBox.Show("Product does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete the product?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DeleteProduct(productNameToDelete);

                
                LoadData();
                LoadProducts();

                MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
            private void btnAddProduct_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            int quantityToAdd = (int)numericQuantityProduct.Value;

            
            if (productName.Length == 0 || !char.IsUpper(productName[0]))
            {
                MessageBox.Show("Product name must start with a capital letter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            if (string.IsNullOrWhiteSpace(productName) || quantityToAdd <= 0)
            {
                MessageBox.Show("Please enter valid product name and quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

           
            if (IsProductExists(productName))
            {
                MessageBox.Show("Product with the same name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddProduct(productName, quantityToAdd);

            LoadData();
            LoadProducts();

            MessageBox.Show("Product added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
    }
}

