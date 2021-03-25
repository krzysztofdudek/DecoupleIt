using GS.DecoupleIt.DependencyInjection.Automatic;

namespace Samples.Clients.Command.Model.Validators
{
    [Singleton]
    internal sealed class DefaultClientValidator : IClientValidator
    {
        public bool IsNameValid(string name)
        {
            return true;
        }
    }
}
