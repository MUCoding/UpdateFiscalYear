using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
var templatePath = configuration["templatePath"]; // Required - cannot use software if this doesn't exist.
var newFiscalYearName = configuration["newFiscalYearFileName"] ?? "FiscalYear_YYYY.tlk"; // Hardcoded default value

var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var exit = false;

var dateRegex = @"\((date )(\d{4})( \d{1,2} \d{1,2})\)";
var eventRegex = @"\((?>[^()]+|(?<o>\()|(?<-o>\)))+(?(o)(?!))\)";

if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
{
    Console.WriteLine("Could not find the template file. Creating a new fiscal year file is not possible.");
    exit = true;
}
else
{
    Console.WriteLine("Input year for new fiscal year. Type 'exit' to quit."); 
}

while (!exit)
{
    var input = Console.ReadLine();
    if (!string.IsNullOrEmpty(input) && input.ToLower() == "exit")
    {
        exit = true;
    }
    else if (string.IsNullOrEmpty(input) || !isValidInput(input))
    {
        Console.WriteLine("Given input was not valid. Insert a valid year.");
    }
    else
    {
        var tempFileContent = File.ReadAllText(templatePath, System.Text.Encoding.Latin1);
        var splittedText = tempFileContent.Split("(event", 2);
        var eventPart = splittedText[1];
        var contentWithoutEvents = splittedText[0] + Regex.Replace($"(event {eventPart}", eventRegex, "");
        var contentWithNewYear = Regex.Replace(contentWithoutEvents, dateRegex, d => $"({d.Groups[1].Value}{input}{d.Groups[3].Value})");
        var fileName = newFiscalYearName.Replace("YYYY", input);
        File.WriteAllText($"{filePath}\\{fileName}", contentWithNewYear, System.Text.Encoding.Latin1);
        Console.WriteLine("Created new fiscal year file: " + fileName);
        exit = true;
    }
}

bool isValidInput(string input)
{
    if (int.TryParse(input, out var year))
    {
        var currentYear = DateTime.Today.Year;
        return year == currentYear || year == currentYear - 1;
    }
    else
    {
        return false;
    }
}