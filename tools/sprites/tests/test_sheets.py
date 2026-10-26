"""Закоммиченные листы: свежесть индекса, схема, палитра, раскладка и QA-гейт (тот же, что `make sprites-check`)."""

import json
import unittest

import numpy as np
from PIL import Image

from glimmer_sprites import build, palette as palettes, provenance, qa, sheet as sheets


class CommittedSheetsTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.palette = palettes.load()
        cls.metas = qa.load_metas()

    def test_there_are_sheets(self):
        self.assertTrue(self.metas, "в content/art/sprites/index.json нет листов")

    def test_index_is_fresh(self):
        self.assertEqual([], sheets.freshness_problems())

    def test_meta_matches_schema_and_image_layout(self):
        for meta in self.metas:
            with self.subTest(meta["id"]):
                palettes.validate_schema(meta, "sprite-sheet.schema.json")
                image = Image.open(build.SPRITES_IMAGE_DIR / meta["image"])
                cell_w, cell_h = meta["cell"]
                rows = sum(len(c["dirs"]) for c in meta["clips"].values())
                self.assertEqual((meta["columns"] * cell_w, rows * cell_h), image.size)
                self.assertEqual("P", image.mode)
                ax, ay = meta["anchor"]
                self.assertTrue(0 <= ax < cell_w and 0 <= ay < cell_h)
                starts = sorted(c["row"] for c in meta["clips"].values())
                self.assertEqual(0, starts[0])
                for clip in meta["clips"].values():
                    self.assertLessEqual(clip["frames"], meta["columns"])
                    self.assertTrue(set(clip["dirs"]) <= set(meta["directions"]["rendered"]))
                    if "phases" in clip:
                        ph = clip["phases"]
                        self.assertEqual([0, ph["active"][0], ph["recovery"][0], clip["frames"]],
                                         [ph["windup"][0], ph["windup"][1], ph["active"][1], ph["recovery"][1]])

    def test_pixels_use_only_the_master_palette(self):
        expected = self.palette.pil_palette()[:3 * (len(self.palette.ids) + 1)]
        for meta in self.metas:
            with self.subTest(meta["id"]):
                image = Image.open(build.SPRITES_IMAGE_DIR / meta["image"])
                self.assertEqual(expected, image.getpalette()[:len(expected)])
                self.assertLessEqual(int(np.asarray(image).max()), len(self.palette.ids))

    def test_qa_gate(self):
        for meta in self.metas:
            with self.subTest(meta["id"]):
                self.assertEqual([], qa.analyse(meta, self.palette).failures)

    def test_every_asset_has_provenance(self):
        self.assertEqual([], provenance.problems())

    def test_sheets_listed_in_meta_dir_are_the_index(self):
        listed = set(json.loads(sheets.INDEX_PATH.read_text(encoding="utf-8"))["sheets"])
        self.assertEqual(listed, {m["id"] for m in self.metas})


if __name__ == "__main__":
    unittest.main()
