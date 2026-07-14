using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using System.ComponentModel;

namespace RealFenixFailures.Application.Session;

public class RealisticSessionState : INotifyPropertyChanged {
    #region Fields

    private readonly FlightSession _session;
    private readonly UserAircraftDto _aircraft;
    private FlightPhaseEnum flightPhase = FlightPhaseEnum.Unknown;
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Properties
    public FlightSession Session => _session;
    public UserAircraftDto Aircraft => _aircraft;

    public FlightPhaseEnum FlightPhase {
        get => flightPhase;
        set {
            if (flightPhase != value) {
                flightPhase = value;
                OnPropertyChanged(nameof(FlightPhase));
            }
        }
    }

    public List<FailurePreset> AvailablePresets { get; internal set; }
    public List<PresetFailureDefinition> ExecutedFailures { get; internal set; }
    public List<FailurePreset> ExecutedPresets => ExecutedFailures.Where(x => x.Preset != null).Select(f => f.Preset!).ToList();

    public IReadOnlyList<FenixFailureDto> ArmedFenixFailures { get; internal set; }
    public IReadOnlyList<FenixFailureDto> ActivatedFenixFailures { get; internal set; }
    #endregion

    #region Constructor
    public RealisticSessionState(FlightSession session, UserAircraftDto aircraft) {
        _session = session;
        _aircraft = aircraft;
    }
    #endregion




}
