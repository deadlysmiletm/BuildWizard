using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using System.Text;
using System;

namespace BuildWizard.ConfigSteps.DefaultModule
{
    internal struct WizardBuildIdentifier : IWizardStep
    {
        public string StepName => "Generate Build Identifier";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => string.Empty;

        public string IdentifierPrefix;
        public bool EndWithBuildDate;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            string buildIdentifier = "";
            if (repository.ContainsData(WizardRepositoryKeys.BUILD_IDENTIFIER))
                buildIdentifier = repository.GetData<string>(WizardRepositoryKeys.BUILD_IDENTIFIER);
            else
                buildIdentifier = IdentifierPrefix;

            if (EndWithBuildDate)
                buildIdentifier = GenerateBuildDateTime(buildIdentifier);

            repository.AddData(WizardRepositoryKeys.BUILD_IDENTIFIER, buildIdentifier);
            report.Success = true;
            return report;
        }

        private string GenerateBuildDateTime(string identifierPrefix)
        {
            DateTime today = DateTime.Today;

            StringBuilder identifierBuilder = new(identifierPrefix);
            identifierBuilder.Append("_")
                .Append(today.Day)
                .Append("-")
                .Append(today.Month)
                .Append("-")
                .Append(today.Year);

            return identifierBuilder.ToString();
        }
    }
}