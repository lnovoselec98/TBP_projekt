using Npgsql;
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

namespace MyPregnancy
{
    public partial class AddNewAppointment : Form
    {
        public int PatientId { get; set; }

        private NpgsqlConnection connection;
        private string connectionString = "Host=localhost:5432;Database=MyPregnancy;Username=postgres;Password=Newpost78;";

        public AddNewAppointment()
        {
            InitializeComponent();
            connection = new NpgsqlConnection(connectionString);
        }

        private void AddNewAppointment_Load(object sender, EventArgs e)
        {
            string query = "select symptom_id, symptom_name from symptoms;";
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                connection.Open();
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ComboboxItem symptom = new ComboboxItem();
                    symptom.Value = (int)reader["symptom_id"];
                    symptom.Text = reader["symptom_name"].ToString();
                    chkSymptoms.Items.Add(symptom);
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            int pregnancyId=0;
            string queryPregnancy = "select pregnancy_id from pregnancies where patient_id=" + PatientId + ";";
            try
            {
                NpgsqlCommand commandPregnancy = new NpgsqlCommand(queryPregnancy, connection);
                connection.Open();
                using NpgsqlDataReader readerPregnancy = commandPregnancy.ExecuteReader();
                while (readerPregnancy.Read())
                {
                    pregnancyId=(int)readerPregnancy["pregnancy_id"];
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

            // Get the appointment date and type from the form
            DateTime appointmentDate = dateTimePickerDate.Value;

            bool appointmentCreated = false;

            // Create the INSERT query
            string queryInsertAppointments = "INSERT INTO appointments (pregnancy_id, appointment_date, week, weight, blood_pressure, baby_weight, baby_length, notes) VALUES (@pregnancyId, @appointmentDate, @week, @weight, @bloodPresure, @babyWeight, @babyLength, @notes)";

            // Set up the command with parameters
            NpgsqlCommand commandInsertAppointment = new NpgsqlCommand(queryInsertAppointments, connection);
            commandInsertAppointment.Parameters.AddWithValue("@pregnancyId", pregnancyId);
            commandInsertAppointment.Parameters.AddWithValue("@appointmentDate", appointmentDate);
            commandInsertAppointment.Parameters.AddWithValue("@week", int.Parse(txtWeek.Text));
            commandInsertAppointment.Parameters.AddWithValue("@weight", int.Parse(txtWeight.Text));
            commandInsertAppointment.Parameters.AddWithValue("@bloodPresure", txtBloodPressure.Text);
            commandInsertAppointment.Parameters.AddWithValue("@babyWeight", int.Parse(txtBabyWeight.Text));
            commandInsertAppointment.Parameters.AddWithValue("@babyLength", int.Parse(txtBabyLength.Text));
            commandInsertAppointment.Parameters.AddWithValue("@notes", rtxtNotes.Text);

            try
            {
                connection.Open();
                int rowsAffected = commandInsertAppointment.ExecuteNonQuery();
                appointmentCreated = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            int appointmentId = 0;
            string queryAppointment = "select appointment_id from appointments where pregnancy_id=" + pregnancyId + " ORDER BY appointment_id DESC;";
            try
            {
                NpgsqlCommand commandAppointment = new NpgsqlCommand(queryAppointment, connection);
                connection.Open();
                using NpgsqlDataReader readerAppointment = commandAppointment.ExecuteReader();
                while (readerAppointment.Read())
                {
                    appointmentId = (int)readerAppointment["appointment_id"];
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            if (!appointmentCreated)
            {
                return;
            }

            string querySymptoms = "INSERT INTO appointments_symptoms (appointment_id, symptom_id) VALUES (@appointmentId, @symptomId);";

            var symptoms = new List<int>();
            foreach (var symptom in chkSymptoms.CheckedItems)
            {
                symptoms.Add((symptom as ComboboxItem).Value);
            }
            foreach (var symptomId in symptoms)
            {
                // Set up the command with parameters
                NpgsqlCommand commandInsertSymptoms = new NpgsqlCommand(querySymptoms, connection);
                commandInsertSymptoms.Parameters.AddWithValue("@appointmentId", appointmentId);
                commandInsertSymptoms.Parameters.AddWithValue("@symptomId", symptomId);

                try
                {
                    connection.Open();
                    int rowsAffected = commandInsertSymptoms.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            this.Close();
        }
    }
}
