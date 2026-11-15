namespace HappyPet.Application.Options
{
    public class ReminderOptions
    {
        public bool Enabled { get; set; } = true;
        public int RunHourLocal { get; set; } = 9;
        public string Timezone { get; set; } = "Asia/Tbilisi";
        public int MaxAttempts { get; set; } = 3;
        public int OverdueLookbackDays { get; set; } = 30;

        /// <summary>
        /// Reminder stages in days before the due date. The closest applicable
        /// stage is used, so a missed job does not send several old reminders.
        /// </summary>
        public int[] Offsets { get; set; } = { 30, 7, 1, 0 };
    }
}
