using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Scope.Views.Behaviors
{
  /// <summary>
  /// This TextBox behavior looks for text between square brackets in the bound text
  /// property and highlights those words with a specified text style.
  /// </summary>
  public class TextBlockHighlightWordsInSquareBrackets : Behavior<TextBlock>
  {
    public static readonly DependencyProperty FormattedTextProperty =
        DependencyProperty.Register(
            "FormattedText",
            typeof(string),
            typeof(TextBlockHighlightWordsInSquareBrackets),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty FormattedTextStyleProperty =
       DependencyProperty.Register(
           "FormattedTextStyle",
           typeof(Style),
           typeof(TextBlockHighlightWordsInSquareBrackets));

    private List<Run> _lastResult;

    protected override void OnAttached()
    {
      base.OnAttached();
      if (_lastResult == null)
      {
        return;
      }
      AssociatedObject.Inlines.Clear();
      foreach (var inline in _lastResult)
      {
        AssociatedObject.Inlines.Add(inline);
      }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      if (e.Property!=FormattedTextProperty)
      {
        return;
      }
      
      string text = e.NewValue as string;

      _lastResult = new List<Run>();

      string[] words = text.Split(new string[] { "├", "┤" }, StringSplitOptions.None);
      
      for (int i = 0; i < words.Length; i++)
      {
        var run = new Run(words[i])
        {
          Style = i % 2 == 1 ? (Style)this.GetValue(FormattedTextStyleProperty) : null
        };

        _lastResult.Add(run);
      }

      base.OnPropertyChanged(e);
    }

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
  }
}
