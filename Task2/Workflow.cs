using System.Linq;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Activities;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Task2
{
    public class Workflow : CodeActivity
    {
        string configEntityName = "scnsoft_autonumberaz";
        string entityName = "scnsoft_securitypaper";
        IEnumerable<Entity> resultEntity;
        Entity configEntity;

        private void SetConfigEntity(IOrganizationService service)
        {
            var queryConfig = new QueryExpression(configEntityName) { ColumnSet = new ColumnSet(true) };
            configEntity = service.RetrieveMultiple(queryConfig).Entities.FirstOrDefault();
        }

        private void SetEntity(IOrganizationService service)
        {
            var queryEntity = new QueryExpression(entityName) { ColumnSet = new ColumnSet("scnsoft_name") };
            AddFilter(queryEntity);
            resultEntity = service.RetrieveMultiple(queryEntity).Entities;
        }

        private void AddFilter(QueryExpression query)
        {
            string prefix = configEntity.GetAttributeValue<string>("scnsoft_prefix");
            string postfix = configEntity.GetAttributeValue<string>("scnsoft_postfix");
            string uniqueNumber = prefix + '-' + '%' + '-' + postfix;

            query.Criteria.AddCondition("scnsoft_name", ConditionOperator.NotLike, uniqueNumber);

            //var filterAr = new FilterExpression(LogicalOperator.And);
            //filterAr.AddCondition(entityName, "scnsoft_name", ConditionOperator.DoesNotContain, "acc");
            //filterAr.AddCondition(entityName, "scnsoft_name", ConditionOperator.DoesNotContain, "tst");
            //query.Criteria.AddFilter(filterAr);

        }

        private string GetAttributeConfig()
        {
            int uniqueNumber = configEntity.GetAttributeValue<int>("scnsoft_uniquenumber");
            string prefix = configEntity.GetAttributeValue<string>("scnsoft_prefix");
            string postfix = configEntity.GetAttributeValue<string>("scnsoft_postfix");

            return prefix + "-" + uniqueNumber + "-" + postfix;
        }

        private void SetIncrement(IOrganizationService service)
        {
            int increment = configEntity.GetAttributeValue<int>("scnsoft_increment");
            int uniqueNumber = configEntity.GetAttributeValue<int>("scnsoft_uniquenumber");

            uniqueNumber += increment;

            configEntity["scnsoft_uniquenumber"] = uniqueNumber;

            service.Update(configEntity);
        }

        protected override void Execute(CodeActivityContext executionContext)
        {
            var context = executionContext.GetExtension<IWorkflowContext>();

            if (context.Depth > 1)
                return;

            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            SetConfigEntity(service);
            SetEntity(service);

            if (resultEntity != null)
            {
                foreach (var entity in resultEntity)
                {
                    entity["scnsoft_name"] = GetAttributeConfig();
                    SetIncrement(service);
                    service.Update(entity);
                }
            }
        }
    }
}
