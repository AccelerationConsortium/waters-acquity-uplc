"""
Waters Empower Bridge - Python wrapper using working C# executables
==================================================================

This wrapper uses subprocess calls to our proven working C# executables
to handle the 32-bit COM object requirements properly.
"""

import subprocess
import json
import os
import time
from typing import Dict, List, Optional, Any
from dataclasses import dataclass


@dataclass 
class SystemInfo:
    """Information about discovered Empower systems"""
    systems: List[str]
    nodes: List[str] 
    methods: List[str]


class EmpowerBridge:
    """Base class for bridging to C# executables"""
    
    def __init__(self, exe_name: str):
        self.exe_name = exe_name
        self.exe_path = os.path.join(os.path.dirname(__file__), exe_name)
        
    def _run_executable(self, args: List[str] = None) -> Dict[str, Any]:
        """Run the C# executable and parse output"""
        if not os.path.exists(self.exe_path):
            raise RuntimeError(f"Executable not found: {self.exe_path}")
        
        cmd = [self.exe_path]
        if args:
            cmd.extend(args)
        
        try:
            # Use Popen with communicate to send newline to bypass "Press any key"
            process = subprocess.Popen(
                cmd,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                cwd=os.path.dirname(self.exe_path)
            )
            
            # Send a newline to bypass the "Press any key" prompt
            stdout, stderr = process.communicate(input='\n', timeout=30)
            
            return {
                'returncode': process.returncode,
                'stdout': stdout,
                'stderr': stderr,
                'success': process.returncode == 0
            }
            
        except subprocess.TimeoutExpired:
            try:
                process.kill()
                process.wait(timeout=5)
            except:
                pass
            raise RuntimeError(f"Executable timeout: {self.exe_name}")
        except Exception as e:
            raise RuntimeError(f"Failed to run {self.exe_name}: {e}")


class EmpowerDiscovery(EmpowerBridge):
    """Wrapper for SystemDiscoveryExtractor.exe"""
    
    def __init__(self):
        super().__init__("SystemDiscoveryExtractor.exe")
    
    def discover_all(self) -> Dict[str, List[str]]:
        """Run system discovery and parse results"""
        result = self._run_executable()
        
        if not result['success']:
            raise RuntimeError(f"Discovery failed: {result['stderr']}")
        
        # Parse the output to extract systems, nodes, and methods
        output = result['stdout']
        
        systems = []
        nodes = []
        methods = []
        
        lines = output.split('\n')
        current_section = None
        
        for line in lines:
            line = line.strip()
            
            if "Available systems:" in line:
                current_section = "systems"
                continue
            elif "Available acquisition servers:" in line:
                current_section = "nodes"
                continue
            elif "Available sample set methods:" in line:
                current_section = "methods"
                continue
            elif line.startswith("- ") or line.startswith("  - "):
                # Extract the item name
                item = line.replace("- ", "").replace("  - ", "").strip()
                if item and current_section:
                    if current_section == "systems":
                        systems.append(item)
                    elif current_section == "nodes":
                        nodes.append(item)
                    elif current_section == "methods":
                        methods.append(item)
        
        return {
            'systems': systems,
            'nodes': nodes,
            'methods': methods,
            'raw_output': output,
            'success': True
        }
    
    def test_connection(self, node: str = None, system: str = None) -> Dict[str, Any]:
        """Test connection using discovery executable"""
        result = self._run_executable()
        
        # Parse connection test results from output
        output = result['stdout']
        connection_successful = "Connection successful" in output or "✅ Connection initiated" in output
        
        return {
            'success': result['success'],
            'connection_test': connection_successful,
            'raw_output': output,
            'error': result['stderr'] if result['stderr'] else None
        }


class EmpowerStaticTest(EmpowerBridge):
    """Wrapper for StaticAutomation.exe"""
    
    def __init__(self):
        super().__init__("StaticAutomation.exe")
    
    def test_authentication(self) -> Dict[str, Any]:
        """Test basic COM authentication"""
        result = self._run_executable()
        
        output = result['stdout']
        login_successful = "login successful" in output.lower() or "✅" in output
        
        return {
            'success': result['success'],
            'authentication': login_successful,
            'raw_output': output,
            'error': result['stderr'] if result['stderr'] else None
        }


class EmpowerToolkitBridge:
    """Main bridge class combining all functionality"""
    
    def __init__(self, config_file: str = "secrets.ini"):
        self.config_file = config_file
        self.config = self._load_config()
        self.discovery = EmpowerDiscovery()
        self.static_test = EmpowerStaticTest()
    
    def _load_config(self) -> Dict[str, str]:
        """Load configuration from secrets.ini"""
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
        
        # Simple key=value parser
        with open(self.config_file, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and not line.startswith('['):
                    parts = line.split('=', 1)
                    if len(parts) == 2:
                        config[parts[0].strip()] = parts[1].strip()
        
        return config
    
    def test_authentication(self) -> bool:
        """Test if authentication works"""
        try:
            result = self.static_test.test_authentication()
            return result.get('authentication', False)
        except Exception:
            return False
    
    def discover_systems(self) -> Dict[str, List[str]]:
        """Discover available systems, nodes, and methods"""
        try:
            return self.discovery.discover_all()
        except Exception as e:
            return {
                'systems': [],
                'nodes': [],
                'methods': [],
                'error': str(e),
                'success': False
            }
    
    def test_connection(self) -> Dict[str, Any]:
        """Test instrument connection"""
        try:
            node = self.config.get('node', 'Waters-h4q6k34')
            system = self.config.get('system', 'Arc HPLC')
            return self.discovery.test_connection(node, system)
        except Exception as e:
            return {
                'success': False,
                'connection_test': False,
                'error': str(e)
            }
    
    def get_system_info(self) -> Dict[str, Any]:
        """Get comprehensive system information"""
        info = {
            'config': self.config,
            'authentication_test': False,
            'discovery_result': {},
            'connection_test': {},
            'timestamp': time.strftime("%Y-%m-%d %H:%M:%S")
        }
        
        # Test authentication
        try:
            info['authentication_test'] = self.test_authentication()
        except Exception as e:
            info['authentication_error'] = str(e)
        
        # Test discovery
        try:
            info['discovery_result'] = self.discover_systems()
        except Exception as e:
            info['discovery_error'] = str(e)
        
        # Test connection
        try:
            info['connection_test'] = self.test_connection()
        except Exception as e:
            info['connection_error'] = str(e)
        
        return info
    
    def run_diagnostics(self) -> str:
        """Run comprehensive diagnostics and return formatted report"""
        info = self.get_system_info()
        
        report = []
        report.append("Waters Empower Toolkit - Python Bridge Diagnostics")
        report.append("=" * 60)
        report.append(f"Timestamp: {info['timestamp']}")
        report.append()
        
        # Configuration
        report.append("Configuration:")
        for key, value in info['config'].items():
            if key == 'password':
                value = '*' * len(value) if value else '(empty)'
            report.append(f"  {key}: {value}")
        report.append()
        
        # Authentication test
        auth_status = "✅ PASS" if info.get('authentication_test') else "❌ FAIL"
        report.append(f"Authentication Test: {auth_status}")
        if 'authentication_error' in info:
            report.append(f"  Error: {info['authentication_error']}")
        report.append()
        
        # Discovery test
        discovery = info.get('discovery_result', {})
        if discovery.get('success'):
            report.append("✅ System Discovery: PASS")
            report.append(f"  Systems found: {len(discovery.get('systems', []))}")
            report.append(f"  Nodes found: {len(discovery.get('nodes', []))}")
            report.append(f"  Methods found: {len(discovery.get('methods', []))}")
            
            if discovery.get('systems'):
                report.append("  Available Systems:")
                for system in discovery['systems']:
                    report.append(f"    - {system}")
            
            if discovery.get('nodes'):
                report.append("  Available Nodes:")
                for node in discovery['nodes']:
                    report.append(f"    - {node}")
        else:
            report.append("❌ System Discovery: FAIL")
            if 'discovery_error' in info:
                report.append(f"  Error: {info['discovery_error']}")
            elif discovery.get('error'):
                report.append(f"  Error: {discovery['error']}")
        report.append()
        
        # Connection test
        connection = info.get('connection_test', {})
        if connection.get('connection_test'):
            report.append("✅ Connection Test: PASS")
        else:
            report.append("❌ Connection Test: FAIL")
            if connection.get('error'):
                report.append(f"  Error: {connection['error']}")
        report.append()
        
        # Summary
        tests = [
            info.get('authentication_test', False),
            discovery.get('success', False),
            connection.get('connection_test', False)
        ]
        passed = sum(tests)
        total = len(tests)
        
        report.append(f"Summary: {passed}/{total} tests passed")
        
        if passed == total:
            report.append("🎉 All tests passed! Empower Toolkit is accessible via Python.")
        elif passed > 0:
            report.append("⚠ Partial success. Some functionality available.")
        else:
            report.append("❌ All tests failed. Check Empower installation and configuration.")
        
        return "\n".join(report)


# Convenience functions
def discover_empower_systems(config_file: str = "secrets.ini") -> Dict[str, List[str]]:
    """Quick function to discover available Empower systems"""
    bridge = EmpowerToolkitBridge(config_file)
    return bridge.discover_systems()


def test_empower_connection(config_file: str = "secrets.ini") -> bool:
    """Quick function to test Empower connection"""
    bridge = EmpowerToolkitBridge(config_file)
    result = bridge.test_connection()
    return result.get('connection_test', False)


def run_empower_diagnostics(config_file: str = "secrets.ini") -> str:
    """Run comprehensive Empower diagnostics"""
    bridge = EmpowerToolkitBridge(config_file)
    return bridge.run_diagnostics()


# For backward compatibility with original wrapper interface
class EmpowerToolkit:
    """Compatibility wrapper for the bridge approach"""
    
    def __init__(self, config_file: str = "secrets.ini"):
        self.bridge = EmpowerToolkitBridge(config_file)
        self.config = self.bridge.config
    
    def initialize(self) -> bool:
        """Test if initialization works"""
        return self.bridge.test_authentication()
    
    def discover_systems(self) -> Dict[str, List[str]]:
        """Discover systems using bridge"""
        result = self.bridge.discover_systems()
        return {
            'systems': result.get('systems', []),
            'nodes': result.get('nodes', []),
            'methods': result.get('methods', [])
        }
    
    def connect_instrument(self) -> bool:
        """Test instrument connection"""
        result = self.bridge.test_connection()
        return result.get('connection_test', False)
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        pass  # No cleanup needed for subprocess approach
