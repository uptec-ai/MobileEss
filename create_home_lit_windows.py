from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parent
INPUT_PATH = ROOT / "EMS_PJT_Hamburger" / "Assets" / "Home" / "Home.png"
OUTPUT_PATH = ROOT / "EMS_PJT_Hamburger" / "Assets" / "Home" / "Home_AllWindows_PorchGlow.png"


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    value = hex_color.lstrip("#")
    return (
        int(value[0:2], 16),
        int(value[2:4], 16),
        int(value[4:6], 16),
        alpha,
    )


def lerp(a: tuple[float, float], b: tuple[float, float], t: float) -> tuple[float, float]:
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


def int_points(points: list[tuple[float, float]]) -> list[tuple[int, int]]:
    return [(int(round(x)), int(round(y))) for x, y in points]


def add_glow(base: Image.Image, draw_fn, blur_radius: int) -> None:
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow, "RGBA")
    draw_fn(glow_draw)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    base.alpha_composite(glow)


def add_polygon_light(base: Image.Image, poly: list[tuple[float, float]]) -> None:
    amber = rgba("#FFD37A", 108)
    bright = rgba("#FFF2B3", 96)
    hot = rgba("#FFF9DA", 74)

    add_glow(base, lambda d: d.polygon(int_points(poly), fill=amber), blur_radius=18)
    add_glow(base, lambda d: d.polygon(int_points(poly), fill=bright), blur_radius=9)

    overlay = Image.new("RGBA", base.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay, "RGBA")
    draw.polygon(int_points(poly), fill=rgba("#FFC95E", 82))

    top_band = [
        lerp(poly[0], poly[3], 0.15),
        lerp(poly[1], poly[2], 0.15),
        lerp(poly[1], poly[2], 0.48),
        lerp(poly[0], poly[3], 0.48),
    ]
    draw.polygon(int_points(top_band), fill=hot)

    bottom_band = [
        lerp(poly[0], poly[3], 0.62),
        lerp(poly[1], poly[2], 0.62),
        lerp(poly[1], poly[2], 0.92),
        lerp(poly[0], poly[3], 0.92),
    ]
    draw.polygon(int_points(bottom_band), fill=rgba("#FFE9A1", 62))

    overlay = overlay.filter(ImageFilter.GaussianBlur(radius=1))
    base.alpha_composite(overlay)


def add_rect_window(base: Image.Image, box: tuple[int, int, int, int]) -> None:
    x0, y0, x1, y1 = box
    poly = [(x0, y0), (x1, y0), (x1, y1), (x0, y1)]
    add_polygon_light(base, poly)

    draw = ImageDraw.Draw(base, "RGBA")
    mid_x = (x0 + x1) // 2
    mid_y = (y0 + y1) // 2
    frame = rgba("#FFF9DF", 120)
    draw.line((mid_x, y0 + 4, mid_x, y1 - 4), fill=frame, width=5)
    draw.line((x0 + 4, mid_y, x1 - 4, mid_y), fill=frame, width=5)
    draw.rectangle((x0, y0, x1, y1), outline=rgba("#FFF8E8", 95), width=3)


def add_quad_window(base: Image.Image, poly: list[tuple[float, float]]) -> None:
    add_polygon_light(base, poly)

    draw = ImageDraw.Draw(base, "RGBA")
    top_mid = lerp(poly[0], poly[1], 0.5)
    bottom_mid = lerp(poly[3], poly[2], 0.5)
    left_mid = lerp(poly[0], poly[3], 0.5)
    right_mid = lerp(poly[1], poly[2], 0.5)
    frame = rgba("#FFF9DF", 115)
    draw.line(int_points([top_mid, bottom_mid]), fill=frame, width=5)
    draw.line(int_points([left_mid, right_mid]), fill=frame, width=5)
    draw.polygon(int_points(poly), outline=rgba("#FFF8E8", 92), width=3)


def add_round_window(base: Image.Image, box: tuple[int, int, int, int]) -> None:
    x0, y0, x1, y1 = box
    add_glow(base, lambda d: d.ellipse((x0, y0, x1, y1), fill=rgba("#FFD67C", 72)), blur_radius=8)
    add_glow(base, lambda d: d.ellipse((x0 + 10, y0 + 10, x1 - 10, y1 - 10), fill=rgba("#FFF1B5", 58)), blur_radius=4)
    draw = ImageDraw.Draw(base, "RGBA")
    draw.ellipse((x0 + 6, y0 + 6, x1 - 6, y1 - 6), fill=rgba("#FFD27A", 44))
    cx = (x0 + x1) // 2
    cy = (y0 + y1) // 2
    frame = rgba("#FFF8E8", 105)
    draw.line((cx, y0 + 6, cx, y1 - 6), fill=frame, width=4)
    draw.line((x0 + 6, cy, x1 - 6, cy), fill=frame, width=4)


def add_porch_glow(base: Image.Image) -> None:
    # Warm interior glow under the porch canopy.
    canopy_outer = [(1078, 1836), (1542, 1858), (1448, 2014), (1140, 1984)]
    canopy_inner = [(1130, 1854), (1492, 1872), (1416, 1980), (1188, 1958)]
    add_glow(base, lambda d: d.polygon(canopy_outer, fill=rgba("#FFC56B", 46)), blur_radius=30)
    add_glow(base, lambda d: d.polygon(canopy_inner, fill=rgba("#FFE0A1", 42)), blur_radius=16)

    # Door surround and glass panel feel lit from inside.
    door_box = (1286, 1890, 1460, 2258)
    add_glow(base, lambda d: d.rounded_rectangle(door_box, radius=24, fill=rgba("#FFBB5A", 32)), blur_radius=24)
    add_glow(base, lambda d: d.ellipse((1322, 1910, 1404, 1996), fill=rgba("#FFE2A8", 48)), blur_radius=10)

    # Warm spill across the porch step and walkway.
    spill = [(1118, 2266), (1510, 2250), (1664, 2486), (1036, 2496)]
    spill_core = [(1176, 2278), (1460, 2266), (1560, 2448), (1090, 2456)]
    add_glow(base, lambda d: d.polygon(spill, fill=rgba("#FFBF66", 32)), blur_radius=34)
    add_glow(base, lambda d: d.polygon(spill_core, fill=rgba("#FFE0AB", 26)), blur_radius=18)

    # Slight wall wash so the porch area reads as actively illuminated.
    wall_wash = [(1092, 1738), (1566, 1752), (1520, 2310), (1108, 2294)]
    add_glow(base, lambda d: d.polygon(wall_wash, fill=rgba("#FFD48A", 18)), blur_radius=28)


def main() -> None:
    image = Image.open(INPUT_PATH).convert("RGBA")

    front_rects = [
        (994, 1458, 1146, 1706),
        (1224, 1462, 1378, 1708),
        (980, 1914, 1154, 2224),
        (1528, 1944, 1708, 2228),
    ]
    for box in front_rects:
        add_rect_window(image, box)

    side_quads = [
        [(1942, 1918), (2050, 1918), (2046, 2228), (1938, 2228)],
        [(2370, 1914), (2482, 1910), (2476, 2232), (2364, 2236)],
        [(2176, 1482), (2286, 1478), (2280, 1702), (2172, 1708)],
    ]
    for poly in side_quads:
        add_quad_window(image, poly)

    add_round_window(image, (1154, 1128, 1270, 1246))

    # A subtle warm spill around the main facade keeps the lit windows cohesive.
    add_glow(
        image,
        lambda d: d.polygon(
            [(898, 1368), (1428, 1382), (1470, 2310), (914, 2278)],
            fill=rgba("#FFC86C", 22),
        ),
        blur_radius=24,
    )
    add_porch_glow(image)

    image.save(OUTPUT_PATH)
    print(OUTPUT_PATH)


if __name__ == "__main__":
    main()
