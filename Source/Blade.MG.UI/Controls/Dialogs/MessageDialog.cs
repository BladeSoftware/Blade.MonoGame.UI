using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Dialogs
{
    public class MessageDialog : DialogBase
    {

        public string MessageText { get; set; }


        public static async Task<DialogResult> OkAsync(Game game, string message)
        {
            var messageDialog = new MessageDialog();

            messageDialog.MessageText = message;

            messageDialog.Buttons = new List<DialogButton>();

            messageDialog.Buttons.Add(new DialogButton
            {
                Id = "Ok",
                Text = "OK",
                Width = 150
            });

            await messageDialog.ShowAsync(game);

            return messageDialog.Result;
        }

        public static async Task<DialogResult> OkCancelAsync(Game game, string message)
        {
            var messageDialog = new MessageDialog();

            messageDialog.MessageText = message;

            messageDialog.Buttons = new List<DialogButton>();

            messageDialog.Buttons.Add(new DialogButton
            {
                Id = "Ok",
                Text = "OK",
                Width = 150
            });

            messageDialog.Buttons.Add(new DialogButton
            {
                Id = "Cancel",
                Text = "Cancel",
                Width = 150
            });

            await messageDialog.ShowAsync(game);

            return messageDialog.Result;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(Game);
        }

        public void BuildScreen(Game game)
        {

            var gridMainLayout = new Grid()
            {
                //Height = 14,
                //Background = Color.Transparent,
                //Background = new Color(Color.DarkSlateBlue, 0.5f),
                Background = new Color(Color.DarkGray, 0.75f),
                //Margin = new Thickness(0, 15, 0, 0),
                //Padding = new Thickness(0, 0, 0, 0),

                HitTestVisible = true  // Prevent events from propagating to windows behind this one

            };


            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            base.AddChild(gridMainLayout);



            var border1 = new Border
            {
                BorderColor = Color.DarkBlue,
                BorderThickness = 2f,
                //Width = 800,
                //Height = 600,

                Background = Color.Gainsboro
            };

            gridMainLayout.AddChild(border1, 1, 1);


            var grid2 = new Grid()
            {
                Background = Color.White,
            };

            grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            //grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto), MinHeight = 150 });
            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

            border1.Content = grid2;


            var labelMessage = new Label()
            {
                Text = MessageText,
                TextColor = Color.Black,
                Margin = new Thickness(0, 10),
                HorizontalAlignment = HorizontalAlignmentType.Center
            };

            grid2.AddChild(labelMessage, 0, 1);


            var stackButtons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Height = 50,
                Margin = new Thickness(0, 20),
                //Background = Color.Red
            };

            grid2.AddChild(stackButtons, 0, 2);

            if (Buttons == null || Buttons.Count == 0)
            {
                // Add a default 'Ok' button
                stackButtons.AddChild(new Button()
                {
                    Text = "Ok",
                    Width = 150,
                    Margin = new Thickness(10, 2),

                    OnClickAsync = async (sender, uiEvent) => { await DialogButtonPressedAsync("Ok"); }
                });

            }
            else
            {
                foreach (var button in Buttons)
                {
                    stackButtons.AddChild(new Button()
                    {
                        Text = button.Text,
                        Width = button.Width ?? 200,
                        Margin = new Thickness(10, 2),
                        FontColor = button?.Color ?? Color.White,

                        //OnClick = (uiEvent) => { DialogButtonPressed(button.Id); }
                        OnClickAsync = async (sender, uiEvent) => { await DialogButtonPressedAsync(button.Id); }

                    });
                }
            }

        }

    }
}
