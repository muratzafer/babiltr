using System.Text;

public static class Translator
{
    private static readonly Dictionary<string, string> _translations = new()
    {
        { "Ç", "C" },
        { "Ğ", "G" },
        { "Ö", "O" },
        { "Ş", "S" },
        { "Ü", "U" },

        { "ç", "c" },
        { "ğ", "g" },
        { "ö", "o" },
        { "ş", "s" },
        { "ü", "u" },

        { "İ", "I" },
        { "ı", "i" }
    };

    public static string Translate(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; 
        }

        var translated = new StringBuilder();

        foreach (char c in input)
        {
            string character = c.ToString(); 

            if (_translations.TryGetValue(character, out var translatedChar))
            {
                translated.Append(translatedChar); 
            }
            else
            {
                translated.Append(character); 
            }
        }

        return translated.ToString(); 
    }
}
