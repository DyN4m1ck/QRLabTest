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
        for line in lines:
            s = line.strip()
            # если строка комментария уже есть среди последних N записанных строк — это дубликат вставки
            if s.startswith("//") and len(s) > 2:
                window = [o.strip() for o in out[-12:]]
                if s in window:
                    total += 1
                    continue
            out.append(line)
        if len(out) != len(lines):
            with open(p, "w", encoding="utf-8") as f:
                f.write("\n".join(out) + "\n")
            print(f"{len(lines)-len(out):3d} dup lines removed: {p}")
print("TOTAL:", total)
