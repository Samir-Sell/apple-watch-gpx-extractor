# apple-watch-gpx-extractor

A simple pet project to quickly allow users to extract all of their gpx data from Apple Health into a single GIS file. Currently the only export file supported is a GeoJSON file format. Simple acquire the executable and call it with your desired positional parameters. The GIS data is all generated from .gpx files found in the Apple Health export. The data is .GPX which is automatically assumed to be SRID 4326 ([WGS 1984](https://epsg.io/4326)).

### Parameters
The tool accepts 3 parameters. 

- `zipfile`: The path as a string to the zipped apple health export.
- `outputFolder`: The desired output folder to export the GIS file format to. It will try to create this location if it does not exist.
- `fileType`: Not implemnted yet, defaults to geojson.

`apple-watch-gpx-extractor.exe <zipfile> <outputFolder> <fileType>`

### Usage
```bash
apple-watch-gpx-extractor.exe "path/to/zip/file.zip" "path/to/directory"
```

### How To Extract Apple Health Data
- Go to Health App on Iphone
- Click pciture or initials on top right of the app
- Scroll to the bottom of the page
- Click "Export All Health Data"

### After Extraction (AKA: What the Hell is a .geoJSON?)
This is for users who have no idea how to handle a .geojson file or for users who do not have local software to handle .geojson files.

Based on the output location chosen (The outFolder), there should now be a .geojson file with todays date. It may be very large depending on how many routes you have recorded in your watch.

- Right click the .geojson file and open it with notepad (May take a while depending on size)
- use `ctrl + a` to copy all of the contents
- Open https://geojson.io/#map=2/0/20 and delete all the text in the JSON tab. 
- Paste your text into the JSON tab and be patient while it visualized your data. 
- Explore!