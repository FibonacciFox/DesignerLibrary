using Avalonia;
using Avalonia.Controls;
using Avalonia.PropertyStore;
using Avalonia.Styling;
using System;
using System.Linq;
using System.Xml.Linq;

public class XamlGenerator
{
    public string GenerateXaml(Control rootControl, string xClass = null)
    {
        var xElement = GenerateXElementForControl(rootControl, true, xClass);
        var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xElement);

        // Добавление стилей
        var styles = GenerateStyles();
        var stylesElement = new XElement("Window.Styles", styles);
        xElement.AddFirst(stylesElement);

        return document.ToString();
    }

    private XElement GenerateXElementForControl(Control control, bool isRoot = false, string xClass = null)
    {
        XNamespace ns = "https://github.com/avaloniaui";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        XNamespace d = "http://schemas.microsoft.com/expression/blend/2008";
        XNamespace mc = "http://schemas.openxmlformats.org/markup-compatibility/2006";

        XElement element;

        if (isRoot)
        {
            element = new XElement(ns + control.GetType().Name,
                new XAttribute(XNamespace.Xmlns + "x", x),
                new XAttribute(XNamespace.Xmlns + "d", d),
                new XAttribute(XNamespace.Xmlns + "mc", mc)
            );

            if (!string.IsNullOrEmpty(xClass))
            {
                element.Add(new XAttribute(x + "Class", xClass));
            }
        }
        else
        {
            element = new XElement(ns + control.GetType().Name);
        }

        var attributes = GenerateProperties(control);
        foreach (var attribute in attributes)
        {
            // Проверка на дублирование атрибутов
            if (!element.Attributes().Any(a => a.Name == attribute.Name))
            {
                element.Add(attribute);
            }
        }

        if (control is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                element.Add(GenerateXElementForControl(child));
            }
        }
        else if (control is ContentControl contentControl && contentControl.Content is Control content)
        {
            element.Add(GenerateXElementForControl(content));
        }

        return element;
    }

    private XAttribute[] GenerateProperties(Control control)
    {
        var properties = AvaloniaPropertyRegistry.Instance.GetRegistered(control);
        return properties
            .Where(property => !property.IsDirect && !property.IsAttached && !property.IsReadOnly)
            .Select(property =>
            {
                var value = control.GetValue(property);
                return value != AvaloniaProperty.UnsetValue && value != null ? new XAttribute(property.Name, value) : null;
            })
            .Where(attribute => attribute != null)
            .ToArray();
    }

    private XElement[] GenerateStyles()
    {
        var buttonStyle = new XElement("Style",
            new XAttribute("Selector", "Button"),
            new XElement("Setter",
                new XAttribute("Property", "Background"),
                new XAttribute("Value", "LightGray")),
            new XElement("Setter",
                new XAttribute("Property", "Foreground"),
                new XAttribute("Value", "Black")),
            new XElement("Setter",
                new XAttribute("Property", "BorderThickness"),
                new XAttribute("Value", "0")),
            new XElement("Setter",
                new XAttribute("Property", "CornerRadius"),
                new XAttribute("Value", "2")),
            new XElement("Setter",
                new XAttribute("Property", "Width"),
                new XAttribute("Value", "8")),
            new XElement("Setter",
                new XAttribute("Property", "Height"),
                new XAttribute("Value", "8"))
        );

        var bigButtonStyle = new XElement("Style",
            new XAttribute("Selector", "Button.Big"),
            new XElement("Setter",
                new XAttribute("Property", "Background"),
                new XAttribute("Value", "Black")),
            new XElement("Setter",
                new XAttribute("Property", "ClipToBounds"),
                new XAttribute("Value", "False")),
            new XElement("Setter",
                new XAttribute("Property", "BorderThickness"),
                new XAttribute("Value", "0")),
            new XElement("Setter",
                new XAttribute("Property", "CornerRadius"),
                new XAttribute("Value", "2")),
            new XElement("Setter",
                new XAttribute("Property", "Width"),
                new XAttribute("Value", "8")),
            new XElement("Setter",
                new XAttribute("Property", "Height"),
                new XAttribute("Value", "8"))
        );

        return new[] { buttonStyle, bigButtonStyle };
    }
}