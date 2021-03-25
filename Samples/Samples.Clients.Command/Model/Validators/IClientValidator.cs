using JetBrains.Annotations;

namespace Samples.Clients.Command.Model.Validators
{
    public interface IClientValidator
    {
        public bool IsNameValid([NotNull] string name);
    }
}
