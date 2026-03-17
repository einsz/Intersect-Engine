using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;

namespace Intersect.Editor.Forms.Helpers;

public static class EtoExtensions
{
    public static string GetText(this DropDown dropDown)
    {
        if (dropDown.SelectedIndex >= 0 && dropDown.Items.Count > dropDown.SelectedIndex)
            return dropDown.Items[dropDown.SelectedIndex].Text;
        return string.Empty;
    }

    public static void SetText(this DropDown dropDown, string text)
    {
        for (int i = 0; i < dropDown.Items.Count; i++)
        {
            if (dropDown.Items[i].Text == text)
            {
                dropDown.SelectedIndex = i;
                return;
            }
        }
    }
}
