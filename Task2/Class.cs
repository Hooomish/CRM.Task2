using System.Linq;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Activities;
using Microsoft.Xrm.Sdk;

namespace WorkflowZA
{
    public class Class1 : CodeActivity
    {
        string configEntity = "scnsoft_autonumberaz";
        string entityName = "scnsoft_securitypaper";
              

        protected override void Execute(CodeActivityContext executionContext)
        {
            var context = executionContext.GetExtension<IWorkflowContext>();


            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var queryConfig = new QueryExpression(this.configEntity) { ColumnSet = new ColumnSet(true) };
            var queryEntity = new QueryExpression(entityName) { ColumnSet = new ColumnSet(true) };

            //query.Criteria.AddCondition(entityName, "scnsoft_name", ConditionOperator.Contains, "acc");
            //query.Criteria.AddCondition(entityName, "scnsoft_name", ConditionOperator.Contains, "tst");

            var filterAr = new FilterExpression(LogicalOperator.And);
            filterAr.AddCondition(entityName, "scnsoft_name", ConditionOperator.DoesNotContain, "acc");
            filterAr.AddCondition(entityName, "scnsoft_name", ConditionOperator.DoesNotContain, "tst");
            queryEntity.Criteria.AddFilter(filterAr);

            var resultEntity = service.RetrieveMultiple(queryEntity);
            var configEntity = service.RetrieveMultiple(queryConfig).Entities.FirstOrDefault();

            int uniqueNumber = configEntity.GetAttributeValue<int>("scnsoft_uniquenumber");
            string prefix = configEntity.GetAttributeValue<string>("scnsoft_prefix");
            string postfix = configEntity.GetAttributeValue<string>("scnsoft_postfix");
            int increment = configEntity.GetAttributeValue<int>("scnsoft_increment");


            if (resultEntity != null)   
            {
                foreach(var entity in resultEntity.Entities)
                {
                    entity["scnsoft_name"] = prefix + "-" + uniqueNumber + "-" + postfix;
                    uniqueNumber += increment;

                    configEntity["scnsoft_unique"] = uniqueNumber;
                    service.Update(configEntity);
                    service.Update(entity);                    
                }
            }
        }
    }
}
