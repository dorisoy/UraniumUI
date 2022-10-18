using InputKit.Shared.Controls;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Windows.Input;
using UraniumUI.Extensions;
using UraniumUI.Resources;

namespace UraniumUI.Material.Controls;
public partial class TreeView : VerticalStackLayout
{
    public static DataTemplate DefaultItemTemplate = new DataTemplate(() =>
    {
        var label = new Label { VerticalOptions = LayoutOptions.Center };
        label.SetBinding(Label.TextProperty, new Binding("Name"));
        return label;
    });

    public TreeView()
    {
        BindableLayout.SetItemTemplate(this, new DataTemplate(() =>
        {
            var holder = new TreeViewNodeHolderView(ItemTemplate, this, ChildrenBinding);
            holder.TreeView = this;
            return holder;
        }));

        DragGestureRecognizer.DragStarting += DragGestureRecognizer_DragStarting;
        DragGestureRecognizer.DropCompleted += DragGestureRecognizer_DropCompleted;
        DropGestureRecognizer.Drop += DropGestureRecognizer_Drop;
    }

    private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is DragGestureRecognizer recognizer && recognizer.Parent is TreeViewNodeHolderView holderView)
        {
            holderView.Opacity = 0.5;
            CurrentDraggingView = holderView;
        }
    }

    private void DragGestureRecognizer_DropCompleted(object sender, DropCompletedEventArgs e)
    {
        if (sender is DragGestureRecognizer recognizer && recognizer.Parent is TreeViewNodeHolderView holderView)
        {
            holderView.Opacity = 1;
            CurrentDraggingView = null;
        }
    }

    private void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
    {
        if (CurrentDraggingView == null)
        {
            return;
        }

        if (sender is DropGestureRecognizer recognizer && recognizer.Parent is TreeViewNodeHolderView targetHolder)
        {
            var targetParentHolder = targetHolder.FindInParents<TreeViewNodeHolderView>();

            var targetItemsSource = targetParentHolder != null ? targetParentHolder.FindChildrenItemsSource() : ItemsSource;

            var index = targetItemsSource.IndexOf(targetHolder.BindingContext);

            var sourceParentHolder = CurrentDraggingView.FindInParents<TreeViewNodeHolderView>();
            var sourceItemsSource = sourceParentHolder != null ? sourceParentHolder.FindChildrenItemsSource() : ItemsSource;

            if (sourceItemsSource == targetItemsSource)
            {
                sourceItemsSource.Remove(CurrentDraggingView.BindingContext);
                targetItemsSource.Insert(index, CurrentDraggingView.BindingContext);
            }
            else
            {
                targetItemsSource.Insert(index, CurrentDraggingView.BindingContext);
                sourceItemsSource.Remove(CurrentDraggingView);
            }
        }
    }

    public TreeViewNodeHolderView CurrentDraggingView { get; internal protected set; }

    public DragGestureRecognizer DragGestureRecognizer { get; } = new();
    public DropGestureRecognizer DropGestureRecognizer { get; } = new();

    private BindingBase childrenBinding = new Binding("Children");
    public BindingBase ChildrenBinding
    {
        get => childrenBinding; set
        {
            childrenBinding = value;
            foreach (TreeViewNodeHolderView view in this.Children.Where(x => x is TreeViewNodeHolderView))
            {
                view.ChildrenBinding = value;
            }
        }
    }

    private string isExpandedPropertyName = "IsExpanded";
    public string IsExpandedPropertyName
    {
        get => isExpandedPropertyName;
        set
        {
            isExpandedPropertyName = value;
            foreach (TreeViewNodeHolderView view in this.Children.Where(x => x is TreeViewNodeHolderView))
            {
                view.ApplyIsExpandedPropertyBindings();
            }
        }
    }

    private string isLeafPropertyName = "IsLeaf";
    public string IsLeafPropertyName
    {
        get => isLeafPropertyName;
        set
        {
            isLeafPropertyName = value;
            foreach (TreeViewNodeHolderView view in this.Children.Where(x => x is TreeViewNodeHolderView))
            {
                view.ApplyIsLeafPropertyBindings();
            }
        }
    }

    protected virtual void OnItemsSourceSet()
    {
        BindableLayout.SetItemsSource(this, ItemsSource);
    }

    private void OnItemTemplateChanged()
    {
        BindableLayout.SetItemTemplate(this, new DataTemplate(() =>
        {
            var holder = new TreeViewNodeHolderView(ItemTemplate, this, ChildrenBinding);
            holder.TreeView = this;
            return holder;
        }));

        OnItemsSourceSet();
    }
    protected virtual void OnArrowColorChanged()
    {
        foreach (var childHolder in Children.OfType<TreeViewNodeHolderView>())
        {
            childHolder.ReFillArrowColor();
        }
    }

    public bool UseAnimation { get; set; } = true;

    /// <summary>
    /// Only indicates if TreeView is busy or not. Doesn't affect anythig visually.
    /// </summary>
    public bool IsBusy { get; set; }

    public IList ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IList), typeof(TreeView), null,
        propertyChanged: (b, o, v) => (b as TreeView).OnItemsSourceSet());

    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(TreeView),
        defaultValue: DefaultItemTemplate, propertyChanged: (b, o, n) => (b as TreeView).OnItemTemplateChanged());

    public ICommand LoadChildrenCommand { get => (ICommand)GetValue(LoadChildrenCommandProperty); set => SetValue(LoadChildrenCommandProperty, value); }

    public static readonly BindableProperty LoadChildrenCommandProperty = BindableProperty.Create(
        nameof(LoadChildrenCommand), typeof(ICommand), typeof(TreeView), null);

    public Color ArrowColor { get => (Color)GetValue(ArrowColorProperty); set => SetValue(ArrowColorProperty, value); }

    public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
        nameof(ArrowColor), typeof(Color), typeof(TreeView), ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.DarkGray),
            propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeView).OnArrowColorChanged());

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.CreateAttached("IsExpanded", typeof(bool), typeof(TreeViewNodeHolderView), false,
            propertyChanged: (bindable, oldValue, newValue) => (bindable as TreeViewNodeHolderView).OnIsExpandedChanged());

    public static bool GetIsExpanded(BindableObject view) => (bool)view.GetValue(IsExpandedProperty);

    public static void SetIsExpanded(BindableObject view, bool value) => view.SetValue(IsExpandedProperty, value);
}