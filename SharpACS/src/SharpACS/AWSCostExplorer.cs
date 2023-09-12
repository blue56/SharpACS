using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CostExplorer.Model;
using Amazon.CostExplorer;
using Amazon.Organizations;
using System.Globalization;
using System.Text.Json;
using Amazon.Organizations.Model;

namespace SharpACS
{
    public class AWSCostExplorer
    {
        public Cost[] Execute(AWSCostExplorerRequest Request)
        {
            List<GroupDefinition> _groupDefinitions = new List<GroupDefinition>();

            /*            GroupDefinition groupDefinition1 = new GroupDefinition();
                        groupDefinition1.Type = "COST_CATEGORY";
                        groupDefinition1.Key = CostCategoryName;
                        _groupDefinitions.Add(groupDefinition1);
            */
            string groupBy2Type = "DIMENSION";
            string groupBy2Key = "LINKED_ACCOUNT";

            GroupDefinition groupDefinition2 = new GroupDefinition();
            groupDefinition2.Type = groupBy2Type;
            groupDefinition2.Key = groupBy2Key;

            _groupDefinitions.Add(groupDefinition2);

            RegionEndpoint _region = RegionEndpoint.GetBySystemName(Request.Region);

            Amazon.Runtime.BasicAWSCredentials credentials =
                new Amazon.Runtime.BasicAWSCredentials(Request.AccessKey, Request.SecretKey);

            // Call AWS Organization
            AmazonOrganizationsClient amazonOrganizationsClient =
                new AmazonOrganizationsClient(credentials, _region);

            Amazon.Organizations.Model.ListAccountsRequest listAccountsRequest
                = new Amazon.Organizations.Model.ListAccountsRequest();

            ListAccountsResponse accountsResponse = amazonOrganizationsClient.ListAccountsAsync(listAccountsRequest).Result;

            List<Account> accountList = new List<Account>();
            accountList.AddRange(accountsResponse.Accounts);

            if (accountsResponse.NextToken != null)
            {                
                do
                {
                    listAccountsRequest.NextToken = accountsResponse.NextToken;
                    accountsResponse = amazonOrganizationsClient.ListAccountsAsync(listAccountsRequest).Result;
                }
                while (accountsResponse.NextToken != null);
            }

            var accounts = accountList.ToArray();

            // Start Cost Explorer

            AmazonCostExplorerClient amazonCostExplorerClient
                = new AmazonCostExplorerClient(credentials, _region);

            GetCostAndUsageRequest getCostAndUsageRequest = new GetCostAndUsageRequest();

            getCostAndUsageRequest.Granularity = Granularity.MONTHLY;

            Amazon.CostExplorer.Model.DateInterval dateInterval = new Amazon.CostExplorer.Model.DateInterval();

            // e.g. "2022-12-12"
            string startDate = Request.Year + "-" + Request.Month.ToString("D2") + "-01";

            dateInterval.Start = startDate;

            int endMonth = (Request.Month + 1) % 12;

            if (endMonth == 0)
            {
                endMonth = 12;
            }

            int endYear = Request.Year;
            if (Request.Month == 12)
            {
                endYear = Request.Year + 1;
            }

            string endDate = endYear + "-" + endMonth.ToString("D2") + "-01";

            dateInterval.End = endDate;

            getCostAndUsageRequest.TimePeriod = dateInterval;

            getCostAndUsageRequest.Metrics.Add(Request.Metric);

            getCostAndUsageRequest.GroupBy.Clear();
            getCostAndUsageRequest.GroupBy.AddRange(_groupDefinitions);

            // Tax
            if (Request.Tax == false)
            {
                var taxExpression = new Amazon.CostExplorer.Model.Expression();
                taxExpression.Dimensions = new DimensionValues();
                taxExpression.Dimensions.Key = Amazon.CostExplorer.Dimension.SERVICE;
                taxExpression.Dimensions.Values = new List<string>();
                taxExpression.Dimensions.Values.Add("Tax");

                var notTaxExpression = new Amazon.CostExplorer.Model.Expression();
                notTaxExpression.Not = taxExpression;

                // Expression
                //Expression expression = new Expression();

                // Add not Tax expression
                //expression.And.Add(notTaxExpression);

                // Add expression to request as filter
                //getCostAndUsageRequest.Filter = expression;
                getCostAndUsageRequest.Filter = notTaxExpression;
            }

            var f = amazonCostExplorerClient.GetCostAndUsageAsync(getCostAndUsageRequest).Result;

            var usCulture = new CultureInfo("en-US");

            bool final = false;

            if (f.ResultsByTime[0].Estimated)
            {
                final = false;
            }
            else
            {
                final = true;
            }

            List<Cost> costList = new List<Cost>();

            foreach (var item in f.ResultsByTime[0].Groups)
            {
                //var costCategoryId = item.Keys[0];
                //var costCategoryId = item.Keys[0];
                //var accountId = item.Keys[1];
                var accountId = item.Keys[0];
                var amountStr = item.Metrics.First().Value.Amount;

                var account = accounts.FirstOrDefault(x => x.Id == accountId);

                Cost cost = new Cost();
                var a = decimal.Parse(amountStr, usCulture);

                cost.Amount = System.Math.Round(a, 2);

                cost.ResourceId = accountId;
                cost.ResourceType = "Account";
                //cost.Name = account.Name + " (" + account.Id + ")";

                if (account != null)
                    cost.Name = account.Name;

                cost.Currency = item.Metrics.First().Value.Unit;
                cost.Service = "AWS";
                cost.Year = Request.Year;
                cost.Month = Request.Month;
                cost.Final = final;

                costList.Add(cost);
            }

            return costList.ToArray();
        }
    }
}