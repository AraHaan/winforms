﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms.Tests;

public class ListView_ListViewItemCollectionTests
{
    [WinFormsTheory]
    [InlineData("name2")]
    [InlineData("NAME2")]
    public void ListViewItemCollection_Find_InvokeKeyExists_ReturnsExpected(string key)
    {
        using ListView listView = new();

        ListViewItem child1 = new()
        {
            Name = "name1"
        };
        ListViewItem child2 = new()
        {
            Name = "name2"
        };
        ListViewItem child3 = new()
        {
            Name = "name2"
        };

        var grandchild1 = new ListViewItem.ListViewSubItem
        {
            Name = "name1"
        };
        var grandchild2 = new ListViewItem.ListViewSubItem
        {
            Name = "name2"
        };
        var grandchild3 = new ListViewItem.ListViewSubItem
        {
            Name = "name2"
        };

        ListView.ListViewItemCollection collection = listView.Items;
        child3.SubItems.Add(grandchild1);
        child3.SubItems.Add(grandchild2);
        child3.SubItems.Add(grandchild3);
        collection.Add(child1);
        collection.Add(child2);
        collection.Add(child3);

        // Search all subitems.
        Assert.Equal(new ListViewItem[] { child2, child3 }, collection.Find(key, searchAllSubItems: true));

        // Call again.
        Assert.Equal(new ListViewItem[] { child2, child3 }, collection.Find(key, searchAllSubItems: true));

        // Don't search all subitems.
        Assert.Equal(new ListViewItem[] { child2, child3 }, collection.Find(key, searchAllSubItems: false));

        // Call again.
        Assert.Equal(new ListViewItem[] { child2, child3 }, collection.Find(key, searchAllSubItems: false));
    }

    [WinFormsTheory]
    [InlineData("NoSuchName")]
    [InlineData("abcd")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    public void ListViewItemCollection_Find_InvokeNoSuchKey_ReturnsEmpty(string key)
    {
        using ListView listView = new();

        ListViewItem child1 = new()
        {
            Name = "name1"
        };
        ListViewItem child2 = new()
        {
            Name = "name2"
        };
        ListViewItem child3 = new()
        {
            Name = "name2"
        };
        var collection = listView.Items;
        collection.Add(child1);
        collection.Add(child2);
        collection.Add(child3);

        Assert.Empty(collection.Find(key, searchAllSubItems: true));
        Assert.Empty(collection.Find(key, searchAllSubItems: false));
    }

    [WinFormsTheory]
    [NullAndEmptyStringData]
    public void ListViewItemCollection_Find_NullOrEmptyKey_ThrowsArgumentNullException(string key)
    {
        using ListView listView = new();
        var collection = listView.Items;
        Assert.Throws<ArgumentNullException>("key", () => collection.Find(key, searchAllSubItems: true));
        Assert.Throws<ArgumentNullException>("key", () => collection.Find(key, searchAllSubItems: false));
    }

    [WinFormsFact]
    public void ListViewItemCollection_Given_That_We_Want_To_Use_List_ListViewItem_To_Add_Items()
    {
        using ListView listView = new();
        var list = new List<ListViewItem>();
        var keys = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            list.Add(new ListViewItem() { $"item{i}" });
            keys.Add($"item{i}");
        }

        ListView.ListViewItemCollection collection = listView.Items;
        collection.AddRange(list);
        foreach (var item in list)
        {
            Asset.Equal(item, collection.Find(key[list.IndexOf(item)], searchAllSubItems: false))l
        }
    }
}
