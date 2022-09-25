namespace SchemaApi.App;

public class Event : ValueObject
{
    public Event(DateTime begin, DateTime end, string location, string summary, string description)
    {
        Begin = begin;
        End = end;
        Location = location;
        Summary = summary;
        Description = description;
    }
        
    public DateTime Begin { get; private set; }
    public DateTime End { get; private set; }
    public string Location { get; private set; }
    public string Summary { get; private set; }
    public string Description { get; private set; }

    public bool IsSameDayAs(DateTime timestamp)
    {
        var lower = timestamp.Date;
        var upper = lower.AddDays(1);

        return lower <= Begin && upper > End;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Begin;
        yield return End;
        yield return Location;
        yield return Summary;
        yield return Description;
    }

    public override string ToString()
    {
        return $"Begin: {Begin}, End: {End}, Location: {Location}, Summary: {Summary}, Description: {Description}";
    }
}