from glob import glob

for filename in glob("*.png"):
    l = []
    with open(filename, "rb") as f:
        while byte := f.read(1):
            l.append(byte)
    l = l[::-1]
    with open(filename, "wb") as f:
        for b in l:
            f.write(b)
