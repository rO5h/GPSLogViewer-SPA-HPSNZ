# GPSLogViewer-SPA-HPSNZ

A **Single Page Application (SPA)** built with **Blazor WebAssembly** that visualizes GPS log data.
static log file: birdyfirsh-exercise-rover-102.json

The app displays GPS tracks from JSON log files and provides overlay panels showing maximum values for,
**SOG (Speed Over Ground)**,
**VMG (Velocity Made Good)**,
**HR (Heart Rate)** channels.

---

## Features

- Render GPS tracks on an interactive **Leaflet map**.
- Display maximum values of SOG, VMG, and HR in an overlay table.
- Smooth streaming of large datasets with performance optimizations
- Batched map drawing to avoid browser freezes. 
- Works with log files in a specific JSON format only - refer:example-data.json

---

## Technical Notes & Limitations

### JSON Structure
- The log file is an **array of objects**
- Each `"values"` array contains numeric GPS data in the order defined by `"columns"`.
- Standard deserialization (like `JsonSerializer.Deserialize`) is not feasible for very large files.

### Streaming Approaches
1. **JsonTextReader (Newtonsoft.Json)**  
   - Supports async streaming.  
   - Experienced startup hiccups when locating `"values"` in large files.

2. **Utf8JsonReader (System.Text.Json)**  
   - Extremely fast, low-allocation.  
   - Synchronous only, cannot stream asynchronously in Blazor/WebAssembly.

3. **Custom Async Streaming Parser (current solution)**  
   - Batch-deserializes only the needed data.  
   - Smooth, low-memory streaming in Blazor/WebAssembly.  
   - Handles very large JSON files efficiently.


### DOM & Rendering
- The DOM size keeps increasing as polylines are added to Leaflet.
- Batched polyline drawing is necessary to maintain performance.
- Leafletforblazor documentions doesnt talks about canvas layer | May be I failed to find them.
---

## Future Improvements - JSON File Type
   - NDJSON format (`{ [value1,value2,...] }` per line) would be more efficient for streaming very large datasets.
---


## Build & Run

### Requirements
- **Visual Studio 2022** or newer (with Blazor WebAssembly workload)
- .NET 9.0 (or latest compatible)
- Git (for cloning repository)

### Running Locally
Simply clone the repository, open GPSDataRenderer.sln in Visual Studio, and press F5 to run.