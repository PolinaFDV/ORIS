using System;
using System.Collections.Generic;
using System.IO;

namespace MyHttpServer.Framework.Core
{
    public static class ContentTypeProvider
    {
        public static string GetContentType(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            switch (fileInfo.Extension)
            {
                case ".html":
                    return "text/html; charset=utf-8";

                case ".css":
                    return "text/css; charset=utf-8";

                case ".js":
                    return "text/javascript; charset=utf-8";

                case ".png":
                    return "image/png";

                case ".ico":
                    return "image/x-icon";

                case ".svg":
                    return "image/svg+xml";

                case ".jpg":
                    return "image/jpeg";

                default:
                    return "application/octet-stream";
            }
        }
    }
}