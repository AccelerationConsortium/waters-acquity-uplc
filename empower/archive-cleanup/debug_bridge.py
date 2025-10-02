"""
Simple bridge test to debug subprocess calls
"""

import subprocess
import os

def test_executable_direct():
    """Test calling C# executable directly"""
    exe_path = "SystemDiscoveryExtractor.exe"
    
    print(f"Testing executable: {exe_path}")
    print(f"Exists: {os.path.exists(exe_path)}")
    print(f"Current dir: {os.getcwd()}")
    
    if os.path.exists(exe_path):
        try:
            print("Running executable...")
            # Use Popen with input to handle "Press any key" prompt
            process = subprocess.Popen(
                [exe_path],
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                cwd=os.getcwd()
            )
            
            # Send newline to bypass prompt
            stdout, stderr = process.communicate(input='\n', timeout=15)
            
            # Create result object to match subprocess.run interface
            result = type('obj', (object,), {
                'returncode': process.returncode,
                'stdout': stdout,
                'stderr': stderr
            })()
            
            print(f"Return code: {result.returncode}")
            print(f"Stdout length: {len(result.stdout)}")
            print(f"Stderr length: {len(result.stderr)}")
            
            if result.stdout:
                print("First 500 chars of output:")
                print(result.stdout[:500])
            
            if result.stderr:
                print("Stderr:")
                print(result.stderr)
            
        except subprocess.TimeoutExpired:
            print("❌ Executable timeout")
        except Exception as e:
            print(f"❌ Error: {e}")
    else:
        print("❌ Executable not found")

if __name__ == "__main__":
    test_executable_direct()
