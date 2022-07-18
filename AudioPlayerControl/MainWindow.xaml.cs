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
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace AudioPlayerControl
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += timer_tick;
            /*Process proc = new Process();
            proc.StartInfo.FileName = @"C:\Users\user\Downloads\ffmpeg.exe";
            proc.StartInfo.Arguments = "-i \"" + filename + "\" -vn -ar 44100 -ac 1 -f f32le -";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);
            proc.Start();
            proc.BeginErrorReadLine();
            ProcessStream(proc.StandardOutput.BaseStream);
            proc.WaitForExit(10000); // 10s
            if (!proc.HasExited)
            {
                proc.Kill();
                Environment.Exit(1);
            }*/
        }
        MediaPlayer mediaPlayer = new MediaPlayer();
        
        string filename;
        double durTest;
     
    /*    static void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // Console.WriteLine(e.Data);
                // do nothing
            }
        }*/
        /*static void ProcessStream(Stream stream)
        {
            int didread;
            int offset = 0;
            byte[] buffer = new byte[sizeof(Single) * (1024 + 1)];

            int length, residual_length;

            while ((didread = stream.Read(buffer, offset, sizeof(Single) * 1024)) != 0)
            {
                length = offset + didread;
                residual_length = length % sizeof(Single);

                if (residual_length == 0)
                {
                    ProcessBuffer(buffer, length);
                    offset = 0;
                }
                else
                {
                    length -= residual_length;
                    ProcessBuffer(buffer, length);
                    Array.Copy(buffer, length, buffer, 0, residual_length);
                    offset = residual_length;
                }
            }
        }
        static void ProcessBuffer(byte[] buffer, int length)
        {
            int index = 0;
            float sample_value;

            while (index < length)
            {
                sample_value = BitConverter.ToSingle(buffer, index);
                index += sizeof(Single);
                // to deal with sample_value
            }
        }*/
        private void timer_tick(object sender, EventArgs e)
        {
            time.Text = mediaPlayer.Position.ToString(@"mm\:ss");
            SliderDuration.Value = mediaPlayer.Position.TotalSeconds;
        }
    
        private void BT_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                DefaultExt = ".mp3"
            };
            bool? DialogOk = fileDialog.ShowDialog();
            if (DialogOk == true)
            {
                filename = fileDialog.FileName;
                TBFileName.Text = fileDialog.SafeFileName;
                mediaPlayer.Open(new Uri(filename));
                /*if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                  //  durTest = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    
                    SliderDuration.Maximum = durTest;
                }*/
               // SliderDuration.Maximum = durTest;
                //    sValue.Content = sample_value.ToString();
            }
        }

     
        private void BT_Click_Play(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                durTest = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SliderDuration.Maximum = durTest;
            }
            mediaPlayer.Play();
            
            timer.Start();
        }

        private void BT_Click_Pause(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void BT_Click_Stop(object sender, RoutedEventArgs e)
        {
            SliderDuration.Value = 0;
            mediaPlayer.Position = new TimeSpan(0, 0, 0);
            time.Text ="00:00";
            mediaPlayer.Stop();
            timer.Stop();
            
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          //  ((Slider)sender).SelectionEnd = e.NewValue;
            mediaPlayer.SpeedRatio = e.NewValue;
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = SliderVolume.Value;
            }
           
        }

        private void SliderDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (btloop.IsPressed)
            {
                if (SliderDuration.Value == SliderDuration.Maximum)
                {
                    mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    mediaPlayer.Play();
                    // btloop.IsEnabled = false;
                }
            }
        }

      private void BT_Click_loop (object sender, RoutedEventArgs e)
        {
          //  btloop.IsEnabled = false;
        }
    }
}
