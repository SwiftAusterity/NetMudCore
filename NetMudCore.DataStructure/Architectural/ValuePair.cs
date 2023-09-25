using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural
{
    public class ValuePair<T>
    {
        [DataType(DataType.Text)]
        public T Actor { get; set; }

        [DataType(DataType.Text)]
        public T Victim { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ValuePair()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Actor = default;
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
            Victim = default;
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public ValuePair(T actor, T victim)
        {
            Actor = actor;
            Victim = victim;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", Actor, Victim);
        }
    }
}
