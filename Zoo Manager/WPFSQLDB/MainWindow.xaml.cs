using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;

namespace WPFSQLDB
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;
        public MainWindow()
        {


            InitializeComponent();
            // Connection Adresse der Datenbank in einem string speichern. => Projektname, Einstellungen , DataSource Name , Definition als "ConnectionString"
            string connectionString = ConfigurationManager.ConnectionStrings["WPFSQLDB.Properties.Settings.MatvejDBConnectionString"].ConnectionString;

            sqlConnection = new SqlConnection(connectionString);
            showZoo();
            showAllAnimals();

        }

        // Liste aller Zoo´s aus der Datenbank

        public void showZoo()
        {
            string query = "SELECT * FROM Zoo";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
            try
            {
                // Verwenden des sqlDataAdapter´s
                using (sqlDataAdapter)
                {
                    // Erstelle ein Object vom Typen DataTable - Eine Tabelle mit speicherinternen Daten da
                    DataTable zooTable = new DataTable();
                    // Befülle die Tabelle mit den Daten aus der Datenbanktabelle "zooTable"
                    sqlDataAdapter.Fill(zooTable);

                    listZoos.DisplayMemberPath = "Location";
                    listZoos.SelectedValuePath = "Id"; // ValuePath! sehr sehr wichtig .. mach den Fehler nicht noch einmal :D
                    listZoos.ItemsSource = zooTable.DefaultView;
                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
            }

        }

        // Liste der Tiere im gewählten Zoo
        public void showAssociatedAnimals()
        {
            // Wichtig für das Löschen von Zoo´s. Sonst erscheint ein Fehler, da Versuch Tiere zu laden aber Zoo nicht mehr vorhanden ist.
            if (listZoos.SelectedValue == null)
            {
                return;
            }
            
                string query = "SELECT * FROM Animal a INNER JOIN AnimalZoo az ON a.Id = az.animalid WHERE az.zooid = @zooid";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlCommand.Parameters.AddWithValue("@zooid", listZoos.SelectedValue);

                    DataTable animalTable = new DataTable();
                    sqlDataAdapter.Fill(animalTable);


                    listAnimals.DisplayMemberPath = "Name";
                    listAnimals.SelectedValuePath = "Id";

                    listAnimals.ItemsSource = animalTable.DefaultView;
                }
            
        }

        
        // Liste aller Tiere aus der Datenbank
        public void showAllAnimals()
        {
            string query = "SELECT * FROM Animal";

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

            using (sqlDataAdapter)
            {
                DataTable allAnimalsTable = new DataTable();
                sqlDataAdapter.Fill(allAnimalsTable);

                allAnimals.DisplayMemberPath = "Name";
                allAnimals.SelectedValuePath = "Id";

                allAnimals.ItemsSource = allAnimalsTable.DefaultView;
            }
        }


        // Hinzufügen eines Zoostandortes in die Tabelle Zoo
        public void addZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "INSERT INTO Zoo VALUES (@Location)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);   // Eingabebefehl "was" "worüber"
                sqlConnection.Open();                                           // Verbindung öffnen
                sqlCommand.Parameters.AddWithValue("@Location", txtBox.Text);   // Parameter Übergabe "was" "woher"
                sqlCommand.ExecuteScalar();                                     // Ausführen
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { sqlConnection.Close(); showZoo(); }                       // Verbindung schließen, Ansicht aktualisieren
        }


        // Hinzufügen einer Tierart zu der Tabelle Zoo
        public void addAnimal_toZoo_Click(object sender, RoutedEventArgs e)
        {
            if(listZoos.SelectedValue == null || allAnimals.SelectedValue == null)
            {
                return;
            }
            try
            {
                //MessageBox.Show("Selected value: " + listZoos.SelectedValue.ToString() + "Selected value: " + allAnimals.SelectedValue.ToString());
                string query = "INSERT INTO AnimalZoo VALUES (@zooid, @animalid)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@zooid", listZoos.SelectedValue);
                sqlCommand.Parameters.AddWithValue("@animalid", allAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString());}
            finally { sqlConnection.Close(); showAssociatedAnimals(); }

        }

        //Hinzufügen einer Tierart in Tabelle "AllAnimal"
        public void addAnimal_to_allAnimal_Click(object sender, RoutedEventArgs e)
        {
            string query = "INSERT INTO Animal VALUES (@Name)";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            sqlCommand.Parameters.AddWithValue("@Name", txtBox.Text);
            sqlCommand.ExecuteScalar();

            sqlConnection.Close(); showAllAnimals();
        }

        // Entferne ein Zoostandort
        public void deleteZoo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //MessageBox.Show("Selected value: " + listZoos.SelectedValue.ToString());
                string query = "DELETE FROM Zoo WHERE Id = @zooid";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@zooid", listZoos.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { sqlConnection.Close(); showAllAnimals(); showAssociatedAnimals(); }
        }

        // Entfernen einer Tierart aus der Tabelle AllAnimals
        public void deleteAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //MessageBox.Show("Selected value: " + allAnimals.SelectedValue.ToString());
                string query = "DELETE FROM Animal WHERE Id = @animalid";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@animalid", allAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            finally { sqlConnection.Close(); showAllAnimals(); }
        }

        // Entferne eine Tierart aus einem Zoo
        public void removeAnimal_Click(object sender, RoutedEventArgs e)
        {
            string query = "DELETE FROM AnimalZoo WHERE animalid = @id";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            try
            {
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@id", listAnimals.SelectedValue);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                // Hier können Sie den Fehler anzeigen oder eine Benachrichtigung an den Benutzer senden
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close(); showAssociatedAnimals(); showZoo();
            }

        }

        //Aktualisiere die Liste der Tiere
        public void showSelectedAnimal()
        {
        try 
        { 
            string query = "SELECT Name FROM Animal WHERE Id = @ids";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            using (sqlDataAdapter)
            {
                sqlCommand.Parameters.AddWithValue("@ids", allAnimals.SelectedValue);
                DataTable animalDataTable = new DataTable();
                sqlDataAdapter.Fill(animalDataTable);

                txtBox.Text = animalDataTable.Rows[0]["Name"].ToString();
            }
        }
            catch(Exception ex) {}
}
        // Aktualisiere die Liste der Zoo´s
        public void showSelectedZoo()
        {
            try { 
            string query = "SELECT Location FROM Zoo WHERE Id = @ids";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            using (sqlDataAdapter)
            {
                sqlCommand.Parameters.AddWithValue("@ids", listZoos.SelectedValue);
                DataTable zooDataTable = new DataTable();
                sqlDataAdapter.Fill(zooDataTable);

                txtBox.Text = zooDataTable.Rows[0]["Location"].ToString();
            }
            }
            catch(Exception ex) {}
        }

        //Ausgabe der Tiere bei der Wahl eines Zoos
        private void listZoos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showAssociatedAnimals();
            showSelectedZoo();
        }

        private void allAnimals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showSelectedAnimal();
        }

        private void UpdateZoo_Click(object sender, RoutedEventArgs e)
        {
            if (listZoos.SelectedValue == null) { return; }
            try 
            { 
            string query = "UPDATE Zoo SET Location = @Location WHERE Id = @zooid";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            sqlConnection.Open();
            sqlCommand.Parameters.AddWithValue("@zooid", listZoos.SelectedValue);
            sqlCommand.Parameters.AddWithValue("@Location", txtBox.Text);
            sqlCommand.ExecuteScalar();

            }
            catch(Exception ex)
            {

            }
            finally{
                sqlConnection.Close();
                showZoo();
            }
        }

        private void UpdateAnimal_Click(object sender, RoutedEventArgs e)
        {
            if (allAnimals.SelectedValue == null) { return; }
            try
            {
                string query = "UPDATE Animal SET Name = @Name WHERE Id = @animalid";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@animalid", allAnimals.SelectedValue);
                sqlCommand.Parameters.AddWithValue("@Name", txtBox.Text);
                sqlCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
                showAllAnimals();
            }
        }
    }
}
