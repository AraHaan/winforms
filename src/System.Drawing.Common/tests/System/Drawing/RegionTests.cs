﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Copyright (C) 2004-2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing.Tests;

public class RegionTests
{
    private static readonly Graphics s_graphic = Graphics.FromImage(new Bitmap(1, 1));

    private static Region CreateDisposedRegion()
    {
        Region region = new();
        region.Dispose();
        return region;
    }

    [Fact]
    public void Ctor_Default()
    {
        using Region region = new();
        Assert.False(region.IsEmpty(s_graphic));
        Assert.True(region.IsInfinite(s_graphic));
        Assert.Equal(new RectangleF(-4194304, -4194304, 8388608, 8388608), region.GetBounds(s_graphic));
    }

    [Theory]
    [InlineData(-1, -2, -3, -4, true)]
    [InlineData(0, 0, 0, 0, true)]
    [InlineData(1, 2, 3, 4, false)]
    public void Ctor_Rectangle(int x, int y, int width, int height, bool isEmpty)
    {
        Rectangle rectangle = new(x, y, width, height);

        using Region region = new(rectangle);
        Assert.Equal(isEmpty, region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal(new RectangleF(x, y, width, height), region.GetBounds(s_graphic));
    }

    [Theory]
    [InlineData(1, 2, 3, float.NegativeInfinity, true)]
    [InlineData(-1, -2, -3, -4, true)]
    [InlineData(0, 0, 0, 0, true)]
    [InlineData(1, 2, 3, 4, false)]
    [InlineData(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, true)]
    public void Ctor_RectangleF(float x, float y, float width, float height, bool isEmpty)
    {
        RectangleF rectangle = new(x, y, width, height);

        using Region region = new(rectangle);
        Assert.Equal(isEmpty, region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal(rectangle, region.GetBounds(s_graphic));
    }

    public static IEnumerable<object[]> Region_TestData()
    {
        yield return new object[] { new Region() };
        yield return new object[] { new Region(new Rectangle(0, 0, 0, 0)) };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)) };
    }

    [Theory]
    [MemberData(nameof(Region_TestData))]
    public void Ctor_RegionData(Region region)
    {
        using (region)
        {
            using Region otherRegion = new(region.GetRegionData());
            using Matrix matrix = new();
            Assert.Equal(region.GetBounds(s_graphic), otherRegion.GetBounds(s_graphic));
            Assert.Equal(region.GetRegionScans(matrix), otherRegion.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Ctor_RegionDataOfRegionWithPath_Success()
    {
        using GraphicsPath graphicsPath = new();
        graphicsPath.AddRectangle(new Rectangle(1, 2, 3, 4));

        using Region region = new(graphicsPath);
        Ctor_RegionData(region);
    }

    [Fact]
    public void Ctor_RegionDataOfRegionWithRegionData_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Region other = new(region.GetRegionData());
        Ctor_RegionData(other);
    }

    [Fact]
    public void Ctor_NullRegionData_ThrowsArgumentNullException()
    {
        AssertExtensions.Throws<ArgumentNullException>("rgnData", () => new Region((RegionData)null));
    }

    [Fact]
    public void Ctor_EmptyRegionData_ThrowsArgumentException()
    {
        using Region region = new();
        RegionData regionData = region.GetRegionData();
        regionData.Data = [];
        AssertExtensions.Throws<ArgumentException>(null, () => new Region(regionData));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(256)]
    public void Ctor_InvalidRegionData_ThrowsExternalException(int dataLength)
    {
        using Region region = new();
        RegionData regionData = region.GetRegionData();
        regionData.Data = new byte[dataLength];
        Assert.Throws<ExternalException>(() => new Region(regionData));
    }

    [Fact]
    public void Ctor_EmptyGraphicsPath_ThrowsExternalException()
    {
        using GraphicsPath graphicsPath = new();
        using Region region = new(graphicsPath);
        RegionData regionData = region.GetRegionData();
        Assert.Throws<ExternalException>(() => new Region(regionData));
    }

    [Fact]
    public void Ctor_NullDataInRegionData_ThrowsNullReferenceException()
    {
        using Region region = new();
        RegionData regionData = region.GetRegionData();
        regionData.Data = null;
        Assert.Throws<NullReferenceException>(() => new Region(regionData));
    }

    [Fact]
    public void Ctor_GraphicsPath()
    {
        using GraphicsPath graphicsPath = new();
        graphicsPath.AddRectangle(new Rectangle(1, 2, 3, 4));
        graphicsPath.AddRectangle(new Rectangle(4, 5, 6, 7));

        using Region region = new(graphicsPath);
        using Matrix matrix = new();
        Assert.Equal(
        [
                    new(1, 2, 3, 3),
                    new(1, 5, 9, 1),
                    new(4, 6, 6, 6)
        ], region.GetRegionScans(matrix));
        Assert.Equal(new RectangleF(1, 2, 9, 10), region.GetBounds(s_graphic));
    }

    [Fact]
    public void Ctor_EmptyGraphicsPath()
    {
        using GraphicsPath graphicsPath = new();
        using Region region = new(graphicsPath);
        using Matrix matrix = new();
        Assert.True(region.IsEmpty(s_graphic));
        Assert.Empty(region.GetRegionScans(matrix));
    }

    public static IEnumerable<object[]> Ctor_InfiniteGraphicsPath_TestData()
    {
        GraphicsPath path1 = new();
        path1.AddRectangle(new Rectangle(-4194304, -4194304, 8388608, 8388608));
        yield return new object[] { path1, true };

        GraphicsPath path2 = new();
        path2.AddRectangle(new Rectangle(-4194304, -4194304, 8388608, 8388608));
        path2.AddRectangle(Rectangle.Empty);
        yield return new object[] { path2, true };

        GraphicsPath path3 = new();
        path3.AddRectangle(new Rectangle(-4194304, -4194304, 8388608, 8388608));
        path3.AddRectangle(new Rectangle(1, 2, 3, 4));
        yield return new object[] { path3, false };

        GraphicsPath path4 = new();
        path4.AddCurve([new(-4194304, -4194304), new(4194304, 4194304)]);
        yield return new object[] { path4, false };

        GraphicsPath path5 = new();
        path5.AddPolygon([new(-4194304, -4194304), new(-4194304, 4194304), new(4194304, 4194304), new(4194304, -4194304)]);
        yield return new object[] { path5, true };

        GraphicsPath path6 = new();
        path6.AddPolygon([new(-4194304, -4194304), new(-4194304, 4194304), new(4194304, 4194304), new(4194304, -4194304), new(-4194304, -4194304)]);
        yield return new object[] { path6, true };
    }

    [Theory]
    [MemberData(nameof(Ctor_InfiniteGraphicsPath_TestData))]
    public void Ctor_InfiniteGraphicsPath_IsInfinite(GraphicsPath path, bool isInfinite)
    {
        using (path)
        using (Region region = new(path))
        {
            Assert.Equal(isInfinite, region.IsInfinite(s_graphic));
        }
    }

    [Fact]
    public void Ctor_GraphicsPathTooLarge_SetsToEmpty()
    {
        using GraphicsPath path = new();
        path.AddCurve([new(-4194304, -4194304), new(4194304, 4194304)]);

        using Region region = new(path);
        using Matrix matrix = new();
        Assert.Empty(region.GetRegionScans(matrix));
    }

    [Fact]
    public void Ctor_NullGraphicsPath_ThrowsArgumentNullException()
    {
        AssertExtensions.Throws<ArgumentNullException>("path", () => new Region((GraphicsPath)null));
    }

    [Fact]
    public void Ctor_DisposedGraphicsPath_ThrowsArgumentException()
    {
        GraphicsPath path = new();
        path.Dispose();

        AssertExtensions.Throws<ArgumentException>(null, () => new Region(path));
    }

    [Theory]
    [MemberData(nameof(Region_TestData))]
    public void Clone(Region region)
    {
        using (region)
        using (Region clone = Assert.IsType<Region>(region.Clone()))
        using (Matrix matrix = new())
        {
            Assert.NotSame(region, clone);

            Assert.Equal(region.GetBounds(s_graphic), clone.GetBounds(s_graphic));
            Assert.Equal(region.GetRegionScans(matrix), clone.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Clone_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().Clone());
    }

    public static IEnumerable<object[]> Complement_TestData()
    {
        yield return new object[]
        {
            new Region(new RectangleF(10, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[] { new(110, 60, 30, 20) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(70, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[] { new(40, 60, 30, 20) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(40, 100, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[] { new(70, 80, 50, 20) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(40, 10, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[] { new(70, 110, 50, 10) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(30, 30, 80, 80)),
            new RectangleF[]
            {
                new(45, 45, 200, 200),
                new(160, 260, 10, 10),
                new(170, 260, 10, 10),
            },
            new RectangleF[] { new(170, 260, 10, 10) }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { RectangleF.Empty },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { new(1, 2, 3, 4) },
            Array.Empty<RectangleF>()
        };
    }

    [Theory]
    [MemberData(nameof(Complement_TestData))]
    public void Complement_Region_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using Region other = new(rect);
                region.Complement(other);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Complement_UnionRegion_Success()
    {
        using Region region = new(new Rectangle(20, 20, 20, 20));
        using Region other = new(new Rectangle(20, 80, 20, 10));
        using Matrix matrix = new();
        other.Union(new Rectangle(60, 60, 30, 10));

        region.Complement(other);
        Assert.Equal(
        [
                new(60, 60, 30, 10),
                new(20, 80, 20, 10)
        ], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Complement_InfiniteAndWithIntersectRegion_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Intersect(new Rectangle(5, 5, -10, -10));
        region.Complement(new Rectangle(-5, -5, 12, 12));

        Assert.False(region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal(
        [
                new(5, -5, 2, 10),
                new(-5, 5, 12, 2)
        ], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Complement_InfiniteRegion_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Matrix matrix = new();
        using Region other = new();
        region.Complement(other);

        Assert.Equal(
        [
                new(-4194304, -4194304, 8388608, 4194306),
                new(-4194304, 2, 4194305, 4),
                new(4, 2, 4194300, 4),
                new(-4194304, 6, 8388608, 4194298)
        ], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Complement_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Complement((Region)null));
    }

    [Fact]
    public void Complement_DisposedRegion_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => new Region().Complement(CreateDisposedRegion()));
    }

    [Fact]
    public void Complement_SameRegion_ThrowsInvalidOperationException()
    {
        using Region region = new();
        Assert.Throws<InvalidOperationException>(() => region.Complement(region));
    }

    [Theory]
    [MemberData(nameof(Complement_TestData))]
    public void Complement_Rectangle_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Complement(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Complement_TestData))]
    public void Complement_RectangleF_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Complement(rect);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Complement_TestData))]
    public void Complement_GraphicsPath_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                region.Complement(path);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Complement_GraphicsPathWithMultipleRectangles_Success()
    {
        Rectangle rect1 = new(20, 30, 60, 80);
        Rectangle rect2 = new(50, 40, 60, 80);

        using Graphics graphics = Graphics.FromImage(new Bitmap(600, 800));
        using Region region1 = new(rect1);
        using Region region2 = new(rect2);
        using Matrix matrix = new();
        graphics.DrawRectangle(Pens.Green, rect1);
        graphics.DrawRectangle(Pens.Red, rect2);

        region1.Complement(region2);
        graphics.FillRegion(Brushes.Blue, region1);
        graphics.DrawRectangles(Pens.Yellow, region1.GetRegionScans(matrix));

        Assert.Equal(
        [
                new(80, 40, 30, 70),
                new(50, 110, 60, 10)
        ], region1.GetRegionScans(matrix));
    }

    [Fact]
    public void Complement_EmptyPathWithInfiniteRegion_MakesEmpty()
    {
        using Region region = new();
        using GraphicsPath graphicsPath = new();
        region.Complement(graphicsPath);
        Assert.True(region.IsEmpty(s_graphic));
    }

    [Fact]
    public void Complement_NullGraphicsPath_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("path", () => region.Complement((GraphicsPath)null));
    }

    [Fact]
    public void Complement_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        using GraphicsPath graphicPath = new();
        using Region other = new();
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Complement(graphicPath));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Complement(default(Rectangle)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Complement(default(RectangleF)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Complement(disposedRegion));
    }

    public static IEnumerable<object[]> Equals_TestData()
    {
        static Region Empty()
        {
            Region emptyRegion = new();
            emptyRegion.MakeEmpty();
            return emptyRegion;
        }

        Region createdRegion = new();
        yield return new object[] { createdRegion, createdRegion, true };
        yield return new object[] { new Region(), new Region(), true };
        yield return new object[] { new Region(), Empty(), false };
        yield return new object[] { new Region(), new Region(new Rectangle(1, 2, 3, 4)), false };

        yield return new object[] { Empty(), Empty(), true };
        yield return new object[] { Empty(), new Region(new Rectangle(0, 0, 0, 0)), true };
        yield return new object[] { Empty(), new Region(new Rectangle(1, 2, 3, 3)), false };

        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new Rectangle(1, 2, 3, 4)), true };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new RectangleF(1, 2, 3, 4)), true };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new Rectangle(2, 2, 3, 4)), false };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new Rectangle(1, 3, 3, 4)), false };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new Rectangle(1, 2, 4, 4)), false };
        yield return new object[] { new Region(new Rectangle(1, 2, 3, 4)), new Region(new Rectangle(1, 2, 3, 5)), false };

        GraphicsPath graphics1 = new();
        graphics1.AddRectangle(new Rectangle(1, 2, 3, 4));

        GraphicsPath graphics2 = new();
        graphics2.AddRectangle(new Rectangle(1, 2, 3, 4));

        GraphicsPath graphics3 = new();
        graphics3.AddRectangle(new Rectangle(2, 2, 3, 4));

        GraphicsPath graphics4 = new();
        graphics4.AddRectangle(new Rectangle(1, 3, 3, 4));

        GraphicsPath graphics5 = new();
        graphics5.AddRectangle(new Rectangle(1, 2, 4, 4));

        GraphicsPath graphics6 = new();
        graphics6.AddRectangle(new Rectangle(1, 2, 3, 5));

        yield return new object[] { new Region(graphics1), new Region(graphics1), true };
        yield return new object[] { new Region(graphics1), new Region(graphics2), true };
        yield return new object[] { new Region(graphics1), new Region(graphics3), false };
        yield return new object[] { new Region(graphics1), new Region(graphics4), false };
        yield return new object[] { new Region(graphics1), new Region(graphics5), false };
        yield return new object[] { new Region(graphics1), new Region(graphics6), false };
    }

    [Theory]
    [MemberData(nameof(Equals_TestData))]
    public void Equals_Valid_ReturnsExpected(Region region, Region other, bool expected)
    {
        using (region)
        using (other)
        {
            Assert.Equal(expected, region.Equals(other, s_graphic));
        }
    }

    [Fact]
    public void Equals_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Equals(null, s_graphic));
    }

    [Fact]
    public void Equals_NullGraphics_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("g", () => region.Equals(region, null));
    }

    [Fact]
    public void Equals_DisposedGraphics_ThrowsArgumentException()
    {
        using Region region = new();
        using Region other = new();
        using Bitmap image = new(10, 10);
        var graphics = Graphics.FromImage(image);
        graphics.Dispose();
        AssertExtensions.Throws<ArgumentException>(null, () => region.Equals(region, graphics));
    }

    [Fact]
    public void Equals_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Equals(new Region(), s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => new Region().Equals(disposedRegion, s_graphic));
    }

    public static IEnumerable<object[]> Exclude_TestData()
    {
        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { new(500, 30, 60, 80) },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new(500, 30, 60, 80) }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { new(520, 40, 60, 80) },
            new RectangleF[]
            {
                new(-4194304, -4194304, 8388608, 4194344),
                new(-4194304, 40, 4194824, 80),
                new(580, 40, 4193724, 80),
                new(-4194304, 120, 8388608, 4194184)
            }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new Rectangle(-4194304, -4194304, 8388608, 8388608) }
        };

        // Intersecting from the right.
        yield return new object[]
        {
            new Region(new Rectangle(10, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[]
            {
                new(10, 10, 100, 50),
                new(10, 60, 30, 20),
                new(10, 80, 100, 30)
            }
        };

        // Intersecting from the left.
        yield return new object[]
        {
            new Region(new Rectangle(70, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[]
            {
                new(70, 10, 100, 50),
                new(140, 60, 30, 20),
                new(70, 80, 100, 30)
            }
        };

        // Intersecting from the top.
        yield return new object[]
        {
            new Region(new Rectangle(40, 100, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[]
            {
                new(40, 100, 30, 20),
                new(120, 100, 20, 20),
                new(40, 120, 100, 80)
            }
        };

        // Intersecting from the bottom.
        yield return new object[]
        {
            new Region(new Rectangle(40, 10, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[]
            {
                new(40, 10, 100, 70),
                new(40, 80, 30, 30),
                new(120, 80, 20, 30)
            }
        };

        // Multiple regions.
        yield return new object[]
        {
            new Region(new Rectangle(30, 30, 80, 80)),
            new RectangleF[]
            {
                new(45, 45, 200, 200),
                new(160, 260, 10, 10),
                new(170, 260, 10, 10)
            },
            new RectangleF[]
            {
                new(30, 30, 80, 15),
                new(30, 45, 15, 65)
            }
        };

        // Intersecting from the top with a larger rect.
        yield return new object[]
        {
            new Region(new Rectangle(50, 100, 100, 100)),
            new RectangleF[] { new(30, 70, 150, 40) },
            new RectangleF[] { new(50, 110, 100, 90) }
        };

        // Intersecting from the right with a larger rect.
        yield return new object[]
        {
            new Region(new Rectangle(70, 60, 100, 70)),
            new RectangleF[] { new(40, 10, 100, 150) },
            new RectangleF[] { new(140, 60, 30, 70) }
        };

        // Intersecting from the left with a larger rect.
        yield return new object[]
        {
            new Region(new Rectangle(70, 60, 100, 70)),
            new RectangleF[] { new(100, 10, 100, 150) },
            new RectangleF[] { new(70, 60, 30, 70) }
        };

        // Intersecting from the bottom with a larger rect.
        yield return new object[]
        {
            new Region(new Rectangle(20, 20, 100, 100)),
            new RectangleF[] { new(10, 80, 140, 150) },
            new RectangleF[] { new(20, 20, 100, 60) }
        };

        yield return new object[]
        {
            new Region(new Rectangle(130, 30, 60, 80)),
            new RectangleF[] { new(170, 40, 60, 80) },
            new RectangleF[]
            {
                new(130, 30, 60, 10),
                new(130, 40, 40, 70)
            }
        };
    }

    [Theory]
    [MemberData(nameof(Exclude_TestData))]
    public void Exclude_Region_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using Region other = new(rect);
                region.Exclude(other);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Exclude_UnionRegion_Success()
    {
        using Region region = new(new RectangleF(20, 20, 20, 20));
        using Region union = new(new RectangleF(20, 80, 20, 10));
        using Matrix matrix = new();
        union.Union(new RectangleF(60, 60, 30, 10));
        region.Exclude(union);
        Assert.Equal([new(20, 20, 20, 20)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Exclude_InfiniteRegion_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Region other = new();
        using Matrix matrix = new();
        region.Exclude(other);
        Assert.Equal([], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Exclude_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Exclude((Region)null));
    }

    [Fact]
    public void Exclude_DisposedRegion_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => new Region().Exclude(CreateDisposedRegion()));
    }

    [Fact]
    public void Exclude_SameRegion_ThrowsInvalidOperationException()
    {
        using Region region = new();
        Assert.Throws<InvalidOperationException>(() => region.Exclude(region));
    }

    [Theory]
    [MemberData(nameof(Exclude_TestData))]
    public void Exclude_Rectangle_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Exclude(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Exclude_TestData))]
    public void Exclude_RectangleF_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Exclude(rect);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Exclude_TestData))]
    public void Exclude_GraphicsPath_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                region.Exclude(path);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Exclude_EmptyPathWithInfiniteRegion_MakesInfinite()
    {
        using Region region = new();
        using GraphicsPath graphicsPath = new();
        region.Exclude(graphicsPath);
        Assert.True(region.IsInfinite(s_graphic));
    }

    [Fact]
    public void Exclude_NullGraphicsPath_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("path", () => region.Exclude((GraphicsPath)null));
    }

    [Fact]
    public void Exclude_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        using GraphicsPath graphicsPath = new();
        using Region other = new();
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Exclude(graphicsPath));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Exclude(default(Rectangle)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Exclude(default(RectangleF)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Exclude(other));
    }

    [Fact]
    public void FromHrgn_ValidHrgn_ReturnsExpected()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        IntPtr handle1 = region.GetHrgn(s_graphic);
        IntPtr handle2 = region.GetHrgn(s_graphic);
        Assert.NotEqual(IntPtr.Zero, handle1);
        Assert.NotEqual(handle1, handle2);

        Region newRegion = Region.FromHrgn(handle1);
        IntPtr handle3 = newRegion.GetHrgn(s_graphic);
        Assert.NotEqual(handle3, handle1);
        Assert.Equal(new RectangleF(1, 2, 3, 4), newRegion.GetBounds(s_graphic));

        region.ReleaseHrgn(handle1);
        region.ReleaseHrgn(handle2);
        newRegion.ReleaseHrgn(handle3);
    }

    [Fact]
    public void FromHrgn_ZeroHrgn_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => Region.FromHrgn(IntPtr.Zero));
    }

    [Fact]
    public void GetHrgn_Infinite_ReturnsZero()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        IntPtr handle = region.GetHrgn(s_graphic);
        Assert.NotEqual(IntPtr.Zero, handle);
        region.ReleaseHrgn(handle);

        region.MakeInfinite();
        Assert.Equal(IntPtr.Zero, region.GetHrgn(s_graphic));
    }

    [Fact]
    public void GetHrgn_Empty_ReturnsNonZero()
    {
        using Region region = new();
        Assert.Equal(IntPtr.Zero, region.GetHrgn(s_graphic));

        region.MakeEmpty();
        IntPtr handle = region.GetHrgn(s_graphic);
        Assert.NotEqual(IntPtr.Zero, handle);
        region.ReleaseHrgn(handle);
    }

    [Fact]
    public void GetHrgn_NullGraphics_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("g", () => region.GetHrgn(null));
    }

    [Fact]
    public void GetHrgn_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().GetHrgn(s_graphic));
    }

    [Fact]
    public void ReleaseHrgn_ZeroHandle_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("regionHandle", () => region.ReleaseHrgn(IntPtr.Zero));
    }

    [Fact]
    public void GetBounds_NullGraphics_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("g", () => region.GetBounds(null));
    }

    [Fact]
    public void GetBounds_DisposedGraphics_ThrowsArgumentException()
    {
        using Region region = new();
        using Bitmap image = new(10, 10);
        var graphics = Graphics.FromImage(image);
        graphics.Dispose();
        AssertExtensions.Throws<ArgumentException>(null, () => region.GetBounds(graphics));
    }

    [Fact]
    public void GetBounds_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().GetBounds(s_graphic));
    }

    [Fact]
    public void GetRegionData_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().GetRegionData());
    }

    [Fact]
    public void GetRegionScans_CustomMatrix_TransformsRegionScans()
    {
        using Matrix matrix = new();
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Matrix emptyMatrix = new();
        matrix.Translate(10, 11);
        matrix.Scale(5, 6);

        Assert.Equal([new(1, 2, 3, 4)], region.GetRegionScans(emptyMatrix));
        Assert.Equal([new(15, 23, 15, 24)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void GetRegionScans_NullMatrix_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("matrix", () => region.GetRegionScans(null));
    }

    [Fact]
    public void GetRegionScans_Disposed_ThrowsArgumentException()
    {
        using Matrix matrix = new();
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().GetRegionScans(matrix));
    }

    [Fact]
    public void GetRegionScans_DisposedMatrix_ThrowsArgumentException()
    {
        using Region region = new();
        Matrix matrix = new();
        matrix.Dispose();
        AssertExtensions.Throws<ArgumentException>(null, () => region.GetRegionScans(matrix));
    }

    [Fact]
    public void Intersect_SmallerRect_Success()
    {
        using Region clipRegion = new();
        using Matrix matrix = new();
        Rectangle smaller = new(5, 5, -10, -10);

        clipRegion.Intersect(smaller);
        Assert.False(clipRegion.IsEmpty(s_graphic));
        Assert.False(clipRegion.IsInfinite(s_graphic));

        RectangleF[] rects = clipRegion.GetRegionScans(matrix);
        Assert.Single(rects);
        Assert.Equal(new RectangleF(-5, -5, 10, 10), rects[0]);
    }

    public static IEnumerable<object[]> Intersect_TestData()
    {
        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { new(500, 30, 60, 80) },
            new RectangleF[] { new(500, 30, 60, 80) }
        };
        yield return new object[]
        {
            new Region(new Rectangle(0, 0, 0, 0)),
            new RectangleF[] { new(500, 30, 60, 80) },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { RectangleF.Empty },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { new(520, 40, 60, 80) },
            new RectangleF[] { new Rectangle(520, 40, 60, 80) }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { RectangleF.Empty },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(new RectangleF(260, 30, 60, 80)),
            new RectangleF[] { new(290, 40, 60, 90) },
            new RectangleF[] { new(290, 40, 30, 70) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(20, 330, 40, 50)),
            new RectangleF[]
            {
                new(50, 340, 40, 50),
                new(70, 360, 30, 50),
                new(80, 400, 30, 10)
            },
            Array.Empty<RectangleF>()
        };
    }

    [Theory]
    [MemberData(nameof(Intersect_TestData))]
    public void Intersect_Region_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using Region rectangleRegion = new(rect);
                region.Intersect(rectangleRegion);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Intersect_InfiniteRegion_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Matrix matrix = new();
        using Region infiniteRegion = new();
        region.Intersect(infiniteRegion);

        Assert.Equal([new Rectangle(1, 2, 3, 4)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Intersect_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Intersect((Region)null));
    }

    [Fact]
    public void Intersect_DisposedRegion_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => new Region().Intersect(CreateDisposedRegion()));
    }

    [Fact]
    public void Intersect_SameRegion_ThrowsInvalidOperationException()
    {
        using Region region = new();
        Assert.Throws<InvalidOperationException>(() => region.Intersect(region));
    }

    [Theory]
    [MemberData(nameof(Intersect_TestData))]
    public void Intersect_Rectangle_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Intersect(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Intersect_InfiniteRegionWithSmallerRectangle_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Intersect(new Rectangle(5, 5, -10, -10));

        Assert.False(region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(-5, -5, 10, 10)], region.GetRegionScans(matrix));
    }

    [Theory]
    [MemberData(nameof(Intersect_TestData))]
    public void Intersect_RectangleF_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Intersect(rect);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Intersect_InfiniteRegionWithSmallerRectangleF_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Intersect(new RectangleF(5, 5, -10, -10));

        Assert.False(region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(-5, -5, 10, 10)], region.GetRegionScans(matrix));
    }

    [Theory]
    [MemberData(nameof(Intersect_TestData))]
    public void Intersect_GraphicsPath_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                region.Intersect(path);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Intersect_EmptyPathWithInfiniteRegion_MakesEmpty()
    {
        using Region region = new();
        using GraphicsPath graphicsPath = new();
        region.Intersect(graphicsPath);
        Assert.True(region.IsEmpty(s_graphic));
    }

    [Fact]
    public void Intersect_NullGraphicsPath_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("path", () => region.Intersect((GraphicsPath)null));
    }

    [Fact]
    public void Intersect_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        using GraphicsPath graphicsPath = new();
        using Region other = new();
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Intersect(graphicsPath));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Intersect(default(Rectangle)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Intersect(default(RectangleF)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Intersect(other));
    }

    [Fact]
    public void IsEmpty_NullGraphics_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("g", () => region.IsEmpty(null));
    }

    [Fact]
    public void IsEmpty_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().IsEmpty(s_graphic));
    }

    [Fact]
    public void IsInfinite_NullGraphics_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("g", () => region.IsInfinite(null));
    }

    [Fact]
    public void IsInfinite_DisposedGraphics_ThrowsArgumentException()
    {
        using Region region = new();
        using Bitmap image = new(10, 10);
        var graphics = Graphics.FromImage(image);
        graphics.Dispose();
        AssertExtensions.Throws<ArgumentException>(null, () => region.IsInfinite(graphics));
    }

    [Fact]
    public void IsInfinite_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().IsInfinite(s_graphic));
    }

    public static IEnumerable<object[]> IsVisible_Rectangle_TestData()
    {
        Region infiniteExclude = new();
        infiniteExclude.Exclude(new Rectangle(387, 292, 189, 133));
        infiniteExclude.Exclude(new Rectangle(387, 66, 189, 133));

        yield return new object[] { infiniteExclude, new Rectangle(66, 292, 189, 133), true };
        yield return new object[] { new Region(), Rectangle.Empty, false };

        yield return new object[] { new Region(new Rectangle(0, 0, 10, 10)), new Rectangle(0, 0, 0, 1), false };
        yield return new object[] { new Region(new Rectangle(500, 30, 60, 80)), new Rectangle(500, 30, 60, 80), true };
        yield return new object[] { new Region(new Rectangle(500, 30, 60, 80)), new Rectangle(520, 40, 60, 80), true };

        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(1, 1, 2, 1), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(1, 1, 2, 2), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(1, 1, 10, 10), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(1, 1, 1, 1), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(2, 2, 1, 1), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(0, 0, 1, 1), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Rectangle(3, 3, 1, 1), false };
    }

    [Theory]
    [MemberData(nameof(IsVisible_Rectangle_TestData))]
    public void IsVisible_Rectangle_ReturnsExpected(Region region, Rectangle rectangle, bool expected)
    {
        using (region)
        using (Bitmap image = new(10, 10))
        {
            var disposedGraphics = Graphics.FromImage(image);
            disposedGraphics.Dispose();

            Assert.Equal(expected, region.IsVisible(rectangle));
            Assert.Equal(expected, region.IsVisible((RectangleF)rectangle));
            Assert.Equal(expected, region.IsVisible(rectangle, s_graphic));
            Assert.Equal(expected, region.IsVisible(rectangle, disposedGraphics));
            Assert.Equal(expected, region.IsVisible(rectangle, null));
            Assert.Equal(expected, region.IsVisible((RectangleF)rectangle, s_graphic));
            Assert.Equal(expected, region.IsVisible((RectangleF)rectangle, disposedGraphics));
            Assert.Equal(expected, region.IsVisible((RectangleF)rectangle, null));

            Assert.Equal(expected, region.IsVisible(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
            Assert.Equal(expected, region.IsVisible((float)rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
            Assert.Equal(expected, region.IsVisible(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, s_graphic));
            Assert.Equal(expected, region.IsVisible(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, disposedGraphics));
            Assert.Equal(expected, region.IsVisible(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, null));
            Assert.Equal(expected, region.IsVisible((float)rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, s_graphic));
            Assert.Equal(expected, region.IsVisible((float)rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, disposedGraphics));
            Assert.Equal(expected, region.IsVisible((float)rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, null));
        }
    }

    public static IEnumerable<object[]> IsVisible_Point_TestData()
    {
        Region infiniteExclude = new();
        infiniteExclude.Exclude(new Rectangle(387, 292, 189, 133));
        infiniteExclude.Exclude(new Rectangle(387, 66, 189, 133));

        yield return new object[] { infiniteExclude, new Point(66, 292), true };
        yield return new object[] { new Region(), Point.Empty, true };

        yield return new object[] { new Region(new Rectangle(500, 30, 60, 80)), new Point(500, 29), false };
        yield return new object[] { new Region(new Rectangle(500, 30, 60, 80)), new Point(500, 30), true };

        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(0, 1), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(1, 0), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(2, 0), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(3, 0), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(1, 1), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(2, 1), true };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(3, 1), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(0, 2), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(2, 2), false };
        yield return new object[] { new Region(new Rectangle(1, 1, 2, 1)), new Point(3, 2), false };
    }

    [Theory]
    [MemberData(nameof(IsVisible_Point_TestData))]
    public void IsVisible_Point_ReturnsExpected(Region region, Point point, bool expected)
    {
        using (region)
        using (Bitmap image = new(10, 10))
        {
            var disposedGraphics = Graphics.FromImage(image);
            disposedGraphics.Dispose();

            Assert.Equal(expected, region.IsVisible(point));
            Assert.Equal(expected, region.IsVisible((PointF)point));
            Assert.Equal(expected, region.IsVisible(point, s_graphic));
            Assert.Equal(expected, region.IsVisible(point, disposedGraphics));
            Assert.Equal(expected, region.IsVisible(point, null));
            Assert.Equal(expected, region.IsVisible((PointF)point, s_graphic));
            Assert.Equal(expected, region.IsVisible((PointF)point, disposedGraphics));
            Assert.Equal(expected, region.IsVisible((PointF)point, null));

            Assert.Equal(expected, region.IsVisible(point.X, point.Y));
            Assert.Equal(expected, region.IsVisible(point.X, point.Y, s_graphic));
            Assert.Equal(expected, region.IsVisible(point.X, point.Y, disposedGraphics));
            Assert.Equal(expected, region.IsVisible(point.X, point.Y, null));

            Assert.Equal(expected, region.IsVisible(point.X, point.Y, s_graphic));
            Assert.Equal(expected, region.IsVisible(point.X, point.Y, disposedGraphics));
            Assert.Equal(expected, region.IsVisible(point.X, point.Y, null));
            Assert.Equal(expected, region.IsVisible((float)point.X, point.Y, s_graphic));
            Assert.Equal(expected, region.IsVisible((float)point.X, point.Y, disposedGraphics));
            Assert.Equal(expected, region.IsVisible((float)point.X, point.Y, null));
        }
    }

    [Fact]
    public void IsVisible_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1f, 2f));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new PointF(1, 2)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new Point(1, 2)));

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1f, 2f, s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new PointF(1, 2), s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new Point(1, 2), s_graphic));

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1f, 2f, 3f, 4f));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new Rectangle(1, 2, 3, 4)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new RectangleF(1, 2, 3, 4)));

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1f, 2f, 3f, 4f, s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new Rectangle(1, 2, 3, 4), s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(new RectangleF(1, 2, 3, 4), s_graphic));

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1, 2, s_graphic));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1, 2, 3, 4));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.IsVisible(1, 2, 3, 4, s_graphic));
    }

    [Theory]
    [MemberData(nameof(Region_TestData))]
    public void MakeEmpty_NonEmpty_Success(Region region)
    {
        using (region)
        {
            region.MakeEmpty();
            Assert.True(region.IsEmpty(s_graphic));
            Assert.False(region.IsInfinite(s_graphic));
            Assert.Equal(RectangleF.Empty, region.GetBounds(s_graphic));

            using (Matrix matrix = new())
            {
                Assert.Empty(region.GetRegionScans(matrix));
            }

            region.MakeEmpty();
            Assert.True(region.IsEmpty(s_graphic));
        }
    }

    [Fact]
    public void MakeEmpty_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().MakeEmpty());
    }

    [Theory]
    [MemberData(nameof(Region_TestData))]
    public void MakeInfinite_NonInfinity_Success(Region region)
    {
        using (region)
        {
            region.MakeInfinite();
            Assert.False(region.IsEmpty(s_graphic));
            Assert.True(region.IsInfinite(s_graphic));
            Assert.Equal(new RectangleF(-4194304, -4194304, 8388608, 8388608), region.GetBounds(s_graphic));

            region.MakeInfinite();
            Assert.False(region.IsEmpty(s_graphic));
            Assert.True(region.IsInfinite(s_graphic));
        }
    }

    [Fact]
    public void MakeInfinite_Disposed_ThrowsArgumentException()
    {
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().MakeInfinite());
    }

    public static IEnumerable<object[]> Union_TestData()
    {
        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { new(500, 30, 60, 80) },
            new RectangleF[] { new(500, 30, 60, 80) }
        };

        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new(500, 30, 60, 80) }
        };

        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { new(520, 30, 60, 80) },
            new RectangleF[] { new(500, 30, 80, 80) }
        };

        yield return new object[]
        {
            new Region(new Rectangle(500, 30, 60, 80)),
            new RectangleF[] { new(520, 40, 60, 80) },
            new RectangleF[]
            {
                new(500, 30, 60, 10),
                new(500, 40, 80, 70),
                new(520, 110, 60, 10),
            }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { new(520, 40, 60, 80) },
            new RectangleF[] { new Rectangle(-4194304, -4194304, 8388608, 8388608) }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new Rectangle(-4194304, -4194304, 8388608, 8388608) }
        };

        // No intersecting rects.
        yield return new object[]
        {
            new Region(new Rectangle(20, 20, 20, 20)),
            new RectangleF[]
            {
                new(20, 80, 20, 10),
                new(60, 60, 30, 10)
            },
            new RectangleF[]
            {
                new(20, 20, 20, 20),
                new(60, 60, 30, 10),
                new(20, 80, 20, 10)
            }
        };

        yield return new object[]
        {
            new Region(new Rectangle(20, 180, 40, 50)),
            new RectangleF[]
            {
                new(50, 190, 40, 50),
                new(70, 210, 30, 50)
            },
            new RectangleF[]
            {
                new(20, 180, 40, 10),
                new(20, 190, 70, 20),
                new(20, 210, 80, 20),
                new(50, 230, 50, 10),
                new(70, 240, 30, 20)
            }
        };

        yield return new object[]
        {
            new Region(new Rectangle(20, 330, 40, 50)),
            new RectangleF[]
            {
                new(50, 340, 40, 50),
                new(70, 360, 30, 50),
                new(80, 400, 30, 10)
            },
            new RectangleF[]
            {
                new(20, 330, 40, 10),
                new(20, 340, 70, 20),
                new(20, 360, 80, 20),
                new(50, 380, 50, 10),
                new(70, 390, 30, 10),
                new(70, 400, 40, 10)
            }
        };

        yield return new object[]
        {
            new Region(new Rectangle(10, 20, 50, 50)),
            new RectangleF[]
            {
                new(100, 100, 60, 60),
                new(200, 200, 80, 80)
            },
            new RectangleF[]
            {
                new(10, 20, 50, 50),
                new(100, 100, 60, 60),
                new(200, 200, 80, 80)
            }
        };

        // Intersecting from the right.
        yield return new object[]
        {
            new Region(new Rectangle(10, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[]
            {
                new(10, 10, 100, 50),
                new(10, 60, 130, 20),
                new(10, 80, 100, 30)
            }
        };

        // Intersecting from the left.
        yield return new object[]
        {
            new Region(new Rectangle(70, 10, 100, 100)),
            new RectangleF[] { new(40, 60, 100, 20) },
            new RectangleF[]
            {
                new(70, 10, 100, 50),
                new(40, 60, 130, 20),
                new(70, 80, 100, 30)
            }
        };

        // Intersecting from the top.
        yield return new object[]
        {
            new Region(new Rectangle(40, 100, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[]
            {
                new(70, 80, 50, 20),
                new(40, 100, 100, 100)
            }
        };

        // Intersecting from the bottom.
        yield return new object[]
        {
            new Region(new Rectangle(40, 10, 100, 100)),
            new RectangleF[] { new(70, 80, 50, 40) },
            new RectangleF[]
            {
                new(40, 10, 100, 100),
                new(70, 110, 50, 10)
            }
        };

        // Multiple regions separated by 0 pixels.
        yield return new object[]
        {
            new Region(new Rectangle(30, 30, 80, 80)),
            new RectangleF[]
            {
                new(45, 45, 200, 200),
                new(160, 260, 10, 10),
                new(170, 260, 10, 10)
            },
            new RectangleF[]
            {
                new(30, 30, 80, 15),
                new(30, 45, 215, 65),
                new(45, 110, 200, 135),
                new(160, 260, 20, 10)
            }
        };
    }

    [Theory]
    [MemberData(nameof(Union_TestData))]
    public void Union_Region_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using Region other = new(rect);
                region.Union(other);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Union_InfiniteRegion_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Region other = new();
        using Matrix matrix = new();
        region.Union(other);

        Assert.Equal([new Rectangle(-4194304, -4194304, 8388608, 8388608)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Union_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Union((Region)null));
    }

    [Fact]
    public void Union_DisposedRegion_ThrowsArgumentException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentException>(null, () => region.Union(CreateDisposedRegion()));
    }

    [Fact]
    public void Union_SameRegion_ThrowsInvalidOperationException()
    {
        using Region region = new();
        Assert.Throws<InvalidOperationException>(() => region.Union(region));
    }

    [Theory]
    [MemberData(nameof(Union_TestData))]
    public void Union_Rectangle_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Union(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Union_TestData))]
    public void Union_RectangleF_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Union(rect);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Union_TestData))]
    public void Union_GraphicsPath_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                region.Union(path);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Union_EmptyPathWithInfiniteRegion_MakesInfinite()
    {
        using Region region = new();
        using GraphicsPath graphicsPath = new();
        region.Union(graphicsPath);
        Assert.True(region.IsInfinite(s_graphic));
    }

    [Fact]
    public void Union_NullGraphicsPath_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("path", () => region.Union((GraphicsPath)null));
    }

    [Fact]
    public void Union_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        using GraphicsPath graphicsPath = new();
        using Region other = new();
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Union(graphicsPath));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Union(default(Rectangle)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Union(default(RectangleF)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Union(disposedRegion));
    }

    [Fact]
    public void Transform_EmptyMatrix_Nop()
    {
        using Region region = new(new RectangleF(1, 2, 3, 4));
        using Matrix matrix = new();
        region.Transform(matrix);
        Assert.Equal([new(1, 2, 3, 4)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Transform_CustomMatrix_Success()
    {
        using Region region = new(new RectangleF(1, 2, 3, 4));
        using Matrix matrix = new();
        using Matrix emptyMatrix = new();
        matrix.Translate(10, 11);
        matrix.Scale(5, 6);

        region.Transform(matrix);
        Assert.Equal([new(15, 23, 15, 24)], region.GetRegionScans(emptyMatrix));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(2, 2, 0)]
    [InlineData(0.5, 0.5, 0)]
    [InlineData(1, 1, 45)]
    public void Transform_Infinity_Nop(float scaleX, float scaleY, int angle)
    {
        using Region region = new();
        using Matrix matrix = new();
        using Matrix emptyMatrix = new();
        matrix.Translate(10, 11);
        matrix.Scale(scaleX, scaleY);
        matrix.Rotate(angle);

        region.Transform(matrix);
        Assert.True(region.IsInfinite(s_graphic));
        Assert.Equal([new(-4194304, -4194304, 8388608, 8388608)], region.GetRegionScans(emptyMatrix));
    }

    [Fact]
    public void Transform_InfinityIntersectScale_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        using Matrix emptyMatrix = new();
        matrix.Scale(2, 0.5f);

        region.Intersect(new Rectangle(-10, -10, 20, 20));
        region.Transform(matrix);
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(-20, -5, 40, 10)], region.GetRegionScans(emptyMatrix));
    }

    [Fact]
    public void Transform_InfinityIntersectTransform_Success()
    {
        using Region region = new();
        using Matrix matrix = new(2, 0, 0, 0.5f, 10, 10);
        using Matrix emptyMatrix = new();
        region.Intersect(new Rectangle(-10, -10, 20, 20));
        region.Transform(matrix);

        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(-10, 5, 40, 10)], region.GetRegionScans(emptyMatrix));
    }

    [Fact]
    public void Transform_NullMatrix_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("matrix", () => region.Transform(null));
    }

    [Fact]
    public void Transform_Disposed_ThrowsArgumentException()
    {
        using Matrix matrix = new();
        AssertExtensions.Throws<ArgumentException>(null, () => CreateDisposedRegion().Transform(matrix));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 3)]
    [InlineData(-2, -3)]
    public void Translate_Int_Success(float dx, float dy)
    {
        using Region region = new(new RectangleF(1, 2, 3, 4));
        using Matrix matrix = new();
        region.Translate(dx, dy);
        Assert.Equal([new(1 + dx, 2 + dy, 3, 4)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Translate_IntInfinityIntersect_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Intersect(new Rectangle(-10, -10, 20, 20));
        region.Translate(10, 10);

        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(0, 0, 20, 20)], region.GetRegionScans(matrix));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 3)]
    public void Translate_Float_Success(int dx, int dy)
    {
        using Region region = new(new RectangleF(1, 2, 3, 4));
        using Matrix matrix = new();
        region.Translate(dx, dy);
        Assert.Equal([new(1 + dx, 2 + dy, 3, 4)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Translate_FloatInfinityIntersect_Success()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Intersect(new Rectangle(-10, -10, 20, 20));
        region.Translate(10f, 10f);

        Assert.False(region.IsInfinite(s_graphic));
        Assert.Equal([new(0, 0, 20, 20)], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Translate_Infinity_Nop()
    {
        using Region region = new();
        using Matrix matrix = new();
        region.Translate(10, 10);
        region.Translate(10f, 10f);

        Assert.True(region.IsInfinite(s_graphic));
        Assert.Equal([new(-4194304, -4194304, 8388608, 8388608)], region.GetRegionScans(matrix));
    }

    [Theory]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Translate_InvalidFloatValue_EmptiesRegion(float f)
    {
        using Region region = new(new RectangleF(1, 2, 3, 4));
        using Matrix matrix = new();
        region.Translate(f, 0);

        Assert.True(region.IsEmpty(s_graphic));
        Assert.False(region.IsInfinite(s_graphic));
        Assert.Empty(region.GetRegionScans(matrix));
    }

    [Fact]
    public void Translate_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Translate(1, 2));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Translate(1f, 2f));
    }

    public static IEnumerable<object[]> Xor_TestData()
    {
        yield return new object[]
        {
            new Region(new RectangleF(500, 30, 60, 80)),
            new RectangleF[] { new(500, 30, 60, 80) },
            Array.Empty<RectangleF>()
        };

        yield return new object[]
        {
            new Region(new RectangleF(500, 30, 60, 80)),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new(500, 30, 60, 80) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(0, 0, 0, 0)),
            new RectangleF[] { new(500, 30, 60, 80) },
            new RectangleF[] { new(500, 30, 60, 80) }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { new(520, 40, 60, 80) },
            new RectangleF[]
            {
                new(-4194304, -4194304, 8388608, 4194344),
                new(-4194304, 40, 4194824, 80),
                new(580, 40, 4193724, 80),
                new(-4194304, 120, 8388608, 4194184)
            }
        };

        yield return new object[]
        {
            new Region(),
            new RectangleF[] { RectangleF.Empty },
            new RectangleF[] { new Rectangle(-4194304, -4194304, 8388608, 8388608) }
        };

        yield return new object[]
        {
            new Region(new RectangleF(380, 30, 60, 80)),
            new RectangleF[] { new(410, 40, 60, 80) },
            new RectangleF[]
            {
                new(380, 30, 60, 10),
                new(380, 40, 30, 70),
                new(440, 40, 30, 70),
                new(410, 110, 60, 10)
            }
        };
    }

    [Theory]
    [MemberData(nameof(Xor_TestData))]
    public void Xor_Region_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using Region other = new(rect);
                region.Xor(other);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Xor_InfiniteRegion_Success()
    {
        using Region region = new(new Rectangle(1, 2, 3, 4));
        using Region other = new();
        using Matrix matrix = new();
        region.Xor(other);

        Assert.Equal(
        [
                new(-4194304, -4194304, 8388608, 4194306),
                new(-4194304, 2, 4194305, 4),
                new(4, 2, 4194300, 4),
                new(-4194304, 6, 8388608, 4194298)
        ], region.GetRegionScans(matrix));
    }

    [Fact]
    public void Xor_NullRegion_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("region", () => region.Xor((Region)null));
    }

    [Fact]
    public void Xor_DisposedRegion_ThrowsArgumentException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentException>(null, () => region.Xor(CreateDisposedRegion()));
    }

    [Fact]
    public void Xor_SameRegion_ThrowsInvalidOperationException()
    {
        using Region region = new();
        Assert.Throws<InvalidOperationException>(() => region.Xor(region));
    }

    [Theory]
    [MemberData(nameof(Xor_TestData))]
    public void Xor_Rectangle_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Xor(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Xor_TestData))]
    public void Xor_RectangleF_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                region.Xor(rect);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Theory]
    [MemberData(nameof(Xor_TestData))]
    public void Xor_GraphicsPath_Success(Region region, RectangleF[] rectangles, RectangleF[] expectedScans)
    {
        using (region)
        {
            foreach (RectangleF rect in rectangles)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                region.Xor(path);
            }

            using Matrix matrix = new();
            Assert.Equal(expectedScans, region.GetRegionScans(matrix));
        }
    }

    [Fact]
    public void Xor_EmptyPathWithInfiniteRegion_MakesInfinite()
    {
        using Region region = new();
        using GraphicsPath graphicsPath = new();
        region.Xor(graphicsPath);
        Assert.True(region.IsInfinite(s_graphic));
    }

    [Fact]
    public void Xor_NullGraphicsPath_ThrowsArgumentNullException()
    {
        using Region region = new();
        AssertExtensions.Throws<ArgumentNullException>("path", () => region.Xor((GraphicsPath)null));
    }

    [Fact]
    public void Xor_Disposed_ThrowsArgumentException()
    {
        Region disposedRegion = CreateDisposedRegion();

        using GraphicsPath graphicsPath = new();
        using Region other = new();
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Xor(graphicsPath));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Xor(default(Rectangle)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Xor(default(RectangleF)));
        AssertExtensions.Throws<ArgumentException>(null, () => disposedRegion.Xor(other));
    }
}
