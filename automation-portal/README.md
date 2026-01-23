# Waters Automation Portal Driver

A Python driver for controlling Waters Automation Portal sample transfer operations via serial communication. This driver provides a clean interface for automated sample handling in Waters Acquity UPC systems.

## Overview

The Waters Automation Portal Driver enables programmatic control of sample transfer operations including:
- Sample extraction from instrument positions
- Sample insertion to instrument positions  
- System initialization and status monitoring
- Error handling and recovery

## Installation

1. **Clone or download** this repository
2. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```
3. **Ensure hardware connection**: Connect Waters Automation Portal via RS232 to COM4 (or change DEFAULT_PORT in config.py)

## Quick Start

### Interactive Menu (Recommended)
For easy operation, use the interactive command-line menu:

```bash
python automation_menu.py
```

This provides a user-friendly interface with guided operations:
1. Connect to Automation Portal
2. Check system status
3. Initialize system (if needed)
4. Extract/Insert samples
5. Monitor operations

### Programmatic Usage

```python
from automation_portal_driver import AutomationPortalDriver

# Create and connect
driver = AutomationPortalDriver()
if driver.connect():
    # Initialize system (required after reset/error)
    driver.initialize()
    
    # Check status
    status = driver.get_status()
    print(f"System: {status['system_state']}, Door: {status['door_status']}")
    
    # Extract sample from position 1 to loading station
    if driver.extract_drawer(1):
        print("Sample extracted successfully")
    
    # Insert sample from loading station to position 0
    if driver.insert_drawer(0):
        print("Sample inserted successfully")
    
    driver.disconnect()
```

## System States & Operations

### System States
- **UNINIT**: System needs initialization
- **OPERATIONAL**: Ready for sample operations  
- **ERROR**: Problem detected, requires initialization
- **SERVICE**: Configuration mode (use web interface)

### Required Workflow
1. **Connect** to automation portal
2. **Initialize** system (if not OPERATIONAL)
3. **Verify** door is closed and system ready
4. **Perform** sample transfer operations

### Sample Positions
- **Position 0**: Drawer 2 (instrument position)
- **Position 1**: Drawer 1 (instrument position)
- **Loading Station**: External position for manual sample placement

## Configuration

### Hardware Settings (config.py)
```python
DEFAULT_PORT = 'COM4'              # Serial port
DEFAULT_BAUDRATE = 38400           # Communication speed
DEFAULT_TIMEOUT = 5.0              # Command timeout
```

### Communication Protocol
- **Serial**: RS232, 8N1, no flow control
- **Commands**: ASCII with CR terminator
- **Responses**: Multi-line status with error codes

## Error Handling

### Common Error Codes
- **15**: Invalid tray number
- **16**: Drawer/tray detection failure  
- **28**: No drawer present at sample manager position
- **27**: Drawer already present at position

### Recovery Steps
1. Check system status: `driver.get_status()`
2. Initialize if needed: `driver.initialize()`
3. Verify door is closed
4. Retry operation

### Error State Recovery
```python
# Check if system is in error state
status = driver.get_status()
if status['system_state'] == 'ERROR':
    print("System in error state, initializing...")
    driver.initialize()
```

## API Reference

### AutomationPortalDriver Class

#### Connection Methods
- `connect()` → bool: Connect to automation portal
- `disconnect()`: Close connection
- `is_connected` → bool: Check connection status

#### System Operations  
- `initialize()` → bool: Initialize system to OPERATIONAL state
- `get_status()` → dict: Get current system status
- `report_version()` → str: Get firmware version

#### Sample Transfer
- `extract_drawer(position: int)` → bool: Extract sample from position (0 or 1)
- `insert_drawer(position: int)` → bool: Insert sample to position (0 or 1)

#### Status Information
```python
status = driver.get_status()
# Returns:
{
    'system_state': 'OPERATIONAL',      # System mode
    'door_status': 'DoorClosed',        # Door position  
    'drawer_tray_status': 'NoDrawerNoTray',  # Sample presence
    'mode': 'NoMovementCmd',            # Current operation
    'status': 'Idle'                    # Movement state
}
```

## Hardware Requirements

- **Waters Automation Portal** with sample manager
- **RS232 connection** to Windows PC (COM4)
- **Python 3.7+** with pyserial
- **Windows operating system**

## Safety Considerations

- **Always initialize** system after reset or error
- **Verify door is closed** before operations
- **Check system status** before sample transfers
- **Handle errors gracefully** with proper recovery

## Troubleshooting

### Connection Issues
```bash
# Check if COM4 is available
python -c "import serial; print(serial.Serial('COM4', 38400))"
```

### System Not Responding
1. Check physical connections
2. Verify COM port in Device Manager
3. Restart automation portal hardware
4. Re-run initialization

### Sample Transfer Failures
1. Verify correct tray positions (0 or 1)  
2. Check that samples are properly loaded
3. Ensure door is fully closed
4. Initialize system if in error state

## Files Structure

```
automation-portal/
├── automation_menu.py              # Interactive command-line interface
├── automation_portal_driver.py     # Core driver implementation  
├── config.py                       # Configuration settings
├── requirements.txt                # Python dependencies
├── setup.py                        # Package installation
├── README.md                       # This documentation
└── docs/                          # Additional documentation
```

## Contributing

When modifying the driver:
1. Test with actual hardware before committing
2. Update configuration in `config.py` as needed
3. Add error handling for new operations
4. Update this README for new features

## License

See LICENSE file for details.
