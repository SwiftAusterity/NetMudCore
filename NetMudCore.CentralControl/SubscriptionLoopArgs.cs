namespace NetMudCore.CentralControl
{
    public class SubscriptionLoopArgs(string designation, int pulse)
    {
        public int CurrentPulse { get; set; } = pulse;

        public string Designation { get; set; } = designation;
    }
}
