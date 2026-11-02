"""Fetch pinned geographic inputs and verify the upstream source receipt. Stdlib only."""
import hashlib
import json
from pathlib import Path
import struct
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / "Assets/Geography/Kathmandu"
COMMIT = "e95b57c51e3a8e292415405e2d4b4577f3462d3b"
BASE = f"https://raw.githubusercontent.com/echo11labs/mandalaverse/{COMMIT}/"

def fetch(path):
    with urllib.request.urlopen(BASE + path, timeout=90) as response:
        return response.read()

def main():
    DEST.mkdir(parents=True, exist_ok=True)
    receipt = fetch("game/data/geography/source-receipt.json")
    expected = json.loads(receipt)["files"]
    files = ["kathmandu-source-height.f32", "kathmandu-ground-height.f32",
             "kathmandu-river-distance.f32", "kathmandu.json", "osm-kathmandu-waterways.geojson",
             "bagdwar-source.json"]
    for name in files:
        target = DEST / name
        data = target.read_bytes() if target.exists() else fetch("game/data/geography/" + name)
        digest = hashlib.sha256(data).hexdigest()
        if digest != expected[name]:
            raise ValueError(f"Checksum mismatch: {name}")
        if name.endswith(".f32"):
            if len(data) != 1025 * 1025 * 4:
                raise ValueError(f"Invalid raster dimensions: {name}")
            values = struct.unpack("<" + "f" * (len(data) // 4), data)
            if not all(-1e6 < value < 1e6 for value in values):
                raise ValueError(f"Invalid raster samples: {name}")
            print(f"{name}: verified; range {min(values):.2f} .. {max(values):.2f}")
        else:
            print(f"{name}: verified")
        target.write_bytes(data)
    (DEST / "source-receipt.json").write_bytes(receipt)
    for source, name in [
        ("game/data/geography/ATTRIBUTION.md", "ATTRIBUTION.md"),
        ("docs/GOPALA-Kathmandu-Geography.md", "UPSTREAM-GEOGRAPHY.md"),
        ("game/data/licenses/Copernicus-DEM-license.pdf", "Copernicus-DEM-license.pdf"),
    ]:
        (DEST / name).write_bytes(fetch(source))
    (DEST / "UPSTREAM.json").write_text(json.dumps({"repository": "https://github.com/echo11labs/mandalaverse",
        "commit": COMMIT, "data_path": "game/data/geography", "retrieved": "2026-11-02"}, indent=2))
    print("Pinned geographic inputs ready.")

if __name__ == "__main__":
    main()
