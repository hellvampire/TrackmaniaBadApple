# Bad apple animation Trackmania2020

### Shoutouts
* [gbx-net](https://github.com/BigBang1112/gbx-net) library to make anything in this repo work
* [KamiKalash](https://item.mania.exchange/user/profile/50960) for creating the original custom snow cube
* XertroV for the original moving item & [tutorial](https://www.youtube.com/watch?v=Di4jZkdXfFM)
* CBT_Enjoyer_69 for the initial idea and inspiration [maps](https://trackmania.exchange/mapsearch?query=author%3A+CBT_Enjoyer_69+tags%3A+%22Moving+Items%22)


### Resource generation
1. [Original 360x270 gif](https://files.catbox.moe/mgunnp.gif)
2. Create `/resources/frames` folder
2. Frame generation with [ffmpeg](https://ffmpeg.org/download.html) `ffmpeg -i ..\bad_apple_360x270.gif -vf "fps=2.11,scale=36:27" frame_%03d.png` (scaled from gif to 300 frames)
3. Generate `/resources/bad_apple_greedy_placement.txt` with `bad_apple_compression.py`

```
New total item count: 14586 instances.

--- BLOCK USAGE PROFILE ---
snow_1x1_1f: Used 4115 times
snow_6x6_1f: Used 1970 times
snow_4x2_1f: Used 1286 times
snow_4x4_1f: Used 1149 times
snow_2x1_1f: Used 935 times
snow_2x4_1f: Used 870 times
snow_1x2_1f: Used 739 times
snow_6x1_1f: Used 549 times
snow_2x2_1f: Used 531 times
snow_3x1_1f: Used 429 times
snow_1x3_1f: Used 422 times
snow_1x6_1f: Used 351 times
snow_3x2_1f: Used 301 times
snow_1x4_1f: Used 229 times
snow_1x5_1f: Used 218 times
snow_4x1_1f: Used 178 times
snow_5x1_1f: Used 160 times
snow_2x3_1f: Used 154 times
```

### Item generation

1. Started from existing [Snow Cube](https://item.mania.exchange/item/view/126838) item
2. Scale the original 32x32 down to an 8x8 in Trackmania editor to generate `/resources/snow_1x1_static.Item.Gbx`
3. (Not 100% necessary) Generate all static variances by running `CreateItems`
4. Follow [tutorial](https://www.youtube.com/watch?v=Di4jZkdXfFM) with an [existing moving item](https://item.mania.exchange/item/view/181848) and change its mesh to the same as the snow_1x1_static
5. Save this new item as `/resources/snow_1x1_1f.Item.Gbx`
6. Generate all dynamic variances by running `CreateItems`

### Removing the clouds
1. Edit Ambiance in the editor
2. Add fog layer
3. Change sky intensity to 100% on both start & end keypoints