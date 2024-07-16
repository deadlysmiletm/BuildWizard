
namespace BuildWizard.Core
{
    public interface IWizardStep
    {
        string StepName { get; }
        bool RequireUnityDomainCompilation { get; }
        string RequireSteps { get; }

        StepReport ExecuteStep(IWizardRepository repository);
    }
}