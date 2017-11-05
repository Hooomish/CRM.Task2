using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace Task2
{
    public class AccountNumberPlugin : IPlugin
    {
        // CrmServiceClient service = new CrmServiceClient(
        //        "AuthType=AD;Url=https://studdev.scnsoft.com/studdev; Domain=MAIN; Username=crm-test17@scnsoft.com; Password=Abcd1234");

        string entityName = "scnsoft_securitypaper";
        string fieldName = "scnsoft_name";
        

        private string GetNameEntity(Entity ConfigEntity, out int uniqueNumber)
        {
            uniqueNumber = ConfigEntity.GetAttributeValue<int>("scnsoft_uniquenumber");
            string prefix = ConfigEntity.GetAttributeValue<string>("scnsoft_prefix");            
            string postfix = ConfigEntity.GetAttributeValue<string>("scnsoft_postfix");

            return (prefix + "-" + uniqueNumber.ToString() + "-" + postfix);
        }

        private void SetUniqueNumber(Entity ConfigEntity, int uniqueNumber, IOrganizationService service)
        {
            int increment = ConfigEntity.GetAttributeValue<int>("scnsoft_increment");

            uniqueNumber += increment;

            ConfigEntity["scnsoft_uniquenumber"] = uniqueNumber;
            service.Update(ConfigEntity);
        }
       
        private Entity CreateNewEntity(IOrganizationService service)
        {
            var autonumberAZ = new Entity("scnsoft_autonumberaz");

            autonumberAZ.Id = Guid.NewGuid();
            autonumberAZ["scnsoft_prefix"] = "acc";
            autonumberAZ["scnsoft_uniquenumber"] = "001";
            autonumberAZ["scnsoft_increment"] = 1;
            autonumberAZ["scnsoft_postfix"] = "tst";
            autonumberAZ["scnsoft_apply"] = entityName;
            autonumberAZ["scnsoft_applytofield"] = fieldName;

            service.Create(autonumberAZ);

            return autonumberAZ;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
                serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            var query = new QueryExpression("scnsoft_autonumberaz") { ColumnSet = new ColumnSet(true) };
            var ConfigEntity = service.RetrieveMultiple(query).Entities.FirstOrDefault();

            if (ConfigEntity == null)
                ConfigEntity = CreateNewEntity(service);
            
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {

                //// Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                ////</snippetAccountNumberPlugin2>

                //// Verify that the target entity represents an account.
                //// If not, this plug-in was not registered correctly.
                if (entity.LogicalName == entityName)
                {
                    int uniqueNumber;
                    entity[fieldName] = GetNameEntity(ConfigEntity, out uniqueNumber);


                    SetUniqueNumber(ConfigEntity, uniqueNumber, service);
                    
                }
            }
        }
    }
}

