import comtypes.client
import uuid
import os
import psutil
from astra_admin import AstraAdmin
from comtypes.client import GetEvents

admin = AstraAdmin()

class SdkHelper:
    # Singleton class
    def __new__(cls) -> None:
        if not hasattr(cls, "instance") or not cls.IsInstanceAlreadyInitialized:
            cls.instance = super(SdkHelper, cls).__new__(cls)
            cls.IsInstanceAlreadyInitialized = True
        return cls.instance

    def __init__(self) -> None:
        pass

    def restart_astra(self) -> None:
        admin.dispose()
        for proc in psutil.process_iter():
            # check whether the process name matches
            if proc.name() == "astra.exe":
                proc.kill()
            elif proc.name() == "AstraSecurityPackSdk.exe":
                proc.kill()

        admin.closing_experiment = {}
        admin._experiments = {}

        admin.astra_com = comtypes.client.CreateObject("WTC.ASTRA8.Application.1")
        admin.reset_events()
        admin.connection = GetEvents(admin.astra_com, admin.events)
        admin.astra_sp_com = comtypes.client.CreateObject("Wyatt.AstraSP.1")

    def restart_astra_and_wait(self):
        self.restart_astra()
        admin.reset_astra()
        admin.set_automation_identity("SDK Testing", "1.0.0.0", os.getpid(), f"{uuid.uuid4()}", 1)
        admin.astra_sp_com.SetAutomationIdentity("SDKTestApp", "0.0.0.0", os.getpid(), f"{uuid.uuid4().hex}", 1)

        if not admin.has_instrument_detection_completed():
            admin.wait_for_instruments()

        if admin.is_security_pack_active():
            self.disable_security_pack()

    def enable_security_pack(self, database_name: str, username: str, password: str) -> None:
        """Enable ASTRA Security Pack

        Args:
            database_name (str): security pack database name
            username (str): username for logon
            password (str): password for logon
        """
        admin.astra_sp_com.SetupDatabaseConnection(database_name, username, password)
        admin.astra_sp_com.EnableSecurityPack(1)

    def disable_security_pack(self) -> None:
        """Disable ASTRA Security Pack"""
        admin.astra_sp_com.EnableSecurityPack(0)