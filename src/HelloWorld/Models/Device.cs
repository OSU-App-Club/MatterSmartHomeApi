using System;
using System.Collections.Generic;

namespace HelloWorld.Models;

public class Device
{
    public string id { get; set; }
    public string name { get; set; }
    public bool? active { get; set; }
    public string wifi { get; set; }
    public string deviceType { get; set; }
    public List<string> groups { get; set; }
}