''' ************************************************************************
''' (c) Copyright 2022 by Wyatt Technology Corporation. All rights reserved.
''' ************************************************************************
Imports AstraLib
Imports SDKCommon
Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Namespace SdkCommandLineAppVB
    ''' <summary>
    ''' Example program of the ASTRA Automation API, showing how to collect data, get and set peaks and baselines and get results and data sets.
    ''' </summary>
    Public Class Program
        ''' <summary>
        ''' Unique identifier of client to the ASTRA Automation API.
        ''' </summary>
        Public Shared ClientId As Guid = Guid.NewGuid()

        ''' <summary>
        ''' Main entry point of application program.
        ''' </summary>
        Public Shared Sub Main()
            Dim process As Process = Process.GetCurrentProcess()
            AstraAdmin.Get.SetAutomationIdentity("SDK Command Line App", "1.0.0.0", process.Id, $"{ClientId}", 1)
            Console.WriteLine("Waiting for instruments...")
            AstraAdmin.Get.WaitForInstruments()
            Console.WriteLine("Instruments detected.")

            If Not SecurityPackLogon() Then
                Console.WriteLine("Could not logon to ASTRA. Incorrect username/password.")
                Return
            End If

            ' Uncomment one of those function calls to exercise a specific aspect of the ASTRA Automation API:
            '
            ' RunSequence()
            ' StartCollectionAndProvideInfoAtTheEnd()
            ' ProcessExperiment()
        End Sub

        ''' <summary>
        ''' Open an experiment, set baselines and peaks, run the experiment, save results and data sets to a file.
        ''' </summary>
        Public Shared Sub ProcessExperiment()
            ' Open an experiment given the path to the experiment file
            Console.WriteLine("Opening experiment, enter path to file:")
            Dim path As String = Console.ReadLine()
            Dim experimentId As Integer = AstraAdmin.Get.OpenExperiment(path)

            ' Retrieve baselines from experiment then modify the array
            Dim baselines = AstraAdmin.Get.GetBaselines(experimentId)
            Console.WriteLine($"Found {baselines.Length} baseline(s).")
            For Each baseline In baselines
                Console.WriteLine($"{baseline.seriesName}: ({baseline.start.x},{baseline.start.y}) to ({baseline.end.x}.{baseline.end.y}).")
            Next

            ' Here, we made sure there is at least one baseline in the array.
            ' If so, we updated the first baseline.
            If baselines.Length <> 0 Then
                baselines(0).start.x = 4.128
                baselines(0).start.y = 0.049
                baselines(0).start.x = 37.36
                baselines(0).start.x = 0.049
                baselines(0).seriesName = "detector1"
                baselines(0).type = BaselineType.eBT_SnapY

                ' Update the experiment baselines with the baselines from here
                AstraAdmin.Get.UpdateBaselines(experimentId, baselines)
            End If

            ' Get peaks from the experiment
            Dim peaks = AstraAdmin.Get.GetPeakRanges(experimentId)

            ' Remove a peak range from the experiment
            AstraAdmin.Get.RemovePeakRange(experimentId, peaks(0).number)
            peaks = AstraAdmin.Get.GetPeakRanges(experimentId)
            Console.WriteLine($"Found {peaks.Length} peak(s).")
            For Each peakRange In peaks
                Console.WriteLine($"{peakRange.number} from {peakRange.start} to {peakRange.end}.")
            Next

            ' Add a peak range 
            AstraAdmin.Get.AddPeakRange(experimentId, 2.0, 3.0)
            peaks = AstraAdmin.Get.GetPeakRanges(experimentId)

            ' Update a peak range given a PeakRange object with the same number.
            Dim peak As New PeakRange With {.number = peaks(0).number, .start = 3.0, .end = 4.0}
            AstraAdmin.Get.UpdatePeakRange(experimentId, peak)

            peaks = AstraAdmin.Get.GetPeakRanges(experimentId)
            Console.WriteLine($"Found {peaks.Length} peak(s).")
            For Each peakRange In peaks
                Console.WriteLine($"{peakRange.number} from {peakRange.start} to {peakRange.end}.")
            Next

            ' Because we made changes to the experiment, we need to run the experiment to get the updated results.
            AstraAdmin.Get.RunExperiment(experimentId)

            ' Extract results to a file
            Console.WriteLine("Saving results, enter path to file:")
            path = Console.ReadLine()
            AstraAdmin.Get.SaveResults(experimentId, path)

            ' Extract data set to a file given the definition name
            Const definitionName As String = "mean square radius vs volume"
            Console.WriteLine("Saving data set, enter path to file:")
            path = Console.ReadLine()
            AstraAdmin.Get.SaveDataSet(experimentId, definitionName, path)
        End Sub

        ''' <summary>
        ''' Run a sequence from configuration given in a CSV file, then save to experiment files.
        ''' </summary>
        Public Shared Sub RunSequence()
            ' Import CSV file.
            Console.WriteLine("Enter CSV file path:")
            Dim filePath As String = Console.ReadLine()

            While String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath)
                Console.WriteLine("File does not exist." & Environment.NewLine & "Enter CSV file path:")
                filePath = Console.ReadLine()
            End While
            Console.WriteLine("Enter path to save experiment files (example: C:\Users\username\Documents\):")
            Dim exportPath As String = Console.ReadLine()

            While Not Directory.Exists(exportPath)
                Console.WriteLine("Directory does not exist." & Environment.NewLine & "Enter path:")
                exportPath = Console.ReadLine()
            End While

            Using reader As New StreamReader(filePath)
                ' Skip first line as it contains the header.
                reader.ReadLine()
                While Not reader.EndOfStream
                    Dim line As String = reader.ReadLine()
                    If String.IsNullOrWhiteSpace(line) Then
                        Continue While
                    End If

                    ' values store data from a row in the csv file, where
                    ' values(0): Enable
                    ' values(1): Name
                    ' values(2): Description
                    ' values(3): Injection
                    ' values(4): Method
                    ' values(5): Duration (minutes)
                    ' values(6): Injection Volume (microL)
                    ' values(7): dn/dc (mL/g)
                    ' values(8): A2 (mol mL/g^2)
                    ' values(9): UV Ext (mL/(mg cm)))
                    ' values(10): Concentration (mg/mL)
                    ' values(11): Flow Rate (mL/min)
                    Dim values() As String = line.Split(","c)

                    If values.Length <> 12 OrElse values(0) = "FALSE" Then
                        Continue While
                    End If

                    Dim injection As Integer = Convert.ToInt32(values(3))

                    For i As Integer = 1 To injection
                        ' Run sequence row data collection, which creates experiment from template and then runs the experiment
                        Dim template As String = values(4)
                        Dim duration As Double = Convert.ToDouble(values(5))
                        Dim injectedVolume As Double = Convert.ToDouble(values(6))
                        Dim flowRate As Double = Convert.ToDouble(values(11))

                        Dim sampleInfo As New SampleInfo With {
                    .name = values(1),
                    .description = values(2),
                    .dndc = Convert.ToDouble(values(7)),
                    .a2 = Convert.ToDouble(values(8)),
                    .uvExtinction = Convert.ToDouble(values(9)),
                    .concentration = Convert.ToDouble(values(10))
                }

                        Dim expFileName As String = If(sampleInfo.name.Length > 0, sampleInfo.name, "untitled")
                        If injection > 1 Then
                            expFileName += $" ({i} of {injection})"
                        End If

                        AstraAdmin.Get.CollectData(template, Path.Combine(exportPath, expFileName), sampleInfo, duration, injectedVolume, flowRate, AddressOf Console.WriteLine)
                    Next
                End While
            End Using
        End Sub

        ''' <summary>
        ''' Run a sequence from configuration given in a CSV file, then save to experiment files.
        ''' </summary>
        Public Shared Sub StartCollectionAndProvideInfoAtTheEnd()
            ' Create an experiment given the path to a template.
            Console.WriteLine("Enter method path:")
            Dim method As String = Console.ReadLine()
            Dim experimentId As Integer = AstraAdmin.Get.NewExperimentFromTemplate(method)

            Console.WriteLine("Enter path to save experiment files (example: C:\Users\username\Documents\):")
            Dim exportPath As String = Console.ReadLine()

            ' Let's start a thread that will automatically stop the collection after 70s.
            ' This assumes a method with a duration of at least 70s.
            Dim thread As New Thread(Sub()
                                         Thread.Sleep(70000)
                                         AstraAdmin.Get.StopCollection(experimentId)
                                     End Sub)
            thread.Start()

            AstraAdmin.Get.CollectDataWithMethodInfoCallback(experimentId, AddressOf Console.WriteLine,
        Function()
            Return New AstraMethodInfo() With {
                .ExperimentPath = exportPath,
                .FlowRate = 1.1,
                .InjectedVolume = 5.2,
                .Sample = New SampleInfo With {
                    .name = "BSA",
                    .description = "BSA Description",
                    .dndc = 0.195,
                    .a2 = 0.1,
                    .uvExtinction = 1,
                    .concentration = 1.5
                }
            }
        End Function)
        End Sub

        ''' <summary>
        ''' Logon to ASTRA. If in security pack mode a username/password/domain is requested.
        ''' </summary>
        ''' <returns>True if not in security pack mode or properly logged on, false otherwise.</returns>
        Private Shared Function SecurityPackLogon() As Boolean
            Dim securityPackActive As Boolean = AstraAdmin.Get.IsSecurityPackActive()
            Dim isLoggedIn As Boolean = AstraAdmin.Get.IsLoggedIn()

            If securityPackActive AndAlso isLoggedIn Then
                Console.WriteLine("You need to login to security pack.")
                Console.WriteLine("username:")
                Dim username As String = Console.ReadLine()
                Console.WriteLine("password:")
                Dim password As String = Console.ReadLine()
                Console.WriteLine("domain:")
                Dim domain As String = Console.ReadLine()
                Dim result = AstraAdmin.Get.ValidateLogon(username, password, domain)

                If result.isValid = 0 Then
                    Return False
                End If
            End If

            Return True
        End Function
    End Class
End Namespace