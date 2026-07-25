import os
from PIL import Image

FRAME_FOLDER = "./resources/frames"
FRAME_COUNT = 156
WIDTH = 32
HEIGHT = 27
OUTPUT_FILE = "resources/bad_apple_greedy_placement_time_1.txt"

BLOCK_CONFIGS = [
    {"w": 6, "h": 6, "t": 1},
    {"w": 4, "h": 4, "t": 1},
    {"w": 2, "h": 2, "t": 1},

    {"w": 6, "h": 1, "t": 1},
    {"w": 5, "h": 1, "t": 1},
    {"w": 4, "h": 1, "t": 1},
    {"w": 3, "h": 1, "t": 1},
    {"w": 2, "h": 1, "t": 1},

    {"w": 1, "h": 6, "t": 1},
    {"w": 1, "h": 5, "t": 1},
    {"w": 1, "h": 4, "t": 1},
    {"w": 1, "h": 3, "t": 1},
    {"w": 1, "h": 2, "t": 1},

    {"w": 4, "h": 2, "t": 1},
    {"w": 2, "h": 4, "t": 1},
    {"w": 3, "h": 2, "t": 1},
    {"w": 2, "h": 3, "t": 1},

    {"w": 1, "h": 1, "t": 1}
]
BLOCK_CONFIGS.sort(key=lambda b: (b['w'] * b['h'] * b['t']), reverse=True)

# --- STEP 1: LOAD FRAMES INTO 3D MATRIX ---
matrix = []
print("Loading frames into memory...")
for f in range(1, FRAME_COUNT + 1):
    filename = f"frame_{f:03d}.png"
    filepath = os.path.join(FRAME_FOLDER, filename)

    if not os.path.exists(filepath):
        matrix.append([[False for _ in range(WIDTH)] for _ in range(HEIGHT)])
        continue

    img = Image.open(filepath).convert("1").resize((WIDTH, HEIGHT))
    frame_data = [[img.getpixel((x, y)) != 255 for x in range(WIDTH)] for y in range(HEIGHT)]
    matrix.append(frame_data)

# True = Available to be grouped, False = Already occupied by a placed block
available = [[[matrix[f][y][x] for x in range(WIDTH)] for y in range(HEIGHT)] for f in range(FRAME_COUNT)]

print("Executing spatial (horizontal/vertical) and temporal compression...")
placement_instructions = []

def can_place_block(start_f, start_y, start_x, w_sz, h_sz, t_sz):
    if start_x + w_sz > WIDTH or start_y + h_sz > HEIGHT or start_f + t_sz > FRAME_COUNT:
        return False
    for f in range(start_f, start_f + t_sz):
        for y in range(start_y, start_y + h_sz):
            for x in range(start_x, start_x + w_sz):
                if not available[f][y][x]:
                    return False
    return True


def mark_as_claimed(start_f, start_y, start_x, w_sz, h_sz, t_sz):
    for f in range(start_f, start_f + t_sz):
        for y in range(start_y, start_y + h_sz):
            for x in range(start_x, start_x + w_sz):
                available[f][y][x] = False


usage_tracker = {}
for config in BLOCK_CONFIGS:
    w_size = config["w"]
    h_size = config["h"]
    t_size = config["t"]

    for f in range(FRAME_COUNT):
        for y in range(HEIGHT):
            for x in range(WIDTH):
                if can_place_block(f, y, x, w_size, h_size, t_size):
                    item_name = f"snow_{w_size}x{h_size}_{t_size}f.Item.Gbx"

                    placement_instructions.append({
                        "x": x,
                        "y": y,
                        "block_type": item_name,
                        "start_frame": f,
                    })
                    item_name = f"snow_{w_size}x{h_size}_{t_size}f"
                    usage_tracker[item_name] = usage_tracker.get(item_name, 0) + 1
                    mark_as_claimed(f, y, x, w_size, h_size, t_size)

with open(OUTPUT_FILE, "w") as f:
    f.write(f"Total Block Instances Placed: {len(placement_instructions)}\n")
    f.write("Z_Coord | Y_Coord | Item_Name | Start_Frame\n")
    f.write("-" * 85 + "\n")
    for item in placement_instructions:
        f.write(
            f"{item['x']:2d} | {item['y']:2d} | {item['block_type']:25s} | {item['start_frame']:3d}\n")

print(f"\nOptimization Finished! Saved to '{OUTPUT_FILE}'")
print(f"New total item count: {len(placement_instructions)} instances.")


print("\n--- BLOCK USAGE PROFILE ---")
for block, count in sorted(usage_tracker.items(), key=lambda item: item[1], reverse=True):
    print(f"{block}: Used {count} times")