using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{
    public class TabPanel : Container
    {
        private class TabPageInternal //: ITabPage
        {
            public string Id { get; set; }
            public object DataContext { get; set; }

            public UIComponent Content { get; set; }

            public override string ToString()
            {
                return DataContext?.ToString() ?? "";
            }
        }

        private static int frameID = 0;

        private StackPanel tabsStackPanel;
        private Panel contentPanel;

        public UIComponent SelectedPage { get; set; } = null;
        public UIComponent SelectedHeader { get; set; } = null;

        [JsonIgnore]
        [XmlIgnore]
        public Type TabHeaderTemplateType { get; set; } = typeof(TabHeaderTemplate);

        public Color DividerColor { get; set; } = Color.Orange;

        private Dictionary<UIComponent, UIComponent> tabHeaderLink = new();  // Dictionary< 'Child Control', 'Tab Header'> ...

        private Dictionary<UIComponent, UIComponent> tabsToRemove = new();  // Dictionary< 'Child Control', 'Tab Header'> ...

        public TabPanel()
        {
            IsHitTestVisible = true;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Background = Color.DarkSlateBlue;

            tabsStackPanel = new StackPanel
            {
                //MinHeight = 20,
                IsHitTestVisible = true,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
                Padding = new Thickness(2, 2, 2, 2),
                Background = Theme.SecondaryContainer, //Color.DimGray,
                Parent = this
            };

            contentPanel = new Panel
            {
                IsHitTestVisible = true
                //Margin = new Thickness(0, 10, 0, 0)
            };

            //tabsStackPanel.Parent = this;
            //contentPanel.Parent = this;

            //internalChildren.Add(tabsStackPanel);
            //internalChildren.Add(contentPanel);

            AddInternalChild(tabsStackPanel);
            AddInternalChild(contentPanel);
        }

        public string AddTab(UIComponent content, object dataContext, bool setAsActiveTab = false, string id = null)
        {
            if (DataContext == null)
            {
                DataContext = new List<TabPageInternal>();
            }

            if (id == null)
            {
                id = Guid.NewGuid().ToString();
            }

            var tabPages = DataContext as List<TabPageInternal>;
            tabPages.Add(new TabPageInternal { Id = id, DataContext = dataContext, Content = content });


            var tabHeaderTemplate = Activator.CreateInstance(TabHeaderTemplateType) as UIComponent;
            tabHeaderTemplate.DataContext = dataContext;

            if (tabsStackPanel != null)
            {
                tabsStackPanel.AddChild(tabHeaderTemplate, null, dataContext);
            }

            // Item is of type TabPage
            tabHeaderLink.Add(content, tabHeaderTemplate);

            if (SelectedPage == null || setAsActiveTab)
            {
                SetActiveTab(content);
            }

            return id;
        }


        public void CloseTab(string tabId)
        {
            var tabPages = DataContext as List<TabPageInternal>;
            if (tabPages != null)
            {
                var tabpage = tabPages.Where(p => p.Id == tabId).FirstOrDefault();
                if (tabpage != null)
                {
                    tabPages.Remove(tabpage);
                }
            }
        }

        public void CloseTab(UIComponent tabPage)
        {
            UIComponent tabHeader = null;

            var tabPages = DataContext as List<TabPageInternal>;

            if (tabPage != null && tabPages != null)
            {

                // Check if we've been passed a Tab Page 
                if (tabHeaderLink.TryGetValue(tabPage, out tabHeader))
                {

                }
                else
                {
                    // We might have been passed a Tab Header
                    var tabKvp = tabHeaderLink.Where(kvp => kvp.Value == tabPage).FirstOrDefault();

                    if (tabKvp.Value != null)
                    {
                        tabPage = tabKvp.Key;
                        tabHeader = tabKvp.Value;

                    }
                }

                if (tabHeader != null)
                {
                    int index = tabsStackPanel.IndexOfChild(tabHeader);

                    //tabsStackPanel.RemoveChild(tabHeader); // Remove Header from Header Stack Panel
                    //tabHeaderLink.Remove(tabPage);
                    tabsToRemove.Add(tabPage, tabHeader);

                    var tabPageInternal = tabPages.Where(p => p.Content == tabPage).FirstOrDefault();
                    if (tabPageInternal != null)
                    {
                        tabPages.Remove(tabPageInternal);
                    }

                    if (SelectedPage == tabPage || SelectedHeader == tabHeader)
                    {
                        SelectedPage = null;
                        SelectedHeader = null;

                        contentPanel.RemoveAllChildren();

                        if (index > 0)
                        {
                            index--;
                        }
                        else if (tabsStackPanel.Children.Count() > 1)
                        {
                            index++;
                        }

                        if (tabsStackPanel.Children.Count() > 1)
                        {
                            SetActiveTab(tabsStackPanel.Children.ElementAt(index));
                        }

                    }

                }
            }

        }


        public override void AddChild(UIComponent item, UIComponent parent = null, object dataContext = null)
        {
            throw new Exception("Children can't be directly added to a TabPabel, please use AddTab(...) instead.");
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            ////-----------------
            //// Is there a better place to put this ?

            //frameID = (frameID % 2000000) + 1;

            //var pages = DataContext as IEnumerable<ITabPage>;
            //if (pages != null)
            //{
            //    int pageIndex = 0;
            //    foreach (var page in pages)
            //    {
            //        var childPage = tabsStackPanel.Children.Where(p => p?.DataContext.GetHashCode() == page.Content.DataContext.GetHashCode()).FirstOrDefault();

            //        if (childPage == null)
            //        {
            //            //childPage = new TabPage { };
            //            childPage = page.Content;
            //            AddChild(childPage, null, page);
            //        }
            //        childPage.FrameID = frameID;

            //        // Sort pages
            //        var childrenAsList = (Children as IList);
            //        var insertedIndex = childrenAsList.IndexOf(childPage);

            //        if (insertedIndex != pageIndex)
            //        {
            //            childrenAsList.Remove(childPage);
            //            childrenAsList.Insert(pageIndex, childPage);
            //        }

            //        pageIndex++;
            //    }

            //    // Remove stale children
            //    for (int i = Children.Count() - 1; i >= 0; i--)
            //    {
            //        if (Children.ElementAt(i).FrameID != frameID)
            //        {
            //            ((IList<UIComponent>)Children).RemoveAt(i);
            //        }
            //    }

            //}
            ////-----------------

            base.Measure(context, ref availableSize, ref parentMinMax);

            //foreach (var child in privateControls)
            //{
            //    child?.Measure(context, ref availableSize, ref parentMinMax);
            //}

            tabsStackPanel.Measure(context, ref availableSize, ref parentMinMax);

            if (contentPanel.Margin.Value.Top != tabsStackPanel.FinalRect.Height)
            {
                contentPanel.Margin = new Thickness(0, tabsStackPanel.FinalRect.Height, 0, 0);
            }

            contentPanel.Measure(context, ref availableSize, ref parentMinMax);

        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            //foreach (var child in privateControls)
            //{
            //    child?.Arrange(context, FinalContentRect, layoutBounds);
            //}

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            var contentLayoutRect = Rectangle.Intersect(layoutBounds, FinalContentRect);

            // Render any child controls
            base.RenderControl(context, contentLayoutRect, parentTransform);


            //// Render the Tab Headers
            //foreach (var child in privateControls)
            //{
            //    child?.RenderControl(context, contentLayoutRect, parentTransform);
            //}

            // Draw the dividing line between the tab headers and tab body
            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);

                Rectangle dividerRect = new Rectangle(tabsStackPanel.FinalRect.Left, tabsStackPanel.FinalRect.Bottom - 2, tabsStackPanel.FinalRect.Width, 2);
                context.Renderer.FillRect(spriteBatch, dividerRect, DividerColor, layoutBounds);
            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

        public bool IsActiveTab(UIComponent tabHeader)
        {
            if (tabHeader == null || SelectedPage == null)
            {
                return false;
            }

            return tabHeader == SelectedHeader;
        }

        public void SetActiveTab(string tabId)
        {
            var tabPages = DataContext as List<TabPageInternal>;
            if (tabPages != null)
            {
                var tabpage = tabPages.Where(p => p.Id == tabId).FirstOrDefault();
                if (tabpage != null)
                {
                    SetActiveTab(tabpage.Content);
                }
            }

        }

        public void SetActiveTab(UIComponent tabPage)
        {
            UIComponent oldHeader = null;
            UIComponent newHeader = null;


            oldHeader = SelectedHeader;


            newHeader = null;

            if (tabPage == null || contentPanel == null)
            {
                SelectedPage = null;
                SelectedHeader = null;
            }
            else
            {
                contentPanel.RemoveAllChildren();

                // Check if we've been passed a Tab Page 
                if (tabHeaderLink.TryGetValue(tabPage, out newHeader))
                {
                }
                else
                {
                    // We might have been passed a Tab Header
                    var tabKvp = tabHeaderLink.Where(kvp => kvp.Value == tabPage).FirstOrDefault();
                    if (tabKvp.Value == tabPage)
                    {
                        tabPage = tabKvp.Key;
                        newHeader = tabKvp.Value;
                    }
                }

                // Select the Tab Page
                SelectedPage = tabPage;
                SelectedHeader = newHeader;

                contentPanel.AddChild(tabPage, null, tabPage?.DataContext);
            }


            oldHeader?.StateHasChanged();
            newHeader?.StateHasChanged();

        }

        private void ProcessTabsActions()
        {
            // Remove tabs queued for removal
            if (tabsToRemove.Count > 0)
            {
                foreach (var tab in tabsToRemove)
                {
                    var tabPage = tab.Key;
                    var tabHeader = tab.Value;

                    tabsStackPanel.RemoveChild(tabHeader); // Remove Header from Header Stack Panel
                    tabHeaderLink.Remove(tabPage);
                }

                tabsToRemove.Clear();
            }
        }

        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {

            // Handle click event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseClickEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

        public override async Task HandleMouseDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {

            // Handle double click event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseDoubleClickEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseDoubleClickEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

        public override async Task HandleMouseRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {

            // Handle right click event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseRightClickEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseRightClickEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {

            // Handle mouse down event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseDownEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {

            // Handle mouse up event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseUpEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {

            // Handle mouse wheel scroll event on Tab Headers
            foreach (var item in tabsStackPanel.Children)
            {
                if (item != null && (item.FinalContentRect.Contains(uiEvent.X, uiEvent.Y) || uiEvent.ForcePropogation))
                {
                    await item.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

                    if (!uiEvent.Handled)
                    {
                        SetActiveTab(item);
                    }
                }
            }

            ProcessTabsActions();

            await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

            // Prevent Click Event for propogating outside the TabPanel
            uiEvent.Handled = true;
        }

    }
}
