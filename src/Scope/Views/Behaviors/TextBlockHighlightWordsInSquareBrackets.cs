using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Scope.Views.Behaviors
{
  /// <summary>
  /// This TextBox behavior looks for text between square brackets in the bound text
  /// property and highlights those words with a specified text style.
  /// </summary>
  public class TextBlockHighlightWordsInSquareBrackets: Behavior<System.Windows.Controls.TextBlock>
  {
    public static readonly DependencyProperty FormattedTextProperty =
        DependencyProperty.Register(
            "FormattedText",
            typeof(string),
            typeof(TextBlockHighlightWordsInSquareBrackets),
            new PropertyMetadata(string.Empty, OnFormattedTextChanged));

    public static readonly DependencyProperty FormattedTextStyleProperty =
       DependencyProperty.Register(
           "FormattedTextStyle",
           typeof(Style),
           typeof(TextBlockHighlightWordsInSquareBrackets));

    public string FormattedText
    {
      get { return (string)AssociatedObject.GetValue(FormattedTextProperty); }
      set { AssociatedObject.SetValue(FormattedTextProperty, value); }
    }

    public Style FormattedTextStyle
    {
      get { return (Style)AssociatedObject.GetValue(FormattedTextStyleProperty); }
      set { AssociatedObject.SetValue(FormattedTextStyleProperty, value); }
    }

    private static void OnFormattedTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      var behavior = o as TextBlockHighlightWordsInSquareBrackets;
      TextBlock textBlock = behavior.AssociatedObject;

      string text = e.NewValue as string;

      if (textBlock == null)
      {
        return;
      }
        textBlock.Inlines.Clear();


      string[] words = text.Split(new string[] { "[", "]" }, StringSplitOptions.None);
      List<Run> actual = new List<Run>();

      for (int i = 0; i < words.Length; i++)
      {
        var run = new Run(words[i]) { Style = i % 2 == 1 ? null:behavior.FormattedTextStyle};

        textBlock.Inlines.Add(run);
      }
      
    }
  }
}
