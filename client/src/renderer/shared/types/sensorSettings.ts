import { ClusterizationAlgorithm } from './clsterizationAlgorith'
import { DataLinkProtocol } from './dataLinkProtocol'
import { Modem } from './modem'
import { NetworkProtocol } from './networkProtocol'

export type SensorSettings = {
    Modem: Modem

    DataLinkProtocol: DataLinkProtocol
    ClusterizationAlgorithm: ClusterizationAlgorithm

    SampleInterval: string

    InitialSensorBattery: number
    BatteryDeadCharge: number

    NetworkProtocol: NetworkProtocol
}
