# Run Project (API + Web + Playwright check)

Start both servers as background processes, verify they are listening, and confirm Playwright MCP is reachable. Fix port conflicts automatically — no new terminal windows.

---

## CONFIGURE (update these for each project)

```
API_PORT=5088
API_START_CMD=cd "D:/My-Projects/amr-engineering-portfolio/apps/api/src/AmrPortfolio.Api" && dotnet run
API_READY_LOG=Now listening on: http://localhost:5088

WEB_PORT=3000
WEB_START_CMD=cd "D:/My-Projects/amr-engineering-portfolio" && npm run dev:web
WEB_READY_LOG=Ready in

SMOKE_URL=http://localhost:3000/en
```

---

## Steps (execute in order)

### 1. Check ports
Run a single PowerShell command to detect whether the configured API and web ports are already in use:
```powershell
$api = netstat -ano | Select-String ":<API_PORT> " | Select-String "LISTENING"
$web = netstat -ano | Select-String ":<WEB_PORT> " | Select-String "LISTENING"
"API :<API_PORT> in use: $($null -ne $api)"; "Web :<WEB_PORT> in use: $($null -ne $web)"
if ($api) { $api }; if ($web) { $web }
```

### 2. Kill occupying processes (if needed)
If either port is occupied, extract the PID from the `netstat` output and kill it:
```powershell
Stop-Process -Id <PID> -Force
```

### 3. Start API (background)
Run `API_START_CMD` with `run_in_background: true`. Wait for `API_READY_LOG` in the output.

### 4. Start Web (background)
Run `WEB_START_CMD` with `run_in_background: true`. Wait for `WEB_READY_LOG` in the output.

### 5. Verify both ports are listening
After both background tasks complete (or after ~5 s), recheck:
```powershell
$api = netstat -ano | Select-String ":<API_PORT> " | Select-String "LISTENING"
$web = netstat -ano | Select-String ":<WEB_PORT> " | Select-String "LISTENING"
"API :<API_PORT> listening: $($null -ne $api)"; "Web :<WEB_PORT> listening: $($null -ne $web)"
```

### 6. Smoke-test Playwright MCP
Call `mcp__playwright__browser_navigate` with `SMOKE_URL` and then `mcp__playwright__browser_snapshot` to confirm the page rendered. Report any console errors found.

---

## Output

Report concisely:
- API status (port + first log line)
- Web status (port + any warnings)
- Playwright status (reachable / not reachable)
- Any errors that need attention (broken imports, crash overlays, etc.)

Do **not** open new terminal windows or PowerShell processes — all work stays inside the Claude Code session.
