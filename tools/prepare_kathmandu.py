"""Convert pinned geography metadata to a Unity JsonUtility-compatible schema."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Assets/Geography/Kathmandu"

def main():
    receipt = json.loads((DATA / "source-receipt.json").read_text())
    for name, checksum in receipt["files"].items():
        path = DATA / name
        if path.exists() and hashlib.sha256(path.read_bytes()).hexdigest() != checksum:
            raise ValueError(f"Input changed: {name}")
    source = json.loads((DATA / "kathmandu.json").read_text())
    rivers = []
    for river in source["rivers"]:
        hidden = any(str(river.get(tag, "")).lower() in ("true", "yes", "1", "culvert", "building_passage")
                     for tag in ("covered", "tunnel"))
        rivers.append({"id": river["id"], "name": river["name"], "kind": river["kind"], "hidden": hidden,
            "points": [{"x": p[0] * 100, "y": p[1] * 100 + 1200, "z": -p[2] * 100}
                       for p in river["points"]]})
    result = {"samples": source["grid_samples"], "extent": source["extent_m"],
        "sampleSpacing": source["sample_spacing_m"], "utmEasting": source["utm_center"][0],
        "utmNorthing": source["utm_center"][1], "rivers": rivers}
    (DATA / "unity-geography.json").write_text(json.dumps(result, separators=(",", ":")), encoding="utf-8")
    print(f"Prepared {len(rivers)} river reaches; east = +X, north = +Z, one Unity unit = one metre.")

if __name__ == "__main__":
    main()
