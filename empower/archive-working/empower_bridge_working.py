#!/usr/bin/env python3
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
    """Python bridge to Waters Empower using working C# executables"""
    
    def __init__(self, executable_dir: str = None):
        """
        Initialize the Empower bridge
        
        Args:
            executable_dir: Directory containing C# executables (defaults to current dir)
        """
        self.executable_dir = executable_dir or os.path.dirname(os.path.abspath(__file__))
        self._validate_executables()
    
    def _validate_executables(self):
        """Validate that required C# executables exist"""
        required_exes = [
            "NonInteractiveDiscovery.exe",
            "ComprehensiveTest.exe", 
            "QuickTest.exe"
        ]
        
        missing = []
        for exe in required_exes:
            exe_path = os.path.join(self.executable_dir, exe)
            if not os.path.exists(exe_path):
                missing.append(exe)
        
        if missing:
            raise FileNotFoundError(f"Missing C# executables: {missing}")
    
    def discover_systems(self) -> SystemInfo:
        """
        Discover available Empower systems, nodes, and methods
        
        Returns:
            SystemInfo with lists of systems, nodes, and methods
        """
        try:
            exe_path = os.path.join(self.executable_dir, "NonInteractiveDiscovery.exe")
            
            # Run with empty input to avoid hanging on "Press any key"
            result = subprocess.run(
                [exe_path],
                input="",
                capture_output=True,
                text=True,
                timeout=30,
                cwd=self.executable_dir
            )
            
            if result.returncode != 0:
                raise RuntimeError(f"C# executable failed: {result.stderr}")
            
            # Parse output to extract systems, nodes, and methods
            systems = []
            nodes = []
            methods = []
            
            lines = result.stdout.split('\n')
            current_section = None
            
            for line in lines:
                line = line.strip()
                
                if "Available Systems:" in line:
                    current_section = "systems"
                elif "Available Acquisition Servers:" in line:
                    current_section = "nodes" 
                elif "Available Sample Set Methods:" in line:
                    current_section = "methods"
                elif line.startswith("- ") and current_section:
                    item = line[2:].strip()
                    if current_section == "systems":
                        systems.append(item)
                    elif current_section == "nodes":
                        nodes.append(item)
                    elif current_section == "methods":
                        methods.append(item)
            
            return SystemInfo(systems=systems, nodes=nodes, methods=methods)
            
        except subprocess.TimeoutExpired:
            raise TimeoutError("System discovery timed out")
        except Exception as e:
            raise RuntimeError(f"System discovery failed: {e}")
    
    def run_diagnostics(self) -> str:
        """
        Run comprehensive diagnostics using C# library
        
        Returns:
            Detailed diagnostic report as string
        """
        try:
            exe_path = os.path.join(self.executable_dir, "QuickTest.exe")
            
            result = subprocess.run(
                [exe_path],
                input="",
                capture_output=True, 
                text=True,
                timeout=30,
                cwd=self.executable_dir
            )
            
            # Return both stdout and stderr for complete diagnostics
            output = result.stdout
            if result.stderr:
                output += "\n\nErrors:\n" + result.stderr
            
            return output
            
        except subprocess.TimeoutExpired:
            return "Diagnostics timed out after 30 seconds"
        except Exception as e:
            return f"Diagnostics failed: {e}"
    
    def test_comprehensive(self) -> str:
        """
        Run comprehensive test suite
        
        Returns:
            Complete test results
        """
        try:
            exe_path = os.path.join(self.executable_dir, "ComprehensiveTest.exe")
            
            result = subprocess.run(
                [exe_path],
                input="",
                capture_output=True,
                text=True, 
                timeout=60,
                cwd=self.executable_dir
            )
            
            output = result.stdout
            if result.stderr:
                output += "\n\nErrors/Warnings:\n" + result.stderr
            
            return output
            
        except subprocess.TimeoutExpired:
            return "Comprehensive test timed out after 60 seconds"
        except Exception as e:
            return f"Comprehensive test failed: {e}"
    
    def get_status(self) -> Dict[str, Any]:
        """
        Get current Empower system status
        
        Returns:
            Dictionary with status information
        """
        try:
            # Use discovery as a health check
            info = self.discover_systems()
            
            status = {
                "connected": len(info.systems) > 0,
                "systems_available": len(info.systems),
                "nodes_available": len(info.nodes),
                "methods_available": len(info.methods),
                "timestamp": time.time()
            }
            
            if info.systems:
                status["primary_system"] = info.systems[0]
            if info.nodes:
                status["primary_node"] = info.nodes[0]
            
            return status
            
        except Exception as e:
            return {
                "connected": False,
                "error": str(e),
                "timestamp": time.time()
            }


# Convenience functions for quick access
def discover_systems(executable_dir: str = None) -> SystemInfo:
    """Quick function to discover Empower systems"""
    bridge = EmpowerBridge(executable_dir)
    return bridge.discover_systems()


def run_diagnostics(executable_dir: str = None) -> str:
    """Quick function to run diagnostics"""
    bridge = EmpowerBridge(executable_dir)
    return bridge.run_diagnostics()


def get_system_status(executable_dir: str = None) -> Dict[str, Any]:
    """Quick function to get system status"""
    bridge = EmpowerBridge(executable_dir) 
    return bridge.get_status()


# Example usage
if __name__ == "__main__":
    print("Waters Empower Bridge - Python Test")
    print("==================================")
    print()
    
    try:
        # Test system discovery
        print("🔍 Discovering systems...")
        info = discover_systems()
        print(f"✅ Found {len(info.systems)} systems, {len(info.nodes)} nodes, {len(info.methods)} methods")
        
        if info.systems:
            print("Systems:", info.systems[:3])  # Show first 3
        if info.nodes:
            print("Nodes:", info.nodes[:3])
        if info.methods:
            print("Methods:", info.methods[:5])  # Show first 5
        
        print()
        
        # Test diagnostics
        print("🏥 Running diagnostics...")
        diag = run_diagnostics()
        print("Diagnostics completed")
        print()
        
        # Test status
        print("📊 Getting status...")
        status = get_system_status()
        print("Status:", status)
        
    except Exception as e:
        print(f"❌ Error: {e}")
