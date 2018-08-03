using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceConfig.Model
{
    class VersionModel
    {
        public string fromVersion;
        public string toVersion;
        public string resourceUrl;
        public string md5;
        public string fileSize;
        public string map_md5;
        public string map_url;
        public string map_size;

        public void init()
        {
            fromVersion = "";
            toVersion = "";
            resourceUrl = "";
            md5 = "";
            fileSize = "";

            map_md5 = "";
            map_size = "";
            map_url = "";
        }

        public void clean()
        {
            fromVersion = "";
            toVersion = "";
            resourceUrl = "";
            md5 = "";
            fileSize = "";
            map_md5 = "";
            map_size = "";
            map_url = "";
        }

        public VersionModel clone()
        {
            VersionModel model = new VersionModel();
            model.fromVersion = fromVersion;
            model.toVersion = toVersion;
            model.resourceUrl = resourceUrl;
            model.md5 = md5;
            model.fileSize = fileSize;

            model.map_md5 = map_md5;
            model.map_size = map_size;
            model.map_url = map_url;

            return model;
        }
    }
}
