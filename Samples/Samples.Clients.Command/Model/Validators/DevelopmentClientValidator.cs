using GS.DecoupleIt.DependencyInjection.Automatic;

namespace Samples.Clients.Command.Model.Validators
{
    [Singleton(Environments = "Development")]
    internal sealed class DevelopmentClientValidator : IClientValidator
    {
        public bool IsNameValid(string name)
        {
            return !name.Contains("TEST");
        }
    }
}
