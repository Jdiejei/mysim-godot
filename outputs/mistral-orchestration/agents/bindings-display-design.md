# Bindings display design — step-1 doc (no code this turn)

Sources read fresh: src/ControlBindings.cs, Main.cs PauseUi region, Hud.cs layer facts.

Where: a BindingsPanel inside Hud's own canvas layer, visible only while Paused and only when the new setting
"show_bindings" is on (default on; toggled from session options). Right-aligned column (right-300..right-16,
top 16), grows down. The Main pause layer (20) sits above the HUD, so the panel reads as the dimmed reference
under the active RESUME/RESTART/MAIN MENU buttons - intentional hierarchy: actions primary, bindings reference.

Content:
- Title: CONTROLS (muted).
- One row per ControlBindings.Actions entry (10 actions, Throttle..ERS overtake (hold)): the action name muted
  on the left, the key name right-aligned showing OS.GetKeycodeString(ControlBindings.Get(i)) - live read, so
  a rebinding done in the controls dialog shows up without reopening pause.
- Footer: the fixed gamepad mapping as a text line: "GAMEPAD  stick steer, RT throttle, LT brake, A pit,
  Y camera, Start pause" (the strings are code facts; the gamepad map is not remappable this scope).

States shown: the 10 keyboard bindings live from ControlBindings.Get (persisted settings + defaults); the
touch/handheld case is covered by the screen-controls setting, not this panel.

Placement rules: right half of the screen, x from right-300 to right-16, clear of the centered pause column
(box at 540,250 with 200 width) and inside the safe margins at every supported canvas (1280x720 / 1280x800 /
1706x720).

Product-gate checks (step 2, next turn): paused shows the panel; every row equals OS.GetKeycodeString of the
live ControlBindings.Get; a settings remap reflects without reopening pause; the setting off hides it; the
panel stays inside the canvas and clear of the center pause column.
