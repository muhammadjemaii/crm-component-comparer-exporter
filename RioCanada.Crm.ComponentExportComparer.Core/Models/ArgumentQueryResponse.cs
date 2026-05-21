using RioCanada.Crm.ComponentExportComparer.Core.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RioCanada.Crm.ComponentExportComparer.Core.Models
{
    public class ArgumentQueryResponse
    {
        public int TotalComponents
        {
            get
            {
                return this.Entities.Count
                    + this.WebResources.Count
                    + this.PluginSteps.Count
                    + this.OptionSets.Count
                    + this.Dashboards.Count
                    + this.SiteMaps.Count
                    + this.SecurityRoles.Count
                    + this.Workflows.Count
                    + this.BusinessRules.Count
                    + this.Actions.Count
                    + this.BusinessProcessFlows.Count
                    + this.ModelDrivenApps.Count
                    + this.EmailTemplates.Count
                    + this.MailMergeTemplates.Count
                    + this.DuplicateRules.Count
                    + this.ConnectionRoles.Count
                    + this.Reports.Count
                    + this.CanvasApps.Count
                    + this.PluginAssemblies.Count
                    + this.CloudFlows.Count
                    ;
            }
        }
        internal List<CRMEntityTable> Entities { get; } = new List<CRMEntityTable>();
        internal List<WebResource> WebResources { get; } = new List<WebResource>();
        internal List<SdkMessageProcessingStep> PluginSteps { get; } = new List<SdkMessageProcessingStep>();
        internal List<OptionSetMetadataBase> OptionSets { get; } = new List<OptionSetMetadataBase>();
        internal List<SystemForm> Dashboards { get; } = new List<SystemForm>();
        internal List<SiteMap> SiteMaps { get; } = new List<SiteMap>();
        internal List<SecurityRole> SecurityRoles { get; } = new List<SecurityRole>();
        internal List<Workflow> Workflows { get; } = new List<Workflow>();
        internal List<Workflow> BusinessRules { get; } = new List<Workflow>();
        internal List<Workflow> Actions { get; } = new List<Workflow>();
        internal List<Workflow> BusinessProcessFlows { get; } = new List<Workflow>();
        internal List<AppModule> ModelDrivenApps { get; } = new List<AppModule>();
        internal List<EmailTemplate> EmailTemplates { get; } = new List<EmailTemplate>();
        internal List<MailMergeTemplate> MailMergeTemplates { get; } = new List<MailMergeTemplate>();
        internal List<DuplicateRule> DuplicateRules { get; } = new List<DuplicateRule>();
        internal List<ConnectionRole> ConnectionRoles { get; } = new List<ConnectionRole>();
        internal List<Report> Reports { get; } = new List<Report>();
        internal List<CanvasApp> CanvasApps { get; } = new List<CanvasApp>();
        internal List<PluginAssembly> PluginAssemblies { get; } = new List<PluginAssembly>();
        internal List<Workflow> CloudFlows { get; } = new List<Workflow>();

        internal static ArgumentQueryResponse Merge(IEnumerable<ArgumentQueryResponse> componentCollections)
        {
            var result = new ArgumentQueryResponse();

            List<CRMEntityTable> entities = new List<CRMEntityTable>();
            List<WebResource> webResources = new List<WebResource>();
            List<SdkMessageProcessingStep> pluginSteps = new List<SdkMessageProcessingStep>();
            List<OptionSetMetadataBase> optionSets = new List<OptionSetMetadataBase>();
            List<SystemForm> dashboards = new List<SystemForm>();
            List<SiteMap> siteMaps = new List<SiteMap>();
            List<SecurityRole> securityRoles = new List<SecurityRole>();
            List<Workflow> workflows = new List<Workflow>();
            List<Workflow> businessRules = new List<Workflow>();
            List<Workflow> actions = new List<Workflow>();
            List<Workflow> businessProcessFlows = new List<Workflow>();
            List<AppModule> modelDrivenApps = new List<AppModule>();
            List<EmailTemplate> emailTemplates = new List<EmailTemplate>();
            List<MailMergeTemplate> mailMergeTemplates = new List<MailMergeTemplate>();
            List<DuplicateRule> duplicateRules = new List<DuplicateRule>();
            List<ConnectionRole> connectionRoles = new List<ConnectionRole>();
            List<Report> reports = new List<Report>();
            List<CanvasApp> canvasApps = new List<CanvasApp>();
            List<PluginAssembly> pluginAssemblies = new List<PluginAssembly>();
            List<Workflow> cloudFlows = new List<Workflow>();

            foreach (var item in componentCollections)
            {
                entities.AddRange(item.Entities);
                webResources.AddRange(item.WebResources);
                pluginSteps.AddRange(item.PluginSteps);
                optionSets.AddRange(item.OptionSets);
                dashboards.AddRange(item.Dashboards);
                siteMaps.AddRange(item.SiteMaps);
                securityRoles.AddRange(item.SecurityRoles);
                workflows.AddRange(item.Workflows);
                businessRules.AddRange(item.BusinessRules);
                actions.AddRange(item.Actions);
                businessProcessFlows.AddRange(item.BusinessProcessFlows);
                modelDrivenApps.AddRange(item.ModelDrivenApps);
                emailTemplates.AddRange(item.EmailTemplates);
                mailMergeTemplates.AddRange(item.MailMergeTemplates);
                duplicateRules.AddRange(item.DuplicateRules);
                connectionRoles.AddRange(item.ConnectionRoles);
                reports.AddRange(item.Reports);
                canvasApps.AddRange(item.CanvasApps);
                pluginAssemblies.AddRange(item.PluginAssemblies);
                cloudFlows.AddRange(item.CloudFlows);
            }

            result.Entities.AddRange(entities.DistinctBy(x => x.Id));
            result.WebResources.AddRange(webResources.DistinctBy(x => x.Id));
            result.PluginSteps.AddRange(pluginSteps.DistinctBy(x => x.Id));
            result.OptionSets.AddRange(optionSets.DistinctBy(x => x.Name));
            result.Dashboards.AddRange(dashboards.DistinctBy(x => x.Id));
            result.SiteMaps.AddRange(siteMaps.DistinctBy(x => x.Id));
            result.SecurityRoles.AddRange(securityRoles.DistinctBy(x => x.Id));
            result.Workflows.AddRange(workflows.DistinctBy(x => x.Id));
            result.BusinessRules.AddRange(businessRules.DistinctBy(x => x.Id));
            result.Actions.AddRange(actions.DistinctBy(x => x.Id));
            result.BusinessProcessFlows.AddRange(businessProcessFlows.DistinctBy(x => x.Id));
            result.ModelDrivenApps.AddRange(modelDrivenApps.DistinctBy(x => x.Id));
            result.EmailTemplates.AddRange(emailTemplates.DistinctBy(x => x.Id));
            result.MailMergeTemplates.AddRange(mailMergeTemplates.DistinctBy(x => x.Id));
            result.DuplicateRules.AddRange(duplicateRules.DistinctBy(x => x.Id));
            result.ConnectionRoles.AddRange(connectionRoles.DistinctBy(x => x.Id));
            result.Reports.AddRange(reports.DistinctBy(x => x.Id));
            result.CanvasApps.AddRange(canvasApps.DistinctBy(x => x.Id));
            result.PluginAssemblies.AddRange(pluginAssemblies.DistinctBy(x => x.Id));
            result.CloudFlows.AddRange(cloudFlows.DistinctBy(x => x.Id));

            return result;
        }

        internal static ArgumentQueryResponse GetResponse(OrganizationService service, ArgumentQueryRequest argumentQuery, Action<int> onProgress = null, BackgroundWorker bgWorker = null)
        {
            var result = new ArgumentQueryResponse();

            List<Solution> solutions = new List<Solution>();

            int queryCount = Math.Min(1, argumentQuery.Solutions.Count)
                + Math.Min(1, argumentQuery.EntityPatterns.Count)
                + Math.Min(1, argumentQuery.WebResourcePatterns.Count)
                + Math.Min(1, argumentQuery.PluginStepPatterns.Count)
                + Math.Min(1, argumentQuery.OptionSetPatterns.Count)
                + Math.Min(1, argumentQuery.DashboardPatterns.Count)
                + Math.Min(1, argumentQuery.SiteMapPatterns.Count)
                + Math.Min(1, argumentQuery.SecurityRolePatterns.Count)
                + Math.Min(1, argumentQuery.WorkflowPatterns.Count)
                + Math.Min(1, argumentQuery.BusinessRulePatterns.Count)
                + Math.Min(1, argumentQuery.ActionPatterns.Count)
                + Math.Min(1, argumentQuery.BusinessProcessFlowPatterns.Count)
                + Math.Min(1, argumentQuery.ModelDrivenAppPatterns.Count)
                + Math.Min(1, argumentQuery.EmailTemplatePatterns.Count)
                + Math.Min(1, argumentQuery.MailMergeTemplatePatterns.Count)
                + Math.Min(1, argumentQuery.DuplicateRulePatterns.Count)
                + Math.Min(1, argumentQuery.ConnectionRolePatterns.Count)
                + Math.Min(1, argumentQuery.ReportPatterns.Count)
                + Math.Min(1, argumentQuery.CanvasAppPatterns.Count)
                + Math.Min(1, argumentQuery.CloudFlowPatterns.Count)
                ;

            if (queryCount == 0) return result;

            int progressStep = 100 / queryCount;
            int progress = 0;

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.Solutions.Count > 0)
            {
                solutions = Solution.FindSolutions(service, argumentQuery.Solutions);

                if (solutions.Count == 0)
                {
                    onProgress?.Invoke(100);
                    return result;
                }

                onProgress?.Invoke((progress += progressStep));
            }

            var solutionIds = solutions.Select(x => x.Id);

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.EntityPatterns.Count > 0)
            {
                var tables = CRMEntityTable.FindByLogicalName(service, argumentQuery.EntityPatterns, solutionIds);
                result.Entities.AddRange(tables.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.WebResourcePatterns.Count > 0)
            {
                var webresources = WebResource.FindByNames(service, argumentQuery.IncludeSystemWebresource, argumentQuery.WebResourcePatterns, solutionIds);
                result.WebResources.AddRange(webresources.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.PluginStepPatterns.Count > 0)
            {
                var pluginSteps = SdkMessageProcessingStep.FindByNames(service, argumentQuery.IncludeSystemPluginStep, argumentQuery.PluginStepPatterns, solutionIds);
                result.PluginSteps.AddRange(pluginSteps.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.OptionSetPatterns.Count > 0)
            {
                var optionSets = SolutionComponent.FindOptionSetByNames(service, argumentQuery.OptionSetPatterns, solutionIds);
                result.OptionSets.AddRange(optionSets.DistinctBy(x => x.Name));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.DashboardPatterns.Count > 0)
            {
                var dashboards = SystemForm.FindDashboardByNames(service, argumentQuery.DashboardPatterns, solutionIds);
                result.Dashboards.AddRange(dashboards.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.SiteMapPatterns.Count > 0)
            {
                var siteMaps = SiteMap.FindSiteMapByNames(service, argumentQuery.SiteMapPatterns, solutionIds);
                result.SiteMaps.AddRange(siteMaps.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.SecurityRolePatterns.Count > 0)
            {
                var securityRoles = SecurityRole.FindSecurityRoleByNames(service, argumentQuery.SecurityRolePatterns, solutionIds);
                result.SecurityRoles.AddRange(securityRoles.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.WorkflowPatterns.Count > 0)
            {
                var workflows = Workflow.FindWorkflowByNames(service, argumentQuery.WorkflowPatterns, solutionIds);
                result.Workflows.AddRange(workflows.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.BusinessRulePatterns.Count > 0)
            {
                var businessRules = Workflow.FindBusinessRuleByNames(service, argumentQuery.BusinessRulePatterns, solutionIds);
                result.BusinessRules.AddRange(businessRules.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.ActionPatterns.Count > 0)
            {
                var actions = Workflow.FindActionByNames(service, argumentQuery.ActionPatterns, solutionIds);
                result.Actions.AddRange(actions.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.BusinessProcessFlowPatterns.Count > 0)
            {
                var businessProcessFlows = Workflow.FindBusinessProcessFlowByNames(service, argumentQuery.BusinessProcessFlowPatterns, solutionIds);
                result.BusinessProcessFlows.AddRange(businessProcessFlows.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.ModelDrivenAppPatterns.Count > 0)
            {
                var modelDrivenApps = AppModule.FindModelDrivenAppByNames(service, argumentQuery.ModelDrivenAppPatterns, solutionIds);
                result.ModelDrivenApps.AddRange(modelDrivenApps.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.EmailTemplatePatterns.Count > 0)
            {
                var emailTemplates = EmailTemplate.FindByNames(service, argumentQuery.EmailTemplatePatterns, solutionIds);
                result.EmailTemplates.AddRange(emailTemplates.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.MailMergeTemplatePatterns.Count > 0)
            {
                var mailMergeTemplates = MailMergeTemplate.FindByNames(service, argumentQuery.MailMergeTemplatePatterns, solutionIds);
                result.MailMergeTemplates.AddRange(mailMergeTemplates.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.DuplicateRulePatterns.Count > 0)
            {
                var duplicateRules = DuplicateRule.FindByNames(service, argumentQuery.DuplicateRulePatterns, solutionIds);
                result.DuplicateRules.AddRange(duplicateRules.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.ConnectionRolePatterns.Count > 0)
            {
                var connectionRoles = ConnectionRole.FindByNames(service, argumentQuery.ConnectionRolePatterns, solutionIds);
                result.ConnectionRoles.AddRange(connectionRoles.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.ReportPatterns.Count > 0)
            {
                var reports = Report.FindByNames(service, argumentQuery.ReportPatterns, solutionIds);
                result.Reports.AddRange(reports.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.CanvasAppPatterns.Count > 0)
            {
                var canvasApps = CanvasApp.FindByNames(service, argumentQuery.CanvasAppPatterns, solutionIds);
                result.CanvasApps.AddRange(canvasApps.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            if (bgWorker?.CancellationPending == true) return result;

            if (argumentQuery.CloudFlowPatterns.Count > 0)
            {
                var cloudFlows = Workflow.FindModernFlowByNames(service, argumentQuery.CloudFlowPatterns, solutionIds);
                result.CloudFlows.AddRange(cloudFlows.DistinctBy(x => x.Id));
                onProgress?.Invoke((progress += progressStep));
            }

            onProgress?.Invoke(100);

            return result;
        }

        internal static ArgumentQueryResponse GetResponse(OrganizationService service, IEnumerable<ArgumentQueryRequest> argumentQueries, Action<int> onProgress = null, BackgroundWorker bgWorker = null)
        {
            var count = argumentQueries.Count();

            if (count == 0)
            {
                return new ArgumentQueryResponse();
            }

            var results = new List<ArgumentQueryResponse>();
            var completed = 0;
            var stepProgress = 100M / count;
            foreach (var item in argumentQueries)
            {
                if (bgWorker?.CancellationPending == true) return new ArgumentQueryResponse();

                results.Add(GetResponse(service, item, (p) =>
                {
                    onProgress?.Invoke(Convert.ToInt32((completed * stepProgress) + ((stepProgress * p) / 100M)));
                }));
                completed++;
            }

            onProgress?.Invoke(100);

            if (bgWorker?.CancellationPending == true) return new ArgumentQueryResponse();

            return Merge(results);
        }
    }
}
