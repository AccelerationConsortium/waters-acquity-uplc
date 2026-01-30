# -*- coding: utf-8 -*-
import os
import time
import uuid

from threading import Thread

from astra_admin import AstraAdmin, BaselineDetails, BaselineType, PeakRange, SampleInfo, AstraMethodInfo

class SdkCommandLineApp:
    """Example program of the ASTRA Automation API, showing how to collect data, get and set peaks and baselines and get results and data sets.
    """
    # Unique identifier of client to the ASTRA Automation API.
    client_id: uuid.UUID = uuid.uuid4().hex

    def main(self) -> None:
        """Main entry point of program.
        """
        AstraAdmin().set_automation_identity(
            "SDK Command Line App", "1.0.0.0",
            os.getpid(),
            self.client_id,
            1
            )

        print("Waiting for instruments...")
        AstraAdmin().wait_for_instruments()
        print("Instruments have been detected.")

        # if not self.security_pack_logon():
        #     print("Could not logon to ASTRA. Incorrect username/password.")
        #     return

        # Uncomment one of those function calls to exercise a specific aspect of the ASTRA Automation API:
        # self.run_sequence()
        # self.start_collection_and_provide_info_at_the_end()
        # self.process_experiment()
        print("Program completed.")

    def process_experiment(self) -> None:
        """Open an experiment, set baselines and peaks, run the experiment, save results and data sets to a file.
        """
        # Open an experiment given the path to the experiment file
        path = input("Opening experiment, enter path to file:")
        experiment_id = AstraAdmin().open_experiment(path)

        # Retrieve baselines from experiment then modify the array
        baselines: list[BaselineDetails] = AstraAdmin().get_baselines(experiment_id)
        print(f"Found {len(baselines)} baseline(s).")
        for baseline in baselines:
            print(f"{baseline.seriesName}: ({baseline.start.x},{baseline.start.y}) to ({baseline.end.x}.{baseline.end.y}).")

        # Here, we made sure there is at least one baseline in the array.
        # If so, we updated the first baseline.
        if len(baselines) > 0:
            baselines[0].start.x = 4.128
            baselines[0].start.y = 0.049
            baselines[0].start.x = 37.360
            baselines[0].start.x = 0.049
            baselines[0].seriesName = "detector1"
            baselines[0].type = BaselineType.SNAP_Y.value

            # Update the experiment baselines with the baselines from here
            AstraAdmin().update_baselines(experiment_id, baselines)

        baselines = AstraAdmin().get_baselines(experiment_id)

        # Get peaks from the experiment
        peaks: list[PeakRange] = AstraAdmin().get_peak_ranges(experiment_id)

        # Remove a peak range from the experiment
        AstraAdmin().remove_peak_range(experiment_id, peaks[0].number)
        peaks = AstraAdmin().get_peak_ranges(experiment_id)
        print(f"Found {len(peaks)} peak(s).")
        for peak_range in peaks:
            print(f"{peak_range.number} from {peak_range.start} to {peak_range.end}.")

        # Add a peak range
        AstraAdmin().add_peak_range(experiment_id, 2.0, 3.0)
        peaks = AstraAdmin().get_peak_ranges(experiment_id)

        # Update a peak range given a PeakRange object with the same number.
        peak = PeakRange(peaks[0].number, 3.0, 4.0)
        AstraAdmin().update_peak_range(experiment_id, peak)

        peaks = AstraAdmin().get_peak_ranges(experiment_id)
        print(f"Found {len(peaks)} peak(s).")
        for peak_range in peaks:
            print(f"{peak_range.number} from {peak_range.start} to {peak_range.end}.")

        # Because we made changes to the experiment, we need to run the experiment to get the updated results.
        AstraAdmin().run_experiment(experiment_id)

        # Extract results to a file
        path = input("Saving results, enter path to file:")
        AstraAdmin().save_results(experiment_id, path)

        # Extract data set to a file given the definition name
        definition_name = "mean square radius vs volume"
        path = input("Saving data set, enter path to file:")
        AstraAdmin().save_data_set(experiment_id, definition_name, path)

    def run_sequence(self) -> None:
        """Run a sequence from configuration given in a CSV file, then save to experiment files.
        """
        # Import CSV file.
        path = input("Enter CSV file path:")

        while len(path) == 0 or not os.path.exists(path):
            path = input("File does not exist.\nEnter CSV file path:")

        export_path = input("Enter path to save experiment files (example: C:\\Users\\username\\Documents\\):")

        while not os.path.exists(export_path):
            export_path = input("Directory does not exist.\nEnter path:")

        lines = open(path, 'r').readlines()
        for line in lines:
            if len(line) == 0:
                continue

            # values store data from a row in the csv file, where
            # values[0]: Enable
            # values[1]: Name
            # values[2]: Description
            # values[3]: Injection
            # values[4]: Method
            # values[5]: Duration (minutes)
            # values[6]: Injection Volume (microL)
            # values[7]: dn/dc (mL/g)
            # values[8]: A2 (mol mL/g^2)
            # values[9]: UV Ext (mL/(mg cm)))
            # values[10]: Concentration (mg/mL)
            # values [11]: Flow Rate (mL/min)
            values = line.split(',')

            if len(values) != 12 or values[0] == "FALSE":
                continue

            injection = int(values[3])

            for i in range(1, injection+1):
                # Run sequence row data collection, which creates experiment from template and then runs the experiment
                template = values[4]
                duration = float(values[5])
                injected_volume = float(values[6])
                flow_rate = float(values[11])

                sample_info = SampleInfo(
                    name=values[1],
                    description=values[2],
                    dndc=float(values[7]),
                    a2=float(values[8]),
                    uvExtinction=float(values[9]),
                    concentration=float(values[10])
                )

                exp_file_name = sample_info.name if len(sample_info.name) > 0 else "untitled"
                if injection > 1:
                    exp_file_name += f" ({i} of {injection})"

                AstraAdmin().collect_data(template, os.path.join(export_path, exp_file_name), sample_info, duration, injected_volume, flow_rate, print)

    def start_collection_and_provide_info_at_the_end(self) -> None:
        """Run a sequence from configuration given in a CSV file, then save to experiment files.
        """
        # Create an experiment given the path to a template.
        method = input("Enter method path:")
        experiment_id = AstraAdmin().new_experiment_from_template(method)

        export_path = input("Enter path to save experiment files (example: C:\\Users\\username\\Documents\\):")

        # Let's start a thread that will automatically stop the collection after 70s.
        # This assumes a method with a duration of at least 70s.
        def stop_after_70s():
            time.sleep(70)
            AstraAdmin().stop_collection(experiment_id)
        Thread(target=stop_after_70s).start()

        AstraAdmin().collect_data_with_method_info_callback(
            experiment_id,
            print,
            AstraMethodInfo(
                experimentPath=export_path,
                flowRate=1.1,
                injectedVolume=5.2,
                sample=SampleInfo(
                    name = "BSA",
                    description = "BSA Description",
                    dndc = 0.195,
                    a2 = 0.1,
                    uvExtinction = 1,
                    concentration = 1.5
                ),
                duration=0.1
            )
        )

    def security_pack_logon(self) -> bool:
        """Logon to ASTRA. If in security pack mode a username/password/domain is requested.

        Returns:
            bool: True if not in security pack mode or properly logged on, false otherwise.
        """
        # Logon to security pack if needed
        security_pack_active = AstraAdmin().is_security_pack_active()
        is_logged_in = AstraAdmin().is_logged_in()

        # If in security pack and not logged on already, logon with security pack credential.
        if security_pack_active and not is_logged_in:
            print("You need to login to security pack.")
            username = input("username:")
            password = input("password:")
            domain = input("domain:")
            result = AstraAdmin().validate_logon(username, password, domain)

            # result.isValid return 0 if logon failed, return 1 if succeeded.
            if result.isValid == 0:
                return False

        return True


if __name__ == "__main__":
    app = SdkCommandLineApp()

    app.main()

    # Call to shutdown ASTRA SDK
    AstraAdmin().dispose()
