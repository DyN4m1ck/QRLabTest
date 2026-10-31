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
        for line in lines:
            s = line.strip()
            if s.startswith("//") and len(s) > 2:
                # дубликат блока комментариев: текущий блок уже встречался непосредственно перед вставкой
                if out and out[-1].strip() == s:
                    # начинаем новый блок? проверяем весь предыдущий блок на совпадение
                    cur = [line]
                    prev_block = []
                    j = len(out) - 1
                    while j >= 0 and out[j].strip().startswith("//"):
                        prev_block.insert(0, out[j]); j -= 1
                    # если последние k строк out совпадают со всем новым блоком — пропускаем
                    k = len(prev_block)
                    newblock_tail = None
                    # проще: сравниваем строку с той же позицией предыдущего блока -> обрабатываем блоками ниже
            out.append(line)
        # пересчитаем корректно: блочный алгоритм
        out = []
        i = 0
        removed_here = 0
        while i < len(lines):
            line = lines[i]
            s = line.strip()
            if s.startswith("//") and len(s) > 2:
                block = []
                while i < len(lines) and lines[i].strip().startswith("//") and len(lines[i].strip()) > 2:
                    block.append(lines[i]); i += 1
                # сравнить с предыдущим записанным блоком такого же размера
                prev = []
                j = len(out) - 1
                while j >= 0 and out[j].strip().startswith("//") and len(out[j].strip()) > 2:
                    prev.insert(0, out[j]); j -= 1
                if prev == block:
                    removed_here += len(block)
                    continue
                out.extend(block)
            else:
                out.append(line); i += 1
        if removed_here:
            with open(p, "w", encoding="utf-8") as f:
                f.write("\n".join(out) + "\n")
            total += removed_here
            print(f"{removed_here:3d} dup lines removed: {p}")
print("TOTAL:", total)
