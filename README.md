# Bad apple animation Trackmania2020

### Shoutouts
* [gbx-net](https://github.com/BigBang1112/gbx-net) library to make anything in this repo work
* [KamiKalash](https://item.mania.exchange/user/profile/50960) for creating the original custom snow cube
* XertroV for the original moving item & [tutorial](https://www.youtube.com/watch?v=Di4jZkdXfFM)
* CBT_Enjoyer_69 for the initial idea and inspiration [maps](https://trackmania.exchange/mapsearch?query=author%3A+CBT_Enjoyer_69+tags%3A+%22Moving+Items%22)


### Resource generation
1. [Original 360x270 gif](https://files.catbox.moe/mgunnp.gif)
2. Frame generation with [ffmpeg](https://ffmpeg.org/download.html) `ffmpeg -i .\bad_apple_360x270.gif -vf "fps=1,scale=36:27" frame_%03d.png`
3. Generate `/resources/bad_apple_greedy_placement.txt` with `bad_apple_compression.py`

```
New total item count: 7474 instances.

--- BLOCK USAGE PROFILE ---
pixel_1x1_1f: Used 1839 times
pixel_4x2_1f: Used 816 times
pixel_2x1_1f: Used 567 times
pixel_4x4_1f: Used 547 times
pixel_2x4_1f: Used 455 times
pixel_6x6_1f: Used 451 times
pixel_1x2_1f: Used 364 times
pixel_2x2_1f: Used 323 times
pixel_6x1_1f: Used 284 times
pixel_1x6_1f: Used 251 times
pixel_3x1_1f: Used 251 times
pixel_1x3_1f: Used 222 times
pixel_3x2_1f: Used 192 times
pixel_4x1_1f: Used 133 times
pixel_1x4_1f: Used 133 times
pixel_6x6_4f: Used 123 times
pixel_4x4_4f: Used 91 times
pixel_1x5_1f: Used 84 times
pixel_5x1_1f: Used 83 times
pixel_2x3_1f: Used 81 times
pixel_1x1_2f: Used 72 times
pixel_1x1_8f: Used 65 times
pixel_1x1_4f: Used 47 times
```

### Item generation

1. Started from existing [Snow Cube](https://item.mania.exchange/item/view/126838) item
2. Scale the original 32x32 down to an 8x8 in Trackmania editor to generate `/resources/snow_1x1_static.Item.Gbx`
3. (Not 100% necessary) Generate all static variances by running `CreateItems`
4. Follow [tutorial](https://www.youtube.com/watch?v=Di4jZkdXfFM) with an [existing moving item](https://item.mania.exchange/item/view/181848) and change its mesh to the same as the snow_1x1_static
5. Save this new item as `/resources/snow_1x1_1f.Item.Gbx`
6. Generate all dynamic variances by running `CreateItems`