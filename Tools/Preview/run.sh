#!/usr/bin/env bash
# Kingdom 2D - Unity'siz doğrulama ve önizleme aracı
#
#   ./run.sh sim   [kare]    : oyunu başsız simüle eder (çökme / mantık kontrolü)
#   ./run.sh shots [kare]    : simülasyondan ekran görüntüleri üretir -> out/shots/*.png
#   ./run.sh sheet [grup]    : sprite sayfası üretir (king | chars | build | all)
#
# Gereksinim: mono-mcs  ve  python3 + Pillow
set -euo pipefail
cd "$(dirname "$0")"
ROOT="$(cd ../.. && pwd)"
OUT="out"
mkdir -p "$OUT/shots"

SRC=$(find "$ROOT/Assets" -name "*.cs" -not -path "*/Editor/*" | sort)

build_sim()  { mcs -target:exe -out:"$OUT/sim.exe"  FakeUnity.cs SimMain.cs $SRC -nowarn:0219; }
build_dump() { mcs -target:exe -out:"$OUT/dump.exe" FakeUnity.cs ArtDump.cs $SRC -nowarn:0219; }

case "${1:-sim}" in
  sim)
    build_sim
    ( cd "$OUT" && mono sim.exe "${2:-40000}" 0 )
    ;;
  shots)
    build_sim
    ( cd "$OUT" && mono sim.exe "${2:-41000}" 3 )
    python3 topng.py 960 540
    ;;
  sheet)
    build_dump
    ( cd "$OUT" && mono dump.exe "${2:-all}" ) | tee "$OUT/sheet.log"
    SZ=$(grep '^SHEET' "$OUT/sheet.log" | awk '{print $2}')
    python3 topng.py "${SZ%x*}" "${SZ#*x}" "$OUT/shots/sheet.raw"
    ;;
  *)
    echo "kullanim: ./run.sh [sim|shots|sheet] [arg]"; exit 1 ;;
esac
