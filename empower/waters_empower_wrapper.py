"""
Python wrapper for Waters Empower C# tools with session management.
Provides a clean Python API for Waters Empower automation.
"""

import subprocess
import os
import re
import time
from typing import Dict, List, Optional, Union


class WatersEmpower:
    """Python wrapper for Waters Empower automation with session-like behavior."""
    
    def __init__(self, exe_directory: str = None):
        """
        Initialize the Waters Empower session manager.
        
        Args:
            exe_directory: Path to directory containing C# executables.
                          If None, uses current directory.
        """
        if exe_directory is None:
            exe_directory = os.path.dirname(os.path.abspath(__file__))
        
        self.exe_dir = exe_directory
        self.sample_set_creator = os.path.join(exe_directory, "SampleSetCreator.exe")
        self.sample_set_reader = os.path.join(exe_directory, "SampleSetReader.exe")
        self.sample_set_extractor = os.path.join(exe_directory, "SampleSetExtractor.exe")
        
        # Session state
        self.logged_in = False
        self.last_login_time = None
        
        # Verify executables exist
        self._verify_executables()
    
    def _verify_executables(self):
        """Verify that required C# executables exist."""
        missing = []
        
        if not os.path.exists(self.sample_set_creator):
            missing.append("SampleSetCreator.exe")
        if not os.path.exists(self.sample_set_reader):
            missing.append("SampleSetReader.exe")
        if not os.path.exists(self.sample_set_extractor):
            missing.append("SampleSetExtractor.exe")
        
        if missing:
            raise FileNotFoundError(f"Missing executables: {', '.join(missing)}")
    
    def login(self) -> Dict[str, Union[bool, str]]:
        """
        Login to Waters Empower.
        Note: Each operation handles login internally, this tracks session state.
        
        Returns:
            Dictionary with success status and message
        """
        try:
            # Test login by trying to get sample sets
            result = self._run_executable(self.sample_set_reader, [], timeout=30)
            
            if "LOGIN SUCCESSFUL" in result.get("output", ""):
                self.logged_in = True
                self.last_login_time = time.time()
                return {
                    "success": True,
                    "message": "Login successful - connection verified"
                }
            else:
                return {
                    "success": False,
                    "message": "Login failed - check credentials in secrets.ini"
                }
                
        except Exception as e:
            return {
                "success": False,
                "message": f"Login error: {str(e)}"
            }
    
    def get_sample_sets(self) -> Dict[str, Union[bool, List[str], str]]:
        """
        Get list of all available sample sets.
        
        Returns:
            Dictionary with success status, sample set list, and message
        """
        try:
            result = self._run_executable(self.sample_set_reader, [], timeout=30)
            
            if result["success"]:
                # Parse sample set names from output
                sample_sets = []
                lines = result["output"].split('\n')
                
                # Look for sample set names in the output
                for line in lines:
                    line = line.strip()
                    # Skip headers, status messages, etc.
                    if (line and 
                        not line.startswith('Waters') and 
                        not line.startswith('=') and
                        not line.startswith('Loading') and
                        not line.startswith('✅') and
                        not line.startswith('🎉') and
                        not line.startswith('Creating') and
                        not line.startswith('Attempting') and
                        not 'sample set methods' in line.lower()):
                        
                        # This might be a sample set name
                        if len(line) > 3 and not line.startswith('  '):
                            sample_sets.append(line)
                
                return {
                    "success": True,
                    "sample_sets": sample_sets,
                    "message": f"Found {len(sample_sets)} sample sets"
                }
            else:
                return {
                    "success": False,
                    "sample_sets": [],
                    "message": result.get("message", "Failed to get sample sets")
                }
                
        except Exception as e:
            return {
                "success": False,
                "sample_sets": [],
                "message": f"Error getting sample sets: {str(e)}"
            }
    
    def create_sample_set(self, sample_set_name: str, 
                         sample_name: str = "KC_Test_A4",
                         vial: str = "1:A,4",
                         injection_volume: str = "25.0",
                         runtime: str = "10.00") -> Dict[str, Union[bool, str, dict]]:
        """
        Create a new sample set based on the existing template.
        
        Args:
            sample_set_name: Name for the new sample set
            sample_name: Name for the sample (default: KC_Test_A4)
            vial: Vial position (default: 1:A,4)
            injection_volume: Injection volume in µL (default: 25.0)
            runtime: Runtime in minutes (default: 10.00)
            
        Returns:
            Dictionary with success status, details, and message
        """
        try:
            result = self._run_executable(
                self.sample_set_creator, 
                [sample_set_name], 
                timeout=60
            )
            
            if result["success"] and "NEW SAMPLE SET CREATED SUCCESSFULLY" in result["output"]:
                # Extract details from output
                details = self._parse_sample_set_details(result["output"], sample_set_name)
                
                return {
                    "success": True,
                    "message": f"Sample set '{sample_set_name}' created successfully",
                    "details": details
                }
            else:
                # Check for specific error conditions
                output = result.get("output", "")
                if "Error storing sample set" in output:
                    message = "Sample set created but not saved to database"
                elif "Login" in output and "❌" in output:
                    message = "Authentication failed"
                elif "Template sample set" in output and "❌" in output:
                    message = "Template sample set not found"
                else:
                    message = "Sample set creation failed"
                
                return {
                    "success": False,
                    "message": message,
                    "details": {}
                }
                
        except Exception as e:
            return {
                "success": False,
                "message": f"Error creating sample set: {str(e)}",
                "details": {}
            }
    
    def run_sample_set(self, sample_set_name: str = None) -> Dict[str, Union[bool, str]]:
        """
        Execute a sample set.
        
        Args:
            sample_set_name: Name of sample set to run (uses default if None)
            
        Returns:
            Dictionary with success status, execution status, and message
        """
        try:
            args = [sample_set_name] if sample_set_name else []
            result = self._run_executable(
                self.sample_set_extractor, 
                args, 
                timeout=120
            )
            
            output = result.get("output", "")
            
            if "ACTIVELY EXECUTING" in output:
                return {
                    "success": True,
                    "status": "running",
                    "message": "Sample set execution started successfully"
                }
            elif "INSTRUMENT IS CURRENTLY BUSY" in output:
                return {
                    "success": False,
                    "status": "busy",
                    "message": "Instrument is currently busy with another operation"
                }
            elif "EXECUTION COMPLETED" in output:
                return {
                    "success": True,
                    "status": "completed",
                    "message": "Sample set execution completed"
                }
            else:
                return {
                    "success": False,
                    "status": "error",
                    "message": "Sample set execution failed"
                }
                
        except Exception as e:
            return {
                "success": False,
                "status": "error",
                "message": f"Error running sample set: {str(e)}"
            }
    
    def get_status(self) -> Dict[str, Union[bool, str]]:
        """
        Get current instrument status.
        
        Returns:
            Dictionary with instrument status information
        """
        try:
            # Use the extractor to check status without running
            result = self._run_executable(
                self.sample_set_extractor, 
                ["--status"], 
                timeout=30
            )
            
            output = result.get("output", "")
            
            if "INSTRUMENT IS CURRENTLY BUSY" in output:
                return {
                    "success": True,
                    "busy": True,
                    "status": "busy",
                    "message": "Instrument is busy"
                }
            elif "INSTRUMENT IS IDLE" in output or "LOGIN SUCCESSFUL" in output:
                return {
                    "success": True,
                    "busy": False,
                    "status": "idle",
                    "message": "Instrument is idle"
                }
            else:
                return {
                    "success": True,
                    "busy": False,
                    "status": "unknown",
                    "message": "Status unknown"
                }
                
        except Exception as e:
            return {
                "success": False,
                "busy": False,
                "status": "error",
                "message": f"Error getting status: {str(e)}"
            }
    
    def logout(self) -> Dict[str, Union[bool, str]]:
        """
        Logout from Waters Empower.
        Note: COM objects cleanup automatically, this tracks session state.
        
        Returns:
            Dictionary with success status and message
        """
        self.logged_in = False
        self.last_login_time = None
        
        return {
            "success": True,
            "message": "Session ended - COM objects will cleanup automatically"
        }
    
    def _run_executable(self, exe_path: str, args: List[str], timeout: int = 60) -> Dict[str, Union[bool, str]]:
        """
        Run a C# executable and return parsed results.
        
        Args:
            exe_path: Path to executable
            args: Command line arguments
            timeout: Timeout in seconds
            
        Returns:
            Dictionary with success status and output
        """
        try:
            cmd = [exe_path] + args
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=timeout,
                cwd=self.exe_dir
            )
            
            return {
                "success": result.returncode == 0,
                "output": result.stdout,
                "error": result.stderr
            }
            
        except subprocess.TimeoutExpired:
            return {
                "success": False,
                "output": "",
                "error": f"Operation timed out after {timeout} seconds"
            }
        except Exception as e:
            return {
                "success": False,
                "output": "",
                "error": str(e)
            }
    
    def _parse_sample_set_details(self, output: str, sample_set_name: str) -> Dict[str, str]:
        """Parse sample set details from C# output."""
        details = {"sample_set": sample_set_name}
        
        # Extract details using regex
        patterns = {
            "sample_name": r"Sample: (.+?) in vial",
            "vial": r"in vial (.+?)$",
            "injection_volume": r"Injection Volume: (.+?)$",
            "runtime": r"Runtime: (.+?)$"
        }
        
        for key, pattern in patterns.items():
            match = re.search(pattern, output, re.MULTILINE)
            if match:
                details[key] = match.group(1).strip()
            else:
                details[key] = "Unknown"
        
        return details
    
    # Context manager support
    def __enter__(self):
        """Enter context manager - login automatically."""
        login_result = self.login()
        if not login_result["success"]:
            raise ConnectionError(f"Login failed: {login_result['message']}")
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Exit context manager - logout automatically."""
        self.logout()


def main():
    """Demo usage of the Waters Empower session manager."""
    try:
        print("Waters Empower Python Session Manager")
        print("====================================")
        
        # Method 1: Manual session management
        print("\n=== Manual Session Management ===")
        empower = WatersEmpower()
        
        # Login
        login_result = empower.login()
        print(f"Login: {login_result}")
        
        if login_result["success"]:
            # Get sample sets
            sample_sets = empower.get_sample_sets()
            print(f"Sample sets: {sample_sets}")
            
            # Create a new sample set
            create_result = empower.create_sample_set("Python_API_Test")
            print(f"Create: {create_result}")
            
            # Check status
            status = empower.get_status()
            print(f"Status: {status}")
            
            # Logout
            logout_result = empower.logout()
            print(f"Logout: {logout_result}")
        
        # Method 2: Context manager (automatic login/logout)
        print("\n=== Context Manager (Automatic) ===")
        with WatersEmpower() as empower:
            sample_sets = empower.get_sample_sets()
            print(f"Sample sets: {sample_sets}")
            
            create_result = empower.create_sample_set("Python_Context_Test")
            print(f"Create: {create_result}")
        # Auto-logout when exiting context
        
    except Exception as e:
        print(f"Error: {e}")


if __name__ == "__main__":
    main()
