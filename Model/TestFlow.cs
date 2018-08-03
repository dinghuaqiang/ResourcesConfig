using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceConfig.Model
{
    class TestFlow : IViewControl
    {
        public string url;
        public string appVersion;
        public string resVersion;
        public string appSize;

        public string bigVersion;
        public string smallVersion;

        public bool forceUpdate;

        public List<VersionModel> baseModel;
        public List<VersionModel> patchModel;
        public List<VersionModel> dllModel;

        public TestFlow()
        {
            init();
        }

        public void init()
        {
            url = "";
            appSize = "";
            appVersion = "";
            resVersion = "";
            bigVersion = "";
            smallVersion = "";
            forceUpdate = true;

            if (baseModel == null)
            {
                baseModel = new List<VersionModel>();
            }
            else
            {
                baseModel.Clear();
            }

            if (patchModel == null)
            {
                patchModel = new List<VersionModel>();
            }
            else
            {
                patchModel.Clear();
            }

            if (dllModel == null)
            {
                dllModel = new List<VersionModel>();
            }
            else
            {
                dllModel.Clear();
            }
        }

        override public void setText()
        {
            Form1.g_Context.TestAppUrlText.Text = url;
            Form1.g_Context.TestAppSizeText.Text = appSize;
            Form1.g_Context.TestCurrentVersionText.Text = appVersion;
            Form1.g_Context.TestBigVersionText.Text = bigVersion;
            Form1.g_Context.TestSmallVersionText.Text = smallVersion;
            Form1.g_Context.TestResVersion.Text = resVersion;
            Form1.g_Context.ForceUpdateAppTestCheckBox.Checked = forceUpdate;

            Form1.g_Context.TestVersionPatchList.Items.Clear();
            Form1.g_Context.TestVersionBaseList.Items.Clear();
            Form1.g_Context.DLLUpdateTestListBox.Items.Clear();

            InitListBox(Form1.g_Context.TestVersionBaseList, baseModel);
            InitListBox(Form1.g_Context.TestVersionPatchList, patchModel);
            InitListBox(Form1.g_Context.DLLUpdateTestListBox, dllModel);

            if (Form1.g_Context.TestVersionBaseList.Items.Count == 0)
                Form1.g_Context.ClearTestBaseInfos();
            if (Form1.g_Context.TestVersionPatchList.Items.Count == 0)
                Form1.g_Context.ClearTestPatchInfos();
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

            Form1.g_Context.TestVersionPatchList.Items.Clear();
            Form1.g_Context.TestVersionBaseList.Items.Clear();
        }
    }
}
