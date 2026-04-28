using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WpfApp2.Models;

namespace WpfApp2.Services
{
    public class FileDataService : IDataService
    {
        public List<Transaction> LoadData(string filePath)
        {
            var transactions = new List<Transaction>();
            if (!File.Exists(filePath)) return transactions;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                // Формат: Дата|Категория|Тип(0/1)|Сумма|Описание
                var parts = line.Split(new[] { '|' }, 5);
                if (parts.Length == 5 &&
                    DateTime.TryParse(parts[0], out DateTime date) &&
                    int.TryParse(parts[2], out int typeInt) &&
                    decimal.TryParse(parts[3], out decimal amount))
                {
                    transactions.Add(new Transaction
                    {
                        Date = date,
                        Category = parts[1],
                        Type = (CategoryType)typeInt,
                        Amount = amount,
                        Description = parts[4]
                    });
                }
            }
            return transactions;
        }

        public void SaveData(string filePath, IEnumerable<Transaction> transactions)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var t in transactions)
                    {
                        writer.WriteLine($"{t.Date:yyyy-MM-dd}|{t.Category}|{(int)t.Type}|{t.Amount}|{t.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Выбрасываем исключение, чтобы ViewModel могла его перехватить и показать через IDialogService
                throw new IOException("Ошибка сохранения файла.", ex);
            }
        }
    }
}