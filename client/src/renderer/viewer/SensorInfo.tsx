import { Card, Text } from '@chakra-ui/react'
import { SensorSimulationState } from '../shared/types/sensorSimulationState'
import { useProjectStore } from '../store/projectStore'

type Props = {
    sensor: SensorSimulationState
}

export default function SensorInfo({ sensor }: Props) {
    const { project } = useProjectStore()
    if (!project) {
        throw new Error('Project не определен')
    }

    const positionString =
        `{ X: ${sensor.Position.X.toFixed(1)}, ` +
        `Y: ${sensor.Position.Y.toFixed(1)}, ` +
        `Z: ${sensor.Position.Z.toFixed(1)} }`

    let clusterString =
        sensor.ClusterId && sensor.ClusterId > -1
            ? sensor.ClusterId.toString()
            : 'Отсутствует'

    if (sensor.IsReference) {
        clusterString += ', является референсным'
    }

    return (
        <>
            <Text whiteSpace='nowrap'>Состояние:</Text>
            <Card padding='5px'>{sensor.State}</Card>

            <Text whiteSpace='nowrap'>Кластер:</Text>
            <Card padding='5px'>{clusterString}</Card>

            <Text whiteSpace='nowrap'>Батарея:</Text>
            <Card padding='5px'>
                {sensor.Battery.toFixed(2)} (
                {(
                    (sensor.Battery /
                        project.SensorSettings.InitialSensorBattery) *
                    100
                ).toFixed(2)}
                %)
            </Card>

            <Text whiteSpace='nowrap'>Координаты:</Text>
            <Card padding='5px'>{positionString}</Card>
        </>
    )
}
