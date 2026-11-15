using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer;
public class Settings
{
    public string[] Prefixes { get; set; } = [];
    public string ConnectionString { get; set; } = string.Empty;
}
