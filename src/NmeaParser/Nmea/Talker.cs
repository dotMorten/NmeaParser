//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

using System.Collections.Generic;

namespace NmeaParser.Nmea
{
    internal static class TalkerHelper
    {
        internal static Talker GetTalker(string messageType)
        {
            if (messageType[0] == 'P')
                return Talker.ProprietaryCode;
            if (TalkerLookupTable.ContainsKey(messageType.Substring(0, 2)))
            {
                return TalkerLookupTable[messageType.Substring(0, 2)];
            }
            return Talker.Unknown;
        }

        private static readonly Dictionary<string, Talker> TalkerLookupTable = new Dictionary<string, Talker>()
        {
            {"AB", Talker.IndependentAISBaseStation                 },
            {"AD", Talker.DependentAISBaseStation                   },
            {"AG", Talker.HeadingTrackControllerGeneral             },
            {"AP", Talker.HeadingTrackControllerMagnetic            },
            {"AI", Talker.MobileClassAorBAISStation                 },
            {"AN", Talker.AISAidstoNavigationStation                },
            {"AR", Talker.AISReceivingStation                       },
            {"AS", Talker.AISStation                                },
            {"AT", Talker.AISTransmittingStation                    },
            {"AX", Talker.AISSimplexRepeaterStation                 },
            {"BI", Talker.BilgeSystems                              },
            {"CD", Talker.DigitalSelectiveCalling                   },
            {"CR", Talker.DataReceiver                              },
            {"CS", Talker.Satellite                                 },
            {"CT", Talker.RadioTelephoneMFHF                        },
            {"CV", Talker.RadioTelephoneVHF                         },
            {"CX", Talker.ScanningReceiver                          },
            {"DE", Talker.DECCANavigator                            },
            {"DF", Talker.DirectionFinder                           },
            {"DU", Talker.DuplexRepeaterStation                     },
            {"EC", Talker.ElectronicChartSystem                     },
            {"EI", Talker.ElectronicChartDisplayInformationSystem   },
            {"EP", Talker.EmergencyPositionIndicatingBeacon         },
            {"ER", Talker.EngineRoomMonitoringSystems               },
            {"FD", Talker.FireDoorControllerMonitoringPoint         },
            {"FE", Talker.FireExtinguisherSystem                    },
            {"FR", Talker.FireDetectionPoint                        },
            {"FS", Talker.FireSprinklerSystem                       },
            {"GA", Talker.GalileoPositioningSystem                  },
            {"GB", Talker.BeiDouNavigationSatelliteSystem           },
            {"GL", Talker.GlonassReceiver                           },
            {"GN", Talker.GlobalNavigationSatelliteSystem           },
            {"GP", Talker.GlobalPositioningSystem                   },
            {"GI", Talker.IndianRegionalNavigationSatelliteSystem   },
            {"GQ", Talker.QuasiZenithSatelliteSystem                },
            {"HC", Talker.CompassMagnetic                           },
            {"HE", Talker.GyroNorthSeeking                          },
            {"HF", Talker.Fluxgate                                  },
            {"HN", Talker.GyroNonNorthSeeking                       },
            {"HD", Talker.HullDoorControllerMonitoringPanel         },
            {"HS", Talker.HullStressMonitoring                      },
            {"II", Talker.IntegratedInstrumentation                 },
            {"IN", Talker.IntegratedNavigation                      },
            {"LC", Talker.LoranC                                    },
            {"P ", Talker.ProprietaryCode                           },
            {"RA", Talker.RadarAndOrRadarPlotting                   },
            {"RC", Talker.PropulsionMachineryIncludingRemoteControl },
            {"SA", Talker.PhysicalShoreAISStation                   },
            {"SD", Talker.SounderDepth                              },
            {"SG", Talker.SteeringGearSteeringEngine                },
            {"SN", Talker.ElectronicPositioningSystem               },
            {"SS", Talker.SounderScanning                           },
            {"TI", Talker.TurnRateIndicator                         },
            {"UP", Talker.MicroprocessorController                  },
            {"U0", Talker.UserID0                                   },
            {"U1", Talker.UserID1                                   },
            {"U2", Talker.UserID2                                   },
            {"U3", Talker.UserID3                                   },
            {"U4", Talker.UserID4                                   },
            {"U5", Talker.UserID5                                   },
            {"U6", Talker.UserID6                                   },
            {"U7", Talker.UserID7                                   },
            {"U8", Talker.UserID8                                   },
            {"U9", Talker.UserID9                                   },
            {"VD", Talker.Doppler                                   },
            {"VM", Talker.SpeedLogWaterMagnetic                     },
            {"VW", Talker.SpeedLogWaterMechanical                   },
            {"VR", Talker.VoyageDataRecorder                        },
            {"WD", Talker.WatertightDoorControllerMonitoringPanel   },
            {"WI", Talker.WeatherInstruments                        },
            {"WL", Talker.WaterLevelDetectionSystems                },
            {"YX", Talker.Transducer                                },
            {"ZA", Talker.AtomicsClock                              },
            {"ZC", Talker.Chronometer                               },
            {"ZQ", Talker.Quartz                                    },
            {"ZV", Talker.RadioUpdate                               },

        };

    }

    /// <summary>
    /// Talker Identifier
    /// </summary>
    public enum Talker
    {
        /// <summary>
        /// Multiple talker IDs sometimes seen in <see cref="IMultiSentenceMessage"/>
        /// </summary>
        Multiple = -2,
        /// <summary>
        /// Unrecognized Talker ID
        /// </summary>
        Unknown = -1,
        /// <summary>Independent AIS Base Station</summary>
        IndependentAISBaseStation, // = AB
        /// <summary>Dependent AIS Base Station</summary>
        DependentAISBaseStation, // = AD 
        /// <summary>Heading Track Controller (Autopilot) - General</summary>
        HeadingTrackControllerGeneral, // = AG
        /// <summary>Heading Track Controller (Autopilot) - Magnetic</summary>
        HeadingTrackControllerMagnetic, // = AP
        /// <summary>Mobile Class A or B AIS Station</summary>
        MobileClassAorBAISStation, // = AI
        /// <summary>AIS Aids to Navigation Station </summary>
        AISAidstoNavigationStation, // = AN
        /// <summary>AIS Receiving Station</summary>
        AISReceivingStation, // = AR
        /// <summary>AIS Station (ITU_R M1371,  (“Limited Base Station’)</summary>
        AISStation, // = AS
        /// <summary>AIS Transmitting Station</summary>
        AISTransmittingStation, // = AT
        /// <summary>AIS Simplex Repeater Station</summary>
        AISSimplexRepeaterStation, // = AX
        /// <summary>BeiDou Navigation Satellite System</summary>
        BeiDouNavigationSatelliteSystem, // == GB
        /// <summary>Bilge Systems</summary>
        BilgeSystems, // = BI
        /// <summary></summary>
        DigitalSelectiveCalling, // = CD
        /// <summary></summary>
        DataReceiver, // = CR
        /// <summary></summary>
        Satellite, // = CS
        /// <summary></summary>
        RadioTelephoneMFHF, // = CT
        /// <summary></summary>
        RadioTelephoneVHF, // = CV
        /// <summary></summary>
        ScanningReceiver, // = CX
        /// <summary></summary>
        DECCANavigator, // = DE
        /// <summary></summary>
        DirectionFinder, // = DF
        /// <summary></summary>
        DuplexRepeaterStation, // = DU
        /// <summary></summary>
        ElectronicChartSystem, // = EC
        /// <summary></summary>
        ElectronicChartDisplayInformationSystem, // = EI
        /// <summary></summary>
        EmergencyPositionIndicatingBeacon, // = EP
        /// <summary></summary>
        EngineRoomMonitoringSystems, // = ER
        /// <summary></summary>
        FireDoorControllerMonitoringPoint, // = FD
        /// <summary></summary>
        FireExtinguisherSystem, // = FE
        /// <summary></summary>
        FireDetectionPoint, // = FR
        /// <summary></summary>
        FireSprinklerSystem, // = FS
        /// <summary>Galileo Positioning System</summary>
        GalileoPositioningSystem, // = GA
        /// <summary>GLONASS Receiver</summary>
        GlonassReceiver, // = GL
        /// <summary>Global Navigation Satellite System (GNSS)</summary>
        GlobalNavigationSatelliteSystem, // = GN
        /// <summary>Global Positioning System (GPS)</summary>
        GlobalPositioningSystem, // = GPS
        /// <summary>Heading Sensor - Compass, Magnetic</summary>
        CompassMagnetic, // = HC
        /// <summary>Heading Sensor - Gyro, North Seeking</summary>
        GyroNorthSeeking, // = HE
        /// <summary>Heading Sensor - Fluxgate</summary>
        Fluxgate, // = HF
        /// <summary>Heading Sensor - Gyro, Non-North Seeking</summary>
        GyroNonNorthSeeking, // = HN
        /// <summary>Hull Door Controller/Monitoring Panel</summary>
        HullDoorControllerMonitoringPanel, // = HD
        /// <summary>Hull Stress Monitoring</summary>
        HullStressMonitoring, // = HS
        /// <summary>Indian Regional Navigation Satellite System (IRNSS)</summary>
        IndianRegionalNavigationSatelliteSystem, // = GI
        /// <summary>Integrated Instrumentation</summary>
        IntegratedInstrumentation, // = II
        /// <summary>Integrated Navigation</summary>
        IntegratedNavigation, // = IN
        /// <summary>Loran C</summary>
        LoranC, // = LC
        /// <summary></summary>
        ProprietaryCode, // = P
        /// <summary></summary>
        RadarAndOrRadarPlotting, // = RA
        /// <summary></summary>
        PropulsionMachineryIncludingRemoteControl, // = RC
        /// <summary></summary>
        PhysicalShoreAISStation, // = SA
        /// <summary></summary>
        SounderDepth, // = SD
        /// <summary></summary>
        SteeringGearSteeringEngine, // = SG
        /// <summary></summary>
        ElectronicPositioningSystem, // = SN
        /// <summary></summary>
        SounderScanning, // = SS
        /// <summary></summary>
        TurnRateIndicator, // = TI
        /// <summary></summary>
        MicroprocessorController, // = UP
        /// <summary>User configured talker identifier</summary>
        UserID0, // = U0
        /// <summary>User configured talker identifier</summary>
        UserID1, // = U1
        /// <summary>User configured talker identifier</summary>
        UserID2, // = U2
        /// <summary>User configured talker identifier</summary>
        UserID3, // = U3
        /// <summary>User configured talker identifier</summary>
        UserID4, // = U4
        /// <summary>User configured talker identifier</summary>
        UserID5, // = U5
        /// <summary>User configured talker identifier</summary>
        UserID6, // = U6
        /// <summary>User configured talker identifier</summary>
        UserID7, // = U7
        /// <summary>User configured talker identifier</summary>
        UserID8, // = U8
        /// <summary>User configured talker identifier</summary>
        UserID9, // = U9
        /// <summary>Velocity sensor - Doppler</summary>
        Doppler, // = VD
        /// <summary>Velocity sensor - Speed Log, Water, Magnetic</summary>
        SpeedLogWaterMagnetic, // = VM
        /// <summary>Velocity sensor - Speed Log, Water Mechanical</summary>
        SpeedLogWaterMechanical, // = VW
        /// <summary>Voyage Data Recorder</summary>
        VoyageDataRecorder, // = VR
        /// <summary>Watertight Door Controller/Monitoring Panel</summary>
        WatertightDoorControllerMonitoringPanel, // = WD
        /// <summary>Weather Instruments</summary>
        WeatherInstruments, // = WI
        /// <summary>Water Level Detection Systems </summary>
        WaterLevelDetectionSystems, // = WL
        /// <summary>Transducer</summary>
        Transducer, // = YX
        /// <summary>Time keeper - Atomics Clock</summary>
        AtomicsClock, // = ZA
        /// <summary>Time keeper - Chronometer</summary>
        Chronometer, // = ZC
        /// <summary>Time keeper - Quartz</summary>
        Quartz, // = ZQ
        /// <summary>Quasi-Zenith Satellite System (QZSS)</summary>
        QuasiZenithSatelliteSystem, 
        /// <summary>Time keeper - Radio Update</summary>
        RadioUpdate, // = ZV

    }
}
