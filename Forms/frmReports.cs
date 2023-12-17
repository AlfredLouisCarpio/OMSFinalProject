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
using System.Windows.Forms.DataVisualization.Charting;

namespace OMSFinal.Forms
{
    public partial class frmReports : Form
    {
        private const string ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";
        public frmReports()
        {
            InitializeComponent();
            LoadReports();
        }
        private void LoadReports()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand("SELECT OrderID, CustomerName, ProductID, Quantity, OrderStatus " +
                                                           "FROM dbo.Orders", connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable reportsTable = new DataTable();
                    adapter.Fill(reportsTable);

                    // Set the DataSource of your DataGridView
                    dataGridViewReports.DataSource = reportsTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        
        private void frmReports_Load(object sender, EventArgs e)
        {
            fillChart();
        }

        private void fillChart()
        {
            try
            {
                DataSet ds = new DataSet();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlDataAdapter adapt = new SqlDataAdapter("SELECT OrderStatus, COUNT(*) as Count FROM dbo.Orders GROUP BY OrderStatus", connection))
                    {
                        adapt.Fill(ds, "Orders");
                    }

                    // Clear existing series
                    chart1.Series.Clear();

                    // Add a new series
                    Series series = new Series("Orders");

                    // Bind data
                    series.Points.DataBindXY(ds.Tables["Orders"].Rows, "OrderStatus", ds.Tables["Orders"].Rows, "Count");

                    // Set chart type
                    series.ChartType = SeriesChartType.Pie;

                    // Set custom colors for the pie slices
                    series.Palette = ChartColorPalette.Pastel;

                    // Add the series to the chart
                    chart1.Series.Add(series);

                    // Add a title to the chart
                    chart1.Titles.Add("Order Status Distribution");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filling chart: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ALFD\\Desktop\\OMSFinal\\OMS.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT OrderID, CustomerName, ProductID, Quantity FROM dbo.Orders WHERE CustomerName LIKE @CustomerName";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CustomerName", "%" + textBox1.Text + "%");

                        SqlDataAdapter adap = new SqlDataAdapter(cmd);
                        DataTable tab = new DataTable();

                        adap.Fill(tab);
                        dataGridViewReports.DataSource = tab;
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {
           
            
        }

        private void dataGridViewReports_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
