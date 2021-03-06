using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.WinForms.WindowsDX {
    public sealed class D3DWindowBackend : DisposableBase, IWindowBackend {

        internal D3DWindowBackend() {
        }

        /// <summary>
        /// Transfers the new <see cref="SwapChainRenderTarget"/> to the editor service object after resizing a custom control.
        /// Make sure to subscribe to this event in your custom control to update your custom <see cref="RenderTarget2D"/>s
        /// according to the recent changes in the back buffer (width and height of back buffers).
        /// </summary>
        public event EventHandler<SwapChainUpdatedEventArgs> SwapChainUpdated;

        [CanBeNull]
        public SwapChainRenderTarget SwapChain => _chain;

        public void Initialize(GraphicsDeviceControl control) {
            if (control.IsDesignMode) {
                return;
            }

            var clientSize = control.ClientSize;

            var width = Math.Max(clientSize.Width, 1);
            var height = Math.Max(clientSize.Height, 1);

            _chain = new SwapChainRenderTarget(control.GraphicsDevice, control.Handle, width, height);
        }

        public void PrepareDraw(GraphicsDeviceControl control, PresentationParameters presentationParameters) {
            if (control.IsDesignMode) {
                return;
            }

            _presentationParameters = presentationParameters;
        }

        public void BeginDraw(GraphicsDeviceControl control) {
            if (control.IsDesignMode) {
                return;
            }

            var graphicsDevice = control.GraphicsDevice;

            graphicsDevice?.SetRenderTarget(_chain);
        }

        public void EndDraw(GraphicsDeviceControl control) {
            if (control.IsDesignMode) {
                return;
            }

            if (_chain == null) {
                throw new InvalidOperationException("A swap chain is not created for this backend.");
            }

            if (_presentationParameters == null) {
                throw new InvalidOperationException("The drawing process is not started. You should call PrepareDraw() and BeginDraw() first.");
            }

            try {
                _chain.PresentInterval = _presentationParameters.PresentationInterval;
                _chain.Present();
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
            }
        }

        public void OnWindowSizeChanged(GraphicsDeviceControl control) {
            var clientSize = control.ClientSize;

            if (clientSize.Width <= 0 || clientSize.Height <= 0) {
                return;
            }

            _chain?.Dispose();

            var graphicsDevice = control.GraphicsDevice;

            if (graphicsDevice != null) {
                _chain = new SwapChainRenderTarget(graphicsDevice, control.Handle, clientSize.Width, clientSize.Height);

                graphicsDevice.PresentationParameters.BackBufferWidth = clientSize.Width;
                graphicsDevice.PresentationParameters.BackBufferHeight = clientSize.Height;

                graphicsDevice.Viewport = new Viewport(0, 0, clientSize.Width, clientSize.Height, 0, 1);
            }

            if (_chain != null) {
                SwapChainUpdated?.Invoke(this, new SwapChainUpdatedEventArgs(_chain));
            }
        }

        protected override void Dispose(bool disposing) {
            _chain?.Dispose();
            _chain = null;
        }

        [CanBeNull]
        private PresentationParameters _presentationParameters;
        [CanBeNull]
        private SwapChainRenderTarget _chain;

    }
}
