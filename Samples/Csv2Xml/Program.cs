using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Csv2Xml
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Csv2Xml <input csv> <output xml>");
				return;
			}

			string inputCsv = args[0];
			string outputXml = args[1];
			
			var statement = Eleven41.AmazonAWS.Billing.BillingCsv.ReadFile(inputCsv);

			foreach (var account in statement.Accounts)
			{
				Console.WriteLine("Account: {0}", account.AccountId);
				foreach (var invoice in account.Invoices)
				{
					Console.WriteLine("\tInvoice: {0}", invoice.InvoiceId);
					Console.WriteLine("\tInvoice Total: {0}", invoice.Total);
				}
			}
			Console.WriteLine("Statement Total: ${0:F2}", statement.Total);

			using (var fileStream = new FileStream(outputXml, FileMode.Create))
			{
				SerializeToStream(statement, fileStream);
				fileStream.Close();
			}
		}

		static void SerializeToStream<T>(T obj, Stream stream)
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
			serializer.Serialize(stream, obj);
		}
	}
}
