using System.IO.Compression;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new Lzo();


const string resources = "../../../../resources";
const string dynamicItems = $"{resources}/items/dynamic";
const string inputPlacement = $"{resources}/bad_apple_greedy_placement.txt";
const string inputMap = $"{resources}/Bad Apple - template.Map.Gbx";
const string outputMap = $"{resources}/Bad Apple - 2D.Map.Gbx";

var startY = 1000;
var radius = 1000;
var intialAnimationDelayMs = 2_000;
var initialBlockWaitMs = 200_000;
var timePerFrameMs = 1_000;
var blockSize = 8;

var map = Gbx.ParseNode<CGameCtnChallenge>(inputMap);
map.UpdateEmbeddedZipData(AddDynamicItems);

var embeddedZipData = map.OpenReadEmbeddedZipData();

var itemPlacements = ReadItemPlacements();
var maxHeight = itemPlacements.Select(p => p.Y).Max();
var maxFrame = itemPlacements.Select(p => p.Frame).Max();

foreach (var itemPlacement in itemPlacements)
{
    var itemEntry = embeddedZipData.GetEntry(itemPlacement.ItemName);

    CGameCtnAnchoredObject templateItem = map.AnchoredObjects.Find(m => m.ItemModel.Id.Contains("pixel_1x1_1f"));
    var itemWidth = int.Parse(itemPlacement.ItemName.Split("_")[1].Substring(0, 1));
    var itemHeight = int.Parse(itemPlacement.ItemName.Split("_")[1].Substring(2, 1));
    var angle = 2.0 * Math.PI * itemPlacement.Frame / maxFrame;
    var startX = (int)(radius * Math.Cos(angle)) + itemPlacement.Frame * blockSize * 2;
    var startZ = (int)(radius * Math.Sin(angle));

    var newPosition = new Vec3(
        startX,
        startY + (maxHeight - itemPlacement.Y - itemHeight) * blockSize,
        startZ + (itemPlacement.Z + itemWidth) * blockSize);
    var placedObject = map.PlaceAnchoredObject(
        new Ident(itemEntry.FullName, templateItem.ItemModel.Collection, templateItem.ItemModel.Author),
        newPosition,
        new Vec3(0, 0, 0) // (float)(angle * 180 / Math.PI)
    );
    placedObject.AnimPhaseOffset = (CGameCtnAnchoredObject.EPhaseOffset)(8 - itemPlacement.Frame % 8);
}

map.MapName = outputMap.Split('/').Last().Split('.').First();
map.Save(outputMap);
Console.WriteLine($"Success! Saved map to: {outputMap}");


// 

void AddDynamicItems(ZipArchive embeddedZip)
{
    foreach (var dynamicFileName in Directory.EnumerateFiles(dynamicItems))
    {
        var allBytes = File.ReadAllBytes(dynamicFileName);
        var fileName = new FileInfo(dynamicFileName).Name;
        var entry = embeddedZip.CreateEntry(fileName);
        entry.Open().Write(allBytes);
        Console.WriteLine($"Saved {fileName} into the map");
    }
}

List<ItemPlacement> ReadItemPlacements()
{
    return File.ReadAllLines(inputPlacement)
        .Skip(3)
        .Select(line =>
        {
            var splitLine = line.Split("|");
            return new ItemPlacement(
                int.Parse(splitLine[0]),
                int.Parse(splitLine[1]),
                splitLine[2].Trim(),
                int.Parse(splitLine[3])
            );
        }).ToList();
}


internal record ItemPlacement(int Z, int Y, string ItemName, int Frame);