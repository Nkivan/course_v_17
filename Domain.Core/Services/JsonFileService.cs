using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Core.Services
{
   
    public class JsonFileService<T>
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public string FilePath => _filePath;

        public JsonFileService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Шлях до JSON-файлу не може бути порожнім.", nameof(filePath));

            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public void Save(List<T> data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(_filePath, json);
        }

        public List<T> Load()
        {
            if (!File.Exists(_filePath))
                return new List<T>();

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }
    }
}
