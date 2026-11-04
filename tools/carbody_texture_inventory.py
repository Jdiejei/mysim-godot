"""Read-only texture inventory for carbody/livery evidence (no rendering, no mutation).

Builds, for one car folder (or one track folder): every source texture's true pixel
size/colour type (PNG IHDR), its Godot import settings (.import), which prefab
renderers bind it (reverse lookup through assets/car_map.json), the per-surface
UV-repair policy state (data/car_surface_uv_repairs.json), and - when an assembly
audit JSON is supplied - the measured per-surface basisDeterminant / metresPerUv /
visibility joined onto the rows. Writes a markdown table and a JSON sidecar.

Usage:
  python tools/carbody_texture_inventory.py --index 16
  python tools/carbody_texture_inventory.py --car 2018_lamborghini_huracan_gt3
  python tools/carbody_texture_inventory.py --track Brands_Hatch --assembly <audit.json>

Exit codes: 0 ok, 2 bad arguments, 3 missing required input (never a silent empty report).
"""
import argparse, json, pathlib, re, struct, sys

PNG_SIG = b"\x89PNG\r\n\x1a\n"
KNOWN_CHUNKS = {"IHDR", "PLTE", "IDAT", "IEND", "tRNS", "gAMA", "cHRM", "sRGB", "iCCP", "pHYs", "tIME", "tEXt", "zTXt", "iTXt", "bKGD"}


def png_info(path):
    data = path.read_bytes()
    if data[:8] != PNG_SIG:
        raise ValueError(f"not a PNG: {path}")
    width, height = struct.unpack(">II", data[16:24])
    bit_depth, colour_type = data[24], data[25]
    chunks, off = [], 8
    while off + 8 <= len(data) and len(chunks) < 4096:
        (length,) = struct.unpack(">I", data[off:off + 4])
        name = data[off + 4:off + 8].decode("latin-1", "replace")
        chunks.append(name)
        off += 12 + length
        if name == "IEND":
            break
    extra = [c for c in chunks if c not in KNOWN_CHUNKS]
    return {
        "width": width, "height": height, "bitDepth": bit_depth,
        "colourType": colour_type, "hasAlpha": colour_type in (4, 6),
        "iccOrGamma": [c for c in chunks if c in ("iCCP", "gAMA", "sRGB", "cHRM")],
        "unknownChunks": sorted(set(extra)), "bytes": len(data),
    }


def import_info(path):
    out = {"present": False}
    if not path.exists():
        return out
    text = path.read_text(encoding="utf-8", errors="replace")

    def grab(pattern, default=None, cast=str):
        m = re.search(pattern, text)
        return cast(m.group(1)) if m else default

    out = {
        "present": True,
        "importer": grab(r'importer="([^"]+)"'),
        "compressMode": grab(r"compress/mode=(\d+)", cast=int),
        "mipmaps": grab(r"mipmaps/generate=(\w+)"),
        "sizeLimit": grab(r"size_limit=(-?\d+)", cast=int),
        "detect3dTo": grab(r"detect_3d/compress_to=(\d+)", cast=int),
        "vramTexture": grab(r'"vram_texture": (\w+)'),
        "premultAlpha": grab(r"premult_alpha=(\w+)"),
        "fixAlphaBorder": grab(r"fix_alpha_border=(\w+)"),
    }
    return out


def car_catalog(root):
    """Parse src/Logic/CarCatalog.cs for the folder order (index -> folder)."""
    path = root / "src" / "Logic" / "CarCatalog.cs"
    text = path.read_text(encoding="utf-8", errors="replace")
    m = re.search(r"Folders=\{(.*?)\}", text, re.S)
    if not m:
        raise ValueError("CarCatalog Folders not found")
    folders = re.findall(r'"([^"]+)"', m.group(1))
    return {i: f for i, f in enumerate(folders)}


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def reverse_bindings(car_map):
    """texture res-path -> list of {car, renderer, guid, slot} from the car map."""
    out = {}
    res = lambda p: ("res://assets/" + p.split("Assets/", 1)[1]) if p and "Assets/" in p else p
    for index, renderers in car_map.get("cars", {}).items():
        for renderer, lists in renderers.items():
            guids = lists[0] if lists else []
            for guid in guids:
                mat = car_map.get("materials", {}).get(guid, {})
                for slot, raw in (mat.get("texs") or {}).items():
                    path = res(raw)
                    if path:
                        out.setdefault(path, []).append(
                            {"car": int(index), "renderer": renderer, "guid": guid, "slot": slot})
    return out


def assembly_index(path):
    """renderer name -> measured fields from a CarAssemblyAudit JSON."""
    data = load_json(path)
    out = {}
    for row in data.get("rows", []):
        mats = row.get("materials") or []
        first = mats[0] if mats else {}
        uv0 = first.get("uv0") or {}
        out[row.get("name", "")] = {
            "visible": row.get("visible"),
            "basisDeterminant": row.get("basisDeterminant"),
            "miscTwinHidden": row.get("miscTwinHidden"),
            "metresPerUv0": uv0.get("metresPerUv"),
            "uv0Count": uv0.get("count"),
            "uv2Present": (first.get("uv2") or {}).get("present"),
            "boundTexture": first.get("texture"),
        }
    return out


def density_note(width, metres_per_uv):
    if not width or not metres_per_uv:
        return None
    return round(width / metres_per_uv, 1)  # texels per metre at mip0


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--root", default=str(pathlib.Path(__file__).resolve().parent.parent))
    ap.add_argument("--car"); ap.add_argument("--index", type=int)
    ap.add_argument("--track")
    ap.add_argument("--assembly", help="assembly-audit JSON to join (optional)")
    ap.add_argument("--out-md"); ap.add_argument("--out-json")
    args = ap.parse_args()
    root = pathlib.Path(args.root)
    assets = root / "assets"
    car_map_path = assets / "car_map.json"
    if args.car and args.index is None:
        index = next((i for i, f in car_catalog(root).items() if f == args.car), None)
        if index is None:
            print(f"unknown car folder: {args.car}"); return 2
    elif args.index is not None:
        index = args.index
        args.car = car_catalog(root).get(index)
        if args.car is None:
            print(f"unknown car index: {args.index}"); return 2
    if args.car:
        folder = assets / "RealRacing" / "Source" / args.car
        if not folder.is_dir():
            print(f"missing car folder: {folder}"); return 3
        if not car_map_path.exists():
            print(f"missing {car_map_path}"); return 3
        car_map = load_json(car_map_path)
        target = f"res://assets/RealRacing/Source/{args.car}/"
        bindings = {k: v for k, v in reverse_bindings(car_map).items() if k.startswith(target)}
    elif args.track:
        index, target, bindings = None, None, {}
        folder = assets / "RealRacing" / "Source" / args.track
        if not folder.is_dir():
            print(f"missing track folder: {folder}"); return 3
    else:
        print("choose --car/--index/--track"); return 2

    repairs_path = root / "data" / "car_surface_uv_repairs.json"
    repairs = load_json(repairs_path) if repairs_path.exists() else {}
    assembly = assembly_index(args.assembly) if args.assembly else {}

    rows = []
    for png in sorted(folder.rglob("*.png")):
        rel = png.relative_to(assets).as_posix()
        info = png_info(png)
        imp = import_info(png.with_name(png.name + ".import"))
        bound = bindings.get("res://assets/" + rel, [])
        repair_rows = []
        for b in bound:
            key = f"{b['car']}/{b['renderer']}"
            if key in repairs:
                r = repairs[key]
                repair_rows.append({"key": key, "scale": r.get("scale"), "offset": r.get("offset"),
                                    "pending": r.get("pending", False)})
        # join measured assembly rows through the binding's renderer names
        joined = []
        for b in bound:
            m = assembly.get(b["renderer"])
            if m:
                joined.append({"renderer": b["renderer"], **m})
        rows.append({
            "file": rel, "pixels": f"{info['width']}x{info['height']}", **info,
            "import": imp, "boundTo": bound, "repairs": repair_rows, "assembly": joined,
            "density": [{"renderer": j["renderer"],
                         "pxPerMetre": density_note(info["width"], j["metresPerUv0"]),
                         "metresPerUv0": j["metresPerUv0"], "visible": j["visible"],
                         "basisDeterminant": j["basisDeterminant"]} for j in joined],
        })

    result = {
        "scope": "read-only texture inventory (no mutation, no rendering)",
        "target": args.car and f"car {args.car} (index {index})" or f"track {args.track}",
        "assemblySource": args.assembly, "rows": rows,
        "notes": [
            "orientation classes and defect columns live in the turn report; this tool emits measured facts only",
            "density pxPerMetre = textureWidth / metresPerUv0, valid only when --assembly supplies metresPerUv0",
        ],
    }
    if args.out_json:
        pathlib.Path(args.out_json).write_text(json.dumps(result, indent=1), encoding="utf-8")
    if args.out_md:
        lines = [f"# Texture inventory — {result['target']}", "",
                 "| file | px | bytes | RGBA | icc | import | bound to | repairs |", "|---|---|---|---|---|---|---|---|"]
        for r in rows:
            imp = r["import"]
            istr = (f"mode={imp.get('compressMode')} mip={imp.get('mipmaps')} "
                    f"limit={imp.get('sizeLimit')} vram={imp.get('vramTexture')}") if imp.get("present") else " NONE"
            bstr = "; ".join(f"car{b['car']}:{b['renderer']}[{b['slot']}]" for b in r["boundTo"]) or "-"
            rstr = "; ".join(f"{k}(scale={s},pending={p})" for k, s, p in
                             [(x["key"], x["scale"], x["pending"]) for x in r["repairs"]]) or "-"
            lines.append(f"| {r['file']} | {r['pixels']} | {r['bytes']} | {r['hasAlpha']} | "
                         f"{r['iccOrGamma'] or '-'} | {istr} | {bstr} | {rstr} |")
        if any(r["density"] for r in rows):
            lines += ["", "| renderer | visible | metresPerUv0 | px/m | det |", "|---|---|---|---|---|"]
            for r in rows:
                for d in r["density"]:
                    lines.append(f"| {d['renderer']} | {d['visible']} | {d['metresPerUv0']} | {d['pxPerMetre']} | {d['basisDeterminant']} |")
        pathlib.Path(args.out_md).write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"inventory rows={len(rows)} target={result['target']}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
