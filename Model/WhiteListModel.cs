using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceConfig.Model
{
    class WhiteListModel : IViewControl
    {
        public List<string> imeiList;
        public List<string> macList;
        public List<string> accountList;

        public WhiteListModel()
        {
            init();
        }

        public void init()
        {
            if (imeiList == null)
            {
                imeiList = new List<string>();
            }
            else
            {
                imeiList.Clear();
            }

            if (macList == null)
            {
                macList = new List<string>();
            }
            else
            {
                macList.Clear();
            }

            if (accountList == null)
            {
                accountList = new List<string>();
            }
            else
            {
                accountList.Clear();
            }
        }

        override public void setText()
        {
            foreach (string value in imeiList)
            {
                Form1.g_Context.IMEIText.AppendText(value + "\n");
            }

            foreach (string value in macList)
            {
                Form1.g_Context.MACText.AppendText(value + "\n");
            }

            foreach (string value in accountList)
            {
                Form1.g_Context.WhiteListText.AppendText(value + "\n");
            }
            //throw new Exception("The method or operation is not implemented.");
        }

        override public void clearText()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        override public void show()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        override public void hide()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        override public void clean()
        {
            init();

            Form1.g_Context.IMEIText.Text = "";
            Form1.g_Context.MACText.Text = "";
            Form1.g_Context.WhiteListText.Text = "";

        }
    }
}
