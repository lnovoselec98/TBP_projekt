using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPregnancy
{
    public class Appointment
    {
        public DateTime AppointmentDate { get; set; }
        public string AppointmentType { get; set; }
        public string Notes { get; set; }
        public float Weight { get; set; }
        public float BabyWeight { get; set; }
        public float BabyLength { get; set; }
        public string BloodPressure { get; set; }
        public string Symptoms { get; set; }
    }
}
