"""
Waters Automation Portal Driver - Sample Transfer Only

This module provides a Python interface for controlling the Waters Automation Portal
for sample transfer operations only. The Automation Portal provides access to sample
tray slots for automated sample handling.

Based on Waters Automation Portal PC Protocol Specification (715008839).
"""

import serial
import socket
import time
import logging
from typing import Optional, Dict, Any
import config


class AutomationPortalError(Exception):
    """Custom exception for Automation Portal communication errors."""

    pass


class AutomationPortalDriver:
    """
    Driver for Waters Automation Portal - Sample Transfer Operations Only.

    This class provides methods to communicate with and control the Waters Automation Portal
    for sample transfer operations: Extract, Insert, GetStatus, Initialize, etc.

    Based on Waters Automation Portal PC Protocol Specification (715008839).
    """

    def __init__(
        self,
        port: str = None,
        baudrate: int = None,
        timeout: float = None,
        host: str = None,
        tcp_port: int = None,
        comm_mode: str = None,
    ):
        """
        Initialize the Waters Automation Portal driver.

        Args:
            port: Serial port for communication (e.g., 'COM1' on Windows)
            baudrate: Communication baudrate (default from config)
            timeout: Timeout for communication in seconds
            host: IP address for TCP/IP communication
            tcp_port: TCP port for network communication
            comm_mode: Communication mode ('serial' or 'tcp')
        """
        # Communication settings
        self.port = port or config.DEFAULT_PORT
        self.baudrate = baudrate or config.DEFAULT_BAUDRATE
        self.timeout = timeout or config.DEFAULT_TIMEOUT
        self.host = host or config.DEFAULT_TCP_HOST
        self.tcp_port = tcp_port or config.DEFAULT_TCP_PORT
        self.comm_mode = comm_mode or config.COMM_MODE_SERIAL

        # Connection state
        self.connection = None
        self.is_connected = False
        self.sequence_number = 0

        # Instrument information (placeholder until connected)
        self.instrument_id = "Waters Automation Portal"
        self.available_modules = ["Sample Transfer", "Automation Portal"]

        # Setup logging
        self.logger = logging.getLogger(__name__)
        if not self.logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter("%(message)s")
            handler.setFormatter(formatter)
            self.logger.addHandler(handler)
            self.logger.setLevel(logging.DEBUG)

    def connect(self) -> bool:
        """
        Establish connection to the Automation Portal.

        Returns:
            True if connection successful, False otherwise
        """
        try:
            if self.comm_mode == config.COMM_MODE_SERIAL:
                self.connection = serial.Serial(
                    port=self.port,
                    baudrate=self.baudrate,
                    timeout=self.timeout,
                    bytesize=serial.EIGHTBITS,
                    parity=serial.PARITY_NONE,
                    stopbits=serial.STOPBITS_ONE,
                )
                self.logger.info(f"Connected via serial to {self.port}")

            elif self.comm_mode == config.COMM_MODE_TCP:
                self.connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self.connection.settimeout(self.timeout)
                self.connection.connect((self.host, self.tcp_port))
                self.logger.info(f"Connected via TCP to {self.host}:{self.tcp_port}")

            else:
                raise AutomationPortalError(
                    f"Invalid communication mode: {self.comm_mode}"
                )

            self.is_connected = True
            return True

        except Exception as e:
            self.logger.error(f"Connection failed: {e}")
            self.is_connected = False
            return False

    def disconnect(self) -> None:
        """Disconnect from the Automation Portal."""
        if self.connection:
            try:
                self.connection.close()
                self.logger.info("Disconnected from Automation Portal")
            except Exception as e:
                self.logger.error(f"Error during disconnect: {e}")
            finally:
                self.connection = None
                self.is_connected = False

    def _get_next_sequence(self) -> int:
        """Get the next sequence number for commands."""
        self.sequence_number = (self.sequence_number % 255) + 1
        return self.sequence_number

    def _send_command(self, command: str, retries: int = None) -> str:
        """
        Send a command to the Automation Portal and return the response.

        Args:
            command: Command string to send
            retries: Number of retry attempts

        Returns:
            Response from the system

        Raises:
            AutomationPortalError: If communication fails
        """
        if not self.is_connected or not self.connection:
            raise AutomationPortalError("Not connected to Automation Portal")

        if retries is None:
            retries = config.MAX_RETRIES

        for attempt in range(retries + 1):
            try:
                # Prepare command with terminator
                command_str = command + config.COMMAND_TERMINATOR

                if self.comm_mode == config.COMM_MODE_SERIAL:
                    # Serial communication - read multiple lines to get full response
                    self.connection.write(command_str.encode("utf-8"))

                    # Read the full response (may be multiple lines)
                    response_lines = []
                    start_time = time.time()
                    while time.time() - start_time < self.timeout:
                        if self.connection.in_waiting > 0:
                            line = self.connection.readline().decode("utf-8").strip()
                            if line:
                                response_lines.append(line)
                                # Check if we have a complete response
                                complete_check = any(
                                    "Completed(" in line or "Error(" in line
                                    for line in response_lines
                                )
                                if complete_check:
                                    break
                        else:
                            time.sleep(0.01)  # Small delay to prevent busy waiting

                    response = "\n".join(response_lines)

                elif self.comm_mode == config.COMM_MODE_TCP:
                    # TCP communication
                    self.connection.send(command_str.encode("utf-8"))
                    response = (
                        self.connection.recv(config.DATA_BUFFER_SIZE)
                        .decode("utf-8")
                        .strip()
                    )

                else:
                    raise AutomationPortalError(
                        f"Invalid communication mode: {self.comm_mode}"
                    )

                self.logger.debug(f"Sent: {command} | Received: {response}")
                return response

            except Exception as e:
                if attempt < retries:
                    self.logger.warning(
                        f"Command failed (attempt {attempt + 1}), retrying: {e}"
                    )
                    time.sleep(config.RETRY_DELAY)
                else:
                    raise AutomationPortalError(
                        f"Communication error after {retries + 1} attempts: {e}"
                    )

        return ""

    def get_status(self) -> Dict[str, Any]:
        """
        Get the current status of the Automation Portal.

        Returns:
            Dictionary containing status information including:
            - success: True if command succeeded
            - system_state: Current system state (OPERATIONAL, etc.)
            - mode: Current system mode
            - status: Movement state
            - door_status: Door position status
            - feeder_status: Feeder status
            - mac_address: Hardware MAC address
        """
        try:
            response = self._send_command("GetStatus")
            self.logger.debug(f"GetStatus raw response: {response}")

            # Parse the response - from the debug output we can see the format:
            # GetStatus\r\n\tReceived(26,GetStatus)\r\n\tCompleted(26,GetStatus,OPERATIONAL,Insert(1),Idle,NoDrawerNoTray,DoorClosed,FeederFullyRetracted,172:16:0:4,00:00:C4:06:01:67)\r\n

            # Parse the response - looking for the Completed line
            # Format: Completed(43,GetStatus,OPERATIONAL,Insert(1),Idle,NoDrawerNoTray,DoorClosed,FeederFullyRetracted,172:16:0:4,00:00:C4:06:01:67)

            if "Completed(" in response:
                # Use regex to parse the response more reliably
                import re

                pattern = (
                    r"Completed\((\d+),GetStatus,([^,]+),"
                    r"([^,)]+(?:\([^)]*\))?),([^,]+),([^,]+),"
                    r"([^,]+),([^,]+),([^,]+),([^,)]+)\)"
                )
                match = re.search(pattern, response)

                if match:
                    (
                        seq,
                        system_state,
                        mode,
                        status,
                        drawer_tray,
                        door,
                        feeder,
                        ip,
                        mac,
                    ) = match.groups()
                    self.logger.debug(
                        f"Regex parsed: seq={seq}, system={system_state}, mode={mode}, status={status}"
                    )
                    return {
                        "success": True,
                        "sequence": int(seq) if seq.isdigit() else 0,
                        "command": "GetStatus",
                        "system_state": system_state,  # OPERATIONAL
                        "mode": mode,  # Insert(1)
                        "status": status,  # Idle
                        "drawer_tray_status": drawer_tray,  # NoDrawerNoTray
                        "door_status": door,  # DoorClosed
                        "feeder_status": feeder,  # FeederFullyRetracted
                        "ip_address": ip,  # 172:16:0:4
                        "mac_address": mac,  # 00:00:C4:06:01:67
                    }
                else:
                    # Fallback: simple manual parsing
                    self.logger.debug("Regex failed, trying manual parsing")
                    completed_start = response.find("Completed(")
                    if completed_start != -1:
                        content_start = completed_start + 10
                        content_end = response.find(")", content_start)
                        if content_end != -1:
                            content = response[content_start:content_end]
                            # Try to extract key information manually
                            if "OPERATIONAL" in content and "DoorClosed" in content:
                                return {
                                    "success": True,
                                    "system_state": "OPERATIONAL",
                                    "mode": (
                                        "Insert(1)"
                                        if "Insert(1)" in content
                                        else "Unknown"
                                    ),
                                    "status": (
                                        "Idle" if "Idle" in content else "Unknown"
                                    ),
                                    "door_status": (
                                        "DoorClosed"
                                        if "DoorClosed" in content
                                        else "Unknown"
                                    ),
                                    "drawer_tray_status": (
                                        "NoDrawerNoTray"
                                        if "NoDrawerNoTray" in content
                                        else "Unknown"
                                    ),
                                    "feeder_status": (
                                        "FeederFullyRetracted"
                                        if "FeederFullyRetracted" in content
                                        else "Unknown"
                                    ),
                                    "mac_address": (
                                        content.split(",")[-1]
                                        if "," in content
                                        else "Unknown"
                                    ),
                                }

            # If parsing failed, return error but still mark as success since we got a response
            return {
                "success": True,
                "raw_response": response,
                "system_state": "Unknown",
                "mode": "Unknown",
                "status": "Unknown",
                "mac_address": "Unknown",
            }

        except Exception as e:
            self.logger.error(f"Error getting status: {e}")
            return {"success": False, "error": str(e)}

    def initialize(self) -> bool:
        """
        Initialize the Automation Portal system.

        Must be called after startup or after an error to switch to operational mode.

        Returns:
            True if initialization successful, False otherwise
        """
        try:
            # Send initialize command - try without sequence first
            response = self._send_command("Initialize")

            # Check immediate response for errors
            if "Error(" in response:
                # Try with sequence if simple command failed
                seq = self._get_next_sequence()
                response = self._send_command(f"Initialize({seq})")

                if "Error(" in response:
                    self.logger.error(f"Initialize command failed: {response}")
                    return False

            # Wait for completion and check final status
            max_wait_time = 30  # seconds
            start_time = time.time()

            while time.time() - start_time < max_wait_time:
                status_response = self._send_command("GetStatus")

                # Check if operation completed successfully
                if "Completed(" in status_response and "Initialize" in status_response:
                    self.logger.info("Initialization completed successfully")
                    return True

                # Check for error in status
                if "Error(" in status_response and "Initialize" in status_response:
                    self.logger.error(f"Initialize operation failed: {status_response}")
                    return False

                # Check if system is already operational (might not need initialization)
                if "OPERATIONAL" in status_response:
                    self.logger.info(
                        "System already operational, initialization not needed"
                    )
                    return True

                time.sleep(0.5)

            # Timeout - operation didn't complete
            self.logger.error(
                f"Initialize operation timed out after {max_wait_time} seconds"
            )
            return False

        except Exception as e:
            self.logger.error(f"Error during initialization: {e}")
            return False

    def extract_drawer(self, tray_position: int) -> bool:
        """
        Extract a tray from the specified sample manager tray position.

        Args:
            tray_position: Tray position (1 or 2)

        Returns:
            True if extraction successful, False otherwise
        """
        if tray_position not in [1, 2]:
            raise ValueError("Tray position must be 1 or 2")

        # Map user positions (1,2) to Waters protocol positions (1,0) 
        # User position 1 = Waters position 1 (Tray 1)
        # User position 2 = Waters position 0 (Tray 2)
        # NOTE: Tray 2 (Waters position 0) was throwing errors during testing - may need hardware check
        waters_position = 1 if tray_position == 1 else 0

        try:
            # Check system ready before extract
            self.logger.info(f"Checking system status before extract from position {tray_position}...")
            if not self._check_ready():
                self.logger.error("System not ready for extract operation")
                return False
            self.logger.info("System ready for extract")

            # Send extract command - format appears to be Extract(tray_position) based on responses
            response = self._send_command(f"Extract({waters_position})")

            # Check the immediate response for success/error
            if "Error(" in response:
                self.logger.error(f"Extract command failed immediately: {response}")
                return False
            
            self.logger.info(f"Extract command sent successfully to position {tray_position}")

            # Wait for extract operation to complete
            if not self._wait_for_ready(timeout=120, operation_name="extract"):
                self.logger.error("Extract operation did not complete properly")
                return False

            self.logger.info(f"Tray extracted successfully from position {tray_position}")
            return True

        except Exception as e:
            self.logger.error(f"Error extracting tray: {e}")
            return False

    def insert_drawer(self, tray_position: int) -> bool:
        """
        Insert a tray into the specified sample manager tray position.

        Args:
            tray_position: Tray position (1 or 2)

        Returns:
            True if insertion successful, False otherwise
        """
        if tray_position not in [1, 2]:
            raise ValueError("Tray position must be 1 or 2")

        # Map user positions (1,2) to Waters protocol positions (1,0)
        # User position 1 = Waters position 1 (Tray 1)  
        # User position 2 = Waters position 0 (Tray 2)
        # NOTE: Tray 2 (Waters position 0) was throwing errors during testing - may need hardware check
        waters_position = 1 if tray_position == 1 else 0

        try:
            # Check system ready before insert
            self.logger.info(f"Checking system status before insert to position {tray_position}...")
            if not self._check_ready():
                self.logger.error("System not ready for insert operation")
                return False
            self.logger.info("System ready for insert")

            # Send insert command - format appears to be Insert(tray_position) based on responses
            response = self._send_command(f"Insert({waters_position})")

            # Check the immediate response for success/error
            if "Error(" in response:
                self.logger.error(f"Insert command failed immediately: {response}")
                return False
            
            self.logger.info(f"Insert command sent successfully to position {tray_position}")

            # Wait for insert operation to complete
            if not self._wait_for_ready(timeout=120, operation_name="insert"):
                self.logger.error("Insert operation did not complete properly")
                return False

            self.logger.info(f"Tray inserted successfully to position {tray_position}")
            return True

        except Exception as e:
            self.logger.error(f"Error inserting tray: {e}")
            return False

    def report_version(self) -> str:
        """
        Get the system version information.

        Returns:
            Version information string
        """
        try:
            response = self._send_command("ReportVersion")
            return response
        except Exception as e:
            self.logger.error(f"Error getting version: {e}")
            return f"Error: {e}"

    def reset_system(self) -> bool:
        """
        Reset the Automation Portal system.

        Returns:
            True if reset successful, False otherwise
        """
        try:
            response = self._send_command("ResetSystem")
            success = "Completed" in response
            if success:
                self.logger.info("System reset completed")
            else:
                self.logger.error(f"System reset failed: {response}")
            return success
        except Exception as e:
            self.logger.error(f"Error resetting system: {e}")
            return False

    def is_drawer_present(self) -> Optional[bool]:
        """
        Check if a tray is currently present.

        Returns:
            True if tray present, False if not, None if status unknown
        """
        try:
            status = self.get_status()
            drawer_status = status.get("drawer_tray_status", "")

            if drawer_status in ["DrawerAndTray", "DrawerOnly"]:
                return True
            elif drawer_status == "NoDrawerNoTray":
                return False
            else:
                return None

        except Exception as e:
            self.logger.error(f"Error checking drawer presence: {e}")
            return None

    def is_door_open(self) -> Optional[bool]:
        """
        Check if the door is currently open.

        Returns:
            True if door open, False if closed, None if status unknown
        """
        try:
            status = self.get_status()
            door_status = status.get("door_status", "")

            if door_status == "DoorOpened":
                return True
            elif door_status == "DoorClosed":
                return False
            else:
                return None

        except Exception as e:
            self.logger.error(f"Error checking door status: {e}")
            return None

    def _check_ready(self) -> bool:
        """Check if system is ready for operations"""
        status = self.get_status()
        is_operational = status.get("system_state") == "OPERATIONAL"
        is_idle = status.get("status") == "Idle"  # Must be exactly 'Idle', not 'WaitingForSmIdle'

        return status.get("success") and is_operational and is_idle

    def _wait_for_ready(
        self, timeout: float = 120, operation_name: str = "operation"
    ) -> bool:
        """Poll until system returns to OPERATIONAL and Idle state"""
        self.logger.info(f"Waiting for {operation_name} to complete...")
        start_time = time.time()

        while time.time() - start_time < timeout:
            status = self.get_status()
            if status.get("success"):
                current_status = status.get("status", "Unknown")
                current_system = status.get("system_state", "Unknown")
                self.logger.debug(f"Status: {current_system} / {current_status}")

                if self._check_ready():
                    elapsed = time.time() - start_time
                    self.logger.info(f"System ready after {elapsed:.1f} seconds")
                    
                    # Add stabilization delay after reaching Idle state
                    self.logger.debug(f"Adding {config.STABILIZATION_DELAY} second stabilization delay...")
                    time.sleep(config.STABILIZATION_DELAY)
                    self.logger.debug("Stabilization complete, system ready for next operation")
                    
                    return True
            else:
                self.logger.warning("Failed to get status")

            time.sleep(2)  # Poll every 2 seconds

        self.logger.error(f"Timeout: System not ready after {timeout} seconds")
        return False

    def __enter__(self):
        """Context manager entry."""
        if self.connect():
            return self
        else:
            raise AutomationPortalError("Failed to connect to Automation Portal")

    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.disconnect()

    # Portal method aliases for compatibility with demo scripts
    def portal_get_status(self) -> Dict[str, Any]:
        """Alias for get_status() method."""
        return self.get_status()

    def portal_report_version(self) -> Dict[str, Any]:
        """Alias for report_version() method."""
        try:
            version_info = self.report_version()
            return {"success": True, "version_info": version_info}
        except Exception as e:
            return {"success": False, "error": str(e)}

    def portal_reset_system(self) -> Dict[str, Any]:
        """Alias for reset_system() method."""
        try:
            success = self.reset_system()
            return {
                "success": success,
                "status": "Reset completed" if success else "Reset failed",
            }
        except Exception as e:
            return {"success": False, "error": str(e)}


# Example usage functions
def example_sample_transfer():
    """Example of basic sample transfer operations."""
    driver = AutomationPortalDriver(port="COM1")

    try:
        # Connect to the system
        if driver.connect():
            print("Connected successfully!")

            # Initialize the system
            if driver.initialize():
                print("System initialized")

                # Get system status
                status = driver.get_status()
                print(f"System status: {status}")

                # Extract tray from position 2
                if driver.extract_drawer(2):
                    print("Tray extracted from position 2")

                    # Wait for user to load/unload samples
                    input("Load/unload samples, then press Enter...")

                    # Insert tray back to position 2
                    if driver.insert_drawer(2):
                        print("Tray inserted back to position 2")

    finally:
        driver.disconnect()


def example_context_manager():
    """Example using context manager for automatic connection handling."""
    with AutomationPortalDriver(port="COM1") as driver:
        # Initialize system
        driver.initialize()

        # Check status
        status = driver.get_status()
        print(f"System status: {status}")

        # Check if drawer is present
        drawer_present = driver.is_drawer_present()
        print(f"Drawer present: {drawer_present}")


if __name__ == "__main__":
    # Run example
    print("Waters Automation Portal Driver - Sample Transfer Example")
    example_sample_transfer()
