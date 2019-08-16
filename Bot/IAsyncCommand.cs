using System.Threading.Tasks;
using System.Windows.Input;

namespace Bot
{
    public interface IAsyncCommand: ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
