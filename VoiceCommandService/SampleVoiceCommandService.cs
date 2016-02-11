using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;

namespace Sample.VoiceCommands
{
    public sealed class SampleVoiceCommandService : IBackgroundTask
    {
        private readonly Random m_random = new Random();

        private BackgroundTaskDeferral m_serviceDeferral;
        private VoiceCommandServiceConnection m_voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_serviceDeferral = taskInstance.GetDeferral();

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (triggerDetails?.Name == "SampleVcs")
            {
                try
                {
                    m_voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    m_voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    VoiceCommand voiceCommand = await m_voiceServiceConnection.GetVoiceCommandAsync();
                    var command = voiceCommand?.CommandName ?? "noCommand";

                    switch (command)
                    {
                        case "randomNumber":
                            GetRandomNumber();
                            break;

                        default:
                            LaunchApp();
                            break;
                    }

                    m_serviceDeferral.Complete();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async void GetRandomNumber()
        {
            var number = m_random.Next(100);

            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = $"Random number: {number}";
            userMessage.SpokenMessage = $"I picked {number}";

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await m_voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async void LaunchApp()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Launching Sample app";

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await m_voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            m_serviceDeferral?.Complete();
        }


        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            m_serviceDeferral?.Complete();
        }
    }
}
