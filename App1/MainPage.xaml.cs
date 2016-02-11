using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.QueryStringDotNET;
using NotificationsExtensions.Toasts;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel Vm => DataContext as MainViewModel;

        public MainPage()
        {
            this.DataContext = new MainViewModel();
            this.InitializeComponent();
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly Random m_random = new Random();
        private int m_value;

        public int Value
        {
            get { return m_value; }
            set { Set(nameof(Value), ref m_value, value); }
        }

        public ICommand NewNumberCommand { get; }

        public ICommand CreateToastCommand { get; }

        public MainViewModel()
        {
            NewNumberCommand = new RelayCommand(() =>
            {
                Value = m_random.Next(0, 100);
            });
            CreateToastCommand = new RelayCommand(CreateToast);
        }

        public void CreateToast()
        {
            ToastVisual visual = new ToastVisual
            {
                TitleText = new ToastText {Text = "Sample notification"},
                BodyTextLine1 = new ToastText {Text = $"Your latest random number: {Value}!"}
            };
            ToastActionsCustom actions = new ToastActionsCustom
            {
                Buttons =
                {
                    new ToastButton("Delete", new QueryString
                    {
                        {"action", "delete"},
                        {"id", Value.ToString()},
                    }.ToString()),
                    new ToastButton("Get a new one", new QueryString
                    {
                        {"action", "regenerate"},
                        {"id", Value.ToString()}
                    }.ToString())
                },
            };
            ToastContent content = new ToastContent
            {
                Visual = visual,
                Actions = actions,
            };
            ToastNotification toast = new ToastNotification(content.GetXml())
            {
                ExpirationTime = DateTime.Now.AddDays(2),
                Tag = Value.ToString(),
                Group = "Random Numbers!"
            };

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
