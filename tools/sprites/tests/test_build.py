import unittest

import numpy as np

from glimmer_sprites import build, palette as palettes


class DownscaleTest(unittest.TestCase):
    def test_coverage_and_mode_per_block(self):
        k = 4
        covered = np.zeros((4, 8), dtype=bool)
        covered[:, :4] = True                 # левый блок покрыт полностью
        covered[:1, 4:] = True                # правый — на четверть: прозрачный
        mat = np.where(covered, 3, 0)
        tone = np.full((4, 8), build.TONE_LIT)
        tone[:1, :4] = build.TONE_SHADOW      # 4 субпикселя тени против 12 света
        depth = np.full((4, 8), 0.5)
        opaque, m, t, d = build.downscale(covered, mat, tone, depth, k, 0.5)
        self.assertEqual([[True, False]], opaque.tolist())
        self.assertEqual(3, m[0, 0])
        self.assertEqual(build.TONE_LIT, t[0, 0])
        self.assertEqual(0.5, d[0, 0])

    def test_inner_line_goes_to_the_far_side_of_a_depth_step(self):
        opaque = np.ones((3, 3), dtype=bool)
        tone = np.full((3, 3), build.TONE_LIT)
        depth = np.full((3, 3), 0.6)
        depth[:, 0] = 0.2                     # левый столбец ближе
        lined = build.inner_lines(opaque, tone, depth, threshold=0.1)
        self.assertEqual([build.TONE_LIT, build.TONE_DEEP, build.TONE_LIT], lined[1].tolist())

    def test_outline_is_outside_the_silhouette(self):
        opaque = np.zeros((5, 5), dtype=bool)
        opaque[2, 2] = True
        ring = build.outline_mask(opaque)
        self.assertEqual(4, int(ring.sum()))
        self.assertFalse(ring[2, 2])
        self.assertFalse(ring[1, 1])


class DespeckleTest(unittest.TestCase):
    def test_one_frame_speck_is_removed_but_motion_stays(self):
        frames = [np.zeros((8, 8), dtype=np.uint8) for _ in range(4)]
        for t, f in enumerate(frames):
            f[2:6, t:t + 2] = 5               # брусок едет вправо — движение
        frames[1][7, 7] = 9                   # вспышка на один кадр
        clean = build.despeckle(frames, loop=False, max_px=2)
        self.assertEqual(0, clean[1][7, 7])
        for a, b in zip(frames, clean):
            np.testing.assert_array_equal(np.where(a == 9, 0, a), b)

    def test_off_switch(self):
        frames = [np.zeros((2, 2), dtype=np.uint8), np.ones((2, 2), dtype=np.uint8), np.zeros((2, 2), dtype=np.uint8)]
        self.assertIs(frames, build.despeckle(frames, loop=True, max_px=0))


class IndexMappingTest(unittest.TestCase):
    def test_material_tones_map_to_ramps(self):
        p = palettes.load()
        materials = {"1": {"ramp": "crimson", "glow": False}, "2": {"ramp": "radiance", "glow": True}}
        opaque = np.array([[True, True, True, False]])
        mat = np.array([[1, 1, 2, 0]])
        tone = np.array([[build.TONE_DEEP, build.TONE_HIGHLIGHT, build.TONE_HIGHLIGHT, 0]])
        out = build.to_indices(opaque, mat, tone, materials, p)
        crimson, radiance = p.ramp_indices("crimson"), p.ramp_indices("radiance")
        self.assertEqual([crimson[0], crimson[3], radiance[-1], 0], out[0].tolist())


if __name__ == "__main__":
    unittest.main()
