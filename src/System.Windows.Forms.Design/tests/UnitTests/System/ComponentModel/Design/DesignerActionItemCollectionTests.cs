// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.ComponentModel.Design.Tests;

public class DesignerActionItemCollectionTests
{
    [Fact]
    public void DesignerActionItemCollection_Ctor_Default()
    {
        DesignerActionItemCollection collection = [];
        Assert.Empty(collection);
    }

    [Fact]
    public void DesignerActionItemCollection_Add_DesignerActionItem_Success()
    {
        DesignerActionItemCollection collection = [];

        SubDesignerActionItem value1 = new("displayName", "category", "description");
        collection.Add(value1);
        Assert.Same(value1, Assert.Single(collection));
        Assert.Same(value1, collection[0]);
        Assert.True(collection.Contains(value1));
        Assert.Equal(0, collection.IndexOf(value1));

        SubDesignerActionItem value2 = new("displayName", "category", "description");
        collection.Add(value2);
        Assert.Equal(new object[] { value1, value2 }, collection.Cast<object>());
        Assert.True(collection.Contains(value2));
        Assert.Equal(1, collection.IndexOf(value2));
    }

    [Fact]
    public void DesignerActionItemCollection_Add_NullValue_ThrowsArgumentNullException()
    {
        DesignerActionItemCollection collection = [];
        Assert.Throws<ArgumentNullException>("value", () => collection.Add(null));
    }

    [Fact]
    public void DesignerActionItemCollection_Insert_DesignerActionItem_Success()
    {
        DesignerActionItemCollection collection = [];

        SubDesignerActionItem value1 = new("displayName", "category", "description");
        collection.Insert(0, value1);
        Assert.Same(value1, Assert.Single(collection));
        Assert.Same(value1, collection[0]);
        Assert.True(collection.Contains(value1));
        Assert.Equal(0, collection.IndexOf(value1));

        SubDesignerActionItem value2 = new("displayName", "category", "description");
        collection.Insert(0, value2);
        Assert.Equal(new object[] { value2, value1 }, collection.Cast<object>());
        Assert.True(collection.Contains(value2));
        Assert.Equal(0, collection.IndexOf(value2));
    }

    [Fact]
    public void DesignerActionItemCollection_Insert_NullValue_ThrowsArgumentNullException()
    {
        DesignerActionItemCollection collection = [];
        Assert.Throws<ArgumentNullException>("value", () => collection.Insert(0, null));
    }

    [Fact]
    public void DesignerActionItemCollection_Remove_Invoke_Success()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);
        Assert.Same(value, Assert.Single(collection));

        collection.Remove(value);
        Assert.Empty(collection);
        Assert.False(collection.Contains(value));
        Assert.Equal(-1, collection.IndexOf(value));
    }

    [Fact]
    public void DesignerActionItemCollection_Remove_NullValue_ThrowsArgumentNullException()
    {
        DesignerActionItemCollection collection = [];
        Assert.Throws<ArgumentNullException>("value", () => collection.Remove(null));
    }

    [Fact]
    public void DesignerActionItemCollection_Item_Set_GetReturnsExpected()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value1 = new("displayName", "category", "description");
        SubDesignerActionItem value2 = new("displayName", "category", "description");
        collection.Add(value1);
        Assert.Same(value1, Assert.Single(collection));

        collection[0] = value2;
        Assert.Same(value2, Assert.Single(collection));
        Assert.Same(value2, collection[0]);
        Assert.False(collection.Contains(value1));
        Assert.Equal(-1, collection.IndexOf(value1));
        Assert.True(collection.Contains(value2));
        Assert.Equal(0, collection.IndexOf(value2));
    }

    [Fact]
    public void DesignerActionItemCollection_Item_SetNull_ThrowsArgumentNullException()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);
        Assert.Throws<ArgumentNullException>("value", () => collection[0] = null);
    }

    [Fact]
    public void DesignerActionItemCollection_CopyTo_Invoke_Success()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);

        var array = new DesignerActionItem[3];
        collection.CopyTo(array, 1);
        Assert.Equal([null, value, null], array);
    }

    [Fact]
    public void DesignerActionItemCollection_Contains_NoSuchValue_ReturnsFalse()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);

        Assert.False(collection.Contains(new SubDesignerActionItem("displayName", "category", "description")));
        Assert.False(collection.Contains(null));
    }

    [Fact]
    public void DesignerActionItemCollection_IndexOf_NoSuchValue_ReturnsNegativeOne()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);

        Assert.Equal(-1, collection.IndexOf(new SubDesignerActionItem("displayName", "category", "description")));
        Assert.Equal(-1, collection.IndexOf(null));
    }

    [Fact]
    public void DesignerActionItemCollection_Clear_Success()
    {
        DesignerActionItemCollection collection = [];
        SubDesignerActionItem value = new("displayName", "category", "description");
        collection.Add(value);

        collection.Clear();
        Assert.Empty(collection);

        // Clear again.
        collection.Clear();
        Assert.Empty(collection);
    }

    private class SubDesignerActionItem : DesignerActionItem
    {
        public SubDesignerActionItem(string displayName, string category, string description) : base(displayName, category, description)
        {
        }
    }
}
