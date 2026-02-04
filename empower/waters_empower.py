import subprocess
import json
from pathlib import Path

class WatersEmpower:
    def __init__(self, exe_path=None):
        if exe_path is None:
            # Default to the same directory as this Python file
            self.exe_path = Path(__file__).parent
        else:
            self.exe_path = Path(exe_path)
    
    def is_ready(self):
        """Returns True if instrument ready for next experiment, False if busy/error"""
        result = subprocess.run([self.exe_path / "SampleSetExtractor.exe", "--status-only"], 
                              capture_output=True, text=True)
        return result.returncode == 0
    
    def get_detailed_status(self):
        """Returns detailed instrument status information"""
        result = subprocess.run([self.exe_path / "SampleSetExtractor.exe", "--status-only"], 
                              capture_output=True, text=True)
        
        status = {
            "ready": result.returncode == 0,
            "raw_output": result.stdout,
            "error_output": result.stderr
        }
        
        # Parse status details from output
        for line in result.stdout.split('\n'):
            if "State:" in line:
                status["state"] = line.split("State:", 1)[1].strip()
            elif "SystemState:" in line:
                status["system_state"] = line.split("SystemState:", 1)[1].strip()
            elif "Current Vial:" in line:
                status["current_vial"] = line.split("Current Vial:", 1)[1].strip()
            elif "Injection:" in line:
                status["injection"] = line.split("Injection:", 1)[1].strip()
            elif "Run Time:" in line:
                status["run_time"] = line.split("Run Time:", 1)[1].strip()
            elif "Active Sample Set:" in line:
                status["active_sample_set"] = line.split("Active Sample Set:", 1)[1].strip()
        
        return status
    
    def list_sample_sets(self):
        """Returns list of all sample set names"""
        result = subprocess.run([self.exe_path / "SampleSetReader.exe", "--list-all"], 
                              capture_output=True, text=True)
        if result.returncode != 0:
            return []
        
        # Parse numbered list output
        sample_sets = []
        for line in result.stdout.split('\n'):
            if '. ' in line and line.strip().split('. ', 1):
                try:
                    sample_sets.append(line.strip().split('. ', 1)[1])
                except IndexError:
                    pass
        return sample_sets
    
    def read_sample_set(self, name):
        """Returns sample set details dict"""
        result = subprocess.run([self.exe_path / "SampleSetReader.exe", "--name", name], 
                              capture_output=True, text=True)
        if result.returncode != 0:
            return None
        
        # Parse output for key details
        details = {"name": name, "lines": []}
        current_line = {}
        
        for line in result.stdout.split('\n'):
            if "--- Sample Line" in line:
                if current_line:
                    details["lines"].append(current_line)
                current_line = {}
            elif ": " in line and not line.startswith("✅"):
                key, value = line.split(": ", 1)
                current_line[key.strip()] = value.strip()
        
        if current_line:
            details["lines"].append(current_line)
        
        return details
    
    def create_sample_set(self, name, template=None, **kwargs):
        """Create new sample set. Returns True on success, False on failure"""
        cmd = [self.exe_path / "SampleSetCreator.exe", "--name", name]
        if template:
            cmd.extend(["--template", template])
        for key, value in kwargs.items():
            # Ensure arguments with special characters are properly handled
            cmd.extend([f"--{key.replace('_', '-')}", str(value)])
        
        # Debug: Print the exact command being run
        print(f"DEBUG: Running command: {cmd}")
        print(f"DEBUG: Command as string: {' '.join(str(x) for x in cmd)}")
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        # Debug: Print subprocess output
        print(f"DEBUG: Return code: {result.returncode}")
        print(f"DEBUG: STDOUT: {result.stdout}")
        print(f"DEBUG: STDERR: {result.stderr}")
        
        return result.returncode == 0
    
    def execute_sample_set(self, name):
        """Execute sample set and return full status info"""
        result = subprocess.run([self.exe_path / "SampleSetExtractor.exe", name], 
                               capture_output=True, text=True)
        
        # Parse status from output
        status_info = {
            "return_code": result.returncode,
            "stdout": result.stdout,
            "stderr": result.stderr
        }
        
        # Extract key status information from output
        for line in result.stdout.split('\n'):
            if "Current State:" in line:
                status_info["state"] = line.split("Current State:", 1)[1].strip()
            elif "INSTRUMENT IS CURRENTLY BUSY" in line:
                status_info["ready"] = False
                status_info["status"] = "busy"
            elif "Instrument is idle and ready" in line:
                status_info["ready"] = True
                status_info["status"] = "idle"
            elif "Run() method succeeded" in line:
                status_info["execution_started"] = True
            elif "Method '" in line and "not found" in line:
                status_info["status"] = "not_found"
        
        return status_info
