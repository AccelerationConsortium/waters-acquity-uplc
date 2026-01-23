"""
Waters Empower Toolkit Python Wrapper
=====================================

A Python interface for Waters Empower Toolkit COM objects based on official documentation.
Uses subprocess to call working C# executables to handle 32-bit COM object requirements.
"""

import subprocess
import json
import time
import configparser
import os
from typing import List, Dict, Optional, Any
from dataclasses import dataclass


@dataclass
class ConnectionStatus:
    """Represents connection status information"""
    done: bool
    text: str
    error_code: int = 0


class EmpowerProject:
    """Wrapper for MillenniumToolkit.Project COM object"""
    
    def __init__(self):
        self._project = None
        self._connected = False
    
    def create(self):
        """Create Project COM object"""
        try:
            self._project = win32com.client.Dispatch("MillenniumToolkit.Project")
            return True
        except Exception as e:
            raise RuntimeError(f"Failed to create Project object: {e}")
    
    def login(self, database: str = "", project: str = "Waters GPC Training", 
              username: str = "system", password: str = "manager"):
        """Login to Empower project"""
        if not self._project:
            raise RuntimeError("Project object not created. Call create() first.")
        
        try:
            self._project.Login(database, project, username, password)
            self._connected = True
            return True
        except Exception as e:
            raise RuntimeError(f"Login failed: {e}")
    
    def logoff(self):
        """Logoff from Empower project"""
        if self._project and self._connected:
            try:
                self._project.Logoff()
                self._connected = False
                return True
            except Exception as e:
                raise RuntimeError(f"Logoff failed: {e}")
        return False
    
    def get_error_description(self, error_code: int) -> str:
        """Get detailed error description for error code"""
        if not self._project:
            return "Project object not available"
        
        try:
            return self._project.TkErrorDescription(error_code)
        except Exception:
            return f"Unknown error code: {error_code}"
    
    @property
    def projects(self) -> List[str]:
        """Get list of available projects"""
        if not self._project:
            return []
        
        try:
            return list(self._project.Projects)
        except Exception:
            return []
    
    @property
    def services(self) -> List[str]:
        """Get list of available services"""
        if not self._project:
            return []
        
        try:
            return list(self._project.Services)
        except Exception:
            return []
    
    @property
    def is_connected(self) -> bool:
        """Check if connected to project"""
        return self._connected
    
    def __enter__(self):
        """Context manager entry"""
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit with cleanup"""
        self.logoff()


class EmpowerInstrument:
    """Wrapper for MillenniumToolkit.Instrument COM object"""
    
    def __init__(self):
        self._instrument = None
        self._connected = False
    
    def create(self):
        """Create Instrument COM object"""
        try:
            self._instrument = win32com.client.Dispatch("MillenniumToolkit.Instrument")
            return True
        except Exception as e:
            raise RuntimeError(f"Failed to create Instrument object: {e}")
    
    @property
    def systems(self) -> List[str]:
        """Get available systems"""
        if not self._instrument:
            return []
        
        try:
            systems = self._instrument.Systems
            return list(systems) if systems else []
        except Exception:
            return []
    
    @property
    def acquisition_servers(self) -> List[str]:
        """Get available acquisition servers (nodes)"""
        if not self._instrument:
            return []
        
        try:
            servers = self._instrument.AcqServers
            return list(servers) if servers else []
        except Exception:
            return []
    
    def connect(self, node_name: str, system_name: str, timeout: int = 30) -> bool:
        """
        Connect to instrument system
        
        Args:
            node_name: Name of acquisition server/node
            system_name: Name of instrument system
            timeout: Connection timeout in seconds
        """
        if not self._instrument:
            raise RuntimeError("Instrument object not created. Call create() first.")
        
        try:
            # Official pattern: Connect(nodeName, systemName) - node first!
            self._instrument.Connect(node_name, system_name)
            
            # Wait for connection to complete
            start_time = time.time()
            while time.time() - start_time < timeout:
                status = self.get_connection_status()
                if status.done:
                    if status.text == "Successfully connected to instrument server" or not status.text:
                        self._connected = True
                        return True
                    else:
                        raise RuntimeError(f"Connection failed: {status.text}")
                time.sleep(1)
            
            raise TimeoutError(f"Connection timeout after {timeout} seconds")
            
        except Exception as e:
            raise RuntimeError(f"Connection failed: {e}")
    
    def disconnect(self):
        """Disconnect from instrument"""
        if self._instrument and self._connected:
            try:
                self._instrument.Disconnect()
                self._connected = False
                return True
            except Exception as e:
                raise RuntimeError(f"Disconnect failed: {e}")
        return False
    
    def get_connection_status(self) -> ConnectionStatus:
        """Get current connection status"""
        if not self._instrument:
            return ConnectionStatus(done=True, text="No instrument object", error_code=-1)
        
        try:
            status = self._instrument.ConnectionStatus
            return ConnectionStatus(
                done=status.Done,
                text=status.Text,
                error_code=getattr(status, 'ErrorCode', 0)
            )
        except Exception:
            return ConnectionStatus(done=True, text="Status unavailable", error_code=-1)
    
    @property
    def is_connected(self) -> bool:
        """Check if connected to instrument"""
        if not self._instrument:
            return False
        
        try:
            return self._instrument.IsConnected
        except Exception:
            return self._connected
    
    @property
    def sample_set_methods(self) -> List[str]:
        """Get available sample set methods"""
        if not self._instrument:
            return []
        
        try:
            methods = self._instrument.SampleSetMethods
            return list(methods) if methods else []
        except Exception:
            return []
    
    def replace_sample_set(self, method_name: str):
        """Execute sample set with specified method"""
        if not self._instrument:
            raise RuntimeError("Instrument object not created")
        
        if not self._connected:
            raise RuntimeError("Not connected to instrument")
        
        try:
            self._instrument.Replace(method_name)
            return True
        except Exception as e:
            raise RuntimeError(f"Sample set execution failed: {e}")
    
    def stop(self):
        """Stop current operation"""
        if self._instrument and self._connected:
            try:
                self._instrument.Stop()
                return True
            except Exception as e:
                raise RuntimeError(f"Stop failed: {e}")
        return False
    
    def pause(self):
        """Pause current operation"""
        if self._instrument and self._connected:
            try:
                self._instrument.Pause()
                return True
            except Exception as e:
                raise RuntimeError(f"Pause failed: {e}")
        return False
    
    def resume(self):
        """Resume paused operation"""
        if self._instrument and self._connected:
            try:
                self._instrument.Resume()
                return True
            except Exception as e:
                raise RuntimeError(f"Resume failed: {e}")
        return False
    
    def queue_sample_set(self, sample_set_name: str):
        """Add sample set to queue"""
        if not self._instrument or not self._connected:
            raise RuntimeError("Instrument not connected")
        
        try:
            self._instrument.QueueSampleSet(sample_set_name)
            return True
        except Exception as e:
            raise RuntimeError(f"Queue operation failed: {e}")
    
    def start_queue(self):
        """Start queue processing"""
        if self._instrument and self._connected:
            try:
                self._instrument.StartQueue()
                return True
            except Exception as e:
                raise RuntimeError(f"Start queue failed: {e}")
        return False
    
    def stop_queue(self):
        """Stop queue processing"""
        if self._instrument and self._connected:
            try:
                self._instrument.StopQueue()
                return True
            except Exception as e:
                raise RuntimeError(f"Stop queue failed: {e}")
        return False
    
    def clear_queue(self):
        """Clear all queued items"""
        if self._instrument and self._connected:
            try:
                self._instrument.ClearQueue()
                return True
            except Exception as e:
                raise RuntimeError(f"Clear queue failed: {e}")
        return False
    
    @property
    def status(self) -> str:
        """Get current instrument status"""
        if not self._instrument or not self._connected:
            return "Not connected"
        
        try:
            return str(self._instrument.Status)
        except Exception:
            return "Status unavailable"
    
    @property
    def progress(self) -> str:
        """Get progress information for current operation"""
        if not self._instrument or not self._connected:
            return "Not connected"
        
        try:
            return str(self._instrument.Progress)
        except Exception:
            return "Progress unavailable"
    
    def __enter__(self):
        """Context manager entry"""
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit with cleanup"""
        self.disconnect()


class EmpowerSampleSetMethod:
    """Wrapper for MillenniumToolkit.SampleSetMethod COM object"""
    
    def __init__(self):
        self._sample_set_method = None
    
    def create(self):
        """Create SampleSetMethod COM object"""
        try:
            self._sample_set_method = win32com.client.Dispatch("MillenniumToolkit.SampleSetMethod")
            return True
        except Exception as e:
            raise RuntimeError(f"Failed to create SampleSetMethod object: {e}")
    
    @property
    def method_names(self) -> List[str]:
        """Get available sample set method names"""
        if not self._sample_set_method:
            return []
        
        try:
            methods = self._sample_set_method.SampleSetMethodNames
            return list(methods) if methods else []
        except Exception:
            return []
    
    def load(self, method_name: str):
        """Load specific method"""
        if not self._sample_set_method:
            raise RuntimeError("SampleSetMethod object not created")
        
        try:
            self._sample_set_method.Load(method_name)
            return True
        except Exception as e:
            raise RuntimeError(f"Load method failed: {e}")
    
    def save(self, method_name: str):
        """Save method"""
        if not self._sample_set_method:
            raise RuntimeError("SampleSetMethod object not created")
        
        try:
            self._sample_set_method.Save(method_name)
            return True
        except Exception as e:
            raise RuntimeError(f"Save method failed: {e}")
    
    def delete(self, method_name: str):
        """Delete method"""
        if not self._sample_set_method:
            raise RuntimeError("SampleSetMethod object not created")
        
        try:
            self._sample_set_method.Delete(method_name)
            return True
        except Exception as e:
            raise RuntimeError(f"Delete method failed: {e}")


class EmpowerToolkit:
    """Main wrapper class for Waters Empower Toolkit"""
    
    def __init__(self, config_file: str = "secrets.ini"):
        self.config_file = config_file
        self.config = self._load_config()
        self.project = EmpowerProject()
        self.instrument = EmpowerInstrument()
        self.sample_set_method = EmpowerSampleSetMethod()
    
    def _load_config(self) -> Dict[str, str]:
        """Load configuration from file"""
        config = {}
        
        if not os.path.exists(self.config_file):
            return {
                'username': 'system',
                'password': 'manager',
                'database': '',
                'project': 'Waters GPC Training',
                'system': 'Arc HPLC',
                'node': 'Waters-h4q6k34'
            }
        
        parser = configparser.ConfigParser()
        parser.read(self.config_file)
        
        # Handle both sectioned and non-sectioned config files
        if 'DEFAULT' in parser:
            config = dict(parser['DEFAULT'])
        else:
            # Try to read as key=value pairs
            with open(self.config_file, 'r') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#') and not line.startswith('['):
                        parts = line.split('=', 1)
                        if len(parts) == 2:
                            config[parts[0].strip()] = parts[1].strip()
        
        return config
    
    def initialize(self) -> bool:
        """Initialize all COM objects and login"""
        try:
            # Create COM objects
            self.project.create()
            self.instrument.create()
            self.sample_set_method.create()
            
            # Login to project
            self.project.login(
                database=self.config.get('database', ''),
                project=self.config.get('project', 'Waters GPC Training'),
                username=self.config.get('username', 'system'),
                password=self.config.get('password', 'manager')
            )
            
            return True
            
        except Exception as e:
            raise RuntimeError(f"Initialization failed: {e}")
    
    def connect_instrument(self) -> bool:
        """Connect to instrument using config settings"""
        try:
            node = self.config.get('node', 'Waters-h4q6k34')
            system = self.config.get('system', 'Arc HPLC')
            
            return self.instrument.connect(node, system)
            
        except Exception as e:
            raise RuntimeError(f"Instrument connection failed: {e}")
    
    def discover_systems(self) -> Dict[str, List[str]]:
        """Discover available systems and nodes"""
        return {
            'systems': self.instrument.systems,
            'nodes': self.instrument.acquisition_servers,
            'methods': self.sample_set_method.method_names
        }
    
    def cleanup(self):
        """Cleanup all connections"""
        try:
            self.instrument.disconnect()
            self.project.logoff()
        except Exception:
            pass
    
    def __enter__(self):
        """Context manager entry"""
        self.initialize()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit with cleanup"""
        self.cleanup()


# Convenience functions for quick operations
def discover_empower_systems(config_file: str = "secrets.ini") -> Dict[str, List[str]]:
    """Quick function to discover available Empower systems"""
    with EmpowerToolkit(config_file) as toolkit:
        return toolkit.discover_systems()


def execute_sample_set(method_name: str, config_file: str = "secrets.ini") -> bool:
    """Quick function to execute a sample set method"""
    with EmpowerToolkit(config_file) as toolkit:
        if toolkit.connect_instrument():
            return toolkit.instrument.replace_sample_set(method_name)
    return False
