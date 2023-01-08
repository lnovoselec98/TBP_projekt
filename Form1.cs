using Npgsql;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace MyPregnancy
{
    public partial class Form1 : Form
    {
        private NpgsqlConnection connection;
        private string connectionString = "Host=localhost:5432;Database=MyPregnancy;Username=postgres;Password=Newpost78;";
        public Form1()
        {
            InitializeComponent();
            connection = new NpgsqlConnection(connectionString);
        }

        private void createAppointmentButton_Click(object sender, EventArgs e)
        {
            if (cmbPatients.Items.Count > 0)
            {
                var addNewAppointmentForm = new AddNewAppointment();
                addNewAppointmentForm.PatientId = (cmbPatients.SelectedItem as ComboboxItem).Value;
                addNewAppointmentForm.ShowDialog();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string query = "SELECT doctor_id, first_name || ' ' || last_name AS name FROM doctors;";
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                connection.Open();
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ComboboxItem doctor = new ComboboxItem();
                    doctor.Value = (int)reader["doctor_id"];
                    doctor.Text = reader["name"].ToString();
                    cmbDoctors.Items.Add(doctor);
                }
                
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
                cmbDoctors.SelectedIndex = 0;
            }
        }

        private void cmbDoctors_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbPatients.Items.Clear();
            cmbPatients.ResetText();
            lblBirthUntil.Text = "Birth until:";
            lblDueDate.Text = "Due date: ";
            dataGridView1.DataSource = null;
            var doctorId = (cmbDoctors.SelectedItem as ComboboxItem).Value;
            string query = "select pat.patient_id, pat.first_name || ' ' || pat.last_name as name from patients as pat join pregnancies as preg on preg.patient_id = pat.patient_id where preg.doctor_id =" + doctorId + ";";
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                connection.Open();
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ComboboxItem patient = new ComboboxItem();
                    patient.Value = (int)reader["patient_id"];
                    patient.Text = reader["name"].ToString();
                    cmbPatients.Items.Add(patient);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
                if (cmbPatients.Items.Count > 0)
                {
                    cmbPatients.SelectedIndex = 0;
                }
            }
        }

        private void cmbPatients_SelectedIndexChanged(object sender, EventArgs e)
        {
            var patientId = (cmbPatients.SelectedItem as ComboboxItem).Value;
            string query = "select app.appointment_date as \"Date\", app.week as \"Week\", app.notes as \"Notes\", app.baby_weight as \"Baby weight\", app.baby_length as \"Baby length\", app.weight as \"Weight\", app.blood_pressure as \"Blood pressure\", array_to_string(array_agg(sym.symptom_name),', ') as \"Symptoms\"" + 
                            " from appointments app" +
                            " left join appointments_symptoms appsym on appsym.appointment_id = app.appointment_id" +
                            " left join symptoms sym on appsym.symptom_id = sym.symptom_id" +
                            " join pregnancies preg on preg.pregnancy_id = app.pregnancy_id" +
                            " where preg.patient_id = " + patientId +
                            " group by app.appointment_date, app.week, app.notes, app.baby_weight, app.baby_length, app.weight, app.blood_pressure" + 
                            ";";
            try
            {                
                var dataAdapter = new NpgsqlDataAdapter(query, connection);
                var commandBuilder = new NpgsqlCommandBuilder(dataAdapter);
                var ds = new DataSet();
                dataAdapter.Fill(ds);
                dataGridView1.ReadOnly = true;
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            string queryDueDate = "SELECT TO_CHAR(age(due_date, current_date), 'mm \"Months\" DD \"Days\"') as birth_until, due_date FROM pregnancies WHERE patient_id=" + patientId + ";";
            try
            {
                NpgsqlCommand commandDueDate = new NpgsqlCommand(queryDueDate, connection);
                connection.Open();
                using NpgsqlDataReader readerDueDate = commandDueDate.ExecuteReader();
                while (readerDueDate.Read())
                {
                    lblBirthUntil.Text = "Birth until: " + readerDueDate["birth_until"].ToString();
                    lblDueDate.Text = "Due date: " + readerDueDate["due_date"].ToString();
                    break;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (cmbPatients.Items.Count > 0)
            {
                cmbPatients_SelectedIndexChanged(null, null);
            }
        }
    }
}