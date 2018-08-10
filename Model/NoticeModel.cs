using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ResourceConfig.Model
{
    class NoticeModel : IViewControl
    {
        public string m_NoticeTitle;
        public string m_NoticeContent;
        public List<System.Windows.Forms.TextBox> NoticeList;

        public NoticeModel()
        {
            init();
        }

        public void init()
        {
            m_NoticeContent = "";
            m_NoticeTitle = "";
            if (NoticeList == null)
            {
                NoticeList = new List<TextBox>();
                NoticeList.Add(Form1.g_Context.Notice1);
                NoticeList.Add(Form1.g_Context.Notice2);
                NoticeList.Add(Form1.g_Context.Notice3);
                NoticeList.Add(Form1.g_Context.Notice4);
                NoticeList.Add(Form1.g_Context.Notice5);
            }
            else
            {
                foreach (TextBox listBox in NoticeList)
                {
                    listBox.Text = "";
                }
            }
        }

        override public void setText()
        {
            Form1.g_Context.NoticeTitle.Text = m_NoticeTitle;
            Form1.g_Context.NoticeContent.Text = m_NoticeContent;
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
        }
    }
}
