namespace SchemaApi.App;

public class EventRepository
{
    private static readonly Dictionary<string, IEnumerable<Event>> _events = new ();

    public IEnumerable<Event> FindBy(string person, DateTime? dateFilter)
    {
        if (_events.TryGetValue(person, out var events))
        {
            var result = events;
            if (dateFilter is not null)
            {
                result = result.Where(x => x.IsSameDayAs(dateFilter.Value));
            }

            return result.ToArray();
        }

        return Enumerable.Empty<Event>();
    }
    
    public void Upsert(string person, IEnumerable<Event> events)
    {
        var list = events.ToArray();
        _events[person] = list;
    }
}