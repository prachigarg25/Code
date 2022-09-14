using System;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace Connection
{
    internal class Program
    {

        static void Main(string[] args)
        {
            string url = "https://onegaming.crm6.dynamics.com";
            string userName = "D365Admin@firstsovereign.co.nz";
            string password = "Sachin248";
            string conn = $@"
        Url = {url};
        AuthType = OAuth;
        UserName = {userName};
        Password = {password};
        AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
        RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
        LoginPrompt=Auto;
        RequireNewInstance = True
        ";
            using (var svc = new CrmServiceClient(conn))
            {
                WhoAmIRequest request = new WhoAmIRequest();
                WhoAmIResponse response = (WhoAmIResponse)svc.Execute(request);



                Console.WriteLine(response.UserId);


                QueryExpression qe = new QueryExpression("account");
                qe.ColumnSet = new ColumnSet("name", "emailaddress1", "statecode");
                qe.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                EntityCollection ec = svc.RetrieveMultiple(qe);
                Console.WriteLine(ec.Entities.Count);


                for (int i = 0; i < ec.Entities.Count; i++)
                {


                    Console.WriteLine(ec[i].Id);
                    var accountname = ec[i].GetAttributeValue<string>("name");
                    if (accountname != null)
                    {
                        Console.WriteLine(accountname);
                    }
                    else { }
                    
                    QueryExpression qe1 = new QueryExpression("contact");
                    qe1.ColumnSet = new ColumnSet("emailaddress1", "statecode", "parentcustomerid");
                    qe1.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, ec[i].Id);
                    EntityCollection ec1 = svc.RetrieveMultiple(qe1);
                    Console.WriteLine(ec1.Entities.Count);
                    if (ec1.Entities.Count != 0)
                    {
                        for (int j = 0; j < ec1.Entities.Count; j++)


                        {
                            Guid contactguid = ec1[j].Id;
                            QueryExpression qe2 = new QueryExpression("connection");
                            qe2.ColumnSet = new ColumnSet("record2id");
                            qe2.Criteria.AddCondition("record2id", ConditionOperator.Equal, contactguid);
                            EntityCollection ec3 = svc.RetrieveMultiple(qe2);
                            Console.WriteLine(ec3.Entities.Count);
                            if (ec3.Entities.Count == 0)

                            {
                                Entity connection = new Entity();
                                connection.LogicalName = "connection";
                                connection.Attributes.Add("record2id", new EntityReference("contact", contactguid));
                                connection.Attributes.Add("record1id", new EntityReference("account", ec[i].Id));
                                // to fill both the connection role lookups, uncomment the below line
                                //connection.Attributes.Add("record2roleid", new EntityReference("connectionrole", new Guid(connRoleId)));
                                // connection.Attributes.Add("record1roleid", new EntityReference("connectionrole", new Guid("connectioRole_guid_here")));
                                svc.Create(connection);
                            }
                        }

                    }

                }
                Console.ReadLine();
            }
        }
    }
}

