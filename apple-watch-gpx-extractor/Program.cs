using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GPXReaderLib.Models;
using Newtonsoft.Json;



class Program
{
    public static void Main(string[] args){

        string zipFilePath;
        string outputFolder;
        string fileType;

        try
        {
            ArgsLengthCheck(args);

            if (args.Length == 3)
            {
                fileType = args[2];
            }

            zipFilePath = args[0];
            outputFolder = args[1];

            ZipFolderCheck(zipFilePath);

            FolderCheck(outputFolder);

            using ZipArchive watchArchive = ZipFile.OpenRead(zipFilePath);
            ProcessGpxData(outputFolder, watchArchive);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            ShowUsage();
        }
    }

    private static void ProcessGpxData(string outputFolder, ZipArchive watchArchive)
    {
        FeatureCollection featureCollection = new FeatureCollection();
        foreach (ZipArchiveEntry entry in watchArchive.Entries)
        {
            if (entry.FullName.EndsWith(".gpx"))
            {
                using StreamReader xmlReader = new StreamReader(entry.Open());
                XDocument gpxFile = XDocument.Load(xmlReader);
                XmlNamespaceManager r = new XmlNamespaceManager(new NameTable());
                r.AddNamespace("p", "http://www.topografix.com/GPX/1/1");
                GPXReaderLib.GpxReader gpxReader = new GPXReaderLib.GpxReader(gpxFile, r);
                IEnumerable<TrackPoint> trackPoints = gpxReader.GetGpxCoordinates();

                var coordinates = new List<Position>();

                foreach (var track in trackPoints)
                {
                    Position position = new Position(track.Latitude, track.Longitude, track.Elevation);
                    coordinates.Add(position);
                }
                LineString lineString = new LineString(coordinates);
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties["name"] = gpxReader.GetGpxName();
                properties["elevation_gain"] = gpxReader.GetElevationGain();


                Feature feature = new Feature(lineString, properties);
                featureCollection.Features.Add(feature);
            }
        }
        string geoJson = JsonConvert.SerializeObject(featureCollection);
        DateTime currentDate = DateTime.Now;
        File.WriteAllText(Path.Combine(outputFolder, currentDate.ToString("yyyy-MM-dd") + ".geojson"), geoJson);
    }

    private static void ArgsLengthCheck(string[] args)
    {
        if (args.Length > 3)
        {
            throw new ArgumentException("Cannot provide more than 3 arguments");
        }

        if (args.Length < 2)
        {
            throw new ArgumentException("Need to provide zipefile and outputFolder.");
        }
    }

    private static void ZipFolderCheck(string zipFilePath)
    {
        if (zipFilePath.EndsWith(".zip") == false)
        {
            throw new ArgumentException("Zip file must be a valid .zip");
        }
    }

    private static void FolderCheck(string outputFolder)
    {
        if (Directory.Exists(outputFolder) == false)
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }

    public static void ShowUsage()
    {
        Console.WriteLine("Usage: apple-watch-gpx-extractor <zipfile> <outputFolder> <fileType>");
        Console.WriteLine("  <zipefile>      The full path to the zip file of apple health data");
        Console.WriteLine("  <outputFolder>  The folder to output processed data. Does not need to exist.");
        Console.WriteLine("  <fileType>      Optional file type. Default is geojson");
    }

}
