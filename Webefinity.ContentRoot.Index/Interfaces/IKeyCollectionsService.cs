using System;
using Webefinity.ContentRoot.IndexUI;

namespace Webefinity.ContentRoot.Index.Interfaces;

public interface IKeyCollectionsService
{
    KeyCollection[] GetKeyCollections();
}
