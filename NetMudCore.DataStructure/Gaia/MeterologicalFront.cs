using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Gaia
{
    [Serializable]
    public class MeterologicalFront(IPressureSystem weatherEvent, float position)
    {
        /// <summary>
        /// The system
        /// </summary>
        [UIHint("PressureSystem")]
        public IPressureSystem Event { get; set; } = weatherEvent;

        /// <summary>
        /// Where the front is on the planet in its movement cycle.
        /// </summary>
        [Display(Name = "Global Position", Description = "Where the front is on the planet in its movement cycle.")]
        [DataType(DataType.Text)]
        public float Position { get; set; } = position;
    }
}
