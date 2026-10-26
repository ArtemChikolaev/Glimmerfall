"""Рендер манекена в проходы для пиксельного конвейера (G2.3). Запускается внутри Blender:

    blender -b --factory-startup -P tools/sprites/blender/render.py -- --model lamplighter --scale 1 --out DIR

На кадр — два прохода Cycles по 1 сэмплу в центр пикселя (без сглаживания, без шума):
  • _n.png — нормаль в осях камеры (x вправо, y вверх, z от зрителя), закодированная n·0,5 + 0,5;
  • _m.png — R: номер материала, B: глубина вдоль взгляда (0 — ближе), A: покрытие.
Свет, полосы тона, палитра, контур и упаковка — дальше на хосте (glimmer_sprites.build), поэтому стиль
правится без перерендера. Вид — Raw: в PNG лежат линейные значения как есть.
"""

from __future__ import annotations

import argparse
import json
import math
import os
import sys
import time

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import models  # noqa: E402

PITCH_DEG = 42.0          # ADR 0003 п. 3: наклон камеры около 42°
CAMERA_DISTANCE = 20.0
SCREEN_ANGLE = {"S": 0, "SE": 45, "E": 90, "NE": 135, "N": 180, "NW": 225, "W": 270, "SW": 315}


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser(prog="render.py")
    p.add_argument("--model", required=True, choices=sorted(models.MODELS))
    p.add_argument("--scale", type=float, default=1.0, help="1 — сетка 640×360, 1.5 — 960×540")
    p.add_argument("--supersample", type=int, default=4)
    p.add_argument("--out", required=True)
    p.add_argument("--clips", default="", help="через запятую; пусто — все")
    return p.parse_args(argv)


def sample_time(f: int, frames: int, loop: bool) -> float:
    """Время кадра. Цикл сэмплируется со сдвигом в полкадра: симметричный пик движения ложится между кадрами
    и держится два кадра, а не мелькает одним (иначе кадры t−1 и t+1 совпадают и пиксели пика вспыхивают)."""
    if loop:
        return (f + 0.5) / frames
    return f / (frames - 1) if frames > 1 else 0.0


def yaw_for(direction: str) -> float:
    """Поворот модели, при котором её взгляд на экране идёт под нужным углом (сим рисуется сверху без сжатия)."""
    a = math.radians(SCREEN_ANGLE[direction])
    return math.atan2(math.sin(a), math.cos(a) / math.sin(math.radians(PITCH_DEG)))


def reset_scene():
    for collection in (bpy.data.objects, bpy.data.meshes, bpy.data.materials, bpy.data.armatures, bpy.data.cameras):
        for block in list(collection):
            collection.remove(block)


def unit_to_world(model):
    w, h = model["width"], model["height"]
    return Matrix.Diagonal((w, w, h, 1.0))


def build_rig(model):
    scene = bpy.context.scene
    turntable = bpy.data.objects.new("turntable", None)
    scene.collection.objects.link(turntable)
    data = bpy.data.armatures.new("rig")
    rig = bpy.data.objects.new("rig", data)
    scene.collection.objects.link(rig)
    rig.parent = turntable
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode="EDIT")
    s = unit_to_world(model)
    for name, (parent, head, tail, z_hint) in models.SKELETON.items():
        bone = data.edit_bones.new(name)
        bone.head = s @ Vector(head)
        bone.tail = s @ Vector(tail)
        bone.align_roll(Vector(z_hint))
        if parent:
            bone.parent = data.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT")
    for bone in rig.data.bones:
        # конвенции поворотов в models.py держатся, пока X кости почти совпадает с мировым +X
        assert bone.x_axis.dot(Vector((1, 0, 0))) > 0.98, f"ось X кости {bone.name} ушла от мировой: {bone.x_axis}"
    for pose_bone in rig.pose.bones:
        pose_bone.rotation_mode = "XYZ"
    return turntable, rig


def _orient(direction: Vector) -> Matrix:
    """Поворот, переводящий +Z в direction."""
    return direction.normalized().to_track_quat("Z", "Y").to_matrix().to_4x4()


def part_mesh(part, bm):
    shape = part["shape"]
    if shape == "limb":
        _, head, tail, _ = models.SKELETON[part["bone"]]
        head, tail = Vector(head), Vector(tail)
        axis = tail - head
        e0, e1 = part["extend"]
        head = head - axis.normalized() * e0
        tail = tail + axis.normalized() * e1
        length = (tail - head).length
        matrix = Matrix.Translation((head + tail) / 2) @ _orient(head - tail)
        bmesh.ops.create_cone(bm, cap_ends=True, segments=10, radius1=part["r"][1], radius2=part["r"][0],
                              depth=length, matrix=matrix)
    elif shape == "ball":
        matrix = Matrix.Translation(part["center"]) @ Euler([math.radians(a) for a in part.get("rot", (0, 0, 0))]).to_matrix().to_4x4() \
            @ Matrix.Diagonal((*part["scale"], 1.0))
        bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=part["radius"], matrix=matrix)
    elif shape == "box":
        matrix = Matrix.Translation(part["center"]) @ Euler([math.radians(a) for a in part["rot"]]).to_matrix().to_4x4() \
            @ Matrix.Diagonal((*part["size"], 1.0))
        bmesh.ops.create_cube(bm, size=1.0, matrix=matrix)
    elif shape == "cyl":
        matrix = Matrix.Translation(part["center"]) @ Euler([math.radians(a) for a in part["rot"]]).to_matrix().to_4x4()
        bmesh.ops.create_cone(bm, cap_ends=True, segments=12, radius1=part["r"][0], radius2=part["r"][1],
                              depth=part["depth"], matrix=matrix)
    elif shape == "cloak":
        top = Vector(part["top"])
        depth = top.z - part["bottom_z"]
        matrix = Matrix.Translation((top.x, top.y, top.z - depth / 2))
        bmesh.ops.create_cone(bm, cap_ends=False, segments=20, radius1=part["r"][1], radius2=part["r"][0],
                              depth=depth, matrix=matrix)
        front = [v for v in bm.verts if v.co.y < top.y - 0.04]
        bmesh.ops.delete(bm, geom=front, context="VERTS")
    else:
        raise ValueError(f"unknown shape {shape}")
    smooth = shape in ("ball", "limb", "cyl", "cloak")
    for face in bm.faces:
        face.smooth = smooth


def build_parts(model, rig, material_ids):
    s = unit_to_world(model)
    objects = []
    for i, part in enumerate(model["parts"]()):
        bm = bmesh.new()
        part_mesh(part, bm)
        bm.transform(s)
        mesh = bpy.data.meshes.new(f"part{i}")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"part{i}_{part['bone']}", mesh)
        bpy.context.scene.collection.objects.link(obj)
        obj.parent = rig
        group = obj.vertex_groups.new(name=part["bone"])
        group.add(list(range(len(mesh.vertices))), 1.0, "REPLACE")
        modifier = obj.modifiers.new("rig", "ARMATURE")
        modifier.object = rig
        key = (part["ramp"], bool(part["glow"]))
        obj["mat_id"] = material_ids.setdefault(key, len(material_ids) + 1)
        objects.append(obj)
    return objects


def apply_pose(rig, pose):
    for pose_bone in rig.pose.bones:
        pose_bone.rotation_euler = (0, 0, 0)
        pose_bone.location = (0, 0, 0)
    for bone, value in pose.items():
        if bone == "loc":
            x, y, z = value
            rig.pose.bones["pelvis"].location = (x, z, -y)   # оси таза: X — мир X, Y — вверх, Z — вперёд (−Y)
        else:
            rig.pose.bones[bone].rotation_euler = [math.radians(a) for a in value]


def pass_materials(depth_near, depth_far):
    normal = bpy.data.materials.new("pass_normal")
    normal.use_nodes = True
    nt = normal.node_tree
    nt.nodes.clear()
    geometry = nt.nodes.new("ShaderNodeNewGeometry")
    to_camera = nt.nodes.new("ShaderNodeVectorTransform")
    to_camera.vector_type, to_camera.convert_from, to_camera.convert_to = "NORMAL", "WORLD", "CAMERA"
    encode = nt.nodes.new("ShaderNodeVectorMath")
    encode.operation = "MULTIPLY_ADD"
    encode.inputs[1].default_value = (0.5, 0.5, 0.5)
    encode.inputs[2].default_value = (0.5, 0.5, 0.5)
    emission = nt.nodes.new("ShaderNodeEmission")
    output = nt.nodes.new("ShaderNodeOutputMaterial")
    nt.links.new(geometry.outputs["Normal"], to_camera.inputs["Vector"])
    nt.links.new(to_camera.outputs["Vector"], encode.inputs[0])
    nt.links.new(encode.outputs["Vector"], emission.inputs["Color"])
    nt.links.new(emission.outputs["Emission"], output.inputs["Surface"])

    ident = bpy.data.materials.new("pass_material")
    ident.use_nodes = True
    nt = ident.node_tree
    nt.nodes.clear()
    attribute = nt.nodes.new("ShaderNodeAttribute")
    attribute.attribute_type, attribute.attribute_name = "OBJECT", "mat_id"
    to_unit = nt.nodes.new("ShaderNodeMath")
    to_unit.operation = "DIVIDE"
    to_unit.inputs[1].default_value = 255.0
    camera = nt.nodes.new("ShaderNodeCameraData")
    depth = nt.nodes.new("ShaderNodeMapRange")
    depth.clamp = True
    depth.inputs["From Min"].default_value = depth_near
    depth.inputs["From Max"].default_value = depth_far
    combine = nt.nodes.new("ShaderNodeCombineColor")
    emission = nt.nodes.new("ShaderNodeEmission")
    output = nt.nodes.new("ShaderNodeOutputMaterial")
    nt.links.new(attribute.outputs["Fac"], to_unit.inputs[0])
    nt.links.new(to_unit.outputs["Value"], combine.inputs[0])
    nt.links.new(camera.outputs["View Z Depth"], depth.inputs["Value"])
    nt.links.new(depth.outputs["Result"], combine.inputs[2])
    nt.links.new(combine.outputs["Color"], emission.inputs["Color"])
    nt.links.new(emission.outputs["Emission"], output.inputs["Surface"])
    return normal, ident


def setup_render(resolution):
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    cycles = scene.cycles
    cycles.device = "CPU"
    cycles.samples = 1
    cycles.use_adaptive_sampling = False
    cycles.use_denoising = False
    cycles.pixel_filter_type = "BOX"
    cycles.filter_width = 0.01          # один луч в центр пикселя — без сглаживания
    cycles.max_bounces = 0
    cycles.seed = 0
    cycles.use_animated_seed = False
    scene.render.use_persistent_data = True
    scene.render.film_transparent = True
    scene.render.resolution_x = scene.render.resolution_y = resolution
    scene.render.resolution_percentage = 100
    scene.render.dither_intensity = 0.0
    scene.view_settings.view_transform = "Raw"
    scene.view_settings.look = "None"
    scene.view_settings.exposure = 0.0
    scene.view_settings.gamma = 1.0
    settings = scene.render.image_settings
    settings.file_format = "PNG"
    settings.color_mode = "RGBA"
    settings.color_depth = "8"
    settings.compression = 15


def camera_axes():
    rotation = Euler((math.radians(90.0 - PITCH_DEG), 0, 0)).to_matrix()
    return rotation, rotation @ Vector((0, 1, 0)), rotation @ Vector((0, 0, -1))


def place_camera(canvas, anchor_y, ppm):
    rotation, up, forward = camera_axes()
    data = bpy.data.cameras.new("camera")
    data.type = "ORTHO"
    data.ortho_scale = canvas / ppm
    data.clip_start, data.clip_end = 1.0, CAMERA_DISTANCE * 2
    camera = bpy.data.objects.new("camera", data)
    bpy.context.scene.collection.objects.link(camera)
    target = up * ((anchor_y - canvas / 2) / ppm)
    camera.location = target - forward * CAMERA_DISTANCE
    camera.rotation_euler = rotation.to_euler()
    bpy.context.scene.camera = camera
    return camera


def screen_extent(objects):
    """Высота силуэта в метрах экрана: проекция вершин (с деформацией ригом) на ось «вверх» камеры."""
    _, up, _ = camera_axes()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    lo, hi = math.inf, -math.inf
    for obj in objects:
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        for v in mesh.vertices:
            y = (evaluated.matrix_world @ v.co).dot(up)
            lo, hi = min(lo, y), max(hi, y)
        evaluated.to_mesh_clear()
    return hi - lo


def carrier_offsets(rig, clip_fn, frames, loop, carrier):
    """Нейтрализация root motion: линейный снос несущей кости по циклу вычитается, покачивание остаётся."""
    if not loop:
        return [(0.0, 0.0)] * frames
    rig.location = (0.0, 0.0, 0.0)
    points = []
    for f in range(frames):
        apply_pose(rig, clip_fn(sample_time(f, frames, loop)))
        bpy.context.view_layer.update()
        head = rig.matrix_local @ rig.pose.bones[carrier].head
        points.append((head.x, head.y))
    mean_f = (frames - 1) / 2
    denominator = sum((f - mean_f) ** 2 for f in range(frames)) or 1.0
    offsets = []
    slopes = []
    for axis in (0, 1):
        mean_v = sum(p[axis] for p in points) / frames
        slopes.append(sum((f - mean_f) * (points[f][axis] - mean_v) for f in range(frames)) / denominator)
    for f in range(frames):
        offsets.append((slopes[0] * (f - mean_f), slopes[1] * (f - mean_f)))
    return offsets


def main():
    args = parse_args()
    model = models.MODELS[args.model]
    started = time.time()
    reset_scene()
    turntable, rig = build_rig(model)
    material_ids: dict = {}
    parts = build_parts(model, rig, material_ids)
    clips = model["clips"]
    if args.clips:
        clips = {k: v for k, v in clips.items() if k in args.clips.split(",")}

    # масштаб: высота силуэта стойки (S, первый кадр первого клипа-цикла) = target_px × scale
    reference, reference_frames = next((fn, n) for fn, n, _, loop, _, _ in model["clips"].values() if loop)
    apply_pose(rig, reference(sample_time(0, reference_frames, True)))
    turntable.rotation_euler = (0, 0, yaw_for("S"))
    bpy.context.view_layer.update()
    extent = screen_extent(parts)
    canvas = int(round(model["canvas"] * args.scale))
    canvas += canvas % 2
    anchor_y = int(round(canvas * model["anchor_y"]))
    target_px = model["target_px"] * args.scale
    ppm = target_px / extent
    supersample = args.supersample
    place_camera(canvas, anchor_y, ppm)
    setup_render(canvas * supersample)
    radius = 2.6 * model["height"]
    normal_pass, material_pass = pass_materials(CAMERA_DISTANCE - radius, CAMERA_DISTANCE + radius)
    view_layer = bpy.context.view_layer
    scene = bpy.context.scene

    manifest = {
        "model": args.model,
        "class": model.get("class", "character"),
        "note": model["note"],
        "scale": args.scale,
        "supersample": supersample,
        "canvas": canvas,
        "anchor": [canvas // 2, anchor_y],
        "target_px": target_px,
        "pixels_per_meter": ppm,
        "pitch_deg": PITCH_DEG,
        "depth_range_m": 2 * radius,
        "materials": {str(i): {"ramp": ramp, "glow": glow} for (ramp, glow), i in material_ids.items()},
        "directions": model["directions"],
        "mirror": model["mirror"],
        "clips": {},
        "blender": bpy.app.version_string,
    }
    renders = 0
    for name, (fn, frames, fps, loop, dirs, phases) in clips.items():
        dirs = dirs or model["directions"]
        manifest["clips"][name] = {"frames": frames, "fps": fps, "loop": loop, "dirs": dirs, "phases": phases}
        turntable.rotation_euler = (0, 0, 0)
        offsets = carrier_offsets(rig, fn, frames, loop, model["carrier"])
        for direction in dirs:
            turntable.rotation_euler = (0, 0, yaw_for(direction))
            folder = os.path.join(args.out, name, direction)
            os.makedirs(folder, exist_ok=True)
            for f in range(frames):
                apply_pose(rig, fn(sample_time(f, frames, loop)))
                rig.location = (-offsets[f][0], -offsets[f][1], 0.0)
                for suffix, material in (("n", normal_pass), ("m", material_pass)):
                    view_layer.material_override = material
                    scene.render.filepath = os.path.join(folder, f"{f:02d}_{suffix}.png")
                    bpy.ops.render.render(write_still=True)
                    renders += 1
    manifest["renders"] = renders
    manifest["render_seconds"] = round(time.time() - started, 1)
    with open(os.path.join(args.out, "frames.json"), "w", encoding="utf-8") as fh:
        json.dump(manifest, fh, ensure_ascii=False, indent=1)
    print(f"render.py: {args.model}@{args.scale} — {renders} renders in {manifest['render_seconds']} s → {args.out}")


if __name__ == "__main__":
    main()
