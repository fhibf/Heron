
namespace Heron.Core.Model {
    public interface IMessengerService {

        void ShowInformation(string message);

        void ShowWarning(string message);

        void ShowError(string message);
    }
}
