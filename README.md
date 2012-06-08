# Eleven41.AmazonAWS.Billing
Library for handling Amazon Web Services billing.

Copyright (C) 2012, Eleven41 Software

## LICENSE
[MIT License](https://github.com/eleven41/Eleven41.AmazonAWS.Billing/blob/master/LICENSE.md)

## REQUIREMENTS

* Visual Studio 2010

## WHAT IT DOES

This library will process an Amazon Web Services billing CSV file and generate an object model based on the data.

* Statement
 * Account
  * Invoice
   * Product
    * Line items
     

The goal is to get the object model in a format where the activity reports can be re-created.

Currently, linked accounts are ignored.

## MORE INFORMATION

Additional information can be found in the [Wiki](https://github.com/eleven41/Eleven41.AmazonAWS.Billing/wiki) including
assumptions and assertions.