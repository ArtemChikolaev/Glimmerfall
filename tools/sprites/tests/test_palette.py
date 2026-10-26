import copy
import json
import re
import unittest

import numpy as np

from glimmer_sprites import color, palette as palettes


class PaletteTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.palette = palettes.load()

    def test_master_palette_follows_style_bible(self):
        self.assertEqual([], self.palette.problems())
        self.assertTrue(48 <= len(self.palette.ids) <= 64)

    def test_gpl_for_aseprite_is_fresh(self):
        self.assertEqual(self.palette.gpl(), palettes.GPL_PATH.read_text(encoding="utf-8"),
                         "content/art/palette.gpl устарел — make palette")

    def test_rules_catch_broken_palettes(self):
        def broken(mutate):
            document = copy.deepcopy(self.palette.document)
            mutate(document)
            return palettes.Palette(document).problems()

        def telegraph_in_fire(d):
            d["ramps"]["fire"]["tones"][0] = "telegraph1"

        def near_duplicate(d):
            d["colors"]["stone4"] = "#3c454e"
            d["colors"]["ink2"] = "#3c454d"
            d["ramps"]["stone"]["tones"].append("ink2")

        def dark_core(d):
            d["ramps"]["frost"]["tones"] = d["ramps"]["frost"]["tones"][:2]

        def unsorted(d):
            d["ramps"]["bone"]["tones"].reverse()

        self.assertTrue(any("зарезервирован" in p for p in broken(telegraph_in_fire)))
        self.assertTrue(any("почти одинаковы" in p for p in broken(near_duplicate)))
        self.assertTrue(any("ядро" in p for p in broken(dark_core)))
        self.assertTrue(any("светлеет" in p for p in broken(unsorted)))

    def test_quantize_maps_palette_to_itself_and_noise_to_nearest(self):
        p = self.palette
        rgba = np.zeros((1, len(p.ids), 4), dtype=np.uint8)
        rgba[0, :, :3] = p.rgb8
        rgba[0, :, 3] = 255
        self.assertEqual(list(range(1, len(p.ids) + 1)), p.quantize(rgba)[0].tolist())
        fire3 = p.rgb8[p.index("fire3") - 1].astype(int)
        nudged = np.array([[[*(fire3 + [-3, 2, 1]).clip(0, 255), 255], [0, 0, 0, 0]]], dtype=np.uint8)
        self.assertEqual([p.index("fire3"), 0], p.quantize(nudged)[0].tolist())

    def test_oklab_roundtrip(self):
        rgb = np.random.default_rng(1).random((50, 3))
        back = color.linear_to_srgb(color.oklab_to_linear(color.srgb_to_oklab(rgb)))
        np.testing.assert_allclose(rgb, back, atol=1e-6)
        self.assertAlmostEqual(1.0, float(color.hex_to_oklab("#ffffff")[0]), places=3)

    def test_palette_authoring_from_oklch(self):
        # цвета палитры подбираются в OKLCH: каждый цвет восстанавливается из своих L, C, h
        for code in self.palette.hexes:
            lightness, chroma, hue = color.oklab_to_oklch(color.hex_to_oklab(code))
            self.assertEqual(code, color.srgb_to_hex(color.oklch_to_srgb(lightness, chroma, hue)))
        out_of_gamut = color.oklch_to_srgb(0.6, 0.5, 140.0)   # хрома за охватом sRGB урезается, а не ломает цвет
        self.assertTrue(np.all((out_of_gamut >= 0) & (out_of_gamut <= 1)))

    def test_style_bible_lightness_table_matches_palette(self):
        text = (palettes.REPO / "docs" / "art" / "style-bible.md").read_text(encoding="utf-8")
        rows = {name: (float(lo), float(hi)) for name, lo, hi in
                re.findall(r"^\| (world|material|vfx|telegraph) \| ([0-9.]+)–([0-9.]+) \|", text, re.M)}
        self.assertEqual(self.palette.lightness, rows, "таблица плана светлоты в docs/art/style-bible.md разошлась с палитрой")

    def test_document_matches_schema_and_order_defines_indices(self):
        raw = json.loads(palettes.PALETTE_PATH.read_text(encoding="utf-8"))
        self.assertEqual(list(raw["colors"]), list(self.palette.ids))
        self.assertEqual(1, self.palette.index(next(iter(raw["colors"]))))


if __name__ == "__main__":
    unittest.main()
