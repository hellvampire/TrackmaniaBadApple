using System.IO.Compression;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new Lzo();


const string resources = "../../../../resources";
const string dynamicItems = $"{resources}/items/dynamic";
const string inputPlacement = $"{resources}/bad_apple_greedy_placement.txt";
const string inputMap = $"{resources}/Bad Apple - template.Map.Gbx";
const string templatedItemName = "pixel_1x1_1f";
const string outputMap = $"{resources}/Bad Apple - Spiral.Map.Gbx";

const int startY = 800;
const int startRadius = 1000;
const int radiusEndFraction = 2;
const int amountOfRotations = 4;
const int blockSize = 8;

var map = Gbx.ParseNode<CGameCtnChallenge>(inputMap);
map.UpdateEmbeddedZipData(AddDynamicItems);

var embeddedZipData = map.OpenReadEmbeddedZipData();
var templateItem = map.AnchoredObjects.Find(m => m.ItemModel.Id.Contains(templatedItemName));

var itemPlacements = ReadItemPlacements();
var maxHeight = itemPlacements.Select(p => p.Y).Max();
var maxWidth = itemPlacements.Select(p => p.Z).Max();
var maxFrame = itemPlacements.Select(p => p.Frame).Max();

var itemCounter = 0;
foreach (var itemPlacement in itemPlacements)
{
    var itemEntry = embeddedZipData.GetEntry($"Items/{itemPlacement.ItemName}");

    var itemHeight = int.Parse(itemPlacement.ItemName.Split("_")[1].Substring(2, 1));
    var percentageAlongAnimation = (double)itemPlacement.Frame / maxFrame;
    var angle = 2.0 * Math.PI * percentageAlongAnimation * amountOfRotations;
    
    var centerX = startRadius * (1 + percentageAlongAnimation * radiusEndFraction) * Math.Cos(angle);
    var centerZ = startRadius * (1 + percentageAlongAnimation * radiusEndFraction) * Math.Sin(angle);

    var targetAngleRad = angle + Math.PI / 2.0; 
    
    var localOffsetX = blockSize / 2.0;
    var localOffsetZ =  (itemPlacement.Z - maxWidth / 2.0) * blockSize;

    var rotatedX = localOffsetX * Math.Cos(targetAngleRad) - localOffsetZ * Math.Sin(targetAngleRad);
    var rotatedZ = localOffsetX * Math.Sin(targetAngleRad) + localOffsetZ * Math.Cos(targetAngleRad);

    var correctedX = centerX + rotatedX;
    var correctedZ = centerZ + rotatedZ;

    var newPosition = new Vec3(
        (float)correctedX,
        startY + (maxHeight - itemPlacement.Y - itemHeight + itemPlacement.Frame) * blockSize,
        (float)correctedZ
    );

    var placedObject = map.PlaceAnchoredObject(
        new Ident(itemEntry.Name, templateItem.ItemModel.Collection, templateItem.ItemModel.Author),
        newPosition,
        new Vec3(float.Pi / 2 - (float) angle, 0, 0) 
    );

    placedObject.AnimPhaseOffset = (CGameCtnAnchoredObject.EPhaseOffset)(8 - itemPlacement.Frame % 8);
    itemCounter++;
}

map.MapName = outputMap.Split('/').Last().Split('.').First();
map.Save(outputMap);
Console.WriteLine($"Success! Saved map with {itemCounter} items to: {outputMap}");


// 

void AddDynamicItems(ZipArchive embeddedZip)
{
    foreach (var dynamicFileName in Directory.EnumerateFiles(dynamicItems))
    {
        var allBytes = File.ReadAllBytes(dynamicFileName);
        var fileName = new FileInfo(dynamicFileName).Name;
        var entry = embeddedZip.CreateEntry($"Items/{fileName}");
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