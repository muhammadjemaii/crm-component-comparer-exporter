using RioCanada.Crm.ComponentExportComparer.Core.Extensions;
using RioCanada.Crm.ComponentExportComparer.Core.Models;
using RioCanada.Crm.ComponentExportComparer.Core.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RioCanada.Crm.ComponentExportComparer.Core
{
    class Exporter
    {
        readonly ArgumentQueryResponse ArgumentQueryResponse;
        readonly List<string> ArgumentQueries;
        readonly Action<ExportProgressSnapshot> OnProgress;
        readonly ExportService.OutFileHandler OutFileFunc;
        readonly BackgroundWorker BgWorker;
        readonly ExportSetting Setting;
        readonly OrganizationService Service;
        readonly Logger Logger;
        readonly bool GenerateIndexFile;
        readonly List<IndexLineItem> IndexData = new List<IndexLineItem>();

        readonly int EntityProgressWeight = 2;
        readonly int WebResourceWeight = 1;
        readonly int PluginWeight = 1;
        readonly int OptionSetWeight = 1;
        readonly int DashboardWeight = 1;
        readonly int SiteMapWeight = 1;
        readonly int SecurityRoleWeight = 2;
        readonly int WorkflowWeight = 1;
        readonly int BusinessRuleWeight = 1;
        readonly int ActionWeight = 1;
        readonly int BusinessProcessFlowWeight = 1;
        readonly int ModelDrivenAppWeight = 1;
        readonly int EmailTemplateWeight = 1;
        readonly int MailMergeTemplateWeight = 1;
        readonly int DuplicateRuleWeight = 1;
        readonly int ConnectionRoleWeight = 1;
        readonly int ReportWeight = 1;
        readonly int CanvasAppWeight = 1;
        readonly int CloudFlowWeight = 1;

        //readonly int ENTITY_BUFFER_SIZE = 5;
        readonly int WEBRESOURCE_BUFFER_SIZE = 50;
        //readonly int OPTIONSET_BUFFER_SIZE = 5;
        readonly int DASHBOARD_BUFFER_SIZE = 10;
        readonly int SITEMAP_BUFFER_SIZE = 10;
        readonly int SECURITY_ROLE_BUFFER_SIZE = 10;
        readonly int WORKFLOW_BUFFER_SIZE = 50;
        readonly int MODEL_DRIVEN_APP_BUFFER_SIZE = 10;
        readonly int EMAIL_TEMPLATE_BUFFER_SIZE = 50;
        readonly int MAIL_MERGE_TEMPLATE_BUFFER_SIZE = 50;
        readonly int DUPLICATE_RULE_BUFFER_SIZE = 50;
        readonly int CONNECTION_ROLE_BUFFER_SIZE = 50;
        readonly int REPORT_BUFFER_SIZE = 10;
        readonly int CANVAS_APP_BUFFER_SIZE = 10;
        readonly int CLOUD_FLOW_BUFFER_SIZE = 50;

        bool IncludeAllProperty { get => this.Setting.IncludeAllProperty; }
        bool ReplaceEmptyStringByNull  { get => this.Setting.ReplaceEmptyStringByNull; }

        int OverallCompleted = 0;
        readonly int OverallTotal = 0;

        int CurrentCompleted = 0;
        int CurrentTotal = 0;
        string CurrentLabel = string.Empty;

        public Exporter(
            OrganizationService service,
            List<string> argumentQueries,
            ArgumentQueryResponse argumentQueryResponse,
            ExportSetting setting,
            Logger logger,
            ExportService.OutFileHandler outFileFunc,
            Action<ExportProgressSnapshot> onProgress,
            BackgroundWorker bgWorker = null,
            bool generateIndexFile = false
        )
        {
            this.ArgumentQueries = argumentQueries;
            this.ArgumentQueryResponse = argumentQueryResponse;
            this.OnProgress = onProgress;
            this.OutFileFunc = outFileFunc;
            this.BgWorker = bgWorker;
            this.Setting = setting;
            this.Service = service;
            this.Logger = logger;
            this.GenerateIndexFile = generateIndexFile;

            if (setting.IncludeEntityColumn) EntityProgressWeight++;
            if (setting.IncludeEntityRelationship) EntityProgressWeight++;
            if (setting.IncludeEntityForm || setting.IncludeEntityDashboard) EntityProgressWeight++;
            if (setting.IncludeEntityView) EntityProgressWeight++;
            if (setting.IncludeEntityRibbon) EntityProgressWeight += 3;

            OverallTotal = argumentQueryResponse.Entities.Count * EntityProgressWeight
                + argumentQueryResponse.WebResources.Count * WebResourceWeight
                + argumentQueryResponse.PluginSteps.Count * PluginWeight
                + argumentQueryResponse.OptionSets.Count * OptionSetWeight
                + argumentQueryResponse.Dashboards.Count * DashboardWeight
                + argumentQueryResponse.SiteMaps.Count * SiteMapWeight
                + argumentQueryResponse.SecurityRoles.Count * SecurityRoleWeight
                + argumentQueryResponse.Workflows.Count * WorkflowWeight
                + argumentQueryResponse.BusinessRules.Count * BusinessRuleWeight
                + argumentQueryResponse.Actions.Count * ActionWeight
                + argumentQueryResponse.BusinessProcessFlows.Count * BusinessProcessFlowWeight
                + argumentQueryResponse.ModelDrivenApps.Count * ModelDrivenAppWeight
                + argumentQueryResponse.EmailTemplates.Count * EmailTemplateWeight
                + argumentQueryResponse.MailMergeTemplates.Count * MailMergeTemplateWeight
                + argumentQueryResponse.DuplicateRules.Count * DuplicateRuleWeight
                + argumentQueryResponse.ConnectionRoles.Count * ConnectionRoleWeight
                + argumentQueryResponse.Reports.Count * ReportWeight
                + argumentQueryResponse.CanvasApps.Count * CanvasAppWeight
                ;
        }

        public void Execute()
        {
            this.ValidateData();

            this.ExportEntity(ArgumentQueryResponse.Entities, EntityProgressWeight); // Entities
            this.ExportWebresource(ArgumentQueryResponse.WebResources, WebResourceWeight); // Webresources
            this.ExportPlugin(ArgumentQueryResponse.PluginSteps, PluginWeight); // Plugins
            this.ExportOptionSet(ArgumentQueryResponse.OptionSets, OptionSetWeight); // OptionSets
            this.ExportDashboard(ArgumentQueryResponse.Dashboards, DashboardWeight); // Dashboards
            this.ExportSitemap(ArgumentQueryResponse.SiteMaps, SiteMapWeight); // SiteMaps
            this.ExportSecurityrole(ArgumentQueryResponse.SecurityRoles, SecurityRoleWeight); // SecurityRoles
            this.ExportWorkflow(ArgumentQueryResponse.Workflows, WorkflowWeight); // Workflows
            this.ExportBusinessrule(ArgumentQueryResponse.BusinessRules, BusinessRuleWeight); // BusinessRules
            this.ExportAction(ArgumentQueryResponse.Actions, ActionWeight); // Actions
            this.ExportBusinessProcessFlow(ArgumentQueryResponse.BusinessProcessFlows, BusinessProcessFlowWeight); // Business Process Flows
            this.ExportModelDrivenApp(ArgumentQueryResponse.ModelDrivenApps, ModelDrivenAppWeight); // Model Driven Apps
            this.ExportEmailTemplate(ArgumentQueryResponse.EmailTemplates, EmailTemplateWeight); // Email Templates
            this.ExportMailMergeTemplate(ArgumentQueryResponse.MailMergeTemplates, MailMergeTemplateWeight); // Mail Merge Templates
            this.ExportDuplicateRule(ArgumentQueryResponse.DuplicateRules, DuplicateRuleWeight); // Duplicate Rules
            this.ExportConnectionRole(ArgumentQueryResponse.ConnectionRoles, ConnectionRoleWeight); // Connection Roles
            this.ExportReport(ArgumentQueryResponse.Reports, ReportWeight); // Reports
            this.ExportCanvasApp(ArgumentQueryResponse.CanvasApps, CanvasAppWeight); // Canvas Apps
            this.ExportCloudFlow(ArgumentQueryResponse.CloudFlows, CloudFlowWeight); // Cloud Flows

            if (BgWorker?.CancellationPending == true) return;

            if (this.GenerateIndexFile)
            {
                IndexData.ForEach(x => CalculateIndexChecksum(x));
                var rootIndexItem = new IndexLineItem
                {
                    Children = IndexData,
                };
                rootIndexItem.Metadata.Add("EnvironmentId", this.Service.EnvironmentId);
                rootIndexItem.Metadata.Add("Server", this.Service.Server);
                rootIndexItem.Metadata.Add("Queries", this.ArgumentQueries);
                rootIndexItem.Metadata.Add("Settings", this.Setting);

                OutFileFunc("index.json", SerializeUtility.SerializeJson(rootIndexItem), null);
            }

            OnProgress(new ExportProgressSnapshot
            {
                CurrentLabel = "Exporting completed.",
                CurrentProgress = 100,
                OverallProgress = 100,
                CurrentCompleted = 0,
                CurrentTotal = 0,
            });
        }

        private static int GetProgressValue(int min, int max, int value)
        {
            return (((max - min) * Math.Min(100, Math.Max(0, value))) / 100) + min;
        }

        private static int GetProgressValue(int min, int max, int total, int completed)
        {
            if (total == 0) return 0;
            return GetProgressValue(min, max, (completed * 100) / total);
        }

        void ValidateData()
        {
            if (!this.Setting.RoleById)
            {
                var duplicateItem = ArgumentQueryResponse.SecurityRoles.GroupBy(x => x.Name.ToLower()).Where(x => x.Count() > 1).FirstOrDefault();

                if (duplicateItem != null)
                {
                    throw new Exception($"Role \"{duplicateItem.FirstOrDefault().Name}\" found as duplicate. Enable \"Compare role by id instead name\" in Query Option or remove duplicate role.");
                }
            }
        }

        void ExportEntity(List<CRMEntityTable> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting tables...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "tables", Name = "Tables", Type = IndexItemType.Folder, Order = 1, Children = new List<IndexLineItem>() });

            var entityFilter = EntityFilters.Entity;
            if (Setting.IncludeEntityColumn) entityFilter |= EntityFilters.Attributes;
            if (Setting.IncludeEntityRelationship) entityFilter |= EntityFilters.Relationships;

            SendProgressSnapshot();
            foreach (var item in items)
            {
                if (BgWorker?.CancellationPending == true) return;
                var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = item.AttributeLogicalName, Name = item.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });
                currentIndexItem.Metadata.Add("Type", "Table");
                currentIndexItem.Metadata.Add("SchemaName", item.AttributeLogicalName);

                RetrieveEntityRequest request = new RetrieveEntityRequest
                {
                    LogicalName = item.AttributeLogicalName,
                    EntityFilters = entityFilter,
                };
                var response = (RetrieveEntityResponse)Service.Execute(request);

                HandleOutFile(
                    $@"tables\{item.AttributeLogicalName}\metadata.json",
                    SerializeUtility.SerializeJson(response.EntityMetadata.GetExportableObject(IncludeAllProperty, ReplaceEmptyStringByNull)),
                    null,
                    currentIndexItem.Children,
                    new IndexLineItem { Key = "metadata.json", Name = "Metadata", Type = IndexItemType.FileJson }
                );
                
                if (Setting.IncludeEntityColumn)
                {
                    var metadata = new Dictionary<string, object>
                    {
                        { "Type", IndexLineItemContentType.Columns },
                        { "Id", item.AttributeLogicalName }
                    };

                    HandleOutFile(
                        $@"tables\{item.AttributeLogicalName}\columns.json",
                        SerializeUtility.SerializeJson(response.EntityMetadata.Attributes.ToList().OrderBy(x => x.SchemaName).Select(x => x.GetMetadataObject(IncludeAllProperty))),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "columns.json", Name = "Columns", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.Columns, Metadata = metadata }
                    );
                }

                if (Setting.IncludeEntityRelationship)
                {
                    var relationshipIndexItem = AddIndexItem(currentIndexItem.Children, new IndexLineItem { Key = "relationships", Name = "Relationships", Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    var metadata = new Dictionary<string, object>
                    {
                        { "Type", IndexLineItemContentType.ManyToMany },
                        { "Id", item.AttributeLogicalName }
                    };

                    HandleOutFile(
                        $@"tables\{item.AttributeLogicalName}\relationships\manytomanyrelationships.json",
                        SerializeUtility.SerializeJson(response.EntityMetadata.ManyToManyRelationships.OrderBy(x => x.SchemaName).Select(x => x.GetMetadataObject(IncludeAllProperty))),
                        null,
                        relationshipIndexItem.Children,
                        new IndexLineItem { Key = "manytomanyrelationships.json", Name = "Many To Many", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.ManyToMany, Metadata = metadata }
                        );

                    metadata = new Dictionary<string, object>
                    {
                        { "Type", IndexLineItemContentType.ManyToOne },
                        { "Id", item.AttributeLogicalName }
                    };

                    HandleOutFile(
                        $@"tables\{item.AttributeLogicalName}\relationships\manytoonerelationships.json",
                        SerializeUtility.SerializeJson(response.EntityMetadata.ManyToOneRelationships.OrderBy(x => x.SchemaName).Select(x => x.GetMetadataObject(IncludeAllProperty))),
                        null,
                        relationshipIndexItem.Children,
                        new IndexLineItem { Key = "manytoonerelationships.json", Name = "Many To One", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.ManyToOne, Metadata = metadata }
                    );

                    metadata = new Dictionary<string, object>
                    {
                        { "Type", IndexLineItemContentType.OneToMany },
                        { "Id", item.AttributeLogicalName }
                    };

                    HandleOutFile(
                        $@"tables\{item.AttributeLogicalName}\relationships\onetomanyrelationships.json",
                        SerializeUtility.SerializeJson(response.EntityMetadata.OneToManyRelationships.OrderBy(x => x.SchemaName).Select(x => x.GetMetadataObject(IncludeAllProperty))), null,
                        relationshipIndexItem.Children,
                        new IndexLineItem { Key = "onetomanyrelationships.json", Name = "One To Many", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.OneToMany, Metadata = metadata }
                    );
                }

                // TODO:
                //EntitySetting[] Settings
                //EntityKeyMetadata[] Keys
                //SecurityPrivilegeMetadata[]

                if (BgWorker?.CancellationPending == true) return;
                if (Setting.IncludeEntityForm || Setting.IncludeEntityDashboard)
                {
                    var systemforms = SystemForm.RetriveMultipleByObjectTypeCode(Service, response.EntityMetadata.ObjectTypeCode ?? 0);

                    if (Setting.IncludeEntityForm)
                    {
                        List<int> formTypes = new List<int> { (int)FormType.Main, (int)FormType.QuickCreate, (int)FormType.QuickViewForm, (int)FormType.Card };

                        var forms = systemforms.FindAll(x => x.Type != null && formTypes.IndexOf(x.Type.Value) != -1);

                        if (forms.Count > 0)
                        {
                            var formsIndexItem = AddIndexItem(currentIndexItem.Children, new IndexLineItem { Key = "forms", Name = "Forms", Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                            forms.ForEach(x =>
                            {
                                var formIndexItem = AddIndexItem(formsIndexItem.Children, new IndexLineItem { Key = x.Id.ToString(), Name = x.Name + (x.Type == null ? string.Empty : $" ({((FormType)x.Type.Value).GetDescription()})"), Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });
                                formIndexItem.Metadata.Add("Type", "Form");
                                formIndexItem.Metadata.Add("Id", x.Id.ToString());

                                HandleOutFile(
                                    $@"tables\{item.AttributeLogicalName}\forms\{x.Id}\metadata.json",
                                    SerializeUtility.SerializeJson(x.GetMetadataObject(IncludeAllProperty)),
                                    null,
                                    formIndexItem.Children,
                                    new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.Columns }
                                );

                                OutFileFunc($@"tables\{item.AttributeLogicalName}\forms\{x.Id}\form.json", SerializeUtility.FormatJson(x.FormJson), null);
                                //HandleOutFile(
                                //    $@"tables\{item.AttributeLogicalName}\forms\{x.Id}\form.json",
                                //    SerializeUtility.FormatJson(x.FormJson),
                                //    null,
                                //    formIndexItem.Children,
                                //    new IndexLineItem { Key = "form.json", Name = "form.json", Type = IndexItemType.FileJson }
                                //);

                                OutFileFunc($@"tables\{item.AttributeLogicalName}\forms\{x.Id}\form_old.xml", x.FormXml, null);

                                HandleOutFile(
                                    $@"tables\{item.AttributeLogicalName}\forms\{x.Id}\form.xml",
                                    DataTransformer.TransformFormXml(x.FormXml, Setting.VerifyTransformedData, this.Service, this.Logger),
                                    null,
                                    formIndexItem.Children,
                                    new IndexLineItem { Key = "form.xml", Name = "form.xml", Type = IndexItemType.FileXml }
                               );
                            });
                        }
                    }

                    if (Setting.IncludeEntityDashboard)
                    {
                        List<int> formTypes = new List<int> { (int)FormType.Dashboard, (int)FormType.InteractionCentricDashboard, (int)FormType.PowerBIDashboard };

                        var forms = systemforms.FindAll(x => x.Type != null && formTypes.IndexOf(x.Type.Value) != -1);

                        if (forms.Count > 0)
                        {
                            var formsIndexItem = AddIndexItem(currentIndexItem.Children, new IndexLineItem { Key = "dashboards", Name = "Dashboards", Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                            forms.ForEach(x =>
                            {
                                var formIndexItem = AddIndexItem(formsIndexItem.Children, new IndexLineItem { Key = x.Id.ToString(), Name = x.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                                HandleOutFile(
                                    $@"tables\{item.AttributeLogicalName}\dashboards\{x.Id}\metadata.json",
                                    SerializeUtility.SerializeJson(x.GetMetadataObject(IncludeAllProperty)),
                                    null,
                                    formIndexItem.Children,
                                    new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                                );
                                HandleOutFile(
                                    $@"tables\{item.AttributeLogicalName}\dashboards\{x.Id}\form.json",
                                    SerializeUtility.FormatJson(x.FormJson),
                                    null,
                                    formIndexItem.Children,
                                    new IndexLineItem { Key = "form.json", Name = "form.json", Type = IndexItemType.FileJson }
                                );
                                HandleOutFile(
                                    $@"tables\{item.AttributeLogicalName}\dashboards\{x.Id}\form.xml",
                                    SerializeUtility.FormatFormXml(x.FormXml, IncludeAllProperty),
                                    null,
                                    formIndexItem.Children,
                                    new IndexLineItem { Key = "form.xml", Name = "form.xml", Type = IndexItemType.FileXml }
                               );
                            });
                        }
                    }
                }

                if (BgWorker?.CancellationPending == true) return;
                if (Setting.IncludeEntityView)
                {
                    var views = SavedQuery.RetriveMultipleByObjectTypeCode(Service, response.EntityMetadata.ObjectTypeCode ?? 0);

                    if (views.Count > 0)
                    {
                        var viewsIndexItem = AddIndexItem(currentIndexItem.Children, new IndexLineItem { Key = "views", Name = "Views", Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                        views.ForEach(x =>
                        {
                            var viewIndexItem = AddIndexItem(viewsIndexItem.Children, new IndexLineItem { Key = x.Id.ToString(), Name = x.Name, Children = new List<IndexLineItem>() });

                            HandleOutFile(
                                $@"tables\{item.AttributeLogicalName}\views\{x.Id}\metadata.json",
                                SerializeUtility.SerializeJson(x.GetMetadataObject(IncludeAllProperty)),
                                null,
                                viewIndexItem.Children,
                                new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                            );
                            HandleOutFile(
                                $@"tables\{item.AttributeLogicalName}\views\{x.Id}\fetch.xml",
                                SerializeUtility.FormatXml(x.FetchXml),
                                null,
                                viewIndexItem.Children,
                                new IndexLineItem { Key = "fetch.xml", Name = "fetch.xml", Type = IndexItemType.FileXml }
                            );
                            HandleOutFile(
                                $@"tables\{item.AttributeLogicalName}\views\{x.Id}\layout.json",
                                SerializeUtility.FormatViewLayoutJson(x.LayoutJson, IncludeAllProperty),
                                null,
                                viewIndexItem.Children,
                                new IndexLineItem { Key = "layout.json", Name = "layout.json", Type = IndexItemType.FileJson }
                            );
                            HandleOutFile(
                                $@"tables\{item.AttributeLogicalName}\views\{x.Id}\layout.xml",
                                SerializeUtility.FormatViewLayoutXml(x.LayoutXml, IncludeAllProperty),
                                null,
                                viewIndexItem.Children,
                                new IndexLineItem { Key = "layout.xml", Name = "layout.xml", Type = IndexItemType.FileXml }
                            );
                        });
                    }
                }

                if (BgWorker?.CancellationPending == true) return;
                if (Setting.IncludeEntityRibbon)
                {
                    RetrieveEntityRibbonRequest ribbonRequest = new RetrieveEntityRibbonRequest
                    {
                        EntityName = item.AttributeLogicalName,
                        RibbonLocationFilter = RibbonLocationFilters.All,
                    };

                    var ribbonResponse = (RetrieveEntityRibbonResponse)Service.Execute(ribbonRequest);

                    var ribbonXml = UnzipRibbon(ribbonResponse.CompressedEntityXml);

                    var transformedRibbonxml = DataTransformer.TransformRibbonXml(ribbonXml, Setting.VerifyTransformedData, this.Logger);

                    OutFileFunc($@"tables\{item.AttributeLogicalName}\ribbon_old.xml", null, ribbonXml);

                    HandleOutFile(
                        $@"tables\{item.AttributeLogicalName}\ribbon.xml",
                        transformedRibbonxml,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "ribbon.xml", Name = "ribbon.xml", Type = IndexItemType.FileXml, ContentType = IndexLineItemContentType.Ribbon }
                    );
                }

                CurrentCompleted++;
                OverallCompleted += weight;
                SendProgressSnapshot();
            }
        }

        void ExportWebresource(List<WebResource> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting webresources...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "webresources", Name = "Webresources", Type = IndexItemType.Folder, Order = 2, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = WEBRESOURCE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var webresources = this.Service.GetData<WebResource>(WebResource.EntityLogicalName, ids);

                foreach (var record in webresources)
                {
                    var metadata = new Dictionary<string, object>
                    {
                        { "Type", "WebResource" },
                        { "Id", record.Id.ToString() }
                    };

                    HandleOutFile(
                        $@"webresources\{record.Name}.metadata.json",
                        SerializeUtility.SerializeJson(record.GetMetadataObject(IncludeAllProperty)),
                        null,
                        indexItem.Children,
                        new IndexLineItem { Key = $"{record.Name}.metadata.json", Name = $"{record.Name}.metadata.json", Type = IndexItemType.FileJson, Metadata = metadata }
                    );
                    HandleOutFile(
                        $@"webresources\{record.Name}",
                        null,
                        Convert.FromBase64String(record.Content),
                        indexItem.Children,
                        new IndexLineItem { Key = record.Name, Name = record.Name, Type = IndexItemType.File, Metadata = metadata }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportPlugin(List<SdkMessageProcessingStep> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting pluginsteps...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "plugings", Name = "Plugins", Type = IndexItemType.Folder, Order = 3, Children = new List<IndexLineItem>() });

            var pluginStepImages = this.Service.GetData<SdkMessageProcessingStepImage, Guid>(
                SdkMessageProcessingStepImage.EntityLogicalName,
                "sdkmessageprocessingstepid",
                items.Select(x => x.Id).ToList()
            );

            SendProgressSnapshot();
            foreach (var item in items)
            {
                if (BgWorker?.CancellationPending == true) return;

                var images = pluginStepImages.FindAll(x => x.SdkMessageProcessingStepId?.Id == item.Id);
                HandleOutFile(
                    $@"plugings\{item.Id}.metadata.json",
                    SerializeUtility.SerializeJson(item.GetMetadataObject(IncludeAllProperty, images, this.Service)),
                    null,
                    indexItem.Children,
                    new IndexLineItem { Key = $"{item.Id}.metadata.json", Name = item.Name, Type = IndexItemType.FileJson }
                );

                CurrentCompleted++;
                OverallCompleted += weight;

                SendProgressSnapshot();
            }
        }

        void ExportOptionSet(List<OptionSetMetadataBase> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting choices...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "choices", Name = "Choices", Type = IndexItemType.Folder, Order = 4, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            foreach (var item in items)
            {
                if (BgWorker?.CancellationPending == true) return;
                HandleOutFile(
                    $@"choices\{item.Name}.metadata.json",
                    SerializeUtility.SerializeJson(item.GetMetadataObject(IncludeAllProperty)),
                    null,
                    indexItem.Children,
                    new IndexLineItem { Key = $"{item.Name}.metadata.json", Name = item.Name, Type = IndexItemType.FileJson }
                );

                CurrentCompleted++;
                OverallCompleted += weight;

                SendProgressSnapshot();
            }
        }

        void ExportDashboard(List<SystemForm> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting dashboards...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "dashboards", Name = "Dashboards", Type = IndexItemType.Folder, Order = 5, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = DASHBOARD_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var forms = this.Service.GetData<SystemForm, Guid>(SystemForm.EntityLogicalName, "formid", ids);

                foreach (var form in forms)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = form.Id.ToString(), Name = form.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"dashboards\{form.Id}\metadata.json",
                        SerializeUtility.SerializeJson(form.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"dashboards\{form.Id}\form.json",
                        SerializeUtility.FormatJson(form.FormJson),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "form.json", Name = "form.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"dashboards\{form.Id}\form.xml",
                        SerializeUtility.FormatXml(form.FormXml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "form.xml", Name = "form.xml", Type = IndexItemType.FileXml }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportSitemap(List<SiteMap> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting sitemaps...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "sitemaps", Name = "Sitemaps", Type = IndexItemType.Folder, Order = 6, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = SITEMAP_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var sitemaps = this.Service.GetData<SiteMap>(SiteMap.EntityLogicalName, ids);

                foreach (var sitemap in sitemaps)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = sitemap.UniqueKey, Name = sitemap.SiteMapName ?? sitemap.SiteMapNameUnique ?? sitemap.Id.ToString(), Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"sitemaps\{sitemap.UniqueKey}\metadata.json",
                        SerializeUtility.SerializeJson(sitemap.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );

                    var transformedSitemapXml = DataTransformer.TransformSiteMapXml(sitemap.SiteMapXML, Setting.VerifyTransformedData, this.Logger);

                    OutFileFunc($@"sitemaps\{sitemap.UniqueKey}\sitemap_old.xml", sitemap.SiteMapXML, null);

                    HandleOutFile(
                        $@"sitemaps\{sitemap.UniqueKey}\sitemap.xml",
                        transformedSitemapXml,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "sitemap.xml", Name = "sitemap.xml", Type = IndexItemType.FileXml }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportSecurityrole(List<SecurityRole> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting securityroles...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "securityroles", Name = "Security Roles", Type = IndexItemType.Folder, Order = 7, Children = new List<IndexLineItem>(), ContentType = IndexLineItemContentType.SecurityRoleFolder });

            SendProgressSnapshot();
            int bufferSize = SECURITY_ROLE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var roles = this.Service.GetData<SecurityRole>(SecurityRole.EntityLogicalName, ids);
                var filteredSecurityRolePrivileges = Privilege.GetByRoleIds(Service, ids);

                foreach (var securityRole in roles)
                {
                    var key = this.Setting.RoleById ? securityRole.Id.ToString() : securityRole.Name;

                    var securityRolePrivileges = filteredSecurityRolePrivileges.FindAll(x => (Guid)x.Get<Microsoft.Xrm.Sdk.AliasedValue>("roleprivileges.roleid").Value == securityRole.Id);

                    var metadata = new Dictionary<string, object>
                    {
                        { "Type", "SecurityRole" },
                        { "Id", securityRole.Id.ToString() }
                    };

                    HandleOutFile(
                        $@"securityroles\{key}.json",
                        SerializeUtility.SerializeJson(new Models.Transform.Metadata.SecurityRoleMergedData
                        {
                            Metadata = securityRole.GetMetadataObject(IncludeAllProperty, this.Setting.RoleById),
                            Privilege = securityRolePrivileges.OrderBy(x => x.Name).Select(x => x.GetMetadataObject(IncludeAllProperty)).ToList(),
                        }),
                        null,
                        indexItem.Children,
                        new IndexLineItem { Key = $"{key}.json", Name = securityRole.Name, Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.SecurityRole, Metadata = metadata }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportWorkflow(List<Workflow> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting workflows...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "workflows", Name = "Workflows", Type = IndexItemType.Folder, Order = 8, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = WORKFLOW_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var workflows = this.Service.GetData<Workflow>(Workflow.EntityLogicalName, ids);

                foreach (var workflow in workflows)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = workflow.UniqueKey, Name = workflow.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"workflows\{workflow.UniqueKey}\metadata.json",
                        SerializeUtility.SerializeJson(workflow.GetMetadataObjectByCategory(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"workflows\{workflow.UniqueKey}\xaml.xml",
                        SerializeUtility.FormatXml(workflow.Xaml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "xaml.xml", Name = "xaml.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"workflows\{workflow.UniqueKey}\clientdata.txt",
                        workflow.ClientData,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "clientdata.txt", Name = "clientdata.txt", Type = IndexItemType.FileTxt }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportBusinessrule(List<Workflow> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting businessrules...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "businessrules", Name = "Business Rules", Type = IndexItemType.Folder, Order = 9, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = WORKFLOW_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var workflows = this.Service.GetData<Workflow>(Workflow.EntityLogicalName, ids);

                foreach (var workflow in workflows)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = workflow.UniqueKey, Name = workflow.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"businessrules\{workflow.UniqueKey}\metadata.json",
                        SerializeUtility.SerializeJson(workflow.GetMetadataObjectByCategory(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"businessrules\{workflow.UniqueKey}\xaml.xml",
                        SerializeUtility.FormatXml(workflow.Xaml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "xaml.xml", Name = "xaml.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"businessrules\{workflow.UniqueKey}\clientdata.txt", workflow.ClientData,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "clientdata.txt", Name = "clientdata.txt", Type = IndexItemType.FileTxt }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportAction(List<Workflow> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting actions...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "actions", Name = "Actions", Type = IndexItemType.Folder, Order = 10, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = WORKFLOW_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var workflows = this.Service.GetData<Workflow>(Workflow.EntityLogicalName, ids);

                foreach (var workflow in workflows)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = workflow.UniqueKey, Name = workflow.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"actions\{workflow.UniqueKey}\metadata.json",
                        SerializeUtility.SerializeJson(workflow.GetMetadataObjectByCategory(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"actions\{workflow.UniqueKey}\xaml.xml",
                        SerializeUtility.FormatXml(workflow.Xaml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "xaml.xml", Name = "xaml.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"actions\{workflow.UniqueKey}\clientdata.txt", workflow.ClientData,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "clientdata.txt", Name = "clientdata.txt", Type = IndexItemType.FileTxt }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportBusinessProcessFlow(List<Workflow> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting businessprocessflows...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "businessprocessflows", Name = "Business Process Flows", Type = IndexItemType.Folder, Order = 11, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = WORKFLOW_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var workflows = this.Service.GetData<Workflow>(Workflow.EntityLogicalName, ids);

                foreach (var workflow in workflows)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = workflow.UniqueKey, Name = workflow.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"businessprocessflows\{workflow.UniqueKey}\metadata.json",
                        SerializeUtility.SerializeJson(workflow.GetMetadataObjectByCategory(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"businessprocessflows\{workflow.UniqueKey}\xaml.xml",
                        SerializeUtility.FormatXml(workflow.Xaml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "xaml.xml", Name = "xaml.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"businessprocessflows\{workflow.UniqueKey}\clientdata.txt", workflow.ClientData,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "clientdata.txt", Name = "clientdata.txt", Type = IndexItemType.FileTxt }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportModelDrivenApp(List<AppModule> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting modeldrivenapps...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "modeldrivenapps", Name = "Model Driven Apps", Type = IndexItemType.Folder, Order = 12, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = MODEL_DRIVEN_APP_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var appModules = this.Service.GetData<AppModule>(AppModule.EntityLogicalName, ids);

                foreach (var appModule in appModules)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = appModule.UniqueName, Name = appModule.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"modeldrivenapps\{appModule.UniqueName}\metadata.json",
                        SerializeUtility.SerializeJson(appModule.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"modeldrivenapps\{appModule.UniqueName}\appmodulemanaged.xml",
                        SerializeUtility.FormatXml(appModule.AppModuleXmlManaged),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "appmodulemanaged.xml", Name = "appmodulemanaged.xml", Type = IndexItemType.FileXml }
                    );

                    var transformedConfigXml = DataTransformer.TransformModelDrivenAppConfigXml(appModule.ConfigXml, Setting.VerifyTransformedData, this.Logger);

                    OutFileFunc($@"modeldrivenapps\{appModule.UniqueName}\config_old.xml", appModule.ConfigXml, null);

                    HandleOutFile(
                        $@"modeldrivenapps\{appModule.UniqueName}\config.xml",
                        transformedConfigXml,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "config.xml", Name = "config.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"modeldrivenapps\{appModule.UniqueName}\eventhandlers.json",
                        SerializeUtility.FormatJson(appModule.EventHandlers),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "eventhandlers.json", Name = "eventhandlers.json", Type = IndexItemType.FileXml }
                    );

                    var transformedDescriptorJson = DataTransformer.TransformModelDrivenAppDescriptor(appModule.Descriptor, Setting.VerifyTransformedData, this.Service, this.Logger);

                    OutFileFunc($@"modeldrivenapps\{appModule.UniqueName}\descriptor_old.json", SerializeUtility.FormatJson(appModule.Descriptor), null);

                    HandleOutFile(
                        $@"modeldrivenapps\{appModule.UniqueName}\descriptor.json",
                        transformedDescriptorJson,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "descriptor.json", Name = "descriptor.json", Type = IndexItemType.FileJson }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportEmailTemplate(List<EmailTemplate> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting email templates...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "emailtemplates", Name = "Email Templates", Type = IndexItemType.Folder, Order = 13, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = EMAIL_TEMPLATE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var templates = this.Service.GetData<EmailTemplate>(EmailTemplate.EntityLogicalName, ids);

                foreach (var template in templates)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = template.Id.ToString(), Name = template.Title, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });
                    currentIndexItem.Metadata.Add("Type", "EmailTemplate");
                    currentIndexItem.Metadata.Add("Id", template.Id.ToString());

                    HandleOutFile(
                        $@"emailtemplates\{template.Id}\metadata.json",
                        SerializeUtility.SerializeJson(template.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"emailtemplates\{template.Id}\body.xml",
                        DataTransformer.TransformXml(template.Body),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "body.xml", Name = "body.xml", Type = IndexItemType.FileXml }
                    );
                    HandleOutFile(
                        $@"emailtemplates\{template.Id}\presentationxml.xml",
                        DataTransformer.TransformXml(template.PresentationXml),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "presentationxml.xml", Name = "presentationxml.xml", Type = IndexItemType.FileXml }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportMailMergeTemplate(List<MailMergeTemplate> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting mail merge templates...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "mailmergetemplates", Name = "Mail Merge Templates", Type = IndexItemType.Folder, Order = 14, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = MAIL_MERGE_TEMPLATE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var templates = this.Service.GetData<MailMergeTemplate>(MailMergeTemplate.EntityLogicalName, ids);

                foreach (var template in templates)
                {
                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = template.Id.ToString(), Name = template.Name, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });
                    currentIndexItem.Metadata.Add("Type", "MailMergeTemplate");
                    currentIndexItem.Metadata.Add("Id", template.Id.ToString());

                    HandleOutFile(
                        $@"mailmergetemplates\{template.Id}\metadata.json",
                        SerializeUtility.SerializeJson(template.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportDuplicateRule(List<DuplicateRule> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting duplicate rules...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "duplicaterules", Name = "Duplicate Rules", Type = IndexItemType.Folder, Order = 15, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = DUPLICATE_RULE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var rules = this.Service.GetData<DuplicateRule>(DuplicateRule.EntityLogicalName, ids);

                foreach (var rule in rules)
                {
                    HandleOutFile(
                        $@"duplicaterules\{rule.Id}.metadata.json",
                        SerializeUtility.SerializeJson(rule.GetMetadataObject(IncludeAllProperty)),
                        null,
                        indexItem.Children,
                        new IndexLineItem { Key = $"{rule.Id}.metadata.json", Name = rule.Name, Type = IndexItemType.FileJson }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportConnectionRole(List<ConnectionRole> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting connection roles...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "connectionroles", Name = "Connection Roles", Type = IndexItemType.Folder, Order = 16, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = CONNECTION_ROLE_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var roles = this.Service.GetData<ConnectionRole>(ConnectionRole.EntityLogicalName, ids);

                foreach (var role in roles)
                {
                    HandleOutFile(
                        $@"connectionroles\{role.Id}.metadata.json",
                        SerializeUtility.SerializeJson(role.GetMetadataObject(IncludeAllProperty)),
                        null,
                        indexItem.Children,
                        new IndexLineItem { Key = $"{role.Id}.metadata.json", Name = role.Name, Type = IndexItemType.FileJson }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportReport(List<Models.Report> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting reports...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "reports", Name = "Reports", Type = IndexItemType.Folder, Order = 17, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = REPORT_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var reports = this.Service.GetData<Models.Report>(Models.Report.EntityLogicalName, ids);

                foreach (var record in reports)
                {
                    var metadata = new Dictionary<string, object>
                    {
                        { "Type", "Report" },
                        { "Id", record.Id.ToString() }
                    };

                    HandleOutFile(
                        $@"reports\{record.Id}.metadata.json",
                        SerializeUtility.SerializeJson(record.GetMetadataObject(IncludeAllProperty)),
                        null,
                        indexItem.Children,
                        new IndexLineItem { Key = $"{record.Id}.metadata.json", Name = record.Name, Type = IndexItemType.FileJson, Metadata = metadata }
                    );

                    if (!string.IsNullOrEmpty(record.BodyText))
                    {
                        var fileName = !string.IsNullOrWhiteSpace(record.FileName) ? record.FileName : $"{record.Id}.rdl";
                        HandleOutFile(
                            $@"reports\{record.Id}\{fileName}",
                            null,
                            System.Text.Encoding.UTF8.GetBytes(record.BodyText),
                            indexItem.Children,
                            new IndexLineItem { Key = fileName, Name = fileName, Type = IndexItemType.File, ContentType = IndexLineItemContentType.Report, Metadata = metadata }
                        );
                    }
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportCanvasApp(List<Models.CanvasApp> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting canvas apps...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "canvasapps", Name = "Canvas Apps", Type = IndexItemType.Folder, Order = 18, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = CANVAS_APP_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();

                // Query with only the primary key and name — the only attributes guaranteed
                // on every canvasapp subtype (including Custom Pages). Everything else is
                // hydrated per-record via RetrieveSafe to avoid attribute-not-found errors.
                var safeQuery = new Microsoft.Xrm.Sdk.Query.QueryExpression(Models.CanvasApp.EntityLogicalName)
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("canvasappid", "name")
                };
                var idCondition = new Microsoft.Xrm.Sdk.Query.ConditionExpression(
                    "canvasappid", Microsoft.Xrm.Sdk.Query.ConditionOperator.In);
                safeQuery.Criteria.AddCondition(idCondition);
                var canvasApps = this.Service.GetData<Models.CanvasApp>(safeQuery, idCondition, ids);

                // Hydrate all optional attributes per record — absent on Custom Pages
                var optionalColumns = new Microsoft.Xrm.Sdk.Query.ColumnSet(
                    "uniquename", "ismanaged", "componentstate", "statecode", "statuscode",
                    "publishedon", "solutionid", "createdon", "modifiedon",
                    "appversion", "introducedversion", "description",
                    "currentversiondefinition", "contenturi", "createdby", "modifiedby");

                foreach (var app in canvasApps)
                {
                    var extra = this.Service.RetrieveSafe(Models.CanvasApp.EntityLogicalName, app.Id, optionalColumns);
                    if (extra == null) continue;
                    foreach (var attr in extra.Attributes)
                    {
                        if (!app.Contains(attr.Key))
                            app[attr.Key] = attr.Value;
                    }
                }

                foreach (var record in canvasApps)
                {
                    // Use UniqueName (schema name) as the folder key so that the same app
                    // in different environments (different GUIDs) maps to the same path and
                    // is compared correctly. Fall back to GUID only if UniqueName is absent.
                    var appKey = !string.IsNullOrWhiteSpace(record.UniqueName)
                        ? record.UniqueName
                        : record.Id.ToString();

                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = appKey, Name = record.Name ?? appKey, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });
                    currentIndexItem.Metadata.Add("Type", "CanvasApp");
                    currentIndexItem.Metadata.Add("Id", record.Id.ToString());

                    HandleOutFile(
                        $@"canvasapps\{appKey}\metadata.json",
                        SerializeUtility.SerializeJson(record.GetMetadataObject(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"canvasapps\{appKey}\definition.json",
                        SerializeUtility.FormatJson(record.CurrentVersionDefinition),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "definition.json", Name = "definition.json", Type = IndexItemType.FileJson, ContentType = IndexLineItemContentType.CanvasApp }
                    );

                    // --- Fine-grained .msapp extraction ---
                    ExportCanvasAppMsappContents(record, appKey, currentIndexItem);
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        void ExportCanvasAppMsappContents(Models.CanvasApp record, string appKey, IndexLineItem parentIndexItem)
        {
            byte[] msappBytes = null;
            try
            {
                msappBytes = record.DownloadMsapp(this.Service);
            }
            catch (Exception ex)
            {
                Logger?.Log($"[CanvasApp] Could not download .msapp for '{record.Name}': {ex.Message}");
            }

            if (msappBytes == null || msappBytes.Length == 0)
                return;

            // Save the raw .msapp for reference
            HandleOutFile(
                $@"canvasapps\{appKey}\app.msapp",
                null,
                msappBytes,
                parentIndexItem.Children,
                new IndexLineItem { Key = "app.msapp", Name = "app.msapp", Type = IndexItemType.File }
            );

            // Extract and save individual entries
            List<Utilities.CanvasAppMsappExtractor.MsappEntry> entries;
            try
            {
                entries = Utilities.CanvasAppMsappExtractor.Extract(msappBytes);
            }
            catch (Exception ex)
            {
                Logger?.Log($"[CanvasApp] Could not extract .msapp for '{record.Name}': {ex.Message}");
                return;
            }

            if (entries.Count == 0)
                return;

            // Build a sub-folder "msapp" to hold extracted entries
            var msappIndexItem = AddIndexItem(parentIndexItem.Children, new IndexLineItem
            {
                Key = "msapp",
                Name = "msapp",
                Type = IndexItemType.Folder,
                Children = new List<IndexLineItem>()
            });

            foreach (var entry in entries)
            {
                // Normalise to backslash for path building
                var relPath = entry.EntryPath.Replace('/', '\\');
                var outPath = $@"canvasapps\{appKey}\msapp\{relPath}";

                // Determine index item type from extension
                var ext = System.IO.Path.GetExtension(entry.EntryPath).ToLower();
                IndexItemType itemType;
                switch (ext)
                {
                    case ".json": itemType = IndexItemType.FileJson; break;
                    case ".xml":  itemType = IndexItemType.FileXml;  break;
                    default:      itemType = entry.IsText ? IndexItemType.FileTxt : IndexItemType.File; break;
                }

                // Build nested folder structure inside the msapp index item
                var entryParent = EnsureMsappFolder(msappIndexItem, entry.EntryPath);

                HandleOutFile(
                    outPath,
                    entry.IsText ? entry.TextContent : null,
                    entry.IsText ? null : entry.BinaryContent,
                    entryParent.Children,
                    new IndexLineItem
                    {
                        Key  = System.IO.Path.GetFileName(entry.EntryPath),
                        Name = System.IO.Path.GetFileName(entry.EntryPath),
                        Type = itemType,
                    }
                );
            }
        }

        void ExportCloudFlow(List<Workflow> items, int weight)
        {
            if (BgWorker?.CancellationPending == true) return;
            if (items.Count == 0) return;

            CurrentCompleted = 0;
            CurrentTotal = items.Count;
            CurrentLabel = "Exporting cloud flows...";
            var indexItem = AddIndexItem(IndexData, new IndexLineItem { Key = "cloudflows", Name = "Cloud Flows", Type = IndexItemType.Folder, Order = 19, Children = new List<IndexLineItem>() });

            SendProgressSnapshot();
            int bufferSize = CLOUD_FLOW_BUFFER_SIZE;
            for (var i = 0; i < items.Count; i += bufferSize)
            {
                if (BgWorker?.CancellationPending == true) return;
                var ids = items.Select(x => x.Id).Skip(i).Take(bufferSize).ToList();
                var flows = this.Service.GetData<Workflow>(Workflow.EntityLogicalName, ids);

                foreach (var flow in flows)
                {
                    // Use UniqueName as key (stable across envs); fall back to Name then GUID.
                    // Sanitize to strip any characters that are illegal in file/folder paths.
                    var flowKey = SanitizeFolderName(
                        !string.IsNullOrWhiteSpace(flow.UniqueName)
                            ? flow.UniqueName
                            : (!string.IsNullOrWhiteSpace(flow.Name) ? flow.Name : flow.Id.ToString()));

                    var currentIndexItem = AddIndexItem(indexItem.Children, new IndexLineItem { Key = flowKey, Name = flow.Name ?? flowKey, Type = IndexItemType.Folder, Children = new List<IndexLineItem>() });

                    HandleOutFile(
                        $@"cloudflows\{flowKey}\metadata.json",
                        SerializeUtility.SerializeJson(flow.GetMetadataObjectByCategory(IncludeAllProperty)),
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "metadata.json", Name = "metadata.json", Type = IndexItemType.FileJson }
                    );
                    HandleOutFile(
                        $@"cloudflows\{flowKey}\clientdata.txt",
                        flow.ClientData,
                        null,
                        currentIndexItem.Children,
                        new IndexLineItem { Key = "clientdata.txt", Name = "clientdata.txt", Type = IndexItemType.FileTxt }
                    );
                }

                CurrentCompleted += ids.Count;
                OverallCompleted += ids.Count * weight;
                SendProgressSnapshot();
            }
        }

        /// <summary>
        /// Removes or replaces characters that are illegal in Windows file/folder names,
        /// including path separators, so that a display name can be used safely as a folder key.
        /// </summary>
        static string SanitizeFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "_";

            // Replace path separators with '_' to avoid accidentally creating sub-folders.
            name = name.Replace('/', '_').Replace('\\', '_');

            // Replace all other invalid file-name characters with '_'.
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            // Trim trailing dots/spaces (Windows disallows them at end of folder names).
            name = name.TrimEnd('.', ' ');

            return string.IsNullOrWhiteSpace(name) ? "_" : name;
        }

        /// <summary>
        /// Walks / creates the folder hierarchy inside msappRoot for the given entryPath.
        /// Returns the direct parent IndexLineItem whose Children list should receive the file.
        /// </summary>
        IndexLineItem EnsureMsappFolder(IndexLineItem msappRoot, string entryPath)
        {
            var parts = entryPath.Replace('\\', '/').Split('/');
            var current = msappRoot;

            // Walk all parts except the last (which is the file name)
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (string.IsNullOrEmpty(part)) continue;

                if (current.Children == null)
                    current.Children = new List<IndexLineItem>();

                var existing = current.Children.Find(x => x.Key == part);
                if (existing == null)
                {
                    existing = AddIndexItem(current.Children, new IndexLineItem
                    {
                        Key      = part,
                        Name     = part,
                        Type     = IndexItemType.Folder,
                        Children = new List<IndexLineItem>()
                    });
                }

                current = existing;
            }

            if (current.Children == null)
                current.Children = new List<IndexLineItem>();

            return current;
        }

        void SendProgressSnapshot()
        {
            OnProgress(new ExportProgressSnapshot
            {
                CurrentLabel = CurrentLabel,
                CurrentProgress = GetProgressValue(0, 100, CurrentTotal, CurrentCompleted),
                OverallProgress = GetProgressValue(0, 100, OverallTotal, OverallCompleted),
                CurrentTotal = CurrentTotal,
                CurrentCompleted = CurrentCompleted,
            });
        }

        IndexLineItem AddIndexItem(List<IndexLineItem> items, IndexLineItem item)
        {
            if (this.GenerateIndexFile)
            {
                items.Add(item);
            }

            return item;
        }

        void HandleOutFile(string path, string content, byte[] data, List<IndexLineItem> listIndexItems, IndexLineItem indexLineItem)
        {
            OutFileFunc(path, content, data);

            if (this.GenerateIndexFile)
            {
                bool isEmpty = string.IsNullOrEmpty(content) && (data == null || data.Length == 0);
                if (!isEmpty)
                {
                    indexLineItem.Checksum = GetChecksum(content, data);
                    AddIndexItem(listIndexItems, indexLineItem);
                }
            }
        }

        /// <summary>
        /// A helper method that decompresses the Ribbon data returned
        /// </summary>
        /// <param name="data">The compressed ribbon data</param>
        /// <returns></returns>
        private static byte[] UnzipRibbon(byte[] data)
        {
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            memStream.Write(data, 0, data.Length);
            System.IO.Packaging.ZipPackage package = (System.IO.Packaging.ZipPackage)System.IO.Packaging.Package.Open(memStream, System.IO.FileMode.Open);

            System.IO.Packaging.ZipPackagePart part = (System.IO.Packaging.ZipPackagePart)package.GetPart(new Uri("/RibbonXml.xml", UriKind.Relative));
            using (System.IO.Stream strm = part.GetStream())
            {
                long len = strm.Length;
                byte[] buff = new byte[len];
                strm.Read(buff, 0, (int)len);
                return buff;
            }
        }

        private static void CalculateIndexChecksum(IndexLineItem item)
        {
            if (item.Children != null && item.Children.Count > 0)
            {
                item.Children.ForEach(x => CalculateIndexChecksum(x));
                item.Checksum = GetChecksum(string.Join("-", item.Children.OrderBy(x => x.Checksum).Select(x => x.Checksum)), null);
            }
        }

        private static string GetChecksum(string content, byte[] data2)
        {
            MD5 md5Hash = MD5.Create();

            byte[] data;
            if (data2 != null && data2.Length > 0)
            {
                data = md5Hash.ComputeHash(data2);
            }
            else if (string.IsNullOrEmpty(content))
            {
                return "00000000000000000000000000000000";
            }
            else
            {

                data = md5Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(content));
            }

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
