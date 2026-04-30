from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parent
OUTPUT_PATH = ROOT / "EMS_PJT_Hamburger" / "Assets" / "ESS_View.png"

SIZE = 768


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    value = hex_color.lstrip("#")
    return (
        int(value[0:2], 16),
        int(value[2:4], 16),
        int(value[4:6], 16),
        alpha,
    )


def add_glow(base: Image.Image, draw_fn, blur_radius: int = 12) -> None:
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow, "RGBA")
    draw_fn(glow_draw)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    base.alpha_composite(glow)


def draw_prism(
    draw: ImageDraw.ImageDraw,
    *,
    x: int,
    y: int,
    w: int,
    h: int,
    dx: int,
    dy: int,
    top_fill: tuple[int, int, int, int],
    front_fill: tuple[int, int, int, int],
    side_fill: tuple[int, int, int, int],
    outline: tuple[int, int, int, int],
    outline_width: int = 3,
) -> dict[str, list[tuple[int, int]]]:
    front = [(x, y), (x + w, y), (x + w, y + h), (x, y + h)]
    top = [(x, y), (x + dx, y - dy), (x + w + dx, y - dy), (x + w, y)]
    side = [(x + w, y), (x + w + dx, y - dy), (x + w + dx, y + h - dy), (x + w, y + h)]

    draw.polygon(top, fill=top_fill)
    draw.polygon(side, fill=side_fill)
    draw.polygon(front, fill=front_fill)

    for poly in (top, side, front):
        draw.line(poly + [poly[0]], fill=outline, width=outline_width)

    return {"top": top, "front": front, "side": side}


def draw_module(
    draw: ImageDraw.ImageDraw,
    *,
    x: int,
    y: int,
    w: int,
    h: int,
    accent: tuple[int, int, int, int],
) -> None:
    draw.rounded_rectangle(
        (x, y, x + w, y + h),
        radius=12,
        fill=rgba("#132334", 235),
        outline=rgba("#6FD9FF", 190),
        width=2,
    )
    draw.rounded_rectangle(
        (x + 10, y + 10, x + w - 10, y + h - 20),
        radius=10,
        fill=rgba("#0C1826", 220),
        outline=rgba("#2B4C67", 210),
        width=2,
    )
    draw.rectangle((x + 18, y + 18, x + w - 18, y + 28), fill=rgba("#1A3248", 230))
    for row in range(3):
        yy = y + 38 + row * 18
        draw.line((x + 18, yy, x + w - 18, yy), fill=rgba("#1E3A50", 205), width=2)
    draw.rounded_rectangle(
        (x + w - 54, y + h - 38, x + w - 18, y + h - 14),
        radius=7,
        fill=accent,
    )


def main() -> None:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    canvas = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas, "RGBA")

    add_glow(
        canvas,
        lambda d: d.ellipse((170, 150, 670, 610), fill=rgba("#1ED4FF", 38)),
        blur_radius=40,
    )
    add_glow(
        canvas,
        lambda d: d.ellipse((210, 180, 600, 540), fill=rgba("#48E6C3", 28)),
        blur_radius=26,
    )

    rack = draw_prism(
        draw,
        x=190,
        y=250,
        w=270,
        h=300,
        dx=130,
        dy=72,
        top_fill=rgba("#27384C", 248),
        front_fill=rgba("#172635", 248),
        side_fill=rgba("#203445", 248),
        outline=rgba("#8BE8FF", 210),
        outline_width=4,
    )

    add_glow(
        canvas,
        lambda d: d.line([rack["top"][1], rack["top"][2], rack["side"][2]], fill=rgba("#71E7FF", 130), width=8),
        blur_radius=10,
    )

    top_face = rack["top"]
    draw.polygon(
        [
            (top_face[0][0] + 42, top_face[0][1] - 18),
            (top_face[0][0] + 98, top_face[0][1] - 50),
            (top_face[0][0] + 144, top_face[0][1] - 26),
            (top_face[0][0] + 88, top_face[0][1] + 4),
        ],
        fill=rgba("#152636", 240),
        outline=rgba("#85E2FF", 190),
    )
    draw.line(
        [
            (top_face[0][0] + 65, top_face[0][1] - 15),
            (top_face[0][0] + 92, top_face[0][1] - 28),
            (top_face[0][0] + 120, top_face[0][1] - 13),
        ],
        fill=rgba("#7AE1FF", 180),
        width=3,
    )

    draw.rounded_rectangle(
        (220, 270, 290, 314),
        radius=10,
        fill=rgba("#0C1521", 230),
        outline=rgba("#5CD8FF", 180),
        width=2,
    )
    draw.rectangle((233, 282, 273, 301), fill=rgba("#6CF8D0", 200))
    for idx in range(3):
        draw.ellipse((251 + idx * 14, 287, 258 + idx * 14, 294), fill=rgba("#103B36", 255))

    accent_green = rgba("#5CF2C5", 235)
    draw_module(draw, x=224, y=334, w=202, h=58, accent=accent_green)
    draw_module(draw, x=224, y=402, w=202, h=58, accent=accent_green)
    draw_module(draw, x=224, y=470, w=202, h=58, accent=accent_green)

    draw.rounded_rectangle(
        (457, 290, 520, 520),
        radius=16,
        fill=rgba("#132435", 235),
        outline=rgba("#7EDDF6", 180),
        width=2,
    )
    for idx in range(7):
        y = 312 + idx * 27
        draw.line((472, y, 507, y - 18), fill=rgba("#436B88", 180), width=3)

    for idx in range(3):
        draw.rounded_rectangle(
            (540, 332 + idx * 54, 583, 363 + idx * 54),
            radius=10,
            fill=rgba("#0F1D2C", 225),
            outline=rgba("#4FD3FF", 160),
            width=2,
        )
        draw.line((550, 348 + idx * 54, 573, 348 + idx * 54), fill=rgba("#69EEFF", 170), width=3)

    cable_points = [(330, 232), (382, 203), (438, 230), (492, 200), (550, 228)]
    add_glow(
        canvas,
        lambda d: d.line(cable_points, fill=rgba("#78EBFF", 85), width=11),
        blur_radius=8,
    )
    draw.line(cable_points, fill=rgba("#89F0FF", 165), width=4)

    battery_outline = [
        (330, 192),
        (376, 166),
        (422, 188),
        (377, 214),
    ]
    draw.polygon(battery_outline, outline=rgba("#9CFBFF", 185), fill=rgba("#112235", 130), width=3)
    draw.line((364, 189, 388, 189), fill=rgba("#63F2CF", 190), width=3)
    draw.line((376, 177, 376, 202), fill=rgba("#63F2CF", 190), width=3)

    add_glow(
        canvas,
        lambda d: d.rounded_rectangle((216, 264, 588, 548), radius=28, outline=rgba("#49DFFF", 42), width=10),
        blur_radius=18,
    )

    canvas.save(OUTPUT_PATH)
    print(OUTPUT_PATH)


if __name__ == "__main__":
    main()
