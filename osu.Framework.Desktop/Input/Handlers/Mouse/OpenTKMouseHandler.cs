﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Drawing;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Platform;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Input;
using MouseState = osu.Framework.Input.MouseState;
using osu.Framework.Statistics;

namespace osu.Framework.Desktop.Input.Handlers.Mouse
{
    internal class OpenTKMouseHandler : InputHandler
    {
        private ScheduledDelegate scheduled;

        public override bool Initialize(GameHost host)
        {
            host.InputThread.Scheduler.Add(scheduled = new ScheduledDelegate(delegate
            {
                OpenTK.Input.MouseState state = OpenTK.Input.Mouse.GetCursorState();
                Point point = host.Window.PointToClient(new Point(state.X, state.Y));

                //todo: reimplement if necessary
                //Vector2 pos = Vector2.Multiply(point, Vector2.Divide(host.DrawSize, this.Size));

                Vector2 pos = new Vector2(point.X, point.Y);

                var tkState = new TkMouseState(state, pos, host.IsActive);

                PendingStates.Enqueue(new InputState { Mouse = tkState });

                FrameStatistics.Increment(StatisticsCounterType.MouseEvents);
            }, 0, 0));

            return true;
        }

        /// <summary>
        /// This input handler is always active, handling the cursor position if no other input handler does.
        /// </summary>
        public override bool IsActive => true;

        /// <summary>
        /// Lowest priority. We want the normal mouse handler to only kick in if all other handlers don't do anything.
        /// </summary>
        public override int Priority => 0;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            scheduled.Cancel();
        }

        private class TkMouseState : MouseState
        {
            public bool WasActive;

            public override int WheelDelta => WasActive ? base.WheelDelta : 0;

            public TkMouseState(OpenTK.Input.MouseState tkState, Vector2 position, bool active)
            {
                WasActive = active;

                if (active && tkState.IsAnyButtonDown)
                {
                    addIfPressed(tkState.LeftButton, MouseButton.Left);
                    addIfPressed(tkState.MiddleButton, MouseButton.Middle);
                    addIfPressed(tkState.RightButton, MouseButton.Right);
                    addIfPressed(tkState.XButton1, MouseButton.Button1);
                    addIfPressed(tkState.XButton2, MouseButton.Button2);
                }

                Wheel = tkState.Wheel;
                Position = position;
            }

            private void addIfPressed(ButtonState tkState, MouseButton button)
            {
                if (tkState == ButtonState.Pressed)
                    PressedButtons.Add(button);
            }
        }
    }
}