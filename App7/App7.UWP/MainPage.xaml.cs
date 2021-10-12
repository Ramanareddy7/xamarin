using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace App7.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            //Loaded +=Button_Click;
        }
         private int _count = 0;
         StringBuilder sb = new StringBuilder();
         StringBuilder signalrsb = new StringBuilder();
        StringBuilder customersb = new StringBuilder();
        List<LineConversation> LineConversations = new List<LineConversation>();
        public async Task ContinuousRecognitionAutoDetectLanguageEng()
        {
            try
            {
                //obj.UserID = "Hello";
                int lineId = 0;
                //var client = EsClient();

                string AgentName = Environment.GetEnvironmentVariable("USERNAME");
                SpeechConfig config = SpeechConfig.FromSubscription("c2733300c04e4a68884c220da5a4d848", "westeurope");
                AutoDetectSourceLanguageConfig autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US" });
                TaskCompletionSource<int> stopMicRecognition = new TaskCompletionSource<int>();
                TaskCompletionSource<int> stopSpeakerRecognition = new TaskCompletionSource<int>();
                AudioConfig micInput = AudioConfig.FromDefaultMicrophoneInput();
                SpeechRecognizer micrecognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig,micInput);
                micrecognizer.Recognizing += (s, e) =>
                {
                //Console.WriteLine($"Agent:{e.Result.Text}");
                };

                micrecognizer.Recognized += async (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Agent: {e.Result.Text}");
                        sb.Append($"{AgentName}: {e.Result.Text}");
                        sb.Append("\n");
                        signalrsb.Append($"{AgentName}: {e.Result.Text}");
                        signalrsb.Append("@");
                        //var lineconverstion = new LineConversation
                        //{
                        //    LineId = lineId + 1,
                        //    Speaker = AgentName,
                        //    LineText = e.Result.Text
                        //};
                        //LineConversations.Add(lineconverstion);
                        //lineId++;
                        //SetText($"{AgentName}: {e.Result.Text}");
                        //obj.UserID = e.Result.Text;
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                };

                micrecognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }

                    stopMicRecognition.TrySetResult(0);
                };

                micrecognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\n    Session started event.");
                };

                micrecognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopMicRecognition.TrySetResult(0);
                };

                await micrecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                PushAudioInputStream pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(48000, 16, 2));
                AudioConfig speakerInput = AudioConfig.FromStreamInput(pushStream);
                SpeechRecognizer speakerRecognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, speakerInput);
                speakerRecognizer.Recognizing += (s, e) =>
                {
                //Console.WriteLine($"Client: Text={e.Result.Text}");
            };

                speakerRecognizer.Recognized += async (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        var recognize = ($"Customer: {e.Result.Text}");
                        sb.Append($"Customer: {e.Result.Text}");
                        sb.Append("\n");
                        signalrsb.Append($"Customer: {e.Result.Text}");
                        sb.Append("@");
                        customersb.Append($"{e.Result.Text}");
                        //var lineconverstion = new LineConversation
                        //{
                        //    LineId = lineId + 1,
                        //    Speaker = "Customer",
                        //    LineText = e.Result.Text
                        //};
                        //LineConversations.Add(lineconverstion);
                        //lineId++;
                        //SetText($"Customer: {e.Result.Text}");
                        //customertext($" {e.Result.Text}");
                        // obj.UserID = "Bye";

                        //}
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        var speech = ($"NOMATCH: Speech could not be recognized.");
                    //timer1.Enabled = true;
                    //timer1.Interval = 10000;
                    await speakerRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                        await micrecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                };

                speakerRecognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }

                    stopSpeakerRecognition.TrySetResult(0);
                };

                speakerRecognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\nSession started event.");
                };

                speakerRecognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\nSession stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopSpeakerRecognition.TrySetResult(0);
                };

                await speakerRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                WasapiLoopbackCapture capture = new WasapiLoopbackCapture();

                capture.DataAvailable += async (s, e) =>
                {
                    if (_count == 0)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(capture.WaveFormat));
                        _count++;
                    }
                    var resampledByte = ToPCM16(e.Buffer, e.BytesRecorded, capture.WaveFormat); //ResampleWasapi(s, e);
                     pushStream.Write((byte[])resampledByte); // try to push buffer here
                };
                capture.RecordingStopped += (s, e) =>
                {

                    capture.Dispose();
                };
                capture.StartRecording();
                Console.WriteLine("Record Started, Press Any key to stop the record");
                Console.ReadLine();
                capture.StopRecording();
                pushStream.Close();

                Task.WaitAny(new[] { stopSpeakerRecognition.Task, stopMicRecognition.Task });
                await speakerRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                await micrecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        delegate void SetTextCallback(string text);
        //private void SetText(string text)
        //{

        //    if (this.Conversation.Invokerequired)
        //    {
        //        SetTextCallback d = new SetTextCallback(SetText);
        //        this.Invoke(d, new object[] { text });
        //    }
        //    else
        //    {
        //        this.Conversation.Items.Add(text);
        //    }
        //}
        //private void customertext(string text)
        //{
        //    if (this.CustomerConversation.InvokeRequired)
        //    {
        //        SetTextCallback d = new SetTextCallback(customertext);
        //        this.Invoke(d, new object[] { text });
        //    }
        //    else
        //    {

        //        //this.textBox2.Text += text;
        //        this.CustomerConversation.Items.Add(text);
        //    }
        //}

        private object ToPCM16(byte[] buffer, int bytesRecorded, WaveFormat waveFormat)
        {
            throw new NotImplementedException();
        }
        private void ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private  void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private  void Button_Click1(object sender, RoutedEventArgs e)
        {
            
        }
        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ContinuousRecognitionAutoDetectLanguageEng();
        }
        private void ListView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Conversation_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
          
        }
    }
}
