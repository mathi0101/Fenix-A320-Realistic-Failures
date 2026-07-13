using Microsoft.Extensions.Logging;
using RealFenixFailures.Integrations.Fenix.Interfaces;
using RealFenixFailures.Integrations.Fenix.Models;
using System.Text.Json;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class MockFenixApiClient : IFenixApiClient {
    private readonly ILogger<MockFenixApiClient> _logger;
    private readonly Dictionary<string, bool> _failureState = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public MockFenixApiClient(ILogger<MockFenixApiClient> logger) {
        _logger = logger;
        InitializeMockData();
    }

    private void InitializeMockData() {
        _lock.EnterWriteLock();
        try {
            _failureState["F_HYDRAULIC_SYSTEM_A"] = false;
            _failureState["F_HYDRAULIC_SYSTEM_B"] = false;
            _failureState["F_ELECTRICAL_SYSTEM_1"] = false;
            _failureState["F_ELECTRICAL_SYSTEM_2"] = false;
            _failureState["F_ENGINE_1_FAILURE"] = false;
            _failureState["F_ENGINE_2_FAILURE"] = false;
            _failureState["F_PRESSURIZATION_SYSTEM"] = false;
            _failureState["F_AIR_CONDITIONING"] = false;
            _failureState["F_LANDING_GEAR"] = false;
            _failureState["F_FLIGHT_CONTROLS"] = false;
        } finally {
            _lock.ExitWriteLock();
        }

        _logger.LogInformation("MockFenixApiClient initialized with {Count} failures", _failureState.Count);
    }

    public async Task<bool> IsApiAlive(CancellationToken ct) {
        await Task.Delay(10, ct);
        _logger.LogDebug("MockFenixApiClient: IsApiAlive returning true");
        return true;
    }

    public async Task<Stream?> GetManualFailuresAsync(CancellationToken ct) {
        _lock.EnterReadLock();
        try {
            var response = new FenixManualFailuresResponse(
                new[] {
                    new FenixAtaBlock(
                        Id: 1,
                        Title: "Hydraulic Systems",
                        ShortTitle: "HYD",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Hydraulic System A",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_HYDRAULIC_SYSTEM_A",
                                        Title: "Hydraulic System A Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_HYDRAULIC_SYSTEM_A", false)
                                    )
                                }
                            ),
                            new FenixFailureGroup(
                                GroupName: "Hydraulic System B",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_HYDRAULIC_SYSTEM_B",
                                        Title: "Hydraulic System B Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_HYDRAULIC_SYSTEM_B", false)
                                    )
                                }
                            )
                        }
                    ),
                    new FenixAtaBlock(
                        Id: 2,
                        Title: "Electrical Systems",
                        ShortTitle: "ELEC",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Electrical System 1",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_ELECTRICAL_SYSTEM_1",
                                        Title: "Electrical System 1 Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_ELECTRICAL_SYSTEM_1", false)
                                    )
                                }
                            ),
                            new FenixFailureGroup(
                                GroupName: "Electrical System 2",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_ELECTRICAL_SYSTEM_2",
                                        Title: "Electrical System 2 Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_ELECTRICAL_SYSTEM_2", false)
                                    )
                                }
                            )
                        }
                    ),
                    new FenixAtaBlock(
                        Id: 3,
                        Title: "Propulsion",
                        ShortTitle: "ENG",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Engines",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_ENGINE_1_FAILURE",
                                        Title: "Engine 1 Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_ENGINE_1_FAILURE", false)
                                    ),
                                    new FenixManualFailure(
                                        Id: "F_ENGINE_2_FAILURE",
                                        Title: "Engine 2 Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_ENGINE_2_FAILURE", false)
                                    )
                                }
                            )
                        }
                    ),
                    new FenixAtaBlock(
                        Id: 4,
                        Title: "Environmental Control",
                        ShortTitle: "ECS",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Pressurization",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_PRESSURIZATION_SYSTEM",
                                        Title: "Pressurization System Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_PRESSURIZATION_SYSTEM", false)
                                    )
                                }
                            ),
                            new FenixFailureGroup(
                                GroupName: "Air Conditioning",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_AIR_CONDITIONING",
                                        Title: "Air Conditioning Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_AIR_CONDITIONING", false)
                                    )
                                }
                            )
                        }
                    ),
                    new FenixAtaBlock(
                        Id: 5,
                        Title: "Landing Gear",
                        ShortTitle: "LDG",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Landing Gear",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_LANDING_GEAR",
                                        Title: "Landing Gear Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_LANDING_GEAR", false)
                                    )
                                }
                            )
                        }
                    ),
                    new FenixAtaBlock(
                        Id: 6,
                        Title: "Flight Controls",
                        ShortTitle: "FC",
                        Groups: new[] {
                            new FenixFailureGroup(
                                GroupName: "Control Surfaces",
                                Failures: new[] {
                                    new FenixManualFailure(
                                        Id: "F_FLIGHT_CONTROLS",
                                        Title: "Flight Controls Failure",
                                        FailureCondition: null,
                                        Failed: _failureState.GetValueOrDefault("F_FLIGHT_CONTROLS", false)
                                    )
                                }
                            )
                        }
                    )
                }
            );

            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, response, cancellationToken: ct);
            stream.Position = 0;

            _logger.LogDebug("MockFenixApiClient: Returning failures stream with {Count} active failures",
                _failureState.Count(f => f.Value));

            return stream;
        } finally {
            _lock.ExitReadLock();
        }
    }

    public async Task<Stream?> SendFailureAsync(FenixSaveManualRequest rq, CancellationToken ct) {
        _lock.EnterWriteLock();
        try {
            _failureState[rq.Id] = rq.Failed;
            _logger.LogInformation("MockFenixApiClient: Set failure {FailureId} to {State}",
                rq.Id, rq.Failed ? "ACTIVE" : "INACTIVE");

            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, new { success = true }, cancellationToken: ct);
            stream.Position = 0;

            return stream;
        } finally {
            _lock.ExitWriteLock();
        }
    }
}
