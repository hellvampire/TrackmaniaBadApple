using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using System.Drawing;

Gbx.LZO = new Lzo();


const string itemIdToUse = "pixel_1x1_1f";
const string resources = "../../../../resources";
const string inputMap = $"{resources}/Bad Apple - template.Map.Gbx";
const string inputFrame = $"{resources}/frames/frame_001.png";
const string outputMap = $"{resources}/Bad Apple - frame 1.Map.Gbx";

var (startX, startY, startZ) = (1000, 100, 1000);
var blockWidth = 8;

var map = Gbx.ParseNode<CGameCtnChallenge>(inputMap);
CGameCtnAnchoredObject itemToUse = map.AnchoredObjects.Find(m => m.ItemModel.Id.Contains(itemIdToUse));

var bmp = new Bitmap(inputFrame);
for (int i = 0; i < bmp.Width; i++)
{
    for (int j = 0; j < bmp.Height; j++)
    {
        var pixel = bmp.GetPixel(i, j);
        if (pixel.GetBrightness() < 0.5)
        {
            map.PlaceAnchoredObject(itemToUse.ItemModel, new Vec3(startX, startY + (bmp.Height-j)*blockWidth, startZ + i*blockWidth), new Vec3(0, 0, 0));
        }
    }
}  


map.MapName = outputMap.Split('/').Last().Split('.').First();
map.Save(outputMap);
Console.WriteLine($"Success! Saved map to: {outputMap}");
