﻿using System.IO;

public class Table
{
    public List<string> Columns { get; set; } = new List<string>();
    public List<Dictionary<string, string>> Rows { get; set; } = new List<Dictionary<string, string>>();

    public static Table LoadFromFile(string filePath)
    {
        Table table = new Table();
        var lines = File.ReadAllLines(filePath);

        if (lines.Length == 0)
            throw new Exception("Файл пуст.");

        table.Columns = lines[0].Split(',').Select(h => h.Trim()).ToList();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',').Select(v => v.Trim()).ToList();
            if (values.Count != table.Columns.Count)
                throw new Exception($"Несоответствие количества столбцов в строке {i + 1}.");

            Dictionary<string, string> row = new Dictionary<string, string>();
            for (int j = 0; j < table.Columns.Count; j++)
            {
                row[table.Columns[j]] = values[j];
            }
            table.Rows.Add(row);
        }

        return table;
    }

    public void SaveToFile(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(string.Join(", ", Columns));

            foreach (var row in Rows)
            {
                List<string> values = new List<string>();
                foreach (var column in Columns)
                {
                    values.Add(row[column]);
                }
                writer.WriteLine(string.Join(", ", values));
            }
        }
    }
}
