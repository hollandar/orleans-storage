using System;

namespace Webefinity.ContentRoot.IndexUI.Models;

public class KeysAndPolicyModel
{
    public KeyCollection[] KeyCollections { get; set; } = [];
    public string AdminPolicy { get; set; } = "IC_Admin";
}
