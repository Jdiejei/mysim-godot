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


def audit_plan(root: pathlib.Path, out_dir: pathlib.Path):
    """The per-track VISUAL AUDIT EXECUTION plan: the batching + per-track evidence
    contracts, generated from the same data (route size = the measured priority
    signal). The audit outputs land in the LEDGER via the verification side; this
    plan defines what each audit captures and the contract its output must satisfy."""
    data = json.loads((root / "data" / "TrackList.json").read_text(encoding="utf-8"))["tracks"]
    if len(data) != 60:
        print(f"audit plan: expected 60 tracks, found {len(data)} - FAIL")
        return 2
    pts = {i: len(tr.get("centrePoints", [])) for i, tr in enumerate(data)}
    pop = [i for i, _ in sorted(pts.items(), key=lambda t: -t[1])[:10]]
    if 1 not in pop:
        pop.append(1)
    rest = [i for i in range(60) if i not in pop]
    by_pts = sorted(rest, key=lambda i: -pts[i])
    batch_b = by_pts[:20]
    batch_c = by_pts[20:]

    captures = [
        {"id": "D0-day-dry", "label": "day/night-matrix D0 row (the day0 baseline capture)",
         "rows": "baseline dry/day view set - FIRST (the D0-first matrix order)"},
        {"id": "N-night", "label": "night-light-track-<N>-wet-<W>[-candidate-<C>]",
         "rows": "the night/wet rows per the battery matrix (wet 0..3; candidate = the night-lighting candidate set where applicable)"},
        {"id": "COV", "label": "night-weather-coverage-track-<N>.json",
         "rows": "the coverage manifest row per track (profiles + the capture list)"},
    ]
    contract = {
        "captureSet": captures,
        "evidenceContract": [
            "the audit yields per-track: the capture rows (the label sets above) + the defect rows; nothing else",
            "each capture row = the same track/N + the SAME camera + the same TOD/wet row as its batch-mate (the matrix rows)",
            "the audit outputs land in the LEDGER via the verification side: the audit JSON + the defect rows fold into the LEDGER's track rows (the ledger owns the schema)",
            "no defect row may be emitted without the capture rows that back it (absent data never invented)",
            "the audit = per-track; no multi-track audit row",
        ],
    }
    plan = {
        "popBatchA": {"tracks": sorted(pop), "order": "first (the largest routes + track 1)"},
        "batchB": {"tracks": batch_b, "order": "second (the mid-size routes)"},
        "batchC": {"tracks": batch_c, "order": "third (the short ovals/tails)"},
        "captureSet": captures,
        "contract": contract,
    }
    (out_dir / "audit_contracts.json").write_text(json.dumps(plan, indent=1), encoding="utf-8")

    doc = ["# Per-track VISUAL AUDIT EXECUTION plan (the battery-attachable plan)", "",
           "Generated from the same data basis as TRACKS-INDEX (route size = the measured priority signal).",
           "The audit batches + per-track evidence contract; the audit outputs land in the LEDGER via the",
           "verification side (the LEDGER owns the row format; this plan defines the inputs).", "",
           "## Batching (the POP set first)",
           "",
           "- **Batch A (first)**: the tracks indexed " + ", ".join(str(i) for i in sorted(pop)),
           "  (the 10 largest routes + track 1; the same POP list the tracks-index uses)",
           "- **Batch B (second)**: " + ", ".join(str(i) for i in batch_b),
           "- **Batch C (third)**: " + ", ".join(str(i) for i in batch_c),
           "", "## The per-track capture set (every track, every batch)",
           ""]
    doc += [f"- **{c['id']}** - {c['label']}: {c['rows']}" for c in captures]
    doc += ["", "## The evidence contract (per-track audit output)", ""]
    doc += [f"- {r}" for r in contract["evidenceContract"]]
    doc += ["", "## Batch computation (the actual point counts)", ""]
    doc += ["| batch | tracks (id: points) |", "|---|---|",
            "| A | " + "; ".join(f"{i}:{pts[i]}" for i in sorted(pop)) + " |",
            "| B | " + "; ".join(f"{i}:{pts[i]}" for i in batch_b) + " |",
            "| C | " + "; ".join(f"{i}:{pts[i]}" for i in batch_c) + " |",
            "", "## Batch-A dispatch (the concrete battery commands)",
            "",
            "The batch-A dispatch spec = `BATCH-A-DISPATCH.md` + `batch_a_dispatch.json`",
            "(same directory): the existing capability path (one `game-review.yml`",
            "depth=weather_review dispatch = the full wet/night matrix; the POP D0 rows =",
            "the review-first subset), the proposed minimal two-line addition for a",
            "D0-ONLY quick item (the `weather_rows` input + the audit-tool loop reading it),",
            "the exact gh commands, the expected per-track D0 file names, and the ledger",
            "row templates. Verification owns both the workflow and the ledger; this spec",
            "is what they fold in.",
            "",
            "The plan computes from data/TrackList.json only; no other condition enters the",
            "batching (the wet/night SETS stay per-track rows; the batch assignment = the",
            "priority order only)."]
    (out_dir / "TRACK-AUDIT-PLAN.md").write_text("\n".join(doc) + "\n", encoding="utf-8")
    print(f"audit plan: A={sorted(pop)} B={len(batch_b)} C={len(batch_c)} -> {out_dir}")
    return 0


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--root", default=str(pathlib.Path(__file__).resolve().parent.parent))
    ap.add_argument("--audit-plan", action="store_true", help="emit the per-track audit batching + evidence contracts")
    a = ap.parse_args()
    root = pathlib.Path(a.root)
    if not (root / "data" / "TrackList.json").exists():
        root = pathlib.Path(__file__).resolve().parent.parent
    out_dir = pathlib.Path("outputs/mistral-orchestration/tracks-index")
    if a.audit_plan:
        return audit_plan(root, out_dir)
    return tracks_index(root, out_dir)


if __name__ == "__main__":
    raise SystemExit(main())
