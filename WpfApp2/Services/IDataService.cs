using System.Collections.Generic;
using WpfApp2.Models;

namespace WpfApp2.Services
{
	public interface IDataService
	{
		List<Transaction> LoadData(string filePath);
		void SaveData(string filePath, IEnumerable<Transaction> transactions);
	}
}