"""TRACKS visual-index generator (the trackside counterpart of the car cards).

Writes (default) outputs/mistral-orchestration/tracks-index/:
- TRACKS-INDEX.md: the 60-track table (name / map-layout / route point count) from
  data/TrackList.json, the POP list = the 10 largest surveyed routes (the only usage
  signal in the data; the criterion is stated, not asserted) + track 1 by name,
- per-POP-track files: the verified facts only - the route row, the floodlight
  tracking, and the visual-audit state. Tracks with no recorded visual defect data
  are marked 'not audited'; absent data is never invented. track0 is the only
  defect-audited track and links to its index.

Run:  py -3 tools/tracks_index.py --root <repo>   (from the workspace root)
"""
import json, argparse, pathlib

DEF_TXT = "no per-track visual defect data verified yet (no defect audit for this track)"
TRACK0_NOTE = ("track0's visual defects are tracked in "
               "outputs/mistral-orchestration/visual-review/track0-index/TRACK0-INDEX.md "
               "(the only track with a defect index)")


def tracks_index(root: pathlib.Path, out_dir: pathlib.Path):
    data = json.loads((root / "data" / "TrackList.json").read_text(encoding="utf-8"))["tracks"]
    if len(data) != 60:
        print(f"TRACKS index: expected 60 tracks, found {len(data)} - FAIL")
        return 2

    sized = [(i, tr, len(tr.get("centrePoints", []))) for i, tr in enumerate(data)]
    ranked = sorted(sized, key=lambda t: -t[2])
    pop = [i for i, _, _ in ranked[:10]]
    if 1 not in pop:
        pop.append(1)

    flood = json.loads((root / "data" / "track_floodlights.json").read_text(encoding="utf-8"))
    flood_keys = set()
    if isinstance(flood, dict):
        flood_keys = set(str(k) for k in flood.keys())
    elif isinstance(flood, list):
        for x in flood:
            if isinstance(x, dict):
                for k in ("track", "map", "index", "id"):
                    if k in x:
                        flood_keys.add(str(x[k]))

    out_dir.mkdir(parents=True, exist_ok=True)
    for i, tr, pts in sized:
        if i not in pop:
            continue
        sec = [f"# track {i} - {tr.get('name')} ({tr.get('map')} / {tr.get('layout')})",
               "",
               f"- scene: `{tr.get('scene')}`",
               f"- sourceSpline sha256: `{str(tr.get('sourceSha256'))[:12]}...`",
               f"- route: {pts} centrePoints (the largest surveyed routes stand in for play priority; the data has no play-count field - the criterion is stated, not measured)",
               f"- floodlights: {'tracked in data/track_floodlights.json' if (str(i) in flood_keys or str(tr.get('map')) in flood_keys) else 'not tracked in data/track_floodlights.json'}",
               "",
               f"Visual audit: {DEF_TXT if i != 0 else TRACK0_NOTE}",
               ""]
        (out_dir / f"track{i}.md").write_text("\n".join(sec) + "\n", encoding="utf-8")

    table = ["# TRACKS visual index (all 60, size-ranked)", "",
             "The POP list = the 10 largest surveyed routes (the data has no play-count "
             "field; the criterion is route size, stated). POP tracks have their own file: "
             + ", ".join(f"track{i}" for i in sorted(pop)), "",
             "| # | name | map/layout | points | pop | visual audit |", "|---|---|---|---|---|---|"]
    for i, tr, pts in ranked:
        tag = "POP" if i in pop else "-"
        cell = "track0 audited (see TRACK0-INDEX)" if i == 0 else DEF_TXT
        table.append(f"| {i} | {tr.get('name')} | {tr.get('map')}/{tr.get('layout')} | {pts} | {tag} | {cell} |")
    table += ["",
              "Defaults: visual defect DATA exists only for track0 (its index); tracks 1-59 "
              "have no recorded visual defect audit yet - absent data is never invented.",
              "",
              "The car-side counterpart: outputs/mistral-orchestration/cards/ (the per-car cards)."]
    (out_dir / "TRACKS-INDEX.md").write_text("\n".join(table) + "\n", encoding="utf-8")
    print(f"tracks index: pop={sorted(pop)} + the 60-row table -> {out_dir}")
    return 0


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--root", default=str(pathlib.Path(__file__).resolve().parent.parent))
    a = ap.parse_args()
    root = pathlib.Path(a.root)
    if not (root / "data" / "TrackList.json").exists():
        root = pathlib.Path(__file__).resolve().parent.parent
    out_dir = pathlib.Path("outputs/mistral-orchestration/tracks-index")
    return tracks_index(root, out_dir)


if __name__ == "__main__":
    raise SystemExit(main())
