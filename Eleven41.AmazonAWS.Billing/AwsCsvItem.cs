using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;

namespace Eleven41.AmazonAWS.Billing
{
	[DelimitedRecord(",")]
	[IgnoreFirst(1)]
	internal class AwsCsvItem
	{
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string InvoiceID;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string PayerAccountID;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string LinkedAccountID;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string RecordType;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string RecordID;

		[FieldConverter(ConverterKind.Date, "yyyy/MM/dd HH:mm:ss")]
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public DateTime? BillingPeriodStartDate;

		[FieldConverter(ConverterKind.Date, "yyyy/MM/dd HH:mm:ss")]
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public DateTime? BillingPeriodEndDate;

		[FieldConverter(ConverterKind.Date, "yyyy/MM/dd HH:mm:ss")]
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public DateTime? InvoiceDate;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string PayerAccountName;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string LinkedAccountName;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string TaxationAddress;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string PayerPONumber;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string ProductCode;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string ProductName;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string SellerOfRecord;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string UsageType;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string Operation;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string RateId;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string ItemDescription;

		[FieldConverter(ConverterKind.Date, "yyyy/MM/dd HH:mm:ss")]
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public DateTime? UsageStartDate;

		[FieldConverter(ConverterKind.Date, "yyyy/MM/dd HH:mm:ss")]
		[FieldQuoted(QuoteMode.OptionalForRead)]
		public DateTime? UsageEndDate;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? UsageQuantity;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? BlendedRate;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string CurrencyCode;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? CostBeforeTaxes;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? Credits;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? TaxAmount;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public string TaxType;

		[FieldQuoted(QuoteMode.OptionalForRead)]
		public double? TotalCost;
	}
}
