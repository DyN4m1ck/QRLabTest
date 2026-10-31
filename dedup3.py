import os

ROOT = "/workspace"
total = 0
for dirpath, dirs, files in os.walk(ROOT):
    if ".git" in dirpath: continue
    for fn in files:
        if not fn.endswith(".cs"): continue
        p = os.path.join(dirpath, fn)
        with open(p, encoding="utf-8") as f:
            lines = [l.rstrip("\n") for l in f.readlines()]
        out = []
        removed_here = 0
        i = 0
        while i < len(lines):
            s = lines[i].strip()
            if s.startswith("//") and len(s) > 2:
                block = []
                while i < len(lines) and lines[i].strip().startswith("//") and len(lines[i].strip()) > 2:
                    block.append(lines[i]); i += 1
                n = len(block)
                # блок записан дважды подряд?
                if n % 2 == 0 and block[:n//2] == block[n//2:]:
                    block = block[:n//2]
                out.extend(block)
            else:
                out.append(lines[i]); i += 1
        if len(out) != len([l for l in lines]):
            removed_here = len(lines) - len(out)
            with open(p, "w", encoding="utf-8") as f:
                f.write("\n".join(out) + "\n")
            total += removed_here
            print(f"{removed_here:3d} dup lines removed: {p}")
print("TOTAL:", total)
