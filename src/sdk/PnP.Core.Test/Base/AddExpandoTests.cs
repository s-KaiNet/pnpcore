﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on adding data via REST or Microsoft Graph - used to test the expando core data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class AddExpandoTests
    {
        private const string ItemTitleValue = "Yesssss";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task AddListItemViaRest()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "AddListItemViaRest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }

                // get items from the list
                await myList.GetAsync(p => p.Items);

                int listItemCount = myList.Items.Count();

                // Add a list item
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "Title", "Yes" }
                };
                var item = await myList.Items.AddAsync(values);

                Assert.IsTrue(item.Requested);
                Assert.IsTrue(item.Id >= 0);
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);

                // Load the list again, include extra list property
                await myList.GetAsync(p => p.Items);

                // Should still have the same amount of items
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);
                // Additional list item fields should be available
                Assert.IsTrue(item.Values["ContentType"].ToString() == "Item");
                dynamic dynamicItem = item;
                Assert.IsTrue(dynamicItem.ContentType == "Item");
                Assert.IsTrue(dynamicItem["ContentType"] == "Item");
            }
        }

        [TestMethod]
        public async Task AddListItemViaRestNonAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);

                string listTitle = "AddListItemViaRestNonAsync";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);
                }

                // get items from the list
                myList.Get(p => p.Items);

                int listItemCount = myList.Items.Count();

                // Add a list item
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "Title", "Yes" }
                };
                var item = myList.Items.Add(values);

                Assert.IsTrue(item.Requested);
                Assert.IsTrue(item.Id >= 0);

                Assert.IsTrue(myList.Items.Contains(item.Id));
                Assert.IsFalse(myList.Items.Contains(item.Id + 1));

                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);

                // Load the list again, include extra list property
                myList.Get(p => p.Items);

                // Should still have the same amount of items
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);
                // Additional list item fields should be available
                Assert.IsTrue(item.Values["ContentType"].ToString() == "Item");
                dynamic dynamicItem = item;
                Assert.IsTrue(dynamicItem.ContentType == "Item");
                Assert.IsTrue(dynamicItem["ContentType"] == "Item");
            }
        }

        [TestMethod]
        public async Task AddListItemViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = await context.Web.GetBatchAsync(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListItemViaBatchRest";
                var myList = web.Lists.FirstOrDefault(l => l.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    batch = context.BatchClient.EnsureBatch();
                    myList = await web.Lists.AddBatchAsync(batch, listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync(batch);
                }

                // get items from the list
                batch = context.BatchClient.EnsureBatch();
                await myList.GetBatchAsync(batch, p => p.Items);
                await context.ExecuteAsync(batch);

                int listItemCount = myList.Items.Count();

                // Add a list item
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "Title", ItemTitleValue }
                };
                batch = context.BatchClient.EnsureBatch();
                var item = await myList.Items.AddBatchAsync(batch, values);
                await context.ExecuteAsync(batch);

                Assert.IsTrue(item.Requested);
                Assert.IsTrue(item.Id >= 0);
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);

                // Load the list again, include extra list property
                batch = context.BatchClient.EnsureBatch();
                await myList.GetBatchAsync(batch, p => p.Items);
                await context.ExecuteAsync(batch);

                // Should still have the same amount of items
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);
                // Additional list item fields should be available
                Assert.IsTrue(item.Values["ContentType"].ToString() == "Item");
                dynamic dynamicItem = item;
                Assert.IsTrue(dynamicItem.ContentType == "Item");
                Assert.IsTrue(dynamicItem["ContentType"] == "Item");
            }
        }

        [TestMethod]
        public async Task AddListItemViaSpecificBatchNonAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = context.Web.GetBatch(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListItemViaSpecificBatchNonAsyncTest";
                var myList = web.Lists.FirstOrDefault(l => l.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    batch = context.BatchClient.EnsureBatch();
                    myList = await web.Lists.AddBatchAsync(batch, listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync(batch);
                }

                // get items from the list
                batch = context.BatchClient.EnsureBatch();
                myList.GetBatch(batch, p => p.Items);
                await context.ExecuteAsync(batch);

                int listItemCount = myList.Items.Count();

                // Add a list item
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "Title", ItemTitleValue }
                };
                batch = context.BatchClient.EnsureBatch();
                var item = myList.Items.AddBatch(batch, values);
                await context.ExecuteAsync(batch);

                Assert.IsTrue(item.Requested);
                Assert.IsTrue(item.Id >= 0);
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);

                // Load the list again, include extra list property
                batch = context.BatchClient.EnsureBatch();
                await myList.GetBatchAsync(batch, p => p.Items);
                await context.ExecuteAsync(batch);

                // Should still have the same amount of items
                Assert.IsTrue(myList.Items.Count() == listItemCount + 1);
                // Additional list item fields should be available
                Assert.IsTrue(item.Values["ContentType"].ToString() == "Item");
                dynamic dynamicItem = item;
                Assert.IsTrue(dynamicItem.ContentType == "Item");
                Assert.IsTrue(dynamicItem["ContentType"] == "Item");
            }
        }

        [TestMethod]
        public async Task AddListItemViaRestExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = context.Web.GetBatch(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListItemViaRestExceptionTest";
                var myList = web.Lists.FirstOrDefault(l => l.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }
                else
                {
                    batch = context.BatchClient.EnsureBatch();
                    myList = await web.Lists.AddBatchAsync(batch, listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync(batch);
                }

                // get items from the list
                batch = context.BatchClient.EnsureBatch();
                myList.GetBatch(batch, p => p.Items);
                await context.ExecuteAsync(batch);

                int listItemCount = myList.Items.Count();

                // Add a list item
                Dictionary<string, object> values = new Dictionary<string, object>
                {
                    { "Title", ItemTitleValue }
                };

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    batch = context.BatchClient.EnsureBatch();
                    var item = myList.Items.AddBatch(batch, null);
                    await context.ExecuteAsync(batch);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var item = await myList.Items.AddAsync(null);
                });

            }
        }
        #endregion
    }
}
