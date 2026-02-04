# Waters Automation Portal Tests

This folder contains test scripts to validate automation portal functionality and troubleshoot issues.

## Available Tests

### `test_tray_status.py`
Comprehensive tray status testing script to validate status detection and movement operations.

**Purpose:** 
- Test actual tray status values returned by the Waters system
- Validate the status checking logic used in the orchestrator
- Help fix the problematic status check in `gpc_orchestrator.py`

**Features:**
- Initial status check
- Extract tray and monitor status changes
- Insert tray and monitor status changes  
- Analyze orchestrator logic against real status values
- Provide recommendations for fixing status checks

**Usage:**
```bash
cd tests
python test_tray_status.py
```

**Options:**
1. Full workflow test - Tests extract/insert with status monitoring
2. Quick status check - Just checks current status without movements

## Known Issues Being Investigated

### Orchestrator Status Logic Issue
The line in `gpc_orchestrator.py`:
```python
if 'NoDrawer' in drawer_status or 'NoTray' in drawer_status:
```

This logic is likely incorrect and needs to be validated against actual status values from the Waters system. The test script will help determine the correct status checking logic.

## Running Tests

1. Ensure the automation portal is connected and operational
2. Run the test script: `python test_tray_status.py`
3. Follow the prompts to test tray movements and status detection
4. Use the analysis output to fix the orchestrator logic

## Expected Outcomes

The test should reveal the actual status values returned by the Waters system, such as:
- `NoDrawerNoTray` - No tray present
- `DrawerAndTray` - Tray with sample plate present  
- `DrawerOnly` - Tray without sample plate
- Other possible status values

Based on these findings, update the orchestrator logic accordingly.
