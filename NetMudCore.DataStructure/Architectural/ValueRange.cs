using System.ComponentModel.DataAnnotations;

namespace NetMudCore.DataStructure.Architectural
{
    public class ValueRange<T>
    {
        [DataType(DataType.Text)]
        public T Low { get; set; }

        [DataType(DataType.Text)]
        public T High { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ValueRange()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            Low = default;
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning disable CS8601 // Possible null reference assignment.
            High = default;
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public ValueRange(T low, T high)
        {
            Low = low;
            High = high;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", Low, High);
        }
    }
}
