namespace Real_State.Tables
{
    public class Appointment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public DateTime AppointmentDate { get; set; } // The date of the appointment
        public TimeSpan AppointmentTime { get; set; } // The time slot (e.g., 8:00 AM)
        public string AppointmentType { get; set; } // "View" or "Buy"

        public bool IsConfirmed { get; set; } = false;
    }
}
