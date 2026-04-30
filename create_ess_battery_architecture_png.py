from __future__ import annotations

from pathlib import Path
from typing import Iterable

from PIL import Image, ImageDraw, ImageFilter, ImageFont


ROOT = Path(__file__).resolve().parent
OUTPUT_DIR = ROOT / "EMS_PJT_Hamburger" / "Assets" / "Generated"
OUTPUT_PATH = OUTPUT_DIR / "ESS_Battery_Architecture_3D.png"

CANVAS_W = 1800
CANVAS_H = 1300


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


def shift(points: Iterable[tuple[float, float]], dx: float, dy: float) -> list[tuple[int, int]]:
    return [(int(round(x + dx)), int(round(y + dy))) for x, y in points]


def load_font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    font_candidates = [
        "C:/Windows/Fonts/malgunbd.ttf" if bold else "C:/Windows/Fonts/malgun.ttf",
        "C:/Windows/Fonts/segoeuib.ttf" if bold else "C:/Windows/Fonts/segoeui.ttf",
        "C:/Windows/Fonts/arialbd.ttf" if bold else "C:/Windows/Fonts/arial.ttf",
    ]
    for font_path in font_candidates:
        if Path(font_path).exists():
            return ImageFont.truetype(font_path, size=size)
    return ImageFont.load_default()


def draw_shadow(base: Image.Image, top_face: list[tuple[int, int]], h: int, blur: int = 22) -> None:
    shadow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow)
    spread = shift(top_face, 65, h + 85)
    shadow_draw.polygon(spread, fill=(6, 15, 27, 95))
    shadow = shadow.filter(ImageFilter.GaussianBlur(radius=blur))
    base.alpha_composite(shadow)


def draw_prism(
    base: Image.Image,
    *,
    x: int,
    y: int,
    w: int,
    h: int,
    depth_x: int,
    depth_y: int,
    top_color: tuple[int, int, int, int],
    front_color: tuple[int, int, int, int],
    side_color: tuple[int, int, int, int],
    outline: tuple[int, int, int, int],
    edge_glow: tuple[int, int, int, int] | None = None,
) -> dict[str, list[tuple[int, int]]]:
    draw = ImageDraw.Draw(base, "RGBA")
    front = [(x, y), (x + w, y), (x + w, y + h), (x, y + h)]
    top = [(x, y), (x + depth_x, y - depth_y), (x + w + depth_x, y - depth_y), (x + w, y)]
    side = [(x + w, y), (x + w + depth_x, y - depth_y), (x + w + depth_x, y + h - depth_y), (x + w, y + h)]

    draw.polygon(front, fill=front_color)
    draw.polygon(side, fill=side_color)
    draw.polygon(top, fill=top_color)
    draw.line(top + [top[0]], fill=outline, width=3)
    draw.line(front + [front[0]], fill=outline, width=3)
    draw.line(side + [side[0]], fill=outline, width=3)

    if edge_glow:
        glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
        glow_draw = ImageDraw.Draw(glow, "RGBA")
        glow_draw.line([top[1], top[2]], fill=edge_glow, width=8)
        glow_draw.line([top[2], top[3]], fill=edge_glow, width=8)
        glow = glow.filter(ImageFilter.GaussianBlur(radius=5))
        base.alpha_composite(glow)

    return {"front": front, "top": top, "side": side}


def draw_module_cells(
    base: Image.Image,
    *,
    x: int,
    y: int,
    cols: int,
    rows: int,
    cell_w: int,
    cell_h: int,
    gap_x: int,
    gap_y: int,
    depth_x: int,
    depth_y: int,
    top_color: tuple[int, int, int, int],
    front_color: tuple[int, int, int, int],
    side_color: tuple[int, int, int, int],
    outline: tuple[int, int, int, int],
) -> None:
    for row in range(rows):
        for col in range(cols):
            col_offset = col * (cell_w + gap_x)
            row_offset = row * gap_y
            y_depth = row * 7
            draw_prism(
                base,
                x=x + col_offset + row_offset,
                y=y - y_depth,
                w=cell_w,
                h=cell_h,
                depth_x=depth_x,
                depth_y=depth_y,
                top_color=top_color,
                front_color=front_color,
                side_color=side_color,
                outline=outline,
                edge_glow=rgba("#9CF8E8", 120),
            )


def draw_cooling_pattern(base: Image.Image, slab: dict[str, list[tuple[int, int]]]) -> None:
    draw = ImageDraw.Draw(base, "RGBA")
    top = slab["top"]
    x0, y0 = top[0]
    x1, y1 = top[2]
    x_left_slant = top[1][0] - top[0][0]
    y_slant = top[0][1] - top[1][1]
    for offset in range(55, 360, 60):
        line = [
            (x0 + offset, y0 - 8),
            (x0 + offset + x_left_slant // 2, y0 - y_slant // 2 - 20),
            (x0 + offset + x_left_slant, y0 - y_slant + 5),
        ]
        draw.line(line, fill=rgba("#A6F4FF", 190), width=4)
    for offset in range(145, 420, 90):
        draw.ellipse(
            (x0 + offset, y0 - 35, x0 + offset + 22, y0 - 13),
            outline=rgba("#C4FBFF", 180),
            width=3,
        )


def draw_chip(base: Image.Image, slab: dict[str, list[tuple[int, int]]]) -> None:
    draw = ImageDraw.Draw(base, "RGBA")
    top = slab["top"]
    cx = (top[0][0] + top[2][0]) // 2
    cy = (top[0][1] + top[2][1]) // 2 - 6
    body = [
        (cx - 92, cy + 8),
        (cx - 12, cy - 38),
        (cx + 88, cy + 6),
        (cx + 6, cy + 55),
    ]
    draw.polygon(body, fill=rgba("#132338", 255), outline=rgba("#8CE8FF", 230))
    inner = shift(body, 0, 0)
    draw.polygon(
        [
            (inner[0][0] + 18, inner[0][1] + 12),
            (inner[1][0] + 8, inner[1][1] + 15),
            (inner[2][0] - 18, inner[2][1] - 10),
            (inner[3][0] - 8, inner[3][1] - 15),
        ],
        outline=rgba("#50D9FF", 180),
        width=3,
    )
    pin_color = rgba("#D4F7FF", 215)
    for i in range(7):
        px = cx - 66 + i * 24
        draw.line([(px, cy - 20), (px + 8, cy - 53)], fill=pin_color, width=3)
        draw.line([(px + 10, cy + 29), (px + 19, cy + 58)], fill=pin_color, width=3)


def draw_label(
    base: Image.Image,
    *,
    anchor: tuple[int, int],
    box_xy: tuple[int, int],
    text: str,
    accent: tuple[int, int, int, int],
) -> None:
    draw = ImageDraw.Draw(base, "RGBA")
    font = load_font(28, bold=True)
    sub_font = load_font(18, bold=False)
    label_text, detail_text = text.split("|", 1)

    box_x, box_y = box_xy
    box_w, box_h = 250, 76
    connector = [anchor, (anchor[0] + 38, anchor[1] - 18), (box_x, box_y + box_h // 2)]
    draw.line(connector, fill=accent, width=4)
    draw.rounded_rectangle(
        (box_x, box_y, box_x + box_w, box_y + box_h),
        radius=22,
        fill=rgba("#0A1322", 222),
        outline=accent,
        width=3,
    )
    draw.text((box_x + 20, box_y + 13), label_text, font=font, fill=rgba("#F7FEFF"))
    draw.text((box_x + 20, box_y + 44), detail_text, font=sub_font, fill=rgba("#B8D7E2"))


def add_glow(base: Image.Image, center: tuple[int, int], radius: int, color: tuple[int, int, int, int]) -> None:
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(glow, "RGBA")
    x, y = center
    draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=color)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=radius // 2))
    base.alpha_composite(glow)


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    canvas = Image.new("RGBA", (CANVAS_W, CANVAS_H), (0, 0, 0, 0))

    add_glow(canvas, (780, 720), 240, rgba("#1CC7C3", 70))
    add_glow(canvas, (980, 490), 200, rgba("#69D9FF", 55))

    base_layer = draw_prism(
        canvas,
        x=380,
        y=760,
        w=620,
        h=165,
        depth_x=190,
        depth_y=105,
        top_color=rgba("#1B3E4D"),
        front_color=rgba("#102B38"),
        side_color=rgba("#163544"),
        outline=rgba("#6FDDE5", 210),
        edge_glow=rgba("#6FDDE5", 120),
    )
    draw_shadow(canvas, base_layer["top"], base_layer["front"][2][1] - base_layer["front"][1][1], blur=28)

    draw = ImageDraw.Draw(canvas, "RGBA")
    for x_offset in range(35, 560, 85):
        draw.rounded_rectangle(
            (420 + x_offset, 816, 420 + x_offset + 46, 836),
            radius=9,
            fill=rgba("#1F5869", 230),
        )
    for y_offset in range(0, 3):
        draw.line(
            [(1030, 782 + y_offset * 28), (1148, 716 + y_offset * 28)],
            fill=rgba("#5DDCEA", 140),
            width=5,
        )

    tray_layer = draw_prism(
        canvas,
        x=455,
        y=645,
        w=535,
        h=55,
        depth_x=160,
        depth_y=88,
        top_color=rgba("#233C57"),
        front_color=rgba("#1A2E44"),
        side_color=rgba("#20374F"),
        outline=rgba("#8CE8FF", 190),
        edge_glow=rgba("#72DBFF", 125),
    )

    draw_module_cells(
        canvas,
        x=505,
        y=596,
        cols=6,
        rows=2,
        cell_w=56,
        cell_h=96,
        gap_x=16,
        gap_y=36,
        depth_x=28,
        depth_y=18,
        top_color=rgba("#5DE9C3"),
        front_color=rgba("#159C84"),
        side_color=rgba("#30C9A7"),
        outline=rgba("#DBFFF4", 210),
    )

    barrier_layer = draw_prism(
        canvas,
        x=490,
        y=500,
        w=490,
        h=18,
        depth_x=148,
        depth_y=82,
        top_color=rgba("#F3B25A"),
        front_color=rgba("#DB8D30"),
        side_color=rgba("#E7A34E"),
        outline=rgba("#FFE6B5", 220),
        edge_glow=rgba("#FFD489", 120),
    )

    cooling_layer = draw_prism(
        canvas,
        x=520,
        y=433,
        w=455,
        h=28,
        depth_x=138,
        depth_y=76,
        top_color=rgba("#4AA9D7"),
        front_color=rgba("#2374A2"),
        side_color=rgba("#3289B8"),
        outline=rgba("#D7F7FF", 210),
        edge_glow=rgba("#B2F3FF", 145),
    )
    draw_cooling_pattern(canvas, cooling_layer)

    top_controller = draw_prism(
        canvas,
        x=670,
        y=310,
        w=255,
        h=70,
        depth_x=96,
        depth_y=54,
        top_color=rgba("#203A62"),
        front_color=rgba("#162A47"),
        side_color=rgba("#1B3355"),
        outline=rgba("#86E7FF", 210),
        edge_glow=rgba("#57D5FF", 135),
    )
    draw_chip(canvas, top_controller)

    arc = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    arc_draw = ImageDraw.Draw(arc, "RGBA")
    arc_draw.arc((420, 235, 1060, 820), start=208, end=328, fill=rgba("#5DE9C3", 170), width=7)
    arc_draw.arc((505, 215, 1160, 860), start=210, end=318, fill=rgba("#7CD8FF", 130), width=5)
    arc = arc.filter(ImageFilter.GaussianBlur(radius=1))
    canvas.alpha_composite(arc)

    ring_color = rgba("#B8FBFF", 180)
    for px, py, radius in [(632, 586, 12), (742, 546, 10), (856, 511, 10), (912, 390, 12)]:
        draw.ellipse((px - radius, py - radius, px + radius, py + radius), outline=ring_color, width=4)

    title_font = load_font(54, bold=True)
    subtitle_font = load_font(24)
    draw.text((185, 110), "ESS Battery 3D Architecture", font=title_font, fill=rgba("#EFFFFF"))
    draw.text((190, 176), "Exploded modular stack for utility-scale energy storage", font=subtitle_font, fill=rgba("#92C7D7"))

    draw_label(
        canvas,
        anchor=(960, 323),
        box_xy=(1220, 255),
        text="BMS 제어부|Pack / Rack Control",
        accent=rgba("#71E7FF"),
    )
    draw_label(
        canvas,
        anchor=(1020, 430),
        box_xy=(1260, 410),
        text="냉각 플레이트|Liquid Cooling Layer",
        accent=rgba("#8BF1FF"),
    )
    draw_label(
        canvas,
        anchor=(1010, 500),
        box_xy=(1240, 560),
        text="절연 보호층|Thermal Barrier",
        accent=rgba("#FFD68F"),
    )
    draw_label(
        canvas,
        anchor=(914, 598),
        box_xy=(1210, 705),
        text="배터리 모듈|LFP Cell Array",
        accent=rgba("#88FFD8"),
    )
    draw_label(
        canvas,
        anchor=(984, 764),
        box_xy=(1180, 885),
        text="랙 하우징|ESS Rack Chassis",
        accent=rgba("#7ADBE7"),
    )

    canvas.save(OUTPUT_PATH)
    print(OUTPUT_PATH)


if __name__ == "__main__":
    main()
