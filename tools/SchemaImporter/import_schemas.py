import os
import sys
import json
import argparse
import re
from datetime import datetime

def load_meta(schemas_dir, standard, release):
    meta_dict = {}
    try:
        for file in os.listdir(schemas_dir):
            if file.endswith("_meta.ts") and standard.lower() in file.lower() and release.lower() in file.lower():
                with open(os.path.join(schemas_dir, file), 'r', encoding='utf-8') as f:
                    content = f.read()
                    matches = re.finditer(r'\{([^\}]+)\}', content)
                    for m in matches:
                        obj_str = m.group(1)
                        name_m = re.search(r'name\s*:\s*[\'"`](.*?)[\'"`]', obj_str)
                        ver_m = re.search(r'version\s*:\s*[\'"`](.*?)[\'"`]', obj_str)
                        if name_m and ver_m:
                            meta_dict[ver_m.group(1).strip()] = name_m.group(1).strip()
    except Exception as e:
        print(f"Warning: could not load meta for {standard} {release}: {e}")
    return meta_dict

def map_element(el, pos, qualifiers):
    out = {
        "position": pos,
        "id": el.get("Id", ""),
        "name": el.get("Desc", ""),
        "required": bool(el.get("Required", False)),
    }
    if "DataType" in el: out["dataType"] = el["DataType"]
    if "MinLength" in el: out["minLength"] = el["MinLength"]
    if "MaxLength" in el: out["maxLength"] = el["MaxLength"]
    
    if "QualifierRef" in el:
        q_ref = el["QualifierRef"]
        if q_ref in qualifiers:
            out["codes"] = dict(sorted(qualifiers[q_ref].items()))
            
    if "Components" in el:
        out["components"] = [
            map_element(c, i + 1, qualifiers) for i, c in enumerate(el["Components"])
        ]
    return out

def map_structure(items):
    out = []
    for item in items:
        mapped = {}
        if "Id" in item: mapped["id"] = item["Id"]
        if "Min" in item: mapped["min"] = item["Min"]
        if "Max" in item: mapped["max"] = item["Max"]
        if "Loop" in item:
            mapped["loop"] = map_structure(item["Loop"])
        out.append(mapped)
    return out

def process_release(schemas_dir, output_dir, standard, release, commit, repo):
    base_dir = os.path.join(schemas_dir, standard.lower(), release)
    main_file = os.path.join(base_dir, f"{release}.json")
    vers_file = os.path.join(base_dir, f"{release}_versions.json")
    
    if not os.path.exists(main_file):
        raise FileNotFoundError(f"Missing main schema file: {main_file}")
    
    with open(main_file, 'r', encoding='utf-8') as f:
        main_data = json.load(f)
        
    vers_data = {}
    if os.path.exists(vers_file):
        with open(vers_file, 'r', encoding='utf-8') as f:
            vers_data = json.load(f)
            
    meta_dict = load_meta(schemas_dir, standard, release)
    
    qualifiers = main_data.get("Qualifiers", {})
    
    imported_at = os.environ.get("SOURCE_DATE_EPOCH")
    if imported_at:
        imported_at_str = datetime.utcfromtimestamp(int(imported_at)).strftime("%Y-%m-%dT%H:%M:%SZ")
    else:
        imported_at_str = "2026-07-15T00:00:00Z"
        
    result = {
        "standard": standard.upper(),
        "release": release,
        "sourceCommit": commit,
        "sourceRepository": repo,
        "importedAt": imported_at_str,
        "segments": {},
        "messages": {}
    }
    
    # Process Segments
    segments = main_data.get("Segments", {})
    for seg_id in sorted(segments.keys()):
        seg = segments[seg_id]
        result["segments"][seg_id] = {
            "name": seg.get("Desc", ""),
            "elements": [
                map_element(el, i + 1, qualifiers) for i, el in enumerate(seg.get("Elements", []))
            ]
        }
        
    # Process Messages
    doc_types = vers_data.get("DocumentTypes", {})
    for doc_key in sorted(doc_types.keys()):
        doc = doc_types[doc_key]
        doc_type_id = doc.get("DocumentType", "")
        # fallback to doc_key if doc_type_id is empty
        if not doc_type_id: doc_type_id = doc_key
        
        name = meta_dict.get(doc_type_id, doc_type_id)
        
        result["messages"][doc_type_id] = {
            "name": name,
            "structure": map_structure(doc.get("TransactionSet", []))
        }
        
    output_file = os.path.join(output_dir, f"{standard.lower()}_{release}.json")
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(result, f, indent=2, sort_keys=False) # Only segment/message keys are sorted
        
    return output_file, result

def main():
    parser = argparse.ArgumentParser(description="Import EDI schemas into NppEdiPlugin internal format.")
    parser.add_argument("--source", required=True, help="Upstream schemas path")
    parser.add_argument("--output", required=True, help="Output directory")
    parser.add_argument("--standard", required=True, choices=["x12", "edifact", "all"], help="Standard to process")
    parser.add_argument("--release", help="Optional release filter")
    
    args = parser.parse_args()
    
    source = args.source
    output = args.output
    standard = args.standard
    release_filter = args.release
    
    commit = "10d39d2495a1a8b5264bf35c311a1e07efa745e7"
    repo = "https://github.com/hellooops/vscode-edi-support"
    
    if not os.path.exists(output):
        os.makedirs(output)
        
    standards = ["x12", "edifact"] if standard == "all" else [standard]
    imported_at = os.environ.get("SOURCE_DATE_EPOCH")
    if imported_at:
        imported_at_str = datetime.utcfromtimestamp(int(imported_at)).strftime("%Y-%m-%dT%H:%M:%SZ")
    else:
        imported_at_str = "2026-07-15T00:00:00Z"
        
    manifest_path = os.path.join(output, "import-manifest.json")
    if os.path.exists(manifest_path):
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)
    else:
        manifest = {
            "importedAt": imported_at_str,
            "sourceRepository": repo,
            "sourceCommit": commit,
            "files": []
        }
        
    for std in standards:
        std_dir = os.path.join(source, std)
        if not os.path.exists(std_dir):
            continue
            
        releases = os.listdir(std_dir)
        for rel in sorted(releases):
            if release_filter and rel != release_filter:
                continue
            
            if not os.path.isdir(os.path.join(std_dir, rel)):
                continue
                
            try:
                out_file, data = process_release(source, output, std, rel, commit, repo)
                # Remove existing entry if re-importing
                manifest["files"] = [f for f in manifest["files"] if not (f["standard"] == std.upper() and f["release"] == rel)]
                manifest["files"].append({
                    "standard": std.upper(),
                    "release": rel,
                    "filename": os.path.basename(out_file),
                    "segments": len(data["segments"]),
                    "messages": len(data["messages"])
                })
                print(f"Successfully imported {std.upper()} {rel}")
            except Exception as e:
                print(f"Error processing {std.upper()} {rel}: {e}")
                
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)

if __name__ == "__main__":
    main()
