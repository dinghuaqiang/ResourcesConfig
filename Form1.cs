using System;
using System.Windows.Forms;
using ResourceConfig.Model;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ResourceConfig.Util;
using System.Management;
using System.Management.Instrumentation;
using System.Net;
using System.Runtime.InteropServices;   

namespace ResourceConfig
{
    //定义CPU的信息结构   
    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_INFO
    {
        public uint dwOemId;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public uint dwProcessorLevel;
        public uint dwProcessorRevision;
    }

    public partial class Form1 : Form
    {
        static int showAllTab = 0;//为1则表示不显示正式流程和测试流程的tab页面
        public static bool USE_DEFAULT_CONNECT_MODEL = true;
        public static string s_confict_file_name = "conflict";
        public static string s_confict_file_local_path = "Config/conflict";

        public static Form1 g_Context;
        ConfigXmlModel g_configXmlModel;

        VersionModel m_VersionModeForCopy;

        public Form1()
        {
            string str = DES.EncryptDES("Tm-:Aq3[)iW&amp;GPfD", DES.ENCRYPT_KEY);
            string str2 = DES.DecryptDES(str, DES.ENCRYPT_KEY);
            InitializeComponent();
            if (showAllTab == 1)
            {
                tabControl1.GetControl(0).Enabled = false;
                tabControl1.GetControl(1).Enabled = false;
                tabControl1.GetControl(3).Enabled = false;
            }
            g_Context = this;

            DirectoryInfo dirctory = new DirectoryInfo("tmp");
            if (dirctory.Exists)
            {
                dirctory.Delete(true);
            }

            g_configXmlModel = new ConfigXmlModel();
            if (g_configXmlModel.parse())
            {
                foreach (PlatformModel model in g_configXmlModel.m_Platforms)
                {
                    ResourceList.Items.Add(model.m_Type + "_" + model.m_Name + "    x");
                }
            }
            else
            {
                MessageBox.Show("解析配置文件失败");
            }

            hideControls();
        }

        private void hideControls()
        {
            string hideItemCfgFile = "Config/hideItemCfg.txt";
            if (File.Exists(hideItemCfgFile) == false)
            {
                return;
            }

            List<string> hideItems = new List<string>();
            FileStream fileStream = new FileStream(hideItemCfgFile, FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);
            string item = "";
            do 
            {
                item = reader.ReadLine();
                if (item == null)
                {
                    break;
                }
                if (item.Contains("#"))
                {
                    item = item.Substring(0, item.IndexOf('#'));
                }
                hideItems.Add(item);
            } while (item != null);

            reader.Close();
            fileStream.Close();

            List<GroupBox> groupBoxList = new List<GroupBox>();
            groupBoxList.Add(groupBox1);
            groupBoxList.Add(groupBox12);
            groupBoxList.Add(groupBox13);
            groupBoxList.Add(groupBox14);
            groupBoxList.Add(groupBox2);
            groupBoxList.Add(groupBox3);
            groupBoxList.Add(groupBox4);
            groupBoxList.Add(groupBox5);
            groupBoxList.Add(groupBox6);
            groupBoxList.Add(groupBox7);
            groupBoxList.Add(groupBox9);
            foreach (GroupBox box in groupBoxList)
            {
                foreach (Control controlItem in box.Controls)
                {
                    if (hideItems.Contains(controlItem.Name))
                    {
                        controlItem.Enabled = false;
                    }
                }
            }
        }

        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);

        bool isForceToUse = false;
        public bool checkConflictUse(string file)
        {
            file = file.Replace("x", "").Replace("√", "").Trim();
            FtpUtil ftpUtil = new FtpUtil(g_configXmlModel.m_Ip + ":" + g_configXmlModel.m_Port, "", g_configXmlModel.m_User, g_configXmlModel.m_Pwd);
            if (textBox_timeout.Text != "")
            {
                int timeout = -1;
                int.TryParse(textBox_timeout.Text, out timeout);
                if (timeout > 0)
                {
                    ftpUtil.setTimeOut(timeout);
                }
            }
            string cpuId = file + GetCpuID();
            string tempFile = "tmp/" + s_confict_file_name;

            FileInfo fileInfo = new FileInfo(tempFile);
            if (!fileInfo.Exists)
            {
                fileInfo.Directory.Create();
            }

            if (ftpUtil.FileExist(s_confict_file_name))
            {
                ftpUtil.Download(fileInfo.FullName, s_confict_file_name);
                StreamReader reader = new StreamReader(fileInfo.FullName);
                string cpuIdInFtp = reader.ReadLine();
                while (cpuIdInFtp != null)
                {
                    if (cpuIdInFtp.StartsWith(file) && cpuIdInFtp != cpuId)
                    {
                        if (DialogResult.Cancel == MessageBox.Show("配置文件" + file + "正在被使用，点击确认强制使用这个文件，但提交该文件可能会覆盖别人的修改结果，点击取消则取消当前操作！", "警告", MessageBoxButtons.OKCancel))
                        {
                            reader.Close();
                            //this.Close();
                            return true;
                        }
                        else
                        {
                            isForceToUse = true;
                            break;
                        }
                    }
                    cpuIdInFtp = reader.ReadLine();
                }
                reader.Close();
            }

            //if (!isForceToUse)
            {
                if (!new FileInfo(tempFile).Exists)
                {
                    return false;
                }
                StreamWriter outputStream = new StreamWriter(tempFile, true);
                outputStream.WriteLine(cpuId);
                outputStream.Close();
                ftpUtil.Upload(tempFile);
            }
            
            
            return false;
        }

        public CPU_INFO GetCpuInfo()
        {
            CPU_INFO CpuInfo;
            CpuInfo = new CPU_INFO();
            GetSystemInfo(ref CpuInfo);

            return CpuInfo;
        }

        string GetCpuID()
        {
            return GetMacAddress();

            try
            {
                //获取CPU序列号代码
                string cpuInfo = "";//cpu序列号
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }


        void menu_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        

        ResourceVersionModel resourceMode = new ResourceVersionModel();
        private void Form1_Load(object sender, EventArgs e)
        {
            
            //resourceMode.parseNormalFlow("C:\\Users\\Administrator\\Desktop\\OneInstall\\config\\fx_android_ResourceVersion\\91\\fx_android_ResourceVersion.xml");
        }

        public void NormalXmlToInvalide(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                StreamReader reader = new StreamReader(path);

                StreamWriter writer = new StreamWriter(path + "_temp");

                string temp = "";
                while ((temp = reader.ReadLine()) != null)
                {
                    if (temp.Contains("&amp;"))
                    {
                        temp = temp.Replace( "&amp;","&");
                    }
                    writer.WriteLine(temp);
                }

                reader.Close();
                writer.Close();

                file.Delete();
                new FileInfo(path + "_temp").MoveTo(path);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ResourceList.Items.Count; i++ )
            {
                if (ResourceList.GetItemChecked(i))
                {
                    string itemName = ResourceList.Items[i].ToString();
                    if (checkConflictUse(itemName))
                    {
                        continue;
                    }

                    PlatformModel model = g_configXmlModel.m_Platforms[i];

                    //if (model.m_Type == "ios")
                    {
                        //NormalXmlToInvalide(new FileInfo("tmp" + model.m_Url).FullName);
                    }

                    FileInfo fileInfo = new FileInfo("tmp" + model.m_Url);
                    string dir = fileInfo.Directory.Parent.Parent.FullName + "/bak";
                    if (!new DirectoryInfo(dir).Exists)
                    {
                        new DirectoryInfo(dir).Create();
                    }

                    if (!new DirectoryInfo(dir + "/" + fileInfo.Directory.Name).Exists)
                    {
                        new DirectoryInfo(dir + "/" + fileInfo.Directory.Name).Create();
                    }

                    fileInfo.CopyTo(dir + "/" + fileInfo.Directory.Name + "/" + fileInfo.Name, true);

                    int index = model.m_Url.LastIndexOf('/'); 
                    string values = model.m_Url.Substring(1, index-1);
                    FtpUtil ftpUtil = new FtpUtil(g_configXmlModel.m_Ip + ":" + g_configXmlModel.m_Port, values, g_configXmlModel.m_User, g_configXmlModel.m_Pwd);
                    if (textBox_timeout.Text != "")
                    {
                        int timeout = -1;
                        int.TryParse(textBox_timeout.Text, out timeout);
                        if (timeout > 0)
                        {
                            ftpUtil.setTimeOut(timeout);
                        }
                    }
                    if (ftpUtil.Upload(new FileInfo("tmp" + model.m_Url).FullName))
                    {
                        ResourceList.SetItemChecked(i, false);
                        InvalidXmlToNormal(new FileInfo("tmp" + model.m_Url).FullName);
                    }
                    else
                    {
                        MessageBox.Show("文件上传失败： " + model.m_Url);
                    }
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filepath = Application.StartupPath + "\\tmp";
            DirectoryInfo info = new DirectoryInfo(filepath);
            if (info.Exists)
            {
                System.Diagnostics.Process.Start(filepath);
            }
            

        }


        private void button1_Click(object sender, EventArgs e)
        {
            FtpUtil ftpUtil = new FtpUtil(g_configXmlModel.m_Ip + ":" + g_configXmlModel.m_Port, "", g_configXmlModel.m_User, g_configXmlModel.m_Pwd);

            
            for (int i = 0; i < ResourceList.Items.Count; i++ )
            {
                int index = 0;
                if (ResourceList.GetItemChecked(i))
                {
                    index = i;
                }
                else
                {
                    continue;
                }

                string itemName = ResourceList.Items[i].ToString();
                if (checkConflictUse(itemName))
                {
                    continue;
                }

                PlatformModel model = g_configXmlModel.m_Platforms[index];
                string ftpPath = model.m_Url;
                FileInfo fileInfo = new FileInfo("tmp" + ftpPath);
                if (!fileInfo.Exists)
                {
                    fileInfo.Directory.Create();
                }
                
                if (ftpUtil.Download(fileInfo.FullName, ftpPath))
                {
                    //if (model.m_Type == "ios")
                    {
                        InvalidXmlToNormal(fileInfo.FullName);
                    }
                    ResourceList.Items[index] = model.m_Type + "_" + model.m_Name + "    √";
                    ResourceList.SetItemChecked(index, false);
                }
                else
                {
                    ResourceList.Items[index] = model.m_Type + "_" + model.m_Name + "    x";
                }
            }
        }

        public void InvalidXmlToNormal(string path)
        {
            FileInfo file = new FileInfo(path);
            string tempPath = path + "_temp";
            if (file.Exists)
            {
                StreamReader reader = new StreamReader(path);

                StreamWriter writer = new StreamWriter(tempPath);

                string temp = "";
                while ((temp = reader.ReadLine()) != null)
                {
                    if (temp.Contains("&") && !temp.Contains(";"))
                    {
                        temp = temp.Replace("&", "&amp;");
                    }
                    writer.WriteLine(temp);
                }

                writer.Close();
                reader.Close();

                //new FileInfo(path).Delete();//删除文件感觉不是同步的，导致立即访问这个文件有访问限制

                //new FileInfo(tempPath).MoveTo(path);
                CopyTo(tempPath, path);
            }
        }

        public void CopyTo(string from, string to)
        {
            StreamReader reader = new StreamReader(from);

            StreamWriter writer = new StreamWriter(to);

            string temp = "";
            while ((temp = reader.ReadLine()) != null)
            {
                writer.WriteLine(temp);
            }

            writer.Close();
            reader.Close();
        }

        public void ClearNormalBaseInfos()
        {
            NormalBaseFromText.Text = "";
            NormalBaseToText.Text = "";
            NormalBaseUrlText.Text = "";
            NormalBaseSizeText.Text = "";
            NormalBaseMd5Text.Text = "";

            NormalBaseMapMD5.Text = "";
            NormalBaseMapSize.Text = "";
            NormalBaseMapUrl.Text = "";

            DLLDownloadBox.Text = "";
            DLLMD5.Text = "";
            DLLSize.Text = "";
        }

        public void ClearNormalPatchInfos()
        {
            NormalPatchFromText.Text = "";
            NormalPatchToText.Text = "";
            NormalPatchUrlText.Text = "";
            NormalPatchSizeText.Text = "";
            NormalPatchMd5Text.Text = "";
        }


        public void ClearTestBaseInfos()
        {
            TestBaseFromText.Text = "";
            TestBaseToText.Text = "";
            TestBaseUrlText.Text = "";
            TestBaseSizeText.Text = "";
            TestBaseMD5Text.Text = "";

            TestBaseMapMD5.Text = "";
            TestBaseMapSize.Text = "";
            TestBaseMapUrl.Text = "";

            DLLTestDownloadBox.Text = "";
            DLLTestMD5.Text = "";
            DLLTestSize.Text = "";
        }

        public void ClearTestPatchInfos()
        {
            TestPatchFromText.Text = "";
            TestPatchToText.Text = "";
            TestPatchUrlText.Text = "";
            TestPatchSizeText.Text = "";
            TestPatchMD5Text.Text = "";
        }

        bool isResourceListSelectedChanged = false;
        int oldSelectedIndex = -1;
        private void ResourceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isResourceListSelectedChanged = true;
            CheckedListBox listBox = sender as CheckedListBox;
            if (listBox.SelectedIndex < 0)
            {
                isResourceListSelectedChanged = false;
                return;
            }

            if (oldSelectedIndex != listBox.SelectedIndex)
            {
                oldSelectedIndex = listBox.SelectedIndex;
                resourceMode.saveAll();
            }
            
            object obj = listBox.Items[listBox.SelectedIndex];
            string manifestTmpPath = "tmp" + g_configXmlModel.m_Platforms[listBox.SelectedIndex].m_Url;
            if (g_configXmlModel.m_Platforms[listBox.SelectedIndex].m_Type == "ios")
            {
                InvalidXmlToNormal(new FileInfo(manifestTmpPath).FullName);
            }
            resourceMode.parse(new FileInfo(manifestTmpPath).FullName, false);

            isResourceListSelectedChanged = false;
        }

        private void onFocusChanged(object sender, EventArgs e)
        {
            resourceMode.saveAll();
        }


#region 正式流程

        bool isSelectChangeCourceTextChange = false;

        private void NormalBaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }
            
            NormalFlow flow = resourceMode.m_NormalFlow;
            VersionModel versionMode = flow.baseModel[listBox.SelectedIndex];

            NormalBaseFromText.Text = versionMode.fromVersion;
            NormalBaseToText.Text = versionMode.toVersion;
            NormalBaseUrlText.Text = versionMode.resourceUrl;
            NormalBaseSizeText.Text = versionMode.fileSize;
            NormalBaseMd5Text.Text = versionMode.md5;

            NormalBaseMapUrl.Text = versionMode.map_url;
            NormalBaseMapMD5.Text = versionMode.map_md5;
            NormalBaseMapSize.Text = versionMode.map_size;

            isSelectChangeCourceTextChange = false;
        }

        private void NormalPatchList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            NormalFlow flow = resourceMode.m_NormalFlow;
            VersionModel versionMode = flow.patchModel[listBox.SelectedIndex];

            NormalPatchFromText.Text = versionMode.fromVersion;
            NormalPatchToText.Text = versionMode.toVersion;
            NormalPatchUrlText.Text = versionMode.resourceUrl;
            NormalPatchSizeText.Text = versionMode.fileSize;
            NormalPatchMd5Text.Text = versionMode.md5;

            isSelectChangeCourceTextChange = false;
        }

        private void NormalAppVersionText_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            NormalFlow flow = resourceMode.m_NormalFlow;
            flow.appVersion = tb.Text;
        }

        private void NormalAppUrlText_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            NormalFlow flow = resourceMode.m_NormalFlow;
            flow.url = tb.Text;
        }

        private void NormalAppSizeText_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            NormalFlow flow = resourceMode.m_NormalFlow;
            flow.appSize = tb.Text;
        }

        private void NormalBaseFromText_TextChanged(object sender, EventArgs e)
        {
            if (isSelectChangeCourceTextChange || NormalBaseList.SelectedIndex < 0)
            {
                return;
            }
            TextBox tb = sender as TextBox;

            NormalFlow flow = resourceMode.m_NormalFlow;
            VersionModel versionMode = flow.baseModel[NormalBaseList.SelectedIndex];

            versionMode.fromVersion = NormalBaseFromText.Text;
            versionMode.toVersion = NormalBaseToText.Text;
            versionMode.resourceUrl = NormalBaseUrlText.Text;
            versionMode.fileSize = NormalBaseSizeText.Text;
            versionMode.md5 = NormalBaseMd5Text.Text;

            versionMode.map_url = TestBaseMapUrl.Text;
            versionMode.map_md5 = TestBaseMapMD5.Text;
            versionMode.map_size = TestBaseMapSize.Text;

            NormalBaseList.Items[NormalBaseList.SelectedIndex] = NormalBaseFromText.Text + " -> " + NormalBaseToText.Text;
        }

        private void NormalPatchFromText_TextChanged(object sender, EventArgs e)
        {
            if (isSelectChangeCourceTextChange || NormalPatchList.SelectedIndex < 0)
            {
                return;
            }

            TextBox tb = sender as TextBox;

            NormalFlow flow = resourceMode.m_NormalFlow;
            VersionModel versionMode = flow.patchModel[NormalPatchList.SelectedIndex];

            versionMode.fromVersion = NormalPatchFromText.Text;
            versionMode.toVersion = NormalPatchToText.Text;
            versionMode.resourceUrl = NormalPatchUrlText.Text;
            versionMode.fileSize = NormalPatchSizeText.Text;
            versionMode.md5 = NormalPatchMd5Text.Text;
            NormalPatchList.Items[NormalPatchList.SelectedIndex] = NormalPatchFromText.Text + " -> " + NormalPatchToText.Text;
        }

#region 拖动文件到listbox里面，添加新的version项


        private void NormalBaseListDragDrop(object sender, DragEventArgs e)
        {
            NormalFlow flow = resourceMode.m_NormalFlow;

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                VersionModel model = new VersionModel();
                model.fileSize = fileSize;
                model.fromVersion = "null";
                model.toVersion = "null";
                model.resourceUrl = file;
                model.md5 = md5;

                flow.baseModel.Add(model);

                NormalBaseList.Items.Add(model.fromVersion + " -> " + model.toVersion);
            }
        }

        private void NormalBaseListDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private void NormalPatchListDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private void NormalPatchListDragDrop(object sender, DragEventArgs e)
        {
            NormalFlow flow = resourceMode.m_NormalFlow;

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                VersionModel model = new VersionModel();
                model.fileSize = fileSize;
                model.fromVersion = "null";
                model.toVersion = "null";
                model.resourceUrl = file;
                model.md5 = md5;

                flow.patchModel.Add(model);

                NormalPatchList.Items.Add(model.fromVersion + " -> " + model.toVersion);
            }
        }
#endregion

        private void BaseListDeleteKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                NormalFlow flow = resourceMode.m_NormalFlow;
                flow.baseModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.baseModel.Count == 0)
                {
                    flow.setText();
                }
            }
            
        }

        private void NormalPatchListDeleteKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                NormalFlow flow = resourceMode.m_NormalFlow;
                flow.patchModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.patchModel.Count == 0)
                {
                    flow.setText();
                }
            }
        }
#endregion

#region 测试流程


        private void TestVersionBaseList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.baseModel[listBox.SelectedIndex];

            TestBaseFromText.Text = versionMode.fromVersion;
            TestBaseToText.Text = versionMode.toVersion;
            TestBaseUrlText.Text = versionMode.resourceUrl;
            TestBaseSizeText.Text = versionMode.fileSize;
            TestBaseMD5Text.Text = versionMode.md5;

            TestBaseMapUrl.Text = versionMode.map_url;
            TestBaseMapMD5.Text = versionMode.map_md5;
            TestBaseMapSize.Text = versionMode.map_size;

            isSelectChangeCourceTextChange = false;
        }

        private void TestVersionPatchList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.patchModel[listBox.SelectedIndex];

            TestPatchFromText.Text = versionMode.fromVersion;
            TestPatchToText.Text = versionMode.toVersion;
            TestPatchUrlText.Text = versionMode.resourceUrl;
            TestPatchSizeText.Text = versionMode.fileSize;
            TestPatchMD5Text.Text = versionMode.md5;

            isSelectChangeCourceTextChange = false;
        }

        private void TestBaseInfoChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            string boxName = tb.Name;
            string boxValue = tb.Text;

            TestFlow flow = resourceMode.m_TestFlow;
            if (boxName == "TestAppUrlText")
            {
                flow.url = boxValue;
            }
            if (boxName == "TestAppSizeText")
            {
                flow.appSize = boxValue;
            }
            if (boxName == "TestCurrentVersionText")
            {
                flow.appVersion = boxValue;
            }
            if (boxName == "TestBigVersionText")
            {
                flow.bigVersion = boxValue;
            }
            if (boxName == "TestSmallVersionText")
            {
                flow.smallVersion = boxValue;
            }
        }

        private void TestBaseFromChanged(object sender, EventArgs e)
        {
            if (isSelectChangeCourceTextChange || TestVersionBaseList.SelectedIndex < 0)
            {
                return;
            }
            TextBox tb = sender as TextBox;

            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.baseModel[TestVersionBaseList.SelectedIndex];

            versionMode.fromVersion = TestBaseFromText.Text;
            versionMode.toVersion = TestBaseToText.Text;
            versionMode.resourceUrl = TestBaseUrlText.Text;
            versionMode.fileSize = TestBaseSizeText.Text;
            versionMode.md5 = TestBaseMD5Text.Text;

            versionMode.map_url = TestBaseMapUrl.Text;
            versionMode.map_md5 = TestBaseMapMD5.Text;
            versionMode.map_size = TestBaseMapSize.Text;

            TestVersionBaseList.Items[TestVersionBaseList.SelectedIndex] = TestBaseFromText.Text + " -> " + TestBaseToText.Text;
        }

        private void TestPatchFromChanged(object sender, EventArgs e)
        {
            if (isSelectChangeCourceTextChange || TestVersionPatchList.SelectedIndex < 0)
            {
                return;
            }
            TextBox tb = sender as TextBox;

            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.patchModel[TestVersionPatchList.SelectedIndex];

            versionMode.fromVersion = TestPatchFromText.Text;
            versionMode.toVersion = TestPatchToText.Text;
            versionMode.resourceUrl = TestPatchUrlText.Text;
            versionMode.fileSize = TestPatchSizeText.Text;
            versionMode.md5 = TestPatchMD5Text.Text;
            TestVersionPatchList.Items[TestVersionPatchList.SelectedIndex] = TestPatchFromText.Text + " -> " + TestPatchToText.Text;
        }

        private void TestBaseListDelete(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                TestFlow flow = resourceMode.m_TestFlow;
                flow.baseModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.baseModel.Count == 0)
                {
                    flow.setText();
                }
            }
        }

        private void TestPatchListDelete(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                TestFlow flow = resourceMode.m_TestFlow;
                flow.patchModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.patchModel.Count == 0)
                {
                    flow.setText();
                }
            }
        }

        private void TestBaseListDragDrop(object sender, DragEventArgs e)
        {
            TestFlow flow = resourceMode.m_TestFlow;

            string urlHead = null;
            string originalUrl = flow.baseModel[0].resourceUrl;
            if (flow.baseModel.Count > 0 && originalUrl != null && originalUrl.Contains("/"))
            {
                urlHead = originalUrl.Substring(0, originalUrl.LastIndexOf("/"));
            }

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                VersionModel model = new VersionModel();
                model.fileSize = fileSize;
                int version = -1;
                if(int.TryParse(file.Substring(file.LastIndexOf('_') + 1), out version))
                {
                    model.fromVersion = "" + (version -1);
                    model.toVersion = "" + version;
                }
                else
                {
                    model.fromVersion = "null";
                    model.toVersion = "null";
                }

                if (urlHead != null)
                {
                    model.resourceUrl = urlHead + "/" + fileInfo.Name;
                }
                else
                {
                    model.resourceUrl = file;
                }
                model.md5 = md5;

                TestBaseMapOnDrop(file + ".map", model);

                flow.baseModel.Add(model);

                TestVersionBaseList.Items.Add(model.fromVersion + " -> " + model.toVersion);
            }
        }

        private void TestBaseMapOnDrop(string file, VersionModel versionMode)
        {
            if (File.Exists(file) == false)
            {
                return;
            }

            TestFlow flow = resourceMode.m_TestFlow;

            FileInfo fileInfo = new FileInfo(file);
            string fileSize = "" + fileInfo.Length;
            string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

            versionMode.map_size = "" + fileSize;
            versionMode.map_url = versionMode.resourceUrl.Substring(0, versionMode.resourceUrl.LastIndexOf('/')) + "/" + fileInfo.Name;
            versionMode.map_md5 = "" + md5;

            //TestBaseMapMD5.Text = versionMode.map_md5;
            //TestBaseMapSize.Text = versionMode.map_size;
            //TestBaseMapUrl.Text = versionMode.map_url;
        }

        private void TestPatchListDragDrop(object sender, DragEventArgs e)
        {
            TestFlow flow = resourceMode.m_TestFlow;

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                VersionModel model = new VersionModel();
                model.fileSize = fileSize;
                model.fromVersion = "null";
                model.toVersion = "null";
                model.resourceUrl = file;
                model.md5 = md5;

                flow.patchModel.Add(model);

                TestVersionPatchList.Items.Add(model.fromVersion + " -> " + model.toVersion);
            }
        }
#endregion

        private void NoticeTextChanged(object sender, EventArgs e)
        {
            if (isResourceListSelectedChanged)
            {
                return;
            }
            TextBox box = sender as TextBox;
            string value = box.Text;
        }

        private void WhiteAccountTextChanged(object sender, EventArgs e)
        {
            if (isResourceListSelectedChanged)
            {
                return;
            }

            string[] imeis = IMEIText.Text.Split('\n');
            string[] ips = MACText.Text.Split('\n');
            string[] users = WhiteListText.Text.Split('\n');

            if (imeis != null)
            {
                resourceMode.m_WhiteListModel.imeiList.Clear();
                resourceMode.m_WhiteListModel.imeiList.AddRange(imeis);
            }

            if (ips != null)
            {
                resourceMode.m_WhiteListModel.macList.Clear();
                resourceMode.m_WhiteListModel.macList.AddRange(ips);
            }

            if (users != null)
            {
                resourceMode.m_WhiteListModel.accountList.Clear();
                resourceMode.m_WhiteListModel.accountList.AddRange(users);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TestVersionPatchList.Focus();
            int index = TestVersionPatchList.SelectedIndex;
            if (index == -1)
            {
                contextMenuStrip1.Items[0].Enabled = false;
                
            }
            else
            {
                contextMenuStrip1.Items[0].Enabled = true;
            }

            if (m_VersionModeForCopy == null)
            {
                contextMenuStrip1.Items[2].Enabled = false;
            }
            else
            {
                contextMenuStrip1.Items[2].Enabled = true;
            }
        }

        private void PatchMenuItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            string clickedItem = e.ClickedItem.Text;
            int index = TestVersionPatchList.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            VersionModel model = resourceMode.m_TestFlow.patchModel[index];

            if (clickedItem == "拷贝到正式流程")
            {
                bool contains = false;
                foreach (VersionModel patchModel in resourceMode.m_NormalFlow.patchModel)
                {
                    if ((model.fromVersion == patchModel.fromVersion) && model.toVersion == patchModel.toVersion)
                    {
                        patchModel.resourceUrl = model.resourceUrl;
                        patchModel.md5 = model.md5;
                        patchModel.fileSize = model.fileSize;
                        contains = true;

                        break;
                    }
                }

                if (!contains)
                {
                    resourceMode.m_NormalFlow.patchModel.Add(model);
                }

                resourceMode.m_NormalFlow.setText();
            }

            if (clickedItem == "复制")
            {
                m_VersionModeForCopy = model.clone();
            }

            if (clickedItem == "粘贴")
            {
                model = m_VersionModeForCopy.clone();

                bool contains = false;
                foreach (VersionModel patchModel in resourceMode.m_TestFlow.patchModel)
                {
                    if ((model.fromVersion == patchModel.fromVersion) && model.toVersion == patchModel.toVersion)
                    {
                        patchModel.resourceUrl = model.resourceUrl;
                        patchModel.md5 = model.md5;
                        patchModel.fileSize = model.fileSize;
                        patchModel.map_md5 = model.map_md5;
                        patchModel.map_size = model.map_size;
                        patchModel.map_url = model.map_url;
                        contains = true;

                        break;
                    }
                }

                if (!contains)
                {
                    resourceMode.m_TestFlow.patchModel.Add(model);
                }

                resourceMode.m_TestFlow.setText();

                PasteAllModel(false, model);
            }
        }

        private void PasteAllModel(bool isBase, VersionModel model)
        {
            for (int i = 0; i < ResourceList.Items.Count; i++)
            {
                if (ResourceList.GetItemChecked(i))
                {
                    string manifestTmpPath = "tmp" + g_configXmlModel.m_Platforms[i].m_Url;
                    if (g_configXmlModel.m_Platforms[i].m_Type == "ios")
                    {
                        InvalidXmlToNormal(new FileInfo(manifestTmpPath).FullName);
                    }

                    ResourceVersionModel tempRmodel = new ResourceVersionModel();
                    tempRmodel.parse(new FileInfo(manifestTmpPath).FullName, true);

                    bool contains = false;
                    List<VersionModel> tempModels = null;
                    if (isBase)
                    {
                        tempModels = tempRmodel.m_TestFlow.baseModel;
                    }
                    else
                    {
                        tempModels = tempRmodel.m_TestFlow.patchModel;
                    }

                    foreach (VersionModel patchModel in tempModels)
                    {
                        if ((model.fromVersion == patchModel.fromVersion) && model.toVersion == patchModel.toVersion)
                        {
                            patchModel.resourceUrl = model.resourceUrl;
                            patchModel.md5 = model.md5;
                            patchModel.fileSize = model.fileSize;
                            patchModel.map_md5 = model.map_md5;
                            patchModel.map_size = model.map_size;
                            patchModel.map_url = model.map_url;
                            contains = true;

                            break;
                        }
                    }

                    if (!contains)
                    {
                        tempModels.Add(model);
                    }

                    tempRmodel.saveAll();
                }
            }
        }

        private void BaseMenuItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            string clickedItem = e.ClickedItem.Text;

            int index = TestVersionBaseList.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            VersionModel model = resourceMode.m_TestFlow.baseModel[index];

            if (clickedItem == "拷贝到正式流程")
            {
                bool contains = false;
                foreach (VersionModel patchModel in resourceMode.m_NormalFlow.baseModel)
                {
                    if ((model.fromVersion == patchModel.fromVersion) && model.toVersion == patchModel.toVersion)
                    {
                        patchModel.resourceUrl = model.resourceUrl;
                        patchModel.md5 = model.md5;
                        patchModel.fileSize = model.fileSize;
                        patchModel.map_url = model.map_url;
                        patchModel.map_md5 = model.map_md5;
                        patchModel.map_size = model.map_size;
                        contains = true;

                        break;
                    }
                }

                if (!contains)
                {
                    resourceMode.m_NormalFlow.baseModel.Add(model);
                }

                resourceMode.m_NormalFlow.setText();
            }
            
            if (clickedItem == "复制")
            {
                m_VersionModeForCopy = model.clone();
            }

            if (clickedItem == "粘贴")
            {
                model = m_VersionModeForCopy.clone();

                bool contains = false;
                foreach (VersionModel patchModel in resourceMode.m_TestFlow.baseModel)
                {
                    if ((model.fromVersion == patchModel.fromVersion) && model.toVersion == patchModel.toVersion)
                    {
                        patchModel.resourceUrl = model.resourceUrl;
                        patchModel.md5 = model.md5;
                        patchModel.fileSize = model.fileSize;
                        patchModel.map_md5 = model.map_md5;
                        patchModel.map_size = model.map_size;
                        patchModel.map_url = model.map_url;
                        contains = true;

                        break;
                    }
                }

                if (!contains)
                {
                    resourceMode.m_TestFlow.baseModel.Add(model);
                }

                resourceMode.m_TestFlow.setText();

                PasteAllModel(true, model);
            }

            
        }

        private void contextMenuStrip2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TestVersionBaseList.Focus();
            int index = TestVersionBaseList.SelectedIndex;
            if (index == -1)
            {
                contextMenuStrip2.Items[0].Enabled = false;

            }
            else
            {
                contextMenuStrip2.Items[0].Enabled = true;
            }

            if (m_VersionModeForCopy == null)
            {
                contextMenuStrip2.Items[2].Enabled = false;
            }
            else
            {
                contextMenuStrip2.Items[2].Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string cpuId = GetCpuID();
            string tempFile = "tmp/" + s_confict_file_name;
            FileInfo fileInfo = new FileInfo(tempFile);
            if (!fileInfo.Exists)
            {
                fileInfo.Directory.Create();
            }

            try
            {
                FtpUtil ftpUtil = new FtpUtil(g_configXmlModel.m_Ip + ":" + g_configXmlModel.m_Port, "", g_configXmlModel.m_User, g_configXmlModel.m_Pwd);
                ftpUtil.Download(tempFile, s_confict_file_name);
                StreamReader reader = new StreamReader(tempFile);
                List<string> list = new List<string>();
                string tempStr = null;
                while ((tempStr = reader.ReadLine()) != null)
                {
                    bool isContains = false;
                    for (int i = 0; i < ResourceList.Items.Count; i++)
                    {
                        string itemName = ResourceList.Items[i].ToString().Replace("x", "").Replace("√", "").Trim() + cpuId;
                        if (itemName == tempStr)
                        {
                            isContains = true;
                            break;
                        }
                    }
                    if (!isContains)
                    {
                        list.Add(tempStr);
                    }
                }
                reader.Close();

                StreamWriter outputStream = new StreamWriter(tempFile);
                foreach (string value in list)
                {
                    outputStream.WriteLine(value);
                }
                outputStream.Close();
                ftpUtil.Upload(tempFile);
            }
            catch (System.Exception ex)
            {
            	
            }
           
            

            for (int i = 0; i < ResourceList.Items.Count; i++)
            {
                if (ResourceList.GetItemChecked(i))
                {
                    if (MessageBox.Show("有选中的资源没有上传，是否退出程序", "退出", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    {
                        //Close();
                        e.Cancel = true;
                    }
                    
                    return;
                }
            }
        }


        private void TestBaseMapOnDrop(object sender, DragEventArgs e)
        {
            int index = TestVersionBaseList.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            isSelectChangeCourceTextChange = true;
            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.baseModel[index];


            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                versionMode.map_size = "" + fileSize;
                versionMode.map_url = versionMode.resourceUrl.Substring(0,versionMode.resourceUrl.LastIndexOf('/')) + "/" + fileInfo.Name;
                versionMode.map_md5 = "" + md5;
            }

            TestBaseMapMD5.Text = versionMode.map_md5;
            TestBaseMapSize.Text = versionMode.map_size;
            TestBaseMapUrl.Text = versionMode.map_url;

            isSelectChangeCourceTextChange = false;
        }

        private void TestBaseMapOnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private List<VersionModel> CloneList(List<VersionModel> sourceList)
        {
            List<VersionModel> retList = new List<VersionModel>();
            for (int i = 0; i < sourceList.Count; ++i)
            {
                retList.Add(sourceList[i].clone());
            }

            return retList;
        }

        private void copy_base_set_Click(object sender, EventArgs e)
        {
            copy_notice.Text = "开始拷贝。。。";
            NormalAppVersionText.Text = TestBigVersionText.Text;
            NormalAppUrlText.Text = TestAppUrlText.Text;
            NormalAppSizeText.Text = TestAppSizeText.Text;
            NormalResVersionBox.Text = TestResVersion.Text;

            if (resourceMode.m_TestFlow.baseModel.Count > 0)
            {
                resourceMode.m_NormalFlow.baseModel.Clear();
                resourceMode.m_NormalFlow.baseModel.AddRange(CloneList(resourceMode.m_TestFlow.baseModel));
            }

            if (resourceMode.m_TestFlow.patchModel.Count > 0)
            {
                resourceMode.m_NormalFlow.patchModel.Clear();
                resourceMode.m_NormalFlow.patchModel.AddRange(CloneList(resourceMode.m_TestFlow.patchModel));
            }

            resourceMode.m_NormalFlow.dllModel.Clear();
            resourceMode.m_NormalFlow.dllModel.AddRange(CloneList(resourceMode.m_TestFlow.dllModel));

            resourceMode.m_NormalFlow.forceUpdate = resourceMode.m_TestFlow.forceUpdate;

            resourceMode.m_NormalFlow.setText();


            string[] users = WhiteListText.Text.Split('\n');
            WhiteListText.Clear();

            if (users != null)
            {
                string tempUser = "";
                foreach (string user in users)
                {
                    tempUser = user;
                    if (user.Contains(":true") && !user.StartsWith("//"))
                    {
                        tempUser = "//" + tempUser;
                    }

                    WhiteListText.AppendText(tempUser + "\n");
                }
            }


            copy_notice.Text = "拷贝完毕！";

            resourceMode.saveAll();
        }

        private void copy_base_set_MouseEnter(object sender, EventArgs e)
        {
            copy_notice.Text = "......";
        }

        private void TestAppUrlText_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;

                string originalText = TestAppUrlText.Text;
                if(originalText != "")
                {
                    originalText = originalText.Substring(0, originalText.LastIndexOf("/"));
                }

                string[] subs = fileInfo.Name.Split('_');
                string appVersion = null;
                if (subs != null)
                {
                    foreach (string str in subs)
                    {
                        if (str.ToLower().StartsWith("v"))
                        {
                            string version = str.ToLower().Replace("v", "").Replace(".", "");
                            int iVer = 0;
                            if (int.TryParse(version, out iVer))
                            {
                                appVersion = str.ToLower().Replace("v", "");
                                break;
                            }
                        }
                    }
                }

                TestAppUrlText.Text = originalText + "/" + fileInfo.Name;
                TestAppSizeText.Text = fileSize;

                if (appVersion != null)
                {
                    if (TestBigVersionText.Text != appVersion)
                    {
                        TestSmallVersionText.Text = TestBigVersionText.Text;
                        TestBaseInfoChanged(TestSmallVersionText, null);
                    }
                    TestBigVersionText.Text = appVersion;
                    TestAppUrlText.Text = originalText.Replace("v" + TestCurrentVersionText.Text, "v" + appVersion) + "/" + fileInfo.Name;
                    TestCurrentVersionText.Text = appVersion;

                    TestBaseInfoChanged(TestBigVersionText, null);
                    TestBaseInfoChanged(TestCurrentVersionText, null);
                }

                TestBaseInfoChanged(TestAppSizeText, null);
                TestBaseInfoChanged(TestAppUrlText, null);
                break;
            }

            onFocusChanged(null, null);
        }

        private void TestAppUrlText_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None;
        }

        private void connectModel_CheckedChanged(object sender, EventArgs e)
        {
            USE_DEFAULT_CONNECT_MODEL = connectModel.Checked;
        }

        private void DLLCopyBtn_Click(object sender, EventArgs e)
        {
            resourceMode.m_NormalFlow.dllModel.Clear();
            resourceMode.m_NormalFlow.dllModel.AddRange(CloneList(resourceMode.m_TestFlow.dllModel));
            DllUpdateListBox.Items.Clear();
            foreach (var item in resourceMode.m_NormalFlow.dllModel)
            {
                DllUpdateListBox.Items.Add(item.fromVersion);
            }
            if (DllUpdateListBox.Items.Count > 0)
            {
                DllUpdateListBox.SelectedIndex = 0;
            }
        }

        private void DLLUpdateTestListBox_DragDrop(object sender, DragEventArgs e)
        {
            TestFlow flow = resourceMode.m_TestFlow;

            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in filePath)
            {
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = "" + fileInfo.Length;
                string md5 = ResourceConfig.Util.FileUtil.GetMD5HashFromFile(file);

                VersionModel model = new VersionModel();
                model.fileSize = fileSize;
                model.fromVersion = fileInfo.Name;
                model.toVersion = "";
                model.resourceUrl = file;
                model.md5 = md5;

                flow.dllModel.Add(model);
                DLLUpdateTestListBox.Items.Add(model.fromVersion);
            }

            DLLUpdateTestListBox.SelectedIndex = 0;
        }

        private void DLLUpdateTestListBox_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void DLLUpdateTestListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            TestFlow flow = resourceMode.m_TestFlow;
            VersionModel versionMode = flow.dllModel[listBox.SelectedIndex];

            DLLTestDownloadBox.Text = versionMode.resourceUrl;
            DLLTestSize.Text = versionMode.fileSize;
            DLLTestMD5.Text = versionMode.md5;
            isSelectChangeCourceTextChange = false;
        }

        private void DLLUpdateTestListBox_Leave(object sender, EventArgs e)
        {
            onFocusChanged(sender, e);
        }

        private void DllUpdateListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            isSelectChangeCourceTextChange = true;
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex < 0)
            {
                return;
            }

            NormalFlow flow = resourceMode.m_NormalFlow;
            VersionModel versionMode = flow.dllModel[listBox.SelectedIndex];

            DLLDownloadBox.Text = versionMode.resourceUrl;
            DLLSize.Text = versionMode.fileSize;
            DLLMD5.Text = versionMode.md5;
            isSelectChangeCourceTextChange = false;
        }

        private void ForceUpdateAppTestCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resourceMode.m_TestFlow.forceUpdate = ForceUpdateAppTestCheckBox.Checked;
            resourceMode.saveAll();
        }

        private void ForceUpdateAppCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resourceMode.m_NormalFlow.forceUpdate = ForceUpdateAppCheckBox.Checked;
            resourceMode.saveAll();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PasteAllDLLModel(resourceMode.m_NormalFlow.dllModel, false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PasteAllDLLModel(resourceMode.m_TestFlow.dllModel);
        }

        private void PasteAllDLLModel(List<VersionModel> dllModel, bool testFlow = true)
        {
            for (int i = 0; i < ResourceList.Items.Count; i++)
            {
                if (ResourceList.GetItemChecked(i))
                {
                    string manifestTmpPath = "tmp" + g_configXmlModel.m_Platforms[i].m_Url;
                    if (g_configXmlModel.m_Platforms[i].m_Type == "ios")
                    {
                        InvalidXmlToNormal(new FileInfo(manifestTmpPath).FullName);
                    }

                    ResourceVersionModel tempRmodel = new ResourceVersionModel();
                    tempRmodel.parse(new FileInfo(manifestTmpPath).FullName, true);
                    if (testFlow)
                    {
                        tempRmodel.m_TestFlow.dllModel = dllModel;
                    }
                    else
                    {
                        tempRmodel.m_NormalFlow.dllModel = dllModel;
                    }
                    tempRmodel.saveAll();
                }
            }
        }

        private void TestDLLTabTextChanged(object sender, EventArgs e)
        {
            if (DLLUpdateTestListBox.Items.Count == 0)
            {
                return;
            }
            var model = resourceMode.m_TestFlow.dllModel[DLLUpdateTestListBox.SelectedIndex];

            var tb = sender as TextBox;
            if (tb.Name == "DLLTestDownloadBox")
            {
                model.resourceUrl = DLLTestDownloadBox.Text;
            }
            if (tb.Name == "DLLTestMD5")
            {
                model.md5 = DLLTestMD5.Text;
            }
            if (tb.Name == "DLLTestSize")
            {
                model.fileSize = DLLTestSize.Text;
            }
        }

        private void NormalDLLTabTextChanged(object sender, EventArgs e)
        {
            if (DllUpdateListBox.Items.Count == 0)
            {
                return;
            }
            var model = resourceMode.m_NormalFlow.dllModel[DllUpdateListBox.SelectedIndex];
            var tb = sender as TextBox;
            if (tb.Name == "DLLDownloadBox")
            {
                model.resourceUrl = DLLDownloadBox.Text;
            }
            if (tb.Name == "DLLMD5")
            {
                model.md5 = DLLMD5.Text;
            }
            if (tb.Name == "DLLSize")
            {
                model.fileSize = DLLSize.Text;
            }
        }

        private void DLLUpdateTestListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                TestFlow flow = resourceMode.m_TestFlow;
                flow.dllModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.dllModel.Count == 0)
                {
                    DLLTestDownloadBox.Text = "";
                    DLLTestSize.Text = "";
                    DLLTestMD5.Text = "";
                }
            }
        }

        private void DllUpdateListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.SelectedIndex < 0)
                {
                    return;
                }

                int selectIndex = listBox.SelectedIndex;

                NormalFlow flow = resourceMode.m_NormalFlow;
                flow.dllModel.RemoveAt(selectIndex);
                listBox.Items.RemoveAt(selectIndex);

                if (selectIndex > 0)
                {
                    listBox.SelectedIndex = selectIndex - 1;
                }
                if (selectIndex == 0 && listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                if (flow.dllModel.Count == 0)
                {
                    DLLDownloadBox.Text = "";
                    DLLSize.Text = "";
                    DLLMD5.Text = "";
                }
            }
        }
        
        private void resVersionBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            NormalFlow flow = resourceMode.m_NormalFlow;
            flow.resVersion = tb.Text;
        }

        private void TestResVersion_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            TestFlow flow = resourceMode.m_TestFlow;
            flow.resVersion = tb.Text;
        }
    }
}
