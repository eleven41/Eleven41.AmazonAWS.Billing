using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eleven41.AmazonAWS.Billing
{
	public class AwsAccount
	{
		[XmlAttribute]
		public string AccountId { get; set; }

		public List<AwsInvoice> Invoices { get; set; }
	}
}
