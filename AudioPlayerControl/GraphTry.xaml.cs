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

namespace AudioPlayerControl
{
    /// <summary>
    /// Interaction logic for ExampleAudioOutputSignalReader.xaml
    /// </summary>
    public partial class GraphTry : UserControl, IDisposable
    {
        // Chart control for waveform data.
        private LightningChartUltimate _chart;

        // Waveform data.
        private double[][] _audioData;

        // Sampling frequency of wav file.
        private int _samplingFrequency;

        // Channel count of wav file.
        private int _channelCount;

        // File name of data.
        private string _fileName;

        // Data lenght in seconds.
        private double _dataLength = 0;
        // Open file dialog.
        private OpenFileDialog _openFileDialog;
        private SignalReader _signalReader;
        private AudioOutput _audioOutput;
        private bool _clean;

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

            _audioOutput = Resources["audioOutput"] as AudioOutput;
            _signalReader = _audioOutput.DataContext as SignalReader;

            CreateChart();
            PrefillAudioDataToChart();
            Start();
            Application.Current.MainWindow.Closing += ApplicationClosingDispose;
        }
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

        public void Start()
        {
            StartPlayback();
        }

        public void Stop()
        {
            _audioOutput.IsOutputEnabled = false;
            _signalReader.StopRequest();
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
            // Clear any gridChart's children.
            gridChart.Children.Clear();

            if (_chart != null)
            {
                // If a chart is already created, dispose it.
                _chart.Dispose();
                _chart = null;
            }

            // Create a new chart.
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

         //  _chart.ViewXY.ZoomPanOptions.WheelZooming = WheelZooming.Horizontal;

            //Set playback cursor 
            LineSeriesCursor cursor = new LineSeriesCursor(_chart.ViewXY, _chart.ViewXY.XAxes[0])
            {
                Style = CursorStyle.VerticalNoTracking
            };
            cursor.LineStyle.Color = Colors.White;
          //  cursor.AllowUserInteraction = false;
            cursor.ValueAtXAxis = 0;
            _chart.ViewXY.LineSeriesCursors.Add(cursor);

            _chart.EndUpdate();

            gridChart.Children.Add(_chart);

        }

        private void InitChart(int channelCount, int samplingFrequency)
        {
            _chart.BeginUpdate();

            //Remove existing series and axes
            _chart.ViewXY.SampleDataSeries.Clear(); // Remove existing series.
            _chart.ViewXY.YAxes.Clear();            // Remove existing y-axes.

            for (int channel = 0; channel < channelCount; channel++)
            {
                //Add Y axis for each channel
                AxisY yAxis = new AxisY(_chart.ViewXY)
                {
                    AxisColor = DefaultColors.SeriesForBlackBackgroundWpf[channel % DefaultColors.SeriesForBlackBackgroundWpf.Length]
                };
                _chart.ViewXY.YAxes.Add(yAxis);

                //Add series for each channel
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

            _chart.BeginUpdate();

            //Set series count and sampling frequency
            InitChart(_channelCount, _samplingFrequency);

            for (int channel = 0; channel < _channelCount; channel++)
            {
                _chart.ViewXY.SampleDataSeries[channel].SamplesDouble = _audioData[channel];
            }

            _dataLength = (_audioData[0].Length - 1) / (double)_samplingFrequency;

            _chart.ViewXY.ZoomToFit();

            _chart.EndUpdate();
        }

        private void StartPlayback()
        {
            buttonStop.IsEnabled = true;
            buttonStart.IsEnabled = false;
            buttonOpen.IsEnabled = false;
            _audioOutput.IsEnabled = true;

            _signalReader.OpenFile(_fileName);
            _signalReader.Start();
        }

        private void SignalReader_DataGenerated(DataGeneratedEventArgs args)
        {
            Dispatcher.Invoke(
                new Action<object>(
                    delegate
                    {
                        // New samples have been read from the the file.
                        int samplesCount = args.Samples[0].Length;

                        // Set the current position to chart's LineSeriesCursor.
                        _chart.BeginUpdate();

                        double dPositionInSec = args.FirstSampleTimeStamp + samplesCount / (double)_samplingFrequency;
                        dPositionInSec %= _dataLength; // Auto-looping needs to be handled by taking a modulo.

                        _chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis = dPositionInSec;

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
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
            _audioOutput.IsOutputEnabled = false;

            _signalReader.StopRequest();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            StartPlayback();
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() != true)
            {
                return;
            }

            _fileName = _openFileDialog.FileName;
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
    }
}
