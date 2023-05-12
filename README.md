A Stream interop for Blazor from Javascript to .NET that supports `Seek` operations (as well as `Position` property). By default, Blazor's `IJSStreamReference` does not support `Seek` method and throws `NotSupportedException`.

![image](https://github.com/LukeVApps/Blazor.SeekableStream/assets/6388546/d75a89d3-6d68-4883-877b-eb0453705d4e)

# Installation

Install the [NuGet package `Blazor.SeekableStream`](https://www.nuget.org/packages/Blazor.SeekableStream) to your Blazor project.

```ps
dotnet add package Blazor.SeekableStream
```

# Usage

See also: [the Demo project](https://github.com/LukeVApps/Blazor.SeekableStream/tree/master/Blazor.SeekableStream.Demo).

Get a hold to a Javascript's `ArrayBuffer`, `Blob` or `Response` object and pass it to .NET as **the usual `IJSObjectReference`**.

For example, this function send a `File` (which is a `Blob`) that the user picked:

**interop.js:**
```js
export function pickFileAsync() {
    return new Promise(r => {
        const txt = document.createElement("input");
        txt.type = "file";
        txt.onchange = () => r(txt.files[0]);
        txt.click();
    });
}
```

Then in .NET, call `AsSeekableStreamReference` on the appropriate `IJSObjectReference` instances. You need to pass an `IJSRuntime` instance as the parameter as well.

**Index.razor.cs:**
```cs
var mod = await Js.InvokeAsync<IJSObjectReference>(
    "import",
    "/interop.js");

var file = await mod.InvokeAsync<IJSObjectReference>(PickMethodName);
var seekable = file.AsSeekableStreamReference(Js);
var stream = await seekable.OpenReadStreamAsync();

// Work with Stream
```

> **Warning**  
> With the current implementation, it's likely the whole file is read into memory because it calls `arrayBuffer()`. I am attempting to resolve it to achieve true random read.