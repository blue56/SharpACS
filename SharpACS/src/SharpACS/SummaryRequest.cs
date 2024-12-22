using System.Globalization;
using Amazon;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Amazon.Organizations;
using Amazon.Organizations.Model;
using CsvHelper;

namespace SharpACS
{
    public class SummaryRequest : Request
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Metric { get; set; }
        public bool Tax { get; set; }
        public string Period { get; set; }
        public string S3Bucketname { get; set; }
        public string S3KeyPattern { get; set; }
        public string? Format { get; set; }

        public void Run()
        {
            var period = DatePeriodTranslator.Translate(Period);

            var costs = ExtractCost(period);
            Write(period, costs);
        }

        private List<Cost> ExtractCost(DatePeriod period)
        {
            List<GroupDefinition> _groupDefinitions = new List<GroupDefinition>();

            string groupBy2Type = "DIMENSION";
            string groupBy2Key = "LINKED_ACCOUNT";

            GroupDefinition groupDefinition2 = new GroupDefinition();
            groupDefinition2.Type = groupBy2Type;
            groupDefinition2.Key = groupBy2Key;

            _groupDefinitions.Add(groupDefinition2);

            RegionEndpoint _region = RegionEndpoint.GetBySystemName(Region);

            Amazon.Runtime.BasicAWSCredentials credentials =
                new Amazon.Runtime.BasicAWSCredentials(AccessKey, SecretKey);

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
                    accountList.AddRange(accountsResponse.Accounts);
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

            dateInterval.Start = period.StartDate.ToString("yyyy-MM-dd");
            dateInterval.End = period.EndDate.ToString("yyyy-MM-dd");

            getCostAndUsageRequest.TimePeriod = dateInterval;

            getCostAndUsageRequest.Metrics.Add(Metric);

            getCostAndUsageRequest.GroupBy.Clear();
            getCostAndUsageRequest.GroupBy.AddRange(_groupDefinitions);

            // Tax
            if (Tax == false)
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

            if (f.ResultsByTime[0].Estimated == true)
            {
                // Fail
                throw new ApplicationException("Estimated cost found");
            }

            List<Cost> costList = new List<Cost>();

            foreach (var item in f.ResultsByTime[0].Groups)
            {
                var accountId = item.Keys[0];
                var amountStr = item.Metrics.First().Value.Amount;
                var curreny = item.Metrics.First().Value.Unit;

                var account = accounts.FirstOrDefault(x => x.Id == accountId);

                Cost cost = new Cost();
                var a = decimal.Parse(amountStr, usCulture);

                cost.Amount = System.Math.Round(a, 2);
                cost.Currency = curreny;

                cost.ResourceId = accountId;
                cost.ResourceType = "Account";
                //cost.Name = account.Name + " (" + account.Id + ")";

                if (account != null)
                    cost.Name = account.Name;

                costList.Add(cost);
            }

            return costList;
        }

        private void Write(DatePeriod period, List<Cost> Costs)
        {
            MemoryStream ms = new MemoryStream();
            TextWriter tw = new StreamWriter(ms);
            var csv = new CsvWriter(tw, CultureInfo.InvariantCulture);
            csv.WriteRecords(Costs);
            tw.Flush();

            string Key = period.GenerateKey(S3KeyPattern);
            ms.Seek(0, SeekOrigin.Begin);
            Helpers.SaveFile(Region, S3Bucketname, Key, ms, "application/csv");
        }
    }
}