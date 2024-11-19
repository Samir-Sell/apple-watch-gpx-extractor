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
            if (entry.FullName.EndsWith(Constants.Gpx.FileExtension))
            {
                using StreamReader xmlReader = new StreamReader(entry.Open());
                XDocument gpxFile = XDocument.Load(xmlReader);
                XmlNamespaceManager r = new XmlNamespaceManager(new NameTable());
                r.AddNamespace("p", Constants.Gpx.NamespaceUri);
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
                properties[Constants.GeoJson.Name] = gpxReader.GetGpxName();
                properties[Constants.GeoJson.ElevationGain] = gpxReader.GetElevationGain();


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
            throw new ArgumentException("Too many arguments. Usage: <zipfile> <outputFolder> <fileType>");
        }

        if (args.Length < 2)
        {
            throw new ArgumentException("Insufficient arguments. Provide <zipfile> and <outputFolder>.");
        }
    }

    private static void ZipFolderCheck(string zipFilePath)
    {
        if (zipFilePath.EndsWith(".zip") == false)
        {
            throw new ArgumentException("Zip file must be a valid .zip");
        }

        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"The zip file at '{zipFilePath}' does not exist.");
        }
    }

    private static void FolderCheck(string outputFolder)
    {
        if (!Directory.Exists(outputFolder))
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (UnauthorizedAccessException)
            {
                throw new ArgumentException($"Insufficient permissions to create directory: {outputFolder}");
            }
            catch (IOException ex)
            {
                throw new ArgumentException($"Error creating directory: {outputFolder}. Details: {ex.Message}");
            }
        }
    }

    public static void ShowUsage()
    {
        Console.WriteLine("Usage: apple-watch-gpx-extractor <zipfile> <outputFolder> <fileType>");
        Console.WriteLine("  <zipefile>      The full path to the zip file of apple health data");
        Console.WriteLine("  <outputFolder>  The folder to output processed data. Does not need to exist.");
        Console.WriteLine("  <fileType>      Optional file type. Default is geojson. Currently, does not work");
    }

}

public static class Constants
{
    public static class GeoJson
    {
        public const string Name = "name";
        public const string ElevationGain = "elevation_gain";
        public const string FileExtension = ".geojson";
    }

    public static class Gpx
    {
        public const string NamespaceUri = "http://www.topografix.com/GPX/1/1";
        public const string FileExtension = ".gpx";
    }
}
