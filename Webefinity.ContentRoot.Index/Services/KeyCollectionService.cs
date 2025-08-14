using System;
using Webefinity.ContentRoot.Index.Interfaces;
using Webefinity.ContentRoot.IndexUI.Interfaces;

namespace Webefinity.ContentRoot.IndexUI.Services;

public class KeyCollectionService : IKeyCollectionsService
{
    private KeyCollection[] keyCollection;

    public KeyCollectionService(KeyCollection[] keyCollection)
    {
        this.keyCollection = keyCollection ?? throw new ArgumentNullException(nameof(keyCollection));
    }
    
    public KeyCollection[] GetKeyCollections()
    {
        return keyCollection;
    }
}
