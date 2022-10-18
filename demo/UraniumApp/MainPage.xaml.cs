using UraniumUI.Pages;

namespace UraniumApp;

public partial class MainPage : UraniumContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCorrectDrop(object sender, DropEventArgs e)
    {
        if (sender is DropGestureRecognizer recognizer  && recognizer.Parent is BoxView box)
        {
            box.Color = Colors.Green;
            
            await Task.Delay(1000);
            box.Color = Colors.Silver;
        }
    }

    private void OnIncorrectDragOver(object sender, DragEventArgs e)
    {
        if (sender is DropGestureRecognizer recognizer && recognizer.Parent is BoxView box)
        {
            box.Color = Colors.Red;
        }
    }

    private void OnIncorrectDragLeave(object sender, DragEventArgs e)
    {
        if (sender is DropGestureRecognizer recognizer && recognizer.Parent is BoxView box)
        {
            box.Color = Colors.Silver;
        }
    }

    private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
    {
        if (sender is DragGestureRecognizer recognizer && recognizer.Parent is BoxView box)
        {
            box.Opacity = 0.5;
        }
    }

    private void DragGestureRecognizer_DropCompleted(object sender, DropCompletedEventArgs e)
    {
        if (sender is DragGestureRecognizer recognizer && recognizer.Parent is BoxView box)
        {
            box.Opacity = 1;
        }
    }
}