using System;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace PriceGenerator
{
	public class ParametersManager
	{
		private static string WebPageContent;

		public void RenewParameters()
		{
			var accounts = Constants.LoadAccounts();
			foreach(var account in accounts)
			{
				var resultAccount = GetTradeWebPageInfo(GenerateTradePageUrl(account));
				resultAccount.Id = account.Id;

				var webMainAccount = GetMainWebPageInfo(GenerateMainPageUrl(account));
				resultAccount.DohWeek = webMainAccount.DohWeek;
				resultAccount.DohDay = webMainAccount.DohDay;

				Constants.UpdateAccountHistory(resultAccount);
			}
		}

		private string GenerateMainPageUrl(Account account)
		{
			return string.Format(@"https://alpari.com/ru/investor/pamm/{0}/", account.Id);
		}

		private string GenerateTradePageUrl(Account account)
		{
			return string.Format(@"https://alpari.com/ru/investor/pamm/{0}/#pamm-leverage", account.Id);
		}

		private static string GetWebPageParameter(Regex regex)
		{
			Match m = regex.Match(WebPageContent);
			if (m.Success)
				return m.Groups[1].Value.ToString();

			return string.Empty;
		}

		private decimal? GetFactorVosst()
		{
			string prefix = @"из максимальной просадки.";
			prefix = prefix.Replace("'", "\"");
			
			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetNishRisk()
		{
			string prefix = @"доходности ниже 10% годовых.";
			prefix = prefix.Replace("'", "\"");

			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetPribVol()
		{
			string prefix = @"в которые велась торговля.";
			prefix = prefix.Replace("'", "\"");

			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetSredDnevUb()
		{
			string prefix = @"Средний дневной убыток";
			prefix = prefix.Replace("'", "\"");

			var postfix = "%</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}
		
		private decimal? GetKalmar()
		{
			string prefix = @"Кальмара будет менее рискованным.";
			prefix = prefix.Replace("'", "\"");

			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetSharp()
		{
			string prefix = @"Шарпа будет менее рискованным.";
			prefix = prefix.Replace("'", "\"");

			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetSortino()
		{
			string prefix = @"Сортино будет менее рискованным.";
			prefix = prefix.Replace("'", "\"");

			var postfix = "</td></tr></table></td></tr><tr class=''><td style=''>";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetDohDay()
		{
			string prefix = @"Доходность за период";
			prefix = prefix.Replace("'", "\"");

			var postfix = "%</td></tr></tbody></table></div";
			postfix = postfix.Replace("'", "\"");

			return GetParameterValue(prefix, postfix);
		}

		private decimal? GetDohWeek()
		{
			string prefix = @"Доходность за период";
			prefix = prefix.Replace("'", "\"");

			//var postfix = "</td><td>";
			var postfix = "%</td></tr></tbody></table></div";
			postfix = postfix.Replace("'", "\"");

			var checker = new BasePriceChecker(prefix, postfix);
			var draftValue = checker.GetElementValue(WebPageContent);
			if (string.IsNullOrEmpty(draftValue))
				return null;

			int index = draftValue.Length - 1;
			string stringValue = string.Empty;
			char currentSymbol;

			do
			{
				currentSymbol = draftValue[index];
				draftValue = draftValue.Substring(0, draftValue.Length - 1);
				--index;
			}
			while (currentSymbol != '%');

			//prefix = draftValue.Substring(0, 1);
			//postfix = "%";

			index = draftValue.Length - 1;
			stringValue = string.Empty;
			while (draftValue[index] != '>')
			{
				stringValue = draftValue[index--] + stringValue;
			}

			stringValue = stringValue.Replace("–", "-");

			if (stringValue == "-")
				return null;

			return Convert.ToDecimal(stringValue);
		}
		
		private decimal? GetParameterValue(string prefix, string postfix, string pageContent = null)
		{
			if (string.IsNullOrEmpty(pageContent))
				pageContent = WebPageContent;

			var checker = new BasePriceChecker(prefix, postfix);
			var draftValue = checker.GetElementValue(pageContent);
			if (string.IsNullOrEmpty(draftValue))
				return null;

			int index = draftValue.Length - 1;
			string stringValue = string.Empty;
			while (draftValue[index] != '>')
			{
				stringValue = draftValue[index--] + stringValue;
			}

			stringValue = stringValue.Replace("–", "-");

			if (stringValue == "-")
				return null;

			return Convert.ToDecimal(stringValue);
		} 

		public string GetWebPageContent(string url, Encoding encoding = null)
		{
			if (encoding == null)
				encoding = Encoding.GetEncoding(1251);

			HttpWebRequest request = (HttpWebRequest.Create(url) as HttpWebRequest);
			HttpWebResponse response = (request.GetResponse() as HttpWebResponse);
			string contents = string.Empty;

			using (Stream stream = response.GetResponseStream())
			{
				int bytesToRead = 8092;
				byte[] buffer = new byte[bytesToRead];

				int length;
				while ((length = stream.Read(buffer, 0, bytesToRead)) > 0)
				{
					contents += encoding.GetString(buffer, 0, length);
				}
			}

			return contents;
		}

		private Account GetMainWebPageInfo(string url)
		{
			try
			{
				WebPageContent = GetWebPageContent(url, Encoding.UTF8);
				Account account = new Account();

				account.DohDay = GetDohDay();
				account.DohWeek = GetDohWeek();

				return account;
			}
			catch (Exception e)
			{
				Logger.LogMessage(e.Message);
				return null;
			}
		}

		private Account GetTradeWebPageInfo(string url)
		{
			try
			{
				WebPageContent = GetWebPageContent(url, Encoding.UTF8);
				Account account = new Account();

				account.FactorVosst = GetFactorVosst();
				account.NishRisk = GetNishRisk();
				account.PribVol = GetPribVol();
				account.SredDnevUb = GetSredDnevUb();
				account.Kalmar = GetKalmar();
				account.Sharp = GetSharp();
				account.Sortino = GetSortino();

				return account;
			}
			catch (Exception e)
			{
				Logger.LogMessage(e.Message);
				return null;
			}
		}
	}
}
