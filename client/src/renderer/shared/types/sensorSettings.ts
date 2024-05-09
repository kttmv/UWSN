import { ClusterizationAlgorithm } from './clsterizationAlgorith'
import { DataLinkProtocol } from './dataLinkProtocol'
import { Modem } from './modem'

export type SensorSettings = {
    Modem: Modem

    DataLinkProtocol: DataLinkProtocol
    ClusterizationAlgorithm: ClusterizationAlgorithm

    SensorSampleInterval: string
    StartSamplingTime: string

    InitialSensorBattery: number
    BatteryDeadCharge: number
}
