using RioCanada.Crm.ComponentExportComparer.Core.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RioCanada.Crm.ComponentExportComparer.Core.Models
{
    [EntityLogicalName(EntityLogicalName)]
    class PluginAssembly : Entity
    {
        public const string EntityLogicalName = "pluginassembly";

        public PluginAssembly() : base(EntityLogicalName) { }
        public PluginAssembly(Guid id) : base(EntityLogicalName, id) { }

        public string Name { get => this.Get<string>("name"); set => this.Set("name", value); }
        public string FriendlyName { get => this.Get<string>("friendlyname"); set => this.Set("friendlyname", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public string Version { get => this.Get<string>("version"); set => this.Set("version", value); }
        public string PublicKeyToken { get => this.Get<string>("publickeytoken"); set => this.Set("publickeytoken", value); }
        public string Culture { get => this.Get<string>("culture"); set => this.Set("culture", value); }

        /// <summary>Base64-encoded content of the assembly DLL.</summary>
        public string Content { get => this.Get<string>("content"); set => this.Set("content", value); }

        public OptionSetValue IsolationMode { get => this.Get<OptionSetValue>("isolationmode"); set => this.Set("isolationmode", value); }
        public OptionSetValue SourceType { get => this.Get<OptionSetValue>("sourcetype"); set => this.Set("sourcetype", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public Guid? SolutionId { get => this.Get<Guid?>("solutionid"); set => this.Set("solutionid", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<PluginAssembly> FindByNames(
            OrganizationService service,
            IEnumerable<string> patterns,
            IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<PluginAssembly>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet(
                    "pluginassemblyid", "name", "friendlyname", "description",
                    "version", "publickeytoken", "culture",
                    "isolationmode", "sourcetype", "componentstate",
                    "ismanaged", "solutionid", "createdon", "modifiedon")
            };

            // Exclude system assemblies
            query.Criteria.AddCondition("name", ConditionOperator.NotLike, "Microsoft.%");
            query.Criteria.AddCondition("name", ConditionOperator.NotLike, "ActivityFeeds.%");

            Utilities.Helper.ApplyPatternFilter(query, "name", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<PluginAssembly>(query);
        }

        /// <summary>
        /// Fetches the assembly DLL content (base64) for a list of assembly IDs.
        /// Content is fetched in a separate query to avoid timeouts on large sets.
        /// </summary>
        public static List<PluginAssembly> FetchContent(OrganizationService service, IEnumerable<Guid> ids)
        {
            if (!ids.Any()) return new List<PluginAssembly>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                ColumnSet = new ColumnSet("pluginassemblyid", "name", "content")
            };

            return service.GetData<PluginAssembly, Guid>(query, "pluginassemblyid", ids);
        }

        public object GetMetadataObject(bool includeAllProperty)
        {
            if (includeAllProperty)
            {
                return new
                {
                    this.ComponentState,
                    this.CreatedBy,
                    this.CreatedOn,
                    this.Culture,
                    this.Description,
                    this.FriendlyName,
                    this.Id,
                    this.IsolationMode,
                    this.IsManaged,
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.Name,
                    this.PublicKeyToken,
                    this.SolutionId,
                    this.SourceType,
                    this.Version,
                };
            }
            else
            {
                return new
                {
                    this.ComponentState,
                    this.Culture,
                    this.Description,
                    this.FriendlyName,
                    this.Id,
                    this.IsolationMode,
                    this.IsManaged,
                    this.Name,
                    this.PublicKeyToken,
                    this.SourceType,
                    this.Version,
                };
            }
        }
    }
}
