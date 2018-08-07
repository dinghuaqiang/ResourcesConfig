using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace ResourceConfig.Model
{
    class ResourceVersionModel
    {
        public bool b_IsUpdateSuccess; // 是否更新成功
        public bool b_IsModified;//是否有改动

        public string m_TempManifestPath;

        public NormalFlow m_NormalFlow;
        public TestFlow m_TestFlow;
        public WhiteListModel m_WhiteListModel;

        public void clean()
        {
            b_IsModified = false;
            b_IsUpdateSuccess = false;
        }

        public void parseNormalFlow(string manifestFile, bool forPaste)
        {
            m_TempManifestPath = manifestFile;

            if (m_NormalFlow != null)
            {
                m_NormalFlow.clean();
            }
            else
            {
                m_NormalFlow = new NormalFlow();
            }


            FileInfo fileInfo = new FileInfo(manifestFile);
            if (!fileInfo.Exists)
            {
                //MessageBox.Show("The file " + manifestFile + " is not exists!" );
                Form1.g_Context.ErrorMsg.Text = "The file " + manifestFile + " is not exists!";


                if (!forPaste)
                {
                    //m_NormalFlow.setText();
                }
                return;
            }
            
            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(manifestFile);

                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion");
                XmlNode ForceUpdateNode = RequireNode(dom, rootNode, "ForceUpdate", "true");
                Boolean.TryParse(ForceUpdateNode.InnerText, out m_NormalFlow.forceUpdate);

                XmlNodeList baseList = dom.SelectNodes("ResourceVersion/VersionBase");
                foreach (XmlNode baseNode in baseList)
                {
                    XmlNode fromNode = baseNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = baseNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = baseNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = baseNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = baseNode.SelectSingleNode("FileSize");
                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    XmlNode mapFile = baseNode.SelectSingleNode("mapFile");
                    XmlNode mapFileMD5 = baseNode.SelectSingleNode("mapFileMD5");
                    XmlNode mapFileSize = baseNode.SelectSingleNode("mapFileSize");

                    if (mapFile != null)
                    {
                        model.map_url = mapFile.InnerText;
                        model.map_md5 = mapFileMD5.InnerText;
                        model.map_size = mapFileSize.InnerText;
                    }

                    m_NormalFlow.baseModel.Add(model);
                }

                XmlNodeList patchList = dom.SelectNodes("ResourceVersion/VersionPatch");
                foreach (XmlNode patchNode in patchList)
                {
                    XmlNode fromNode = patchNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = patchNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = patchNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = patchNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = patchNode.SelectSingleNode("FileSize");

                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    m_NormalFlow.patchModel.Add(model);
                }

                XmlNodeList dllList = dom.SelectNodes("ResourceVersion/VersionDll");
                foreach (XmlNode patchNode in dllList)
                {
                    XmlNode fromNode = patchNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = patchNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = patchNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = patchNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = patchNode.SelectSingleNode("FileSize");

                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    m_NormalFlow.dllModel.Add(model);
                }

                XmlNode versionNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/Version");
                XmlNode resVersionNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/ResVersion");
                XmlNode urlNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/url");
                XmlNode sizeNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/size");

                m_NormalFlow.appVersion = versionNode.InnerText;
                m_NormalFlow.resVersion = resVersionNode.InnerText;
                m_NormalFlow.url = urlNode.InnerText;
                if (sizeNode != null)
                {
                    m_NormalFlow.appSize = sizeNode.InnerText;
                }

                if (!forPaste)
                {
                    //m_NormalFlow.setText();
                }
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Form1.g_Context.ErrorMsg.Text = ex.Message;
            }
            
        }

        public void parseTestFlow(string manifestFile, bool forPaste)
        {
            m_TempManifestPath = manifestFile;

            if (m_TestFlow != null)
            {
                m_TestFlow.clean();
            }
            else
            {
                m_TestFlow = new TestFlow();
            }


            FileInfo fileInfo = new FileInfo(manifestFile);
            if (!fileInfo.Exists)
            {
                //MessageBox.Show("The file " + manifestFile + " is not exists!" );
                Form1.g_Context.ErrorMsg.Text = "The file " + manifestFile + " is not exists!";


                if (!forPaste)
                {
                    //m_TestFlow.setText();
                }
                return;
            }

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(manifestFile);

                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion/test_tag");
                XmlNode ForceUpdateNode = RequireNode(dom, rootNode, "ForceUpdate", "true");
                Boolean.TryParse(ForceUpdateNode.InnerText, out m_TestFlow.forceUpdate);

                XmlNodeList baseList = dom.SelectNodes("ResourceVersion/test_tag/VersionBase");
                foreach (XmlNode baseNode in baseList)
                {
                    XmlNode fromNode = baseNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = baseNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = baseNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = baseNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = baseNode.SelectSingleNode("FileSize");

                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    XmlNode mapFile = baseNode.SelectSingleNode("mapFile");
                    XmlNode mapFileMD5 = baseNode.SelectSingleNode("mapFileMD5");
                    XmlNode mapFileSize = baseNode.SelectSingleNode("mapFileSize");

                    if (mapFile != null)
                    {
                        model.map_url = mapFile.InnerText;
                        model.map_md5 = mapFileMD5.InnerText;
                        model.map_size = mapFileSize.InnerText;
                    }

                    m_TestFlow.baseModel.Add(model);
                }

                XmlNodeList patchList = dom.SelectNodes("ResourceVersion/test_tag/VersionPatch");
                foreach (XmlNode patchNode in patchList)
                {
                    XmlNode fromNode = patchNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = patchNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = patchNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = patchNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = patchNode.SelectSingleNode("FileSize");

                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    m_TestFlow.patchModel.Add(model);
                }

                XmlNodeList dllList = dom.SelectNodes("ResourceVersion/test_tag/VersionDll");
                foreach (XmlNode patchNode in dllList)
                {
                    XmlNode fromNode = patchNode.SelectSingleNode("FromVersion");
                    XmlNode ToVersion = patchNode.SelectSingleNode("ToVersion");
                    XmlNode PatchFile = patchNode.SelectSingleNode("PatchFile");
                    XmlNode PatchFileMD5 = patchNode.SelectSingleNode("PatchFileMD5");
                    XmlNode FileSize = patchNode.SelectSingleNode("FileSize");

                    VersionModel model = new VersionModel();
                    model.fileSize = FileSize.InnerText;
                    model.fromVersion = fromNode.InnerText;
                    model.toVersion = ToVersion.InnerText;
                    model.md5 = PatchFileMD5.InnerText;
                    model.resourceUrl = PatchFile.InnerText;

                    m_TestFlow.dllModel.Add(model);
                }

                XmlNode big_app_version = dom.SelectSingleNode("ResourceVersion/test_tag/big_app_version");
                XmlNode small_app_version = dom.SelectSingleNode("ResourceVersion/test_tag/small_app_version");
                XmlNode app_current_version = dom.SelectSingleNode("ResourceVersion/test_tag/app_current_version");
                XmlNode app_res_version = dom.SelectSingleNode("ResourceVersion/test_tag/app_res_version");
                XmlNode app_update_url = dom.SelectSingleNode("ResourceVersion/test_tag/app_update_url");
                XmlNode test_size = dom.SelectSingleNode("ResourceVersion/test_tag/test_size");
                
                m_TestFlow.url = app_update_url.InnerText;
                if (test_size != null)
                {
                    m_TestFlow.appSize = test_size.InnerText;
                }

                m_TestFlow.appVersion = app_current_version.InnerText;
                m_TestFlow.resVersion = app_res_version.InnerText;
                m_TestFlow.bigVersion = big_app_version.InnerText;
                m_TestFlow.smallVersion = small_app_version.InnerText;

                
                if (!forPaste)
                {
                    //m_TestFlow.setText();
                }
            }
            catch (System.Exception ex)
            {
                Form1.g_Context.ErrorMsg.Text = ex.Message + ex.Source.ToString();
            }
        }

        public void parseNoticeFlow(string manifestFile, bool forPaste)
        {
            m_TempManifestPath = manifestFile;
            FileInfo fileInfo = new FileInfo(manifestFile);
            if (!fileInfo.Exists)
            {
                //MessageBox.Show("The file " + manifestFile + " is not exists!" );
                Form1.g_Context.ErrorMsg.Text = "The file " + manifestFile + " is not exists!";


                if (!forPaste)
                {
                    //m_NoticeModel.setText();
                }
                return;
            }

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(manifestFile);

                XmlNode game_notice_title = dom.SelectSingleNode("ResourceVersion/game_notice_title");
                XmlNode game_notice_message = dom.SelectSingleNode("ResourceVersion/game_notice_message");

                if (!forPaste)
                {
                    //m_NoticeModel.setText();
                }
            }
            catch (System.Exception ex)
            {
                Form1.g_Context.ErrorMsg.Text = ex.Message;
            }
        }

        public void parseWhileListModel(string manifestFile, bool forPaste)
        {
            m_TempManifestPath = manifestFile;

            if (m_WhiteListModel != null)
            {
                m_WhiteListModel.clean();
            }
            else
            {
                m_WhiteListModel = new WhiteListModel();
            }


            FileInfo fileInfo = new FileInfo(manifestFile);
            if (!fileInfo.Exists)
            {
                //MessageBox.Show("The file " + manifestFile + " is not exists!" );
                Form1.g_Context.ErrorMsg.Text = "The file " + manifestFile + " is not exists!";


                if (!forPaste)
                {
                    //m_WhiteListModel.setText();
                }
                return;
            }

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(manifestFile);

                XmlNodeList macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_ip_list/legal_client_ip");
                foreach (XmlNode macNode in macList)
                {
                    m_WhiteListModel.macList.Add(macNode.InnerText);
                }

                macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_user_list/legal_client_user");
                foreach (XmlNode macNode in macList)
                {
                    m_WhiteListModel.accountList.Add(macNode.InnerText);
                }

                macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_machine_list/legal_client_machine");
                foreach (XmlNode macNode in macList)
                {
                    m_WhiteListModel.imeiList.Add(macNode.InnerText);
                }

                if (!forPaste)
                {
                    //m_WhiteListModel.setText();
                }
            }
            catch (System.Exception ex)
            {
                Form1.g_Context.ErrorMsg.Text = ex.Message;
            }
        }


        public void parse(string maifestFile, bool forPaste)
        {
            parseNormalFlow(maifestFile, forPaste);
            parseTestFlow(maifestFile, forPaste);
            parseNoticeFlow(maifestFile, forPaste);
            parseWhileListModel(maifestFile, forPaste);

            if (!forPaste)
            {
                m_NormalFlow.setText();
                m_TestFlow.setText();
                m_WhiteListModel.setText();
            }
        }

        public void saveNormalFlow()
        {
            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(m_TempManifestPath);

                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion");

                XmlNodeList baseList = dom.SelectNodes("ResourceVersion/VersionBase");
                foreach (XmlNode baseNode in baseList)
                {
                    baseNode.ParentNode.RemoveChild(baseNode);
                }
                foreach (VersionModel model in m_NormalFlow.baseModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionBase");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    if (model.map_url != null && model.map_url != "")
                    {
                        XmlNode mapFile = dom.CreateElement("mapFile");
                        XmlNode mapFileMD5 = dom.CreateElement("mapFileMD5");
                        XmlNode mapFileSize = dom.CreateElement("mapFileSize");

                        mapFile.InnerText = model.map_url;
                        mapFileMD5.InnerText = model.map_md5;
                        mapFileSize.InnerText = model.map_size;

                        baseNode.AppendChild(mapFile);
                        baseNode.AppendChild(mapFileMD5);
                        baseNode.AppendChild(mapFileSize);
                    }


                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNodeList patchList = dom.SelectNodes("ResourceVersion/VersionPatch");
                foreach (XmlNode patchNode in patchList)
                {
                    patchNode.ParentNode.RemoveChild(patchNode);
                }

                foreach (VersionModel model in m_NormalFlow.patchModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionPatch");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNodeList dllList = dom.SelectNodes("ResourceVersion/VersionDll");
                if (dllList != null && dllList.Count > 0)
                {
                    foreach (XmlNode dllNode in dllList)
                    {
                        dllNode.ParentNode.RemoveChild(dllNode);
                    }
                }

                foreach (VersionModel model in m_NormalFlow.dllModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionDll");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNode versionNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/Version");
                XmlNode resVersionNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/ResVersion");
                XmlNode urlNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/url");
                XmlNode sizeNode = dom.SelectSingleNode("ResourceVersion/CodeVersion_last/size");

                SetNodeInnerText(dom, rootNode, "ForceUpdate", "" + m_NormalFlow.forceUpdate);

                versionNode.InnerText = m_NormalFlow.appVersion;
                resVersionNode.InnerText = m_NormalFlow.resVersion;
                urlNode.InnerText = m_NormalFlow.url;
                if(sizeNode != null)
                {
                    sizeNode.InnerText = m_NormalFlow.appSize;
                }
                dom.Save(m_TempManifestPath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void saveTestFlow()
        {
            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(m_TempManifestPath);

                XmlNodeList baseList = dom.SelectNodes("ResourceVersion/test_tag/VersionBase");
                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion/test_tag");

                SetNodeInnerText(dom, rootNode, "ForceUpdate", "" + m_TestFlow.forceUpdate);

                foreach (XmlNode baseNode in baseList)
                {
                    baseNode.ParentNode.RemoveChild(baseNode);
                }
                foreach (VersionModel model in m_TestFlow.baseModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionBase");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    if (model.map_url != null && model.map_url != "")
                    {
                        XmlNode mapFile = dom.CreateElement("mapFile");
                        XmlNode mapFileMD5 = dom.CreateElement("mapFileMD5");
                        XmlNode mapFileSize = dom.CreateElement("mapFileSize");

                        mapFile.InnerText = model.map_url;
                        mapFileMD5.InnerText = model.map_md5;
                        mapFileSize.InnerText = model.map_size;

                        baseNode.AppendChild(mapFile);
                        baseNode.AppendChild(mapFileMD5);
                        baseNode.AppendChild(mapFileSize);
                    }

                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNodeList patchList = dom.SelectNodes("ResourceVersion/test_tag/VersionPatch");
                foreach (XmlNode patchNode in patchList)
                {
                    patchNode.ParentNode.RemoveChild(patchNode);
                }

                foreach (VersionModel model in m_TestFlow.patchModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionPatch");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNodeList dllList = dom.SelectNodes("ResourceVersion/test_tag/VersionDll");
                if (dllList != null && dllList.Count > 0)
                {
                    foreach (XmlNode dllNode in dllList)
                    {
                        dllNode.ParentNode.RemoveChild(dllNode);
                    }
                }

                foreach (VersionModel model in m_TestFlow.dllModel)
                {
                    XmlNode baseNode = dom.CreateElement("VersionDll");
                    XmlNode fromVersion = dom.CreateElement("FromVersion");
                    XmlNode ToVersion = dom.CreateElement("ToVersion");
                    XmlNode PatchFile = dom.CreateElement("PatchFile");
                    XmlNode PatchFileMD5 = dom.CreateElement("PatchFileMD5");
                    XmlNode FileSize = dom.CreateElement("FileSize");

                    rootNode.AppendChild(baseNode);
                    baseNode.AppendChild(fromVersion);
                    baseNode.AppendChild(ToVersion);
                    baseNode.AppendChild(PatchFile);
                    baseNode.AppendChild(PatchFileMD5);
                    baseNode.AppendChild(FileSize);

                    fromVersion.InnerText = model.fromVersion;
                    ToVersion.InnerText = model.toVersion;
                    PatchFile.InnerText = model.resourceUrl;
                    PatchFileMD5.InnerText = model.md5;
                    FileSize.InnerText = model.fileSize;
                }

                XmlNode big_app_version = dom.SelectSingleNode("ResourceVersion/test_tag/big_app_version");
                XmlNode small_app_version = dom.SelectSingleNode("ResourceVersion/test_tag/small_app_version");
                XmlNode app_current_version = dom.SelectSingleNode("ResourceVersion/test_tag/app_current_version");
                XmlNode app_res_version = dom.SelectSingleNode("ResourceVersion/test_tag/app_res_version");
                XmlNode app_update_url = dom.SelectSingleNode("ResourceVersion/test_tag/app_update_url");
                XmlNode test_size = dom.SelectSingleNode("ResourceVersion/test_tag/test_size");
                app_update_url.InnerText = m_TestFlow.url;
                if (test_size != null)
                {
                    test_size.InnerText = m_TestFlow.appSize;
                }

                app_current_version.InnerText = m_TestFlow.appVersion;
                big_app_version.InnerText = m_TestFlow.bigVersion;
                small_app_version.InnerText = m_TestFlow.smallVersion;
                app_res_version.InnerText = m_TestFlow.resVersion;

                dom.Save(m_TempManifestPath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void saveNoticeFlow()
        {
            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(m_TempManifestPath);

                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion");

                XmlNode game_notice_title = dom.SelectSingleNode("ResourceVersion/game_notice_title");
                XmlNode game_notice_message = dom.SelectSingleNode("ResourceVersion/game_notice_message");
                XmlNodeList notice = dom.SelectNodes("ResourceVersion/notice");

                XmlNodeList patchList = dom.SelectNodes("ResourceVersion/notice");
                foreach (XmlNode patchNode in patchList)
                {
                    patchNode.ParentNode.RemoveChild(patchNode);
                }
                dom.Save(m_TempManifestPath);
            }
            catch (System.Exception ex)
            {
                Form1.g_Context.ErrorMsg.Text = ex.Message;
            }
        }


        public void saveWhiteListModel()
        {
            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(m_TempManifestPath);

                XmlNode rootNode = dom.SelectSingleNode("ResourceVersion");

                XmlNodeList macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_ip_list/legal_client_ip");
                foreach (XmlNode macNode in macList)
                {
                    macNode.ParentNode.RemoveChild(macNode);
                }

                macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_user_list/legal_client_user");
                foreach (XmlNode macNode in macList)
                {
                    macNode.ParentNode.RemoveChild(macNode);
                }

                macList = dom.SelectNodes("ResourceVersion/test_tag/legal_client_machine_list/legal_client_machine");
                foreach (XmlNode macNode in macList)
                {
                    macNode.ParentNode.RemoveChild(macNode);
                }

                XmlNode macRoot = dom.SelectSingleNode("ResourceVersion/test_tag/legal_client_ip_list");
                XmlNode imeiRoot = dom.SelectSingleNode("ResourceVersion/test_tag/legal_client_machine_list");
                XmlNode userRoot = dom.SelectSingleNode("ResourceVersion/test_tag/legal_client_user_list");

                foreach (string value in m_WhiteListModel.macList)
                {
                    if (value.Trim() == "")
                    {
                        continue;
                    }
                    XmlNode node = dom.CreateElement("legal_client_ip");
                    node.InnerText = value;

                    macRoot.AppendChild(node);
                }

                foreach (string value in m_WhiteListModel.accountList)
                {
                    if (value.Trim() == "")
                    {
                        continue;
                    }
                    XmlNode node = dom.CreateElement("legal_client_user");
                    node.InnerText = value;

                    userRoot.AppendChild(node);
                }

                foreach (string value in m_WhiteListModel.imeiList)
                {
                    if (value.Trim() == "")
                    {
                        continue;
                    }
                    XmlNode node = dom.CreateElement("legal_client_machine");
                    node.InnerText = value;

                    imeiRoot.AppendChild(node);
                }

                dom.Save(m_TempManifestPath);
            }
            catch (System.Exception ex)
            {
                Form1.g_Context.ErrorMsg.Text = ex.Message;
            }
        }


        public void saveAll()
        {
            if (m_TempManifestPath == null || !new FileInfo(m_TempManifestPath).Exists)
            {
                return;
            }
            saveNormalFlow();
            saveNoticeFlow();
            saveTestFlow();
            saveWhiteListModel();
        }


        public void update()
        {

        }


        public void upload()
        {

        }

        private XmlNode RequireNode(XmlDocument doc, XmlNode root, string nodeName, string defaultValue)
        {
            var node = root.SelectSingleNode(nodeName);
            if (node == null)
            {
                node = doc.CreateElement(nodeName);
                node.InnerText = defaultValue;
                root.AppendChild(node);
            }

            return node;
        }

        private void SetNodeInnerText(XmlDocument doc, XmlNode root, string nodePath, string value)
        {
            var node = root.SelectSingleNode(nodePath);
            if (node == null)
            {
                node = doc.CreateElement(nodePath);
                node.InnerText = value;
                root.AppendChild(node);
            }
            else
            {
                node.InnerText = value;
            }
        }
    }

    
}
