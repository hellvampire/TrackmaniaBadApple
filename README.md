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
New total item count: 7719 instances.

--- BLOCK USAGE PROFILE ---
snow_1x1_1f: Used 1907 times
snow_6x6_1f: Used 1136 times
snow_4x2_1f: Used 785 times
snow_4x4_1f: Used 578 times
snow_2x1_1f: Used 559 times
snow_2x4_1f: Used 430 times
snow_1x2_1f: Used 382 times
snow_6x1_1f: Used 322 times
snow_2x2_1f: Used 294 times
snow_3x1_1f: Used 241 times
snow_1x3_1f: Used 234 times
snow_1x6_1f: Used 225 times
snow_3x2_1f: Used 176 times
snow_1x4_1f: Used 106 times
snow_4x1_1f: Used 102 times
snow_1x5_1f: Used 92 times
snow_5x1_1f: Used 81 times
snow_2x3_1f: Used 69 times
```

### Item generation

1. Started from existing [Snow Cube](https://item.mania.exchange/item/view/126838) item
2. Scale the original 32x32 down to an 8x8 in Trackmania editor to generate `/resources/snow_1x1_static.Item.Gbx`
3. (Not 100% necessary) Generate all static variances by running `CreateItems`
4. Follow [tutorial](https://www.youtube.com/watch?v=Di4jZkdXfFM) with an [existing moving item](https://item.mania.exchange/item/view/181848) and change its mesh to the same as the snow_1x1_static
5. Save this new item as `/resources/snow_1x1_1f.Item.Gbx`
6. Generate all dynamic variances by running `CreateItems`