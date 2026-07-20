using System.IO.Compression;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using GBX.NET.LZO;

Gbx.LZO = new Lzo();

const string itemIdToUse = "snow_1x1_static";
const string resources = "../../../../resources";
const string inputMap = $"{resources}/Bad Apple - template.Map.Gbx";
const string itemsLocation = $"{resources}/items";
const string originalLocation = $"{itemsLocation}/{itemIdToUse}.Item.Gbx";

if (!File.Exists(originalLocation))
{
    var map = Gbx.ParseNode<CGameCtnChallenge>(inputMap);
    var embeddedZipData = map.OpenReadEmbeddedZipData();
    var zippedItem = embeddedZipData.Entries.ToList().Find(e => e.Name.Contains(itemIdToUse));
    Directory.CreateDirectory(itemsLocation);
    zippedItem.ExtractToFile(originalLocation);
    Console.WriteLine($"Extracted item to {originalLocation}");
}

void ScaleCrystalCube(Vec3[] positions, Vec3[] refPositions, float sizeX, float sizeY) {
    var width = sizeX * 8f;
    var midpointX = sizeX * 4f;
    var height = sizeY * 8f;

    for (int i = 0; i < refPositions.Length; i++) {
        var currentPos = refPositions[i];

        float newX = currentPos.X < 0.1f ? 0f : (currentPos.X < 5f ? midpointX : width);
        float newY = currentPos.Y < 4f ? 0f : height;
        float newZ = currentPos.Z < -4f ? -8f : 0f;

        positions[i] = new Vec3(newX, newY, newZ);
    }
}

var gbx = Gbx.Parse<CGameItemModel>(originalLocation);
var originalItem = gbx.Node;
var geometryLayer = (CPlugCrystal.GeometryLayer)((CGameCommonItemEntityModelEdition)originalItem.EntityModelEdition).MeshCrystal.Layers[0];
var positions = geometryLayer.Crystal.Positions;
var originalPositions = new[]
{
    new Vec3(8.0f, 0.0f, -8.0f),
    new Vec3(8.0f, 0.0f, 0.0f),
    new Vec3(0.0f, 0.0f, 0.0f),
    new Vec3(4.0f, 0.0f, -8.0f),
    new Vec3(0.0f, 0.0f, -8.0f),
    new Vec3(4.0f, 8.0f, -8.0f),
    new Vec3(0.0f, 8.0f, -8.0f),
    new Vec3(0.0f, 8.0f, 0.0f),
    new Vec3(8.0f, 8.0f, -8.0f),
    new Vec3(8.0f, 8.0f, 0.0f)
};
var variancesList = new List<string>
    { "1x1", "4x2", "2x1", "2x4", "6x6", "1x2", "2x2", "6x1", "1x6", "3x1", "1x3", "3x2", "4x1", "1x4", 
    "4x4", "1x5", "5x1", "2x3"};

foreach (var variance in variancesList)
{
    var splitVariance = variance.Split("x");
    ScaleCrystalCube(positions, originalPositions, int.Parse(splitVariance[0]), int.Parse(splitVariance[1]));
    
    gbx.Save($"{itemsLocation}/snow_{variance}_static.Item.Gbx");
}
