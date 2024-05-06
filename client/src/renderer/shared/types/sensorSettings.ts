import { ClusterizationAlgorithm } from './clsterizationAlgorith'
import { DataLinkProtocol } from './dataLinkProtocol'

export type SensorSettings = {
    DataLinkProtocol: DataLinkProtocol
    ClusterizationAlgorithm: ClusterizationAlgorithm

    SensorSampleInterval: string
    StartSamplingTime: string

    InitialSensorBattery: number
}
