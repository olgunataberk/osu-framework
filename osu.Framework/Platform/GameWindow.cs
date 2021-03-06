﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using OpenTK;
using OpenTK.Graphics.ES30;

namespace osu.Framework.Platform
{
    public abstract class GameWindow : OpenTK.GameWindow
    {
        internal Version GLVersion;
        internal Version GLSLVersion;

        protected GameWindow(int width, int height) : base(width, height)
        {
            Closing += (sender, e) => e.Cancel = ExitRequested?.Invoke() ?? false;
            Closed += (sender, e) => Exited?.Invoke();

            MakeCurrent();

            string version = GL.GetString(StringName.Version);
            string versionNumberSubstring = getVersionNumberSubstring(version);
            GLVersion = new Version(versionNumberSubstring);
            version = GL.GetString(StringName.ShadingLanguageVersion);
            if (!string.IsNullOrEmpty(version))
            {
                try
                {
                    GLSLVersion = new Version(versionNumberSubstring);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"couldn't set GLSL version using string '{version}'");
                }
            }

            if (GLSLVersion == null)
                GLSLVersion = new Version();

            //Set up OpenGL related characteristics
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);

            Logger.Log($@"GL Initialized
                        GL Version:                 {GL.GetString(StringName.Version)}
                        GL Renderer:                {GL.GetString(StringName.Renderer)}
                        GL Shader Language version: {GL.GetString(StringName.ShadingLanguageVersion)}
                        GL Vendor:                  {GL.GetString(StringName.Vendor)}
                        GL Extensions:              {GL.GetString(StringName.Extensions)}", LoggingTarget.Runtime, LogLevel.Important);

            Context.MakeCurrent(null);
        }

        private CursorState cursorState = CursorState.Visible;

        /// <summary>
        /// Controls the state of the OS cursor.
        /// </summary>
        public CursorState CursorState
        {
            get { return cursorState; }
            set
            {
                cursorState = value;
                switch (cursorState)
                {
                    case CursorState.Visible:
                        base.CursorVisible = true;
                        base.Cursor = MouseCursor.Default;
                        break;
                    case CursorState.Hidden:
                        base.CursorVisible = true;
                        base.Cursor = MouseCursor.Empty;
                        break;
                    case CursorState.HiddenAndConfined:
                        base.CursorVisible = false;
                        base.Cursor = MouseCursor.Empty;
                        break;
                }
            }
        }

        /// <summary>
        /// We do not support directly using <see cref="Cursor"/>.
        /// It is controlled internally. Use <see cref="CursorState"/> instead.
        /// </summary>
        public new bool Cursor
        {
            get { throw new InvalidOperationException($@"{nameof(Cursor)} is not supported. Use {nameof(CursorState)}."); }
            set { throw new InvalidOperationException($@"{nameof(Cursor)} is not supported. Use {nameof(CursorState)}."); }
        }

        /// <summary>
        /// We do not support directly using <see cref="CursorVisible"/>.
        /// It is controlled internally. Use <see cref="CursorState"/> instead.
        /// </summary>
        public new bool CursorVisible
        {
            get { throw new InvalidOperationException($@"{nameof(CursorVisible)} is not supported. Use {nameof(CursorState)}."); }
            set { throw new InvalidOperationException($@"{nameof(CursorVisible)} is not supported. Use {nameof(CursorState)}."); }
        }

        private string getVersionNumberSubstring(string version)
        {
            string result = version.Split(' ').FirstOrDefault(s => char.IsDigit(s, 0));
            if (result != null) return result;
            throw new ArgumentException(nameof(version));
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public abstract void SetupWindow(FrameworkConfigManager config);

        /// <summary>
        /// Return value decides whether we should intercept and cancel this exit (if possible).
        /// </summary>
        public event Func<bool> ExitRequested;

        public event Action Exited;

        protected void OnExited() => Exited?.Invoke();

        protected bool OnExitRequested() => ExitRequested?.Invoke() ?? false;

        public virtual Vector2 Position { get; set; }

        public virtual void CycleMode() {}
    }

    /// <summary>
    /// Describes our supported states of the OS cursor.
    /// </summary>
    public enum CursorState
    {
        /// <summary>
        /// The OS cursor is always visible and can move anywhere.
        /// </summary>
        Visible,
        /// <summary>
        /// The OS cursor is hidden while hovering the <see cref="GameWindow"/>, but can still move anywhere.
        /// </summary>
        Hidden,
        /// <summary>
        /// The OS cursor is hidden while hovering the <see cref="GameWindow"/>.
        /// It is confined to the <see cref="GameWindow"/> while the window is in focus and can move freely otherwise.
        /// </summary>
        HiddenAndConfined,
    }
}
