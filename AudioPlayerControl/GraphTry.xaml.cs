// ------------------------------------------------------------------------------------------------------------------------
// LightningChart® example code: SignalReader component is used for data capture from a file and generating a playback. 
// Data is forwarded to AutdioOutput component. Use headphones or other output device to hear an audio signal.
//
// If you need any assistance, or notice error in this example code, please contact support@lightningchart.com. 
//
// Permission to use this code in your application comes with LightningChart® license. 
//
// https://www.arction.com | support@lightningchart.com | sales@lightningchart.com
//
// © Arction Ltd 2009-2021. All rights reserved.  
// ------------------------------------------------------------------------------------------------------------------------
using Arction.Wpf.Charting;
using Arction.Wpf.Charting.Axes;
using Arction.Wpf.Charting.SeriesXY;
using Arction.Wpf.Charting.Views.ViewXY;
/*using Arction.Wpf.SemibindableCharting;
using Arction.Wpf.SemibindableCharting.Axes;
using Arction.Wpf.SemibindableCharting.SeriesXY;
using Arction.Wpf.SemibindableCharting.Views.ViewXY;*/
using Arction.Wpf.SignalProcessing;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Threading;

namespace AudioPlayerControl
{
    /// <summary>
    /// Interaction logic for ExampleAudioOutputSignalReader.xaml
    /// </summary>
    public partial class GraphTry : UserControl, IDisposable
    {
        
        private LightningChartUltimate _chart;

     
        private double[][] _audioData;

       
        private int _samplingFrequency;

        
        private int _channelCount;

      
        private string _fileName;

     
        private double _dataLength = 0; 
       
        private OpenFileDialog _openFileDialog;
        private SignalReader _signalReader;
        private AudioOutput _audioOutput;
        private bool _clean;
        DispatcherTimer timer = new DispatcherTimer();
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GraphTry()
        {
            _clean = false;
            _openFileDialog = new OpenFileDialog();
           
            _fileName = Environment.CurrentDirectory + "\\Content\\Whistle_48kHz.wav";
            string deploymentKey = "lgCAABW2ij + vBNQBJABVcGRhdGVhYmxlVGlsbD0yMDE5LTA2LTE1I1JldmlzaW9uPTACgD + BCRGnn7c6dwaDiJovCk5g5nFwvJ + G60VSdCrAJ + jphM8J45NmxWE1ZpK41lW1wuI4Hz3bPIpT7aP9zZdtXrb4379WlHowJblnk8jEGJQcnWUlcFnJSl6osPYvkxfq / B0dVcthh7ezOUzf1uXfOcEJ377 / 4rwUTR0VbNTCK601EN6 / ciGJmHars325FPaj3wXDAUIehxEfwiN7aa7HcXH6RqwOF6WcD8voXTdQEsraNaTYbIqSMErzg6HFsaY5cW4IkG6TJ3iBFzXCVfvPRZDxVYMuM + Q5vztCEz5k + Luaxs + S + OQD3ELg8 + y7a / Dv0OhSQkqMDrR / o7mjauDnZVt5VRwtvDYm6kDNOsNL38Ry / tAsPPY26Ff3PDl1ItpFWZCzNS / xfDEjpmcnJOW7hmZi6X17LM66whLUTiCWjj81lpDi + VhBSMI3a2I7jmiFONUKhtD91yrOyHrCWObCdWq + F5H4gjsoP0ffEKcx658a3ZF8VhtL8d9 + B0YtxFPNBQs =";
            LightningChartUltimate.SetDeploymentKey(deploymentKey);
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += timer_tick;
            _audioOutput = Resources["audioOutput"] as AudioOutput;
            _signalReader = _audioOutput.DataContext as SignalReader;
            
            CreateChart();
            PrefillAudioDataToChart();
         //   Start();
            Application.Current.MainWindow.Closing += ApplicationClosingDispose;
        }
        MediaPlayer mediaPlayer = new MediaPlayer();

        double durTest;
        double currentTime;
        double dPositionInSec;
        double xTime;
        int samplesCount;
        double curSoundSpeed;
        private void ApplicationClosingDispose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_clean)
            {
                Dispose();
            }
        }
        internal bool IsRunning
        {
            get
            {
                return _signalReader.IsReaderEnabled;
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            time.Content = mediaPlayer.Position.ToString(@"mm\:ss");
            sliderDuration.Value = mediaPlayer.Position.TotalSeconds;
        }
        public void Start()
        {      
            StartPlayback();
        }

        public void Stop()
        {
            sliderDuration.Value = 0;
            time.Content = "00:00";
            _audioOutput.IsOutputEnabled = false;
            _signalReader.StopRequest();
            mediaPlayer.Position = new TimeSpan(0, 0, 0);
            
            mediaPlayer.Stop();
            timer.Stop();
        }

        /// <summary>
        /// Call this method to stop threads, dispose unmanaged resources or 
        /// any other job that needs to be done before this example object is 
        /// ready for garbage collector. Note! You have two recommended ways 
        /// to implement CleanUp. 1. Prevent multiple calls by using e.g. a 
        /// bool variable in your class. 2. Implement CleanUp so that it 
        /// can be called safely multiple times.
        /// </summary>
        public void Dispose()
        {
            _clean = true;

            if (IsRunning == true)
            {
                Stop();

                Dispatcher.Invoke(
                new Action<object>(
                    delegate
                    {
                        _signalReader.DataGenerated -= SignalReader_DataGenerated;
                        _signalReader.Stopped -= SignalReader_Stopped;
                        _signalReader.Started -= SignalReader_Started;
                    }
                ),
                (object)null
            );
            }
            else
            {
                gridChart.Children.Clear();

                if (_chart != null)
                {
                    _chart.Dispose();
                    _chart = null;
                }
            }

        }

        private void CreateChart()
        {
           
            gridChart.Children.Clear();

            if (_chart != null)
            {
                
                _chart.Dispose();
                _chart = null;
            }

            
            _chart = new LightningChartUltimate();

            _chart.BeginUpdate();

            _chart.Title.Text = "Waveform";
            _chart.ChartName = "Waveform chart";

            _chart.ViewXY.XAxes[0].Title.Text = "Time";
            _chart.ViewXY.XAxes[0].ValueType = AxisValueType.Time;
            _chart.ViewXY.XAxes[0].AutoFormatLabels = false;
            _chart.ViewXY.XAxes[0].LabelsTimeFormat = "HH:mm.ss";
            _chart.ViewXY.YAxes[0].Title.Text = "Voltage";

            _chart.ViewXY.AxisLayout.YAxesLayout = YAxesLayout.Stacked;

         

           
            LineSeriesCursor cursor = new LineSeriesCursor(_chart.ViewXY, _chart.ViewXY.XAxes[0])
            {
                Style = CursorStyle.VerticalNoTracking
            };
            cursor.LineStyle.Color = Colors.White;
          
            cursor.ValueAtXAxis = 0;
            _chart.ViewXY.LineSeriesCursors.Add(cursor);

            _chart.EndUpdate();

            gridChart.Children.Add(_chart);

        }

        private void InitChart(int channelCount, int samplingFrequency)
        {
            _chart.BeginUpdate();

            
            _chart.ViewXY.SampleDataSeries.Clear(); 
            _chart.ViewXY.YAxes.Clear();            

            for (int channel = 0; channel < channelCount; channel++)
            {
               
                AxisY yAxis = new AxisY(_chart.ViewXY)
                {
                    AxisColor = DefaultColors.SeriesForBlackBackgroundWpf[channel % DefaultColors.SeriesForBlackBackgroundWpf.Length]
                };
                _chart.ViewXY.YAxes.Add(yAxis);

                
                SampleDataSeries sds = new SampleDataSeries(_chart.ViewXY, _chart.ViewXY.XAxes[0], yAxis)
                {
                    SamplingFrequency = samplingFrequency,
                    FirstSampleTimeStamp = 1.0 / samplingFrequency
                };
                sds.LineStyle.Color = yAxis.AxisColor;
                
               // sds.AllowUserInteraction = false;

                if (channel == 0)
                {
                    sds.Title.Text = "Left";
                }
                else if (channel == 1)
                {
                    sds.Title.Text = "Right";
                }
                else
                {
                    sds.Title.Text = "Ch " + (channel + 1).ToString();
                }

                sds.LineStyle.Width = 1;

                _chart.ViewXY.SampleDataSeries.Add(sds);
            }

            _chart.EndUpdate();
        }

        private void PrefillAudioDataToChart()
        {
            SignalReader.Marker[] markers = null;

            _signalReader.ReadAllData(_fileName,
                out _channelCount, out _samplingFrequency, out _audioData, out markers);
            freqLabel.Content = _samplingFrequency;
            _chart.BeginUpdate();

            
            InitChart(_channelCount, _samplingFrequency);

            for (int channel = 0; channel < _channelCount; channel++)
            {
                _chart.ViewXY.SampleDataSeries[channel].SamplesDouble = _audioData[channel];
            }

            _dataLength = (_audioData[0].Length - 1) / (double)_samplingFrequency;

            _chart.ViewXY.ZoomToFit();

            _chart.EndUpdate();
        }
       
        public void StartPlayback()
        {
            
            buttonStop.IsEnabled = true;
            buttonStart.IsEnabled = true;
            buttonOpen.IsEnabled = false;
            buttonOpen.IsEnabled = true;
            _audioOutput.IsEnabled = true;
            nameOfFile.Content = _fileName;
            //  _signalReader.OpenFile(_fileName);
            // buttonStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
          /*  if (buttonStart.IsPressed)
            {
                _signalReader.Start();
            }*/
        }

        private void SignalReader_DataGenerated(DataGeneratedEventArgs args)
        {
            Dispatcher.Invoke(
                new Action<object>(
                    delegate
                    {
                        
                        samplesCount = args.Samples[0].Length;

                      
                        _chart.BeginUpdate();
                        xTime = args.FirstSampleTimeStamp + samplesCount / (double) _samplingFrequency;
                        dPositionInSec = args.FirstSampleTimeStamp + samplesCount / (double)_samplingFrequency;
                        dPositionInSec %= _dataLength; 
                        dPositionInSec += currentTime;
                       // _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = dPositionInSec;
                        _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = sliderDuration.Value;
                        

                        _chart.EndUpdate();
                    }
                ),
                (object)null
            );
        }

        private void SignalReader_Started(StartedEventArgs args)
        {
            Dispatcher.Invoke(
                new System.Action(
                    delegate
                    {
                        _channelCount = _signalReader.ChannelCount;
                        _samplingFrequency = _signalReader.SamplingFrequency;
                    }
                )
            );
        }

        private void SignalReader_Stopped()
        {
            Dispatcher.Invoke(
                new System.Action(
                    delegate
                    {
                        _audioOutput.IsOutputEnabled = false;
                        if (_clean == true)
                        {
                            Dispose();
                        }
                    }
                )
            );
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            buttonOpen.IsEnabled = true;
            currentTime = 0;
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
            _audioOutput.IsOutputEnabled = false;
            nameOfFile.Content = _fileName;
            mediaPlayer.Position = new TimeSpan(0, 0, 0);
            time.Content = "00:00";
            sliderDuration.Value = 0;
            mediaPlayer.Stop();
            timer.Stop();
            _signalReader.StopRequest();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            //  currentTime = mediaPlayer.Position.TotalSeconds;
            //currentTime = sliderDuration.Value;
            //    sliderDuration.Value += mediaPlayer.Position.TotalSeconds;
            // _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = currentTime;
           
                _signalReader.Start();
            
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                durTest = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderDuration.Maximum = durTest;
            }
            
            
            //  _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = currentTime;
            mediaPlayer.Play();
            buttonPause.IsEnabled = true;
        
            timer.Start();
            
            StartPlayback();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() != true)
            {
                return;
            }
            buttonStart.IsEnabled = true;
            _fileName = _openFileDialog.FileName;
            nameOfFile.Content = _fileName;
            mediaPlayer.Open(new Uri(_fileName));
            if (System.IO.Path.GetExtension(_fileName) == ".sid")
            {
                _signalReader.Factor = 10.0;
            }
            else
            {
                _signalReader.Factor = 1.0;
            }

            PrefillAudioDataToChart();
            StartPlayback();
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            currentTime = 0;
            mediaPlayer.Pause();
            currentTime = mediaPlayer.Position.TotalSeconds;
            //dPositionInSec = currentTime;
         //   _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = currentTime;
           _signalReader.StopRequest();
            
           buttonStart.IsEnabled = true;
            buttonPause.IsEnabled = false;
          timer.Stop();
        }

        private void Slider_VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = volumeSlider.Value;
            }
        }

        private void sliderDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void renderNewSpeed()
        {
            _chart.ViewXY.XAxes[0].ValueType = AxisValueType.Time;
        }
        private void SliderSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.SpeedRatio = e.NewValue;
            curSoundSpeed = e.NewValue;
            // xTime = mediaPlayer.Position.TotalSeconds;
           // xTime += 20;
            //_chart.ViewXY.XAxes[0].DateTimeRange = xTime.ToString();
            
           // renderNewSpeed();
           
        }

        private void firstLoopValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(firstLoopValue.Text, out int firstLoopTime);
            if (checkBox1.IsChecked == true)
            {
                mediaPlayer.Position = new TimeSpan(0, 0, firstLoopTime);
            }
        }

        private void endLoopValue_TextChanged(object sender, TextChangedEventArgs e)
        {
          //  int.TryParse(endLoopValue.Text, out int endLoopTime);
            
        }

        private void SliderBalance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Balance = e.NewValue;
        }
    }
}
