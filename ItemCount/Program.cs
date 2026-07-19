using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

Gbx.LZO = new Lzo();

var map = Gbx.ParseNode<CGameCtnChallenge>("../../../../resources/Bad Apple - template.Map.Gbx");

foreach (var item in map.GetAnchoredObjects().GroupBy(x => x.ItemModel.Id))
{
    Console.WriteLine($"{item.Key}: {item.Count()}");
}