using System.IO.Compression;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Meta;
using GBX.NET.Engines.Plug;
using GBX.NET.LZO;
using TmEssentials;

Gbx.LZO = new Lzo();

const string itemIdToUse = "snow_1x1_1f";
const string resources = "../../../../resources";
const string inputMap = $"{resources}/Bad Apple - template.Map.Gbx";
const string inputPlacement = $"{resources}/bad_apple_greedy_placement.txt";
const string itemsLocation = $"{resources}/items";
const string originalLocation = $"{itemsLocation}/{itemIdToUse}.Item.Gbx";
const int translationMax = -10_000;
const int frameMs = 1_000;
const int blockWaitMs = 7_000;
const int flyInMs = 0;

if (!File.Exists(originalLocation))
{
    ExtractItemFromMap();
}

var gbx = Gbx.Parse<CGameItemModel>(originalLocation);
var originalItem = gbx.Node;

if (originalItem.EntityModelEdition != null)
{
    GenerateStaticItems();
}
else if (originalItem.EntityModel != null)
{
    GenerateDynamicItems();
}
else
{
    Console.WriteLine($"Unexpected form for item {itemIdToUse}");
}


void GenerateDynamicItems()
{
    var prefab = (CPlugPrefab)originalItem.EntityModel;
    var cPlugDynaObjectModel = (CPlugDynaObjectModel)prefab.Ents[0].Model;
    var sKinematicConstraint = (NPlugDyna_SKinematicConstraint)prefab.Ents[1].Model;
    var cPlugVisuals = cPlugDynaObjectModel.Mesh.Visuals;


    var varianceList = ExtractDynamicPlacements();
    var vertexStream = cPlugVisuals[0].VertexStreams[0];
    ((NPlugDynaObjectModel_SInstanceParams)prefab.Ents[0].Params).CastStaticShadow = false;
    sKinematicConstraint.TransMax = translationMax;
    sKinematicConstraint.TransAxis = NPlugDyna_SKinematicConstraint.EAxis.Y;
    var originalPositions = vertexStream.Positions.ToArray();

    Directory.CreateDirectory($"{itemsLocation}/dynamic");
    foreach (var (z, y, numFrames) in varianceList)
    {
        ScaleCube(vertexStream.Positions, originalPositions, z, y);
        sKinematicConstraint.TransAnimFunc.SubFuncs = GenerateAnimationsFunctions(numFrames);

        var name = $"snow_{z}x{y}_{numFrames}f";
        Console.WriteLine($"Saving {name}...");
        originalItem.Name = name;
        gbx.Save($"{itemsLocation}/dynamic/{name}.Item.Gbx");
    }

    Console.WriteLine("Dynamic generation success!");
}

void GenerateStaticItems()
{
    var cGameCommonItemEntityModelEdition = (CGameCommonItemEntityModelEdition)originalItem.EntityModelEdition;
    var geometryLayer = (CPlugCrystal.GeometryLayer)cGameCommonItemEntityModelEdition.MeshCrystal.Layers[0];
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

    var variancesList = ExtractStaticPlacements();

    Directory.CreateDirectory($"{itemsLocation}/static");
    foreach (var variance in variancesList)
    {
        var splitVariance = variance.Split("x");
        ScaleCube(positions, originalPositions, int.Parse(splitVariance[0]), int.Parse(splitVariance[1]));

        var name = $"snow_{variance}_static";
        Console.WriteLine($"Saving {name}...");
        gbx.Save($"{itemsLocation}/static/{name}.Item.Gbx");
    }

    Console.WriteLine("Static generation success!");
}

void ScaleCube(Vec3[] positions, Vec3[] refPositions, float sizeZ, float sizeY)
{
    var width = sizeZ * -8f;
    var height = sizeY * 8f;

    for (var i = 0; i < refPositions.Length; i++)
    {
        var newX = refPositions[i].X;
        var newY = refPositions[i].Y < 4f ? 0f : height;
        var newZ = refPositions[i].Z < -4f ? width : 0f;

        positions[i] = new Vec3(newX, newY, newZ);
    }
}

void ExtractItemFromMap()
{
    var map = Gbx.ParseNode<CGameCtnChallenge>(inputMap);
    var embeddedZipData = map.OpenReadEmbeddedZipData();
    var zippedItem = embeddedZipData.Entries.ToList().Find(e => e.Name.Contains(itemIdToUse));
    Directory.CreateDirectory(itemsLocation);
    zippedItem.ExtractToFile(originalLocation);
    Console.WriteLine($"Extracted item to {originalLocation}");
}

// 1x1, 4x3, ...
HashSet<string> ExtractStaticPlacements()
{
    var lines = File.ReadAllLines(inputPlacement);
    return lines.Skip(3).Select(line => line.Split("_")[1]).ToHashSet();
}

// 1x2_4f becomes (1, 2, 4)
HashSet<(int, int, int)> ExtractDynamicPlacements()
{
    var lines = File.ReadAllLines(inputPlacement);
    return lines.Skip(3).Select(line =>
    {
        var item2D = line.Split("_");

        var z = int.Parse(item2D[1].Split("x")[0]);
        var y = int.Parse(item2D[1].Split("x")[1]);
        var numFrames = int.Parse(item2D[2][0].ToString());

        return (z, y, numFrames);
    }).ToHashSet();
}


NPlugDyna_SKinematicConstraint.SubAnimFunc[] GenerateAnimationsFunctions(int numFrames)
{
    var flyInFn = new NPlugDyna_SKinematicConstraint.SubAnimFunc
    {
        Duration = new TimeInt32(flyInMs),
        Ease = NPlugDyna_SKinematicConstraint.AnimEase.QuadOut,
        Reverse = true
    };
    var frameFn = new NPlugDyna_SKinematicConstraint.SubAnimFunc
    {
        Duration = new TimeInt32(frameMs * numFrames),
        Reverse = false
    };
    var returnFn = new NPlugDyna_SKinematicConstraint.SubAnimFunc
    {
        Duration = new TimeInt32(0),
        Ease = NPlugDyna_SKinematicConstraint.AnimEase.Linear,
        Reverse = false
    };
    var waitFn = new NPlugDyna_SKinematicConstraint.SubAnimFunc
    {
        Duration = new TimeInt32(blockWaitMs),
        Reverse = true
    };

    return [flyInFn, frameFn, returnFn, waitFn];
}