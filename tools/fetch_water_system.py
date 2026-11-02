"""Fetch and inspect LazySakana's pinned water package; safely admit only checked paths."""
import argparse
import hashlib
import io
import json
from pathlib import Path, PurePosixPath
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / "tools/.cache/lazysakana-water"
URL = "https://github.com/SakanaLazy/Water_Material_Experimentation/releases/download/v1.0.0/WaterSystem_Complete.unitypackage"
EXPECTED = "6f328f4cc2121157246e04dc6c105cc31bbb27ad7e87b3e6a72dff270f948336"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--admit", action="store_true")
    args = parser.parse_args()
    CACHE.mkdir(parents=True, exist_ok=True)
    package = CACHE / "WaterSystem_Complete.unitypackage"
    if not package.exists():
        with urllib.request.urlopen(URL, timeout=90) as response:
            package.write_bytes(response.read())
    if hashlib.sha256(package.read_bytes()).hexdigest() != EXPECTED:
        raise ValueError("Release checksum mismatch")
    staged = CACHE / "inspected"
    records = []
    with tarfile.open(package, "r:gz") as archive:
        members = {m.name: m for m in archive.getmembers() if m.isfile()}
        for name, member in members.items():
            if not name.endswith("/pathname"):
                continue
            path = archive.extractfile(member).read().decode("utf-8").strip()
            parts = PurePosixPath(path).parts
            if not parts or parts[0] != "Assets" or ".." in parts or ":" in path:
                raise ValueError(f"Unsafe package path: {path}")
            guid = name.split("/")[0]
            for filename, suffix in [("asset", ""), ("asset.meta", ".meta")]:
                item = members.get(guid + "/" + filename)
                if item is None:
                    continue
                data = archive.extractfile(item).read()
                target = ROOT / (path + suffix)
                if target.exists() and target.read_bytes() != data:
                    raise ValueError(f"Existing project asset would be overwritten: {path + suffix}")
                copy = staged / (path + suffix)
                copy.parent.mkdir(parents=True, exist_ok=True)
                copy.write_bytes(data)
                records.append({"path": path + suffix, "sha256": hashlib.sha256(data).hexdigest(), "bytes": len(data)})
    manifest = {"provider": "LazySakana", "version": "1.0.0", "release": URL,
        "sha256": EXPECTED, "license": "Custom Water System Attribution Licence",
        "source": "https://github.com/SakanaLazy/Water_Material_Experimentation", "files": records}
    (CACHE / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(f"Verified release SHA-256; staged {len(records)} files.")
    for record in records:
        if record["path"].endswith((".cs", ".mat", "LICENSE.md", "README.md")):
            print(record["path"])
    if args.admit:
        # Validate every destination before writing any file; preserve package GUIDs.
        for record in records:
            source = staged / record["path"]
            target = ROOT / record["path"]
            if hashlib.sha256(source.read_bytes()).hexdigest() != record["sha256"]:
                raise ValueError("Staged file changed")
            if target.exists() and target.read_bytes() != source.read_bytes():
                raise ValueError(f"Destination changed: {target}")
        for record in records:
            target = ROOT / record["path"]
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes((staged / record["path"]).read_bytes())
        receipts = ROOT / "docs/third_party/LazySakanaWater"
        receipts.mkdir(parents=True, exist_ok=True)
        (receipts / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
        print("Water System admitted with original asset GUIDs; no project settings changed.")

if __name__ == "__main__":
    main()
