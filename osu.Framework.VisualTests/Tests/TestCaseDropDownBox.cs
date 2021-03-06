﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Screens.Testing;

namespace osu.Framework.VisualTests.Tests
{
    internal class TestCaseDropDownBox : TestCase
    {
        public override string Description => @"Drop-down boxes";

        private StyledDropDownMenu styledDropDownMenu;

        public override void Reset()
        {
            base.Reset();
            string[] testItems = new string[10];
            int i = 0;
            while (i < 10)
                testItems[i] = @"test " + i++;
            styledDropDownMenu = new StyledDropDownMenu
            {
                Width = 150,
                Position = new Vector2(200, 70),
                Description = @"Drop-down menu",
                Depth = 1,
                Items = testItems.Select(item => new KeyValuePair<string, string>(item, item)),
                SelectedIndex = 4,
            };
            Add(styledDropDownMenu);

            AddButton("AddItem", () => styledDropDownMenu.AddDropDownItem(@"test " + i, @"test " + i++));
        }

        private class StyledDropDownMenu : DropDownMenu<string>
        {
            protected override DropDownHeader CreateHeader()
            {
                return new StyledDropDownHeader();
            }

            protected override DropDownMenuItem<string> CreateDropDownItem(string key, string value) => new StyledDropDownMenuItem(key);

            public StyledDropDownMenu()
            {
                Header.CornerRadius = 4;
                ContentContainer.CornerRadius = 4;
            }

            protected override void AnimateOpen()
            {
                ContentContainer.Show();
            }

            protected override void AnimateClose()
            {
                ContentContainer.Hide();
            }
        }

        private class StyledDropDownHeader : DropDownHeader
        {
            private SpriteText label;
            protected override string Label
            {
                get { return label.Text; }
                set { label.Text = value; }
            }

            public StyledDropDownHeader()
            {
                Foreground.Padding = new MarginPadding(4);
                BackgroundColour = new Color4(255, 255, 255, 100);
                BackgroundColourHover = Color4.HotPink;
                Children = new[]
                {
                    label = new SpriteText(),
                };
            }
        }

        private class StyledDropDownMenuItem : DropDownMenuItem<string>
        {
            public StyledDropDownMenuItem(string text) : base(text, text)
            {
                AutoSizeAxes = Axes.Y;
                Foreground.Padding = new MarginPadding(2);

                Children = new[]
                {
                    new SpriteText { Text = text },
                };
            }
        }
    }
}