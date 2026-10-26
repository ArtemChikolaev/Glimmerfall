"""Цвет: sRGB ⇄ OKLab/OKLCH (Björn Ottosson, 2020) — векторно на numpy.

OKLab выбран потому, что расстояние в нём близко к воспринимаемому: светлота L ∈ [0, 1] для плана светлоты,
евклидово расстояние — для квантования в фиксированную палитру и проверки зарезервированного цвета телеграфов.
"""

from __future__ import annotations

import numpy as np

_LMS_FROM_LINEAR = np.array([
    [0.4122214708, 0.5363325363, 0.0514459929],
    [0.2119034982, 0.6806995451, 0.1073969566],
    [0.0883024619, 0.2817188376, 0.6299787005],
])
_LAB_FROM_LMS_ = np.array([
    [0.2104542553, 0.7936177850, -0.0040720468],
    [1.9779984951, -2.4285922050, 0.4505937099],
    [0.0259040371, 0.7827717662, -0.8086757660],
])
_LMS__FROM_LAB = np.linalg.inv(_LAB_FROM_LMS_)
_LINEAR_FROM_LMS = np.linalg.inv(_LMS_FROM_LINEAR)


def hex_to_srgb(code: str) -> np.ndarray:
    """'#rrggbb' → sRGB [0, 1]."""
    code = code.lstrip("#")
    if len(code) != 6:
        raise ValueError(f"expected #rrggbb, got #{code}")
    return np.array([int(code[i:i + 2], 16) for i in (0, 2, 4)], dtype=np.float64) / 255.0


def srgb_to_hex(rgb: np.ndarray) -> str:
    r, g, b = (int(round(float(c) * 255.0)) for c in np.clip(rgb, 0.0, 1.0))
    return f"#{r:02x}{g:02x}{b:02x}"


def srgb_to_linear(c: np.ndarray) -> np.ndarray:
    c = np.asarray(c, dtype=np.float64)
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def linear_to_srgb(c: np.ndarray) -> np.ndarray:
    c = np.clip(np.asarray(c, dtype=np.float64), 0.0, None)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * np.power(c, 1 / 2.4) - 0.055)


def linear_to_oklab(rgb: np.ndarray) -> np.ndarray:
    """Линейный sRGB (..., 3) → OKLab (..., 3)."""
    lms = np.asarray(rgb, dtype=np.float64) @ _LMS_FROM_LINEAR.T
    return np.cbrt(lms) @ _LAB_FROM_LMS_.T


def oklab_to_linear(lab: np.ndarray) -> np.ndarray:
    lms_ = np.asarray(lab, dtype=np.float64) @ _LMS__FROM_LAB.T
    return (lms_ ** 3) @ _LINEAR_FROM_LMS.T


def srgb_to_oklab(rgb: np.ndarray) -> np.ndarray:
    return linear_to_oklab(srgb_to_linear(rgb))


def hex_to_oklab(code: str) -> np.ndarray:
    return srgb_to_oklab(hex_to_srgb(code))


def oklab_to_oklch(lab: np.ndarray) -> np.ndarray:
    lab = np.asarray(lab, dtype=np.float64)
    chroma = np.hypot(lab[..., 1], lab[..., 2])
    hue = np.degrees(np.arctan2(lab[..., 2], lab[..., 1])) % 360.0
    return np.stack([lab[..., 0], chroma, hue], axis=-1)


def oklch_to_srgb(lightness: float, chroma: float, hue: float) -> np.ndarray:
    """OKLCH → sRGB [0, 1]; вне охвата sRGB хрома уменьшается, светлота и тон сохраняются."""
    h = np.radians(hue)

    def linear(c: float) -> np.ndarray:
        return oklab_to_linear(np.array([lightness, c * np.cos(h), c * np.sin(h)]))

    def inside(rgb: np.ndarray) -> bool:
        return bool(np.all(rgb >= -1e-9) and np.all(rgb <= 1 + 1e-9))

    rgb = linear(chroma)
    if not inside(rgb):
        lo, hi = 0.0, chroma
        for _ in range(40):
            mid = (lo + hi) / 2
            if inside(linear(mid)):
                lo = mid
            else:
                hi = mid
        rgb = linear(lo)
    return linear_to_srgb(np.clip(rgb, 0.0, 1.0))


def hue_distance(a: float, b: float) -> float:
    d = abs(a - b) % 360.0
    return min(d, 360.0 - d)
