using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ResourceConfig.Model
{
    abstract class IViewControl
    {
        abstract public void setText();
        abstract public void clearText();
        abstract public void show();
        abstract public void hide();
        abstract public void clean();

        public void InitListBox(ListBox box, List<VersionModel> modelList)
        {
            foreach (VersionModel mode in modelList)
            {
                if (string.IsNullOrEmpty(mode.toVersion))
                {
                    box.Items.Add(mode.fromVersion);
                }
                else
                {
                    box.Items.Add(mode.fromVersion + " -> " + mode.toVersion);
                }
            }
            if (box.Items.Count > 0)
            {
                box.SelectedIndex = 0;
            }
        }
    }
}
