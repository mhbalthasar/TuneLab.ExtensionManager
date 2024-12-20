﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TuneLab.Extensions;

internal class ExtensionDescription
{
    public required string name { get; set; }
    public string version { get; set; } = "1.0.0";
    public string[] assemblies { get; set; } = [];
    public string[] platforms { get; set; } = [];
}
