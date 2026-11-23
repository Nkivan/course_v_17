using System.Globalization;

namespace Domain.UI
{
   
    public static class ConsoleInput
    {
        public static string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                Console.WriteLine("Помилка: значення не може бути порожнім. Спробуйте ще раз.");
            }
        }

        public static string ReadOptionalString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;

                Console.WriteLine($"Помилка: введіть ціле число в межах [{min} .. {max}].");
            }
        }

        public static double ReadDouble(string prompt, double min, double max, bool inclusiveMin = true)
        {
            while (true)
            {
                Console.Write(prompt);
                string? raw = Console.ReadLine()?.Trim().Replace(',', '.');
                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    bool minOk = inclusiveMin ? value >= min : value > min;
                    if (minOk && value <= max)
                        return value;
                }

                string boundSymbol = inclusiveMin ? "[" : "(";
                Console.WriteLine($"Помилка: введіть коректне число в діапазоні {boundSymbol}{min} .. {max}].");
            }
        }

        public static DateTime ReadDate(string prompt)
        {
            string[] formats = { "yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy" };
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input) &&
                    DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    return parsed.Date;
                }

                Console.WriteLine("Помилка: введіть дату у форматі РРРР-ММ-ДД (наприклад, 2026-12-15) або ДД.ММ.РРРР.");
            }
        }

        public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
        {
            TEnum[] values = Enum.GetValues<TEnum>();
            Console.WriteLine(prompt);
            for (int i = 0; i < values.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {values[i]}");
            }

            int choice = ReadInt("Ваш вибір: ", 1, values.Length);
            return values[choice - 1];
        }
    }
}
