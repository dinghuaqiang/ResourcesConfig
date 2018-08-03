using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using ResourceConfig.Util;
using System.Windows.Forms;

namespace ResourceConfig.Model
{
    class ConfigXmlModel
    {
        public string m_Ip;
        public string m_Port;
        public string m_User;
        public string m_Pwd;

        public string m_ProjectName;
        public List<PlatformModel> m_Platforms;

        string m_configFile = "Config/Config.xml";

        public bool parse()
        {
            if (m_Platforms == null)
            {
                m_Platforms = new List<PlatformModel>();
            }
            else
            {
                m_Platforms.Clear();
            }

            FileInfo fileInfo = new FileInfo(m_configFile);
            if (!fileInfo.Exists)
            {
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileInfo.FullName);

                XmlElement node = (XmlElement)doc.SelectSingleNode("project");

                m_ProjectName = node.GetAttribute("name");

                m_Ip = doc.SelectSingleNode("project/ftp/ip").InnerText;
                m_Port = doc.SelectSingleNode("project/ftp/port").InnerText;
                m_User = doc.SelectSingleNode("project/ftp/account").InnerText;
                m_Pwd = doc.SelectSingleNode("project/ftp/pwd").InnerText;
                m_Pwd = DES.DecryptDES(m_Pwd, DES.ENCRYPT_KEY);

                XmlNodeList platformList = doc.SelectNodes("project/platform");

                foreach (XmlElement element in platformList)
                {
                    PlatformModel model = new PlatformModel();
                    model.m_Name = element.GetAttribute("name");
                    model.m_Type = element.GetAttribute("type");
                    model.m_Url = element.SelectSingleNode("./ftpUrl").InnerText;

                    m_Platforms.Add(model);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("ConfigXmlModel::parse " + ex.Message);
            }

            return false;
        }
    }
}
