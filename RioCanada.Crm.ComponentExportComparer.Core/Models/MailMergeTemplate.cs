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
    class MailMergeTemplate : Entity
    {
        public const string EntityLogicalName = "mailmergetemplate";

        public MailMergeTemplate() : base(EntityLogicalName) { }
        public MailMergeTemplate(Guid id) : base(EntityLogicalName, id) { }

        public string Name { get => this.Get<string>("name"); set => this.Set("name", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public OptionSetValue MailMergeType { get => this.Get<OptionSetValue>("mailmergetype"); set => this.Set("mailmergetype", value); }
        public string TemplateTypeCode { get => this.Get<string>("templatetypecode"); set => this.Set("templatetypecode", value); }
        public bool IsPersonal { get => this.Get<bool>("ispersonal"); set => this.Set("ispersonal", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public BooleanManagedProperty IsCustomizable { get => this.Get<BooleanManagedProperty>("iscustomizable"); set => this.Set("iscustomizable", value); }
        public string IntroducedVersion { get => this.Get<string>("introducedversion"); set => this.Set("introducedversion", value); }
        public Guid SolutionId { get => this.Get<Guid>("solutionid"); set => this.Set("solutionid", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public OptionSetValue StatusCode { get => this.Get<OptionSetValue>("statuscode"); set => this.Set("statuscode", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<MailMergeTemplate> FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<MailMergeTemplate>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("mailmergetemplateid", "name")
            };

            Utilities.Helper.ApplyPatternFilter(query, "name", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<MailMergeTemplate>(query);
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
                    this.Description,
                    this.Id,
                    this.IntroducedVersion,
                    this.IsCustomizable,
                    this.IsManaged,
                    this.IsPersonal,
                    this.MailMergeType,
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.Name,
                    this.SolutionId,
                    this.StatusCode,
                    this.TemplateTypeCode,
                };
            }
            else
            {
                return new
                {
                    this.Description,
                    this.IsManaged,
                    this.IsPersonal,
                    this.MailMergeType,
                    this.Name,
                    this.StatusCode,
                    this.TemplateTypeCode,
                };
            }
        }
    }
}
