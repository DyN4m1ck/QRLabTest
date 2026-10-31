import os

ROOT = "/workspace"
total = 0
for dirpath, dirs, files in os.walk(ROOT):
    if ".git" in dirpath: continue
    for fn in files:
        if not fn.endswith(".cs"): continue
        p = os.path.join(dirpath, fn)
        with open(p, encoding="utf-8") as f:
            lines = f.read().splitlines(keepends=True)
        out = []
        removed_here = 0
        for line in lines:
            if out and line.strip().startswith("//") and line == out[-1] and line.strip() != "//":
                removed_here += 1
                continue
            out.append(line)
        if removed_here:
            with open(p, "w", encoding="utf-8") as f:
                f.writelines(out)
            total += removed_here
            print(f"{removed_here:3d} dupes removed: {p}")
print("TOTAL:", total)
