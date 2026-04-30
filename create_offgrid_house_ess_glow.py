from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parent
OUTPUT_PATH = ROOT / "EMS_PJT_Hamburger" / "Assets" / "Home" / "OffGrid_House_ESS_Glow.png"
SIZE = 1024


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    value = hex_color.lstrip("#")
    return (
        int(value[0:2], 16),
        int(value[2:4], 16),
        int(value[4:6], 16),
        alpha,
    )


def add_glow(base: Image.Image, draw_fn, blur_radius: int) -> None:
    glow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow, "RGBA")
    draw_fn(glow_draw)
    glow = glow.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    base.alpha_composite(glow)


def draw_house(draw: ImageDraw.ImageDraw) -> None:
    front_wall = [(360, 418), (621, 418), (621, 728), (360, 728)]
    side_wall = [(621, 418), (732, 354), (732, 664), (621, 728)]
    roof_front = [(340, 422), (490, 306), (642, 422)]
    roof_side = [(490, 306), (642, 422), (746, 360), (596, 248)]
    base_step = [(345, 728), (643, 728), (693, 760), (380, 760)]

    draw.polygon(base_step, fill=rgba("#BFC7D1", 235), outline=rgba("#8591A1", 190))
    draw.polygon(side_wall, fill=rgba("#E4EBF2", 250), outline=rgba("#C7D2DD", 230))
    draw.polygon(front_wall, fill=rgba("#F9FCFF", 252), outline=rgba("#D7E0E7", 240))
    draw.polygon(roof_side, fill=rgba("#1B2535", 250), outline=rgba("#74B7FF", 120))
    draw.polygon(roof_front, fill=rgba("#243249", 250), outline=rgba("#85C2FF", 130))

    draw.rectangle((382, 452, 475, 538), fill=rgba("#FFD783", 235), outline=rgba("#FFF3CC", 220), width=4)
    draw.rectangle((505, 452, 598, 538), fill=rgba("#FFD783", 235), outline=rgba("#FFF3CC", 220), width=4)
    draw.rectangle((398, 572, 505, 716), fill=rgba("#F5F8FC", 252), outline=rgba("#D8E2EA", 230), width=4)
    draw.rectangle((522, 582, 596, 664), fill=rgba("#FFD995", 230), outline=rgba("#FFF1CC", 215), width=4)

    side_window_top = [(640, 452), (694, 420), (694, 500), (640, 532)]
    side_window_bottom = [(640, 552), (694, 520), (694, 604), (640, 636)]
    draw.polygon(side_window_top, fill=rgba("#FFD68A", 230), outline=rgba("#FFF1D2", 215))
    draw.polygon(side_window_bottom, fill=rgba("#FFD68A", 230), outline=rgba("#FFF1D2", 215))

    draw.rectangle((514, 598, 604, 706), fill=rgba("#EDF2F7", 252), outline=rgba("#D0D9E2", 225), width=4)
    for y in range(460, 720, 52):
        draw.line((621, y, 732, y - 64), fill=rgba("#F4F7FA", 70), width=2)
    draw.line((360, 428, 621, 428), fill=rgba("#FFFFFF", 110), width=4)
    draw.line((621, 418, 732, 354), fill=rgba("#FFFFFF", 90), width=3)


def draw_ess(draw: ImageDraw.ImageDraw) -> None:
    front = [(190, 640), (276, 640), (276, 774), (190, 774)]
    side = [(276, 640), (326, 612), (326, 744), (276, 774)]
    top = [(190, 640), (240, 612), (326, 612), (276, 640)]

    draw.polygon(side, fill=rgba("#162030", 242), outline=rgba("#4FD6FF", 140))
    draw.polygon(front, fill=rgba("#0F1726", 246), outline=rgba("#6DE7FF", 160))
    draw.polygon(top, fill=rgba("#1E2A3D", 246), outline=rgba("#77EDFF", 160))

    for idx in range(3):
        y = 664 + idx * 32
        draw.rounded_rectangle(
            (206, y, 259, y + 18),
            radius=6,
            fill=rgba("#1A2C43", 240),
            outline=rgba("#3A5875", 180),
            width=2,
        )
    draw.rounded_rectangle((208, 676, 250, 690), radius=6, fill=rgba("#50F0C5", 230))
    draw.rounded_rectangle((208, 708, 238, 722), radius=6, fill=rgba("#57C8FF", 225))
    draw.rounded_rectangle((208, 740, 226, 754), radius=6, fill=rgba("#9AE7FF", 220))

    bolt = [(292, 665), (307, 665), (296, 693), (314, 693), (288, 730), (295, 703), (280, 703)]
    draw.polygon(bolt, fill=rgba("#6CEEFF", 220))


def draw_energy_flow(base: Image.Image) -> None:
    add_glow(
        base,
        lambda d: d.line([(318, 704), (386, 704), (440, 680), (468, 648), (486, 640)], fill=rgba("#45EAFF", 130), width=18),
        blur_radius=14,
    )
    add_glow(
        base,
        lambda d: d.line([(318, 732), (392, 732), (452, 700), (492, 670)], fill=rgba("#77FFD8", 110), width=14),
        blur_radius=12,
    )
    draw = ImageDraw.Draw(base, "RGBA")
    draw.line([(318, 704), (386, 704), (440, 680), (468, 648), (486, 640)], fill=rgba("#8DF7FF", 235), width=6)
    draw.line([(318, 732), (392, 732), (452, 700), (492, 670)], fill=rgba("#B8FFD4", 220), width=5)
    for point in [(350, 704), (402, 694), (452, 670), (382, 732), (436, 708)]:
        add_glow(
            base,
            lambda d, p=point: d.ellipse((p[0] - 10, p[1] - 10, p[0] + 10, p[1] + 10), fill=rgba("#8FF5FF", 140)),
            blur_radius=8,
        )


def main() -> None:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    canvas = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))

    add_glow(
        canvas,
        lambda d: d.ellipse((180, 180, 900, 930), fill=rgba("#0E1A30", 120)),
        blur_radius=90,
    )
    add_glow(
        canvas,
        lambda d: d.ellipse((220, 210, 850, 860), fill=rgba("#17345C", 80)),
        blur_radius=70,
    )
    add_glow(
        canvas,
        lambda d: d.ellipse((330, 420, 705, 850), fill=rgba("#4D7CFF", 32)),
        blur_radius=46,
    )
    add_glow(
        canvas,
        lambda d: d.ellipse((160, 740, 760, 875), fill=rgba("#4ACFFF", 70)),
        blur_radius=34,
    )

    draw = ImageDraw.Draw(canvas, "RGBA")
    draw_house(draw)
    draw_ess(draw)
    draw_energy_flow(canvas)

    add_glow(
        canvas,
        lambda d: d.rectangle((374, 444, 600, 716), outline=rgba("#FFF4B8", 42), width=8),
        blur_radius=18,
    )
    add_glow(
        canvas,
        lambda d: d.polygon([(352, 420), (490, 312), (642, 420), (622, 420), (490, 332), (370, 420)], fill=rgba("#8ED1FF", 36)),
        blur_radius=18,
    )

    canvas.save(OUTPUT_PATH)
    print(OUTPUT_PATH)


if __name__ == "__main__":
    main()
