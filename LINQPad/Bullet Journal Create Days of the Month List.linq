<Query Kind="Statements" />

var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
from = from.AddMonths(1);
var current = from;
var list = new List<string>();
while (current.Month == from.Month)
{
	list.Add(current.ToString("dd.ddd"));
	current = current.AddDays(1);
}
list.Dump(from.ToString(("MMM yyyy")));