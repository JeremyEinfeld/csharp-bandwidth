﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Threading.Tasks;
using Bandwidth.Net.Data;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bandwidth.Net.Tests.Clients
{
    [TestClass]
    public class AccountTests
    {
        [TestMethod]
        public void GetTest()
        {
            using (ShimsContext.Create())
            {
                var account = new Account
                {
                    AccountType = AccountType.PrePay,
                    Balance = 100
                };
                ShimHttpClient.AllInstances.GetAsyncString = (c, url) =>
                {
                    Assert.AreEqual(string.Format("users/{0}/account", Helper.UserId), url);
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = Helper.CreateJsonContent(account)
                    };
                    return Task.Run(() => response);
                };
                using (var client = Helper.CreateClient())
                {
                    var result = client.Account.Get().Result;
                    Helper.AssertObjects(account, result);
                }
            }
        }

        [TestMethod]
        public void GetTransactionsTest()
        {
            using (ShimsContext.Create())
            {
                var transactions = new[]
                {
                    new AccountTransaction
                    {
                        Id = "1",
                        Type = AccountTransactionType.AutoRecharge,
                        Amount = 10,
                        Units = 1,
                        Time = DateTime.Now.AddMinutes(-10)
                    },
                    new AccountTransaction
                    {
                        Id = "2",
                        Type = AccountTransactionType.Payment,
                        Amount = 16,
                        Units = 2,
                        Time = DateTime.Now.AddMinutes(-15)
                    }
                };
                ShimHttpClient.AllInstances.GetAsyncString = (c, url) =>
                {
                    Assert.AreEqual(string.Format("users/{0}/account/transactions", Helper.UserId), url);
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = Helper.CreateJsonContent(transactions)
                    };
                    return Task.Run(() => response);
                };
                using (var client = Helper.CreateClient())
                {
                    var result = client.Account.GetTransactions().Result;
                    Assert.AreEqual(2, result.Length);
                    Helper.AssertObjects(transactions[0], result[0]);
                    Helper.AssertObjects(transactions[1], result[1]);
                }
            }
        }
    }
}
