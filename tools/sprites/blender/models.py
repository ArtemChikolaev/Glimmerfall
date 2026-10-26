"""Процедурные манекены для проверки конвейера (G2.3): общий риг гуманоида, варианты — пропорциями и деталями.

Это тестовые модели из примитивов, построенные кодом, — не арт и не ИИ. Настоящие модели придут из Blender
с тем же ригом (ADR 0003 п. 4); клипы здесь — функции позы от нормированного времени t.

Оси: Z вверх, персонаж смотрит в −Y (на камеру при направлении S), его левая рука — +X.
Повороты костей — градусы в локальных осях, X кости всегда совпадает с мировым +X:
  • кости вниз (руки, ноги): rx < 0 — мах вперёд, rz < 0 — левую конечность наружу (у правой знак обратный);
  • кости вверх (корпус, голова): rx > 0 — наклон вперёд, ry > 0 — поворот корпуса влево;
  • стопа: rx > 0 — носок вниз.
Сдвиг таза "loc" — в мировых осях (x, y, z) относительно покоя; корень "root" вращается у стоп (смерть).
"""

from __future__ import annotations

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


def ball(bone, center, radius, ramp, scale=(1, 1, 1), glow=False):
    return {"shape": "ball", "bone": bone, "center": center, "radius": radius, "scale": scale, "ramp": ramp, "glow": glow}


def box(bone, center, size, ramp, rot=(0, 0, 0), glow=False):
    return {"shape": "box", "bone": bone, "center": center, "size": size, "rot": rot, "ramp": ramp, "glow": glow}


def cyl(bone, center, radius, depth, ramp, rot=(0, 0, 0), radius_top=None, glow=False):
    return {"shape": "cyl", "bone": bone, "center": center, "r": (radius, radius if radius_top is None else radius_top),
            "depth": depth, "rot": rot, "ramp": ramp, "glow": glow}


def body(cloth, skin, legs, boots, arm_r=0.05, torso=(0.15, 0.19), head_r=0.105):
    parts = [
        cyl("pelvis", (0, 0, 0.98), 0.15, 0.16, cloth),
        cyl("spine", (0, 0, 1.17), torso[0], 0.26, cloth, radius_top=torso[1] * 0.95),
        cyl("chest", (0, 0, 1.37), torso[1] * 0.95, 0.20, cloth, radius_top=torso[1]),
        ball("head", (0, -0.005, 1.60), head_r, skin, scale=(0.95, 1.0, 1.12)),
    ]
    for side, x in (("L", 1), ("R", -1)):
        parts += [
            ball(f"upper_arm.{side}", (0.19 * x, 0, 1.42), arm_r * 1.25, cloth),
            limb(f"upper_arm.{side}", arm_r, arm_r * 0.9, cloth),
            limb(f"forearm.{side}", arm_r * 0.9, arm_r * 0.8, cloth),
            ball(f"hand.{side}", (0.22 * x, 0, 0.88), arm_r * 0.95, skin),
            limb(f"thigh.{side}", 0.075, 0.06, legs),
            limb(f"shin.{side}", 0.06, 0.05, boots, extend=(0.0, 0.02)),
            box(f"foot.{side}", (0.10 * x, -0.055, 0.045), (0.09, 0.2, 0.08), boots),
        ]
    return parts


def lamplighter_parts():
    # светлый ключ героя: льняная туника (рампа bone) — в сером кадре герой светлее орды (проверка G2.6);
    # с туникой indigo он сливался с гулями по светлоте (Δ 0,03)
    parts = body(cloth="bone", skin="skin", legs="indigo", boots="earth")
    parts += [
        # капюшон чуть больше головы и сдвинут назад: лицо выглядывает спереди
        ball("head", (0, 0.03, 1.625), 0.13, "crimson", scale=(1.0, 1.0, 1.08)),
        # плащ — половина конуса за спиной, от плеч до колен
        {"shape": "cloak", "bone": "chest", "top": (0, 0.02, 1.47), "bottom_z": 0.50, "r": (0.21, 0.31), "ramp": "crimson", "glow": False},
        # фонарь в левой руке: ручка, латунные крышки и светящееся стекло
        cyl("hand.L", (0.22, 0, 0.80), 0.008, 0.10, "brass"),
        cyl("hand.L", (0.22, 0, 0.745), 0.055, 0.025, "brass", radius_top=0.035),
        cyl("hand.L", (0.22, 0, 0.665), 0.045, 0.12, "radiance", glow=True),
        cyl("hand.L", (0.22, 0, 0.592), 0.05, 0.03, "brass"),
        # клинок в правой руке: гарда и лезвие вниз-вперёд
        box("hand.R", (-0.22, 0, 0.85), (0.12, 0.03, 0.025), "brass"),
        box("hand.R", (-0.22, 0, 0.58), (0.035, 0.022, 0.52), "steel"),
    ]
    return parts


def ghoul_parts():
    # плоть светлее мира (план светлоты), тёмная только тряпка; руки толще — тонкие пропадают при даунскейле
    parts = body(cloth="rot", skin="rot", legs="rot", boots="rot", arm_r=0.056, torso=(0.12, 0.16), head_r=0.10)
    parts += [
        # набедренная тряпка и когти
        cyl("pelvis", (0, 0, 0.90), 0.16, 0.18, "earth", radius_top=0.15),
        box("hand.L", (0.22, -0.03, 0.80), (0.07, 0.06, 0.12), "bone"),
        box("hand.R", (-0.22, -0.03, 0.80), (0.07, 0.06, 0.12), "bone"),
        # челюсть вперёд
        box("head", (0, -0.07, 1.55), (0.10, 0.08, 0.06), "bone"),
    ]
    return parts


def brute_parts():
    parts = body(cloth="rot", skin="rot", legs="rot", boots="gloom", arm_r=0.075, torso=(0.2, 0.27), head_r=0.095)
    parts += [
        # костяные наплечники и нагрудник
        ball("upper_arm.L", (0.24, 0, 1.45), 0.11, "bone", scale=(1.0, 1.1, 0.75)),
        ball("upper_arm.R", (-0.24, 0, 1.45), 0.11, "bone", scale=(1.0, 1.1, 0.75)),
        box("chest", (0, -0.17, 1.33), (0.30, 0.06, 0.22), "bone"),
        # тяжёлые кулаки
        ball("hand.L", (0.22, 0, 0.86), 0.085, "bone"),
        ball("hand.R", (-0.22, 0, 0.86), 0.085, "bone"),
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


def monster_run(t):
    s = _wave(t)
    return _merge(MONSTER_READY, _legs_cycle(t, stride=30, knee=58, lift=14), {
        "loc": (0, -0.02, -0.03 + 0.035 * abs(math.cos(2 * math.pi * t))),
        "spine": (6, 0, 0),
        "chest": (0, 9 * s, 3 * s),
        "upper_arm.L": (22 * s, 0, 0),
        "upper_arm.R": (-22 * s, 0, 0),
        "forearm.L": (-12 * max(0.0, s), 0, 0),
        "forearm.R": (-12 * max(0.0, -s), 0, 0),
        "head": (0, -6 * s, 0),
    })


def monster_attack(t, frames=7, windup_end=2, strike_end=3):
    ready = MONSTER_READY
    keys = {
        0: ready,
        windup_end: _merge(ready, {"spine": (-24, 0, 0), "head": (16, 0, 0), "upper_arm.L": (-130, 0, -30),
                                   "upper_arm.R": (-130, 0, 30), "forearm.L": (-40, 0, 0), "forearm.R": (-40, 0, 0),
                                   "loc": (0, 0.05, 0.03)}),
        strike_end: _merge(ready, {"spine": (24, 0, 0), "head": (-4, 0, 0), "upper_arm.L": (-40, 0, 8),
                                   "upper_arm.R": (-40, 0, -8), "forearm.L": (-6, 0, 0), "forearm.R": (-6, 0, 0),
                                   "thigh.L": (-30, 0, 0), "loc": (0, -0.12, -0.06)}),
        frames - 1: ready,
    }
    return _blend(keys, t * (frames - 1))


def monster_hit(t, frames=3):
    keys = {0: MONSTER_READY,
            1: _merge(MONSTER_READY, {"spine": (-22, 0, 0), "head": (-10, 0, 0), "upper_arm.L": (10, 0, -16),
                                      "upper_arm.R": (10, 0, 16), "loc": (0, 0.05, 0.0)}),
            2: MONSTER_READY}
    return _blend(keys, t * (frames - 1))


def monster_death(t, frames=10):
    keys = death_keys(MONSTER_READY)
    # те же позы, ужатые в 10 кадров
    scaled = {round(k * (frames - 1) / 11): v for k, v in keys.items()}
    return _blend(scaled, t * (frames - 1))


# ---------- варианты ----------
# height — рост в метрах (скелет рассчитан на 1,0 ≈ 1,78 м в капюшоне), width — множитель ширины,
# target_px — высота силуэта стойки (S, кадр 0) на сетке 640×360 (ADR 0003 п. 3), canvas — холст рендера, px.
# clips: имя → (функция, кадров, кадров/с, цикл, направления, фазы удара).

HERO_DIRS = ["S", "SE", "E", "NE", "N"]
HERO_MIRROR = {"SW": "SE", "W": "E", "NW": "NE"}
MONSTER_DIRS = ["S", "E", "N"]
MONSTER_MIRROR = {"W": "E"}

MODELS = {
    "lamplighter": {
        "note": "тестовый герой-фонарщик: капюшон, плащ, фонарь, клинок",
        "height": 1.0, "width": 1.0, "target_px": 48, "canvas": 96, "anchor_y": 0.70,
        "parts": lamplighter_parts,
        "carrier": "spine",
        "directions": HERO_DIRS, "mirror": HERO_MIRROR,
        "clips": {
            "idle": (hero_idle, 8, 8, True, None, None),
            "run": (hero_run, 8, 12, True, None, None),
            "attack": (hero_attack, 9, 15, False, None, {"windup": [0, 3], "active": [3, 5], "recovery": [5, 9]}),
            "hit": (hero_hit, 4, 12, False, None, None),
            "death": (hero_death, 12, 12, False, ["S"], None),
        },
    },
    "ghoul": {
        "note": "тестовый рядовой враг: сутулый, когти, без оружия",
        "height": 0.92, "width": 0.95, "target_px": 30, "canvas": 64, "anchor_y": 0.72,
        "parts": ghoul_parts,
        "carrier": "spine",
        "directions": MONSTER_DIRS, "mirror": MONSTER_MIRROR,
        "clips": {
            "run": (monster_run, 8, 12, True, None, None),
            "attack": (monster_attack, 7, 12, False, None, {"windup": [0, 3], "active": [3, 4], "recovery": [4, 7]}),
            "hit": (monster_hit, 3, 12, False, None, None),
            "death": (monster_death, 10, 12, False, ["S"], None),
        },
    },
    "pillar": prop("обломок колонны с упавшим куском капители", pillar_parts, 70, 128),
    "tomb": prop("надгробие", tomb_parts, 22, 48),
    "cross": prop("могильный крест на холмике", cross_parts, 26, 48),
    "brazier": prop("жаровня на треноге с углями (пламя — эффект в игре)", brazier_parts, 26, 48),
    "statue": prop("статуя фонарщика в капюшоне на постаменте", statue_parts, 72, 128),
    "sarcophagus": prop("саркофаг со сдвинутой крышкой", sarcophagus_parts, 20, 64, anchor_y=0.7),
    "brute": {
        "note": "тестовая элита: тот же риг, массивные пропорции, костяная броня",
        "height": 1.12, "width": 1.45, "target_px": 60, "canvas": 128, "anchor_y": 0.72,
        "parts": brute_parts,
        "carrier": "spine",
        "directions": MONSTER_DIRS, "mirror": MONSTER_MIRROR,
        "clips": {
            "run": (monster_run, 8, 10, True, None, None),
            "attack": (lambda t: monster_attack(t, frames=9, windup_end=3, strike_end=5), 9, 10, False, None,
                       {"windup": [0, 4], "active": [4, 6], "recovery": [6, 9]}),
            "hit": (monster_hit, 3, 10, False, None, None),
            "death": (monster_death, 10, 10, False, ["S"], None),
        },
    },
}
