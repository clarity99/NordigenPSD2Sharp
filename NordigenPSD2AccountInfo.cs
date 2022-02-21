using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NordigenPSD2Sharp
{

  public class TransactionAmount
  {
    public string currency { get; set; }
    public decimal amount { get; set; }
  }


  public class TransAccount
  {
    public string bban { get; set; }
    public string iban { get; set; }
  }

  [DebuggerDisplay("{debtorName} {transactionAmount.amount} EUR {bookingDate} {remittanceInformationUnstructured}")]
  public class Transaction

  {
    public string debtorName { get; set; }
    public string creditorName { get; set; }
    public TransAccount debtorAccount { get; set; }
    public TransAccount creditorAccount { get; set; }
    public TransactionAmount transactionAmount { get; set; }
    public DateTime valueDate { get; set; }
    public DateTime bookingDate { get; set; }
    public string remittanceInformationUnstructured { get; set; }
    public string[] remittanceInformationUnstructuredArray { get; set; }
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

  public class Balance
  {
    public TransactionAmount balanceAmount { get; set; }
    public string balanceType { get; set; }
    public DateTime lastChangeDateTime { get; set; }
  }

  public class RequisitionLink
  {
    public string initiate { get; set; }
  }

  public class RequisitionStatus
  {
    public string @short { get; set; }
    public string @long { get; set; }
    public string description { get; set; }

  }

  public class Requisition
  {
    public string id { get; set; }
    public string redirect { get; set; }
    public string aggreements { get; set; }
    public string reference { get; set; }
    public string link { get; set; }
    public string status { get; set; }
    public string[] accounts { get; set; }
  }

  public class Banks
  {
    public string id { get; set; }
    public string name { get; set; }
    public string bic { get; set; }
    public int transaction_total_days { get; set; }
    public string logo { get; set; }
    public string[] countries { get; set; }
  }

  public class Accounts
  {
    public string id { get; set; }
    public string status { get; set; }
    public string agreements { get; set; }
    public string[] accounts { get; set; }
    public string reference { get; set; }
  }

  public class AccountDetails
  {
    public AccountDetails2 account { get; set; }
  }
  public class AccountDetails2
  {
    public string resourceId { get; set; }
    public string iban { get; set; }
    public string currency { get; set; }
    public string ownerName { get; set; }
  }

  public class AgreementReq
  {
    public int max_historical_days { get; set; }
    public int access_valid_for_days { get; set; }
    public string[] access_scope { get; set; }
    public string institution_id { get; set; }
  }
  public class Agreement
  {
    public string id { get; set; }
    public DateTime created { get; set; }
    public int max_historical_days { get; set; }
    public int access_valid_for_days { get; set; }
    public string accepted { get; set; }
    public string institution_id { get; set; }
  }

  public class BalancesMain
  {
    public Balance[] balances { get; set; }
  }

  public class Token
  {
    public string access { get; set; }
    public int access_expires { get; set; }
    public string refresh { get; set; }
    public int refresh_expires { get; set; }
  }

  public class NordigenException: Exception
  {
    public NordigenException(): base() { }
    public NordigenException(string msg) : base(msg) { }
    public string summary { get; set; }
    public string detail { get; set; }
    public int status_code { get; set; }
  }

  public class NordigenPSD2AccountInfo
  {
    private string _token;
		private readonly string burl = "https://ob.nordigen.com/api/v2/";

    public NordigenPSD2AccountInfo()
    {
      _token = "needtosetlater!";
    }

    public NordigenPSD2AccountInfo(string token)
    {
      _token = token;
    }

    public void SetAuth(string token)
    {
      _token = token;
    }

		private HttpClient SetupClient()
    {
			var c = new HttpClient();
			
			var auth = "Bearer "+_token;
			c.DefaultRequestHeaders.Add("accept", "application/json");
			c.DefaultRequestHeaders.Add("Authorization", auth);
			return c;

		}

    public async Task<T> GetDataAsync<T>(HttpClient c, string url)
    {
      var res = await c.GetAsync(url);
      if (res.IsSuccessStatusCode)
      {
        return await res.Content.ReadFromJsonAsync<T>();
      } else
      {
        var err = await res.Content.ReadFromJsonAsync<NordigenException>();
        throw err;
      }
    }

    public async Task<T> PostDataAsync<T>(HttpClient c, string url, string content)
    {
      var sc = new StringContent(content, Encoding.UTF8, "application/json");
      var res = await c.PostAsync(url, sc);
      if (res.IsSuccessStatusCode)
      {
        return await res.Content.ReadFromJsonAsync<T>();
      }
      else
      {
        var errs = await res.Content.ReadAsStringAsync();
        var nordExc = JsonSerializer.Deserialize<NordigenException>(errs);
        
        var nexc2 =  new NordigenException(errs);
        nexc2.status_code = nordExc.status_code;
        nexc2.detail = nordExc.detail;
        nexc2.summary = nordExc.summary;
        throw nexc2;

      }
    }

    public async Task<Transactions> GetTransactionsAsync(string accountId)
    {
			var transurl = $"{burl}accounts/{accountId}/transactions/";
      using (var c = SetupClient())
      {
        var trans = await GetDataAsync<TransactionsMain>(c, transurl);
        return trans.transactions;
      }
      
		}

    public async Task<Token> GetToken(string secretid, string secretkey)
    {
      var transurl = $"{burl}token/new/";
      using (var c = new HttpClient())
      {
        var cs = $"{{  \"secret_id\": \"{secretid}\", \"secret_key\": \"{secretkey}\"}}";
        var token = await PostDataAsync<Token>(c, transurl, cs);
        return token;
      }
    }

    public async Task<Token> RefreshToken(string refreshToken)
    {
      var transurl = $"{burl}token/refresh/";
      using (var c = new HttpClient())
      {
        var cs = $"{{  \"refresh\": \"{refreshToken}\" }}";
        var token = await PostDataAsync<Token>(c, transurl, cs);
        return token;
      }
    }


    public async Task<Balance[]> GetBalancesAsync(string accountId)
    {
      var transurl = $"{burl}accounts/{accountId}/balances/";
      using (var c = SetupClient())
      {
        var trans = await GetDataAsync<BalancesMain>(c, transurl);
        return trans.balances;
      }
    }

    public async Task<AccountDetails> GetAccountDetailsAsync(string accountId)
    {
      var transurl = $"{burl}accounts/{accountId}/details/";
      using (var c = SetupClient())
      {
        var trans = await GetDataAsync<AccountDetails>(c, transurl);
        return trans;
      }
    }

    public async Task<Agreement> CreateAgreementAsync(string institutionId, int maxHistoricalDays, int accessValidForDays, string[] accessScope)
    {
      var transurl = $"{burl}agreements/enduser/";
      using (var c = SetupClient())
      {

        var reqData = new AgreementReq
        {
          max_historical_days = maxHistoricalDays,
          access_valid_for_days = accessValidForDays,
          access_scope = accessScope,
          institution_id = institutionId
        };
        var rString = JsonSerializer.Serialize(reqData);

        var req = await PostDataAsync<Agreement>(c, transurl, rString);
        return req;
      }
    }

    public async Task<Requisition> CreateRequisitionAsync(string institution_id, string redirect, string reference, string agreement, string user_language)
    {
      var transurl = $"{burl}requisitions/";
      using (var c = SetupClient())
      {

        var reqs = $"{{ \"redirect\": \"{redirect}\", \"institution_id\": \"{institution_id}\" ";
        if (!string.IsNullOrEmpty(reference))
          reqs += $", \"reference\": \"{reference}\"";
        if (!string.IsNullOrEmpty(agreement))
          reqs += $", \"agreement\": \"{agreement}\"";
        if (!string.IsNullOrEmpty(user_language))
          reqs += $", \"user_language\": \"{user_language}\"";
        reqs += "}";

        var req = await PostDataAsync<Requisition>(c, transurl, reqs);
        return req;
      }
      
    }

    public async Task<Accounts> GetAccountsAsync(string reqid)
    {
      var transurl = $"{burl}requisitions/{reqid}/";
      using (var c = SetupClient())
      {
        var accts = await GetDataAsync<Accounts>(c, transurl);
        return accts;
      }

    }

    public async Task<Banks[]> GetBanksAsync(string country)
    {
      var transurl = $"{burl}institutions/?country={country}";
      using (var c = SetupClient())
      {
        var banks = await GetDataAsync<Banks[]>(c, transurl);
        return banks;
      }
    
    }
  }

}
