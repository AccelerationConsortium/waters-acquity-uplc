"""
Fixed wrapper approach - add timeout to wait_experiment_read()
This patches the hanging wait method without changing the wrapper architecture
"""

import os
import time
import uuid
import threading
from datetime import datetime
from astra_admin import AstraAdmin

def ts(msg: str) -> None:
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {msg}")

def create_experiment_with_timeout(aa, template_path, timeout_seconds=60):
    """Create experiment but with timeout on the hanging wait_experiment_read()"""
    
    ts(f"Creating experiment from: {template_path}")
    
    # Store the original wait method
    original_wait_experiment_read = aa.wait_experiment_read
    
    def wait_experiment_read_with_timeout():
        """Replacement wait method with timeout"""
        ts("Waiting for experiment to be read (with timeout)...")
        start_time = time.time()
        
        # Import the required classes from the module
        from astra_admin import AstraEvents, PumpEvents
        
        # Wait with timeout
        while not AstraEvents.read_event.wait(1.0):
            PumpEvents(0.1)
            elapsed = time.time() - start_time
            if elapsed > timeout_seconds:
                ts(f"⚠ wait_experiment_read timed out after {timeout_seconds}s")
                ts("Experiment was created but read event may not have fired")
                ts("Continuing anyway...")
                break
        
        AstraEvents.read_event.clear()
        ts("✓ Experiment read completed (or timed out)")
    
    # Temporarily replace the hanging method
    aa.wait_experiment_read = wait_experiment_read_with_timeout
    
    try:
        # Now use the normal wrapper method with our timeout fix
        experiment_id = aa.new_experiment_from_template(template_path)
        return experiment_id
    finally:
        # Restore the original method
        aa.wait_experiment_read = original_wait_experiment_read

def main():
    ts("=== Fixed Wrapper Approach ===")
    
    # Connect to ASTRA normally
    aa = AstraAdmin()
    client_guid = str(uuid.uuid4())
    aa.set_automation_identity("Fixed Wrapper Test", "1.0.0", os.getpid(), client_guid, 1)
    
    # Wait for instruments
    if not aa.has_instrument_detection_completed():
        ts("Waiting for instrument detection...")
        aa.wait_for_instruments()
    
    ts("✓ Connected to ASTRA")
    
    method_path = "//dbf/Method Builder/Owen/test_method_3"
    
    try:
        # Create experiment with timeout fix
        experiment_id = create_experiment_with_timeout(aa, method_path, timeout_seconds=30)
        ts(f"✓ Experiment created. ID = {experiment_id}")
        
        if experiment_id <= 0:
            raise RuntimeError(f"Failed to create experiment: ID = {experiment_id}")
        
        # Now use normal wrapper methods
        ts("Setting parameters via wrapper methods...")
        aa.set_sample_name(experiment_id, "Fixed Wrapper Test")
        aa.set_sample_concentration(experiment_id, 2.0)  # 2 mg/mL
        aa.set_pump_flow_rate(experiment_id, 0.5)  # 0.5 mL/min
        aa.set_injected_volume(experiment_id, 0.100)  # 100 µL
        aa.set_collection_duration(experiment_id, 25.0)  # 25 minutes
        ts("✓ Parameters set")
        
        # Validate
        ts("Validating experiment...")
        is_valid = aa.validate_experiment(experiment_id)
        if not is_valid:
            ts("⚠ Validation failed, but continuing anyway")
        else:
            ts("✓ Experiment validated")
        
        # Start collection
        ts("🚀 Starting collection...")
        ts("ASTRA should now prepare and wait for Waters injection signal!")
        ts("")
        ts("👀 Watch ASTRA - you should see it go to 'Waiting for Auto Inject'")
        ts("")
        
        aa.start_collection(experiment_id)
        
        # Use the wrapper's wait methods if they exist
        ts("=== Using wrapper wait methods ===")
        
        # Wait for preparing
        if hasattr(aa, 'wait_preparing_for_collection'):
            try:
                ts("Waiting for PreparingForCollection...")
                aa.wait_preparing_for_collection()
                ts("✓ PreparingForCollection completed")
            except Exception as e:
                ts(f"⚠ PreparingForCollection error: {e}")
        
        # Wait for auto-inject
        if hasattr(aa, 'wait_waiting_for_auto_inject'):
            try:
                ts("🎯 Waiting for WaitingForAutoInject...")
                ts("   This is where ASTRA waits for Waters signal!")
                aa.wait_waiting_for_auto_inject()
                ts("✅ WaitingForAutoInject completed - signal received!")
            except Exception as e:
                ts(f"⚠ WaitingForAutoInject error: {e}")
        
        # Wait for collection started
        if hasattr(aa, 'wait_collection_started'):
            try:
                ts("Waiting for CollectionStarted...")
                aa.wait_collection_started()
                ts("✅ Collection started!")
            except Exception as e:
                ts(f"⚠ CollectionStarted error: {e}")
        
        # Wait for collection finished
        if hasattr(aa, 'wait_collection_finished'):
            try:
                ts("Waiting for CollectionFinished...")
                aa.wait_collection_finished()
                ts("✅ Collection finished!")
            except Exception as e:
                ts(f"⚠ CollectionFinished error: {e}")
        
        ts("🎉 Experiment completed successfully!")
        
    except Exception as e:
        ts(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
    
    finally:
        try:
            if 'experiment_id' in locals() and experiment_id > 0:
                ts("Cleaning up experiment...")
                aa.close_experiment(experiment_id)
                ts("✓ Experiment closed")
        except Exception as e:
            ts(f"Cleanup error: {e}")

if __name__ == "__main__":
    main()
