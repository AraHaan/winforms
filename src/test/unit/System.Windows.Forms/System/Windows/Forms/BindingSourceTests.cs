﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.ComponentModel;
using Moq;

namespace System.Windows.Forms.Tests;

public class BindingSourceTests
{
    [WinFormsFact]
    public void Ctor_Default()
    {
        using SubBindingSource source = new();
        Assert.True(source.AllowEdit);
        Assert.True(source.AllowNew);
        Assert.True(source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Empty(source);
        Assert.Equal(0, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Throws<IndexOutOfRangeException>(() => source.CurrencyManager.Current);
        Assert.True(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(-1, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Null(source.Current);
        Assert.Empty(source.DataMember);
        Assert.Null(source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.True(source.IsBindingSuspended);
        Assert.False(source.IsFixedSize);
        Assert.False(source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.False(source.IsSynchronized);
        Assert.Empty(source.List);
        Assert.IsType<BindingList<object>>(source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    public static IEnumerable<object[]> Ctor_Object_String_Null_TestData()
    {
        yield return new object[] { null, null, typeof(BindingList<object>) };
        yield return new object[] { null, string.Empty, typeof(BindingList<object>) };
        yield return new object[] { null, "dataMember", typeof(BindingList<object>) };
        yield return new object[] { new DataClass { List = null }, nameof(DataClass.List), typeof(BindingList<int>) };
        yield return new object[] { new DataClass { List = null }, nameof(DataClass.List), typeof(BindingList<int>) };

        yield return new object[] { new ObjectDataClass { List = null }, nameof(ObjectDataClass.List), typeof(BindingList<object>) };
        yield return new object[] { new ObjectDataClass { List = null }, nameof(ObjectDataClass.List).ToLowerInvariant(), typeof(BindingList<object>) };
    }

    [WinFormsTheory]
    [MemberData(nameof(Ctor_Object_String_Null_TestData))]
    public void Ctor_Object_String_Null(object dataSource, string dataMember, Type expectedType)
    {
        using SubBindingSource source = new(dataSource, dataMember);
        Assert.True(source.AllowEdit);
        Assert.True(source.AllowNew);
        Assert.True(source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Empty(source);
        Assert.Equal(0, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Throws<IndexOutOfRangeException>(() => source.CurrencyManager.Current);
        Assert.True(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(-1, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Null(source.Current);
        Assert.Same(dataMember, source.DataMember);
        Assert.Same(dataSource, source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.True(source.IsBindingSuspended);
        Assert.False(source.IsFixedSize);
        Assert.False(source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.False(source.IsSynchronized);
        Assert.Empty(source.List);
        Assert.IsType(expectedType, source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    public static IEnumerable<object[]> Ctor_Object_String_Empty_TestData()
    {
        foreach (string dataMember in new string[] { null, string.Empty })
        {
            List<int> emptyList = [];
            yield return new object[] { emptyList, dataMember, true, false, emptyList };

            int[] emptyArray = Array.Empty<int>();
            yield return new object[] { emptyArray, dataMember, false, true, emptyArray };

            Mock<IListSource> mockEmptyListSource = new(MockBehavior.Strict);
            mockEmptyListSource
                .Setup(s => s.GetList())
                .Returns(emptyList);
            yield return new object[] { mockEmptyListSource.Object, dataMember, true, false, emptyList };
        }
    }

    [WinFormsTheory]
    [MemberData(nameof(Ctor_Object_String_Empty_TestData))]
    public void Ctor_Object_String_Empty(object dataSource, string dataMember, bool expectedAllowRemove, bool expectedIsFixedSize, IList expected)
    {
        using SubBindingSource source = new(dataSource, dataMember);
        Assert.True(source.AllowEdit);
        Assert.False(source.AllowNew);
        Assert.Equal(expectedAllowRemove, source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Equal(expected, source);
        Assert.Equal(expected.Count, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Throws<IndexOutOfRangeException>(() => source.CurrencyManager.Current);
        Assert.True(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(-1, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Null(source.Current);
        Assert.Same(dataMember, source.DataMember);
        Assert.Same(dataSource, source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.True(source.IsBindingSuspended);
        Assert.Equal(expectedIsFixedSize, source.IsFixedSize);
        Assert.False(source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.False(source.IsSynchronized);
        Assert.Same(expected, source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    public static IEnumerable<object[]> Ctor_Object_String_TestData()
    {
        foreach (string dataMember in new string[] { null, string.Empty })
        {
            List<int> nonEmptyList = [1, 2, 3];
            yield return new object[] { nonEmptyList, dataMember, true, false, true, false, false, false, nonEmptyList };

            FixedSizeList<int> fixedSizeList = new() { 1, 2, 3 };
            yield return new object[] { fixedSizeList, dataMember, true, false, false, true, false, false, fixedSizeList };

            ReadOnlyList<int> readOnlyList = new() { 1, 2, 3 };
            yield return new object[] { readOnlyList, dataMember, false, false, false, false, true, false, readOnlyList };

            SynchronizedList<int> synchronizedList = new() { 1, 2, 3 };
            yield return new object[] { synchronizedList, dataMember, true, false, true, false, false, true, synchronizedList };

            int[] nonEmptyArray = [1, 2, 3];
            yield return new object[] { nonEmptyArray, dataMember, true, false, false, true, false, false, nonEmptyArray };

            Mock<IListSource> mockNonEmptyListSource = new(MockBehavior.Strict);
            mockNonEmptyListSource
                .Setup(s => s.GetList())
                .Returns(nonEmptyList);
            yield return new object[] { mockNonEmptyListSource.Object, dataMember, true, false, true, false, false, false, nonEmptyList };
        }

        List<int> list = [1, 2, 3];
        DataClass listDataClass = new() { List = list };
        yield return new object[] { listDataClass, nameof(DataClass.List), true, false, true, false, false, false, list };
        yield return new object[] { listDataClass, nameof(DataClass.List).ToLowerInvariant(), true, false, true, false, false, false, list };

        Mock<IListSource> mockListSource = new(MockBehavior.Strict);
        mockListSource
            .Setup(s => s.GetList())
            .Returns(list);
        ListSourceDataClass listSourceDataClass = new() { ListSource = mockListSource.Object };
        yield return new object[] { listSourceDataClass, nameof(ListSourceDataClass.ListSource), true, false, true, false, false, false, list };
        yield return new object[] { listSourceDataClass, nameof(ListSourceDataClass.ListSource).ToLowerInvariant(), true, false, true, false, false, false, list };
    }

    [WinFormsTheory]
    [MemberData(nameof(Ctor_Object_String_TestData))]
    public void Ctor_Object_String(object dataSource, string dataMember, bool expectedAllowEdit, bool expectedAllowNew, bool expectedAllowRemove, bool expectedIsFixedSize, bool expectedReadOnly, bool expectedIsSynchronized, IList expected)
    {
        using SubBindingSource source = new(dataSource, dataMember);
        Assert.Equal(expectedAllowEdit, source.AllowEdit);
        Assert.Equal(expectedAllowNew, source.AllowNew);
        Assert.Equal(expectedAllowRemove, source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Equal(expected, source);
        Assert.Equal(expected.Count, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Equal(expected[0], source.CurrencyManager.Current);
        Assert.False(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(0, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Equal(expected[0], source.Current);
        Assert.Same(dataMember, source.DataMember);
        Assert.Same(dataSource, source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.False(source.IsBindingSuspended);
        Assert.Equal(expectedIsFixedSize, source.IsFixedSize);
        Assert.Equal(expectedReadOnly, source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.Equal(expectedIsSynchronized, source.IsSynchronized);
        Assert.Same(expected, source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    public static IEnumerable<object[]> Ctor_Object_String_BindingList_TestData()
    {
        foreach (string dataMember in new string[] { null, string.Empty })
        {
            List<int> emptyList = [];

            List<int> nonEmptyList = [1, 2, 3];
            EnumerableWrapper<int> emptyEnumerable = new(emptyList);
            yield return new object[] { emptyEnumerable, dataMember, true, false, true, false, false, false, new List<int>[] { emptyList }, typeof(BindingList<EnumerableWrapper<int>>) };

            EnumerableWrapper<int> nonEmptyEnumerable = new(nonEmptyList);
            yield return new object[] { nonEmptyEnumerable, dataMember, true, false, true, false, false, false, nonEmptyList, typeof(BindingList<int>) };
        }

        object o1 = new();
        yield return new object[] { new ObjectDataClass { List = o1 }, nameof(ObjectDataClass.List), true, true, true, false, false, false, new BindingList<object> { o1 }, typeof(BindingList<object>) };

        object o2 = new();
        yield return new object[] { new ObjectDataClass { List = o2 }, nameof(ObjectDataClass.List).ToLowerInvariant(), true, true, true, false, false, false, new BindingList<object> { o2 }, typeof(BindingList<object>) };

        yield return new object[] { new ObjectDataClass { List = 1 }, nameof(ObjectDataClass.List), true, true, true, false, false, false, new BindingList<int> { 1 }, typeof(BindingList<int>) };
        yield return new object[] { new ObjectDataClass { List = 1 }, nameof(ObjectDataClass.List).ToLowerInvariant(), true, true, true, false, false, false, new BindingList<int> { 1 }, typeof(BindingList<int>) };
    }

    [WinFormsTheory]
    [MemberData(nameof(Ctor_Object_String_BindingList_TestData))]
    public void Ctor_Object_String_BindingList(object dataSource, string dataMember, bool expectedAllowEdit, bool expectedAllowNew, bool expectedAllowRemove, bool expectedIsFixedSize, bool expectedReadOnly, bool expectedIsSynchronized, IList expected, Type expectedType)
    {
        using SubBindingSource source = new(dataSource, dataMember);
        Assert.Equal(expectedAllowEdit, source.AllowEdit);
        Assert.Equal(expectedAllowNew, source.AllowNew);
        Assert.Equal(expectedAllowRemove, source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Equal(expected, source);
        Assert.Equal(expected.Count, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Equal(expected[0], source.CurrencyManager.Current);
        Assert.False(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(0, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Equal(expected[0], source.Current);
        Assert.Same(dataMember, source.DataMember);
        Assert.Same(dataSource, source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.False(source.IsBindingSuspended);
        Assert.Equal(expectedIsFixedSize, source.IsFixedSize);
        Assert.Equal(expectedReadOnly, source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.Equal(expectedIsSynchronized, source.IsSynchronized);
        Assert.Equal(expected, source.List);
        Assert.NotSame(expected, source.List);
        Assert.IsType(expectedType, source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    [WinFormsTheory]
    [NullAndEmptyStringData]
    public void Ctor_Object_String_IBindingList(string dataMember)
    {
        PropertyDescriptor sortProperty = TypeDescriptor.GetProperties(typeof(DataClass))[0];
        object syncRoot = new();
        Mock<IBindingList> mockList = new(MockBehavior.Strict);
        mockList
            .Setup(p => p.Count)
            .Returns(0);
        mockList
            .Setup(p => p.AllowEdit)
            .Returns(false);
        mockList
            .Setup(p => p.AllowNew)
            .Returns(false);
        mockList
            .Setup(p => p.AllowRemove)
            .Returns(false);
        mockList
            .Setup(p => p.GetEnumerator())
            .Returns(new List<int>().GetEnumerator());
        mockList
            .Setup(p => p.IsFixedSize)
            .Returns(false);
        mockList
            .Setup(p => p.IsReadOnly)
            .Returns(false);
        mockList
            .Setup(p => p.IsSynchronized)
            .Returns(false);
        mockList
            .Setup(p => p.IsSorted)
            .Returns(true);
        mockList
            .Setup(p => p.SortDirection)
            .Returns(ListSortDirection.Descending);
        mockList
            .Setup(p => p.SortProperty)
            .Returns(sortProperty);
        mockList
            .Setup(p => p.SupportsChangeNotification)
            .Returns(false);
        mockList
            .Setup(p => p.SupportsSearching)
            .Returns(true);
        mockList
            .Setup(p => p.SupportsSorting)
            .Returns(true);
        mockList
            .Setup(p => p.SyncRoot)
            .Returns(syncRoot);

        ListSortDescriptionCollection sortDescriptions = new();
        var mockListView = mockList.As<IBindingListView>();
        mockListView
            .Setup(p => p.SortDescriptions)
            .Returns(sortDescriptions);
        mockListView
            .Setup(p => p.SupportsAdvancedSorting)
            .Returns(true);
        mockListView
            .Setup(p => p.SupportsFiltering)
            .Returns(true);

        using SubBindingSource source = new(mockList.Object, dataMember);
        Assert.False(source.AllowEdit);
        Assert.False(source.AllowNew);
        Assert.False(source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Null(source.Container);
        Assert.Empty(source);
        Assert.Equal(0, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Throws<IndexOutOfRangeException>(() => source.CurrencyManager.Current);
        Assert.True(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(-1, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Null(source.Current);
        Assert.Same(dataMember, source.DataMember);
        Assert.Same(mockList.Object, source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.True(source.IsBindingSuspended);
        Assert.False(source.IsFixedSize);
        Assert.False(source.IsReadOnly);
        Assert.True(source.IsSorted);
        Assert.False(source.IsSynchronized);
        Assert.Same(mockList.Object, source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.Null(source.Site);
        Assert.Null(source.Sort);
        Assert.Same(sortDescriptions, source.SortDescriptions);
        Assert.Equal(ListSortDirection.Descending, source.SortDirection);
        Assert.Same(sortProperty, source.SortProperty);
        Assert.True(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.True(source.SupportsFiltering);
        Assert.True(source.SupportsSearching);
        Assert.True(source.SupportsSorting);
        Assert.Same(syncRoot, source.SyncRoot);
    }

    [WinFormsTheory]
    [NullAndEmptyStringData]
    public void Ctor_Object_String_ICurrencyManagerProvider(string dataMember)
    {
        Mock<ICurrencyManagerProvider> mockCurrencyManagerProvider = new(MockBehavior.Strict);
        mockCurrencyManagerProvider
            .Setup(p => p.CurrencyManager)
            .Returns<CurrencyManager>(null)
            .Verifiable();
        using BindingSource source = new(mockCurrencyManagerProvider.Object, dataMember);
        mockCurrencyManagerProvider.Verify(p => p.CurrencyManager, Times.Once());
    }

    [WinFormsTheory]
    [InlineData(typeof(PrivateDefaultConstructor))]
    [InlineData(typeof(NoDefaultConstructor))]
    [InlineData(typeof(ThrowingDefaultConstructor))]
    public void Ctor_InvalidDataSourceType_ThrowsNotSupportedException(Type dataSource)
    {
        Assert.Throws<NotSupportedException>(() => new BindingSource(dataSource, "dataMember"));
    }

    [WinFormsFact]
    public void Ctor_NoSuchDataMember_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new BindingSource(new DataClass(), "NoSuchProperty"));
    }

    [WinFormsFact]
    public void Ctor_IContainer()
    {
        using Container container = new();
        using SubBindingSource source = new(container);
        Assert.True(source.AllowEdit);
        Assert.True(source.AllowNew);
        Assert.True(source.AllowRemove);
        Assert.True(source.CanRaiseEvents);
        Assert.Same(container, source.Container);
        Assert.Empty(source);
        Assert.Equal(0, source.CurrencyManager.Count);
        Assert.Empty(source.CurrencyManager.Bindings);
        Assert.Same(source.CurrencyManager.Bindings, source.CurrencyManager.Bindings);
        Assert.Throws<IndexOutOfRangeException>(() => source.CurrencyManager.Current);
        Assert.True(source.CurrencyManager.IsBindingSuspended);
        Assert.Same(source, source.CurrencyManager.List);
        Assert.Equal(-1, source.CurrencyManager.Position);
        Assert.Same(source.CurrencyManager, source.CurrencyManager);
        Assert.Null(source.Current);
        Assert.Empty(source.DataMember);
        Assert.Null(source.DataSource);
        Assert.False(source.DesignMode);
        Assert.NotNull(source.Events);
        Assert.Same(source.Events, source.Events);
        Assert.Null(source.Filter);
        Assert.True(source.IsBindingSuspended);
        Assert.False(source.IsFixedSize);
        Assert.False(source.IsReadOnly);
        Assert.False(source.IsSorted);
        Assert.False(source.IsSynchronized);
        Assert.Empty(source.List);
        Assert.IsType<BindingList<object>>(source.List);
        Assert.True(source.RaiseListChangedEvents);
        Assert.NotNull(source.Site);
        Assert.Null(source.Sort);
        Assert.Empty(source.SortDescriptions);
        Assert.Equal(ListSortDirection.Ascending, source.SortDirection);
        Assert.Null(source.SortProperty);
        Assert.False(source.SupportsAdvancedSorting);
        Assert.True(source.SupportsChangeNotification);
        Assert.False(source.SupportsFiltering);
        Assert.False(source.SupportsSearching);
        Assert.False(source.SupportsSorting);
        Assert.Same(source.List.SyncRoot, source.SyncRoot);
    }

    [WinFormsFact]
    public void Ctor_NullContainer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("container", () => new SubBindingSource(null));
    }

    [WinFormsFact]
    public void ISupportInitializeNotification_GetProperties_ReturnsExpected()
    {
        using BindingSource bindingSource = [];
        ISupportInitializeNotification source = bindingSource;
        Assert.True(source.IsInitialized);
    }

    [WinFormsFact]
    public void BeginInitEndInit_Invoke_Success()
    {
        using BindingSource bindingSource = [];
        ISupportInitializeNotification source = bindingSource;
        source.BeginInit();
        Assert.False(source.IsInitialized);

        source.EndInit();
        Assert.True(source.IsInitialized);
    }

    [WinFormsFact]
    public void BeginInitEndInit_WithInitializedEvent_CallsEvent()
    {
        using BindingSource bindingSource = [];
        ISupportInitializeNotification source = bindingSource;
        int callCount = 0;
        EventHandler handler = (sender, e) =>
        {
            callCount++;
            Assert.Same(source, sender);
            Assert.Equal(EventArgs.Empty, e);
        };
        source.Initialized += handler;

        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);

        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(2, callCount);

        // Remove handler.
        source.Initialized -= handler;
        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(2, callCount);
    }

    [WinFormsFact]
    public void BeginInitEndInit_WithDataSource_CallsEvent()
    {
        using BindingSource bindingSource = new(new DataSource(), nameof(DataSource.Member));
        ISupportInitializeNotification source = bindingSource;
        int callCount = 0;
        EventHandler handler = (sender, e) =>
        {
            callCount++;
            Assert.Same(source, sender);
            Assert.Equal(EventArgs.Empty, e);
        };
        source.Initialized += handler;

        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);

        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(2, callCount);
    }

    [WinFormsFact]
    public void BeginInitEndInit_WithISupportInitializeNotificationDataSource_WaitsForDataSourceInitialize()
    {
        ISupportInitializeNotificationDataSource dataSource = new() { IsInitializedResult = false };
        using BindingSource bindingSource = new(dataSource, nameof(ISupportInitializeNotificationDataSource.Member));
        ISupportInitializeNotification source = bindingSource;
        int callCount = 0;
        EventHandler handler = (sender, e) =>
        {
            callCount++;
            Assert.Same(source, sender);
            Assert.Equal(EventArgs.Empty, e);
        };
        source.Initialized += handler;

        source.BeginInit();
        Assert.False(source.IsInitialized);
        Assert.Equal(0, callCount);

        // Call the source init - should not initialize and should instead wait for
        // the data source initialization.
        source.EndInit();
        Assert.False(source.IsInitialized);
        Assert.Equal(0, callCount);

        // Then, call the data source init.
        dataSource.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);

        // Call again.
        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);
    }

    [WinFormsFact]
    public void BeginInitEndInit_WithISupportInitializeNotificationDataSourceInitialized_DoesNotWaitForDataSourceInitialize()
    {
        ISupportInitializeNotificationDataSource dataSource = new() { IsInitializedResult = true };
        using BindingSource bindingSource = new(dataSource, nameof(ISupportInitializeNotificationDataSource.Member));
        ISupportInitializeNotification source = bindingSource;
        int callCount = 0;
        EventHandler handler = (sender, e) =>
        {
            callCount++;
            Assert.Same(source, sender);
            Assert.Equal(EventArgs.Empty, e);
        };
        source.Initialized += handler;

        source.BeginInit();
        Assert.False(source.IsInitialized);
        Assert.Equal(0, callCount);

        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);

        // Call again.
        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(2, callCount);
    }

    public static IEnumerable<object[]> BeginInitEndInit_SetDataSourceInInit_TestData()
    {
        yield return new object[] { null };
        yield return new object[] { new DataSource() };
    }

    [WinFormsTheory]
    [MemberData(nameof(BeginInitEndInit_SetDataSourceInInit_TestData))]
    public void BeginInitEndInit_SetDataSourceInInit_Success(object newDataSource)
    {
        ISupportInitializeNotificationDataSource dataSource = new() { IsInitializedResult = false };
        using BindingSource bindingSource = new(dataSource, nameof(ISupportInitializeNotificationDataSource.Member));
        ISupportInitializeNotification source = bindingSource;
        int callCount = 0;
        EventHandler handler = (sender, e) =>
        {
            callCount++;
            Assert.Same(source, sender);
            Assert.Equal(EventArgs.Empty, e);
        };
        source.Initialized += handler;
        dataSource.Initialized += (sender, e) =>
        {
            ((BindingSource)source).DataSource = newDataSource;
        };

        source.BeginInit();
        Assert.False(source.IsInitialized);
        Assert.Equal(0, callCount);

        // Call the source init - should not initialize and should instead wait for
        // the data source initialization.
        source.EndInit();
        Assert.False(source.IsInitialized);
        Assert.Equal(0, callCount);

        // Then, call the data source init.
        dataSource.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(1, callCount);

        // Call again.
        source.EndInit();
        Assert.True(source.IsInitialized);
        Assert.Equal(2, callCount);
    }

    [WinFormsTheory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Name asc")]
    [InlineData("Name DESC")]
    [InlineData("IDate ASC, Name desc")]
    public void Sort_SetValidValue_UpdatesSortProperty(string sortValue)
    {
        SimpleBindingList<Journal> list = new(
        [
            new Journal { IDate = DateTime.Now, Name = "A" },
            new Journal { IDate = DateTime.Now.AddDays(-2), Name = "B" }
        ]);

        using var source = new BindingSource { DataSource = list };
        if (sortValue is not null)
        {
            source.Sort = sortValue;
            Assert.Equal(sortValue, source.Sort);
        }
        else
        {
            Assert.Throws<NotImplementedException>(() => source.Sort = sortValue);
        }
    }

    [WinFormsFact]
    public void Sort_SetNullValue_ResetsSortProperty()
    {
        using BindingSource source = [];
        source.Sort = null;
        Assert.Null(source.Sort);
    }

    private class FixedSizeList<T> : VirtualList<T>
    {
        public override bool IsFixedSize => true;
    }

    private class ReadOnlyList<T> : VirtualList<T>
    {
        public override bool IsReadOnly => true;
    }

    private class SynchronizedList<T> : VirtualList<T>
    {
        public override bool IsSynchronized => true;
    }

    private class VirtualList<T> : IList
    {
        private readonly List<T> _innerList = [];

        public VirtualList()
        {
        }

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();

        public int Count => _innerList.Count;

        public object SyncRoot => ((IList)_innerList).SyncRoot;

        public virtual bool IsSynchronized => ((IList)_innerList).IsSynchronized;

        public virtual bool IsReadOnly => ((IList)_innerList).IsReadOnly;

        public virtual bool IsFixedSize => ((IList)_innerList).IsFixedSize;

        public void Add(T item) => _innerList.Add(item);

        public int Add(object item) => ((IList)_innerList).Add(item);

        public void Clear() => _innerList.Clear();

        public bool Contains(T item) => _innerList.Contains(item);

        public bool Contains(object item) => ((IList)_innerList).Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int arrayIndex) => ((IList)_innerList).CopyTo(array, arrayIndex);

        public bool Remove(T item) => _innerList.Remove(item);

        public void Remove(object item) => ((IList)_innerList).Remove(item);

        public T this[int index]
        {
            get => _innerList[index];
            set => _innerList[index] = value;
        }

        object IList.this[int index]
        {
            get => ((IList)_innerList)[index];
            set => ((IList)_innerList)[index] = value;
        }

        public int IndexOf(T item) => _innerList.IndexOf(item);

        public int IndexOf(object item) => ((IList)_innerList).IndexOf(item);

        public void Insert(int index, T item) => _innerList.Insert(index, item);

        public void Insert(int index, object item) => ((IList)_innerList).Insert(index, item);

        public void RemoveAt(int index) => _innerList.RemoveAt(index);
    }

    private class EnumerableWrapper<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _innerEnumerable;

        public EnumerableWrapper(IEnumerable<T> innerEnumerable)
        {
            _innerEnumerable = innerEnumerable;
        }

        public IEnumerator<T> GetEnumerator() => _innerEnumerable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerEnumerable.GetEnumerator();
    }

    private class DataClass
    {
        public IList<int> List { get; set; }
    }

    private class EnumerableDataClass
    {
        public IEnumerable Enumerable { get; set; }
    }

    private class ListSourceDataClass
    {
        public IListSource ListSource { get; set; }
    }

    private class ObjectDataClass
    {
        public object List { get; set; }
    }

    private class SubBindingSource : BindingSource
    {
        public SubBindingSource() : base()
        {
        }

        public SubBindingSource(object dataSource, string dataMember) : base(dataSource, dataMember)
        {
        }

        public SubBindingSource(IContainer container) : base(container)
        {
        }

        public new bool CanRaiseEvents => base.CanRaiseEvents;

        public new bool DesignMode => base.DesignMode;

        public new EventHandlerList Events => base.Events;
    }

    private class PrivateDefaultConstructor : List<int>, ITypedList
    {
        private PrivateDefaultConstructor() { }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }
    }

    private class NoDefaultConstructor : List<int>, ITypedList
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public NoDefaultConstructor(int i) { }
#pragma warning restore IDE0060

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }
    }

    private class ThrowingDefaultConstructor : List<int>, ITypedList
    {
        public ThrowingDefaultConstructor()
        {
            throw new DivideByZeroException();
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            throw new NotImplementedException();
        }
    }

    private class DataSource
    {
        public int Member { get; set; }
    }

    private class ISupportInitializeNotificationDataSource : ISupportInitializeNotification
    {
        public int Member { get; set; }

        public bool IsInitializedResult { get; set; }

        public bool IsInitialized => IsInitializedResult;

        public event EventHandler Initialized;

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            Initialized(this, null);
        }
    }
}

public class Journal
{
    public DateTime IDate { get; set; }
    public string Name { get; set; }
}

public class SimpleBindingList<T> : BindingList<T>, IBindingListView
{
    public SimpleBindingList(List<T> list) : base(list) { }
    public string Filter { get; set; }
    public ListSortDescriptionCollection SortDescriptions => new ListSortDescriptionCollection();
    public bool SupportsAdvancedSorting => true;
    public bool SupportsFiltering => true;
    public void ApplySort(ListSortDescriptionCollection sorts) { }
    public void RemoveFilter() { }
    public void RemoveSort() => throw new NotImplementedException();
}
