from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from PIL import Image, ImageDraw, ImageFilter, ImageFont


ROOT = Path(__file__).resolve().parent
OUTPUT_DIR = ROOT / "EMS_PJT_Hamburger" / "Assets" / "Generated"


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    value = hex_color.lstrip("#")
    return (
        int(value[0:2], 16),
        int(value[2:4], 16),
        int(value[4:6], 16),
        alpha,
    )


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


def lerp(a: tuple[float, float], b: tuple[float, float], t: float) -> tuple[float, float]:
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


def to_int_points(points: Iterable[tuple[float, float]]) -> list[tuple[int, int]]:
    return [(int(round(x)), int(round(y))) for x, y in points]


@dataclass
class IsoProjector:
    origin_x: float
    origin_y: float
    scale: float

    def p(self, x: float, y: float, z: float) -> tuple[float, float]:
        return (
            self.origin_x + self.scale * (0.95 * x - 0.62 * y),
            self.origin_y + self.scale * (0.28 * x + 0.36 * y - z),
        )


@dataclass
class ContainerSpec:
    length: float = 870
    depth: float = 280
    height: float = 420


def draw_face(
    draw: ImageDraw.ImageDraw,
    points: list[tuple[float, float]],
    *,
    fill: tuple[int, int, int, int],
    outline: tuple[int, int, int, int],
    outline_width: int,
) -> None:
    poly = to_int_points(points)
    draw.polygon(poly, fill=fill)
    draw.line(poly + [poly[0]], fill=outline, width=outline_width)


def add_glow(base: Image.Image, draw_fn, blur_radius: int) -> None:
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow, "RGBA")
    draw_fn(glow_draw)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    base.alpha_composite(glow)


def add_soft_background(img: Image.Image) -> None:
    width, height = img.size
    pixels = img.load()
    for y in range(height):
        t = y / max(height - 1, 1)
        shade = int(248 - 10 * t)
        for x in range(width):
            x_wave = (x / max(width - 1, 1)) * 8
            local = shade - int(abs(x_wave - 4) * 1.8)
            pixels[x, y] = (max(local, 236), max(local, 236), max(local + 2, 238), 255)


def side_rect(prj: IsoProjector, x0: float, x1: float, z0: float, z1: float) -> list[tuple[float, float]]:
    return [prj.p(x0, 0, z0), prj.p(x1, 0, z0), prj.p(x1, 0, z1), prj.p(x0, 0, z1)]


def front_rect(prj: IsoProjector, y0: float, y1: float, z0: float, z1: float) -> list[tuple[float, float]]:
    return [prj.p(0, y0, z0), prj.p(0, y1, z0), prj.p(0, y1, z1), prj.p(0, y0, z1)]


def top_rect(prj: IsoProjector, x0: float, x1: float, y0: float, y1: float, z: float) -> list[tuple[float, float]]:
    return [prj.p(x0, y0, z), prj.p(x1, y0, z), prj.p(x1, y1, z), prj.p(x0, y1, z)]


def draw_side_vent(
    draw: ImageDraw.ImageDraw,
    prj: IsoProjector,
    *,
    x0: float,
    x1: float,
    z0: float,
    z1: float,
    outline: tuple[int, int, int, int],
) -> None:
    draw_face(
        draw,
        side_rect(prj, x0, x1, z0, z1),
        fill=rgba("#EEF2F6", 245),
        outline=outline,
        outline_width=3,
    )
    for step in range(6):
        z = z0 + 10 + step * ((z1 - z0 - 20) / 5)
        a = prj.p(x0 + 10, 0, z)
        b = prj.p(x1 - 10, 0, z)
        draw.line(to_int_points([a, b]), fill=rgba("#9BA6B4", 210), width=3)


def draw_front_vent(
    draw: ImageDraw.ImageDraw,
    prj: IsoProjector,
    *,
    y0: float,
    y1: float,
    z0: float,
    z1: float,
    outline: tuple[int, int, int, int],
) -> None:
    draw_face(
        draw,
        front_rect(prj, y0, y1, z0, z1),
        fill=rgba("#F1F4F7", 245),
        outline=outline,
        outline_width=3,
    )
    for step in range(6):
        z = z0 + 10 + step * ((z1 - z0 - 20) / 5)
        a = prj.p(0, y0 + 8, z)
        b = prj.p(0, y1 - 8, z)
        draw.line(to_int_points([a, b]), fill=rgba("#A1ABB9", 215), width=3)


def draw_warning_front(draw: ImageDraw.ImageDraw, prj: IsoProjector, y: float, z: float) -> None:
    p1 = prj.p(0, y, z)
    p2 = prj.p(0, y + 28, z + 48)
    p3 = prj.p(0, y + 56, z)
    draw.polygon(to_int_points([p1, p2, p3]), fill=rgba("#F6D93B", 255), outline=rgba("#383838", 230))
    cx = (p1[0] + p2[0] + p3[0]) / 3
    cy = (p1[1] + p2[1] + p3[1]) / 3
    draw.line([(int(cx), int(cy - 14)), (int(cx), int(cy + 4))], fill=rgba("#2C2C2C", 255), width=4)
    draw.ellipse((int(cx - 3), int(cy + 10), int(cx + 3), int(cy + 16)), fill=rgba("#2C2C2C", 255))


def draw_warning_side(draw: ImageDraw.ImageDraw, prj: IsoProjector, x: float, z: float) -> None:
    p1 = prj.p(x, 0, z)
    p2 = prj.p(x + 26, 0, z + 48)
    p3 = prj.p(x + 52, 0, z)
    draw.polygon(to_int_points([p1, p2, p3]), fill=rgba("#F6D93B", 255), outline=rgba("#383838", 230))
    cx = (p1[0] + p2[0] + p3[0]) / 3
    cy = (p1[1] + p2[1] + p3[1]) / 3
    draw.line([(int(cx), int(cy - 14)), (int(cx), int(cy + 4))], fill=rgba("#2C2C2C", 255), width=4)
    draw.ellipse((int(cx - 3), int(cy + 10), int(cx + 3), int(cy + 16)), fill=rgba("#2C2C2C", 255))


def draw_side_panel(
    base: Image.Image,
    prj: IsoProjector,
    *,
    icon_mode: bool,
) -> None:
    draw = ImageDraw.Draw(base, "RGBA")
    panel = side_rect(prj, 220, 700, 110, 318)
    draw_face(
        draw,
        panel,
        fill=rgba("#FBFCFD", 250),
        outline=rgba("#D6DBE3", 255),
        outline_width=4,
    )
    inner = side_rect(prj, 245, 675, 132, 296)
    draw_face(
        draw,
        inner,
        fill=rgba("#FFFFFF", 252),
        outline=rgba("#E3E8EE", 255),
        outline_width=3,
    )

    left_mid = lerp(inner[0], inner[3], 0.52)
    right_mid = lerp(inner[1], inner[2], 0.52)

    if icon_mode:
        for idx in range(7):
            t = idx / 6
            p = lerp(inner[0], inner[3], 0.18 + t * 0.62)
            q = (p[0] + 38, p[1] - 18)
            draw.line(to_int_points([p, q]), fill=rgba("#4DA7FF", 210), width=6)
        bolt = [
            (left_mid[0] + 8, left_mid[1] - 48),
            (left_mid[0] + 34, left_mid[1] - 48),
            (left_mid[0] + 12, left_mid[1] - 4),
            (left_mid[0] + 40, left_mid[1] - 4),
            (left_mid[0] - 6, left_mid[1] + 44),
            (left_mid[0] + 8, left_mid[1] + 4),
        ]
        draw.polygon(to_int_points(bolt), fill=rgba("#4E87FF", 220))
    else:
        stripe_x = left_mid[0] + 16
        stripe_y = left_mid[1] - 70
        for idx in range(7):
            draw.line(
                [(int(stripe_x + idx * 7), int(stripe_y + idx * 22)), (int(stripe_x + idx * 7 + 20), int(stripe_y + idx * 22))],
                fill=rgba("#44A8FF", 210),
                width=6,
            )
        big_font = load_font(48, bold=True)
        sub_font = load_font(28, bold=True)
        draw.text((int(left_mid[0] + 120), int(left_mid[1] - 76)), "ESS", font=big_font, fill=rgba("#2F9BF7"))
        draw.text((int(left_mid[0] + 118), int(left_mid[1] - 12)), "GRID STORAGE", font=sub_font, fill=rgba("#4E57B8"))

    draw.line(to_int_points([left_mid, right_mid]), fill=rgba("#E7ECF2", 170), width=2)


def draw_front_doors(draw: ImageDraw.ImageDraw, prj: IsoProjector, spec: ContainerSpec) -> None:
    seams = [65, 128, 194, 245]
    for y in seams:
        a = prj.p(0, y, 30)
        b = prj.p(0, y, spec.height - 18)
        draw.line(to_int_points([a, b]), fill=rgba("#BCC4CF", 255), width=3)

    for y in [72, 135, 202]:
        top = prj.p(0, y, 260)
        bottom = prj.p(0, y + 18, 110)
        draw.line(to_int_points([top, bottom]), fill=rgba("#DEE4EA", 210), width=3)

    handle_a = front_rect(prj, 119, 129, 160, 290)
    handle_b = front_rect(prj, 186, 196, 160, 290)
    draw_face(draw, handle_a, fill=rgba("#D2D7DE", 255), outline=rgba("#949EAA", 255), outline_width=2)
    draw_face(draw, handle_b, fill=rgba("#D2D7DE", 255), outline=rgba("#949EAA", 255), outline_width=2)


def draw_control_box(draw: ImageDraw.ImageDraw, prj: IsoProjector) -> None:
    body = front_rect(prj, 150, 238, 210, 330)
    draw_face(draw, body, fill=rgba("#B9C2CB", 255), outline=rgba("#717B86", 255), outline_width=3)
    display = front_rect(prj, 168, 208, 260, 306)
    draw_face(draw, display, fill=rgba("#38414A", 255), outline=rgba("#99A8B8", 210), outline_width=2)
    screen = front_rect(prj, 175, 195, 275, 294)
    draw_face(draw, screen, fill=rgba("#7FD6FF", 220), outline=rgba("#A6E8FF", 200), outline_width=2)

    led_1 = front_rect(prj, 213, 220, 260, 273)
    led_2 = front_rect(prj, 226, 233, 260, 273)
    led_3 = front_rect(prj, 213, 220, 242, 255)
    draw_face(draw, led_1, fill=rgba("#48EF92", 255), outline=rgba("#C6F7D4", 255), outline_width=1)
    draw_face(draw, led_2, fill=rgba("#FFCA5A", 255), outline=rgba("#FFE8B6", 255), outline_width=1)
    draw_face(draw, led_3, fill=rgba("#7FD6FF", 255), outline=rgba("#D6F4FF", 255), outline_width=1)

    wire_paths = [
        [prj.p(0, 177, 209), prj.p(0, 177, 140), prj.p(0, 170, 80)],
        [prj.p(0, 198, 209), prj.p(0, 198, 145), prj.p(0, 192, 78)],
        [prj.p(0, 219, 209), prj.p(0, 219, 148), prj.p(0, 212, 84)],
    ]
    for path in wire_paths:
        draw.line(to_int_points(path), fill=rgba("#C9D0D9", 230), width=3)


def draw_rails(draw: ImageDraw.ImageDraw, prj: IsoProjector, spec: ContainerSpec) -> None:
    upper = [prj.p(8, 0, 14), prj.p(spec.length + 34, 0, 14), prj.p(spec.length + 34, 0, 0), prj.p(8, 0, 0)]
    lower = [prj.p(8, 0, -12), prj.p(spec.length + 34, 0, -12), prj.p(spec.length + 34, 0, -28), prj.p(8, 0, -28)]
    draw_face(draw, upper, fill=rgba("#C1C7D0", 255), outline=rgba("#929BA8", 255), outline_width=2)
    draw_face(draw, lower, fill=rgba("#AEB6C1", 255), outline=rgba("#7E8795", 255), outline_width=2)
    for idx in range(12):
        x = 48 + idx * 70
        a = prj.p(x, 0, 3)
        b = prj.p(x + 12, 0, 3)
        draw.line(to_int_points([a, b]), fill=rgba("#7A8390", 190), width=2)


def draw_rivets(draw: ImageDraw.ImageDraw, prj: IsoProjector, spec: ContainerSpec) -> None:
    for x in range(40, int(spec.length), 110):
        for z in [38, spec.height - 24]:
            point = prj.p(x, 0, z)
            draw.ellipse((int(point[0] - 4), int(point[1] - 4), int(point[0] + 4), int(point[1] + 4)), fill=rgba("#AAB2BD"))
    for y in range(28, int(spec.depth), 62):
        for z in [26, spec.height - 20]:
            point = prj.p(0, y, z)
            draw.ellipse((int(point[0] - 4), int(point[1] - 4), int(point[0] + 4), int(point[1] + 4)), fill=rgba("#AAB2BD"))


def render_container_variant(
    *,
    width: int,
    height: int,
    background: str,
    icon_mode: bool,
) -> Image.Image:
    aa = 2
    base_size = (width * aa, height * aa)
    img = Image.new("RGBA", base_size, (0, 0, 0, 0))
    if background == "light":
        add_soft_background(img)

    draw = ImageDraw.Draw(img, "RGBA")
    s = min(width / 1600, height / 1000)
    scale = s * aa
    origin_x = 350 * scale if not icon_mode else 410 * scale
    origin_y = 565 * scale if not icon_mode else 610 * scale
    prj = IsoProjector(origin_x, origin_y, scale)
    spec = ContainerSpec()

    if background == "light":
        add_glow(
            img,
            lambda d: d.ellipse(
                (
                    int(origin_x - 240 * scale),
                    int(origin_y - 120 * scale),
                    int(origin_x + 1150 * scale),
                    int(origin_y + 410 * scale),
                ),
                fill=rgba("#AEB7C1", 55),
            ),
            blur_radius=int(55 * scale),
        )

    if icon_mode:
        add_glow(
            img,
            lambda d: d.ellipse(
                (
                    int(origin_x - 180 * scale),
                    int(origin_y - 260 * scale),
                    int(origin_x + 930 * scale),
                    int(origin_y + 250 * scale),
                ),
                fill=rgba("#53E0FF", 42),
            ),
            blur_radius=int(48 * scale),
        )

    shadow_poly = [
        prj.p(-24, -10, -4),
        prj.p(spec.length + 120, -10, -4),
        prj.p(spec.length + 120, spec.depth + 36, -4),
        prj.p(-24, spec.depth + 36, -4),
    ]
    if background == "light":
        add_glow(
            img,
            lambda d: d.polygon(to_int_points(shadow_poly), fill=rgba("#5B6470", 88)),
            blur_radius=int(34 * scale),
        )

    top = top_rect(prj, 0, spec.length, 0, spec.depth, spec.height)
    side = side_rect(prj, 0, spec.length, 0, spec.height)
    front = front_rect(prj, 0, spec.depth, 0, spec.height)

    draw_face(draw, top, fill=rgba("#F8FAFC", 255), outline=rgba("#C8D0D8", 255), outline_width=max(2, int(3 * scale)))
    draw_face(draw, side, fill=rgba("#EEF2F6", 255), outline=rgba("#C2CBD5", 255), outline_width=max(2, int(3 * scale)))
    draw_face(draw, front, fill=rgba("#E4E9EF", 255), outline=rgba("#BDC6D1", 255), outline_width=max(2, int(3 * scale)))

    roof_cap = top_rect(prj, 10, spec.length - 25, 15, spec.depth - 8, spec.height + 10)
    draw_face(draw, roof_cap, fill=rgba("#FCFDFE", 220), outline=rgba("#E2E7ED", 210), outline_width=max(2, int(2 * scale)))

    for x in [120, 618]:
        vent = top_rect(prj, x, x + 72, 14, 54, spec.height + 6)
        draw_face(draw, vent, fill=rgba("#D6DCE3", 240), outline=rgba("#9AA5B2", 230), outline_width=max(1, int(2 * scale)))
        for step in range(5):
            y0 = 18 + step * 6
            line = [prj.p(x + 8, y0, spec.height + 9), prj.p(x + 62, y0, spec.height + 9)]
            draw.line(to_int_points(line), fill=rgba("#7C8795", 180), width=max(1, int(2 * scale)))

    for x in [60, 210, 360, 510, 660, 810]:
        a = prj.p(x, 0, 40)
        b = prj.p(x, 0, spec.height - 20)
        draw.line(to_int_points([a, b]), fill=rgba("#D9E0E7", 135), width=max(1, int(2 * scale)))

    draw_side_panel(img, prj, icon_mode=icon_mode)
    draw_front_doors(draw, prj, spec)
    draw_control_box(draw, prj)

    draw_front_vent(draw, prj, y0=24, y1=56, z0=28, z1=118, outline=rgba("#A6B1BE", 255))
    draw_front_vent(draw, prj, y0=24, y1=56, z0=260, z1=350, outline=rgba("#A6B1BE", 255))
    draw_side_vent(draw, prj, x0=150, x1=230, z0=315, z1=382, outline=rgba("#AAB5C2", 255))
    draw_side_vent(draw, prj, x0=546, x1=620, z0=24, z1=58, outline=rgba("#AAB5C2", 255))
    draw_side_vent(draw, prj, x0=808, x1=852, z0=310, z1=362, outline=rgba("#AAB5C2", 255))
    draw_side_vent(draw, prj, x0=816, x1=852, z0=115, z1=173, outline=rgba("#AAB5C2", 255))

    small_box = side_rect(prj, 160, 220, 54, 112)
    draw_face(draw, small_box, fill=rgba("#F4F7FA", 250), outline=rgba("#B3BFCA", 255), outline_width=max(2, int(2 * scale)))
    detail = side_rect(prj, 176, 206, 70, 92)
    draw_face(draw, detail, fill=rgba("#C7D3DF", 255), outline=rgba("#9EABB8", 255), outline_width=max(1, int(2 * scale)))
    draw.line(to_int_points([prj.p(188, 0, 68), prj.p(188, 0, 42), prj.p(186, 0, 22)]), fill=rgba("#C5CCD5", 210), width=max(1, int(2 * scale)))

    draw_warning_front(draw, prj, 36, 126)
    draw_warning_side(draw, prj, 788, 118)

    draw_rails(draw, prj, spec)
    draw_rivets(draw, prj, spec)

    if icon_mode:
        add_glow(
            img,
            lambda d: d.line(
                to_int_points([prj.p(240, 0, 208), prj.p(708, 0, 208)]),
                fill=rgba("#69E7FF", 130),
                width=max(5, int(6 * scale)),
            ),
            blur_radius=int(12 * scale),
        )

    return img.resize((width, height), Image.Resampling.LANCZOS)


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    original = render_container_variant(width=1800, height=1100, background="light", icon_mode=False)
    transparent = render_container_variant(width=1800, height=1100, background="transparent", icon_mode=False)
    panel_icon = render_container_variant(width=900, height=900, background="transparent", icon_mode=True)

    original.save(OUTPUT_DIR / "ESS_Container_3D_Original.png")
    transparent.save(OUTPUT_DIR / "ESS_Container_3D_Transparent.png")
    panel_icon.save(OUTPUT_DIR / "ESS_Container_Panel_Icon.png")

    print(OUTPUT_DIR / "ESS_Container_3D_Original.png")
    print(OUTPUT_DIR / "ESS_Container_3D_Transparent.png")
    print(OUTPUT_DIR / "ESS_Container_Panel_Icon.png")


if __name__ == "__main__":
    main()
