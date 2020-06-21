using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFPL
{
    /// <summary>
    /// Placeholder attached property class
    /// </summary>
    public static class PlaceholderService
    {
        /// <summary>
        /// Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(object),
            typeof(PlaceholderService),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPlaceholderChanged)));

        #region Fields

        /// <summary>
        /// Mapping of ItemsControls
        /// </summary>
        private static readonly Dictionary<object, ItemsControl> itemsControls = new Dictionary<object, ItemsControl>();

        #endregion

        /// <summary>
        /// Get the Placeholder property
        /// </summary>
        /// <param name="d">Object to get the property from</param>
        /// <returns>The value of the Placeholder property</returns>
        public static object GetPlaceholderProperty(DependencyObject d)
        {
            return d.GetValue(PlaceholderProperty);
        }

        /// <summary>
        /// Set the Placeholder property
        /// </summary>
        /// <param name="d">Object to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetPlaceholder(DependencyObject d, object value)
        {
            d.SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        /// Handles changes to the Placeholder dependency property
        /// </summary>
        /// <param name="d">Object that fired the event</param>
        /// <param name="e">event data</param>
        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control control = (Control)d;
            control.Loaded += Control_Loaded;

            if (d is ComboBox)
            {
                control.GotKeyboardFocus += Control_GotKeyboardFocus;
                control.LostKeyboardFocus += Control_Loaded;
            }
            else if (d is TextBox)
            {
                control.GotKeyboardFocus += Control_GotKeyboardFocus;
                control.LostKeyboardFocus += Control_Loaded;
                ((TextBox)control).TextChanged += Control_GotKeyboardFocus;
            }

            DependencyObject d1 = d;
            if (d1 is ItemsControl && !(d is ComboBox))
            {
                ItemsControl i = (ItemsControl)d;
                i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                itemsControls.Add(i.ItemContainerGenerator, i);
                DependencyPropertyDescriptor prop = GetPropertyDescriptor(i);
                prop.AddValueChanged(i, ItemsSourceChanged);
            }
        }

        private static DependencyPropertyDescriptor GetPropertyDescriptor(ItemsControl i)
        {
            return DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
        }

        #region Event Handlers

        /// <summary>
        /// Handle GotKeyboardFocus event
        /// </summary>
        private static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            Control c = (Control)sender;
            if (ShouldShowPlaceholder(c))
                ShowPlaceholder(c);
            else
                RemovePlaceholder(c);
        }

        /// <summary>
        /// Handle Loaded and LostKeyboardFocus event
        /// </summary>
        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            if (ShouldShowPlaceholder(control))
                ShowPlaceholder(control);
        }

        /// <summary>
        /// Event handler for the ItemsSource changed event
        /// </summary>
        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl c = (ItemsControl)sender;
            if (c.ItemsSource != null)
            {
                if (ShouldShowPlaceholder(c))
                    ShowPlaceholder(c);
                else
                    RemovePlaceholder(c);
            }
            else
                ShowPlaceholder(c);
        }

        /// <summary>
        /// Event handler for the items changed event
        /// </summary>
        private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            if (itemsControls.TryGetValue(sender, out ItemsControl control))
            {
                if (ShouldShowPlaceholder(control))
                    ShowPlaceholder(control);
                else
                    RemovePlaceholder(control);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Remove placeholder from element
        /// </summary>
        /// <param name="control">Element to remove placeholder from</param>
        private static void RemovePlaceholder(UIElement control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(control);

                if (adorners == null)
                    return;

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is PlaceholderAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Show placeholder on the control
        /// </summary>
        /// <param name="control">Control to show the placeholder on</param>
        private static void ShowPlaceholder(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            if (layer != null)
                layer.Add(new PlaceholderAdorner(control, GetPlaceholderProperty(control)));
        }

        /// <summary>
        /// Indicates whether or not the placeholder should be shown on the specified control
        /// </summary>
        /// <param name="c">Control to test</param>
        /// <returns>true if the placeholder should be shown; false otherwise</returns>
        private static bool ShouldShowPlaceholder(Control c)
        {
            if (c is ComboBox)
                return (c as ComboBox).Text == string.Empty;
            else if (c is TextBoxBase)
                return (c as TextBox).Text == string.Empty;
            else if (c is ItemsControl)
                return (c as ItemsControl).Items.Count == 0;
            else
                return false;
        }

        #endregion
    }

    /// <summary>
    /// Adorner for placeholder
    /// </summary>
    internal class PlaceholderAdorner : Adorner
    {
        #region Private Fields

        /// <summary>
        /// ContentPresenter to hold placeholder
        /// </summary>
        private readonly ContentPresenter contentPresenter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of PlaceholderAdorner
        /// </summary>
        public PlaceholderAdorner(UIElement adornedElement, object placeholder) :
           base(adornedElement)
        {
            IsHitTestVisible = false;

            contentPresenter = new ContentPresenter
            {
                Content = placeholder,
                Margin = new Thickness(Control.Margin.Left + Control.Padding.Left, Control.Margin.Top + Control.Padding.Top, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.5
            };

            if (Control is ItemsControl && !(Control is ComboBox))
            {
                contentPresenter.VerticalAlignment = VerticalAlignment.Center;
                contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding("IsVisible")
            {
                Source = adornedElement,
                Converter = new BooleanToVisibilityConverter()
            };
            SetBinding(VisibilityProperty, binding);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the number of children for the ContainerVisual
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the control that is being adorned
        /// </summary>
        private Control Control
        {
            get { return (Control)AdornedElement; }
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Returns a specified child Visual for the parent ContainerVisual
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        /// <summary>
        /// Implements custom measuring behavior for the adorner.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            // Get adorner to cover the whole control
            contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a FrameworkElement derived class. 
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}
