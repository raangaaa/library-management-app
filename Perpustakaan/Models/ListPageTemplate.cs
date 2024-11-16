using System;
using System.Reflection.Emit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Perpustakaan.Models;

public class ListPageTemplate
{
    public ListPageTemplate(Type type, string IconKey, string label)
    {
        ModelType = type;
        Label = label;

        Application.Current!.TryFindResource(IconKey, out var res);
        ListPageIcon = (StreamGeometry)res!;
    }

    public string Label { get; }
    public Type ModelType { get; }
    public StreamGeometry ListPageIcon { get;}
}