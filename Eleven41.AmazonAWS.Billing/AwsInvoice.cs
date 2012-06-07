using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eleven41.AmazonAWS.Billing
{
	public class AwsInvoice
	{
		[XmlAttribute]
		public string InvoiceId { get; set; }

		[XmlAttribute]
		public DateTime InvoiceDate { get; set; }

		[XmlAttribute]
		public DateTime StartDate { get; set; }

		[XmlAttribute]
		public DateTime EndDate { get; set; }

		public List<AwsProduct> Products { get; set; }

		[XmlAttribute]
		public double Taxes { get; set; }

		[XmlAttribute]
		public double Total { get; set; }
	}
}
