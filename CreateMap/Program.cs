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
const string platformName = "PlatformPlasticBase";
const string outputMap = $"{resources}/Bad Apple - Spiral.Map.Gbx";

const int startX = 1000;
const int startY = 500;
const int startZ = 1000;
const int startRadius = 500;
const int radiusEndFraction = 2;
const int amountOfRotations = 4;
const int blockSize = 8;
const float platformLength = 18f; // smaller than the original 32 to make the curve smoother
const float platformSampleStep = 0.0005f;
const float platformRadiusAdjustment = -100f;

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
    var percentageAlongAnimation = (double)itemPlacement.Frame / maxFrame;
    PlaceItemSpiral(itemPlacement, percentageAlongAnimation);
    itemCounter++;
}

var amountOfBlocksPlaced = GeneratePlatformBlocksSpiral();

map.MapName = outputMap.Split('/').Last().Split('.').First();
map.Save(outputMap);
Console.WriteLine($"Success! Saved map with {itemCounter} items and {amountOfBlocksPlaced} blocks to: {outputMap}");

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

void PlaceItemSpiral(ItemPlacement itemPlacement, double percentageAlongAnimation)
{
    var itemEntry = embeddedZipData.GetEntry($"Items/{itemPlacement.ItemName}");

    var itemHeight = int.Parse(itemPlacement.ItemName.Split("_")[1].Substring(2, 1));
    var angle = 2.0 * Math.PI * percentageAlongAnimation * amountOfRotations;
    
    var centerX = startRadius * (1 + percentageAlongAnimation * radiusEndFraction) * Math.Cos(angle) + startX;
    var centerZ = startRadius * (1 + percentageAlongAnimation * radiusEndFraction) * Math.Sin(angle) + startZ;

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
}

Vec3 SpiralPoint(double t)
{
    var radius = startRadius * (1 + t * radiusEndFraction) + platformRadiusAdjustment;
    var angle = 2.0 * Math.PI * amountOfRotations * t;

    return new Vec3(
        (float)(startX + radius * Math.Cos(angle)),
        startY + (float)(t * maxFrame * blockSize) - 32,
        (float)(startZ + radius * Math.Sin(angle))
    );
}

int GeneratePlatformBlocksSpiral()
{
    var previous = SpiralPoint(0);
    double previousT = 0;

    float accumulated = 0;
    var blocksPlaced = 0;

    for (double t = platformSampleStep; t <= 1.0; t += platformSampleStep)
    {
        var current = SpiralPoint(t);
        var segmentLength = (current - previous).GetMagnitude();

        while (accumulated + segmentLength >= platformLength)
        {
            var alpha = (platformLength - accumulated) / segmentLength;

            var edgeStart = previous + (current - previous) * alpha;
            var platformT = previousT + (t - previousT) * alpha;

            var remainingSegmentLength = segmentLength * (1 - alpha);
            
            var edgeEnd = edgeStart;
            
            if (accumulated + segmentLength >= platformLength * 2)
            {
                var alphaEnd = (platformLength * 2 - accumulated) / segmentLength;
                edgeEnd = previous + (current - previous) * alphaEnd;
            }
            else
            {
                var lookaheadT = t;
                var lookaheadPrevious = current;
                var lookaheadAccumulated = remainingSegmentLength;
                
                while (lookaheadAccumulated < platformLength && lookaheadT + platformSampleStep <= 1.0)
                {
                    lookaheadT += platformSampleStep;
                    var lookaheadCurrent = SpiralPoint(lookaheadT);
                    var lookaheadSeg = (lookaheadCurrent - lookaheadPrevious).GetMagnitude();
                    
                    if (lookaheadAccumulated + lookaheadSeg >= platformLength)
                    {
                        var lookaheadAlpha = (platformLength - lookaheadAccumulated) / lookaheadSeg;
                        edgeEnd = lookaheadPrevious + (lookaheadCurrent - lookaheadPrevious) * lookaheadAlpha;
                        break;
                    }
                    lookaheadAccumulated += lookaheadSeg;
                    lookaheadPrevious = lookaheadCurrent;
                }
            }

            var platformCenter = (edgeStart + edgeEnd) * 0.5f;

            var tangent = (edgeEnd - edgeStart).GetNormalized();
            var worldUp = new Vec3(0, 1, 0);
            var right = Vec3.GetCrossProduct(tangent, worldUp).GetNormalized();
            var up = Vec3.GetCrossProduct(right, tangent).GetNormalized();

            var yaw = MathF.Atan2(tangent.X, tangent.Z); 
            var horizontal = MathF.Sqrt(tangent.X * tangent.X + tangent.Z * tangent.Z);
            var pitch = -MathF.Atan2(tangent.Y, horizontal);

            var offset = new Vec3(16, 0, 16);
            var worldOffset = new Vec3(
                right.X * offset.X + up.X * offset.Y + tangent.X * offset.Z,
                right.Y * offset.X + up.Y * offset.Y + tangent.Y * offset.Z,
                right.Z * offset.X + up.Z * offset.Y + tangent.Z * offset.Z
            );

            var block = map.PlaceBlock(new Ident(platformName), (-1, 0, -1), Direction.North);
            block.Bit21 = true;
            block.AbsolutePositionInMap = platformCenter - worldOffset;
            block.YawPitchRoll = new Vec3(yaw, pitch, 0f); 
            block.IsFree = true;
            block.Color = DifficultyColor.Black;
            
            blocksPlaced++;
            
            accumulated = -remainingSegmentLength;
            previous = edgeStart;
            previousT = platformT;
            segmentLength = (current - previous).GetMagnitude();
        }

        accumulated += segmentLength;
        previous = current;
        previousT = t;
    }

    return blocksPlaced;
}

internal record ItemPlacement(int Z, int Y, string ItemName, int Frame);