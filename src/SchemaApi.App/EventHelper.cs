using Ical.Net;
using Ical.Net.CalendarComponents;

namespace SchemaApi.App;

public static class EventHelper
{
    public static Event? ParseSingle(IEnumerable<string> content)
    {
        return Parse(content).SingleOrDefault();
    }
        
    private static bool ShouldIgnore(CalendarEvent calendarEvent)
    {
        return ShouldIgnore(calendarEvent.Summary) || ShouldIgnore(calendarEvent.Description);
    }

    private static bool ShouldIgnore(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
            
        var ignoreTriggers = new[]
        {
            "tilsyn",
            "årgang",
        };

        return ignoreTriggers.Any(x => text.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }
        
    public static IEnumerable<Event> Parse(IEnumerable<string> content)
    {
        var lines = content
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (lines.Count == 0)
        {
            return Enumerable.Empty<Event>();
        }
            
        if (lines.First() != "BEGIN:VCALENDAR")
        {
            lines.Insert(0, "BEGIN:VCALENDAR");
        }

        if (lines.Last() != "END:VCALENDAR")
        {
            lines.Add("END:VCALENDAR");
        }
            
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
            
        foreach (var line in lines)
        {
            writer.WriteLine(line);
        }
            
        writer.Flush();
        stream.Position = 0;

        var result = new LinkedList<Event>();

        try
        {
            var calendar = Calendar.Load(string.Join("\n", lines));
            
            foreach (var calendarEvent in calendar!.Events)
            {
                if (ShouldIgnore(calendarEvent))
                {
                    continue;
                }
                    
                result.AddLast(new Event(
                    begin: calendarEvent.Start.Value,
                    end: calendarEvent.End.Value,
                    summary: calendarEvent.Summary,
                    description: calendarEvent.Description,
                    location: calendarEvent.Location
                ));
            }
        }
        catch (Exception err)
        {
            // ignored
            // throw;
        }

        return result;
    }
}