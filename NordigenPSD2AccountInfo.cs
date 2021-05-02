using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NordigenPSD2Sharp
{

	public class TransactionAmount
	{
		public string currency { get; set; }
		public decimal amount { get; set; }
	}
	public class Transaction

	{
		public string debtorName { get; set; }
		public string creditorName { get; set; }
		//public string debtorAccount { get; set; }
		public TransactionAmount transactionAmount { get; set; }
		public DateTime valueDate { get; set; }
		public DateTime bookingDate { get; set; }
		public string remittanceInformationUnstructured { get; set; }
	}

	public class Transactions
	{
		public Transaction[] booked { get; set; }
		public Transaction[] pending { get; set; }
	}

	class TransactionsMain
	{
		public Transactions transactions { get; set; }
	}

	public class NordigenPSD2AccountInfo
  {
    private string _token;
		private readonly string burl = "https://ob.nordigen.com/api/";

		public NordigenPSD2AccountInfo(string token)
    {
      _token = token;
    }

		private HttpClient SetupClient()
    {
			var c = new HttpClient();
			
			var auth = "Token "+_token;
			c.DefaultRequestHeaders.Add("accept", "application/json");
			c.DefaultRequestHeaders.Add("Authorization", auth);
			return c;

		}

		public async Task<Transactions> GetTransactionsAsync(string accountId)
    {
			var transurl = $"{burl}accounts/{accountId}/transactions/";
			var c = SetupClient();
			var trans = await c.GetFromJsonAsync<TransactionsMain>(transurl);
      c.Dispose();
      return trans.transactions;
		}
  }

}
