"""Download official ambientCG CC0 terrain PBR maps; retain provenance and byte hashes."""
from concurrent.futures import ThreadPoolExecutor
import hashlib
import io
import json
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / "Assets/Art/Environment/TerrainTextures/ambientCG"
CACHE = ROOT / "tools/.cache/terrain-textures"
ASSETS = {
    "Grass004": ("Meadow", 1.4), "Ground037": ("WoodlandFloor", 2.1),
    "Ground054": ("Alluvium", 3.5), "Ground048": ("WetMeadow", 1.4),
    "Rock030": ("ExposedRock", 3.0), "Grass003": ("DryGrass", 1.4),
}

def download(url):
    request = urllib.request.Request(url, headers={"User-Agent": "Dewabhumi-asset-import/1.0"})
    with urllib.request.urlopen(request, timeout=120) as response:
        return response.read(), response.url

def obtain(asset):
    role, metres = ASSETS[asset]
    filename = f"{asset}_2K-JPG.zip"
    url = f"https://ambientcg.com/get?file={filename}"
    archive = CACHE / filename
    archive.parent.mkdir(parents=True, exist_ok=True)
    if archive.exists():
        data = archive.read_bytes()
        final_url = url
    else:
        data, final_url = download(url)
        archive.write_bytes(data)
    folder = DEST / asset
    folder.mkdir(parents=True, exist_ok=True)
    retained = []
    with zipfile.ZipFile(io.BytesIO(data)) as package:
        if package.testzip() is not None:
            raise ValueError(f"Archive CRC check failed: {asset}")
        for member in package.infolist():
            name = Path(member.filename).name
            if not name.endswith(("_Color.jpg", "_NormalGL.jpg", "_Roughness.jpg", "_AmbientOcclusion.jpg", "_Displacement.jpg")):
                continue
            if member.file_size > 40_000_000:
                raise ValueError(f"Unexpected image size: {name}")
            image = package.read(member)
            (folder / name).write_bytes(image)
            retained.append({"path": f"{asset}/{name}", "sha256": hashlib.sha256(image).hexdigest(), "bytes": len(image)})
    if not any(f["path"].endswith("_Color.jpg") for f in retained) or not any(f["path"].endswith("_NormalGL.jpg") for f in retained):
        raise ValueError(f"Required maps missing: {asset}")
    print(f"{asset}: archive CRC verified, {len(retained)} PBR maps retained.", flush=True)
    return {"asset": asset, "role": role, "tileMetres": metres,
        "scaleNote": "Artist-selected scale" if asset == "Rock030" else "Provider approximate physical dimensions",
        "page": f"https://ambientcg.com/view?id={asset}", "license": "CC0-1.0",
        "archiveUrl": url, "resolvedUrl": final_url, "archiveSha256": hashlib.sha256(data).hexdigest(), "files": retained}

def main():
    DEST.mkdir(parents=True, exist_ok=True)
    with ThreadPoolExecutor(max_workers=3) as executor:
        records = list(executor.map(obtain, ASSETS))
    manifest = {"provider": "ambientCG / Lennart Demes", "retrieved": "2026-11-02",
        "license": "CC0-1.0", "licenseUrl": "https://docs.ambientcg.com/license/", "assets": records}
    (DEST / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    license_html, _ = download(manifest["licenseUrl"])
    (CACHE / "ambientCG-license.html").write_bytes(license_html)
    (DEST / "LICENSE.txt").write_text("ambientCG materials by Lennart Demes\nCreative Commons CC0 1.0 Universal\n"
        "https://docs.ambientcg.com/license/\nhttps://creativecommons.org/publicdomain/zero/1.0/\n\n"
        "Free to copy, modify and redistribute, including commercial games. Attribution is not required.\n"
        "Source pages, source archive hashes and retained map hashes are in manifest.json.\n"
        "These materials are artistic surface choices, not surveyed Kathmandu geology or period vegetation.\n", encoding="utf-8")
    print("All terrain texture sets ready.")

if __name__ == "__main__":
    main()
