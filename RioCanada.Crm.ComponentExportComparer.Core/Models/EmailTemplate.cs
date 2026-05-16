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
    class EmailTemplate : Entity
    {
        public const string EntityLogicalName = "template";

        public EmailTemplate() : base(EntityLogicalName) { }
        public EmailTemplate(Guid id) : base(EntityLogicalName, id) { }

        public string Title { get => this.Get<string>("title"); set => this.Set("title", value); }
        public string Subject { get => this.Get<string>("subject"); set => this.Set("subject", value); }
        public string Body { get => this.Get<string>("body"); set => this.Set("body", value); }
        public string Description { get => this.Get<string>("description"); set => this.Set("description", value); }
        public OptionSetValue TemplateTypeCode { get => this.Get<OptionSetValue>("templatetypecode"); set => this.Set("templatetypecode", value); }
        public bool IsPersonal { get => this.Get<bool>("ispersonal"); set => this.Set("ispersonal", value); }
        public bool IsManaged { get => this.Get<bool>("ismanaged"); set => this.Set("ismanaged", value); }
        public BooleanManagedProperty IsCustomizable { get => this.Get<BooleanManagedProperty>("iscustomizable"); set => this.Set("iscustomizable", value); }
        public string IntroducedVersion { get => this.Get<string>("introducedversion"); set => this.Set("introducedversion", value); }
        public string PresentationXml { get => this.Get<string>("presentationxml"); set => this.Set("presentationxml", value); }
        public string SubjectPresentationXml { get => this.Get<string>("subjectpresentationxml"); set => this.Set("subjectpresentationxml", value); }
        public Guid SolutionId { get => this.Get<Guid>("solutionid"); set => this.Set("solutionid", value); }
        public OptionSetValue ComponentState { get => this.Get<OptionSetValue>("componentstate"); set => this.Set("componentstate", value); }
        public EntityReference CreatedBy { get => this.Get<EntityReference>("createdby"); set => this.Set("createdby", value); }
        public DateTime? CreatedOn { get => this.Get<DateTime?>("createdon"); set => this.Set("createdon", value); }
        public EntityReference ModifiedBy { get => this.Get<EntityReference>("modifiedby"); set => this.Set("modifiedby", value); }
        public DateTime? ModifiedOn { get => this.Get<DateTime?>("modifiedon"); set => this.Set("modifiedon", value); }

        public static List<EmailTemplate> FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)
        {
            if (!patterns.Any(x => !string.IsNullOrWhiteSpace(x)))
                return new List<EmailTemplate>();

            QueryExpression query = new QueryExpression(EntityLogicalName)
            {
                Distinct = true,
                ColumnSet = new ColumnSet("templateid", "title")
            };

            Utilities.Helper.ApplyPatternFilter(query, "title", patterns);
            Utilities.Helper.ApplySolutionFilter(query, EntityLogicalName + "id", solutionIds);

            return service.GetBigData<EmailTemplate>(query);
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
                    this.ModifiedBy,
                    this.ModifiedOn,
                    this.SolutionId,
                    this.Subject,
                    this.TemplateTypeCode,
                    this.Title,
                };
            }
            else
            {
                return new
                {
                    this.Description,
                    this.IsManaged,
                    this.IsPersonal,
                    this.Subject,
                    this.TemplateTypeCode,
                    this.Title,
                };
            }
        }
    }
}
