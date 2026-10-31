#!/usr/bin/env python3
"""Удаёт строки /// <summary> и /// </summary> из .cs-файлов, оставляя текст комментария."""
import os, re

ROOT = "/workspace"
changed = []
for dirpath, dirs, files in os.walk(ROOT):
    if ".git" in dirpath:
        continue
    for fn in files:
        if not fn.endswith(".cs"):
            continue
        p = os.path.join(dirpath, fn)
        with open(p, encoding="utf-8") as f:
            lines = f.read().splitlines(keepends=True)
        out = []
        removed = False
        for line in lines:
            s = line.strip()
            if s == "/// <summary>" or s == "/// </summary>" or s == "<summary>" or s == "</summary>":
                removed = True
                continue
            out.append(line)
        # схлопнуть "/// <summary>текст</summary>" в одну строку "/// текст", если встретится
        fixed = []
        for line in out:
            m = re.match(r"^(\s*)/// <summary>(.*)</summary>\s*$", line)
            if m:
                inner = m.group(2).strip()
                line = f"{m.group(1)}/// {inner}\n" if inner else None
            if line is not None:
                fixed.append(line)
        if removed or fixed != lines:
            with open(p, "w", encoding="utf-8") as f:
                f.writelines(fixed)
            changed.append(p)
print("changed files:", len(changed))
for c in changed:
    print(c)
