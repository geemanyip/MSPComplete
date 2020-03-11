using System;
using System.Collections.Generic;
using System.Linq;
using ManagementProxy.ManagementService;

namespace RetrieveRunbooks
{
    public class Program
    {
        private const string BitTitanApiUrl = "https://www.bittitan.com/Api/1.0/ManagementService.asmx";

        public static void Main(string[] args)
        {
            Console.Write("Enter your BitTitan username: ");
            string username = Console.ReadLine();

            Console.Write("Enter your BitTitan password: ");
            string password = Console.ReadLine();

            // create reference to BitTitan web service
            ManagementService bittitan = new ManagementService();
            bittitan.Url = BitTitanApiUrl;

            // generate authentication token to BitTitan service
            Ticket ticket = RetrieveTicket(bittitan, username, password);

            // retrieve all runbooks
            List<OfferingMetadata> runbooks = RetrieveRunbooks(bittitan, ticket);

            // filter out runbooks that don't have a name
            runbooks = runbooks.Where(r => !string.IsNullOrEmpty(r.Name)).ToList();

            Console.WriteLine();
            Console.WriteLine("Found {0} runbook(s).", runbooks.Count);
            Console.WriteLine();

            foreach (OfferingMetadata runbook in runbooks)
            {
                Console.WriteLine(runbook.Name);
            }
        }

        public static Ticket RetrieveTicket(ManagementService bittitan, string username, string password)
        {
            LoginRequest loginRequest = new LoginRequest() { EmailAddress = username, Password = password, ServiceType = ServiceType.BitTitan };
            LoginResponse loginResponse = bittitan.ExecuteAndCheck<LoginResponse>(loginRequest);
            return loginResponse.Ticket;
        }

        public static List<OfferingMetadata> RetrieveRunbooks(ManagementService bittitan, Ticket ticket)
        {
            List<OfferingMetadata> result = new List<OfferingMetadata>();

            RetrieveRequest retrieveRequest = null;
            RetrieveResponse retrieveResponse = null;
            int offset = 0; // object offset
            int pageSize = 100; // max page size is 100

            while (true)
            {
                // Retrieve the service list
                retrieveRequest = new RetrieveRequest()
                {
                    Ticket = ticket,
                    EntityName = typeof(OfferingMetadata).Name,
                    Count = pageSize,
                    Offset = offset * pageSize,
                    Condition = new Condition()
                    {
                        IsOrCondition = false,
                        Filters = new Filter[]
                        {
                            new Filter()
                            {
                                ComparisonOperator = ComparisonOperator.Equal,
                                PropertyName = "IsDeleted",
                                Value = false
                            }
                        }
                    },
                    Orders = new Order[]
                    {
                new Order()
                {
                    PropertyName = "Name",
                    IsAscending = true
                }
                    }
                };

                retrieveResponse = bittitan.ExecuteAndCheck<RetrieveResponse>(retrieveRequest);
                if (retrieveResponse != null && retrieveResponse.Entities != null && retrieveResponse.Entities.Length > 0)
                {
                    result.AddRange(retrieveResponse.Entities.OfType<OfferingMetadata>().ToList());
                }
                else
                {
                    break;
                }

                offset++;
            }

            return result;
        }
    }
}
