import os
import pathlib


class KnownPaths:
    astra_process_name = "astra"

    def get_build_path(self) -> str:
        path = self.get_this_file_path()
        # Path is hardcoded for 64-bit testing regardless of the selected platform in the solution.
        artifacts_dir = os.path.abspath(os.path.join(os.path.dirname(path), r"\..\..\..\bin64"))

        return os.path.join(artifacts_dir, "Debug")

    def get_astra_exe_path(self) -> str:
        """Location of the ASTRA executable.

        Returns:
            str: Full path to the ASTRA executable.
        """
        return f"{self.get_build_path()}\\{self.astra_process_name}.exe"

    def get_this_file_path(self) -> str:
        """Trick function to get the path of this current class on disk.

        Returns:
            str: Path of this current class on disk.
        """
        return str(pathlib.Path(__file__).resolve())

    def get_qa_test_data(self) -> str:
        """Return path for QA test data

        Returns:
            str: Full path to the QA test data folder
        """
        path = self.get_this_file_path()
        return os.path.abspath(os.path.dirname(path) + r"\..\..\..\..\QA-Test-Data")

    def get_experiment_data(self, experiment_name: str) -> str:
        """Return file name for an experiment.

        Args:
            experiment_name (str): _description_

        Returns:
            str: Full path to the experiment
        """
        experiment_path = os.path.join(self.get_qa_test_data(), "experiments")
        return os.path.join(experiment_path, experiment_name)

    def get_masters_data(self, experiment_name: str) -> str:
        """Return file name for an experiment.

        Args:
            experiment_name (str): _description_

        Returns:
            str: Full path to the experiment
        """
        experiment_path = os.path.join(self.get_qa_test_data(), "Masters")
        return os.path.join(experiment_path, experiment_name)

    def get_artifacts_path(self) -> str:
        """Return path for artifacts

        Returns:
            str: Full path to the artifacts folder
        """
        artifacts_path = self.get_this_file_path()
        return os.path.abspath(os.path.dirname(artifacts_path) + r"\..\..\..\..\ci\artifacts")

    def get_screenshot_path(self) -> str:
        """Path where screenshots are saved.

        Returns:
            str: Full path to the Screenshots-SdkTests folder.
        """
        artifacts_dir = os.path.join(self.get_artifacts_path(), "Screenshots-SdkTests")

        return artifacts_dir

    def get_masters_path(self) -> str:
        """Return path for results masters

        Returns:
            str: Full path to the masters folder
        """
        return os.path.join(self.get_qa_test_data(), "Masters")

    def get_masters_reports_path(self) -> str:
        return os.path.join(self.get_masters_path(), "ExportedReports")

    def get_results_path(self) -> str:
        """Return path for results

        Returns:
            str: Full path to the results folder
        """
        return os.path.join(self.get_artifacts_path(), "Results")

    def get_reports_path(self) -> str:
        """Return path for reports results tests

        Returns:
            str: Full path to the results folder
        """
        return os.path.join(self.get_artifacts_path(), "Reports")
