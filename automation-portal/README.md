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
3. **Ensure hardware connection**: Connect Waters Automation Portal via RS232 to COM4 (or change port in config.yaml)

## Quick Start

### Interactive Menu (Recommended)
For easy operation, use the interactive command-line menu:

```bash
python automation_menu.py
```

This provides a user-friendly interface with guided operations and status monitoring.

### Usage Examples
For programmatic examples and API demonstrations:

```bash
python usage_examples.py --help
```

Available commands:
- `python usage_examples.py status` - Check system status
- `python usage_examples.py extract 1` - Extract from tray 1
- `python usage_examples.py insert 2` - Insert to tray 2
- `python usage_examples.py workflow` - Complete sample transfer workflow

### Basic Programmatic Usage

```python
from automation_portal_driver import AutomationPortalDriver

# Create and connect
driver = AutomationPortalDriver()
if driver.connect():
    # Check status and initialize if needed
    status = driver.get_status()
    if status['system_state'] != 'OPERATIONAL':
        driver.initialize()
    
    # Sample operations
    driver.extract_drawer(1)  # Extract from tray 1
    driver.insert_drawer(1)   # Insert back to tray 1
    
    driver.disconnect()
```

## Configuration

Settings are configured in `config.yaml`:

```yaml
serial:
  port: 'COM4'
  baudrate: 38400
  timeout: 5.0
```

For detailed configuration options, see `config.yaml`.

## System States & Operations

### System States
- **UNINIT**: System needs initialization
- **OPERATIONAL**: Ready for sample operations  
- **ERROR**: Problem detected, requires initialization

### Sample Positions
- **Position 1**: Tray 1 (user position 1)
- **Position 0**: Tray 2 (user position 2)

Note: User positions 1,2 map to driver positions 1,0 respectively.

## Error Handling

### Common Error Codes
- **15**: Invalid tray number
- **16**: Drawer/tray detection failure  
- **28**: No drawer present at sample manager position

### Recovery Steps
1. Check system status: `driver.get_status()`
2. Initialize if needed: `driver.initialize()`
3. Retry operation

## Files

```
automation-portal/
├── automation_menu.py              # Interactive CLI interface
├── automation_portal_driver.py     # Core driver implementation  
├── usage_examples.py               # API usage examples and CLI
├── config.py                       # YAML configuration loader
├── config.yaml                     # Configuration file
├── requirements.txt                # Python dependencies
└── README.md                       # This documentation
```
