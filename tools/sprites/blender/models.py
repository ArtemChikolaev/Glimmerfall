"""Процедурные модели для конвейера (G2.3): общий риг гуманоида, варианты — пропорциями и деталями.

Это модели-заглушки из примитивов, построенные кодом (ИИ-агентом) — не финальный арт (ADR 0003 п. 8): настоящие
модели придут из Blender с тем же ригом (ADR 0003 п. 4); клипы здесь — функции позы от нормированного времени t.
Тема — мрачное тёмное фэнтези (ADR 0003, уточнение 2026-10-26): нежить, демоны, мех, кость и тёмное железо.

Оси: Z вверх, персонаж смотрит в −Y (на камеру при направлении S), его левая рука — +X.
Повороты костей — градусы в локальных осях, X кости всегда совпадает с мировым +X:
  • кости вниз (руки, ноги): rx < 0 — мах вперёд, rz < 0 — левую конечность наружу (у правой знак обратный);
  • кости вверх (корпус, голова): rx > 0 — наклон вперёд, ry > 0 — поворот корпуса влево;
  • стопа: rx > 0 — носок вниз.
Сдвиг таза "loc" — в мировых осях (x, y, z) относительно покоя; корень "root" вращается у стоп (смерть).
"""

from __future__ import annotations

import functools
import math

UP = (0.0, -1.0, 0.0)       # подсказка оси Z для костей вверх (Z кости смотрит вперёд)
DOWN = (0.0, 1.0, 0.0)      # для костей вниз (Z кости смотрит назад)
FOOT = (0.0, 0.0, -1.0)

# кость: (родитель, голова, хвост, подсказка оси Z) — для роста 1,0 (масштабируется вариантом)
SKELETON = {
    "root": (None, (0, 0, 0), (0, 0, 0.12), UP),
    "pelvis": ("root", (0, 0, 0.92), (0, 0, 1.04), UP),
    "spine": ("pelvis", (0, 0, 1.04), (0, 0, 1.28), UP),
    "chest": ("spine", (0, 0, 1.28), (0, 0, 1.46), UP),
    "head": ("chest", (0, 0, 1.50), (0, 0, 1.72), UP),
    "upper_arm.L": ("chest", (0.19, 0, 1.42), (0.21, 0, 1.16), DOWN),
    "forearm.L": ("upper_arm.L", (0.21, 0, 1.16), (0.22, 0, 0.93), DOWN),
    "hand.L": ("forearm.L", (0.22, 0, 0.93), (0.22, 0, 0.85), DOWN),
    "upper_arm.R": ("chest", (-0.19, 0, 1.42), (-0.21, 0, 1.16), DOWN),
    "forearm.R": ("upper_arm.R", (-0.21, 0, 1.16), (-0.22, 0, 0.93), DOWN),
    "hand.R": ("forearm.R", (-0.22, 0, 0.93), (-0.22, 0, 0.85), DOWN),
    "thigh.L": ("pelvis", (0.10, 0, 0.94), (0.10, 0, 0.52), DOWN),
    "shin.L": ("thigh.L", (0.10, 0, 0.52), (0.10, 0, 0.09), DOWN),
    "foot.L": ("shin.L", (0.10, 0, 0.09), (0.10, -0.15, 0.03), FOOT),
    "thigh.R": ("pelvis", (-0.10, 0, 0.94), (-0.10, 0, 0.52), DOWN),
    "shin.R": ("thigh.R", (-0.10, 0, 0.52), (-0.10, 0, 0.09), DOWN),
    "foot.R": ("shin.R", (-0.10, 0, 0.09), (-0.10, -0.15, 0.03), FOOT),
}


# ---------- детали (меши на костях; жёсткая привязка к одной кости) ----------
# limb: усечённый конус от головы к хвосту кости; ball: шар (эллипсоид) в точке кости; box: брусок.
# Координаты — в мировых осях покоя для роста 1,0 и ширины 1,0, до масштаба варианта.

def limb(bone, r0, r1, ramp, glow=False, extend=(0.0, 0.0)):
    return {"shape": "limb", "bone": bone, "r": (r0, r1), "ramp": ramp, "glow": glow, "extend": extend}


def ball(bone, center, radius, ramp, scale=(1, 1, 1), glow=False, rot=(0, 0, 0)):
    return {"shape": "ball", "bone": bone, "center": center, "radius": radius, "scale": scale, "rot": rot,
            "ramp": ramp, "glow": glow}


def box(bone, center, size, ramp, rot=(0, 0, 0), glow=False):
    return {"shape": "box", "bone": bone, "center": center, "size": size, "rot": rot, "ramp": ramp, "glow": glow}


def cyl(bone, center, radius, depth, ramp, rot=(0, 0, 0), radius_top=None, glow=False):
    return {"shape": "cyl", "bone": bone, "center": center, "r": (radius, radius if radius_top is None else radius_top),
            "depth": depth, "rot": rot, "ramp": ramp, "glow": glow}


def cloak(top, bottom_z, r, ramp):
    """Плащ — половина конуса за спиной, от плеч вниз (на кости груди)."""
    return {"shape": "cloak", "bone": "chest", "top": top, "bottom_z": bottom_z, "r": r, "ramp": ramp, "glow": False}


def _rot_to(d):
    """Углы Эйлера (XYZ, градусы), которые поворачивают +Z в единичный вектор d (для цилиндров и рогов)."""
    return (math.degrees(-math.asin(max(-1.0, min(1.0, d[1])))), math.degrees(math.atan2(d[0], d[2])), 0.0)


def _unit(v):
    n = math.sqrt(sum(a * a for a in v)) or 1.0
    return tuple(a / n for a in v)


def horn(bone, base, direction, bend, length, radius, ramp, segments=3, glow=False, tip=0.2):
    """Рог, шип или хвост из сужающихся отрезков: направление плавно переходит от direction к bend (оси покоя).
    Острие не нулевое (tip — доля радиуса у конца) — иначе пропадает при даунскейле и рвёт силуэт."""
    parts = []
    point = base
    step = length / segments
    for k in range(segments):
        d = _unit(tuple(a * (1 - (k + 0.5) / segments) + b * (k + 0.5) / segments for a, b in zip(direction, bend)))
        end = tuple(p + q * step for p, q in zip(point, d))
        center = tuple((p + q) / 2 for p, q in zip(point, end))
        r0 = radius * (1 - (1 - tip) * k / segments)
        r1 = radius * (1 - (1 - tip) * (k + 1) / segments)
        parts.append(cyl(bone, center, r0, step * 1.1, ramp, rot=_rot_to(d), radius_top=r1, glow=glow))
        point = end
    return parts


def pair(make):
    """Деталь и её зеркало по X: make(sign) → список деталей, sign = +1 слева (+X), −1 справа."""
    return make(1) + make(-1)


def body(cloth, skin, legs, boots, arm_r=0.05, torso=(0.15, 0.19), head_r=0.105, arms=None):
    arms = arms or cloth
    parts = [
        cyl("pelvis", (0, 0, 0.98), 0.15, 0.16, cloth),
        cyl("spine", (0, 0, 1.17), torso[0], 0.26, cloth, radius_top=torso[1] * 0.95),
        cyl("chest", (0, 0, 1.37), torso[1] * 0.95, 0.20, cloth, radius_top=torso[1]),
        ball("head", (0, -0.005, 1.60), head_r, skin, scale=(0.95, 1.0, 1.12)),
    ]
    for side, x in (("L", 1), ("R", -1)):
        parts += [
            ball(f"upper_arm.{side}", (0.19 * x, 0, 1.42), arm_r * 1.25, arms),
            limb(f"upper_arm.{side}", arm_r, arm_r * 0.9, arms),
            limb(f"forearm.{side}", arm_r * 0.9, arm_r * 0.8, arms),
            ball(f"hand.{side}", (0.22 * x, 0, 0.88), arm_r * 0.95, skin),
            limb(f"thigh.{side}", 0.075, 0.06, legs),
            limb(f"shin.{side}", 0.06, 0.05, boots, extend=(0.0, 0.02)),
            box(f"foot.{side}", (0.10 * x, -0.055, 0.045), (0.09, 0.2, 0.08), boots),
        ]
    return parts


def eyes(bone, z, spread, forward, ramp, radius=0.02):
    """Светящиеся глаза — угли нежити и демонов: в тёмном мире лицо читается по ним."""
    return [ball(bone, (spread, forward, z), radius, ramp, glow=True), ball(bone, (-spread, forward, z), radius, ramp, glow=True)]


# Суммарный наклон правой кисти вперёд в стойке героя (плечо −8°, предплечье −30°, кисть −14°, см. HERO_READY):
# стойка поворачивает кисть на −52° вокруг X, поэтому оружие в покое смотрит вверх-вперёд на +52° — и в стойке
# оказывается вертикальным.
STANCE_TILT = 52.0


def _along(distance, hand=(-0.22, 0.0, 0.88), side=0.0):
    """Точка на оси оружия в правой кисти (в покое — вверх-вперёд на STANCE_TILT), distance — в долях роста;
    side — сдвиг поперёк оружия по X (лезвие секиры)."""
    tilt = math.radians(STANCE_TILT)
    return (hand[0] + side, hand[1] - distance * math.sin(tilt), hand[2] + distance * math.cos(tilt))


# ---------- герои ----------

# Облики (косметика) героя — рампы основных материалов; на сим и сид не влияют (CLAUDE.md), игра выбирает лист.
LAMPLIGHTER_SKINS = {
    # капюшон, одежда и плащ, ворот, маска
    "default": ("darkcloth", "darkred", "fur", "steel"),
    "ashen": ("oldbone", "blackiron", "darkcloth", "bone"),
}


def lamplighter_parts(skin="default"):
    # фонарщик-палач (огонь): тёмно-алый плащ с капюшоном, железная маска и меховой ворот — светлый ключ со всех
    # сторон (G2.6); фонарь с живым огнём и пылающий фальшион — ближний бой дугой (ADR 0003, уточнение 2026-10-26).
    # Облик «пепельный палач»: пепельный капюшон, доспех тёмного железа, костяная маска с рожками.
    hood, robe, collar, mask = LAMPLIGHTER_SKINS[skin]
    parts = body(cloth=robe, skin="skin", legs="leather", boots="blackiron", arm_r=0.05, torso=(0.15, 0.19))
    parts += [
        cyl("chest", (0, 0.0, 1.47), 0.215, 0.09, collar, radius_top=0.17),
        # светлый ключ со всех сторон: стальной наплечник справа и череп-наплечник слева, маска
        ball("upper_arm.R", (-0.21, 0, 1.47), 0.085, "steel", scale=(1.1, 1.1, 0.75)),
        ball("upper_arm.L", (0.21, 0.0, 1.5), 0.07, "bone", scale=(1.0, 1.0, 0.95)),
        *pair(lambda s: [ball("upper_arm.L", (0.21 + s * 0.022, -0.06, 1.505), 0.016, "gloom")]),
        # глубокий капюшон с козырьком вперёд, под ним маска с прорезью, где тлеет огонь
        ball("head", (0, 0.025, 1.625), 0.13, hood, scale=(1.0, 1.0, 1.06)),
        ball("head", (0, -0.045, 1.69), 0.08, hood, scale=(1.1, 0.8, 0.55)),
        ball("head", (0, -0.072, 1.6), 0.075, mask, scale=(0.95, 0.45, 1.05)),
        box("head", (0, -0.104, 1.615), (0.085, 0.012, 0.016), "fire", glow=True),
        cloak((0, 0.02, 1.47), 0.42, (0.21, 0.3), robe),
        cyl("pelvis", (0, 0, 0.94), 0.165, 0.07, "leather"),
        box("pelvis", (0.13, -0.11, 0.85), (0.028, 0.028, 0.16), "blackiron", rot=(0, 20, 0)),
        limb("forearm.L", 0.052, 0.046, "blackiron"),
        limb("forearm.R", 0.052, 0.046, "blackiron"),
        # фонарь с огнём в левой руке
        cyl("hand.L", (0.22, 0, 0.80), 0.008, 0.10, "blackiron"),
        cyl("hand.L", (0.22, 0, 0.745), 0.055, 0.025, "blackiron", radius_top=0.035),
        cyl("hand.L", (0.22, 0, 0.665), 0.045, 0.12, "fire", glow=True),
        cyl("hand.L", (0.22, 0, 0.592), 0.05, 0.03, "blackiron"),
        # фальшион: гарда, широкое тёмное лезвие и раскалённая кромка
        box("hand.R", (-0.22, 0, 0.85), (0.13, 0.035, 0.03), "blackiron"),
        box("hand.R", (-0.22, 0, 0.58), (0.06, 0.024, 0.5), "blackiron"),
        box("hand.R", (-0.22, -0.018, 0.57), (0.024, 0.014, 0.46), "fire", glow=True),
    ]
    if skin == "ashen":
        parts += pair(lambda s: horn("head", (s * 0.05, -0.075, 1.66), (s * 0.3, -0.2, 1), (s * 0.6, 0.2, 0.8), 0.1, 0.022,
                                     "oldbone", segments=2, tip=0.5))
    return parts


def warden_parts(skin="default"):
    # страж-громила (мороз): голые руки, меховая мантия и плащ — светлый ключ со всех сторон, рогатый шлем тёмного
    # железа, наплечник с шипами, огромная секира; фонарь отряда на поясе. Облик «кровавый медведь»: голова медведя
    # вместо шлема, бурая мантия и кровавый плащ, ожерелье из клыков — светлый ключ спереди.
    bear = skin == "bloodbear"
    parts = body(cloth="leather", skin="skin", legs="leather", boots="blackiron", arm_r=0.076, torso=(0.18, 0.26),
                 head_r=0.105, arms="skin")
    parts += [
        # голый торс клином: узкая талия, широкая грудь; перевязь через грудь
        cyl("spine", (0, 0, 1.17), 0.18, 0.26, "skin", radius_top=0.24),
        cyl("chest", (0, 0, 1.37), 0.245, 0.2, "skin", radius_top=0.27),
        box("chest", (0, -0.22, 1.3), (0.06, 0.04, 0.38), "leather", rot=(0, 38, 0)),
        # меховая мантия на плечах — ключ светлоты со всех сторон; короткий плащ
        ball("chest", (0, 0.04, 1.48), 0.26, "fur" if bear else "bone", scale=(1.2, 0.95, 0.38)),
        cloak((0, 0.05, 1.46), 0.9, (0.25, 0.3), "darkred" if bear else "fur"),
        ball("upper_arm.L", (0.28, 0, 1.47), 0.12, "blackiron", scale=(1.05, 1.1, 0.8)),
        *horn("upper_arm.L", (0.3, 0.0, 1.54), (0.4, 0, 1), (0.9, 0.2, 0.5), 0.17, 0.038, "oldbone", segments=2),
        *horn("upper_arm.L", (0.28, 0.07, 1.53), (0.3, 0.4, 1), (0.8, 0.6, 0.3), 0.13, 0.032, "oldbone", segments=2),
        limb("forearm.L", 0.08, 0.074, "blackiron"),
        limb("forearm.R", 0.08, 0.074, "blackiron"),
        box("head", (0, -0.094, 1.515), (0.13, 0.06, 0.1), "bone"),
        cyl("pelvis", (0, 0, 0.87), 0.18, 0.26, "leather", radius_top=0.17),
        cyl("pelvis", (0, 0, 0.99), 0.19, 0.06, "blackiron"),
        cyl("pelvis", (0.18, -0.1, 0.86), 0.035, 0.08, "radiance", glow=True),
        cyl("pelvis", (0.18, -0.1, 0.915), 0.04, 0.02, "blackiron"),
        ball("shin.L", (0.10, 0, 0.42), 0.074, "fur", scale=(1, 1, 0.6)),
        ball("shin.R", (-0.10, 0, 0.42), 0.074, "fur", scale=(1, 1, 0.6)),
        # секира: древко сквозь кисть, в стойке стоит вертикально; лезвие-полумесяц торчит наружу: тело лезвия,
        # высокая стальная кромка и два «рога» полумесяца к её концам
        box("hand.R", _along(0.24), (0.042, 0.042, 1.2), "leather", rot=(STANCE_TILT, 0, 0)),
        box("hand.R", _along(0.7, side=-0.1), (0.18, 0.03, 0.17), "blackiron", rot=(STANCE_TILT, 0, 0)),
        box("hand.R", _along(0.7, side=-0.215), (0.045, 0.032, 0.42), "steel", rot=(STANCE_TILT, 0, 0)),
        box("hand.R", _along(0.82, side=-0.155), (0.12, 0.03, 0.05), "blackiron", rot=(STANCE_TILT, -35, 0)),
        box("hand.R", _along(0.58, side=-0.155), (0.12, 0.03, 0.05), "blackiron", rot=(STANCE_TILT, 35, 0)),
        cyl("hand.R", _along(0.88), 0.03, 0.1, "oldbone", rot=(STANCE_TILT, 0, 0), radius_top=0.01),
    ]
    if bear:
        # голова медведя: морда вперёд, уши, клыки; ожерелье из клыков на груди
        parts += [
            ball("head", (0, 0.015, 1.65), 0.14, "fur", scale=(1.05, 1.15, 0.92)),
            *pair(lambda s: [ball("head", (s * 0.09, 0.03, 1.77), 0.038, "fur")]),
            box("head", (0, -0.13, 1.67), (0.1, 0.1, 0.075), "fur"),
            box("head", (0, -0.14, 1.625), (0.085, 0.03, 0.02), "bone"),
            *eyes("head", 1.69, 0.055, -0.115, "fire", radius=0.015),
            *[box("chest", (0.17 * math.sin(a), -0.2 * math.cos(a), 1.4 - 0.05 * math.cos(a)), (0.025, 0.02, 0.05), "bone")
              for a in (-1.0, -0.5, 0.0, 0.5, 1.0)],
        ]
    else:
        # рогатый шлем тёмного железа с наносником
        parts += [
            ball("head", (0, 0.0, 1.635), 0.122, "blackiron", scale=(1.0, 1.05, 1.0)),
            box("head", (0, -0.112, 1.6), (0.035, 0.02, 0.1), "blackiron"),
            *pair(lambda s: horn("head", (s * 0.1, 0.0, 1.69), (s, 0, 0.2), (s * 0.2, 0.1, 1), 0.38, 0.055, "oldbone")),
        ]
    return parts


def sparkwright_parts(skin="default"):
    # искровед-оккультист (молния): остроконечный капюшон, глаза-разряды во тьме под ним, мантия с костяной
    # оторочкой (светлый ключ), железная клетка с молнией за спиной, посох с черепом и шаром молнии.
    # Облик «костяной провидец»: бледная мантия с кровавой каймой, капюшон с ветвистыми рогами.
    seer = skin == "boneseer"
    robe = "oldbone" if seer else "darkcloth"
    trim = "darkred" if seer else "bone"
    parts = body(cloth=robe, skin="skin", legs="gloom", boots="leather", arm_r=0.046, torso=(0.14, 0.17), head_r=0.1)
    parts += [
        # глубокий капюшон с козырьком: под ним тьма и глаза-разряды
        ball("head", (0, 0.02, 1.63), 0.122, robe, scale=(1.0, 1.0, 1.08)),
        ball("head", (0, -0.05, 1.7), 0.08, robe, scale=(1.05, 0.85, 0.5)),
        ball("head", (0, -0.075, 1.6), 0.07, "gloom", scale=(0.9, 0.5, 1.0)),
        *eyes("head", 1.61, 0.03, -0.102, "lightning", radius=0.017),
        # костяное ожерелье-воротник, манжеты, кушак и кромка мантии — светлый ключ со всех сторон
        cyl("chest", (0, 0.0, 1.46), 0.215, 0.09, "bone", radius_top=0.165),
        limb("forearm.L", 0.05, 0.05, trim, extend=(-0.1, 0.0)),
        limb("forearm.R", 0.05, 0.05, trim, extend=(-0.1, 0.0)),
        cyl("pelvis", (0, 0, 0.83), 0.17, 0.34, robe, radius_top=0.155),
        cyl("pelvis", (0, 0, 0.97), 0.165, 0.06, trim),
        box("pelvis", (0.05, -0.16, 0.8), (0.06, 0.02, 0.26), trim),
        cyl("pelvis", (0, 0, 0.67), 0.178, 0.05, trim),
        # клетка с молнией за спиной: рама и прутья тёмного железа
        box("chest", (0, 0.17, 1.2), (0.22, 0.13, 0.03), "blackiron"),
        box("chest", (0, 0.17, 1.46), (0.22, 0.13, 0.03), "blackiron"),
        *[box("chest", (x, 0.235, 1.33), (0.022, 0.022, 0.27), "blackiron") for x in (-0.09, -0.03, 0.03, 0.09)],
        ball("chest", (0, 0.18, 1.33), 0.08, "lightning", glow=True),
        # посох с черепом и шаром молнии; в стойке вертикален, навершие — над капюшоном
        box("hand.R", _along(0.08), (0.032, 0.032, 1.42), "leather", rot=(STANCE_TILT, 0, 0)),
        ball("hand.R", _along(0.66), 0.058, "oldbone"),
        ball("hand.R", _along(0.79), 0.066, "lightning", glow=True),
    ]
    if seer:
        parts += [p for s in (1, -1) for p in
                  horn("head", (s * 0.06, 0.02, 1.73), (s * 0.4, 0.1, 1), (s * 0.9, 0.2, 0.6), 0.24, 0.032, "bone", tip=0.45)
                  + horn("head", (s * 0.12, 0.04, 1.84), (s * 0.1, 0.2, 1), (s * 0.3, 0.4, 0.9), 0.1, 0.025, "bone",
                         segments=2, tip=0.5)]
    return parts


# ---------- враги ----------

def skeleton_body(arm_r=0.038, leg_r=0.044):
    # старая кость темнее воска (рампа oldbone): светлое оставлено героям. Скелет узнаётся по трём признакам:
    # большой череп с тёмными глазницами (в них угли), просвет тонкого хребта между тазом и рёбрами, щели рёбер.
    # Суставы — шары, иначе тонкие кости рвут силуэт при даунскейле.
    parts = [
        box("pelvis", (0, 0, 0.965), (0.17, 0.09, 0.07), "oldbone"),
        *pair(lambda s: [ball("pelvis", (s * 0.075, 0, 0.99), 0.05, "oldbone", scale=(1, 0.7, 1))]),
        cyl("spine", (0, 0.02, 1.16), 0.03, 0.26, "oldbone"),
        cyl("chest", (0, 0.0, 1.37), 0.135, 0.2, "oldbone", radius_top=0.155),
        *[box("chest", (0, -0.13, z), (0.24, 0.04, 0.03), "gloom") for z in (1.31, 1.37, 1.43)],
        ball("head", (0, -0.005, 1.62), 0.108, "oldbone", scale=(1.0, 1.0, 0.95)),
        *pair(lambda s: [ball("head", (s * 0.04, -0.093, 1.625), 0.03, "gloom")]),
        *eyes("head", 1.625, 0.04, -0.108, "fire", radius=0.016),
        box("head", (0, -0.075, 1.528), (0.1, 0.07, 0.045), "oldbone"),
    ]
    for side, x in (("L", 1), ("R", -1)):
        parts += [
            ball(f"upper_arm.{side}", (0.19 * x, 0, 1.42), 0.045, "oldbone"),
            limb(f"upper_arm.{side}", arm_r, arm_r * 0.85, "oldbone"),
            ball(f"forearm.{side}", (0.21 * x, 0, 1.16), 0.036, "oldbone"),
            limb(f"forearm.{side}", arm_r * 0.85, arm_r * 0.75, "oldbone"),
            ball(f"hand.{side}", (0.22 * x, 0, 0.88), 0.04, "oldbone"),
            limb(f"thigh.{side}", leg_r, leg_r * 0.85, "oldbone"),
            ball(f"shin.{side}", (0.10 * x, 0, 0.52), 0.043, "oldbone"),
            limb(f"shin.{side}", leg_r * 0.85, leg_r * 0.7, "oldbone"),
            box(f"foot.{side}", (0.10 * x, -0.05, 0.035), (0.07, 0.17, 0.05), "oldbone"),
        ]
    return parts


def skeleton_parts():
    # скелет-воин: ржавый круглый щит и зазубренный меч
    return skeleton_body() + [
        cyl("forearm.L", (0.25, -0.07, 1.04), 0.13, 0.03, "leather", rot=(90, 0, 0)),
        ball("forearm.L", (0.25, -0.09, 1.04), 0.035, "blackiron"),
        box("hand.R", (-0.22, 0, 0.84), (0.1, 0.03, 0.025), "blackiron"),
        box("hand.R", (-0.22, -0.01, 0.62), (0.04, 0.02, 0.42), "blackiron"),
    ]


def skeleton_archer_parts():
    # костяной лучник: истлевший капюшон, колчан за спиной, лук поперёк руки — при прицеле встаёт вертикально
    return skeleton_body() + [
        ball("head", (0, 0.035, 1.64), 0.11, "darkcloth", scale=(1.0, 0.9, 1.08)),
        cloak((0, 0.02, 1.46), 1.05, (0.15, 0.19), "darkcloth"),
        cyl("chest", (0.07, 0.12, 1.33), 0.042, 0.32, "leather", rot=(-15, 0, 12)),
        box("hand.L", (0.22, 0.0, 0.86), (0.03, 0.24, 0.03), "leather"),
        box("hand.L", (0.22, -0.2, 0.83), (0.026, 0.18, 0.026), "leather", rot=(-22, 0, 0)),
        box("hand.L", (0.22, 0.2, 0.83), (0.026, 0.18, 0.026), "leather", rot=(22, 0, 0)),
    ]


def ghoul_parts():
    # гуль: сутулая гнилая плоть, длинные костяные когти, пасть вперёд, выпирающий хребет, гнилые глаза
    parts = body(cloth="darkrot", skin="darkrot", legs="darkrot", boots="darkrot", arm_r=0.056, torso=(0.12, 0.16), head_r=0.10)
    parts += [
        cyl("pelvis", (0, 0, 0.90), 0.16, 0.18, "leather", radius_top=0.15),
        box("hand.L", (0.22, -0.035, 0.77), (0.075, 0.06, 0.17), "oldbone"),
        box("hand.R", (-0.22, -0.035, 0.77), (0.075, 0.06, 0.17), "oldbone"),
        box("head", (0, -0.075, 1.55), (0.11, 0.08, 0.06), "oldbone"),
        *eyes("head", 1.62, 0.035, -0.087, "poison", radius=0.018),
        ball("spine", (0, 0.12, 1.18), 0.034, "oldbone"),
        ball("chest", (0, 0.14, 1.31), 0.036, "oldbone"),
        ball("chest", (0, 0.13, 1.43), 0.034, "oldbone"),
    ]
    return parts


def demon_horns(z=1.66, spread=0.07, length=0.28, radius=0.04, forward=0.0):
    """Рога демона: в стороны, затем вверх-назад; forward > 0 загибает их вперёд."""
    return pair(lambda s: horn("head", (s * spread, -0.01, z), (s * 0.75, -forward, 0.6), (s * 0.1, 0.6 - forward, 1.0),
                               length, radius, "oldbone"))


def demon_body(skin="darkred", arm_r=0.078, torso=(0.17, 0.3), head_r=0.09, chest="darkred"):
    """Демон: торс клином — грудь угловатым бруском (шар в пикселях превращает демона в колобок), узкая талия,
    обугленные ноги-копыта, длинные руки с костяными когтями."""
    parts = body(cloth=skin, skin=skin, legs="blackiron", boots="gloom", arm_r=arm_r, torso=(torso[0], torso[0] + 0.04),
                 head_r=head_r)
    parts += [
        box("chest", (0, 0.0, 1.39), (torso[1] * 2.0, 0.24, 0.22), chest),
        box("chest", (0, -0.115, 1.34), (torso[1] * 1.2, 0.06, 0.14), skin),
        *pair(lambda s: [ball(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.24, 0, 1.45), arm_r * 1.35, chest)]),
        *pair(lambda s: [box(f"hand.{'L' if s > 0 else 'R'}", (s * 0.22, -0.04, 0.78), (0.1, 0.07, 0.16), "oldbone")]),
    ]
    return parts


def back_spikes(count=3, ramp="oldbone"):
    return [p for i in range(count) for p in horn("chest" if i < 2 else "spine", (0, 0.16 - 0.02 * i, 1.44 - 0.12 * i),
                                                  (0, 0.6, 0.8), (0, 0.9, 0.3), 0.13 - 0.02 * i, 0.035, ramp, segments=2)]


def brute_parts():
    # демон-громила: торс клином, обугленные копыта, рога в стороны и вверх, угли глаз, шипы на плечах и хребте,
    # когти, хвост; грудь — со светлым бликом (рампа crimson), иначе громила тонет в тёмном мире
    parts = demon_body(chest="crimson")
    parts += [
        cyl("pelvis", (0, 0, 0.9), 0.19, 0.16, "leather", radius_top=0.18),
        *eyes("head", 1.6, 0.035, -0.092, "fire"),
        box("head", (0, -0.075, 1.52), (0.12, 0.08, 0.05), "bone"),
        *pair(lambda s: [box(f"hand.{'L' if s > 0 else 'R'}", (s * 0.22, -0.05, 0.76), (0.1, 0.06, 0.12), "bone")]),
        *demon_horns(length=0.32, radius=0.045),
        *pair(lambda s: horn(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.27, 0.0, 1.52), (s * 0.5, 0.1, 0.9),
                             (s * 0.7, 0.3, 0.6), 0.18, 0.04, "oldbone", segments=2)),
        *back_spikes(),
        *horn("pelvis", (0, 0.13, 0.95), (0, 0.8, -0.4), (0, 0.6, 0.7), 0.42, 0.04, "darkred"),
    ]
    return parts


def horned_parts():
    # рогач (таран): бычья морда вперёд, огромные рога вперёд, бурый загривок, обугленные копыта — бросается
    # по прямой
    parts = demon_body(arm_r=0.084, torso=(0.17, 0.28), head_r=0.1)
    parts += [
        ball("chest", (0, 0.09, 1.49), 0.2, "fur", scale=(1.15, 0.9, 0.55)),
        box("head", (0, -0.1, 1.56), (0.14, 0.14, 0.1), "darkred"),
        box("head", (0, -0.17, 1.55), (0.09, 0.03, 0.06), "oldbone"),
        *eyes("head", 1.63, 0.05, -0.105, "fire"),
        *pair(lambda s: horn("head", (s * 0.09, -0.02, 1.66), (s * 1.0, -0.1, 0.3), (s * 0.35, -1.0, 0.4), 0.4, 0.068,
                             "oldbone", segments=4)),
    ]
    return parts


def ravager_parts():
    # опустошитель (элита): демон в нагруднике тёмного железа с трещинами жара, рога, шипы, крылья-перепонки, тесак
    parts = demon_body(arm_r=0.085, torso=(0.19, 0.31), head_r=0.1, chest="blackiron")
    parts += [
        *pair(lambda s: [box("chest", (s * 0.05, -0.335, 1.37), (0.03, 0.02, 0.16), "fire", glow=True, rot=(0, s * 25, 0))]),
        *pair(lambda s: [ball(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.28, 0, 1.47), 0.13, "blackiron",
                              scale=(1.1, 1.1, 0.8))]),
        *pair(lambda s: horn(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.3, 0.0, 1.54), (s * 0.4, 0.1, 1),
                             (s * 0.8, 0.3, 0.5), 0.18, 0.042, "oldbone", segments=2)),
        *eyes("head", 1.6, 0.038, -0.097, "fire", radius=0.022),
        box("head", (0, -0.08, 1.52), (0.12, 0.08, 0.05), "oldbone"),
        *demon_horns(z=1.66, spread=0.075, length=0.4, radius=0.055),
        *back_spikes(),
        # рваный плащ и большие костяные гребни из лопаток вместо крыльев: тонкие лучи крыльев рвали силуэт
        # при даунскейле (QA «силуэт распадается»), гребни и плащ держат его цельным
        cloak((0, 0.06, 1.46), 0.62, (0.25, 0.32), "darkred"),
        *pair(lambda s: horn("chest", (s * 0.12, 0.09, 1.46), (s * 0.4, 0.6, 0.8), (s * 0.6, 0.6, 0.6), 0.3, 0.06, "oldbone",
                             tip=0.4)),
        cyl("pelvis", (0, 0, 0.9), 0.2, 0.16, "leather", radius_top=0.19),
        # тесак в правой руке
        box("hand.R", (-0.22, -0.04, 0.6), (0.03, 0.16, 0.46), "blackiron"),
        box("hand.R", (-0.22, -0.13, 0.58), (0.02, 0.03, 0.42), "steel"),
    ]
    return parts


def gloom_warden_parts():
    # Страж мрака (босс): латы тёмного железа, корона рогов, глаза-угли, пасть жара в нагруднике, рваный плащ,
    # огромный меч с раскалённой кромкой
    parts = body(cloth="blackiron", skin="darkred", legs="blackiron", boots="blackiron", arm_r=0.085, torso=(0.2, 0.3),
                 head_r=0.11)
    parts += [
        cloak((0, 0.04, 1.47), 0.15, (0.27, 0.42), "darkred"),
        cyl("chest", (0, 0, 1.37), 0.3, 0.22, "blackiron", radius_top=0.33),
        ball("chest", (0, -0.315, 1.36), 0.11, "fire", glow=True, scale=(1.0, 0.15, 0.4)),
        *[box("chest", (x, -0.335, 1.36), (0.018, 0.02, 0.07), "blackiron") for x in (-0.06, -0.02, 0.02, 0.06)],
        *pair(lambda s: [ball(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.29, 0, 1.47), 0.15, "blackiron",
                              scale=(1.1, 1.15, 0.85))]),
        *pair(lambda s: horn(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.31, 0.0, 1.56), (s * 0.3, 0.0, 1),
                             (s * 0.7, 0.2, 0.7), 0.2, 0.045, "oldbone", segments=2)),
        *pair(lambda s: horn(f"upper_arm.{'L' if s > 0 else 'R'}", (s * 0.33, 0.08, 1.52), (s * 0.5, 0.5, 0.8),
                             (s * 0.9, 0.5, 0.2), 0.16, 0.04, "oldbone", segments=2)),
        ball("head", (0, 0, 1.63), 0.125, "blackiron", scale=(1.0, 1.05, 1.08)),
        box("head", (0, -0.118, 1.62), (0.11, 0.015, 0.022), "fire", glow=True),
        *[part for a in (-50, -25, 0, 25, 50) for part in horn(
            "head", (0.11 * math.sin(math.radians(a)), 0.11 * math.cos(math.radians(a)) * 0.3 - 0.02, 1.74),
            (math.sin(math.radians(a)) * 0.3, 0.0, 1), (math.sin(math.radians(a)) * 0.5, 0.2, 0.9), 0.13, 0.03, "oldbone",
            segments=2)],
        *demon_horns(z=1.68, spread=0.1, length=0.46, radius=0.06),
        cyl("pelvis", (0, 0, 0.96), 0.22, 0.08, "blackiron"),
        *[ball("pelvis", (0.19 * math.sin(a), -0.17 * math.cos(a), 0.92), 0.045, "oldbone") for a in (-0.9, -0.3, 0.3, 0.9)],
        cyl("pelvis", (0, 0, 0.82), 0.23, 0.26, "darkred", radius_top=0.21),
        *pair(lambda s: [box("pelvis", (s * 0.15, -0.12, 0.8), (0.14, 0.04, 0.24), "blackiron", rot=(8, 0, s * 8))]),
        # меч одной рукой: так клип удара общий с рядовыми; широкое лезвие с раскалённой кромкой
        box("hand.R", (-0.22, 0, 0.86), (0.22, 0.05, 0.05), "blackiron"),
        box("hand.R", (-0.22, 0, 0.42), (0.13, 0.035, 0.86), "blackiron"),
        box("hand.R", (-0.22, -0.02, 0.42), (0.04, 0.02, 0.8), "fire", glow=True),
    ]
    return parts


# ---------- реквизит карты: статичные модели на корневой кости, мировые рампы (план светлоты: мир темнее фигур) ----------

def pillar_parts():
    return [
        box("root", (0, 0, 0.1), (0.72, 0.72, 0.2), "stone"),
        cyl("root", (0, 0, 0.88), 0.25, 1.36, "stone", radius_top=0.22),
        box("root", (0.03, 0.02, 1.62), (0.42, 0.36, 0.16), "stone", rot=(0, 9, 14)),
        box("root", (0.5, -0.22, 0.12), (0.34, 0.3, 0.24), "stone", rot=(0, 0, 25)),
    ]


def tomb_parts():
    return [
        box("root", (0, 0, 0.04), (0.6, 0.3, 0.08), "stone"),
        box("root", (0, 0, 0.36), (0.46, 0.13, 0.56), "stone"),
        ball("root", (0, 0, 0.64), 0.23, "stone", scale=(1.0, 0.28, 0.55)),
    ]


def cross_parts():
    return [
        ball("root", (0, 0, 0.0), 0.42, "earth", scale=(0.75, 1.0, 0.22)),
        box("root", (0, 0, 0.5), (0.1, 0.1, 0.92), "earth", rot=(0, 6, 0)),
        box("root", (0, 0, 0.72), (0.52, 0.1, 0.1), "earth", rot=(0, 6, 0)),
    ]


def brazier_parts():
    # тёмное железо в мировых тонах, светятся только угли — пламя и свет добавляет игра
    return [
        cyl("root", (0, 0, 0.03), 0.26, 0.06, "stone"),
        cyl("root", (0, 0, 0.36), 0.05, 0.62, "stone"),
        cyl("root", (0, 0, 0.72), 0.17, 0.2, "stone", radius_top=0.32),
        ball("root", (0, 0, 0.82), 0.27, "fire", scale=(1.0, 1.0, 0.3), glow=True),
    ]


def statue_parts():
    return [
        box("root", (0, 0, 0.25), (0.78, 0.78, 0.5), "stone"),
        box("root", (0, 0, 0.53), (0.86, 0.86, 0.06), "stone"),
        cyl("root", (0, 0, 1.1), 0.33, 1.1, "stone", radius_top=0.17),
        box("root", (0, -0.15, 1.36), (0.44, 0.14, 0.14), "stone"),
        ball("root", (0, -0.01, 1.74), 0.13, "stone", scale=(0.95, 1.0, 1.1)),
        ball("root", (0, 0.03, 1.77), 0.16, "stone", scale=(1.0, 1.0, 1.08)),
    ]


def sarcophagus_parts():
    return [
        box("root", (0, 0, 0.19), (1.05, 0.52, 0.38), "stone"),
        box("root", (0.04, 0.02, 0.42), (1.1, 0.58, 0.09), "stone", rot=(0, 0, 7)),
    ]


def shrine_parts():
    # святилище фонарщиков (место акта): постамент и железная клетка с вечным огнём — сияние видно издалека
    return [
        box("root", (0, 0, 0.08), (0.7, 0.7, 0.16), "stone"),
        cyl("root", (0, 0, 0.5), 0.17, 0.7, "stone", radius_top=0.14),
        box("root", (0, 0, 0.9), (0.42, 0.42, 0.08), "blackiron"),
        *[box("root", (0.17 * sx, 0.17 * sy, 1.12), (0.04, 0.04, 0.38), "blackiron") for sx in (-1, 1) for sy in (-1, 1)],
        box("root", (0, 0, 1.33), (0.42, 0.42, 0.06), "blackiron"),
        cyl("root", (0, 0, 1.43), 0.16, 0.14, "blackiron", radius_top=0.02),
        ball("root", (0, 0, 1.12), 0.15, "radiance", scale=(1.0, 1.0, 1.25), glow=True),
    ]


def altar_parts():
    # алтарь риска (место акта): кровавая плита на костях, рога по углам, черепа, чаша с огнём
    return [
        box("root", (0, 0, 0.12), (1.0, 0.7, 0.24), "stone"),
        box("root", (0, 0, 0.34), (0.86, 0.56, 0.2), "darkred"),
        *[p for sx in (-1, 1) for sy in (-1, 1) for p in horn(
            "root", (0.4 * sx, 0.26 * sy, 0.42), (0.4 * sx, 0.2 * sy, 1), (-0.3 * sx, 0.1 * sy, 1), 0.5, 0.06, "oldbone",
            tip=0.35)],
        *[ball("root", (x, -0.3, 0.3), 0.08, "oldbone") for x in (-0.25, 0.0, 0.25)],
        cyl("root", (0, 0, 0.5), 0.17, 0.1, "blackiron", radius_top=0.22),
        ball("root", (0, 0, 0.56), 0.16, "fire", scale=(1.0, 1.0, 0.45), glow=True),
    ]


def bones_parts():
    # груда костей: кости вразброс и череп сверху
    return [
        *[box("root", (x, y, 0.04), (0.34, 0.05, 0.05), "oldbone", rot=(0, 0, a))
          for x, y, a in ((0, 0, 20), (0.1, 0.08, -35), (-0.12, 0.05, 70), (0.05, -0.1, 110), (-0.05, -0.04, -10))],
        ball("root", (0.02, 0.0, 0.12), 0.1, "oldbone", scale=(1.0, 1.1, 0.9)),
        *[ball("root", (0.02 + s * 0.035, -0.09, 0.13), 0.022, "gloom") for s in (-1, 1)],
    ]


def stake_parts():
    # кол с насаженным скелетом: древко из земли, рёбра и череп на острие
    return [
        ball("root", (0, 0, 0.0), 0.3, "earth", scale=(1.0, 1.0, 0.25)),
        cyl("root", (0, 0, 0.75), 0.05, 1.5, "leather", radius_top=0.02),
        cyl("root", (0, 0, 1.05), 0.13, 0.3, "oldbone", radius_top=0.15),
        *[box("root", (0, -0.13, z), (0.24, 0.04, 0.03), "gloom") for z in (0.98, 1.06, 1.14)],
        ball("root", (0, 0, 1.32), 0.11, "oldbone"),
        *[ball("root", (s * 0.04, -0.095, 1.33), 0.026, "gloom") for s in (-1, 1)],
        *pair(lambda s: [cyl("root", (s * 0.2, 0, 1.0), 0.03, 0.4, "oldbone", rot=(0, s * 60, 0))]),
    ]


def deadtree_parts():
    # мёртвое дерево: кривой ствол и голые ветви
    return [
        cyl("root", (0, 0, 0.6), 0.14, 1.2, "leather", radius_top=0.08),
        *horn("root", (0, 0, 1.15), (0.3, 0, 1), (0.9, 0.2, 0.6), 0.7, 0.07, "leather", tip=0.3),
        *horn("root", (0, 0, 1.0), (-0.5, 0.2, 1), (-1.0, -0.2, 0.4), 0.6, 0.06, "leather", tip=0.3),
        *horn("root", (0, 0, 1.25), (-0.1, -0.3, 1), (0.2, -0.8, 0.6), 0.5, 0.05, "leather", tip=0.3),
        *horn("root", (0, 0, 0.1), (0.8, 0.3, -0.1), (1, 0.2, -0.3), 0.35, 0.06, "leather", segments=2, tip=0.4),
        *horn("root", (0, 0, 0.1), (-0.6, -0.5, -0.1), (-0.8, -0.6, -0.3), 0.3, 0.05, "leather", segments=2, tip=0.4),
    ]


def _static(t):
    return {}


def prop(note, parts, target_px, canvas, anchor_y=0.78):
    return {
        "note": note, "class": "prop", "height": 1.0, "width": 1.0, "target_px": target_px, "canvas": canvas,
        "anchor_y": anchor_y, "parts": parts, "carrier": "root",
        # статичный реквизит: «направления» — варианты поворота одной модели, без зеркала
        "directions": ["S", "SE", "E", "NE"], "mirror": {},
        "clips": {"idle": (_static, 1, 1, True, None, None)},
    }


# ---------- клипы: t ∈ [0, 1) для циклов, [0, 1] для разовых ----------

def _wave(t, phase=0.0):
    return math.sin(2 * math.pi * t + phase)


def _blend(keys, frame):
    """Поза между ключами {кадр: поза} со сглаживанием (smoothstep) — позы к позам."""
    frames = sorted(keys)
    if frame <= frames[0]:
        return keys[frames[0]]
    if frame >= frames[-1]:
        return keys[frames[-1]]
    for a, b in zip(frames, frames[1:]):
        if a <= frame <= b:
            u = (frame - a) / (b - a)
            u = u * u * (3 - 2 * u)
            pa, pb = keys[a], keys[b]
            out = {}
            for bone in set(pa) | set(pb):
                va = pa.get(bone, (0, 0, 0))
                vb = pb.get(bone, (0, 0, 0))
                out[bone] = tuple(x + (y - x) * u for x, y in zip(va, vb))
            return out
    raise AssertionError("unreachable")


def _merge(*poses):
    out = {}
    for pose in poses:
        for bone, value in pose.items():
            base = out.get(bone, (0, 0, 0))
            out[bone] = tuple(a + b for a, b in zip(base, value))
    return out


HERO_READY = {
    "spine": (4, 0, 0),
    "head": (-3, 0, 0),
    "upper_arm.L": (-24, 0, -6),
    "forearm.L": (-52, 0, 0),
    "hand.L": (76, 0, 0),
    "upper_arm.R": (-8, 0, 10),
    "forearm.R": (-30, 0, 0),
    "hand.R": (-14, 0, 0),
    "thigh.L": (-5, 0, -2),
    "thigh.R": (4, 0, 2),
    "shin.L": (8, 0, 0),
    "shin.R": (6, 0, 0),
    "foot.L": (-3, 0, 0),
    "foot.R": (-8, 0, 0),
}


def hero_idle(t):
    b = _wave(t)
    return _merge(HERO_READY, {
        "loc": (0, 0, -0.010 + 0.010 * b),
        "spine": (1.5 * b, 0, 0),
        "chest": (1.0 * b, 0, 0),
        "head": (-1.0 * b, 0, 0),
        "upper_arm.L": (2.5 * _wave(t, 0.6), 0, 0),
        "hand.L": (-2.5 * _wave(t, 0.6), 0, 0),
        "upper_arm.R": (-1.5 * b, 0, 0),
        "shin.L": (2 * max(0.0, -b), 0, 0),
        "shin.R": (2 * max(0.0, -b), 0, 0),
    })


def _legs_cycle(t, stride, knee, lift):
    pose = {}
    for side, phase in (("L", 0.0), ("R", math.pi)):
        s = math.sin(2 * math.pi * t + phase)
        c = math.cos(2 * math.pi * t + phase)
        pose[f"thigh.{side}"] = (-stride * s, 0, 0)
        # колено сгибается при махе назад и в проносе, почти прямое в момент постановки
        pose[f"shin.{side}"] = (knee * (0.55 - 0.45 * s) * (0.6 + 0.4 * max(0.0, c)) + lift * max(0.0, -s), 0, 0)
        pose[f"foot.{side}"] = (-10 * s, 0, 0)
    return pose


def hero_run(t):
    s = _wave(t)
    return _merge(_legs_cycle(t, stride=34, knee=62, lift=18), {
        "loc": (0, -0.02, -0.035 + 0.03 * abs(math.cos(2 * math.pi * t))),
        "spine": (12, 0, 0),
        "chest": (2, 7 * s, 0),
        "head": (-10, -5 * s, 0),
        "upper_arm.L": (-30 + 14 * s, 0, -8),
        "forearm.L": (-48, 0, 0),
        "hand.L": (80 - 14 * s, 0, 0),
        "upper_arm.R": (-6 - 30 * s, 0, 10),
        "forearm.R": (-55, 0, 0),
        "hand.R": (-30, 0, 0),
    })


def hero_attack(t, frames=9):
    # удар дугой: замах над головой с разворотом корпуса → рубящий удар вниз-поперёк с шагом → возврат
    ready = HERO_READY
    keys = {
        0: ready,
        2: _merge(ready, {"spine": (-6, -24, 0), "chest": (0, -10, 0), "head": (0, 14, 0),
                          "upper_arm.R": (-150, 0, 22), "forearm.R": (-45, 0, 0), "hand.R": (-10, 0, 0),
                          "upper_arm.L": (10, 0, -10), "loc": (0, 0.03, -0.01)}),
        3: _merge(ready, {"spine": (6, -6, 0), "upper_arm.R": (-118, 0, 6), "forearm.R": (-12, 0, 0),
                          "hand.R": (-6, 0, 0), "loc": (0, -0.03, -0.02)}),
        4: _merge(ready, {"spine": (18, 26, 0), "chest": (4, 12, 0), "head": (-4, -14, 0),
                          "upper_arm.R": (-34, 0, -36), "forearm.R": (-8, 0, 0), "hand.R": (-12, 0, 0),
                          "thigh.R": (14, 0, 0), "thigh.L": (-26, 0, 0), "shin.L": (16, 0, 0),
                          "loc": (0, -0.09, -0.05)}),
        6: _merge(ready, {"spine": (12, 16, 0), "upper_arm.R": (-30, 0, -20), "forearm.R": (-14, 0, 0),
                          "thigh.L": (-16, 0, 0), "loc": (0, -0.06, -0.035)}),
        8: ready,
    }
    return _blend(keys, t * (frames - 1))


def hero_hit(t, frames=4):
    ready = HERO_READY
    keys = {
        0: ready,
        1: _merge(ready, {"spine": (-16, 6, 0), "head": (-14, 0, 0), "upper_arm.L": (12, 0, -18),
                          "upper_arm.R": (14, 0, 22), "loc": (0, 0.05, -0.02)}),
        3: ready,
    }
    return _blend(keys, t * (frames - 1))


def death_keys(ready):
    return {
        0: ready,
        2: _merge(ready, {"spine": (-20, 0, 0), "head": (-22, 0, 0), "upper_arm.L": (14, 0, -30),
                          "upper_arm.R": (14, 0, 30), "loc": (0, 0.04, -0.01)}),
        5: _merge(ready, {"spine": (14, 0, 0), "head": (16, 0, 0), "thigh.L": (-50, 0, -6), "thigh.R": (-38, 0, 6),
                          "shin.L": (95, 0, 0), "shin.R": (85, 0, 0), "foot.L": (-20, 0, 0), "foot.R": (-20, 0, 0),
                          "upper_arm.L": (-30, 0, -12), "upper_arm.R": (-36, 0, 12), "loc": (0, 0.02, -0.34)}),
        8: _merge(ready, {"root": (-48, 0, 0), "spine": (-8, 0, 0), "thigh.L": (-60, 0, -10), "thigh.R": (-50, 0, 10),
                          "shin.L": (80, 0, 0), "shin.R": (70, 0, 0), "upper_arm.L": (40, 0, -36),
                          "upper_arm.R": (36, 0, 34), "forearm.L": (-30, 0, 0), "forearm.R": (-36, 0, 0),
                          "loc": (0, 0.05, -0.38)}),
        11: _merge(ready, {"root": (-84, 0, 0), "spine": (-6, 0, 0), "head": (-10, 0, 0), "thigh.L": (-34, 0, -14),
                           "thigh.R": (-24, 0, 12), "shin.L": (50, 0, 0), "shin.R": (30, 0, 0),
                           "upper_arm.L": (40, 0, -34), "upper_arm.R": (30, 0, 30), "forearm.L": (-50, 0, 0),
                           "forearm.R": (-60, 0, 0), "loc": (0, 0.05, -0.42)}),
    }


def hero_death(t, frames=12):
    return _blend(death_keys(HERO_READY), t * (frames - 1))


MONSTER_READY = {
    "spine": (30, 0, 0),
    "chest": (8, 0, 0),
    "head": (-32, 0, 0),
    "upper_arm.L": (-26, 0, -8),
    "upper_arm.R": (-26, 0, 8),
    "forearm.L": (-20, 0, 0),
    "forearm.R": (-20, 0, 0),
    "thigh.L": (-14, 0, -4),
    "thigh.R": (-6, 0, 4),
    "shin.L": (24, 0, 0),
    "shin.R": (20, 0, 0),
    "foot.L": (-8, 0, 0),
    "foot.R": (-12, 0, 0),
    "loc": (0, 0.02, -0.05),
}

# скелет держится прямее нежити из плоти: голова над тазом, руки с оружием вперёд
SKELETON_READY = {
    "spine": (10, 0, 0),
    "chest": (4, 0, 0),
    "head": (-12, 0, 0),
    "upper_arm.L": (-16, 0, -6),
    "upper_arm.R": (-20, 0, 6),
    "forearm.L": (-26, 0, 0),
    "forearm.R": (-30, 0, 0),
    "thigh.L": (-8, 0, -3),
    "thigh.R": (-2, 0, 3),
    "shin.L": (14, 0, 0),
    "shin.R": (10, 0, 0),
    "foot.L": (-6, 0, 0),
    "foot.R": (-8, 0, 0),
    "loc": (0, 0.01, -0.025),
}

# рогач опускает голову рогами вперёд
HORNED_READY = _merge(MONSTER_READY, {"spine": (10, 0, 0), "head": (-2, 0, 0)})

# демон стоит прямее нежити: голова над плечами, рога видны над силуэтом
DEMON_READY = _merge(MONSTER_READY, {"spine": (-16, 0, 0), "chest": (-2, 0, 0), "head": (18, 0, 0),
                                      "upper_arm.L": (6, 0, -4), "upper_arm.R": (6, 0, 4)})


def monster_run(t, ready=MONSTER_READY, stride=30, knee=58, lift=14, arms=22):
    s = _wave(t)
    return _merge(ready, _legs_cycle(t, stride=stride, knee=knee, lift=lift), {
        "loc": (0, -0.02, -0.03 + 0.035 * abs(math.cos(2 * math.pi * t))),
        "spine": (6, 0, 0),
        "chest": (0, 9 * s, 3 * s),
        "upper_arm.L": (arms * s, 0, 0),
        "upper_arm.R": (-arms * s, 0, 0),
        "forearm.L": (-12 * max(0.0, s), 0, 0),
        "forearm.R": (-12 * max(0.0, -s), 0, 0),
        "head": (0, -6 * s, 0),
    })


def monster_attack(t, frames=7, windup_end=2, strike_end=3, ready=MONSTER_READY, raise_arms=-130):
    """Замах обеими руками над головой и удар вниз. Тяжёлым (широкие плечи) — замах ниже: со спины кисти над
    наплечниками отрывались от силуэта крошкой."""
    keys = {
        0: ready,
        windup_end: _merge(ready, {"spine": (-24, 0, 0), "head": (16, 0, 0), "upper_arm.L": (raise_arms, 0, -30),
                                   "upper_arm.R": (raise_arms, 0, 30), "forearm.L": (-40, 0, 0), "forearm.R": (-40, 0, 0),
                                   "loc": (0, 0.05, 0.03)}),
        strike_end: _merge(ready, {"spine": (24, 0, 0), "head": (-4, 0, 0), "upper_arm.L": (-40, 0, 8),
                                   "upper_arm.R": (-40, 0, -8), "forearm.L": (-6, 0, 0), "forearm.R": (-6, 0, 0),
                                   "thigh.L": (-30, 0, 0), "loc": (0, -0.12, -0.06)}),
        frames - 1: ready,
    }
    return _blend(keys, t * (frames - 1))


def archer_attack(t, frames=8):
    # натяжение: левая рука с луком вперёд, правая тянет тетиву к лицу; выстрел — правая рука отлетает назад
    ready = SKELETON_READY
    draw = _merge(ready, {"spine": (-10, 24, 0), "chest": (0, 10, 0), "head": (10, -26, 0),
                          "upper_arm.L": (-74, 0, 6), "forearm.L": (26, 0, 0),
                          "upper_arm.R": (-70, 0, 18), "forearm.R": (-95, 0, 0)})
    loose = _merge(draw, {"upper_arm.R": (16, 0, 24), "forearm.R": (64, 0, 0), "loc": (0, 0.03, 0)})
    keys = {0: ready, 3: draw, 5: draw, 6: loose, frames - 1: ready}
    return _blend(keys, t * (frames - 1))


def monster_hit(t, frames=3, ready=MONSTER_READY):
    keys = {0: ready,
            1: _merge(ready, {"spine": (-22, 0, 0), "head": (-10, 0, 0), "upper_arm.L": (10, 0, -16),
                              "upper_arm.R": (10, 0, 16), "loc": (0, 0.05, 0.0)}),
            2: ready}
    return _blend(keys, t * (frames - 1))


def monster_death(t, frames=10, ready=MONSTER_READY):
    keys = death_keys(ready)
    # те же позы, ужатые в 10 кадров
    scaled = {round(k * (frames - 1) / 11): v for k, v in keys.items()}
    return _blend(scaled, t * (frames - 1))


# ---------- варианты ----------
# height — рост в метрах (скелет рассчитан на 1,0 ≈ 1,78 м в капюшоне), width — множитель ширины,
# target_px — высота силуэта стойки (S, кадр 0) на сетке 640×360 (ADR 0003 п. 3), canvas — холст рендера, px.
# clips: имя → (функция, кадров, кадров/с, цикл, направления, фазы удара).

HERO_DIRS = ["S", "SE", "E", "NE", "N"]
HERO_MIRROR = {"SW": "SE", "W": "E", "NW": "NE"}


def hero(note, parts, height, width, target_px, canvas=120):
    """Герой отряда: общий риг и общие клипы — различаются пропорции и детали (ADR 0003 п. 4)."""
    return {
        "note": note,
        "height": height, "width": width, "target_px": target_px, "canvas": canvas, "anchor_y": 0.68,
        "parts": parts,
        "carrier": "spine",
        "directions": HERO_DIRS, "mirror": HERO_MIRROR,
        "clips": {
            "idle": (hero_idle, 8, 8, True, None, None),
            "run": (hero_run, 8, 12, True, None, None),
            "attack": (hero_attack, 9, 15, False, None, {"windup": [0, 3], "active": [3, 5], "recovery": [5, 9]}),
            "hit": (hero_hit, 4, 12, False, None, None),
            "death": (hero_death, 12, 12, False, ["S"], None),
        },
    }


MONSTER_DIRS = ["S", "E", "N"]
MONSTER_MIRROR = {"W": "E"}


def monster(note, parts, height, width, target_px, canvas, ready=MONSTER_READY, fps=12, attack=None, heavy=False,
            run=None):
    """Враг: общий риг, свои пропорции, детали и стойка. heavy — медленный удар в 9 кадров (элиты, громилы, босс)."""
    if attack is None:
        if heavy:
            attack = (lambda t: monster_attack(t, frames=9, windup_end=3, strike_end=5, ready=ready, raise_arms=-105), 9, fps,
                      False, None,
                      {"windup": [0, 4], "active": [4, 6], "recovery": [6, 9]})
        else:
            attack = (lambda t: monster_attack(t, ready=ready), 7, fps, False, None,
                      {"windup": [0, 3], "active": [3, 4], "recovery": [4, 7]})
    return {
        "note": note,
        "height": height, "width": width, "target_px": target_px, "canvas": canvas, "anchor_y": 0.72,
        "parts": parts,
        "carrier": "spine",
        "directions": MONSTER_DIRS, "mirror": MONSTER_MIRROR,
        "clips": {
            "run": (run or (lambda t: monster_run(t, ready=ready)), 8, fps, True, None, None),
            "attack": attack,
            "hit": (lambda t: monster_hit(t, ready=ready), 3, fps, False, None, None),
            "death": (lambda t: monster_death(t, ready=ready), 10, fps, False, ["S"], None),
        },
    }


def skeleton_run(t):
    return monster_run(t, ready=SKELETON_READY, stride=26, knee=50, lift=10, arms=18)


MODELS = {
    "lamplighter": hero("герой-фонарщик (огонь): палач в капюшоне, железная маска, фонарь с огнём, пылающий фальшион",
                        lamplighter_parts, 1.0, 1.0, 48),
    "warden": hero("герой-страж (мороз): громила с голыми руками, мех, рогатый шлем, огромная секира",
                   warden_parts, 1.2, 1.18, 64, canvas=150),
    "sparkwright": hero("герой-искровед (молния): оккультист в остроконечном капюшоне, клетка с молнией, посох с черепом",
                        sparkwright_parts, 1.0, 0.94, 50),
    # облики (косметика): тот же риг и клипы, другие материалы и детали — лист <герой>_<облик>
    "lamplighter_ashen": hero("облик фонарщика «пепельный палач»: пепельный капюшон, доспех тёмного железа, костяная маска",
                              functools.partial(lamplighter_parts, "ashen"), 1.0, 1.0, 48),
    "warden_bloodbear": hero("облик стража «кровавый медведь»: голова медведя, кровавый плащ, ожерелье из клыков",
                             functools.partial(warden_parts, "bloodbear"), 1.2, 1.18, 64, canvas=150),
    "sparkwright_boneseer": hero("облик искроведа «костяной провидец»: бледная мантия, ветвистые рога",
                                 functools.partial(sparkwright_parts, "boneseer"), 1.0, 0.94, 50),
    "skeleton": monster("скелет-воин: ржавый щит и меч, угли в глазницах", skeleton_parts, 0.95, 0.9, 32, 80,
                        ready=SKELETON_READY, run=skeleton_run),
    "skeleton_archer": monster("костяной лучник: истлевший капюшон, колчан, лук", skeleton_archer_parts, 0.95, 0.9, 32, 72,
                               ready=SKELETON_READY, run=skeleton_run,
                               attack=(archer_attack, 8, 12, False, None,
                                       {"windup": [0, 5], "active": [5, 6], "recovery": [6, 8]})),
    "ghoul": monster("гуль: сутулая гнилая плоть, длинные когти, гнилые глаза", ghoul_parts, 0.92, 0.95, 30, 64),
    "brute": monster("демон-громила: красная кожа, рога, когти, хвост", brute_parts, 1.05, 1.25, 44, 96, heavy=True, fps=10,
                     ready=DEMON_READY),
    "horned": monster("рогач: бычья морда, рога вперёд, загривок", horned_parts, 1.0, 1.3, 38, 96, ready=HORNED_READY),
    "ravager": monster("опустошитель (элита): демон в железе, костяные крылья, тесак", ravager_parts, 1.15, 1.4, 64, 150,
                       heavy=True, fps=10, ready=DEMON_READY),
    "gloom_warden": monster("Страж мрака (босс): латы, корона рогов, меч с раскалённой кромкой", gloom_warden_parts,
                            1.3, 1.6, 120, 260, heavy=True, fps=10),
    "pillar": prop("обломок колонны с упавшим куском капители", pillar_parts, 70, 128),
    "tomb": prop("надгробие", tomb_parts, 22, 48),
    "cross": prop("могильный крест на холмике", cross_parts, 26, 48),
    "brazier": prop("жаровня на треноге с углями (пламя — эффект в игре)", brazier_parts, 26, 48),
    "statue": prop("статуя фонарщика в капюшоне на постаменте", statue_parts, 72, 128),
    "sarcophagus": prop("саркофаг со сдвинутой крышкой", sarcophagus_parts, 20, 64, anchor_y=0.7),
    "shrine": prop("святилище фонарщиков: постамент и клетка с вечным огнём (место акта)", shrine_parts, 44, 96),
    "altar": prop("алтарь риска: кровавая плита, рога, черепа, чаша с огнём (место акта)", altar_parts, 34, 96),
    "bones": prop("груда костей с черепом", bones_parts, 12, 48, anchor_y=0.7),
    "stake": prop("кол с насаженным скелетом", stake_parts, 44, 96),
    "deadtree": prop("мёртвое дерево с голыми ветвями", deadtree_parts, 72, 160),
}
