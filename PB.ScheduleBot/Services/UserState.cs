namespace PB.ScheduleBot.Services
{
    public class UserState
    {
        public enum State
        {
            Inital,
        }

        public UserState()
        {
            state = State.Inital;
        }

        public State state { get; set; }
    }
}