//using Blade.UI;
//using Blade.UI.Components;
//using Blade.UI.Controls;
//using Microsoft.Xna.Framework;
//using System.IO;

//namespace GameStudio.UI
//{
//    public class StartPage : UIWindow
//    {

//        public override void Initialize(Game game)
//        {
//            base.Initialize(game);
//        }

//        public override void LoadContent()
//        {
//            base.LoadContent();

//            BuildScreen(base.Game);
//        }

//        public void BuildScreen(Game game)
//        {

//            var gridMainLayout = new Grid()
//            {
//                //Height = 14,
//                //Background = Color.Transparent,
//                //Background = new Color(Color.DarkSlateBlue, 0.5f),
//                Background = new Color(Color.DarkGray, 0.75f),
//                Margin = new Thickness(0, 15, 0, 0),
//                Padding = new Thickness(0, 0, 0, 0),

//                HitTestVisible = true  // Prevent events from propogating to windows behind this one

//            };


//            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto) });
//            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

//            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
//            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

//            base.AddChild(gridMainLayout);



//            var border1 = new Border
//            {
//                BorderColor = Color.DarkBlue,
//                BorderThickness = 2f,
//                Width = 800,
//                Height = 600,

//                Background = Color.Gainsboro
//            };

//            gridMainLayout.AddChild(border1, 1, 1);


//            var grid2 = new Grid()
//            {
//                //Background = Color.Gainsboro,
//            };

//            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//            //grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

//            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
//            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

//            border1.Content = grid2;


//            var labelHeader = new Label()
//            {
//                Text = "Recent Projects",
//                TextColor = Color.Black,
//                Margin = new Thickness(0, 10),
//                HorizontalAlignment = HorizontalAlignmentType.Center
//            };

//            grid2.AddChild(labelHeader, 0, 0);


//            var recentItemsListView = new ListView()
//            {
//                //Background = Color.Orange,
//                //Margin = new Thickness(10, 0),
//                Padding = new Thickness(10),
//                HorizontalScrollBarVisible = false
//            };

//            grid2.AddChild(recentItemsListView, 0, 1);


//            var stackButtons = new StackPanel()
//            {
//                Name = "Start Page:Button Stack Panel",
//                Orientation = Orientation.Horizontal,
//                HorizontalScrollBarVisible = false,
//                VerticalScrollBarVisible = false,
//                HorizontalAlignment = HorizontalAlignmentType.Center,
//                Height = 50,
//                Margin = new Thickness(0, 20),
//                //Background = Color.Red
//            };

//            grid2.AddChild(stackButtons, 0, 2);


//            var buttonOpen = new Button()
//            {
//                Text = "Open Project",
//                Width = 200,
//                Margin = new Thickness(10, 2),

//                OnClick = (sender, uiEvent) => { OpenProject(); }
//            };

//            var buttonNew = new Button()
//            {
//                Text = "New Project",
//                Width = 200,
//                Margin = new Thickness(10, 2),

//                OnClick = (sender, uiEvent) => { NewProject(); }
//            };

//            stackButtons.AddChild(buttonNew);
//            stackButtons.AddChild(buttonOpen);



//            RecentItems recentItems = new RecentItems();
//            recentItems.Items.Add(new RecentItem() { Name = @"FreeFall", Path = @"C:\Users\Michael\Documents\Game Studio\FreeFall\FreeFall.gsproj" });
//            recentItems.Items.Add(new RecentItem() { Name = @"Grid Racer", Path = @"C:\Users\Michael\Documents\Game Studio\Grid Racer\Grid Racer.gsproj" });
//            recentItems.Items.Add(new RecentItem() { Name = @"test", Path = @"C:\Users\Michael\Documents\Game Studio\test\test.gsproj" });

//            recentItemsListView.DataContext = recentItems.Items;
//            recentItemsListView.ItemTemplateType = typeof(RecentItemTemplate);

//            recentItemsListView.OnDoubleClick = (sender, uiEvent) => { OpenRecentItem(recentItemsListView.SelectedItem); };


//        }

//        private void NewProject()
//        {
//            UIManager.Remove(this);

//            // TODO: Start a new Project
//        }

//        private void OpenProject()
//        {
//            string selectedProjectPath = null;

//            // TODO: Browse for a project 


//            //var dialog = new Dialog();
//            //dialog.ShowDialog(new MessageDialog() { MessageText = "Project Browse not implemented." }, Game, (result) => {  });

//            if (string.IsNullOrWhiteSpace(selectedProjectPath))
//            {
//                // No project file selected
//                return;
//            }

//            UIManager.Remove(this);

//            // TODO: Open an existing project
//            OpenProject(selectedProjectPath);
//        }

//        private void OpenRecentItem(object selectedItem)
//        {
//            RecentItem recentItem = selectedItem as RecentItem;

//            if (recentItem == null)
//            {
//                // User hasn't selected a recent item
//                return;
//            }


//            if (string.IsNullOrWhiteSpace(recentItem?.Path) || !File.Exists(recentItem.Path))
//            {
//                // TODO: Error - Invalid Recent Item - possibly moved or deleted ?
//                return;
//            }

//            UIManager.Remove(this);

//            OpenProject(recentItem.Path);
//        }

//        private void OpenProject(string projectPath)
//        {
//            // TODO: Open an existing project
//            // ..

//        }

//    }
//}
