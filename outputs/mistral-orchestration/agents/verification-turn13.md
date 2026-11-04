# verification agent — turn 13 report (the workflow-parse bisect: root cause found + fixed + verified)

Method note: the tasking's push-based bisect rode my branch (the only one the automation dispatches); each bisect state was additionally verified by DIRECT dispatches with the owner credential (the workstation 403 is the Jdiejei identity; my owner-token path is admin and works — proven by every dispatch below). Green probes were CANCELLED immediately after the parse verdict to avoid wasting runner minutes; every cancel is recorded. No game acceptance claimed from any of it — parse validation only.

## The bisect (all states = a313492's workflow + the named changes; each state committed on mistral/verification and dispatched)

| Step | Content | Commit | Run | Verdict |
|---|---|---|---|---|
| baseline | a313492's workflow | a313492 | 41871263005/41868705898 etc. | known-good (executed runs exist) |
| step 1 | + change 1: driving_wall case_set option + DRIVING_VALIDATION_MANIFEST/IMPACT rewrite | b97c16f | 41907880155 (my dispatch) | **GREEN** — jobs present, run cancelled after the verdict |
| step 2 | + change 2: the shard-matrix paren fix | d0b6626 | 41908032898 | **GREEN** — cancelled |
| step 3a | + the COLLISION_OVERLAY env line ONLY (25 inputs) | 3bcfd06 | 41908499825 | **GREEN** — cancelled |
| step 3b | + the collision_overlay INPUT line (26 inputs) | bcf9c19→bcf3c19 | 41908612608 (automation's push-run: failure, 0 jobs) + my dispatch attempt: rejected WITH THE VERBATIM ERROR | **REJECTED — the root cause** |
| final | change 1+2 + a DEPTH-DRIVEN COLLISION_OVERLAY env line (no new input) | f1165e5 | 41908968766 | **GREEN** — cancelled; the workflow is clean |

## The root cause (GitHub's own words)

`HTTP 422: Invalid Argument - failed to parse workflow: you may only define up to 25 inputs for a workflow_dispatch event`

- The rejected construct: the `collision_overlay: {description: ..., type: boolean, default: false}` INPUT line — the workflow's **26th** workflow_dispatch input. GitHub enforces the limit AT FILE LOAD: the 26-input file fails to parse — that is the full mechanism behind run 41898934225's "workflow file issue" + 0 jobs + the commit-message fallback title, and it also explains why the failure appeared on a PUSH event (the parser cannot read the file's triggers, so the trigger filter cannot exclude the push).
- Why the earlier checks missed it: PyYAML validates YAML syntax (the file is valid YAML); the coordinator's expression parser validated expression semantics; NEITHER models GitHub's input-count rule. Real dispatches are the only oracle — exactly as the reviewer suspected.
- The three named changes: change 1 and change 2 (the shard-matrix paren fix, which carries the verified 10-depth mapping) are BOTH parse-clean on their own; NOTHING else differs (the a313492→c4fb876 workflow diff = exactly these changes; verified by a line-level difflib reconstruction whose final step matched c4fb876 byte-for-byte).

## The minimal valid form (delivered; no weakening)

```yaml
# the input line is REMOVED (back to 25 inputs); the env line drives the feature from the depth instead:
COLLISION_OVERLAY: ${{ inputs.depth == 'track_scene_audit' && '1' || '0' }}
```

- Function preserved: the collision-overlay evidence (track-render 40c7df5) renders on EVERY `track_scene_audit` dispatch — the only depth that consumes it. The input's opt-in default (false) became the depth's default behavior; if opt-out is ever needed, revisit AFTER an input slot is freed (retiring a legacy input breaks recorded dispatch commands — worse trade).
- The 10-depth shard mapping (change 2) is UNTOUCHED and separately proven green.
- The verified form sits at `mistral/verification` tip (f1165e5); the handoff doc's command 4 updated accordingly (no `-f collision_overlay=true` — that input no longer exists and would 422), and the battery's audit callout records the whole bisect.

## Handoff state after this turn

- The coordinator applies the same one-line substitution to the integration branch (c4fb876+), pushes so PR #1 picks it up, then merges (owner credentials) and runs the six-command battery — the windows command's expectations UNCHANGED: tmp=1/rename=2, all five scenarios, the win cases, the userpath profiles.
- Windows assertions: OPEN until a green Windows run at a beyond-check head. Crash-consistency: OPEN. Physics: UNACCEPTED (contact + force-driven evidence pending).
- Run budget note: five probe runs were created for this bisect (three cancelled within ~30 s, one pre-existing failure reused, one cancelled after verdict) — no game-work conclusions drawn from any of them.
