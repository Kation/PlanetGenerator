using Stride.Core.Presentation.Controls;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Valve.VR;

namespace PlanetGenerator.PlanetViewer
{
    public class StrideControl : Control, IDisposable
    {
        private GameForm _control;
        private GameEngineHost _host;
        private GameContext _gameContext;
        private Game _game;
        private bool _isRunning;
        private IntPtr _windowHandle;
        private TaskCompletionSource _gameTcs;
        private Thread _gameThread;
        private ContentPresenter _container;

        static StrideControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StrideControl), new FrameworkPropertyMetadata(typeof(StrideControl)));
        }

        public StrideControl()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += StrideControl_Loaded;
                Unloaded += StrideControl_Unloaded;
            }
            _game = new Game();
        }

        public Game Game => _game;

        private void StrideControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void StrideControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        public override void OnApplyTemplate()
        {
            _container = (ContentPresenter)GetTemplateChild("container");
        }

        public async Task Run()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(StrideControl));
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            if (_isRunning)
                return;
            _isRunning = true;

            _gameTcs = new TaskCompletionSource();
            _gameThread = new Thread(GameThread);
            _gameThread.IsBackground = true;
            _gameThread.Name = "Stride Thread";
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.Start();

            await _gameTcs.Task;
            _host = new GameEngineHost(_windowHandle);
            _container.Content = _host;
        }

        private void GameThread()
        {
            _control = new GameForm();
            _control.TopLevel = false;
            _control.Visible = false;
            _windowHandle = _control.Handle;

            _game.Script.Scheduler.Add(() =>
            {
                _game.Window.IsBorderLess = true;
                _gameTcs.SetResult();
                return Task.CompletedTask;
            });
            _gameContext = new GameContextWinforms(_control);
            _game.Run(_gameContext);
            _gameTcs.SetResult();
        }

        public async Task Stop()
        {
            if (_disposed && !_disposing)
                throw new ObjectDisposedException(nameof(StrideControl));
            _gameTcs = new TaskCompletionSource();
            _game.Exit();
            await _gameTcs.Task;
            _container.Content = null;
            _host.Dispose();
            _control.Dispose();
            _control = null;
        }

        public Task RunSceneScript(Action<Scene> action)
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            _game.Script.Scheduler.Add(() =>
            {
                action(_game.SceneSystem.SceneInstance.RootScene);
                tcs.SetResult();
                return Task.CompletedTask;
            });
            return tcs.Task;
        }

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    if (_control != null)
        //    {
        //        _control.Width = (int)sizeInfo.NewSize.Width;
        //        _control.Height = (int)sizeInfo.NewSize.Height;
        //    }
        //}

        private bool _disposing;
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposing = true;
            _disposed = true;
            if (_isRunning)
                Stop();
            _game.Dispose();
            _game = null;
            _disposing = false;
        }
    }
}
