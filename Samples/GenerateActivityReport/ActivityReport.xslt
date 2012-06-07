<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
								xmlns:msxml="urn:schemas-microsoft-com:xslt" 
>
  <xsl:output method="html" indent="yes"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Activity Report</title>

				<style type="text/css">
					table {
					display: table;
					border-collapse: separate;
					border-spacing: 2px;
					border-color: gray;
					border: 1px solid #CC9;
					border-spacing: 0px;
					}

					tr {
					display: table-row;
					vertical-align: inherit;
					border-color: inherit;
					}

					td {
					vertical-align: middle;
					font-size: small;
					font-family: verdana,arial,helvetica,sans-serif;
					margin: 0;
					}

					nobr {
					white-space: nowrap;
					}

					.padlt20 {
					padding-left: 20px;
					}

					.hdrdkorange {
					color: #E47911;
					font-size: 12px;
					font-weight: bold;
					}

					.alignrt {
					text-align: right
					}

					.right {
					float: right;
					}

					.bordgreytop {
					border-top: 1px solid #CCC;
					padding: 1px 4px 1px 4px;
					}

					.bordgreybot {
					border-bottom: 1px solid #CCC;
					padding: 1px 4px 1px 4px;
					}

					.bgyel {
					background-color: #FEFEE5;
					}

					.txtxxsm {
					font-size: xx-small;
					}

					.txtxsm {
					font-size: x-small;
					}

					.bold {
					font-weight: bold;
					}

					.taupeHeader {
					background-color: #EEC;
					font-size: 14px;
					font-weight: bold;
					padding: 8px;
					border-bottom: 1px solid #CC9;
					}
				</style>
			</head>
			<body>
				<div>
					<xsl:apply-templates />
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="AwsStatement">
		<h1>Activity Report</h1>
		<p>
			Start Date: <xsl:value-of select="@StartDate"/><br />
			End Date: <xsl:value-of select="@EndDate"/>
		</p>

		<xsl:apply-templates select="Accounts/AwsAccount" />

	</xsl:template>

	<xsl:template match="AwsAccount">

		<h3>
			Account: <xsl:value-of select="@AccountId"/>
		</h3>

		<xsl:apply-templates select="Invoices/AwsInvoice" />

	</xsl:template>

	<xsl:template match="AwsInvoice">

		<h3>
			Invoice: <xsl:value-of select="@InvoiceId"/>
		</h3>

		<table border="0" cellspacing="0" cellpadding="0">
			<tbody>
				<tr>
					<td class="taupeheader bordgreybot bordgreytop" colspan="5">
						<span class="right">
							$<xsl:value-of select="format-number(@Total, '#,###,##0.00')"/>
						</span>
						AWS Service Charges
					</td>
				</tr>
				<xsl:apply-templates select="Products/AwsProduct" />
			</tbody>
		</table>

	</xsl:template>

	<xsl:template match="AwsProduct">

		<tr>
			<td class="bordgreybot" style="padding-top: 4px;" colspan="4">
				<span class="hdrdkorange">
					<nobr><xsl:value-of select="@ProductName"/></nobr>
				</span>
			</td>
			<td class="bold bordgreybot alignrt" style="font-size: 12px">
					$<xsl:value-of select="format-number(sum(Items/AwsLineItem/@CostBeforeTaxes), '#,###,##0.00')"/>
			</td>
		</tr>
		
		<xsl:variable name="sorteditems">
			<xsl:for-each select="Items/AwsLineItem">
				<xsl:sort select="@Region" order="ascending"/>
				<xsl:sort select="@Category" order="ascending"/>
				<xsl:copy-of select="current()"/>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="relItems" select="msxml:node-set($sorteditems)" />
		
		<xsl:for-each select="$relItems/AwsLineItem">

			<xsl:variable name="renderRegion">
				<xsl:choose>
					<xsl:when test="not(preceding::AwsLineItem[1]/@Region = @Region)">1</xsl:when>
					<xsl:otherwise>0</xsl:otherwise>
				</xsl:choose>

			</xsl:variable>
			<xsl:variable name="renderCategory">
				<xsl:choose>
					<xsl:when test="not(preceding::AwsLineItem[1]/@Region = @Region)">1</xsl:when>
					<xsl:when test="not(preceding::AwsLineItem[1]/@Category = @Category)">1</xsl:when>
					<xsl:otherwise>0</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:if test="$renderRegion = 1">
				<xsl:if test="@Region">
					<tr>
						<td class="bordgreybot bold txtxsm" colspan="5">
							<xsl:value-of select="@RegionName" />
						</td>
					</tr>
				</xsl:if>
			</xsl:if>
			
			<xsl:if test="$renderCategory = 1">
				<xsl:if test="@Category">
					<tr>
						<td class="bordgreybot" width="14"> </td>
						<td class="bordgreybot bold txtxsm" colspan="4">
							<xsl:value-of select="@CategoryName" />
						</td>
					</tr>
				</xsl:if>
			</xsl:if>

			<tr>
				<td class="bordgreybot" width="14"> </td>
				<td class="bordgreybot txtxxsm" width="10%"> </td>
				<td class="bordgreybot txtxsm">
					<xsl:value-of select="@Description"/>
				</td>
				<td class="bordgreybot txtxsm">
					<xsl:value-of select="@FormattedUsageQuantity"/>
				</td>
				<td class="bordgreybot txtxsm alignrt">
					$<xsl:value-of select="format-number(@CostBeforeTaxes, '#,###,##0.00')"/>
				</td>
			</tr>
			
		</xsl:for-each>

	</xsl:template>

</xsl:stylesheet>
