<Query Kind="Program" />

public const string TODO_TAG = "TODO";
public const string EVENT_TAG = "EVENT";
public const int BIRTHDAY_WARNING_DAYS = 10;

void Main()
{
	var from = new DateTime(2023, 12, 1);
	//from = from.AddMonths(0);
	var current = from;
	
	//TODO: Add Holidays

	var dayOnly = new Dictionary<int, string>(){
		{1, "Log US Together Hours"},
		{22, "Rent"}
	};

	var dayOfWeek = new Dictionary<DayOfWeek, string>(){
		{DayOfWeek.Tuesday, "Mohamad Conversation Practice"}
	};

	var birthdays = new List<Birthday>(){
		new Birthday("Jarred Moyher", new DateTime(1980, 01, 05)),
		new Birthday("Marilyn Pennington", new DateTime(1951, 01, 05)),
		new Birthday("Geetha Gandhi (Year born not certain)", new DateTime(1979, 01, 11)), //Year not certain
		new Birthday("Mike Kalkstein", new DateTime(1975, 01, 13)),
		new Birthday("Mohamad Khawanda", new DateTime(1992, 02, 02)),
		new Birthday("Giselle Hicks", new DateTime(2016, 03, 04)),
		new Birthday("Lela Van Horn", new DateTime(1972, 03, 04)),
		new Birthday("Samuel Kalkstein", new DateTime(2014, 03, 21)),
		new Birthday("Carol Kalkstein", new DateTime(1951, 04, 16)),
		new Birthday("Richard Kalkstein", new DateTime(1951, 04, 23)),
		new Birthday("Kenyan Hicks", new DateTime(1988, 05, 18)),
		new Birthday("Jason Hill", new DateTime(1977, 05, 20)),
		new Birthday("Benjamin Kalkstein", new DateTime(2010, 06, 03)),
		new Birthday("Barb Moyher", new DateTime(1950, 06, 06)),
		new Birthday("MY BIRTHDAY!", new DateTime(1977, 06, 08)),
		new Birthday("Madison Moyher", new DateTime(2013, 06, 11)),
		new Birthday("Dolly Anne Hill", new DateTime(2018, 07, 18)),
		new Birthday("John Moyher", new DateTime(1950, 08, 12)),
		new Birthday("Tristan Chiappisi", new DateTime(1991, 09, 03)),
		new Birthday("Dale Pennington", new DateTime(1945, 09, 09)),
		new Birthday("Jennifer Hicks", new DateTime(1987, 10, 21)),
		new Birthday("Jericho Hicks", new DateTime(2013, 10, 26)),
		new Birthday("Susan Kalkstein", new DateTime(1989, 12, 04)),
		new Birthday("Jaxon Moyher", new DateTime(2011, 12, 05)),
		new Birthday("Heidi's Birthday", new DateTime(1976, 12, 20)),
	};

/* Not fully implemented
	var events = new List<Event>(){
		new Event("We First Met", new DateTime(1996, 01, 27)),
		new Event("Our Wedding", new DateTime(1998, 07, 11)),
		
		//Specific Dates
		new Event("New Years Eve", new DateTime(from.Year, 12, 31)),
		new Event("New Years Day", new DateTime(from.Year, 01, 01)),
		new Event("Xmas Eve", new DateTime(from.Year, 12, 24)),
		new Event("Xmas Day", new DateTime(from.Year, 01, 01)),
		
		//Varying dates
		new Event("MLK Day", new DateTime(from.Year, 02, 12)),//3rd Mon in Jan  https://en.wikipedia.org/wiki/Martin_Luther_King_Jr._Day
		new Event("Superbowl 2023", new DateTime(2023, 02, 12)),
	};
*/

	var list = new Dictionary<string, string>();
	var tuesdays = new List<string>();
	while (current.Month == from.Month)
	{
		var dayText = String.Empty;

		if (DayOfWeek.Tuesday == current.DayOfWeek)
		{
			tuesdays.Add(current.ToString("dd.ddd"));	
		}
		
		dayOnly
			.Where(_ => _.Key == current.Day)
			.ToList()
			.ForEach(_ => dayText += $"{TODO_TAG} {_.Value}" + Environment.NewLine);
		

		dayText = dayText + String.Join(Environment.NewLine,
			birthdays.Where(_ => _.Born.AddDays(-(BIRTHDAY_WARNING_DAYS)).Month == current.Month && _.Born.AddDays(-(BIRTHDAY_WARNING_DAYS)).Day == current.Day)
			.Select(_ => $"{TODO_TAG} {_.Person} Birthday Upcoming on " + _.Born.ToString("MMM dd"))
			.ToArray());

		dayText = dayText + String.Join(Environment.NewLine, birthdays.Where(_ => _.Born.Month == current.Month && _.Born.Day == current.Day)
			.Select(_ => $"{EVENT_TAG} {_.Person} Birthday turning {_.GetAgeOn(current)}")
			.ToArray());

		list.Add(current.ToString("dd.ddd"), dayText);
		current = current.AddDays(1);
	}

	tuesdays.Dump("US Together Hours");

	list.Dump(from.ToString(("MMM yyyy")));
}

public class Birthday
{
	public Birthday(string person, DateTime born)
	{
		Person = person;
		Born = born;
	}

	public string Person { get; set; }

	public DateTime Born { get; set; }

	public DateTime Died { get; set; }

	public int GetAgeOn(DateTime date)
	{
		return date.Year - Born.Year;
	}
}

public class Event
{
	public Event(string name, DateTime occurred)
	{
		Name = name;
		Occurred = occurred;
	}

	public string Name { get; set; }

	public DateTime Occurred { get; set; }

	public int GetYearsSince(DateTime date)
	{
		return date.Year - Occurred.Year;
	}
}