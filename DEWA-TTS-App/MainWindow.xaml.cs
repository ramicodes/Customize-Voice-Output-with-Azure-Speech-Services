using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CognitiveServicesTTS;
using System.Media;
using System.IO;
using System.Threading;

namespace DEWA_TTS_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);

            Dispatcher.Invoke(() =>
            {
                _logText.Text += (formattedStr + "\n");
                _logText.ScrollToEnd();
            });
        }
        private void SpeakBtn_Click(object sender, RoutedEventArgs e)
        {
            WriteLine("Starting Authtentication");
            string accessToken;

            // Note: The way to get api key:
            // Free: https://www.microsoft.com/cognitive-services/en-us/subscriptions?productId=/products/Bing.Speech.Preview
            // Paid: https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/Bing.Speech/pricingtier/S0
            Authentication auth = new Authentication("b1fdfe7637da45da95bb2ec668f1f0f2");

            try
            {
                accessToken = auth.GetAccessToken();
                Console.WriteLine("Token: {0}\n", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed authentication.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }

            WriteLine("Starting TTSSample request code execution.");

            string requestUri = "https://speech.platform.bing.com/synthesize";

            var cortana = new Synthesize();

            cortana.OnAudioAvailable += PlayAudio;
            cortana.OnError += ErrorHandler;

            // Reuse Synthesize object to minimize latency
            cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = InputText.Text,
                VoiceType = Gender.Male,
                // Refer to the documentation for complete list of supported locales.
                Locale = "ar-SA",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                VoiceName = "Microsoft Server Speech Text to Speech Voice (ar-SA, Naayf)",
                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                SpeakingRate = SpeakingRateTxt.Text,
                Pitch = PitchTxt.Text,
                Contour = ContourTxt.Text,
                Volume = VolumeTxt.Text,
                AuthorizationToken = "Bearer " + accessToken,
            }).Wait();
        }

        private static void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            Console.WriteLine(args.EventData);

            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(args.EventData);
            player.PlaySync();
            args.EventData.Dispose();
        }

        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }

    }
}
