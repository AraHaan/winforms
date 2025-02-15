﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms;

[Editor($"System.Windows.Forms.Design.BorderSidesEditor, {Assemblies.SystemDesign}", typeof(UITypeEditor))]
[Flags]
public enum ToolStripStatusLabelBorderSides
{
    All = Border3DSide.Top | Border3DSide.Bottom | Border3DSide.Left | Border3DSide.Right, // not mapped to Border3DSide.All because we NEVER want to fill the middle.
    Bottom = Border3DSide.Bottom,
    Left = Border3DSide.Left,
    Right = Border3DSide.Right,
    Top = Border3DSide.Top,
    None = 0
}
