"""Метрики QA на синтетике: каждая ловит свой дефект и молчит на чистом движении."""

import math
import unittest

import numpy as np

from glimmer_sprites import qa


def disc(cx, cy, r=6, size=32, value=5):
    y, x = np.mgrid[0:size, 0:size]
    frame = np.zeros((size, size), dtype=np.uint8)
    frame[(x - cx) ** 2 + (y - cy) ** 2 <= r * r] = value
    return frame


class FlickerTest(unittest.TestCase):
    def test_smooth_translation_does_not_flicker(self):
        for speed in (1, 2):
            frames = [disc(6 + speed * t, 16) for t in range(8)]
            self.assertEqual(0.0, max(qa.flicker_rates(frames, loop=False)), f"{speed} px/кадр")

    def test_salt_noise_flickers(self):
        rng = np.random.default_rng(3)
        frames = []
        for _ in range(8):
            f = disc(16, 16, r=10)
            ys, xs = rng.integers(8, 24, size=(2, 12))
            f[ys, xs] = 7                        # кипение цвета внутри силуэта
            frames.append(f)
        self.assertGreater(np.mean(qa.flicker_rates(frames, loop=True)), qa.GATE.flicker_mean)

    def test_thin_tip_moving_a_pixel_per_frame_is_motion(self):
        frames = []
        for t in range(6):
            f = np.zeros((16, 16), dtype=np.uint8)
            f[4:12, 3] = 5
            f[8, 4:6 + t] = 5                    # тонкий «клинок» растёт на пиксель за кадр
            frames.append(f)
        self.assertEqual(0.0, max(qa.flicker_rates(frames, loop=False)))

    def test_peak_on_a_frame_pops_and_half_frame_sampling_holds_it(self):
        # пик синуса ровно на кадре — кадры t−1 и t+1 совпадают, и край в пике «вспыхивает» на один кадр;
        # конвейер сэмплирует цикл со сдвигом в полкадра (render.sample_time), и пик держится два кадра
        on_peak = [disc(16, 16 + round(2 * math.sin(2 * math.pi * t / 8)), r=8) for t in range(8)]
        held = [disc(16, 16 + round(2 * math.sin(2 * math.pi * (t + 0.5) / 8)), r=8) for t in range(8)]
        peak_worst = max(qa.flicker_rates(on_peak, loop=True))
        held_worst = max(qa.flicker_rates(held, loop=True))
        self.assertGreater(peak_worst, qa.GATE.flicker_max)
        self.assertLess(held_worst, peak_worst / 2)
        self.assertLess(held_worst, qa.GATE.flicker_max)


class MotionMetricsTest(unittest.TestCase):
    def test_strobe_detects_alternating_poses(self):
        a, b = disc(12, 16), disc(20, 16)
        strobe = qa.sequence_metrics([a, b] * 4, loop=True)["strobe"]
        smooth = qa.sequence_metrics([disc(16 + round(4 * math.sin(2 * math.pi * (t + 0.5) / 8)), 16) for t in range(8)], loop=True)["strobe"]
        self.assertLess(strobe, 0.8)
        self.assertGreater(smooth, 0.8)

    def test_drift_detects_root_motion_left_in_a_loop(self):
        drifting = qa.sequence_metrics([disc(8 + 2 * t, 16) for t in range(8)], loop=True)
        bobbing = qa.sequence_metrics([disc(16 + round(3 * math.sin(2 * math.pi * (t + 0.5) / 8)), 16) for t in range(8)], loop=True)
        self.assertGreater(drifting["drift_px"], 5)
        self.assertLess(bobbing["drift_px"], 1.5)

    def test_pieces_and_dropout(self):
        whole = disc(16, 16, r=8)
        broken = whole.copy()
        broken[:, 15:18] = 0                    # силуэт разрезан пополам
        m = qa.sequence_metrics([whole, broken, whole], loop=False)
        self.assertEqual(2, m["pieces"])
        self.assertGreater(m["dropout"], 0.1)


if __name__ == "__main__":
    unittest.main()
