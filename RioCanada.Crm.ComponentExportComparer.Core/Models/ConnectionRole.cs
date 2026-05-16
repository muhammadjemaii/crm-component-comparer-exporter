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
    class ConnectionRole : Entity
    {
        public const string EntityLogicalName = "connectionrole";

        public ConnectionRole() : base(EntityLogicalName) { }
        public ConnectionRole(Guid id) : base(EntityLogicalName, id) { }

        public string Name { get => this.Get<string>("name"); set => this.Set("name", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public OptionSetValue Category { get => this.Get<OptionSetValue>("category"); set => this.Set("category", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public OptionSetValue StatusCode { get => this.Get<OptionSetValue>("statuscode"); set => this.Set("statuscode", value); }
        public OptionSetValue StateCode { get => this.Get<OptionSetValue>("statecode"); set => this.Set("statecode", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public BooleanManagedProperty IsCustomizable { get => this.Get<BooleanManagedProperty>("iscustomizable"); set => this.Set("iscustomizable", value); }
        public string IntroducedVersion { get => this.Get<string>("introducedversion"); set => this.Set("introducedversion", value); }
        public Guid SolutionId { get => this.Get<Guid>("solutionid"); set => this.Set("solutionid", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<ConnectionRole> FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<ConnectionRole>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("connectionroleid", "name")
            };

            Utilities.Helper.ApplyPatternFilter(query, "name", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<ConnectionRole>(query);
        }

        public object GetMetadataObject(bool includeAllProperty)
        {
            if (includeAllProperty)
            {
                return new
                {
                    this.Category,
                    this.ComponentState,
                    this.CreatedBy,
                    this.CreatedOn,
                    this.Description,
                    this.Id,
                    this.IntroducedVersion,
                    this.IsCustomizable,
                    this.IsManaged,
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.Name,
                    this.SolutionId,
                    this.StateCode,
                    this.StatusCode,
                };
            }
            else
            {
                return new
                {
                    this.Category,
                    this.Description,
                    this.IsManaged,
                    this.Name,
                    this.StateCode,
                    this.StatusCode,
                };
            }
        }
    }
}
