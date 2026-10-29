"""Run Godot (or any tool) fully hidden and low priority: no console window, no focus steal, never touches or closes
any existing window. All bulk tools use this."""
import subprocess, os
def run_hidden(cmd, env=None, timeout=600):
    si = subprocess.STARTUPINFO(); si.dwFlags |= subprocess.STARTF_USESHOWWINDOW; si.wShowWindow = 0  # SW_HIDE
    flags = subprocess.CREATE_NO_WINDOW | subprocess.BELOW_NORMAL_PRIORITY_CLASS
    return subprocess.run(cmd, env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=timeout, startupinfo=si, creationflags=flags)
