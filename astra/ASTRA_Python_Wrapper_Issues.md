# ASTRA Python Wrapper Hanging Issues - Technical Report

**Date:** January 12, 2026  
**Environment:** ASTRA 8.2.2.119, Python automation wrapper  
**Severity:** Critical - Blocks production automation workflows

## Problem Summary

The ASTRA Python automation wrapper (`astra_admin.py`) has severe reliability issues where multiple wrapper methods hang indefinitely, blocking automation workflows. This particularly affects critical Waters HPLC integration where reliable automation is essential.

## Environment Details

- **ASTRA Version:** 8.2.2.119
- **Python Wrapper:** AstraAdmin from SDKPython folder  
- **Python:** 3.x with comtypes>=1.1.0
- **Operating System:** Windows
- **Use Case:** Waters HPLC automated injection and data collection

## Specific Methods That Hang

### Critical Hanging Methods
1. **`aa.new_experiment_from_template()`** - Hangs after creating experiment
2. **`aa.collect_data_with_method_info_callback()`** - Hangs during collection setup
3. **`aa.wait_experiment_read()`** - Used internally by other methods, hangs
4. **Parameter setting methods** - All hang consistently:
   - `aa.set_sample_name()`
   - `aa.set_injected_volume()`
   - `aa.set_pump_flow_rate()`
   - `aa.set_collection_duration()`
5. **`aa.validate_experiment()`** - Sometimes hangs during validation
6. **`aa.wait_for_instruments()`** - Can hang during instrument detection
7. **Collection wait methods** - All event waiting methods:
   - `aa.wait_preparing_for_collection()`
   - `aa.wait_waiting_for_auto_inject()`
   - `aa.wait_collection_started()`
   - `aa.wait_collection_finished()`

### Methods That Work Reliably
- **Direct COM calls:** `aa.astra_com.NewExperimentFromTemplate()`
- **Direct COM calls:** `aa.astra_com.StartCollection()`  
- **Status checks:** `aa.has_instrument_detection_completed()`
- **Basic setup:** `aa.set_automation_identity()`
- **Authentication:** `aa.validate_logon()`

## Reproduction Case

The hanging occurs consistently when running typical automation workflows:

```python
from astra_admin import AstraAdmin

aa = AstraAdmin()
aa.set_automation_identity("test", "1.0", 1234, "guid", 1)

# This hangs indefinitely:
experiment_id = aa.new_experiment_from_template("//dbf/Method Builder/test")

# This also hangs:
aa.set_sample_name(experiment_id, "Test Sample")

# This hangs too:
aa.collect_data_with_method_info_callback(experiment_id, callback_fn)
```

## Current Workaround

We've had to bypass the wrapper entirely and use direct COM calls:

```python
# Direct COM bypasses hanging (works reliably):
experiment_id = aa.astra_com.NewExperimentFromTemplate(template_path)
collection_result = aa.astra_com.StartCollection(experiment_id)

# Manual event polling instead of wrapper wait methods
while not some_condition:
    time.sleep(1)
    # Check status manually
```

## Impact on Production

This issue forces users to either:
1. **Use unreliable wrapper methods** and deal with random hanging (unacceptable for production)
2. **Bypass the wrapper entirely** and use direct COM calls (losing wrapper benefits like error handling, event management)
3. **Implement complex manual workarounds** with timeouts and manual state polling

For Waters HPLC integration, this is particularly problematic because:
- Injection timing is critical and cannot be delayed by hanging methods
- Data collection workflows must be reliable for analytical results
- Manual intervention breaks automation benefits

## Technical Analysis

### Possible Root Causes
1. **Threading/Event Issues:** The wrapper uses `threading.Event` objects extensively - could be causing deadlocks
2. **COM Interface Problems:** Wrapper might not be properly handling COM object lifecycles
3. **Event Callback Issues:** The event-driven architecture may have race conditions
4. **Blocking Wait Implementation:** Wait methods appear to block indefinitely with no timeout support

### Architecture Issues
The wrapper implements an event-driven system with methods like:
```python
def wait_waiting_for_auto_inject(self):
    self._events.waiting_for_auto_inject.wait()  # No timeout!
```

These blocking waits with no timeout mechanism are prone to hanging if the expected event never fires or is missed.

## Questions for Wyatt Technical Support

1. **Known Issue Status:** Is this a known issue? Are there newer versions of the Python wrapper available?

2. **Threading Architecture:** The wrapper uses `threading.Event` objects extensively. Could this be causing deadlocks or race conditions?

3. **COM Interface Reliability:** Why do direct COM calls (`aa.astra_com.*`) work reliably while wrapper methods hang?

4. **Timeout Support:** Can wrapper methods accept timeout parameters? The current implementation has no timeout mechanism.

5. **Event System Design:** Is the event-driven architecture properly handling all edge cases where events might be missed?

6. **Alternative Automation Approach:** Is there a more reliable way to do Python automation with ASTRA?

7. **Debugging Tools:** Are there debugging tools or logging options to diagnose why wrapper methods hang?

## Request for Resolution

We need a reliable Python automation solution for Waters HPLC integration. Acceptable solutions would include:

1. **Fixed wrapper** that doesn't hang with proper timeout handling
2. **Official guidance** on direct COM usage patterns and best practices  
3. **Alternative automation approach** that provides reliable automation capabilities
4. **Hybrid approach documentation** showing which methods are safe vs. which should be avoided

The current wrapper makes production automation unreliable due to random hanging issues, which is unacceptable for analytical instrumentation workflows.

## Files and Examples

Our current workaround script demonstrates the issues and shows which direct COM calls work reliably. We can provide:

- Full reproduction script showing hanging methods
- Working direct COM implementation 
- Logs showing where hangs occur
- Performance comparison between wrapper vs. direct COM approaches

## Priority

**HIGH** - This blocks production automation workflows for Waters HPLC integration, affecting analytical laboratory operations.
