namespace TaskWebAPI.Models
{
    public enum MarkerType
    {
        Default = 0,
        Insert = 1,
        Update = 2,
        Delete = 3
    }

    public class UpdateMarker
    {
        public int TaskId { get; set; } = 0;

        public MarkerType Marker { get; set; } = MarkerType.Default;
    }
}
