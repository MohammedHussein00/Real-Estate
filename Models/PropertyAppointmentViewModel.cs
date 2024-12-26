using Real_State.Tables;

namespace Real_State.ViewModels
{
    public class PropertyAppointmentViewModel
    {
        public Property Property { get; set; } // Property details
        public string AppointmentType { get; set; } // Property details
        public List<Appointment> Appointments { get; set; } // Existing appointments
    }
}
