using Microsoft.JSInterop;
using System;
using Shiny.Stores;
using Shiny.Infrastructure;

namespace Shiny.Web.Stores;


public class LocalStorageStore : IKeyValueStore, IShinyStartupTask
{
    readonly IJSInProcessRuntime jsProc;
    readonly ISerializer serializer;
    IJSInProcessObjectReference module = null!;


    public LocalStorageStore(IJSRuntime jsRuntime, ISerializer serializer)
    {
        this.jsProc = (IJSInProcessRuntime)jsRuntime;
        this.serializer = serializer;
    }


    public async void Start()
    {
        this.module = await this.jsProc.ImportInProcess("Shiny.Core.Blazor", "storage.js");
    }


    public string Alias => "settings";
    public bool IsReadOnly => false;


    public bool Remove(string key) => this.module.Invoke<bool>("remove", key);
    public void Clear() => this.module.InvokeVoid("clear");
    public bool Contains(string key) => this.module.Invoke<bool>("exists", key);


    public object Get(Type type, string key)
    {
        var value = this.module.Invoke<string>("get", key);
        if (value == null)
            return null!;

        return this.serializer.Deserialize(type, value);
    }


    public void Set(string key, object value)
    {
        var json = this.serializer.Serialize(value);
        //this.module.Invoke<bool>("set", key, json);
        this.module.InvokeVoid("set", key, json);
    }
}
