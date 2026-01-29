"""
Configuration settings for Waters Acquity UPC Driver
"""

# Serial communication settings
DEFAULT_PORT = 'COM4'
DEFAULT_BAUDRATE = 38400  # Waters Automation Portal specification (715008839)
DEFAULT_TIMEOUT = 5.0

# Network communication settings (Automation Portal often uses TCP/IP)
DEFAULT_TCP_HOST = '192.168.1.100'  # Default Waters instrument IP
DEFAULT_TCP_PORT = 34567  # Common Waters automation port
NETWORK_TIMEOUT = 10.0

# Protocol settings
COMMAND_TERMINATOR = '\r'  # Waters uses only CR (not CRLF)
RESPONSE_TERMINATOR = '\r\n'
MAX_RETRIES = 3
RETRY_DELAY = 1.0  # seconds

# System limits and defaults (Waters Acquity UPC typical ranges)
MAX_FLOW_RATE = 4.0  # mL/min (UPC typical max)
MIN_FLOW_RATE = 0.01  # mL/min
DEFAULT_FLOW_RATE = 1.0  # mL/min

MAX_TEMPERATURE = 100.0  # °C (UPC column heater max)
MIN_TEMPERATURE = 15.0  # °C (ambient minimum)
DEFAULT_TEMPERATURE = 40.0  # °C

MAX_PRESSURE = 414.0  # bar (6000 psi - UPC typical max)
MIN_PRESSURE = 0.0  # bar
DEFAULT_PRESSURE = 207.0  # bar (3000 psi)

# Sample handling
MAX_INJECTION_VOLUME = 50.0  # μL
MIN_INJECTION_VOLUME = 0.1  # μL
DEFAULT_INJECTION_VOLUME = 5.0  # μL

# Waters Acquity specific modules
MODULES = {
    'BSM': 'Binary Solvent Manager',
    'SM': 'Solvent Manager',
    'TUV': 'Tunable UV Detector',
    'FLR': 'Fluorescence Detector',
    'ELS': 'Evaporative Light Scattering Detector',
    'CM': 'Column Manager',
    'SQD': 'Single Quadrupole Detector',
    'QDa': 'QDa Detector'
}

# Communication modes
COMM_MODE_SERIAL = 'serial'
COMM_MODE_TCP = 'tcp'
DEFAULT_COMM_MODE = COMM_MODE_SERIAL

# Command timeout settings
COMMAND_TIMEOUT = 10.0  # seconds
DATA_TIMEOUT = 30.0  # seconds
STATUS_TIMEOUT = 5.0  # seconds

# Portal operation settings
STABILIZATION_DELAY = 5.0  # seconds - delay after reaching Idle state before next operation

# Data collection settings
DEFAULT_SAMPLING_RATE = 10  # Hz
MAX_DATA_POINTS = 100000
DATA_BUFFER_SIZE = 1024

# Method file extensions
METHOD_EXTENSIONS = ['.met', '.method', '.ezx']

# Waters instrument status codes
STATUS_CODES = {
    0: 'Ready',
    1: 'Running',
    2: 'Error',
    3: 'Standby',
    4: 'Maintenance',
    5: 'Initializing'
}

# Error codes (typical Waters error ranges)
ERROR_CODES = {
    1000: 'Communication Error',
    1001: 'Invalid Command',
    1002: 'Parameter Out of Range',
    1003: 'System Not Ready',
    1004: 'Method Not Found',
    1005: 'Hardware Error'
}

# Logging configuration
LOG_LEVEL = 'INFO'
LOG_FORMAT = '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
LOG_FILE = 'waters_acquity.log'

# Waters Automation Portal Error Codes (from Protocol Specification 715008839)
PORTAL_ERROR_CODES = {
    1: "Command parsing failure",
    2: "Sequence number is below the expected range",
    3: "Sequence number is above the expected range",
    4: "PC command is too long",
    5: "Maximum size of a command is exceeded",
    6: "Sample manager communication problem",
    7: "Sample manager is busy",
    8: "Sample manager was not in idle state",
    9: "Both door sensors are active",
    10: "Door movement problem while opening",
    11: "Door movement problem while closing",
    12: "Feeder calibration failure",
    13: "Feeder movement problem while expanding",
    14: "Feeder movement problem while retracting",
    15: "Invalid tray number",
    16: "Drawer and/or tray detection failure",
    17: "No drawer and no tray detected at retraction",
    18: "Door did not move when initializing",
    19: "Drawer and tray present when start extraction",
    20: "Drawer present when start extraction",
    21: "Picked up nothing during extraction",
    22: "No drawer or tray present at start insertion",
    23: "Drawer present after insertion",
    24: "Drawer and tray present after insertion",
    25: "Feeder motor controller overtemperature",
    26: "Door motor controller overtemperature",
    27: "Insert: Already drawer present at SM position",
    28: "Extract: No drawer present at SM position",
    29: "SM rotated to an incorrect angle",
    30: "Timeout on PC command"
}

# Portal System Modes
PORTAL_SYSTEM_MODES = {
    'UNINIT': 'System powered-on or reset',
    'OPERATIONAL': 'Successfully initialized, waiting for Extract/Insert',
    'ERROR': 'Problem detected during command execution',
    'SERVICE': 'Web interface for configuration/calibration',
    'FW-UPGRADE': 'Firmware upgrade mode'
}

# Portal Door Status Values
PORTAL_DOOR_STATUS = {
    'DoorOpened': 'Door is fully open',
    'DoorClosed': 'Door is fully closed',
    'DoorIntermediate': 'Door is in intermediate position',
    'DoorBothSensors': 'Both sensors active (physically impossible - fatal error)'
}

# Portal Feeder Status Values
PORTAL_FEEDER_STATUS = {
    'FeederFullyInSM': 'Feeder fully inserted in sample manager',
    'FeederIntermediate': 'Feeder in intermediate position',
    'FeederNotCalibrated': 'Feeder calibration required',
    'FeederFullyRetracted': 'Feeder fully retracted'
}

# Portal Drawer/Tray Status Values
PORTAL_DRAWER_TRAY_STATUS = {
    'DrawerAndTray': 'Drawer with sample plate present',
    'DrawerOnly': 'Drawer without sample plate present',
    'NoDrawerNoTray': 'No drawer or tray present',
    'TrayWithoutDrawer': 'Tray without drawer (physically impossible - fatal error)',
    'Empty': 'Position is empty',
    'Unknown': 'Status unknown (typically due to error)'
}

# Portal Move Commands
PORTAL_MOVE_COMMANDS = {
    'NoMoveCmd': 'No movement command active',
    'Initialize': 'System initialization in progress',
    'Extract(0)': 'Extract from tray position 2 (Waters internal: 0)',
    'Extract(1)': 'Extract from tray position 1 (Waters internal: 1)', 
    'Insert(0)': 'Insert to tray position 2 (Waters internal: 0)',
    'Insert(1)': 'Insert to tray position 1 (Waters internal: 1)'
}

# Portal Move States
PORTAL_MOVE_STATES = {
    'NoMovement': 'No movement in progress',
    'Idle': 'System idle and ready',
    'SERVICE-Idle': 'Service mode idle',
    'FW_UPGRADE-Idle': 'Firmware upgrade mode idle'
}

# Portal Communication Settings
PORTAL_COMM_SETTINGS = {
    'SERIAL_PORT': 'COM4',  # Default RS232 port
    'SERIAL_BAUDRATE': 38400,  # As specified in Waters documentation 715008839
    'SERIAL_TIMEOUT': 5.0,
    'SERIAL_DATABITS': 8,
    'SERIAL_STOPBITS': 1,
    'SERIAL_PARITY': 'N',  # None (0 parity bits per specification)
    'SERIAL_FLOW_CONTROL': 'none',  # No handshake per Waters script
    'TCP_PORT': 502,  # Default Modbus TCP port (if TCP mode supported)
    'TCP_TIMEOUT': 10.0,
    'COMMAND_TERMINATOR': '\r',  # Waters uses CR only
    'RESPONSE_TIMEOUT': 30.0,  # Some portal operations can take time
    'MAX_COMMAND_LENGTH': 1024,  # Based on protocol specification
    'SEQUENCE_NUMBER_MIN': 1,
    'SEQUENCE_NUMBER_MAX': 255
}

# Portal Command Timeouts (in seconds)
PORTAL_TIMEOUTS = {
    'GetStatus': 5,
    'Initialize': 120,  # Initialization can take up to 2 minutes
    'Extract': 60,      # Extract operations typically take ~1 minute
    'Insert': 60,       # Insert operations typically take ~1 minute
    'ReportVersion': 5,
    'ResetSystem': 30
}

# Portal Validation Ranges
PORTAL_VALIDATION = {
    'TRAY_POSITIONS': [1, 2],  # Valid tray positions (user interface)
    'MAX_EXTRACT_DISTANCE': 175,  # mm - maximum feeder expansion
    'MIN_DRAWER_DETECT_DISTANCE': 70,  # mm - minimum distance for drawer detection
    'MAX_DRAWER_DETECT_DISTANCE': 80,  # mm - maximum distance for drawer detection
    'REFERENCE_POSITION_TOLERANCE': 1.0,  # degrees - SM angle tolerance
    'TRAY_RELEASE_ANGLE': 10.0  # degrees - angle for tray decoupling
}
