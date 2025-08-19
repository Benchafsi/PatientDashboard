using Microsoft.AspNetCore.Mvc;
using PatientDashboard.Controllers;
using PatientDashboard.Models;
using PatientDashboard.Services.Interfaces;
using PatientDashboardTests.TestData;

namespace PatientDashboardTests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly Mock<IVitalSignService> _mockVitalSignService;
    private readonly Mock<IVitalSimulationService> _mockSimulationService;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();
        _mockVitalSignService = new Mock<IVitalSignService>();
        _mockSimulationService = new Mock<IVitalSimulationService>();

        _controller = new PatientsController(
            _mockPatientService.Object,
            _mockVitalSignService.Object,
            _mockSimulationService.Object);
    }

    [Fact]
    public async Task GetPatients_ShouldReturnOkResult_WithAllPatients()
    {
        // Arrange
        var patients = PatientFaker.GeneratePatients(5);
        _mockPatientService.Setup(s => s.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(patients);

        _mockPatientService.Verify(s => s.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPatient_ShouldReturnOkResult_WhenPatientExists()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        _mockPatientService.Setup(s => s.GetByIdAsync(patient.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _controller.GetPatient(patient.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(patient);

        _mockPatientService.Verify(
            s => s.GetByIdAsync(patient.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPatient_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientId = 999;
        _mockPatientService.Setup(s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _controller.GetPatient(patientId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        _mockPatientService.Verify(
            s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVitals_ShouldReturnOkResult_WhenVitalsExist()
    {
        // Arrange
        var patientId = 123;
        var vitals = VitalSignFaker.GenerateVitalSigns(3, patientId);
        _mockVitalSignService.Setup(
            s => s.GetByPatientIdAsync(patientId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vitals);

        // Act
        var result = await _controller.GetVitals(patientId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(vitals);

        _mockVitalSignService.Verify(
            s => s.GetByPatientIdAsync(patientId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVitals_ShouldReturnNotFound_WhenNoVitalsExist()
    {
        // Arrange
        var patientId = 123;
        _mockVitalSignService.Setup(
            s => s.GetByPatientIdAsync(patientId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VitalSign>());

        // Act
        var result = await _controller.GetVitals(patientId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        _mockVitalSignService.Verify(
            s => s.GetByPatientIdAsync(patientId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartSimulation_ShouldReturnAcceptedResult_WhenPatientExists()
    {
        // Arrange
        var patientId = 123;
        var patient = PatientFaker.GeneratePatient();
        patient.Id = patientId;

        _mockPatientService.Setup(s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _controller.StartSimulation(patientId);

        // Assert
        result.Should().BeOfType<AcceptedResult>();
        var acceptedResult = result as AcceptedResult;
        acceptedResult!.Value.Should().BeEquivalentTo(new { started = true, id = patientId });

        _mockPatientService.Verify(
            s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockSimulationService.Verify(s => s.Start(patientId, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task StartSimulation_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientId = 999;
        _mockPatientService.Setup(s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _controller.StartSimulation(patientId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        _mockPatientService.Verify(
            s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockSimulationService.Verify(s => s.Start(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task StartSimulation_ShouldStartSimulationWithCorrectDuration()
    {
        // Arrange
        var patientId = 123;
        var patient = PatientFaker.GeneratePatient();
        patient.Id = patientId;

        _mockPatientService.Setup(s => s.GetByIdAsync(patientId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _controller.StartSimulation(patientId);

        // Assert
        _mockSimulationService.Verify(s => s.Start(patientId, TimeSpan.FromSeconds(30)), Times.Once);
    }
}
