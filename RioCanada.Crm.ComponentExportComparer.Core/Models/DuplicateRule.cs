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
    class DuplicateRule : Entity
    {
        public const string EntityLogicalName = "duplicaterule";

        public DuplicateRule() : base(EntityLogicalName) { }
        public DuplicateRule(Guid id) : base(EntityLogicalName, id) { }

        public string Name { get => this.Get<string>("name"); set => this.Set("name", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public string BaseEntityName { get => this.Get<string>("baseentityname"); set => this.Set("baseentityname", value); }
        public string MatchingEntityName { get => this.Get<string>("matchingentityname"); set => this.Set("matchingentityname", value); }
        public bool IsCaseSensitive { get => this.Get<bool>("iscasesensitive"); set => this.Set("iscasesensitive", value); }
        public OptionSetValue StatusCode { get => this.Get<OptionSetValue>("statuscode"); set => this.Set("statuscode", value); }
        public OptionSetValue StateCode { get => this.Get<OptionSetValue>("statecode"); set => this.Set("statecode", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<DuplicateRule> FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<DuplicateRule>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("duplicateruleid", "name")
            };

            Utilities.Helper.ApplyPatternFilter(query, "name", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<DuplicateRule>(query);
        }

        public object GetMetadataObject(bool includeAllProperty)
        {
            if (includeAllProperty)
            {
                return new
                {
                    this.BaseEntityName,
                    this.ComponentState,
                    this.CreatedBy,
                    this.CreatedOn,
                    this.Description,
                    this.Id,
                    this.IsCaseSensitive,
                    this.IsManaged,
                    this.MatchingEntityName,
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.Name,
                    this.StateCode,
                    this.StatusCode,
                };
            }
            else
            {
                return new
                {
                    this.BaseEntityName,
                    this.Description,
                    this.IsCaseSensitive,
                    this.IsManaged,
                    this.MatchingEntityName,
                    this.Name,
                    this.StateCode,
                    this.StatusCode,
                };
            }
        }
    }
}
