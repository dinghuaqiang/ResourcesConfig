using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceConfig;
using System.Windows.Forms;

namespace ResourceConfig.Model
{
    class NormalFlow : IViewControl
    {
        public string url;
        public string appVersion;
        public string appSize;
        public string resVersion;
        /// <summary>
        /// 是否强更
        /// </summary>
        public bool forceUpdate;

        public List<VersionModel> baseModel;
        public List<VersionModel> patchModel;
        public List<VersionModel> dllModel;

        public NormalFlow()
        {
            init();
        }

        public void init()
        {
            url = "";
            appVersion = "";
            appSize = "";
            resVersion = "";
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
            Form1.g_Context.NormalAppUrlText.Text = url;
            Form1.g_Context.NormalAppVersionText.Text = appVersion;
            Form1.g_Context.NormalAppSizeText.Text = appSize;
            Form1.g_Context.NormalResVersionBox.Text = resVersion;
            Form1.g_Context.ForceUpdateAppCheckBox.Checked = forceUpdate;

            Form1.g_Context.NormalPatchList.Items.Clear();
            Form1.g_Context.NormalBaseList.Items.Clear();
            Form1.g_Context.DllUpdateListBox.Items.Clear();

            InitListBox(Form1.g_Context.NormalBaseList, baseModel);
            InitListBox(Form1.g_Context.NormalPatchList, patchModel);
            InitListBox(Form1.g_Context.DllUpdateListBox, dllModel);

            if (Form1.g_Context.NormalBaseList.Items.Count == 0)
                Form1.g_Context.ClearNormalBaseInfos();
            if (Form1.g_Context.NormalPatchList.Items.Count == 0)
                Form1.g_Context.ClearNormalPatchInfos();
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
            Form1.g_Context.NormalPatchList.Items.Clear();
            Form1.g_Context.NormalBaseList.Items.Clear();
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
