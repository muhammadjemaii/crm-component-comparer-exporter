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
    class CanvasApp : Entity
    {
        public const string EntityLogicalName = "canvasapp";

        public CanvasApp() : base(EntityLogicalName) { }
        public CanvasApp(Guid id) : base(EntityLogicalName, id) { }

        public string Name { get => this.Get<string>("name"); set => this.Set("name", value); }
        public string UniqueName { get => this.Get<string>("uniquename"); set => this.Set("uniquename", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public string AppVersion { get => this.Get<string>("appversion"); set => this.Set("appversion", value); }
        public string IntroducedVersion { get => this.Get<string>("introducedversion"); set => this.Set("introducedversion", value); }
        public string ContentUri { get => this.Get<string>("contenturi"); set => this.Set("contenturi", value); }
        public string CurrentVersionDefinition { get => this.Get<string>("currentversiondefinition"); set => this.Set("currentversiondefinition", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public OptionSetValue StateCode { get => this.Get<OptionSetValue>("statecode"); set => this.Set("statecode", value); }
        public OptionSetValue StatusCode { get => this.Get<OptionSetValue>("statuscode"); set => this.Set("statuscode", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public DateTime? PublishedOn { get => this.Get<DateTime?>("publishedon"); set => this.Set("publishedon", value); }
        public Guid? SolutionId { get => this.Get<Guid?>("solutionid"); set => this.Set("solutionid", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<CanvasApp> FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<CanvasApp>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("canvasappid", "name")
            };

            Utilities.Helper.ApplyPatternFilter(query, "name", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<CanvasApp>(query);
        }

        public object GetMetadataObject(bool includeAllProperty)
        {
            if (includeAllProperty)
            {
                return new
                {
                    this.AppVersion,
                    this.ComponentState,
                    this.ContentUri,
                    this.CreatedBy,
                    this.CreatedOn,
                    this.Description,
                    this.Id,
                    this.IntroducedVersion,
                    this.IsManaged,
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.Name,
                    this.PublishedOn,
                    this.SolutionId,
                    this.StateCode,
                    this.StatusCode,
                    this.UniqueName,
                };
            }
            else
            {
                return new
                {
                    this.AppVersion,
                    this.Description,
                    this.IsManaged,
                    this.Name,
                    this.StateCode,
                    this.StatusCode,
                    this.UniqueName,
                };
            }
        }
    }
}
