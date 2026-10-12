#!/usr/bin/env python3
"""Ham RGBA dosyalarını PNG'ye çevirir.  kullanim: topng.py <genislik> <yukseklik> [dosya...]"""
import sys, glob
from PIL import Image

w, h = int(sys.argv[1]), int(sys.argv[2])
files = sys.argv[3:]
explicit = bool(files)
if not files:
    files = sorted(glob.glob('out/shots/*.raw'))
for f in files:
    if not explicit and 'sheet' in f:
        continue
    out = f[:-4] + '.png'
    Image.frombytes('RGBA', (w, h), open(f, 'rb').read()).convert('RGB').save(out)
    print('  ->', out)
