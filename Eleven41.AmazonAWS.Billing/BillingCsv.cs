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
		// Opens the specified file and processes the CSV data into
		// an AwsStatement object.
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

		// Processes the specified stream as CSV data and creates
		// an AwsStatement object.
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
			// Split the CSV data into individual line item records
			var items = ParseCsv(reader);

			// Create our resulting statement object
			AwsStatement statement = new AwsStatement();
			statement.Accounts = new List<AwsAccount>();

			// Get a list of the distinct payer accounts in the data
			var payerAccounts = items.Where(i => !String.IsNullOrEmpty(i.PayerAccountID)).Select(i => i.PayerAccountID).Distinct();

			// For each account, 
			//  create an account object
			//  add it to the statement
			//  and fill it with data
			foreach (var accountId in payerAccounts)
			{
				// Get the items that apply to this account
				var accountItems = items.Where(i => i.PayerAccountID == accountId);

				// Create an account object
				AwsAccount account = new AwsAccount();
				account.Invoices = new List<AwsInvoice>();
				account.AccountId = accountId;
				statement.Accounts.Add(account);

				// Get the list of invoices from the items which
				// apply to the current account
				var invoiceIds = accountItems.Where(i => !String.IsNullOrEmpty(i.InvoiceID)).Select(i => i.InvoiceID).Distinct();

				// For each invoice
				//  create an invoice object
				//  add it to the account object
				//  and fill it with data
				foreach (var invoiceId in invoiceIds)
				{
					// Get the list of items for the current account 
					// and the current invoice
					var invoiceItems = accountItems.Where(i => i.InvoiceID == invoiceId).Select(i => i);
					System.Diagnostics.Debug.Assert(invoiceItems.Count() > 0);

					// Create an invoice object
					AwsInvoice invoice = new AwsInvoice();
					invoice.InvoiceId = invoiceId;
					invoice.Products = new List<AwsProduct>();
					invoice.InvoiceDate = invoiceItems.First().InvoiceDate.Value;
					account.Invoices.Add(invoice);

					// Get the line items for this invoice
					var lineItems = invoiceItems
						.Where(i => i.RecordType == "PayerLineItem")
						.Select(i => i);

					// Get the list of unique products used 
					// by this invoice
					var productCodes = lineItems.Select(i => i.ProductCode).Distinct().OrderBy(i => i);

					// For each product
					//  create a product object
					//  add it to the invoice
					//  and fill it with data
					foreach (var productCode in productCodes)
					{
						// Get the list of line items which only apply to this product
						var productLineItems = lineItems.Where(i => i.ProductCode == productCode).Select(i => i).OrderBy(i => i.UsageType);
						System.Diagnostics.Debug.Assert(productLineItems.Count() > 0);

						// We need a product name, so get it from one of the items
						string productName = productLineItems.First().ProductName;

						// Create our product object
						AwsProduct product = new AwsProduct();
						product.Items = new List<AwsLineItem>();
						product.ProductCode = productCode;
						product.ProductName = productName;
						invoice.Products.Add(product);

						// For each line item
						//  create a line item object
						//  and populate it's data.
						foreach (var lineItem in productLineItems)
						{
							// Create our line item object and fill it with data
							AwsLineItem item = new AwsLineItem();
							item.Description = lineItem.ItemDescription;
							item.UsageQuantity = lineItem.UsageQuantity.Value;
							item.FormattedUsageQuantity = Format3Or6(item.UsageQuantity);
							item.CostBeforeTaxes = lineItem.CostBeforeTaxes.Value;
							item.UsageType = lineItem.UsageType;

							// Find an appropriate region and category for this item/product.
							SetItemRegion(item, product);
							SetItemCategory(item, product, lineItem.Operation);
							
							product.Items.Add(item);
						}
					}

					// Try to find an invoice total amongst the chaos.
					// If one is found, then save some of it's data
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

			// Try to find an statement total amongst the chaos.
			// If one is found, then save some of it's data
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

		// Finds an appropriate region for the supplied item in the product's list of
		// regions.  If one is found, then return the existing item.
		// If one is not found, create one, add it to the product's list, and return it.
		// Some products don't use regions.  In these cases, return null.
		private static void SetItemRegion(AwsLineItem item, AwsProduct product)
		{
			// These products don't use regions
			if (product.ProductCode == "AmazonRoute53")
				return;
			else if (product.ProductCode == "AmazonSES")
				return;

			string usageType = item.UsageType;

			// Default to US East
			string code = "us-east-1";
			string name = "US East (Northern Virginia) Region";

			// S3 uses a different name for US East
			if (product.ProductCode == "AmazonS3")
				name = "US Standard Region";
			else if (product.ProductCode == "AWSDataTransfer")
				name = "US East (Northern Virginia) and US Standard Regions";

			// If usage type starts with special prefixes, then that means
			// it belongs to a particular region that is not US East.
			if (usageType.StartsWith("US-"))
			{
				// No change to region, is US East (Virginia)
				item.UsageType = item.UsageType.Substring(3);
			} 
			else if (usageType.StartsWith("USW1-"))
			{
				code = "us-west-1";
				name = "US West (Northern California) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("USW2-"))
			{
				code = "us-west-2";
				name = "US West (Oregon) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("EU-"))
			{
				code = "eu-west-1";
				name = "EU West (Ireland) Region";
				item.UsageType = item.UsageType.Substring(3);
			}
			else if (usageType.StartsWith("APS1-"))
			{
				code = "ap-southeast-1";
				name = "Asia Pacific (Singapore) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("APN1-"))
			{
				code = "ap-northeast-1";
				name = "Asia Pacific (Tokyo) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("UGW1-"))
			{
				code = "us-gov-west-1";
				name = "GovCloud (US) Region";
				item.UsageType = item.UsageType.Substring(5);
			}
			else if (usageType.StartsWith("SAE1-"))
			{
				code = "sa-east-1";
				name = "South America (Sao Paulo) Region";
				item.UsageType = item.UsageType.Substring(5);
			}

			item.Region = code;
			item.RegionName = name;
		}

		private static void SetItemCategory(AwsLineItem item, AwsProduct product, string operation)
		{
			if (product.ProductCode != "AmazonEC2" &&
				product.ProductCode != "AmazonRDS")
				return;

			string usageType = item.UsageType;

			string code = null;
			string name = null;

			if (usageType.StartsWith("EBS:"))
			{
				code = "EBS";
				name = "Amazon EC2 EBS";
				item.UsageType = item.UsageType.Substring(4);
			}
			else if (usageType.StartsWith("CW:"))
			{
				code = "CW";
				name = "Amazon CloudWatch";
				item.UsageType = item.UsageType.Substring(3);
			}
			else if (usageType.StartsWith("ElasticIP:"))
			{
				code = "ElasticIP";
				name = "Elastic IP Addresses";
				item.UsageType = item.UsageType.Substring(10);
			}
			else if (usageType.StartsWith("SpotUsage:"))
			{
				code = "SpotUsage";
				name = "Spot Instances";
			}
			else if (operation == "LoadBalancing")
			{
				code = operation;
				name = "Elastic Load Balancing";
			}
			else if (operation == "RunInstances")
			{
				code = operation;
				name = "Amazon EC2 running Linux/UNIX";
			}
			else if (operation == "RunInstances:0002")
			{
				code = operation;
				name = "Amazon EC2 running Windows";
			}
			else if (operation == "RunInstances:0006")
			{
				code = operation;
				name = "Amazon EC2 running Windows with SQL Server";
			}
			else if (operation == "CreateDBInstance" ||
				usageType == "RDS:StorageIOUsage")
			{
				code = "CreateDBInstance";
				name = "Amazon RDS Storage";
			}
			else if (usageType == "RDS:ChargedBackupUsage")
			{
				code = usageType;
				name = "Amazon RDS Automated Backups";
			}
			else if (operation == "CreateDBInstance:0002")
			{
				code = operation;
				name = "Amazon RDS for MySQL Community Edition";
			}

			item.Category = code;
			item.CategoryName = name;
		}

		// Format the specified number to 0, 3 or 6 decimal digits as necessary.
		// If the number is very small, then use 6 digits.
		// If the number is a whole number, then use 0.
		// Otherwise, use 3.
		private static string Format3Or6(double d)
		{
			string result = d.ToString("N3");
			if (result == "0.000")
			{
				// The number is very small and cannot be represented by 3 decimal digits.
				// Use 6 instead.
				result = d.ToString("#,0.######");
			}
			else if (result.EndsWith(".000"))
			{
				// The number is a whole number, so remove the decimal.
				result = result.Substring(0, result.Length - 4);
			}
			return result;
		}

		// Use FileHelper to parse the CSV file into line items.
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
	}
}
