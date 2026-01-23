"""
Configuration loader for Waters Automation Portal Driver
Loads settings from config.yaml file
"""

import yaml
import os
from typing import Dict, Any

# Get the directory where this script is located
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CONFIG_FILE = os.path.join(SCRIPT_DIR, 'config.yaml')

def load_config() -> Dict[str, Any]:
    """Load configuration from YAML file."""
    try:
        with open(CONFIG_FILE, 'r', encoding='utf-8') as f:
            return yaml.safe_load(f)
    except FileNotFoundError:
        raise FileNotFoundError(f"Configuration file not found: {CONFIG_FILE}")
    except yaml.YAMLError as e:
        raise ValueError(f"Error parsing configuration file: {e}")

# Load configuration once when module is imported
_config = load_config()

# Serial communication settings
DEFAULT_PORT = _config['serial']['port']
DEFAULT_BAUDRATE = _config['serial']['baudrate']
DEFAULT_TIMEOUT = _config['serial']['timeout']

# Network communication settings
DEFAULT_TCP_HOST = _config['network']['host']
DEFAULT_TCP_PORT = _config['network']['port']

# Protocol settings
COMMAND_TERMINATOR = _config['protocol']['command_terminator']
RESPONSE_TERMINATOR = _config['protocol']['response_terminator']
MAX_RETRIES = _config['protocol']['max_retries']
RETRY_DELAY = _config['protocol']['retry_delay']
DATA_BUFFER_SIZE = _config['protocol']['data_buffer_size']

# Communication modes
COMM_MODE_SERIAL = _config['communication']['mode_serial']
COMM_MODE_TCP = _config['communication']['mode_tcp']

# Export all config sections for direct access
PORTAL_ERROR_CODES = _config['error_codes']
PORTAL_SYSTEM_MODES = _config['system_modes']
PORTAL_DOOR_STATUS = _config['door_status']
PORTAL_FEEDER_STATUS = _config['feeder_status']
PORTAL_DRAWER_TRAY_STATUS = _config['drawer_tray_status']
PORTAL_MOVE_COMMANDS = _config['move_commands']
PORTAL_MOVE_STATES = _config['move_states']
COMMAND_TIMEOUTS = _config['timeouts']
PORTAL_VALIDATION = _config['validation']

def get_config() -> Dict[str, Any]:
    """Get the full configuration dictionary."""
    return _config

def get_error_message(error_code: int) -> str:
    """Get error message for a given error code."""
    return PORTAL_ERROR_CODES.get(error_code, f"Unknown error code: {error_code}")
