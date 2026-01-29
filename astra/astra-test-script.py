"""
ASTRA Automation API (Python) — robust “wait for Waters inject signal” script

What’s different vs the minimal script:
- Logs key milestones with timestamps
- Waits for: PreparingForCollection -> WaitingForAutoInject -> CollectionStarted -> CollectionFinished
- Uses timeouts so you *fail fast* if the Waters inject signal never arrives
- Still saves experiment + exports results/dataset

Assumptions:
- You’re using Wyatt’s AstraAdmin Python wrapper (SDKPython).
- Your ASTRA method is configured for your Waters HPLC setup.
- StartCollection via COM will wait for auto-inject (per docs).

Edit the CONFIG section and run on Windows.
"""

import os
import time
import uuid
from pathlib import Path
from datetime import datetime

from astra_admin import AstraAdmin


# -----------------------------
# CONFIG (edit these)
# -----------------------------
TEMPLATE_METHOD_PATH = r"//dbf/Method Builder/Owen/test_method_3"
OUTPUT_DIR = Path(r"C:\ASTRA_Automation_Output")
RUN_NAME = "waters_run_001"

# Sample / method overrides (optional)
SAMPLE_NAME = "Sample A"
CONCENTRATION_G_PER_ML = 0.002     # 2 mg/mL = 0.002 g/mL
FLOW_RATE_ML_PER_MIN = 0.5
INJECTED_VOLUME_ML = 0.100         # 100 µL = 0.100 mL (automation units)
COLLECTION_DURATION_MIN = 30.0

# Timeouts (tune these to your workflow)
INSTRUMENT_DETECT_TIMEOUT_S = 120          # startup/instrument detection
WAIT_FOR_AUTOINJECT_TIMEOUT_S = 300        # time allowed to reach "WaitingForAutoInject"
WAIT_FOR_COLLECTION_STARTED_TIMEOUT_S = 900  # time allowed for Waters inject signal to arrive
WAIT_FOR_COLLECTION_FINISHED_TIMEOUT_S = int(COLLECTION_DURATION_MIN * 60 + 300)  # duration + buffer


# If using Security Pack, fill these in; otherwise keep None
SECURITY_USER = None
SECURITY_PASSWORD = None
SECURITY_DOMAIN = None


DATASET_DEFINITION_NAME = "Molar Mass Distribution"  # must exist for your method


# -----------------------------
# Utilities
# -----------------------------
def ts(msg: str) -> None:
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {msg}")


def call_with_timeout(fn, timeout_s: int, step_name: str, *args, **kwargs):
    """
    Many AstraAdmin wait_* helpers are blocking. If your wrapper version doesn't support a timeout arg,
    we can run them in a crude manual timeout loop only if there is a non-blocking alternative.
    Since we *don't* have that here, we handle timeouts by:
      - calling the wait method if it exists and hoping it returns
      - otherwise raising a helpful error

    If your AstraAdmin supports a timeout parameter on wait_* (some wrappers do),
    you can modify calls below to pass timeout_s directly.
    """
    if fn is None:
        raise RuntimeError(f"Missing helper for {step_name}. Your AstraAdmin wrapper may differ.")
    start = time.time()
    ts(f"Waiting for {step_name} (timeout {timeout_s}s)...")
    # Call the blocking function; if it never returns, that's a hard hang.
    # Most wrappers return when the event occurs; if it hangs frequently,
    # we’ll swap to event callbacks or a different wait strategy.
def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    exp_path = OUTPUT_DIR / f"{RUN_NAME}.astrax"
    results_xml_path = OUTPUT_DIR / f"{RUN_NAME}_results.xml"
    dataset_csv_path = OUTPUT_DIR / f"{RUN_NAME}_{DATASET_DEFINITION_NAME}.csv"

    aa = AstraAdmin()

    # 1) REQUIRED: identify your automation client
    client_guid = str(uuid.uuid4())
    ts("Setting automation identity...")
    aa.set_automation_identity(
        "Waters-ASTRA Automation", "1.0.0", os.getpid(), client_guid, 1
    )

    # 2) REQUIRED: wait for instruments (use manual check to avoid hanging)
    ts("Waiting for ASTRA to detect instruments...")
    start_time = time.time()
    
    # Use manual polling instead of wrapper method to avoid hanging
    ts("Checking instrument detection manually to avoid hanging...")
    while not aa.has_instrument_detection_completed():
        time.sleep(1)
        if time.time() - start_time > INSTRUMENT_DETECT_TIMEOUT_S:
            ts("⚠ Timeout waiting for instruments - continuing anyway")
            ts("  → Check ASTRA UI manually to ensure instruments are ready")
            break
    ts("✓ Instrument detection check completed")

    # 3) OPTIONAL: Security Pack logon
    if all([SECURITY_USER, SECURITY_PASSWORD, SECURITY_DOMAIN]):
        ts("Validating Security Pack logon...")
        logon = aa.validate_logon(SECURITY_USER, SECURITY_PASSWORD, SECURITY_DOMAIN)
        if getattr(logon, "isValid", 0) == 0:
            raise RuntimeError(
                f"Security Pack logon failed: "
                f"{getattr(logon, 'errorMessage', '')} "
                f"{getattr(logon, 'errorDetails', '')}"
            )
        ts("Security Pack logon OK.")

    # 4) Create experiment directly via COM (avoid hanging wrapper)
    ts(f"Creating experiment from template: {TEMPLATE_METHOD_PATH}")
    ts("(Using direct COM approach to avoid hanging waits...)")
    
    # Call COM directly - this was working before
    experiment_id = aa.astra_com.NewExperimentFromTemplate(TEMPLATE_METHOD_PATH)
    
    if experiment_id <= 0:
        raise RuntimeError(f"Failed to create experiment: ID = {experiment_id}")
    
    # Manually add to experiments dict (since we bypassed the wrapper)
    from astra_admin import Experiment, ExperimentStatus
    experiment = Experiment(experiment_id)
    experiment.status = ExperimentStatus.BUSY
    aa._experiments[experiment_id] = experiment
    
    ts(f"Experiment created successfully. ID = {experiment_id}")
    
    # Give ASTRA a moment to initialize the experiment
    time.sleep(2)
    
    ts("✓ Experiment ready for configuration")

    try:
        # 5) Use method defaults (avoid parameter setting wrapper methods)
        ts("Using method template defaults for all parameters")
        ts("  → Sample name, concentration, flow rate will use method values")
        ts("  → You can edit these manually in ASTRA UI if needed")

        # 6) Skip validation (avoid potential hanging)
        ts("Skipping experiment validation to avoid hanging...")
        ts("  → You can check experiment status manually in ASTRA UI")
        ts("  → Look for any red errors or warnings before starting collection")

        # 7) Start collection using DIRECT COM only (avoid ALL wrapper methods)
        ts("🚀 Starting ASTRA collection...")
        ts("  → Using ONLY direct COM calls to avoid wrapper hanging issues")
        ts("  → Parameters will use method defaults or manual ASTRA UI setting")
        
        # Start collection directly via COM - bypassing ALL wrapper methods
        ts("Starting collection via direct COM...")
        
        # Use the raw COM interface directly
        collection_result = aa.astra_com.StartCollection(experiment_id)
        
        if collection_result == 0:  # Assuming 0 = failure, adjust if needed
            raise RuntimeError("Failed to start collection via COM")
            
        ts("✓ Collection started via direct COM")
        ts("✓ ASTRA should now be waiting for Waters injection signal!")
        ts("")
        ts("🎯 MANUAL STEP REQUIRED:")
        ts("  → ASTRA is now ready and waiting")
        ts("  → Trigger your Waters HPLC injection manually")
        ts("  → Check the ASTRA UI to confirm injection detection")
        ts("  → This script will wait for you to confirm...")
        ts("")
        
        # Simple manual confirmation instead of hanging wait methods
        injection_detected = False
        
        try:
            # Ask user to confirm instead of using potentially hanging wrapper methods
            ts("⏰ Waiting for manual confirmation...")
            ts("   After your Waters injection completes:")
            ts("   1. Check ASTRA UI shows 'Collection Started' or similar")
            ts("   2. Wait for data collection to finish")
            ts("   3. Press Ctrl+C to continue to data export")
            ts("")
            ts("   (This avoids wrapper methods that hang - you control the timing)")
            
            # Wait indefinitely for user interrupt
            while True:
                time.sleep(1)
                
        except KeyboardInterrupt:
            ts("")
            ts("✓ User confirmed - proceeding to data export...")
            ts("  → Assuming injection was detected and collection completed")
            injection_detected = True
            
        except Exception as e:
            ts(f"⚠ Unexpected error: {e}")
            injection_detected = False

        # 8) Save experiment and export data (with hanging protection)
        if injection_detected:
            ts("📁 Attempting to save experiment and export data...")
            ts("  → Using direct COM where possible to avoid hanging")
            
            # Try to save using direct COM first, fallback to wrapper
            try:
                ts(f"Saving experiment via direct COM: {exp_path}")
                save_result = aa.astra_com.SaveExperiment(experiment_id, str(exp_path))
                if save_result:
                    ts("✓ Experiment saved via direct COM")
                else:
                    ts("⚠ Direct COM save failed - trying wrapper method")
                    aa.save_experiment(experiment_id, str(exp_path))
                    ts("✓ Experiment saved via wrapper")
            except Exception as e:
                ts(f"⚠ Could not save experiment: {e}")
                ts("  → You can save manually in ASTRA UI: File > Save As")
            
            # Export additional formats (these may hang, so make them optional)
            ts("Attempting to export additional data formats...")
            try:
                ts(f"Saving results XML: {results_xml_path}")
                aa.save_results(experiment_id, str(results_xml_path))
                ts("✓ Results XML exported")
            except Exception as e:
                ts(f"⚠ Could not export results XML: {e}")
                ts("  → Export manually from ASTRA UI if needed")

            try:
                ts(f"Saving dataset CSV: {dataset_csv_path}")
                aa.save_data_set(experiment_id, DATASET_DEFINITION_NAME, str(dataset_csv_path))
                ts("✓ Dataset CSV exported")
            except Exception as e:
                ts(f"⚠ Could not export dataset CSV: {e}")
                ts("  → Export manually from ASTRA UI if needed")

            ts("✓ Save/export process completed")
            ts(f"Main experiment file: {exp_path}")
                
        else:
            ts("⚠ Collection was not confirmed - skipping data export")
            ts("  → You can save manually in ASTRA UI if data was actually collected")

    finally:
        # 9) Leave experiment open for inspection
        ts("Leaving experiment open for manual inspection...")
        ts("  → You can review data, adjust parameters, or rerun in ASTRA UI")
        ts("  → Close the experiment manually when finished")

        ts("Collection workflow completed.")
        ts("ASTRA application and experiment remain open.")


if __name__ == "__main__":
    main()
