using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using PharmacyManagement.ViewModels;
#if SQL_CHECK
using System.Data;
using Microsoft.Data.SqlClient;
using PharmacyManagement.Data;
#endif

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            CheckMvvm();
            Console.WriteLine("PASS T08: notification, two-way Binding, Command, CanExecuteChanged.");

            if (args.Contains("--sql"))
            {
#if SQL_CHECK
                using var connection = DbHelper.GetConnection();
                Check(connection.State == ConnectionState.Open, "DbHelper must return an open connection.");
                using var command = new SqlCommand("SELECT 1", connection);
                Check(Convert.ToInt32(command.ExecuteScalar()) == 1, "SELECT 1 must return 1.");
                Console.WriteLine("PASS T09: open connection and SELECT 1.");
#else
                throw new InvalidOperationException("Run with -p:CheckSql=true -- --sql to check T09.");
#endif
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static void CheckMvvm()
    {
        var vm = new TestViewModel();
        var changes = new List<string?>();
        vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
        vm.Ten = "Bao";
        vm.Ten = "Bao";
        Check(changes.SequenceEqual(new[] { "Ten" }), "An unchanged value must not notify twice.");
        Check(!vm.AssignTen("Bao"), "SetProperty must report an unchanged value.");
        Check(vm.AssignTen("Trinh"), "SetProperty must report a changed value.");

        var input = new TextBox { DataContext = vm };
        input.SetBinding(TextBox.TextProperty, new Binding(nameof(TestViewModel.Ten))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        var output = new TextBlock { DataContext = vm };
        output.SetBinding(TextBlock.TextProperty, new Binding(nameof(TestViewModel.Ten)));
        Check(output.Text == "Trinh", "Binding must read the initial value.");
        input.SetCurrentValue(TextBox.TextProperty, "Bao Trinh");
        Check(vm.Ten == "Bao Trinh" && output.Text == vm.Ten, "Typing must update the ViewModel and TextBlock.");
        vm.Ten = "";
        Check(input.Text == "" && output.Text == "", "PropertyChanged must update both controls.");

        object? received = null;
        var command = new RelayCommand(p => received = p, _ => !string.IsNullOrWhiteSpace(vm.Ten));
        var button = new Button { Command = command, CommandParameter = "save" };
        Check(!button.IsEnabled, "The button must be disabled for an empty name.");
        var requeryCount = 0;
        EventHandler handler = (_, _) => requeryCount++;
        command.CanExecuteChanged += handler;
        vm.Ten = "Bao";
        CommandManager.InvalidateRequerySuggested();
        PumpDispatcher();
        Check(button.IsEnabled && requeryCount > 0, "Requery must enable the button and notify subscribers.");
        command.Execute(button.CommandParameter);
        Check(Equals(received, "save"), "Execute must forward CommandParameter.");
        command.CanExecuteChanged -= handler;
        var previousCount = requeryCount;
        CommandManager.InvalidateRequerySuggested();
        PumpDispatcher();
        Check(requeryCount == previousCount, "Removed handlers must not receive requery events.");

        var unconditional = new RelayCommand(p => received = p);
        Check(unconditional.CanExecute(null), "A command without a predicate must be enabled.");
        unconditional.Execute(null);
        Check(received is null, "Execute must accept a null parameter.");
        try
        {
            _ = new RelayCommand(null!);
            throw new InvalidOperationException("A null execute delegate must be rejected.");
        }
        catch (ArgumentNullException) { }
        GC.KeepAlive(handler); // CommandManager giữ weak reference tới handler.
    }

    private static void PumpDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class TestViewModel : BaseViewModel
    {
        private string _ten = "";
        public string Ten { get => _ten; set => SetProperty(ref _ten, value); }
        public bool AssignTen(string value) => SetProperty(ref _ten, value, nameof(Ten));
    }
}
