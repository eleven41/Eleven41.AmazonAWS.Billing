using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FileHelpers;

namespace Eleven41.AmazonAWS.Billing
{
	public static class BillingCsv
	{
		public static AwsStatement ReadFile(string file)
		{
			using (var reader = new System.IO.StreamReader(file))
			{
				try
				{
					return Read(reader);
				}
				finally
				{
					reader.Close();
				}
			}
		}

		public static AwsStatement ReadStream(System.IO.Stream stream)
		{
			using (var reader = new System.IO.StreamReader(stream))
			{
				try
				{
					return Read(reader);
				}
				finally
				{
					reader.Close();
				}
			}
		}

		private static AwsStatement Read(System.IO.TextReader reader)
		{
			var items = ParseCsv(reader);

			AwsStatement statement = new AwsStatement();
			statement.Accounts = new List<AwsAccount>();

			var payerAccounts = items.Where(i => !String.IsNullOrEmpty(i.PayerAccountID)).Select(i => i.PayerAccountID).Distinct();

			foreach (var accountId in payerAccounts)
			{
				var accountItems = items.Where(i => i.PayerAccountID == accountId);

				AwsAccount account = new AwsAccount();
				account.Invoices = new List<AwsInvoice>();
				account.AccountId = accountId;
				statement.Accounts.Add(account);

				var invoiceIds = accountItems.Where(i => !String.IsNullOrEmpty(i.InvoiceID)).Select(i => i.InvoiceID).Distinct();

				foreach (var invoiceId in invoiceIds)
				{
					var invoiceItems = accountItems.Where(i => i.InvoiceID == invoiceId).Select(i => i);

					AwsInvoice invoice = new AwsInvoice();
					invoice.InvoiceId = invoiceId;
					invoice.Products = new List<AwsProduct>();
					invoice.InvoiceDate = invoiceItems.First().InvoiceDate.Value;
					account.Invoices.Add(invoice);

					var lineItems = invoiceItems
						.Where(i => i.RecordType == "PayerLineItem")
						.Select(i => i);

					var productCodes = lineItems.Select(i => i.ProductCode).Distinct().OrderBy(i => i);

					foreach (var productCode in productCodes)
					{
						var productLineItems = lineItems.Where(i => i.ProductCode == productCode).Select(i => i).OrderBy(i => i.UsageType);

						string productName = productLineItems.First().ProductName;

						AwsProduct product = new AwsProduct();
						product.ProductCode = productCode;
						product.ProductName = productName;
						invoice.Products.Add(product);

						foreach (var lineItem in productLineItems)
						{
							AwsLineItem item = new AwsLineItem();
							item.Description = lineItem.ItemDescription;
							item.UsageQuantity = lineItem.UsageQuantity.Value;
							item.FormattedUsageQuantity = Format3Or6(item.UsageQuantity);
							item.CostBeforeTaxes = lineItem.CostBeforeTaxes.Value;
							item.UsageType = lineItem.UsageType;

							AwsRegion region = FindOrCreateItemRegion(item, product);

							if (region != null)
							{
								if (region.Items == null)
									region.Items = new List<AwsLineItem>();
								region.Items.Add(item);
							}
							else
							{
								if (product.NonRegionItems == null)
									product.NonRegionItems = new List<AwsLineItem>();
								product.NonRegionItems.Add(item);
							}
						}
					}

					var invoiceTotal = invoiceItems.Where(i => i.RecordType == "InvoiceTotal").Select(i => i).FirstOrDefault();
					if (invoiceTotal != null)
					{
						invoice.StartDate = invoiceTotal.BillingPeriodStartDate.Value;
						invoice.EndDate = invoiceTotal.BillingPeriodEndDate.Value;
						invoice.Taxes = invoiceTotal.TaxAmount.Value;
						invoice.Total = invoiceTotal.TotalCost.Value;
					}
				}

				// Account total?
			}

			var statementTotal = items.Where(i => i.RecordType == "StatementTotal").Select(i => i).FirstOrDefault();
			if (statementTotal != null)
			{
				statement.StartDate = statementTotal.BillingPeriodStartDate.Value;
				statement.EndDate = statementTotal.BillingPeriodEndDate.Value;
				statement.Taxes = statementTotal.TaxAmount.Value;
				statement.Total = statementTotal.TotalCost.Value;
			}

			return statement;
		}

		private static AwsRegion FindOrCreateItemRegion(AwsLineItem item, AwsProduct product)
		{
			// These products don't use regions
			if (product.ProductCode == "AmazonRoute53")
				return null;
			else if (product.ProductCode == "AWSDataTransfer")
				return null;
			else if (product.ProductCode == "AmazonSES")
				return null;

			string usageType = item.UsageType;

			// Default to US East
			string code = "us-east-1";
			string name = "US East (Northern Virginia) Region";

			// S3 uses a different name for US East
			if (product.ProductCode == "AmazonS3")
				name = "US Standard Region";

			// If usage type starts with special prefixes, then that means
			// it belongs to a particular region that is not US East.
			if (usageType.StartsWith("USW1-"))
			{
				code = "us-west-1";
				name = "US West (California) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("USW2-"))
			{
				code = "us-west-2";
				name = "US West (Oregon) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("EUW1-"))
			{
				code = "eu-west-1";
				name = "EU West (Ireland) Region";
				item.UsageType = item.UsageType.Substring(5);
			}

			AwsRegion region = null;
			if (product.Regions != null)
				region = product.Regions.Where(r => r.RegionCode == code).Select(r => r).FirstOrDefault();
			if (region == null)
			{
				region = new AwsRegion()
				{
					RegionCode = code,
					RegionName = name
				};
				if (product.Regions == null)
					product.Regions = new List<AwsRegion>();
				product.Regions.Add(region);
			}
			return region;
		}

		private static string Format3Or6(double d)
		{
			string result = d.ToString("N3");
			if (result == "0.000")
			{
				result = d.ToString("#,0.######");
			}
			else if (result.EndsWith(".000"))
			{
				result = result.Substring(0, result.Length - 4);
			}
			return result;
		}

		private static AwsCsvItem[] ParseCsv(System.IO.TextReader reader)
		{
			List<AwsCsvItem> results = new List<AwsCsvItem>();

			FileHelperAsyncEngine engine = new FileHelperAsyncEngine(typeof(AwsCsvItem));

			// Read
			engine.BeginReadStream(reader);

			// The engine is IEnumerable 
			foreach (AwsCsvItem item in engine)
			{
				results.Add(item);
			}

			engine.Close();

			return results.ToArray();
		}

		private static DateTime? GetDateTime(string s)
		{
			if (String.IsNullOrEmpty(s))
				return null;

			return DateTime.Parse(s);
		}
	}
}
