# BATCH_REPORT — settings-persistence + reference-visual cycle

**Rolling report (latest block at top). Authoritative handoff: REVIEW_PACKET.md / REVIEW_REQUEST.json.**

## Block: settings-persistence VERIFIED at `dfe0ed3` (run **41710482854**: SUCCESS)
- All five scenario boots green: legacy **7/7**, foreign **4/4**, corrupt **5/5**, malformed **4/4**, failures **8/8** — 28 assertions, `all_ok: true` (settings-persist-summary.json, artifact fetched to `outputs/repair-20261029/cloud/settings-persist-pass/`).
- Consolidated fix chain (each a distinct diagnosed change after a failing run): `b0fa604` (atomic rename, no direct write) → `18f69a1`/`9bed931` (fixture wired; compile errors: CS0117/CS1061/CS0122/CS0019/CS0266) → `dfe0ed3` (second-disk-read URI bug + WrıteReport arity + marker migration policy) → `c1701d9` (URI-vs-globalized read in three asserts; type-safe `Get` via `VariantType` gates; audit case-name sync) → **`dfe0ed3` PASS**.
- Failure history preserved: failed settings runs `41675023687`/`41674946556`@`18f69a1` (fixture compile), `41688779865`@`9bed931` (Node/fixture contract), persistent-side `41699253306`@`e1f6d21` (atomic-replace + URI reads + type coercion), `41703220126`/`41707159183` (URI reads + case-name sync + malformed type fallback).
- **Verification bar met on Linux runner only.** Windows-local atomicity/replacement behavior is a separate later requirement (extracts in REVIEW_PACKET) — not certified by Linux green.

## Block: reference visual evidence (run **41712623422** at `d5d42ce`, earlier **41700551316** at `91e6991`: success-but-empty because the unconditional settings exit killed the reference branch mid-audit — fixed by removing the stray exit)
- `reference_evidence` depth: car16/track0 seven fixed views + two live drive frames; the run at `d5d42ce` awaited at snapshot time.
- Camera: chase tightened (back len*1.5→1.28, up clamp 2.0–2.8, FOV 46→43; far 2.1x→1.85x) at `26c0f5a` — VERIFICATION = the reference frames from the reference run, not yet reviewed at snapshot time.

## Prior blocks (condensed)
- A2C fence candidate REJECTED (supervisor manual review + measured unchanged speckle); no further fence runs.
- Mesh 72/73 machine-screened only; manual review pending.
- 403 root cause: unset-`GH_TOKEN` shell ordering (self-inflicted); owner credential verified `admin`; no permission changes.
- Authoritative handoff: `outputs/mistral-orchestration/REVIEW_PACKET.md` (+ `REVIEW_REQUEST.json`); review verdicts: `VISUAL_REVIEW.md`, `SNAPSHOT_REVIEW.md`.
